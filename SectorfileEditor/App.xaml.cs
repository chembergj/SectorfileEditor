using NLog.Config;
using SectorfileEditor.Control;
using SectorfileEditor.Model;
using SectorfileEditor.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace SectorfileEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string UserSettingsFilename = "settings.xml";
        public string DefaultSettingspath =
            AppDomain.CurrentDomain.BaseDirectory + UserSettingsFilename;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigurationItemFactory.Default.Targets
                .RegisterDefinition("LogWindow", typeof(LogWindowTarget));

            try
            {
                ApplicationSettings.Load(DefaultSettingspath);
            }
            catch(System.IO.FileNotFoundException ex)
            {
                // Ignore
            }
    }

        protected override void OnExit(ExitEventArgs e)
        {
            ApplicationSettings.Save(DefaultSettingspath);

            base.OnExit(e);
        }
    }
}
