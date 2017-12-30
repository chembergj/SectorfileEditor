using SectorfileEditor.Model;
using System;
using System.Windows;

namespace SectorfileEditor.View
{
    /// <summary>
    /// Interaction logic for ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : Window
    {
        public Action<bool> ShowGeoStateChanged;
        public Action<bool> ShowRegionStateChanged;
        public Action<bool> EditGeoStateChanged;
        public Action<bool> EditRegionStateChanged;
        public Action<string, double?> MoveCenterClicked;

        public ToolWindow()
        {
            InitializeComponent();
        }
        
        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
            double zoomFactor = -1;

            if (textBoxLatLong.Text.Length == 0)
            {
                MessageBox.Show("Missing coordinate", "Input error", MessageBoxButton.OK);
            }
            else if (textBoxZoom.Text.Length > 0 && !double.TryParse(textBoxZoom.Text, out zoomFactor))
            {
                MessageBox.Show("Zoom-factor not a valid value (should be positive, decimal number)", "Input error", MessageBoxButton.OK);
            }
            else if(!LatLongUtil.RegexCoordinate.IsMatch(textBoxLatLong.Text))
            {
                MessageBox.Show("Invalid value for coordinate. Should be latitude and longitude, for example 'N055.36.59.567 E012.38.33.943'", "Input error", MessageBoxButton.OK);
            }
            else
            {
                MoveCenterClicked?.Invoke(textBoxLatLong.Text, zoomFactor == -1 ? new double?() : zoomFactor);
            }
        }

        public void UpdateLabels()
        {
            textBoxTranslate_X_Curr.Text = LatLongUtil.TranslateTransform.X.ToString();
            textBoxTranslate_Y_Curr.Text = LatLongUtil.TranslateTransform.Y.ToString();
            textBoxZoom_X_Curr.Text = LatLongUtil.ScaleTransform.ScaleX.ToString();
            textBoxZoom_Y_Curr.Text = LatLongUtil.ScaleTransform.ScaleY.ToString();
        }

        private void checkBoxShowGeo_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            ShowGeoStateChanged?.Invoke(checkBoxShowGeo.IsChecked.GetValueOrDefault());
        }

        private void checkBoxShowRegions_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            ShowRegionStateChanged?.Invoke(checkBoxShowRegions.IsChecked.GetValueOrDefault());
        }

        private void checkBoxEditGeo_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            EditGeoStateChanged?.Invoke(checkBoxEditGeo.IsChecked.GetValueOrDefault());
        }

        private void checkBoxEditRegions_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            EditRegionStateChanged?.Invoke(checkBoxEditRegions.IsChecked.GetValueOrDefault());
        }
    }
}
