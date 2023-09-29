using System;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Launcher
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autologin = autologin.IsChecked.Value;
            Properties.Settings.Default.username = user.Text;
            Properties.Settings.Default.password = pass.Password;
            Properties.Settings.Default.clientPath = path.Text;
            Properties.Settings.Default.Save();

            ((MainWindow)Owner).LoadClientStatus();

            Close();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            user.Text = Properties.Settings.Default.username;
            pass.Password = Properties.Settings.Default.password;
            autologin.IsChecked = Properties.Settings.Default.autologin;
            path.Text = Properties.Settings.Default.clientPath;
        }

        private void Button_ChangeClientFolder(object sender, RoutedEventArgs e)
        {
            string gameFolderPath;
            GameClientTools.SelectGameFolder(out gameFolderPath);
            if (!String.IsNullOrEmpty(gameFolderPath))
                path.Text = gameFolderPath;
        }

        private void resetPath_Click(object sender, RoutedEventArgs e)
        {
            path.Text = null;
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
