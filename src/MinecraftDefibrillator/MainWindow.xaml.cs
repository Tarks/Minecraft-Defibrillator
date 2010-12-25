using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MinecraftDefibrillator.Properties;
using Utilities;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace MinecraftDefibrillator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static globalKeyboardHook Gkh = new globalKeyboardHook();
        private string _launcherLocation;
        private const string _changeLauncherBtnPrefixText = "Change Launcher Location, currently: \n";
        public bool IsControlPressed { get; set; }
        public bool IsShiftPressed { get; set; }
        private Settings Settings = MinecraftDefibrillator.Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Gkh.HookedKeys.Add(Keys.R);
            Gkh.HookedKeys.Add(Keys.LControlKey);
            Gkh.HookedKeys.Add(Keys.RControlKey);
            Gkh.HookedKeys.Add(Keys.LShiftKey);
            Gkh.HookedKeys.Add(Keys.RShiftKey);
            Gkh.KeyUp += gkh_KeyUp;
            Gkh.KeyDown += new System.Windows.Forms.KeyEventHandler(Gkh_KeyDown);

            if (Settings.LauncherLocation == "")
            {
                RequestMinecraftLauncherLocation();
            }

            SetLauncherLocation(Settings.LauncherLocation);

            if (!Settings.FirstRun)
            {
                if (Settings.LaunchMinecraftWhenStarted &&
                    !String.IsNullOrEmpty(Settings.LauncherLocation))
                {
                    LaunchMinecraft();
                }
            }
            else
            {
                Settings.FirstRun = false;
                Settings.Save();
            }

            cmbStartOnLaunch.IsChecked = Settings.LaunchMinecraftWhenStarted;
            cmbAutoLogin.IsChecked = Settings.LaunchMinecraftWhenStarted;
        }

        private void RequestMinecraftLauncherLocation()
        {
            MessageBox.Show(
                "Hey, when you click ok a file dialog will pop up, please " +
                "select your Minecraft Launcher. This'll be saved so " +
                "you won't have to do it again !");

            if (!ShowFileDialogForMinecraftLocation())
            {
                MessageBox.Show("Hmm, seems like you didn't select anything, " +
                                "in the window that pops up find the File that " +
                                "you use to play Minecraft, select it and click " +
                                "the open button");
                if (!ShowFileDialogForMinecraftLocation())
                {
                    MessageBox.Show("You didn't manage it again, I'll quit for now but " +
                                    "feel free to try me again !");
                    Application.Current.Shutdown();
                }
            }
        }

        void Gkh_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    {
                        IsShiftPressed = true;
                    }
                    break;
                case Keys.LControlKey:
                case Keys.RControlKey:
                    {
                        IsControlPressed = true;
                    }
                    break;
            }
        }


        //KeyEventArgs doesn't seem to work
        void gkh_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    {
                        IsShiftPressed = false;
                    } break;
                case Keys.LControlKey:
                case Keys.RControlKey:
                    {
                        IsControlPressed = false;
                    } break;
                case Keys.R:
                    {
                        if (!IsShiftPressed || !IsControlPressed) { return; }
                        foreach (var process in Process.GetProcesses())
                        {
                            if (process.MainWindowTitle == "Minecraft Launcher" || process.MainWindowTitle == "Minecraft")
                            {
                                process.Kill();
                            }
                        }

                        LaunchMinecraft();
                        e.Handled = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }
        private void SelectMinecraftClicked(object sender, RoutedEventArgs e)
        {
            ShowFileDialogForMinecraftLocation();
        }

        private bool ShowFileDialogForMinecraftLocation()
        {
            var fd = new OpenFileDialog { Filter = "Minecraft Launcher | *.*" };
            fd.ShowDialog();
            if (!String.IsNullOrEmpty(fd.FileName) && fd.CheckFileExists)
            {
                SetLauncherLocation(fd.FileName);
                return true;
            }

            return false;
        }

        private void SetLauncherLocation(string location)
        {
            _launcherLocation = location;
            BtnChangeLauncherLocation.Content = _changeLauncherBtnPrefixText + _launcherLocation;

            //Saves everytime on startup, not a big issue.
            Settings.LauncherLocation = _launcherLocation;
            Properties.Settings.Default.Save();
        }

        private void LaunchMinecraftClick(object sender, RoutedEventArgs e)
        {
            LaunchMinecraft();
        }

        private void LaunchMinecraft()
        {
            if (_launcherLocation != "")
            {
                if (!File.Exists(_launcherLocation))
                {
                    MessageBox.Show("Can't find the Minecraft Launcher, let's try changing " +
                                    "the location");

                    RequestMinecraftLauncherLocation();

                }

                try
                {
                    Process.Start(_launcherLocation);
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                    Debugger.Break();
                }

            }
            if (Settings.AutoLogin) RunAutoLogin();
        }

        private void RunAutoLogin()
        {
            Thread.Sleep(3000);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait(" ");
        }

        private void cmbStartChecked(object sender, RoutedEventArgs e)
        {
            Settings.LaunchMinecraftWhenStarted = true;
            Properties.Settings.Default.Save();
        }

        private void cmbStartUnChecked(object sender, RoutedEventArgs e)
        {
            Settings.LaunchMinecraftWhenStarted = false;
            Properties.Settings.Default.Save();
        }

        private void cmbAutoLoginChecked(object sender, RoutedEventArgs e)
        {
            Settings.AutoLogin = true;
            Properties.Settings.Default.Save();

        }

        private void cmbAutoLoginUnChecked(object sender, RoutedEventArgs e)
        {
            Settings.AutoLogin = false;
            Properties.Settings.Default.Save();
        }
    }
}