using System.ComponentModel;
using System.IO;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Schema.Interfaces;
using MagnetarCA.Utils;
using Newtonsoft.Json;

namespace MagnetarCA.Schema
{
    public class Company : INotifyPropertyChanged, IRootBasedObject
    {
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

        [JsonConstructor]
        public Company()
        {
        }

        public Company(string root)
        {
            Root = root;
        }

        public Company Clone()
        {
            return MemberwiseClone() as Company;
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Root))
                return;

            if (!Directory.Exists(Root))
            {
                Directory.CreateDirectory(Root);
            }

            // (Konrad) Write Company Info to file.
            File.WriteAllText(this.GetCompanyDetailPath(), Serialize());
        }

        public void SetRootFromFilePath(string filePath)
        {
            Root = Path.GetDirectoryName(filePath);
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static Company Deserialize(string json)
        {
            return Json.Deserialize<Company>(json);
        }

        public override bool Equals(object obj)
        {
            return obj is Company item &&
                   Name.Equals(item.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
