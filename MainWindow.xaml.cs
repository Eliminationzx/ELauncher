using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Cursor = System.Windows.Input.Cursor;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variables
        public NotifyIcon ni;
        public Downloader dwn;
        public PlayerMini pmini;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            if (!CheckForInternetConnection())
            {
                MessageBox.Show("Unable to connect to the Internet, check your connection and try again", "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            IsEnabled = true;

            dwn = new Downloader(this);
            pmini = new PlayerMini(this);

            InitNotifyIcon();
            InitMainWindowCursors();
            InitCheckedExpansion();
        }

        private void LoadContent()
        {
            LoadServersStatsAsync();
            LoadLauncherVideoAsync();
            LoadNewsAsync();
            LoadClientStatus();
        }

        private void InitCheckedExpansion()
        {
            var serversList = explode(",", Properties.Settings.Default.serverList);

            foreach (var server in serversList)
            {
                var btn_expansion = FindName(String.Format("btn_{0}_play", server)) as System.Windows.Controls.RadioButton;
                btn_expansion.IsEnabled = true;
            }

            string firstServer = serversList.FirstOrDefault();
            if (String.IsNullOrEmpty(firstServer))
                return;

            var btn_choosenExpansion = FindName(String.Format("btn_{0}_play", firstServer)) as System.Windows.Controls.RadioButton;
            btn_choosenExpansion.IsChecked = true;

            ChangeCurrentServer(firstServer);
        }

        private async void LoadServersStatsAsync()
        {
            string server = Properties.Settings.Default.chosenServer;
            if (!ServerValidation(server))
                return;

            var statsInfoList = await JsonTools.GetServersStatsAsync(server);

            if (IsNullOrEmpty(statsInfoList))
                return;

            var onlineLabel = FindName("onlinePlayers") as System.Windows.Controls.Label;
            var stats_canv = FindName(String.Format("server_{0}_status", server)) as Canvas;

            int onlineCount = 0;
            for (int i = 0; i < statsInfoList.Count; i++)
            {
                var child = stats_canv.Children[i] as System.Windows.Shapes.Path;
                child.ToolTip = statsInfoList[i].name;
                ((RadialGradientBrush)child.Fill).GradientStops = DrawServerStatsIcons(statsInfoList[i].status);
                onlineCount += statsInfoList[i].online;
            }

            onlineLabel.Content = String.Format("{0} players online", onlineCount);
        }

        private GradientStopCollection DrawServerStatsIcons(string status)
        {
            GradientStopCollection greenRadientStopCollection = new GradientStopCollection
            {
                new GradientStop(Colors.DarkGreen, 1),
                new GradientStop(Colors.LightGreen, 0.528),
            };

            GradientStopCollection rednRadientStopCollection = new GradientStopCollection
            {
                new GradientStop(Colors.DarkRed, 1),
                new GradientStop(Colors.Red, 0.528),
            };

            return status == "online" ? greenRadientStopCollection : rednRadientStopCollection;
        }

        private static bool ServerValidation(string server)
        {
            return !String.IsNullOrEmpty(server) && Properties.Settings.Default.serverList.Contains(server);
        }

        public static string[] explode(string separator, string source)
        {
            return source.Split(new string[] { separator }, StringSplitOptions.None);
        }

        private void ChangeCurrentServer(string server)
        {
            string chosen_server = Properties.Settings.Default.chosenServer;
            if (!ServerValidation(server))
                return;

            Properties.Settings.Default.chosenServer = server;

            var bg_canv = FindName("bg_expansion") as Canvas;
            System.Windows.Controls.Image bgImage = new System.Windows.Controls.Image();
            bgImage.Source = new BitmapImage(new Uri(String.Format("pack://application:,,,/img/bg_{0}.jpg", server)));
            bgImage.Stretch = Stretch.UniformToFill;
            bgImage.StretchDirection = StretchDirection.UpOnly;
            bg_canv.Children.Add(bgImage);

            Border border = new Border();
            border.Width = 1024;
            border.Height = 80;
            var converter = new BrushConverter();
            var colorBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#26969696");
            border.BorderBrush = colorBrush;
            border.BorderThickness = new Thickness(0, 5, 0, 1);
            System.Windows.Controls.Image borderImage = new System.Windows.Controls.Image();
            borderImage.Source = new BitmapImage(new Uri(String.Format("pack://application:,,,/img/bg_{0}.jpg", server)));
            borderImage.Stretch = Stretch.UniformToFill;
            borderImage.StretchDirection = StretchDirection.UpOnly;
            borderImage.Effect = new BlurEffect() { Radius = 20 };
            border.Child = borderImage;
            bg_canv.Children.Add(border);

            LoadContent();
        }

        private async void LoadLauncherVideoAsync()
        {
            pmini.ResetPlayer();

            string server = Properties.Settings.Default.chosenServer;
            if (!ServerValidation(server))
                return;

            var launcherVideosInfoList = await JsonTools.GetLauncherVideosAsync(server);

            if (IsNullOrEmpty(launcherVideosInfoList))
                return;

            pmini.LoadSource(launcherVideosInfoList);
        }

        private void InitNotifyIcon()
        {
            int tsa_index = 0;
            ToolStripItem[] tsa_collector = new ToolStripItem[10];
            tsa_collector[tsa_index++] = new ToolStripMenuItem(String.Format("Downloading progress ({0}%)", progress_total.Value.ToString()), System.Drawing.Image.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/img/icons/down-arrow.png")).Stream), (s, e) => dwn.Cancel()) { Visible = false };
            tsa_collector[tsa_index++] = new NotifyMenuToolStripSeparator();
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Home", null, (s, e) => Process.Start(Properties.Settings.Default.mainURL));
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Community", null, (s, e) => Process.Start(Properties.Settings.Default.forumsURL));
            tsa_collector[tsa_index++] = new ToolStripMenuItem("FAQ", null, (s, e) => Process.Start(Properties.Settings.Default.faqURL));
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Control panel", null, (s, e) => Process.Start(Properties.Settings.Default.cpURL));
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Broadcasts", null, (s, e) => Process.Start(Properties.Settings.Default.streamsURL));
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Check updates", null, (s, e) => LauncherVersionSync(true));
            tsa_collector[tsa_index++] = new NotifyMenuToolStripSeparator();
            tsa_collector[tsa_index++] = new ToolStripMenuItem("Exit", System.Drawing.Image.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/img/icons/close-icon.png")).Stream), (s, e) => Application.Current.Shutdown());

            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.AddRange(tsa_collector);
            cms.Renderer = new NotifyMenuRender();

            ni = new NotifyIcon();
            ni.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/favicon.ico")).Stream);
            ni.ContextMenuStrip = cms;
            ni.Visible = true;
            ni.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    Show();
            };
        }

        private class NotifyMenuRender : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Rectangle rc = new Rectangle(System.Drawing.Point.Empty, e.Item.Size);
                e.Item.ForeColor = System.Drawing.Color.White;
                System.Drawing.Color c = e.Item.Selected ? System.Drawing.Color.FromArgb(50, 147, 209) : System.Drawing.Color.FromArgb(35, 39, 48);
                using (SolidBrush brush = new SolidBrush(c))
                    e.Graphics.FillRectangle(brush, rc);
            }
        }

        public class NotifyMenuToolStripSeparator : ToolStripSeparator
        {
            public NotifyMenuToolStripSeparator()
            {
                Paint += ExtendedToolStripSeparator_Paint;
            }

            private void ExtendedToolStripSeparator_Paint(object sender, PaintEventArgs e)
            {
                // Get the separator's width and height.
                ToolStripSeparator toolStripSeparator = (ToolStripSeparator)sender;
                int width = toolStripSeparator.Width;
                int height = toolStripSeparator.Height;

                // Choose the colors for drawing.
                // I've used Color.White as the foreColor.
                System.Drawing.Color foreColor = System.Drawing.Color.FromArgb(50, 147, 209);
                // Color.Teal as the backColor.
                System.Drawing.Color backColor = System.Drawing.Color.FromArgb(35, 39, 48);

                // Fill the background.
                e.Graphics.FillRectangle(new SolidBrush(backColor), 0, 0, width, height);

                // Draw the line.
                e.Graphics.DrawLine(new System.Drawing.Pen(foreColor), 4, height / 2, width - 4, height / 2);
            }
        }

        private void InitMainWindowCursors()
        {
            Cursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/img/cursors/arrow.cur")).Stream);

            List<System.Windows.Controls.Button> listButtons = new List<System.Windows.Controls.Button>();
            GetLogicalChildCollection(this, listButtons);

            foreach (var btn in listButtons)
            {
                btn.Cursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/img/cursors/magnify.ani")).Stream);
            }
        }

        /// <summary>
        /// Load news to NewsLoader Control
        /// </summary>
        private void LoadNewsAsync()
        {
            news_box.SetNewsAsync();
        }

        private async void LauncherVersionSync(bool checkUpdateFromNotifyMenu = false)
        {
            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var launcherVersion = await JsonTools.GetLauncherVersionAsync();

            bool error = String.IsNullOrEmpty(launcherVersion.version);

            if (!error && !launcherVersion.version.Equals(currentVersion))
            {
                version.Content = String.Format("ver. {0} (OLD)", currentVersion);
                ShowModalWithEffect(new LNewVer());
                return;
            }

            version.Content = String.Format("ver. {0}", currentVersion);

            if (checkUpdateFromNotifyMenu)
            {
                string msg = error ? "Error during version synchronization, check your connection and try again" : "You have the latest version of the Launcher";
                SendAppNotify(Assembly.GetExecutingAssembly().GetName().Name, msg, ToolTipIcon.Info);
            }
        }

        private void btn_download_Click(object sender, RoutedEventArgs e)
        {
            if (dwn.IsDownloading())
                dwn.Cancel();
            else
                dwn.Start();
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (dwn.IsDownloading())
                return;

            GameClientTools.RunGame();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            if (pmini.IsPlaying())
                pmini.Pause();

            SendAppNotify(Assembly.GetExecutingAssembly().GetName().Name, "The startup program continues to run in the background. To expand it, double-click the left mouse button", ToolTipIcon.Info);
            Hide();
        }

        public void LoadClientStatus()
        {
            if (dwn.IsDownloading())
                return;

            if (GameClientTools.ClientValidation(String.IsNullOrEmpty(Properties.Settings.Default.clientPath) ? Properties.Settings.Default.clientPath = Environment.CurrentDirectory : Properties.Settings.Default.clientPath))
            {
                labelmsg.Content = "The game is ready";
                btn_play.Visibility = Visibility.Visible;
                btn_download.Visibility = Visibility.Hidden;
                btn_repair.Visibility = Visibility.Visible;
                btn_custom.Visibility = Visibility.Visible;
                btn_custom_remove.Visibility = Visibility.Visible;

                string locale = GameClientTools.GetClientLocale();
                btn_play.ToolTip = String.Format("Client version: {0}{1}Localization: {2}{3}Size: {4}", GameClientTools.GetClientVersionByPath(Properties.Settings.Default.clientPath), 
                    Environment.NewLine, locale, Environment.NewLine, Downloader.detectSizeToString(GameClientTools.GetClientSize()));
            }
            else
            {
                labelmsg.Content = "The game is not installed";
                btn_play.Visibility = Visibility.Hidden;
                btn_repair.Visibility = Visibility.Hidden;
                btn_custom.Visibility = Visibility.Hidden;
                btn_custom_remove.Visibility = Visibility.Hidden;
                btn_download.Visibility = Visibility.Visible;
                btn_download.ToolTip = String.Format("Client version: {0}", GameClientTools.GetClientVersionByName(Properties.Settings.Default.chosenServer));
            }
        }

        public void SendAppNotify(string title, string msg, ToolTipIcon icon)
        {
            ni.ShowBalloonTip(Properties.Settings.Default.notifyMessageDuration, title, msg, icon);
        }

        /// <summary>
        /// Show WindowDialog with blur effects
        /// </summary>
        /// <param name="window">Modal window to show</param>
        private void ShowModalWithEffect (Window window)
        {
            var blur = new BlurEffect() { Radius = 15 };
            var current = Background;
            Effect = blur;

            if (window is Settings && dwn.IsDownloading())
            {
                (window as Settings).clientPathGrid.IsEnabled = false;
            }

            if (pmini.IsPlaying())
                pmini.Pause();

            window.Owner = this;

            if (window.ShowDialog() == false)
            {
                Effect = null;
            }
        }

        private void version_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Set motions only on Left button click
            if (e.LeftButton != MouseButtonState.Pressed) return;

            ShowModalWithEffect(new VerUpd());
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            ShowModalWithEffect(new Settings());
        }

        private void btn_feedback_Click(object sender, RoutedEventArgs e)
        {
            if (UrlValidation(Properties.Settings.Default.feedbackURL))
                Process.Start(Properties.Settings.Default.feedbackURL);
        }

        private void btn_expansion_play_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.RadioButton;
            if (btn.IsChecked.Value)
            {
                string expansion = btn.Name.Split('_')[1];
                ChangeCurrentServer(expansion);
            }
        }

        private void btn_logo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.mainURL);
        }

        private void link_faq_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.faqURL);
        }

        private void link_streams_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.streamsURL);
        }

        private void link_cp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.cpURL);
        }

        private void link_community_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.forumsURL);
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        /// <summary>
        /// Internet connection checker
        /// </summary>
        /// <returns>Returns true if connection exist</returns>
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool CheckForInternetConnection()
        {
            int desc;
            bool state = InternetGetConnectedState(out desc, 0);
            Console.WriteLine(String.Format("INTERNET CONNECTION DESCRIPTION: {0}", desc.ToString()));
            return state;
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LauncherVersionSync();
        }

        public static bool IsNullOrEmpty<T>(List<T> list)
        {
            return list == null || list.Count == 0 || !list.Any();
        }

        public static bool UrlValidation(string url)
        {
            return !String.IsNullOrEmpty(url) && (url.Contains("https://") || url.Contains("http://"));
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btn_expansion_play_Checked(object sender, RoutedEventArgs e)
        {
            string expansion = (sender as System.Windows.Controls.RadioButton).Name.Split('_')[1];
            var stats_canv = FindName(String.Format("server_{0}_status", expansion)) as Canvas;
            if (stats_canv.Visibility == Visibility.Hidden)
                stats_canv.Visibility = Visibility.Visible;
        }

        private void btn_expansion_play_Unchecked(object sender, RoutedEventArgs e)
        {
            string expansion = (sender as System.Windows.Controls.RadioButton).Name.Split('_')[1];
            var stats_canv = FindName(String.Format("server_{0}_status", expansion)) as Canvas;
            if (stats_canv.Visibility == Visibility.Visible)
                stats_canv.Visibility = Visibility.Hidden;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                var hwndTarget = hwndSource.CompositionTarget;
                if (hwndTarget != null) hwndTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void btn_repair_Click(object sender, RoutedEventArgs e)
        {
            if (dwn.IsDownloading())
                return;

            dwn.Start(DownloadTypes.Repair);
        }

        private void btn_custom_Click(object sender, RoutedEventArgs e)
        {
            if (dwn.IsDownloading())
                return;
          
            dwn.Start(DownloadTypes.Custom);
        }

        private void btn_custom_remove_Click(object sender, RoutedEventArgs e)
        {
            if (dwn.IsDownloading())
                return;

            if (MessageBox.Show("Do you really want to remove all downloaded custom patches?", "Confirmation of removal", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                dwn.Start(DownloadTypes.Custom, DownloadActions.Remove);
            }
        }

        private void btn_download_cancel_Click(object sender, RoutedEventArgs e)
        {
            dwn.Cancel();
        }

        private void btn_youtube_Click(object sender, RoutedEventArgs e)
        {
            if (UrlValidation(Properties.Settings.Default.youtubeURL))
                Process.Start(Properties.Settings.Default.youtubeURL);
        }

        private void btn_twitch_Click(object sender, RoutedEventArgs e)
        {
            if (UrlValidation(Properties.Settings.Default.twitchURL))
                Process.Start(Properties.Settings.Default.twitchURL);
        }

        private void btn_discord_Click(object sender, RoutedEventArgs e)
        {
            if (UrlValidation(Properties.Settings.Default.discordURL))
                Process.Start(Properties.Settings.Default.discordURL);
        }

        private void btn_facebook_Click(object sender, RoutedEventArgs e)
        {
            if (UrlValidation(Properties.Settings.Default.facebookURL))
                Process.Start(Properties.Settings.Default.facebookURL);
        }
    }
}
