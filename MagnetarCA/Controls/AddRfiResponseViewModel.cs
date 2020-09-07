using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MagnetarCA.Schema;
using MagnetarCA.Utils;

namespace MagnetarCA.Controls
{
    public class AddRfiResponseViewModel : ViewModelBase
    {
        public RelayCommand AddAttachment { get; set; }
        public RelayCommand<string> DeleteAttachment { get; set; }

        private Response _response;
        public Response Response
        {
            get { return _response; }
            set { _response = value; RaisePropertyChanged(() => Response); }
        }

        public AddRfiResponseViewModel(Response r)
        {
            Response = r;

            AddAttachment = new RelayCommand(OnAddAttachment);
            DeleteAttachment = new RelayCommand<string>(OnDeleteAttachment);
        }

        private void OnDeleteAttachment(string aPath)
        {
            Response.Attachments.Remove(aPath);
        }

        private void OnAddAttachment()
        {
            if (!(Dialogs.SelectFile(true) is string[] files) || !files.Any())
                return;

            foreach (var source in files)
            {
                Response.Attachments.Add(source);
            }
        }
    }
}
