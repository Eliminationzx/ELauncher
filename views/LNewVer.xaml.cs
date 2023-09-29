using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System;
using System.ComponentModel;
using System.IO;

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для LNewVer.xaml
    /// </summary>
    public partial class LNewVer
    {
        public LNewVer()
        {
            InitializeComponent();
        }

        private void updateNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentName = Assembly.GetExecutingAssembly().Location;
                string newName = currentName + ".old";

                if (File.Exists(newName))
                    File.Delete(newName);

                File.Move(currentName, newName);

                WebClient wc = new WebClient();
                wc.DownloadFileAsync(new Uri(Properties.Settings.Default.launcherURL), currentName);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
            }
            catch
            {
                // TODO: catch exception
            }
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Application.Current.Shutdown();

            string currentName = Assembly.GetExecutingAssembly().Location;
            if (File.Exists(currentName))
                Process.Start(currentName);
        }

        private void updateLater_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
