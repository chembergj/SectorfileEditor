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
            
            SctFileReader reader = new SctFileReader();
            // for (int i = 0; i < testPoints.Count() - 1; i++) { geoLines.Add(new SectorFileGeoLine() { Data = testPoints[i] + " " + testPoints[i + 1] }); }
            reader.GeoLineHandler += line =>
            {
                var from = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.Start);
                var to = LatLongUtil.GetLatLongDecimalPointFromLatLongString(line.End);
                geoLines.Add(new SectorFileLatLongDegreeLine(from.X, from.Y, to.X, to.Y));
            };

            reader.Parse("..\\..\\..\\UnitTestProject1\\Testdata\\EKDK_official_16_13.sct");

            logger.Debug("Read " + geoLines.Count + " geo lines");



            var initialCenter = "N063.15.57.950 E002.44.02.579";
            CenterAt(initialCenter, 90);
         }

 
        public void DrawLines()
        {
            int skipped = 0;
            WriteableBitmap writeableBmp = BitmapFactory.New((int)ActualWidth, (int)ActualHeight);
            geoImage.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {

                writeableBmp.Clear(Colors.White);
                var brush = (Brush)Brushes.Black.GetAsFrozen();
                geoLines.ForEach(geoLine =>
                    {
                        var from = LatLongUtil.Transform(geoLine.LatitudeStart, geoLine.LongitudeStart);
                        var to = LatLongUtil.Transform(geoLine.LatitudeEnd, geoLine.LongitudeEnd);
                        if ((to - from).Length > 0.7)
                        {
                            writeableBmp.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, Colors.Black);
                        }
                        else if (from.Y < 7 || to.Y < 7)
                        {
                            // Debug.WriteLine(string.Format("!!! {0} geoLine {1} {2} - {3} {4}", from.ToString(), geoLine.LatitudeStart, geoLine.LongitudeStart, geoLine.LatitudeEnd, geoLine.LongitudeEnd));
                        }
                        else
                        {
                            skipped++;
                        }

                    });
                Debug.WriteLine("Skipped " + skipped + " points ");
            }
        
        }


        public void CenterAt(String latLong, double zoom)
        {
            var splitted = latLong.Split(' ');
            var latdec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[0]);
            var londec = LatLongUtil.ConvertDegreeAngleToDouble(splitted[1]);

            Point p = new Point(londec, latdec);

            LatLongUtil.TranslateTransform.X = -p.X;
            LatLongUtil.TranslateTransform.Y = -LatLongUtil.LatitudeToY(p.Y);

            LatLongUtil.ScaleTransform.ScaleX = zoom;
            LatLongUtil.ScaleTransform.ScaleY = zoom;

            DrawLines();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void geoImage_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(geoImage);
            textBlockXY.Text = string.Format("({0},{1})", mousePosition.X, mousePosition.Y);
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
            double zoom = e.Delta > 0 ? 1.05 : 1 / 1.05;

            LatLongUtil.ScaleTransform.ScaleX *= zoom;
            LatLongUtil.ScaleTransform.ScaleY *= zoom;
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
    }
}
