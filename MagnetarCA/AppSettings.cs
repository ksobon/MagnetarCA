using System;
using System.ComponentModel;

namespace MagnetarCA
{
    public sealed class AppSettings : INotifyPropertyChanged
    {
        private static readonly Lazy<AppSettings> _lazy = new Lazy<AppSettings>(() => new AppSettings());

        public static AppSettings Instance
        {
            get { return _lazy.Value; }
        }

        private StoredSettings _storedSettings = new StoredSettings();
        public StoredSettings StoredSettings
        {
            get { return _storedSettings; }
            set { _storedSettings = value; RaisePropertyChanged(nameof(StoredSettings)); }
        }

        static AppSettings()
        {
        }

        private AppSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
