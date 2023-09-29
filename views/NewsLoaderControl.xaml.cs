using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Linq;

namespace Launcher
{
    public partial class NewsLoaderControl : UserControl
    {
        #region variables
        private LinkedListNode<NewsInfo> node;
        private News newsList;
        private DispatcherTimer itemChanger;
        #endregion

        public NewsLoaderControl()
        {
            InitializeComponent();

            node = null;
            newsList = new News();

            itemChanger = new DispatcherTimer();
            itemChanger.Tick += new EventHandler(itemChanger_Tick);
        }

        public ImageSource NewsImage
        {
            get { return news_image.Source; }
            set { news_image.Source = value; }
        }

        public string NewsHead
        {
            get { return news_head.Text; }
            set { news_head.Text = value; }
        }

        public string NewsBody
        {
            get { return news_body.Text; }
            set { news_body.Text = value; }
        }

        public static bool IsNullOrEmpty<T>(LinkedList<T> list)
        {
            return list == null || list.Count == 0 || !list.Any();
        }

        private void changeNewsItem(string direction, bool  manual = false)
        {
            if (newsList == null || newsList.First == newsList.Last)
                return;

            switch (direction)
            {
                case "RIGHT":
                    node = node.Next == null ? newsList.First : node.Next;
                    break;
                case "LEFT":
                    node = node.Previous == null ? newsList.Last : node.Previous;
                    break;
                default:
                    break;
            }

            ChangeStoryBoardTarget("ChangeItemsBegin", true);

            if (manual)
                SetItemChanger(new TimeSpan(0, 0, 30), true);
        }

        private void ChangeStoryBoardTarget(string res_target, bool completed = false)
        {
            Storyboard sb = FindResource(res_target) as Storyboard;
            Storyboard.SetTarget(sb, MainGrid);
            if (completed)
                sb.Completed += sb_Completed;
            sb.Begin();
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            ChangeStoryBoardTarget("ChangeItemsEnd");
            UpdateNewsData();
        }

        private static bool ServerValidation(string server)
        {
            return Properties.Settings.Default.serverList.Contains(server);
        }

        public async void SetNewsAsync()
        {
            NewsClear(); // clear before add new one
 
            string server = Properties.Settings.Default.chosenServer;
            if (!ServerValidation(server))
                return;

            newsList = await JsonTools.GetNewsAsync(server);

            if (!IsNullOrEmpty(newsList))
            {
                news_indacator_label.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
                btn_left.Visibility = Visibility.Visible;
                btn_right.Visibility = Visibility.Visible;

                node = newsList.First;

                UpdateNewsData();

                SetItemChanger(new TimeSpan(0, 0, 30));
            }
            else
            {
                news_indacator_label.Visibility = Visibility.Visible;
                news_indicator_text.Text = "News list is empty...";
                btn_left.Visibility = Visibility.Hidden;
                btn_right.Visibility = Visibility.Hidden;
            }
        }

        private void SetItemChanger(TimeSpan interval, bool reload = false)
        {
            if (reload)
            {
                itemChanger.Interval = interval;
                itemChanger.Stop();
                itemChanger.Start();
            }
            else
            {
                itemChanger.Interval = interval;
                itemChanger.Start();
            }
        }

        private void itemChanger_Tick(object sender, EventArgs e)
        {
            changeNewsItem("RIGHT", true);
        }

        private void UpdateNewsData()
        {
            NewsImage = String.IsNullOrEmpty(node.Value.thumbnail_url) ? null : new BitmapImage(new Uri(node.Value.thumbnail_url));
            NewsHead = node.Value.title;
            NewsBody = node.Value.body;
        }

        private void NewsClear()
        {
            NewsImage = null;
            NewsHead = null;
            NewsBody = null;
            newsList = null;
        }

        private void btn_right_Click(object sender, RoutedEventArgs e)
        {
            changeNewsItem("RIGHT", true);
        }

        private void btn_left_Click(object sender, RoutedEventArgs e)
        {
            changeNewsItem("LEFT", true);
        }
    }
}
