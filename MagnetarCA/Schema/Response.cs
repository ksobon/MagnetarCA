using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Schema.Interfaces;
using MagnetarCA.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace MagnetarCA.Schema
{
    public class Response : INotifyPropertyChanged, IRootBasedObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Type { get { return GetType().Name; } }

        public Guid ParentId { get; set; }

        private string _root;
        [JsonIgnore]
        public string Root
        {
            get { return _root; }
            set { _root = value; RaisePropertyChanged(nameof(Root)); }
        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged(nameof(Number)); }
        }

        private Actions _proposedAction = Actions.Revise;
        [JsonConverter(typeof(StringEnumConverter))]
        public Actions ProposedAction
        {
            get { return _proposedAction; }
            set { _proposedAction = value; RaisePropertyChanged(nameof(ProposedAction)); }
        }

        private DateTime _creationDate = DateTime.UtcNow;
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; RaisePropertyChanged(nameof(CreationDate)); }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set { _details = value; RaisePropertyChanged(nameof(Details)); }
        }

        private ObservableCollection<Attachment> _attachments = new ObservableCollection<Attachment>();
        public ObservableCollection<Attachment> Attachments
        {
            get { return _attachments; }
            set { _attachments = value; RaisePropertyChanged(nameof(Attachments)); }
        }

        [JsonConstructor]
        public Response()
        {
        }

        public Response(string root, int number, Guid parentId)
        {
            Root = root;
            Number = number;
            ParentId = parentId;
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Root))
                return;

            // (Konrad) Check how many responses exist already, and add another one.
            var existingResponses = Directory.GetFiles(Root, "rfi_response_*");
            Number = existingResponses.Length + 1;

            // (Konrad) Write RFI Info to file.
            File.WriteAllText(this.GetRfiResponseDetailPath(), Serialize());
        }

        public void SetRootFromFilePath(string filePath)
        {
            Root = Path.GetDirectoryName(filePath);
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static Response Deserialize(string json)
        {
            return Json.Deserialize<Response>(json);
        }

        public override bool Equals(object obj)
        {
            return obj is Response item && Id.Equals(item.Id);
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

    public enum Actions
    {
        [Description("Revise")]
        Revise,
        [Description("Revise and Resubmit")]
        ReviseResubmit
    }
}
