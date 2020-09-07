using System.Windows.Forms;

namespace MagnetarCA.Utils
{
    public static class Dialogs
    {
        public static string SelectDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            var result = dialog.ShowDialog();

            return result != DialogResult.OK ? string.Empty : dialog.SelectedPath;
        }

        public static object SelectFile(bool multi = false)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = "*.*",
                Multiselect = multi
            };

            var result = dialog.ShowDialog();
            object filePaths;
            if (multi) filePaths = dialog.FileNames;
            else filePaths = dialog.FileName;

            return result != DialogResult.OK ? string.Empty : filePaths;
        }
    }
}
