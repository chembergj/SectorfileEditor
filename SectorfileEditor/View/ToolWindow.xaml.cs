using SectorfileEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SectorfileEditor.View
{
    /// <summary>
    /// Interaction logic for ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : Window
    {
        public MainWindow MainWindow { get; set; }

        public ToolWindow()
        {
            InitializeComponent();
        }

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
           
            MainWindow.CenterAt(textBoxLatLong.Text, double.Parse(textBoxZoom.Text));

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LatLongUtil.TranslateTransform.X = double.Parse(textBoxTranslate_X.Text);
            LatLongUtil.TranslateTransform.Y = double.Parse(textBoxTranslate_Y.Text);
            LatLongUtil.ScaleTransform.ScaleX = double.Parse(textBoxZoom_X.Text);
            LatLongUtil.ScaleTransform.ScaleY = double.Parse(textBoxZoom_Y.Text);
       

            MainWindow.DrawLines();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxTranslate_X.Text = LatLongUtil.TranslateTransform.X.ToString();
            textBoxTranslate_Y.Text = LatLongUtil.TranslateTransform.Y.ToString();
            textBoxZoom_X.Text = LatLongUtil.ScaleTransform.ScaleX.ToString();
            textBoxZoom_Y.Text = LatLongUtil.ScaleTransform.ScaleY.ToString();
         
        }

        public void UpdateLabels()
        {
            textBoxTranslate_X_Curr.Text = LatLongUtil.TranslateTransform.X.ToString();
            textBoxTranslate_Y_Curr.Text = LatLongUtil.TranslateTransform.Y.ToString();
            textBoxZoom_X_Curr.Text = LatLongUtil.ScaleTransform.ScaleX.ToString();
            textBoxZoom_Y_Curr.Text = LatLongUtil.ScaleTransform.ScaleY.ToString();
        }
    }
}
