using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceCatalogWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IHandle<ShowControl>
    {
        private MainWindowVM _vm;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _vm = new MainWindowVM();
            _vm.PropertyChanged += _vm_PropertyChanged;
            EventMessenger.Instance.Subscribe(this);

            // we need to have code directly using these references so they are copied to referenced projects
            int x = 0;
            var dummy1 = WPFTextBoxAutoComplete.AutoCompleteBehavior.AutoCompleteItemsSource;
            var dummy2 = Xceed.Wpf.Toolkit.AutoSelectBehavior.Never;
            var dummy3 = Telerik.Windows.Controls.Input.ParseMode.Auto;
            var dummy4 = SharpSvn.SvnAccept.Merged;
            var dummy5 = System.Net.Http.ClientCertificateOption.Automatic;
            if (dummy1 == WPFTextBoxAutoComplete.AutoCompleteBehavior.AutoCompleteItemsSource
                || dummy2 == Xceed.Wpf.Toolkit.AutoSelectBehavior.OnFocus
                || dummy3 == Telerik.Windows.Controls.Input.ParseMode.Always
                || dummy4 == SharpSvn.SvnAccept.Theirs
                || dummy5 == System.Net.Http.ClientCertificateOption.Manual)
                x++;
        }

        #region Install page
        private void InstallPage_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://fsc-dev1.level3.com/DevTool/publish.htm");
        }
        #endregion
        #region Exit Click
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
        #region ShowControl Clicks

        private void TelerikDiagram_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new Controls.Diagraming.TelerikDiagram()));
        }
        private void CacheStatus_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new CacheStatus()));
        }
        private void RegExTester_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new RegExTester()));
        }
        private void BuildProduct_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new ProductBuilder()));
        }
        private void MyLocations_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new LocationBuilder()));
        }
        private void RuleDevelopment_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new EntityViewer()));
        }
        private void PcatDebugger_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new PcatDebugger()));
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new LogViewer()));
        }

        private void UpdateLocal_Click(object sender, RoutedEventArgs e)
        {
            //Handle(new ShowControl(new UpdateLocalUI()));
        }
        #endregion

        #region vm_PropertyChanged
        private MenuItem _lastMenu;
        private void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedTab"))
            {
                // remove last control's menu
                if (_lastMenu != null)
                    menu.Items.Remove(_lastMenu);

                // add new control's menu
                var menuControl = _vm.SelectedTab?.Control as IMenuControl;
                if (menuControl != null && menuControl.Menu != null)
                {
                    menu.Items.Add(menuControl.Menu);
                    _lastMenu = menuControl.Menu;
                }
                else
                    _lastMenu = null;
            }
        }
        #endregion
        #region ShowControl
        public void Handle(ShowControl message)
        {
            var tab = new TabVM()
            {
                Control = message.Control
            };

            _vm.Tabs.Add(tab);
            _vm.SelectedTab = tab;
        }
        #endregion
        #region RemoveTab Click
        private void RemoveTab_Click(object sender, RoutedEventArgs e)
        {
            var tab = (sender as Button)?.Tag as TabVM;
            if (tab != null
                && ((tab.Control is ICanCloseControl && ((ICanCloseControl)tab.Control).CanClose())
                    || !(tab.Control is ICanCloseControl))
                )
                _vm.Tabs.Remove(tab);
        }
        #endregion
    }

    public class MainWindowVM : ViewModelBase
    {
        private System.Timers.Timer _updateCheckTimer;

        #region Version
        private string _Version;
        public string Version
        {
            get { return _Version; }
            set
            {
                if (_Version != value)
                {
                    _Version = value;
                    NotifyOfPropertyChange("Version");
                }
            }
        }
        #endregion
        #region Tabs
        private ObservableCollection<TabVM> _Tabs;
        public ObservableCollection<TabVM> Tabs
        {
            get { return _Tabs; }
            set
            {
                if (_Tabs != value)
                {
                    _Tabs = value;
                    NotifyOfPropertyChange("Tabs");
                }
            }
        }
        #endregion
        #region SelectedTab
        private TabVM _SelectedTab;
        public TabVM SelectedTab
        {
            get { return _SelectedTab; }
            set
            {
                if (_SelectedTab != value)
                {
                    _SelectedTab = value;
                    NotifyOfPropertyChange("SelectedTab");
                }
            }
        }
        #endregion
        #region IsDeveloper
        private bool _IsDeveloper;
        public bool IsDeveloper
        {
            get { return _IsDeveloper; }
            set
            {
                if (_IsDeveloper != value)
                {
                    _IsDeveloper = value;
                    NotifyOfPropertyChange("IsDeveloper");
                }
            }
        }
        #endregion


        public MainWindowVM()
        {
            if (!Debugger.IsAttached && ApplicationDeployment.IsNetworkDeployed)
            {
                Version = "Version: " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

                _updateCheckTimer = new System.Timers.Timer();
                _updateCheckTimer.Interval = 24 * 60 * 60 * 1000;
                _updateCheckTimer.Elapsed += (s, e) => { RemindUserToUpdate(); };
                _updateCheckTimer.Start();
            }
            else
                Version = "Version: Debug";

            Tabs = new ObservableCollection<TabVM>();
            IsDeveloper = false; // UserPermissions.Current.IsDeveloper(null);
        }

        #region RemindUserToUpdate
        private bool _askingUserToUpdate = false;
        private void RemindUserToUpdate()
        {
            try
            {
                if (!_askingUserToUpdate) // avoid multiple MessageBoxes for days the user is off
                {
                    _askingUserToUpdate = true;
                    if (ApplicationDeployment.CurrentDeployment.CheckForUpdate()
                        && MessageBox.Show("There is a new version of the Dev Tool.  Would you like to update now?", "Dev Tool Updates", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ApplicationDeployment.CurrentDeployment.Update();
                        MessageBox.Show("The update has been downloaded.  Please restart the DevTool to use the new version.");
                    }
                    _askingUserToUpdate = false;
                }
            }
            catch (Exception e)
            {
                _askingUserToUpdate = false;
                MessageBox.Show(e.ToString(), "Check for updates error");
            }
        }
        #endregion
    }
    public class TabVM : ViewModelBase
    {
        #region Control
        private UserControl _Control;
        public UserControl Control
        {
            get { return _Control; }
            set
            {
                if (_Control != value)
                {
                    _Control = value;
                    NotifyOfPropertyChange("Control");
                }
            }
        }
        #endregion
    }

    public class ShowControl
    {
        public UserControl Control { get; set; }

        public ShowControl(UserControl control)
        {
            Control = control;
        }
    }

    public interface IMenuControl
    {
        MenuItem Menu { get; }
    }

    public interface ICanCloseControl
    {
        bool CanClose();
    }
}
