using System;
using System.IO;
using System.Windows;
using MagnetarCA.Utils;

namespace MagnetarCA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // (Konrad) Initiate Nlog logger.
            NLogUtils.CreateConfiguration();

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsPath = Path.Combine(appDataPath, "MagnetarCA\\settings.json");
            if (File.Exists(settingsPath))
            {
                using (var file = File.OpenText(settingsPath))
                {
                    var json = file.ReadToEnd();
                    var storedSettings = StoredSettings.Deserialize(json);
                    AppSettings.Instance.StoredSettings = storedSettings ?? new StoredSettings();
                }
            }
            else
            {
                AppSettings.Instance.StoredSettings = new StoredSettings();
            }

            // (Konrad) Show Main Window.
            var vm = new MainWindowViewModel();
            var v = new MainWindow
            {
                DataContext = vm
            };

            v.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsPath = Path.Combine(appDataPath, "MagnetarCA\\settings.json");
            if (!File.Exists(settingsPath))
            {
                Directory.CreateDirectory(Path.Combine(appDataPath, "MagnetarCA"));
            }

            var json = AppSettings.Instance.StoredSettings.Serialize();
            File.WriteAllText(settingsPath, json);

            base.OnExit(e);
        }
    }
}
