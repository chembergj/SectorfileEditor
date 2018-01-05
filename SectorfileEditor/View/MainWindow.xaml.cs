using NLog;
using SectorfileEditor.Control;
using SectorfileEditor.Model;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using SectorfileEditor.Model.SectorFile;

namespace SectorfileEditor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        ToolWindow toolWindow;
        private Point origin;
        readonly Color editColor = Colors.Pink;
        static readonly int editPointDelta = 3; // Half the width and height of an edit point
        static readonly Rect savedBackgroundRect = new Rect(0.0, 0.0, (double)2 * editPointDelta, (double)2 * editPointDelta);
        bool draggingEditPoint = false;

        // Data items from sector files
        List<LatLongDegreeLine> geoLines = new List<LatLongDegreeLine>();
        List<LatLongRegion> regions = new List<LatLongRegion>();
        Dictionary<String, SymbolSettingsFileLine> symbolSettings = new Dictionary<string, SymbolSettingsFileLine>();
        Dictionary<string, Color> sectorfileColors = new Dictionary<string, Color>();
        SectorFileInfo fileInfo = null;


        // User options
        bool showGeo = true;
        bool showRegions = true;
        bool editGeo = false;
        bool editRegions = false;
        private LatLongDegreePoint DraggingEditPoint;

        public MainWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                foreach (var target in NLog.LogManager.Configuration.AllTargets.Where(t => t is LogWindowTarget).Cast<LogWindowTarget>())
                {
                    target.LogReceived += LogReceived;
                }
            }

        }

        protected void LogReceived(LogEventInfo log)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                logTextBox.Text += log.Message + "\n";

            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Starting Sectorfile Editor, version " + Assembly.GetExecutingAssembly().GetName().Version);

            toolWindow = new ToolWindow();
            toolWindow.ShowGeoStateChanged = toolWindow_ShowGeoStateChanged;
            toolWindow.ShowRegionStateChanged = toolWindow_ShowRegionStateChanged;
            toolWindow.EditGeoStateChanged = toolWindow_EditGeoStateChanged;
            toolWindow.EditRegionStateChanged = toolWindow_EditRegionStateChanged;
            toolWindow.MoveCenterClicked = toolWindow_MoveCenterClicked;
            toolWindow.Show();
            this.Left = toolWindow.Left + toolWindow.Width + 10;

            ReadSymbolSettingsFile();
            ReadSectorFile();

            var clearColor = Color.FromRgb(
                symbolSettings["Sector:inactive sector background"].R,
                symbolSettings["Sector:inactive sector background"].G,
                symbolSettings["Sector:inactive sector background"].B);
            this.Background = new SolidColorBrush(clearColor);

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            string initialCenter; // = "N057.53.15.000 E006.15.22.000";
            var initialZoom = 90.0;

            if (!string.IsNullOrEmpty(ApplicationSettings.Instance.Center))
            {
                initialCenter = ApplicationSettings.Instance.Center;
                initialZoom = ApplicationSettings.Instance.ZoomFactor;
            }
            else
            {
                // Use default lat/long from sector file
                initialCenter = fileInfo.DefaultLatitude + " " + fileInfo.DefaultLongitude;
            }

            CenterAt(initialCenter, initialZoom);

            // Prepare buffer for storing background before drawing edit point
            savedBackground = BitmapFactory.New(2*editPointDelta, 2*editPointDelta);
        }

        private void toolWindow_EditRegionStateChanged(bool editState)
        {
            editRegions = editState;
            DrawLines();
        }

        private void toolWindow_EditGeoStateChanged(bool editState)
        {
            editGeo = editState;
            DrawLines();
        }

        private void toolWindow_MoveCenterClicked(string latLong, double? zoom)
        {
            CenterAt(latLong, zoom.HasValue ? zoom.Value : LatLongUtil.ScaleTransform.ScaleX);
        }

        private void toolWindow_ShowRegionStateChanged(bool showRegions)
        {
            this.showRegions = showRegions;
            DrawLines();
        }

        private void toolWindow_ShowGeoStateChanged(bool showGeo)
        {
            this.showGeo = showGeo;
            DrawLines();
        }

        private void ReadSymbolSettingsFile()
        {
            var reader = new SettingsFileReader<SymbolSettingsFileLine>();
            reader.SettingsLineHandler += line => symbolSettings.Add(line.Key, line);
            reader.Parse("..\\..\\..\\UnitTestProject1\\Testdata\\Symbols.txt");
            logger.Debug("Read " + symbolSettings.Count + " symbol lines");
        }

        private void ReadSectorFile()
        {
            SctFileReader reader = new SctFileReader();
            reader.GeoLineHandler += line =>
             {
                 var from = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.Start);
                 var to = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.End);
                 geoLines.Add(new LatLongDegreeLine(new LatLongDegreePoint(from.X, from.Y), new LatLongDegreePoint(to.X, to.Y), line.ColorName));
             };
            reader.DefineHandler += (key, color) =>
            {
                byte blue = (byte)((color & 0xff0000) >> 16);
                byte green = (byte)((color & 0xff00) >> 8);
                byte red = (byte)(color & 0xff);
                if (!sectorfileColors.ContainsKey(key))
                {
                    sectorfileColors.Add(key, Color.FromRgb(red, green, blue));
                }
            };
            reader.RegionHandler += region =>
            {
                regions.Add(new LatLongRegion(region.Name, region.ColorName,
                    region.Coordinates
                    .Select(c =>
                        {
                            var point = LatLongUtil.GetLatLongDecimalPointFromLatLongString(c);
                            return new LatLongDegreePoint(point.X, point.Y);
                        }
                    )
                    .ToList()));
            };
            reader.SectorfileInfoHandler += info => fileInfo = info;
            reader.Parse("..\\..\\..\\UnitTestProject1\\Testdata\\EKDK_official_16_13.sct");

            logger.Debug("Read " + geoLines.Count + " geo lines");
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_EXITSIZEMOVE = 0x0232;

            if (msg == WM_EXITSIZEMOVE)
            {
                DrawLines();
            }

            return IntPtr.Zero;
        }

        public void DrawLines()
        {
            int skipped = 0;
            WriteableBitmap writeableBmp = BitmapFactory.New((int)ActualWidth, (int)ActualHeight);
            geoImage.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {
                var clearColor = Color.FromRgb(
                    symbolSettings["Sector:inactive sector background"].R,
                    symbolSettings["Sector:inactive sector background"].G,
                    symbolSettings["Sector:inactive sector background"].B);
                writeableBmp.Clear(clearColor);

                skipped = 0;
                if (showRegions)
                {
                    regions.ForEach(region =>
                    {
                        var color = Colors.Black;
                        sectorfileColors.TryGetValue(region.ColorName, out color);
                        var points = new int[(region.Coordinates.Count + 1) * 2];
                        int i = 0;
                        region.Coordinates.ForEach(c =>
                        {

                            var p = LatLongUtil.Transform(c.Latitude, c.Longitude);
                            points[i++] = (int)p.X;
                            points[i++] = (int)p.Y;
                            c.X = (int)p.X;
                            c.Y = (int)p.Y;
                        });
                        points[i++] = points[0];
                        points[i++] = points[1];

                        writeableBmp.FillPolygon(points, color);
                        if (editRegions)
                        {
                            for (int j = 0; j < region.Coordinates.Count * 2; j += 2)
                            {
                                writeableBmp.FillRectangle(points[j] - editPointDelta, points[j + 1] - editPointDelta, points[j] + editPointDelta, points[j + 1] + editPointDelta, editColor);
                            }
                        }

                    });
                }

                if (showGeo)
                {
                    skipped = 0;
                    var geoColor = Color.FromRgb(
                        symbolSettings["Geo:line"].R,
                        symbolSettings["Geo:line"].G,
                        symbolSettings["Geo:line"].B);

                    geoLines.ForEach(geoLine =>
                        {
                            var from = LatLongUtil.Transform(geoLine.Start.Latitude, geoLine.Start.Longitude);
                            var to = LatLongUtil.Transform(geoLine.End.Latitude, geoLine.End.Longitude);
                            if (ShouldDrawLine(from, to))
                            {
                                var color = geoColor;
                                if (!string.IsNullOrEmpty(geoLine.Color))
                                {
                                    sectorfileColors.TryGetValue(geoLine.Color, out color);
                                }
                                writeableBmp.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, color);
                                geoLine.Start.X = (int)from.X;
                                geoLine.Start.Y = (int)from.Y;
                                geoLine.End.X = (int)to.X;
                                geoLine.End.Y = (int)to.Y;
                            }
                            else
                            {
                                skipped++;
                            }
                        });
                    Debug.WriteLine("Skipped " + skipped + " geo points ");
                }

            }

        }


        private bool ShouldDrawLine(Point from, Point to)
        {
            return ((int)from.X != (int)to.X || (int)from.Y != (int)to.Y)    // from/to are different both x an y
                && (from.X >= 0 || from.Y > 0 || to.X >= 0 || to.Y >= 0);    // either from or to are in the visible area 
        }

        public void CenterAt(String latLong, double zoom)
        {
            // Center
            textBlockLatLong.Text = latLong;
            var splitted = latLong.Split(' ');
            var latdec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[0]);
            var londec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[1]);

            LatLongUtil.TranslateTransform.X = -londec;
            LatLongUtil.TranslateTransform.Y = LatLongUtil.LatitudeToY(latdec);

            // Set zoom
            LatLongUtil.ScaleTransform.ScaleX = zoom;
            LatLongUtil.ScaleTransform.ScaleY = zoom;
            textBlockZoom.Text = zoom.ToString("###0.00");

            // Now centered at upper-left corner, center at the windows center
            var dragVector = new Point(ActualWidth / 2 / LatLongUtil.ScaleTransform.ScaleX, ActualHeight / 2 / LatLongUtil.ScaleTransform.ScaleY);
            LatLongUtil.TranslateTransform.X += dragVector.X;
            LatLongUtil.TranslateTransform.Y += dragVector.Y;

            DrawLines();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            geoImage.Width = grid.ActualWidth;
            geoImage.Height = grid.RowDefinitions[0].ActualHeight;
        }

        private void geoImage_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(geoImage);
            textBlockLatLong.Text = LatLongUtil.GetLatLongStringFromPoint(mousePosition);

            if (geoImage.IsMouseCaptured)
            {
                // Panning mode
                Vector v = e.GetPosition(this) - origin;
                var pos = e.GetPosition(this);

                var dragVector = new Point(v.X / LatLongUtil.ScaleTransform.ScaleX, v.Y / LatLongUtil.ScaleTransform.ScaleY);
                LatLongUtil.TranslateTransform.X += dragVector.X;
                LatLongUtil.TranslateTransform.Y += dragVector.Y;
                origin = e.GetPosition(this);
                toolWindow.UpdateLabels();
            }


            else if (draggingEditPoint)
            {
                // Move point
                Debug.WriteLine("Moving in edit mode");

                var bitmap = (WriteableBitmap)geoImage.Source;

                // Restore previous area
                if (previousEditPointRect.HasValue)
                {
                    bitmap.Blit(previousEditPointRect.Value, savedBackground, savedBackgroundRect);
                }

                // Save area before drawing
                
                previousEditPointRect = new Rect(mousePosition.X - editPointDelta, mousePosition.Y - editPointDelta, 2 * editPointDelta, 2 * editPointDelta);
                savedBackground.Blit(savedBackgroundRect, bitmap, previousEditPointRect.Value);
                // Draw moved edit point
                bitmap.FillRectangle((int)mousePosition.X - editPointDelta, (int)mousePosition.Y - editPointDelta, (int)mousePosition.X + editPointDelta, (int)mousePosition.Y + editPointDelta, editColor);


                // Draw lines from each neighbor to point being edited

            }
        }

        Rect? previousEditPointRect = null;
        WriteableBitmap savedBackground = null;

        private void geoImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            var mousePosition = e.GetPosition(geoImage);

            if (((WriteableBitmap)geoImage.Source).GetPixel((int)mousePosition.X, (int)mousePosition.Y) == editColor)
            {

                // Edit mode and user rightclicked an edit point
                LatLongRegion foundRegion = null;
                LatLongDegreePoint hitPoint = FindHitPoint(mousePosition, out foundRegion);
                if (hitPoint != null)
                {
                    var msg = LatLongUtil.GetLatLongStringFromPoint(new Point(hitPoint.X, hitPoint.Y));
                    if (foundRegion != null)
                    {
                        msg += "\nRegion: " + foundRegion.Name
                            + "\nColor code: " + foundRegion.ColorName;
                    }
                    MessageBox.Show(msg);
                }
            }
            else
            {
                // Enter pan mode
                origin = e.GetPosition(this);
                this.Cursor = Cursors.Hand;
                geoImage.CaptureMouse();
            }
        }

        // Find the nearest point that was just clicked on by the user
        private LatLongDegreePoint FindHitPoint(Point mousePosition, out LatLongRegion foundRegion)
        {
            var bitmap = (WriteableBitmap)geoImage.Source;

            foundRegion = null;
            LatLongDegreePoint foundPoint = null;

            foreach (var r in regions)
            {
                foundPoint = r.Coordinates.Find(point => Math.Abs(point.X - mousePosition.X) < editPointDelta && Math.Abs(point.Y - mousePosition.Y) < editPointDelta);
                if (foundPoint != null)
                {
                    foundRegion = r;
                    break;
                }
            }

            return foundPoint;
        }

        private void geoImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            var imageCenter = new Point(geoImage.ActualWidth / 2, geoImage.ActualHeight / 2);
            var centerCoordBeforeZoom = LatLongUtil.TranslateTransform.Inverse.Transform(LatLongUtil.ScaleTransform.Inverse.Transform(imageCenter));

            double zoom = e.Delta > 0 ? 1.2 : 1 / 1.2;
            LatLongUtil.ScaleTransform.ScaleX *= zoom;
            LatLongUtil.ScaleTransform.ScaleY *= zoom;

            var centerCoordAfterZoom = LatLongUtil.TranslateTransform.Inverse.Transform(LatLongUtil.ScaleTransform.Inverse.Transform(imageCenter));
            LatLongUtil.TranslateTransform.X += centerCoordAfterZoom.X - centerCoordBeforeZoom.X;
            LatLongUtil.TranslateTransform.Y += centerCoordAfterZoom.Y - centerCoordBeforeZoom.Y;


            DrawLines();
            toolWindow.UpdateLabels();
            textBlockZoom.Text = LatLongUtil.ScaleTransform.ScaleX.ToString("###0.00");

            e.Handled = true;
        }



        private void geoImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            geoImage.ReleaseMouseCapture();
            DrawLines();
            this.Cursor = Cursors.Arrow;
        }

        private void MenuItem_LoadESSymbologyClick(object sender, RoutedEventArgs e)
        {

        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            geoImage.Height = imageRow.ActualHeight;
            geoImage.Width = grid.ActualWidth;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ApplicationSettings.Instance.Center = LatLongUtil.GetLatLongStringFromPoint(new Point(geoImage.ActualWidth / 2, geoImage.ActualHeight / 2));
            ApplicationSettings.Instance.ZoomFactor = LatLongUtil.ScaleTransform.ScaleX;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void geoImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(geoImage);

            if (!draggingEditPoint && ((WriteableBitmap)geoImage.Source).GetPixel((int)mousePosition.X, (int)mousePosition.Y) == editColor)
            {
                Debug.WriteLine("Start edit");
                draggingEditPoint = true;
                LatLongRegion foundRegion;
                DraggingEditPoint = FindHitPoint(mousePosition, out foundRegion);
            }
           
        }

        private void geoImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggingEditPoint)
            {
                draggingEditPoint = false;
                var mousePosition = e.GetPosition(geoImage);
                DraggingEditPoint.X = mousePosition.X;
                DraggingEditPoint.Y = mousePosition.Y;
                double latitude;
                double longitude;
                LatLongUtil.GetLatLongDegreeFromPoint(mousePosition, out latitude, out longitude);
                DraggingEditPoint.Latitude = latitude;
                DraggingEditPoint.Longitude = longitude;
                previousEditPointRect = null;
                DrawLines();
            }
        }

    }
}
