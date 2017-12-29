using NLog;
using SectorfileEditor.Control;
using SectorfileEditor.Model;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

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
        Dictionary<string, Color> sectorfileColors = new Dictionary<string, Color>();


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

        List<SectorFileLatLongDegreeLine> geoLines = new List<SectorFileLatLongDegreeLine>();
        Dictionary<String, SymbolSettingsFileLine> symbolSettings = new Dictionary<string, SymbolSettingsFileLine>();

        string[] testPoints = new string[]
        {
            "N055.37.39.323 E012.39.17.124",
            "N055.37.35.901 E012.39.24.001",
            "N055.37.33.810 E012.39.24.091",
            "N055.37.33.824 E012.39.23.824",
            "N055.37.33.130 E012.39.23.861",
            "N055.37.33.060 E012.39.18.863",
            "N055.37.32.054 E012.39.18.340",
            "N055.37.32.193 E012.39.30.388",
            "N055.37.33.178 E012.39.29.769",
            "N055.37.33.117 E012.39.24.798",
            "N055.37.33.813 E012.39.24.768",
            "N055.37.33.812 E012.39.24.549",
            "N055.37.36.011 E012.39.24.484",
            "N055.37.39.535 E012.39.17.317",
            "N055.37.40.078 E012.39.12.956",
            "N055.37.39.822 E012.39.12.875"
        };



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Starting Sectorfile Editor, version " + Assembly.GetExecutingAssembly().GetName().Version);

            toolWindow = new ToolWindow() { MainWindow = this };
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
            
            var initialCenter = "N057.53.15.000 E006.15.22.000";
            CenterAt(initialCenter, 90);
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
            // for (int i = 0; i < testPoints.Count() - 1; i++) { geoLines.Add(new SectorFileGeoLine() { Data = testPoints[i] + " " + testPoints[i + 1] }); }
            reader.GeoLineHandler += line =>
            {
                var from = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.Start);
                var to = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.End);
                geoLines.Add(new SectorFileLatLongDegreeLine(from.X, from.Y, to.X, to.Y, line.ColorName));
            };
            reader.DefineHandler += (key, color) => {
                byte blue = (byte)((color & 0xff0000) >> 16); 
                byte green = (byte)((color & 0xff00) >> 8);
                byte red = (byte)(color & 0xff);
                if (!sectorfileColors.ContainsKey(key))
                {
                    sectorfileColors.Add(key, Color.FromRgb(red, green, blue));
                }
            };

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
                var geoColor = Color.FromRgb(
                    symbolSettings["Geo:line"].R,
                    symbolSettings["Geo:line"].G,
                    symbolSettings["Geo:line"].B);

                geoLines.ForEach(geoLine =>
                    {
                        var from = LatLongUtil.Transform(geoLine.LatitudeStart, geoLine.LongitudeStart);
                        var to = LatLongUtil.Transform(geoLine.LatitudeEnd, geoLine.LongitudeEnd);
                        if (ShouldDrawLine(from, to))
                        {
                            var color = geoColor;
                            if(!string.IsNullOrEmpty(geoLine.Color))
                            {
                                sectorfileColors.TryGetValue(geoLine.Color, out color);
                            }
                            writeableBmp.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, color);
                        }
                        else
                        {
                            skipped++;
                        }
                    });
                Debug.WriteLine("Skipped " + skipped + " points ");
            }
        
        }

        private bool ShouldDrawLine(Point from, Point to)
        {
            return ((int)from.X != (int)to.X || (int)from.Y != (int)to.Y)    // from/to are different both x an y
                && (from.X >= 0 || from.Y > 0 || to.X >= 0 || to.Y >= 0);    // either from or to are in the visible area 
        }

        public void CenterAt(String latLong, double zoom)
        {
            var splitted = latLong.Split(' ');
            var latdec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[0]);
            var londec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[1]);

            Point p = new Point(londec, latdec);

            LatLongUtil.TranslateTransform.X = -p.X;
            LatLongUtil.TranslateTransform.Y = LatLongUtil.LatitudeToY(p.Y);

            LatLongUtil.ScaleTransform.ScaleX = zoom;
            LatLongUtil.ScaleTransform.ScaleY = zoom;

            DrawLines();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
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
                Vector v = e.GetPosition(this) - origin;
                var pos = e.GetPosition(this);

                var dragVector = new Point(v.X / LatLongUtil.ScaleTransform.ScaleX, v.Y / LatLongUtil.ScaleTransform.ScaleY);
                LatLongUtil.TranslateTransform.X += dragVector.X;
                LatLongUtil.TranslateTransform.Y += dragVector.Y;
                origin = e.GetPosition(this);
                toolWindow.UpdateLabels();
            }
        }

        private void geoImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            origin = e.GetPosition(this);
            this.Cursor = Cursors.Hand;
            geoImage.CaptureMouse();
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
    }
}
