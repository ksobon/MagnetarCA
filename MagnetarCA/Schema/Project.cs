using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Schema.Interfaces;
using MagnetarCA.Utils;
using Newtonsoft.Json;
using NLog;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace MagnetarCA.Schema
{
    public class Project : INotifyPropertyChanged, IRootBasedObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly List<string> _folderStructure = new List<string>
        {
            "CA",
            "CA\\DocumentChange",
            "CA\\RFI",
            "CA\\RFI\\RFI_Sync",
            "CA\\Schema",
            "CA\\Submittal",
            "Project",
            "Project\\Company_Sync",
            "Project\\People_Sync",
            "Project\\ProjectInfo_Sync",
            "Project\\Schedule_Sync",
            "Project\\Schema_Sync",
            "Project\\Specification_Sync"
        };

        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Type { get { return GetType().Name; } }

        private string _root;
        [JsonIgnore]
        public string Root
        {
            get { return _root; }
            set { _root = value; RaisePropertyChanged(nameof(Root)); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(nameof(Name)); }
        }

        private string _number;
        public string Number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged(nameof(Number)); }
        }

        private DateTime _creationDate = DateTime.UtcNow;
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; RaisePropertyChanged(nameof(CreationDate)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged(nameof(IsSelected)); }
        }

        private Company _owner;
        public Company Owner
        {
            get { return _owner; }
            set { _owner = value; RaisePropertyChanged(nameof(Owner)); }
        }

        private Company _contractor;
        public Company Contractor
        {
            get { return _contractor; }
            set { _contractor = value; RaisePropertyChanged(nameof(Contractor)); }
        }

        private ObservableCollection<Company> _companies = new ObservableCollection<Company>();
        [JsonIgnore]
        public ObservableCollection<Company> Companies
        {
            get { return _companies; }
            set { _companies = value; RaisePropertyChanged(nameof(Companies)); }
        }

        private ObservableCollection<Rfi> _rfis = new ObservableCollection<Rfi>();
        [JsonIgnore]
        public ObservableCollection<Rfi> Rfis
        {
            get { return _rfis; }
            set { _rfis = value; RaisePropertyChanged(nameof(Rfis)); }
        }

        [JsonConstructor]
        public Project()
        {
        }

        public Project Clone()
        {
            return MemberwiseClone() as Project;
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Root))
                return;

            var projectFolder = Directory.CreateDirectory(this.GetProjectFolder());
            foreach (var path in _folderStructure)
            {
                var folderPath = Path.Combine(projectFolder.FullName, path);
                if (!PathUtils.TryCreateDirectory(folderPath))
                {
                    _logger.Error($"Could not create directory: {folderPath}");
                }
            }

            // (Konrad) Write Project Info to file.
            File.WriteAllText(this.GetProjectDetailPath(), Serialize());
        }

        public void SetRootFromFilePath(string filePath)
        {
            Root = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(filePath, @"..\..\..\..\")));
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static Project Deserialize(string json)
        {
            return Json.Deserialize<Project>(json);
        }

        public override bool Equals(object obj)
        {
            return obj is Project item && Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
