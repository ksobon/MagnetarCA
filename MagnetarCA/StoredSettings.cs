using System.Collections.Generic;
using System.ComponentModel;
using MagnetarCA.Utils;
using Newtonsoft.Json;

namespace MagnetarCA
{
    public class StoredSettings : INotifyPropertyChanged
    {
        private List<string> _projectRoots = new List<string>();
        public List<string> ProjectRoots
        {
            get { return _projectRoots; }
            set { _projectRoots = value; RaisePropertyChanged(nameof(ProjectRoots)); }
        }

        private string _selectedProject;
        public string SelectedProject
        {
            get { return _selectedProject; }
            set { _selectedProject = value; RaisePropertyChanged(nameof(SelectedProject)); }
        }

        [JsonConstructor]
        public StoredSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static StoredSettings Deserialize(string json)
        {
            return Json.Deserialize<StoredSettings>(json);
        }
    }
}
