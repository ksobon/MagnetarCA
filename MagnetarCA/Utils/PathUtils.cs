using System.IO;

namespace MagnetarCA.Utils
{
    public static class PathUtils
    {
        public static bool TryCreateDirectory(string dir)
        {
            if (Directory.Exists(dir)) return true;
            try
            {
                Directory.CreateDirectory(dir);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
