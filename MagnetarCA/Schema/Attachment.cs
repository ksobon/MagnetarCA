using System;
using System.ComponentModel;
using System.IO;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Schema.Interfaces;
using MagnetarCA.Utils;
using Newtonsoft.Json;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace MagnetarCA.Schema
{
    public class Attachment : INotifyPropertyChanged, IRootBasedObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Type { get { return GetType().Name; } }

        public Guid ParentId { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(nameof(Name)); }
        }

        private string _source;
        [JsonIgnore]
        public string Source
        {
            get { return _source; }
            set { _source = value; RaisePropertyChanged(nameof(Source)); }
        }

        private string _root;
        [JsonIgnore]
        public string Root
        {
            get { return _root; }
            set { _root = value; RaisePropertyChanged(nameof(Root)); }
        }

        [JsonConstructor]
        public Attachment()
        {
        }

        public Attachment(string source, string root, Guid parentId)
        {
            Source = source;
            Root = root;
            ParentId = parentId;

            Name = Path.GetFileName(source);
        }

        public Attachment(string source, string root, Guid parentId, int responseNumber)
        {
            Source = source;
            Root = root;
            ParentId = parentId;

            Name = $"{responseNumber:D3}_{Path.GetFileName(source)}";
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Source))
                return;

            var destination = this.GetAttachmentPath();
            File.Copy(Source, destination, true);

            // (Konrad) Write Attachment Info to file.
            File.WriteAllText(this.GetAttachmentDetailPath(), Serialize());
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static Attachment Deserialize(string json)
        {
            return Json.Deserialize<Attachment>(json);
        }

        public void SetRootFromFilePath(string filePath)
        {
            Root = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(filePath, @"..\..\")));
        }

        public override bool Equals(object obj)
        {
            return obj is Attachment item && Id.Equals(item.Id);
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
