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
        private string _hexFile = "";
        private readonly StateMachine _state = new StateMachine();

        private delegate void UiUpdateHandler();

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            _state.OnStateChange += _state_OnStateChange;
        }

        private async void _state_OnStateChange(eState oldState, eState newState)
        {
            if(newState == eState.INIT_WITH_FILE)
            {
                // Nu skal vi controllere om microbit er tilstede
                _ultrabitDrive = GetUltrabitUSB();

                if(_ultrabitDrive != "")
                {
                    // Microbit er tilstede så påbegynd kopiering af .hex fil
                    _state.State = eState.COPY_IN_PROGRESS;
                } else
                {
                    _state.State = eState.INIT_NO_MICROBIT;
                }   
            }

            if(newState == eState.INIT_NO_MICROBIT)
            {
                RegisterManagementEventWatching();
                image.Dispatcher.Invoke(new UiUpdateHandler(UiNoMicrobit));
            }

            if(newState == eState.MICROBIT_CONNECTED)
            {
                image.Dispatcher.Invoke(new UiUpdateHandler(UiMicrobitConnected));
            }

            if(newState == eState.COPY_IN_PROGRESS)
            {
                GridCopy.Visibility = Visibility.Visible;
                DispatcherTimer t = new DispatcherTimer();
                t.Interval = TimeSpan.FromMilliseconds(10);
                t.Tick += TimerCopyProgress;
                t.Start();

                using (FileStream SourceStream = File.Open(_hexFile, FileMode.Open))
                {
                    using (FileStream DestinationStream = File.Create(_ultrabitDrive + System.IO.Path.GetFileName(_hexFile)))
                    {
                        await SourceStream.CopyToAsync(DestinationStream);

                        label1.Content = "Genstarter ultra:bit / Rebooting ultra:bit";

                        DispatcherTimer t1 = new DispatcherTimer();
                        t1.Interval = TimeSpan.FromSeconds(6);
                        t1.Tick += TimerOnCopyComplete;
                        t1.Start();
                    }
                }
            }

            if(newState == eState.COPY_COMPLETED)
            {
                GridCopy.Visibility = Visibility.Hidden;
                image1.Source = new BitmapImage(new Uri(@"Resources/ready.png", UriKind.Relative));
                image1.Visibility = Visibility.Visible;

                DispatcherTimer t = new DispatcherTimer();
                t.Tick += T_Tick;
                t.Interval = TimeSpan.FromSeconds(5);
                t.Start();
            }
        }

        private void TimerOnCopyComplete(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            // Vi skal lige slette filen fra downloadmappen
            File.Delete(_hexFile);

            _state.State = eState.COPY_COMPLETED;
        }

        private void UiNoMicrobit()
        {
            image.Source = new BitmapImage(new Uri(@"Resources/logo_sad.png", UriKind.Relative));
            image1.Visibility = Visibility.Visible;
        }

        private void UiMicrobitConnected()
        {
            image.Source = new BitmapImage(new Uri(@"Resources/logo_happy.png", UriKind.Relative));
            image1.Visibility = Visibility.Hidden;
            _state.State = eState.COPY_IN_PROGRESS;
        }

        private void TimerCopyProgress(object sender, EventArgs e)
        {
            progressBar1.Value += 0.12;

            if (progressBar1.Value >= 100)
            {
                (sender as DispatcherTimer).Stop();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Application.Current.Properties["Filename"] = @"C:\Users\kmai\Lokale Dokumenter\Visual Studio Projects\ultrabit-filemover\Ultrabit\Ultrabit\bin\Debug\microbit-test.hex";

            if (Application.Current.Properties["Filename"] != null)
            {
                _hexFile = Application.Current.Properties["Filename"].ToString();
                _state.State = eState.INIT_WITH_FILE;
            } else
            {
                _state.State = eState.INIT_NO_FILE;
                this.Close();
            }
        }

        

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher.EventArrived -= watcher_EventArrived;
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
                if(d.DriveType == DriveType.Removable && d.VolumeLabel == "MICROBIT")
                {
                    return d.Name;
                }
            }

            return "";
        }

        private void RegisterManagementEventWatching()
        {
            _watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            _watcher.EventArrived += watcher_EventArrived;
            _watcher.Query = query;
            _watcher.Start();
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            _ultrabitDrive = GetUltrabitUSB();
            if (_ultrabitDrive != "")
            {
                _state.State = eState.MICROBIT_CONNECTED;
            }
        }
    }
}
