// ReSharper disable UnusedMemberInSuper.Global

using System;

namespace MagnetarCA.Schema.Interfaces
{
    public interface IRootBasedObject
    {
        Guid Id { get; set; }
        DateTime Timestamp { get; set; }
        string Type { get; }

        string Root { get; set; }
        void SetRootFromFilePath(string filePath);
    }
}
