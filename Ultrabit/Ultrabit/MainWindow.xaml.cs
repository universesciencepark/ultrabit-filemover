using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
using System.Windows.Threading;

namespace Ultrabit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ManagementEventWatcher _watcher;
        private string _ultrabitDrive = "";
        private string _hexFile = @"C:\Users\kmai\Lokale Dokumenter\Visual Studio Projects\ultrabit-filemover\Ultrabit\Ultrabit\bin\Debug\dfdf.hex";

        private delegate void UiUpdateHandler();

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterManagementEventWatching();

            if (Application.Current.Properties["Filename"] != null)
            {
                _hexFile = Application.Current.Properties["Filename"].ToString();
            }
            UpdateUi();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher.EventArrived -= watcher_EventArrived;
            }
        }

        private void UpdateUi()
        {
            _ultrabitDrive = GetUltrabitUSB();

            if (_ultrabitDrive == "")
            {
                image.Source = new BitmapImage(new Uri(@"Resources/logo_sad.png", UriKind.Relative));
            } else
            {
                image.Source = new BitmapImage(new Uri(@"Resources/logo_happy.png", UriKind.Relative));
                image1.Source = new BitmapImage(new Uri(@"Resources/ready.png", UriKind.Relative));

                DispatcherTimer t = new DispatcherTimer();
                t.Tick += T_Tick;
                t.Interval = TimeSpan.FromSeconds(5);
                t.Start();

                File.Move(_hexFile, _ultrabitDrive + System.IO.Path.GetFileName(_hexFile));
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.Close();
        }

        private string GetUltrabitUSB()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach(DriveInfo d in drives)
            {
                if(d.DriveType == DriveType.Removable && d.VolumeLabel == "USB DISK")
                {
                    return d.Name;
                }
            }

            return "";
        }

        private void RegisterManagementEventWatching()
        {
            _watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            _watcher.EventArrived += watcher_EventArrived;
            _watcher.Query = query;
            _watcher.Start();
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            image.Dispatcher.Invoke(new UiUpdateHandler(UpdateUi));
        }
    }
}
