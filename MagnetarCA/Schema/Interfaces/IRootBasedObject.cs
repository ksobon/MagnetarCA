// ReSharper disable UnusedMemberInSuper.Global

namespace MagnetarCA.Schema.Interfaces
{
    public interface IRootBasedObject
    {
        string Root { get; set; }
        void SetRootFromFilePath(string filePath);
    }
}
