using GalaSoft.MvvmLight;
using MagnetarCA.Schema;

namespace MagnetarCA.Controls
{
    public class AddCompanyViewModel : ViewModelBase
    {
        private Company _company;
        public Company Company
        {
            get { return _company; }
            set { _company = value; RaisePropertyChanged(() => Company); }
        }

        public AddCompanyViewModel(Company c)
        {
            Company = c;
        }
    }
}
