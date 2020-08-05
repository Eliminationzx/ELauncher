using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для VerUpd.xaml
    /// </summary>
    public partial class VerUpd
    {
        public VerUpd()
        {
            InitializeComponent();
            FillNewsBox();
        }

        private async void FillNewsBox()
        {
            try
            {
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var changelog = await JsonTools.GetLauncherChangelogAsync(currentVersion);
                NewsBox.AppendText(changelog.descr);
            }
            catch (Exception ex)
            {
                // TODO: add exception message
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void rectangle1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
