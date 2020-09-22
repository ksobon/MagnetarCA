using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Schema.Interfaces;
using MagnetarCA.Utils;
using Newtonsoft.Json;
using NLog;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace MagnetarCA.Schema
{
    public class Rfi : INotifyPropertyChanged, IRootBasedObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly List<string> _folderStructure = new List<string>
        {
            "Attachments",
            "Responses"
        };

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

        private string _number;
        public string Number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged(nameof(Number)); }
        }

        private Company _contractor;
        public Company Contractor
        {
            get { return _contractor; }
            set { _contractor = value; RaisePropertyChanged(nameof(Contractor)); }
        }

        private string _contractorRfiNumber;
        public string ContractorRfiNumber
        {
            get { return _contractorRfiNumber; }
            set { _contractorRfiNumber = value; RaisePropertyChanged(nameof(ContractorRfiNumber)); }
        }

        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; RaisePropertyChanged(nameof(Subject)); }
        }

        private DateTime _receivedDate = DateTime.UtcNow;
        public DateTime ReceivedDate
        {
            get { return _receivedDate; }
            set { _receivedDate = value; RaisePropertyChanged(nameof(ReceivedDate)); }
        }

        private DateTime _dueDate = DateTime.UtcNow.AddDays(7);
        public DateTime DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; RaisePropertyChanged(nameof(DueDate)); }
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

        private ObservableCollection<Response> _responses = new ObservableCollection<Response>();
        [JsonIgnore]
        public ObservableCollection<Response> Responses
        {
            get { return _responses; }
            set { _responses = value; RaisePropertyChanged(nameof(Responses)); }
        }

        [JsonConstructor]
        public Rfi()
        {
        }

        public Rfi(string root, Guid parentId)
        {
            Root = root;
            ParentId = parentId;
        }

        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Root))
                return;

            // (Konrad) Write RFI Info to file.
            var rfiFolder = Directory.CreateDirectory(this.GetRfiFolder());
            foreach (var path in _folderStructure)
            {
                var folderPath = Path.Combine(rfiFolder.FullName, path);
                if (!PathUtils.TryCreateDirectory(folderPath))
                {
                    _logger.Error($"Could not create directory: {folderPath}");
                }
            }

            // (Konrad) Write RFI Info to file.
            File.WriteAllText(this.GetRfiDetailPath(), Serialize());
        }

        public void SetRootFromFilePath(string filePath)
        {
            Root = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(filePath, @"..\..\")));
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }

        public static Rfi Deserialize(string json)
        {
            return Json.Deserialize<Rfi>(json);
        }

        public override bool Equals(object obj)
        {
            return obj is Rfi item && Id.Equals(item.Id);
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
