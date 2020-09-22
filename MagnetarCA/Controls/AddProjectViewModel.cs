using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MagnetarCA.Schema;
using MagnetarCA.Utils;

namespace MagnetarCA.Controls
{
    public class AddProjectViewModel : ViewModelBase
    {
        public RelayCommand SelectProjectRoot { get; set; }
        public RelayCommand CreateCompany { get; set; }

        private Project _project;
        public Project Project
        {
            get { return _project; }
            set { _project = value; RaisePropertyChanged(() => Project); }
        }

        public AddProjectViewModel(Project project)
        {
            Project = project;

            SelectProjectRoot = new RelayCommand(OnSelectProjectRoot);
            CreateCompany = new RelayCommand(OnCreateCompany);
        }

        private async void OnCreateCompany()
        {
            var root = Path.Combine(Project.Root, $"{Project.Name} {Project.Number}\\Project\\Company_Sync");
            var company = new Company(root, Project.Id);
            var vm = new AddCompanyViewModel(company);
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(vm, "AddCompanyDialogHost");
            if (result is bool boolResult && boolResult)
            {
                var c = vm.Company;
                c.Init();

                Project.Companies.Add(c);
            }
        }

        private void OnSelectProjectRoot()
        {
            var rootPath = Dialogs.SelectDirectory();
            Project.Root = rootPath;
        }
    }
}
