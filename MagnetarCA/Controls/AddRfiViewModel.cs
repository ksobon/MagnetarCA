using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MagnetarCA.Schema;
using MagnetarCA.Utils;

namespace MagnetarCA.Controls
{
    public class AddRfiViewModel : ViewModelBase
    {
        public RelayCommand AddAttachment { get; set; }
        public RelayCommand<Attachment> DeleteAttachment { get; set; }
        public RelayCommand CreateCompany { get; set; }

        private Rfi _rfi;
        public Rfi Rfi
        {
            get { return _rfi; }
            set { _rfi = value; RaisePropertyChanged(() => Rfi); }
        }

        private ObservableCollection<Company> _companies = new ObservableCollection<Company>();
        public ObservableCollection<Company> Companies
        {
            get { return _companies; }
            set { _companies = value; RaisePropertyChanged(nameof(Companies)); }
        }

        public AddRfiViewModel(Rfi rfi)
        {
            Rfi = rfi;
            Companies = GetCompanies();

            AddAttachment = new RelayCommand(OnAddAttachment);
            DeleteAttachment = new RelayCommand<Attachment>(OnDeleteAttachment);
            CreateCompany = new RelayCommand(OnCreateCompany);
        }

        private void OnDeleteAttachment(Attachment att)
        {
            Rfi.Attachments.Remove(att);
            File.Delete(att.Root);
        }

        private void OnAddAttachment()
        {
            if (!(Dialogs.SelectFile(true) is string[] files) || !files.Any())
                return;

            foreach (var source in files)
            {
                var att = new Attachment(source, null, Rfi.Id);
                Rfi.Attachments.Add(att);
            }
        }

        private async void OnCreateCompany()
        {
            var company = new Company();
            var vm = new AddCompanyViewModel(company);
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(vm, "AddCompanyDialogHost");
            if (result is bool boolResult && boolResult)
            {
                var root = Rfi.Root; // RFI_Sync location
                var companyDir = Path.Combine(root, "..\\..\\..\\Project\\Company_Sync");
                var c = vm.Company;
                c.Root = companyDir;
                c.ParentId = Rfi.ParentId;
                c.Init();

                Companies.Add(c);
            }
        }

        #region Utilities

        private ObservableCollection<Company> GetCompanies()
        {
            var companies = new ObservableCollection<Company>();
            var root = Rfi.Root; // RFI_Sync location
            var companyDir = Path.Combine(root, "..\\..\\..\\Project\\Company_Sync");
            if (!Directory.Exists(companyDir))
                return companies;

            var companyFiles = Directory.GetFiles(companyDir);
            foreach (var cFile in companyFiles)
            {
                using (var cf = File.OpenText(cFile))
                {
                    var cJson = cf.ReadToEnd();
                    var company = Company.Deserialize(cJson);
                    if (company == null)
                        continue;

                    companies.Add(company);
                }
            }

            return companies;
        }

        #endregion
    }
}
