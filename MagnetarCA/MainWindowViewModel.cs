#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MagnetarCA.Controls;
using MagnetarCA.Schema;
using MagnetarCA.Schema.Extensions;
using MagnetarCA.Utils;
using MaterialDesignThemes.Wpf;
using NLog;

#endregion

namespace MagnetarCA
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public SnackbarMessageQueue Messages { get; } = new SnackbarMessageQueue();

        public RelayCommand CreateProject { get; set; }
        public RelayCommand AddProject { get; set; }
        public RelayCommand CreateCompany { get; set; }
        public RelayCommand AddCompany { get; set; }
        public RelayCommand AddRfi { get; set; }
        public RelayCommand AddRfiResponse { get; set; }
        public RelayCommand AddAttachment { get; set; }
        public RelayCommand AddResponseAttachment { get; set; }
        public RelayCommand<string> DeleteAttachment { get; set; }
        public RelayCommand<string> DeleteResponseAttachment { get; set; }
        public RelayCommand<Pages> SwitchPage { get; set; }
        public RelayCommand<Pages> SwitchRfiPage { get; set; }
        public RelayCommand<Project> SelectProject { get; set; }
        public RelayCommand<Project> EditProject { get; set; }
        public RelayCommand<Company> EditCompany { get; set; }
        public RelayCommand WindowClosing { get; set; }

        private Pages _switchView;
        public Pages SwitchView
        {
            get { return _switchView; }
            set { _switchView = value; RaisePropertyChanged(() => SwitchView); }
        }

        private Pages _switchRfiView;
        public Pages SwitchRfiView
        {
            get { return _switchRfiView; }
            set { _switchRfiView = value; RaisePropertyChanged(() => SwitchRfiView); }
        }

        private AppSettings _settings;
        public AppSettings Settings
        {
            get { return _settings; }
            set { _settings = value; RaisePropertyChanged(() => Settings); }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            set { _selectedProject = value; RaisePropertyChanged(() => SelectedProject); }
        }

        private Rfi _selectedRfi;
        public Rfi SelectedRfi
        {
            get { return _selectedRfi; }
            set
            {
                _selectedRfi = value;
                RaisePropertyChanged(() => SelectedRfi);

                SelectedResponse = value?.Responses.FirstOrDefault();
            }
        }

        private Response _selectedResponse;
        public Response SelectedResponse
        {
            get { return _selectedResponse; }
            set { _selectedResponse = value; RaisePropertyChanged(() => SelectedResponse); }
        }

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { _projects = value; RaisePropertyChanged(() => Projects); }
        }

        #endregion

        public MainWindowViewModel()
        {
            Settings = AppSettings.Instance;
            Projects = new ObservableCollection<Project>(GetProjects());

            var selected = Projects.FirstOrDefault(x => x.Id.ToString() == AppSettings.Instance.StoredSettings.SelectedProject);
            if (selected != null)
            {
                selected.IsSelected = true;
                SelectedProject = selected;
                SelectedRfi = selected.Rfis.FirstOrDefault();
            }

            OnSwitchPage(Pages.Home); // set home page.

            RefreshWatchers(); // monitor folders for changes

            CreateProject = new RelayCommand(OnCreateProject);
            AddProject = new RelayCommand(OnAddProject);
            CreateCompany = new RelayCommand(OnCreateCompany);
            AddCompany = new RelayCommand(OnAddCompany);
            AddRfi = new RelayCommand(OnAddRfi);
            AddRfiResponse = new RelayCommand(OnAddRfiResponse);
            AddAttachment = new RelayCommand(OnAddAttachment);
            AddResponseAttachment = new RelayCommand(OnAddResponseAttachment);
            DeleteAttachment = new RelayCommand<string>(OnDeleteAttachment);
            DeleteResponseAttachment = new RelayCommand<string>(OnDeleteResponseAttachment);
            SwitchPage = new RelayCommand<Pages>(OnSwitchPage);
            SwitchRfiPage = new RelayCommand<Pages>(OnSwitchRfiPage);
            SelectProject = new RelayCommand<Project>(OnSelectProject);
            EditProject = new RelayCommand<Project>(OnEditProject);
            EditCompany = new RelayCommand<Company>(OnEditCompany);
            WindowClosing = new RelayCommand(OnWindowClosing);
        }

        private void OnSwitchPage(Pages p)
        {
            if (p == Pages.Home && SelectedProject != null)
            {
                SwitchView = Pages.Project;
            }
            else
            {
                SwitchView = p;
            }
        }

        private void OnSwitchRfiPage(Pages p)
        {
            SwitchRfiView = p;
        }

        private void OnDeleteAttachment(string aPath)
        {
            SelectedRfi.Attachments.Remove(aPath);
            File.Delete(aPath);
        }

        private void OnDeleteResponseAttachment(string aPath)
        {
            SelectedResponse.Attachments.Remove(aPath);
            File.Delete(aPath);
        }

        private void OnAddAttachment()
        {
            if (!(Dialogs.SelectFile(true) is string[] files) || !files.Any())
                return;

            for (var i = files.Length - 1; i >= 0; i--)
            {
                var source = files[i];
                var destination = SelectedRfi.GetRfiAttachmentPath(source);
                File.Copy(source, destination);

                SelectedRfi.Attachments.Add(destination);
            }
        }

        private void OnAddResponseAttachment()
        {
            if (!(Dialogs.SelectFile(true) is string[] files) || !files.Any())
                return;

            for (var i = files.Length - 1; i >= 0; i--)
            {
                var source = files[i];
                var destination = SelectedResponse.GetRfiResponseAttachmentPath(source);
                File.Copy(source, destination);

                SelectedResponse.Attachments.Add(destination);
            }
        }

        private async void OnAddRfi()
        {
            var root = Path.Combine(SelectedProject.Root, $"{SelectedProject.Name} {SelectedProject.Number}\\CA\\RFI\\RFI_Sync");
            var rfi = new Rfi(root);
            var vm = new AddRfiViewModel(rfi);
            var result = await DialogHost.Show(vm, "AddRfiDialogHost");
            if (result is bool boolResult && boolResult)
            {
                var r = vm.Rfi;
                r.Init();

                foreach (var c in vm.Companies)
                {
                    if (!SelectedProject.Companies.Contains(c))
                        SelectedProject.Companies.Add(c);
                }

                SelectedProject.Rfis.Add(r);
            }
        }

        private async void OnAddRfiResponse()
        {
            var root = Path.Combine(SelectedProject.Root, $"{SelectedProject.Name} {SelectedProject.Number}\\CA\\RFI\\RFI_Sync\\RFI_{SelectedRfi.Number}\\Responses");
            var number = SelectedRfi.Responses.Count + 1;
            var response = new Response(root, number);
            var vm = new AddRfiResponseViewModel(response);
            var result = await DialogHost.Show(vm, "AddRfiResponseDialogHost");
            if (result is bool boolResult && boolResult)
            {
                
                var r = vm.Response;
                r.Init();

                SelectedRfi.Responses.Add(r);
            }
        }

        private async void OnCreateProject()
        {
            var project = new Project();
            var vm = new AddProjectViewModel(project);
            var result = await DialogHost.Show(vm, "AddProjectDialogHost");
            if (result is bool boolResult && boolResult)
            {
                var p = vm.Project;
                p.Init();

                Projects.Add(p);
            }
        }

        private void OnAddProject()
        {
            var rootPath = Dialogs.SelectDirectory();
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                Messages.Enqueue("Please specify Project Root folder.");
                return;
            }

            var project = GetProject(rootPath);
            if (project == null)
                return;

            Projects.Add(project);
        }

        private async void OnCreateCompany()
        {
            var root = Path.Combine(SelectedProject.Root, $"{SelectedProject.Name} {SelectedProject.Number}\\Project\\Company_Sync");
            var company = new Company(root);
            var vm = new AddCompanyViewModel(company);
            var result = await DialogHost.Show(vm, "AddCompanyDialogHost");
            if (result is bool boolResult && boolResult)
            {
                var c = vm.Company;
                c.Init();

                SelectedProject.Companies.Add(c);
            }
        }

        private void OnAddCompany()
        {
            var companyPath = Dialogs.SelectFile() as string;
            if (string.IsNullOrWhiteSpace(companyPath))
            {
                Messages.Enqueue("Please specify Company file path.");
                return;
            }

            using (var f = File.OpenText(companyPath))
            {
                var json = f.ReadToEnd();
                var company = Company.Deserialize(json);
                if (company == null)
                {
                    Messages.Enqueue($"Failed to deserialize Company: {companyPath}");
                    return;
                }

                company.SetRootFromFilePath(companyPath);

                SelectedProject.Companies.Add(company);
            }
        }

        private static async void OnEditCompany(Company c)
        {
            var undoCopy = c.Clone();
            var vm = new AddCompanyViewModel(c);
            var result = await DialogHost.Show(vm, "AddCompanyDialogHost");
            if (result is bool boolResult && boolResult == false)
            {
                // (Konrad) User cancelled, let's undo changes.
                c.Name = undoCopy.Name;
            }

            // (Konrad) Company name changed. Let's delete old file, and write new one.
            File.Delete(undoCopy.GetCompanyDetailPath());

            // (Konrad) Folder Watcher will pick up that we wrote a new file, and add it to the list.
            File.WriteAllText(vm.Company.GetCompanyDetailPath(), vm.Company.Serialize());
        }

        private static async void OnEditProject(Project p)
        {
            var undoCopy = p.Clone();
            var vm = new AddProjectViewModel(p);
            var result = await DialogHost.Show(vm, "AddProjectDialogHost");
            if (result is bool boolResult && boolResult == false)
            {
                // (Konrad) User cancelled, let's undo changes.
                p.Name = undoCopy.Name;
                p.Number = undoCopy.Number;
                p.Root = undoCopy.Root;
                p.Owner = undoCopy.Owner;
                p.Contractor = undoCopy.Contractor;
            }
        }

        private List<FileSystemWatcher>  Watchers { get; } = new List<FileSystemWatcher>();

        private void OnSelectProject(Project p)
        {
            var isSelected = p.IsSelected;
            if (!isSelected)
                return;

            foreach (var project in Projects)
            {
                if (p.Equals(project))
                    continue;

                project.IsSelected = false;
            }

            SelectedProject = p;
            SelectedRfi = p.Rfis.FirstOrDefault();
            SelectedResponse = SelectedRfi?.Responses.FirstOrDefault();
            SwitchView = Pages.Project;
            SwitchRfiView = Pages.Rfis;

            RefreshWatchers();
        }

        private void OnWindowClosing()
        {
            // (Konrad) Kill all watchers.
            foreach (var watcher in Watchers)
                watcher?.Dispose();

            Watchers.Clear();

            // (Konrad) Update Stored Settings.
            Settings.StoredSettings.SelectedProject = SelectedProject?.Id.ToString();
            Settings.StoredSettings.ProjectRoots = Projects.Select(x => x.Root).ToList();

            // (Konrad) Update all files on drive.
            foreach (var project in Projects)
            {
                var pJson = project.Serialize();
                File.WriteAllText(project.GetProjectDetailPath(), pJson);

                foreach (var company in project.Companies)
                {
                    var cJson = company.Serialize();
                    File.WriteAllText(company.GetCompanyDetailPath(), cJson);
                }

                foreach (var rfi in project.Rfis)
                {
                    var rJson = rfi.Serialize();
                    File.WriteAllText(rfi.GetRfiDetailPath(), rJson);

                    foreach (var response in rfi.Responses)
                    {
                        var resJson = response.Serialize();
                        File.WriteAllText(response.GetRfiResponseDetailPath(), resJson);
                    }
                }
            }
        }

        #region Watchers

        private void RefreshWatchers()
        {
            // (Konrad) Kill all watchers.
            foreach (var watcher in Watchers)
                watcher?.Dispose();

            Watchers.Clear();

            if (SelectedProject == null)
                return;

            var projectWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(SelectedProject.GetProjectDetailPath()),
                NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.LastAccess |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName,
                Filter = Path.GetFileName(SelectedProject.GetProjectDetailPath()),
                EnableRaisingEvents = true
            };

            projectWatcher.Changed += ProjectWatcherOnChanged;

            var companyWatcher = new FileSystemWatcher
            {
                Path = SelectedProject.GetCompanyFolder(),
                NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.LastAccess |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName,
                Filter = "*.json",
                EnableRaisingEvents = true
            };

            companyWatcher.Changed += CompanyWatcherOnChanged;
            companyWatcher.Created += CompanyWatcherOnCreated;
            companyWatcher.Deleted += CompanyWatcherOnDeleted;

            var rfiWatcher = new FileSystemWatcher
            {
                Path = SelectedProject.GetRfiFolder(),
                NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.LastAccess |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName,
                Filter = "rfi_detail_*.json",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            rfiWatcher.Changed += RfiWatcherOnChanged;
            rfiWatcher.Created += RfiWatcherOnCreated;

            var responseWatcher = new FileSystemWatcher
            {
                Path = SelectedProject.GetRfiFolder(),
                NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.LastAccess |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName,
                Filter = "rfi_response_*.json",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            responseWatcher.Changed += ResponseWatcherOnChanged;
            responseWatcher.Created += ResponseWatcherOnCreated;

            // (Konrad) Store watchers so they can be disabled if active project changes.
            Watchers.Add(projectWatcher);
            Watchers.Add(companyWatcher);
            Watchers.Add(rfiWatcher);
            Watchers.Add(responseWatcher);
        }

        private void ResponseWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var response = Response.Deserialize(json);
                if (response == null)
                    return;

                response.SetRootFromFilePath(e.FullPath);

                var oneUp = Path.GetFullPath(Path.Combine(response.Root, @"..\"));
                var dirName = Path.GetFileName(Path.GetDirectoryName(oneUp));
                var rfiNumber = dirName?.Split('_').LastOrDefault();
                if (rfiNumber == null)
                    return;

                var rfi = SelectedProject.Rfis.FirstOrDefault(x => x.Number == rfiNumber);
                if (rfi == null)
                    return;

                if (!rfi.Responses.Contains(response))
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => rfi.Responses.Add(response)));

                Messages.Enqueue("Project updated externally. Added new Response.");
            }
        }

        private void ResponseWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var response = Response.Deserialize(json);
                if (response == null)
                    return;

                response.SetRootFromFilePath(e.FullPath);

                var oneUp = Path.GetFullPath(Path.Combine(response.Root, @"..\"));
                var dirName = Path.GetFileName(Path.GetDirectoryName(oneUp));
                var rfiNumber = dirName?.Split('_').LastOrDefault();
                if (rfiNumber == null)
                    return;

                var rfi = SelectedProject.Rfis.FirstOrDefault(x => x.Number == rfiNumber);
                var found = rfi?.Responses.FirstOrDefault(x => x.Equals(response));
                if (found == null)
                    return;

                if (found.Number == response.Number &&
                    found.ProposedAction == response.ProposedAction &&
                    found.CreationDate == response.CreationDate &&
                    found.Details == response.Details &&
                    found.Attachments == response.Attachments)
                    return; // no changes

                // (Konrad) Update only properties that users can actually modify.
                found.Number = response.Number;
                found.ProposedAction = response.ProposedAction;
                found.CreationDate = response.CreationDate;
                found.Details = response.Details;
                found.Attachments = response.Attachments;

                Messages.Enqueue("Project updated externally. Edited Response details.");
            }
        }

        private void RfiWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var rfi = Rfi.Deserialize(json);
                if (rfi == null)
                    return;

                rfi.SetRootFromFilePath(e.FullPath);

                var found = SelectedProject.Rfis.FirstOrDefault(x => x.Equals(rfi));
                if (found == null)
                    return;

                if (found.Number == rfi.Number &&
                    found.Subject == rfi.Subject &&
                    Equals(found.Contractor, rfi.Contractor) &&
                    found.ContractorRfiNumber == rfi.ContractorRfiNumber &&
                    found.ReceivedDate == rfi.ReceivedDate &&
                    found.DueDate == rfi.DueDate &&
                    found.Details == rfi.Details &&
                    found.Attachments == rfi.Attachments)
                    return; // no changes

                // (Konrad) Update only properties that users can actually modify.
                found.Number = rfi.Number;
                found.Subject = rfi.Subject;
                found.Contractor = rfi.Contractor;
                found.ContractorRfiNumber = rfi.ContractorRfiNumber;
                found.ReceivedDate = rfi.ReceivedDate;
                found.DueDate = rfi.DueDate;
                found.Details = rfi.Details;
                found.Attachments = rfi.Attachments;

                Messages.Enqueue("Project updated externally. Edited RFI details.");
            }
        }

        private void RfiWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var rfi = Rfi.Deserialize(json);
                if (rfi == null)
                    return;

                rfi.SetRootFromFilePath(e.FullPath);

                if (!SelectedProject.Rfis.Contains(rfi))
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => SelectedProject.Rfis.Add(rfi)));

                Messages.Enqueue("Project updated externally. Added new RFI.");
            }
        }

        private void CompanyWatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var found = SelectedProject.Companies.FirstOrDefault(x => x.GetCompanyDetailPath() == e.FullPath);
            if (found == null)
                return;

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                SelectedProject.Companies.Remove(found)));

            Messages.Enqueue("Project updated externally. Removed a Company.");
        }

        private void CompanyWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var company = Company.Deserialize(json);
                if (company == null)
                    return;

                company.SetRootFromFilePath(e.FullPath);

                if (!SelectedProject.Companies.Contains(company))
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => SelectedProject.Companies.Add(company)));

                Messages.Enqueue("Project updated externally. Added new Company.");
            }
        }

        private void CompanyWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var company = Company.Deserialize(json);
                if (company == null)
                    return;

                company.SetRootFromFilePath(e.FullPath);

                var found = SelectedProject.Companies.FirstOrDefault(x => x.Equals(company));
                if (found == null)
                    return;

                if (found.Name == company.Name)
                    return; // no changes

                // (Konrad) Update only properties that users can actually modify.
                found.Name = company.Name;

                Messages.Enqueue("Project updated externally. Edited Company details.");
            }
        }

        private void ProjectWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;

            Thread.Sleep(TimeSpan.FromSeconds(1));

            using (var f = File.OpenText(e.FullPath))
            {
                var json = f.ReadToEnd();
                var project = Project.Deserialize(json);
                if (project == null)
                    return;

                project.SetRootFromFilePath(e.FullPath);

                var found = Projects.FirstOrDefault(x => x.Equals(project));
                if (found == null)
                    return;

                if (found.Name == project.Name &&
                    found.Number == project.Number &&
                    Equals(found.Owner, project.Owner) &&
                    Equals(found.Contractor, project.Contractor))
                    return; // no changes

                // (Konrad) Update only properties that users can actually modify.
                found.Name = project.Name;
                found.Number = project.Number;
                found.Owner = project.Owner;
                found.Contractor = project.Contractor;

                Messages.Enqueue("Project updated externally. Edited Project details.");
            }
        }

        #endregion

        #region Utilities

        private Project GetProject(string rootDir)
        {
            if (!Directory.Exists(rootDir))
            {
                _logger.Error($"Directory doesn't exist: {rootDir}");
                Messages.Enqueue($"Directory doesn't exist: {rootDir}");
                return null;
            }

            try
            {
                var dirs = Directory.GetDirectories(rootDir, "ProjectInfo_Sync",
                    SearchOption.AllDirectories);
                if (!dirs.Any())
                {
                    Messages.Enqueue("Could not find Project Info in specified Folder.");
                    _logger.Error("Could not find Project Info in specified Folder.");
                    return null;
                }

                foreach (var dir in dirs)
                {
                    var files = Directory.GetFiles(dir);
                    foreach (var file in files)
                    {
                        using (var f = File.OpenText(file))
                        {
                            var json = f.ReadToEnd();
                            var project = Project.Deserialize(json);
                            if (project == null)
                                continue;

                            project.SetRootFromFilePath(file);

                            return project;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
                return null;
            }

            _logger.Error($"Could not parse Project object from specified location: {rootDir}");
            return null;
        }

        private List<Project> GetProjects()
        {
            var projects = new List<Project>();
            foreach (var root in Settings.StoredSettings.ProjectRoots)
            {
                if (!Directory.Exists(root))
                    continue;

                var dirs = Directory.GetDirectories(root, "ProjectInfo_Sync", SearchOption.AllDirectories);
                foreach (var dir in dirs)
                {
                    var projectFiles = Directory.GetFiles(dir, "project_detail_*.json");
                    foreach (var pFile in projectFiles)
                    {
                        using (var pf = File.OpenText(pFile))
                        {
                            var pJson = pf.ReadToEnd();
                            var project = Project.Deserialize(pJson);
                            if (project == null)
                                continue;

                            project.SetRootFromFilePath(pFile);

                            var companyFiles = Directory.GetFiles(project.GetCompanyFolder());
                            foreach (var cFile in companyFiles)
                            {
                                using (var cf = File.OpenText(cFile))
                                {
                                    var cJson = cf.ReadToEnd();
                                    var company = Company.Deserialize(cJson);
                                    if (company == null)
                                        continue;

                                    company.SetRootFromFilePath(cFile);

                                    project.Companies.Add(company);
                                }
                            }
                            
                            var rfiDirs = Directory.GetDirectories(project.GetRfiFolder(), "RFI_*", SearchOption.AllDirectories);
                            foreach (var rDir in rfiDirs)
                            {
                                var rfiFiles = Directory.GetFiles(rDir);
                                foreach (var rfiFile in rfiFiles)
                                {
                                    using (var rf = File.OpenText(rfiFile))
                                    {
                                        var rJson = rf.ReadToEnd();
                                        var rfi = Rfi.Deserialize(rJson);
                                        if (rfi == null)
                                            continue;

                                        rfi.SetRootFromFilePath(rfiFile);

                                        var responseFiles = Directory.GetFiles(rfi.GetResponsesFolder());
                                        foreach (var responseFile in responseFiles)
                                        {
                                            using (var rff = File.OpenText(responseFile))
                                            {
                                                var rffJson = rff.ReadToEnd();
                                                var response = Response.Deserialize(rffJson);
                                                if (response == null)
                                                    continue;

                                                response.SetRootFromFilePath(responseFile);

                                                rfi.Responses.Add(response);
                                            }
                                        }

                                        project.Rfis.Add(rfi);
                                    }
                                }
                            }

                            projects.Add(project);
                        }
                    }
                }
            }
            
            return projects;
        }

        #endregion
    }

    public enum Pages
    {
        Home,
        Projects,
        Project,
        Settings,
        Companies,
        Rfis,
        RfiResponses
    }
}
