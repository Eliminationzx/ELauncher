using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    public class DriveService
    {
        public static ClientItems GetChildrens(string parent_id)
        {
            string url = String.Format("{0}/drive/v3/files?q=%27{1}%27+in+parents&fields=files({2})&key={3}", Properties.Settings.Default.GDRIVE_APIDOMAIN, parent_id, Properties.Settings.Default.GDRIVEAPI_FIELDS, Properties.Settings.Default.GDRIVEAPI_KEY);
            return JsonTools.DeserializeClientItemsJSON(url);
        }

        public static string NormalizedWebContentLink(string item_id)
        {
             return String.Format("{0}/drive/v3/files/{1}?key={2}&alt=media", Properties.Settings.Default.GDRIVE_APIDOMAIN, item_id, Properties.Settings.Default.GDRIVEAPI_KEY);
        }
    }

    public struct QueuedFileInfo
    {
        public string url { get; set; }
        public string localDest { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
        public long size { get; set; }
    }

    public enum DownloadTypes
    {
        Default,
        Repair,
        Custom
    }

    public enum DownloadActions
    {
        Default,
        Remove
    }

    public class Downloader
    {
        private WebClient webClient;
        private Queue<QueuedFileInfo> downloadQueue;
        private QueuedFileInfo _prevQueuedFileInfo;
        private DateTime start_timer;
        private Stopwatch sw_cf;
        private System.Windows.Forms.Timer base_timer;
        private int dwn_item_counter, dwn_queued_item_counter, dwn_retryCount;
        private long dwn_items_bytes, dwn_last_bytes, dwn_completed_bytes;
        private MainWindow _mw;
        private bool _isDownloading;

        public Downloader(MainWindow mw)
        {
            _mw = mw;

            webClient = new WebClient();

            downloadQueue = new Queue<QueuedFileInfo>();

            base_timer = new System.Windows.Forms.Timer();
            base_timer.Interval = 1000;
            base_timer.Tick += new EventHandler(Base_timer_Tick);

            sw_cf = new Stopwatch();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

            dwn_item_counter = 0;
            dwn_queued_item_counter = 0;
            dwn_retryCount = 0;

            dwn_items_bytes = 0;
            dwn_last_bytes = 0;
            dwn_completed_bytes = 0;

            _isDownloading = false;
        }

        private void Base_timer_Tick(object sender, EventArgs e)
        {
            _mw.download_elapsed.Content = DateTime.Now.Subtract(start_timer).ToString(@"hh\:mm\:ss");
        }

        private void Reset()
        {
            downloadQueue.Clear();

            dwn_item_counter = 0;
            dwn_queued_item_counter = 0;
            dwn_retryCount = 0;
            dwn_items_bytes = 0;
            dwn_last_bytes = 0;
            dwn_completed_bytes = 0;

            _isDownloading = false;

            _mw.progress_file.Value = 0;
            _mw.progress_total.Value = 0;

            _mw.progress_total_bytes.Content = null;
            _mw.download_progress_files_label.Content = null;
            _mw.download_progress_bytes.Content = null;
            _mw.progress_total_pct.Content = "0%";
        }

        public bool IsDownloading()
        {
            return _isDownloading;
        }

        protected long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        protected string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public static bool verifyMd5Hash(string input, string hash)
        {
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(input, hash) == 0;
        }

        public void Start(DownloadTypes type = DownloadTypes.Default, DownloadActions action = DownloadActions.Default)
        {
            string folderId;
            switch (type == DownloadTypes.Default ? Properties.Settings.Default.chosenServer : GameClientTools.GetClientExpansion(Properties.Settings.Default.clientPath))
            {
                 case "vanilla":
                     folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.vanillaCPatchFID : Properties.Settings.Default.clientVanillaFID;
                     break;
                case "bc":
                    folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.bcCPatchFID : Properties.Settings.Default.clientBCFID;
                    break;
                case "wotlk":
                     folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.wotlkCPatchFID : Properties.Settings.Default.clientWotlkFID;
                     break;
                case "cata":
                    folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.cataCPatchFID : Properties.Settings.Default.clientCATAFID;
                    break;
                case "mop":
                     folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.mopCPatchFID : Properties.Settings.Default.clientMopFID;
                     break;
                case "wod":
                    folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.wodCPatchFID : Properties.Settings.Default.clientWODFID;
                    break;
                case "legion":
                     folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.legionCPatchFID : Properties.Settings.Default.clientLegionFID;
                     break;
                case "bfa":
                    folderId = type == DownloadTypes.Custom ? Properties.Settings.Default.bfaCPatchFID : Properties.Settings.Default.clientBFAFID;
                    break;
                default:
                     folderId = null;
                     break;
             }

            if (String.IsNullOrEmpty(folderId))
                return;

            Proceed(true);
            Download(folderId, action);
        }

        public void Cancel()
        {
            webClient.CancelAsync();
        }

        private void Proceed(bool status)
        {
            Reset();

            _isDownloading = status;

            _mw.DownloadBar.Visibility = status ? Visibility.Visible : Visibility.Hidden;

            _mw.btn_play.IsEnabled = !status;
            _mw.btn_download.IsEnabled = !status;

            if (_mw.btn_play.Visibility == Visibility.Visible)
            {
                _mw.btn_repair.Visibility = status ? Visibility.Hidden : Visibility.Visible;
                _mw.btn_custom.Visibility = status ? Visibility.Hidden : Visibility.Visible;
                _mw.btn_custom_remove.Visibility = status ? Visibility.Hidden : Visibility.Visible;
            }

            _mw.btn_download_cancel.Visibility = status ? Visibility.Visible : Visibility.Hidden;

            _mw.ni.ContextMenuStrip.Items[0].Visible = status;

            if (status)
            {
                start_timer = DateTime.Now;
                base_timer.Start();
            }
            else
            {
                sw_cf.Stop();
                base_timer.Stop();
                _mw.LoadClientStatus();
            }
        }

        private async void Download(string folder_id, DownloadActions action)
        {
            _mw.labelmsg.Content = "Initialization of download...";
            await PopulateItemsQueueAsync(new ClientItemInfo() { id = folder_id, mimeType = "application/vnd.google-apps.folder" }, Properties.Settings.Default.clientPath, action);

            _mw.progress_total_bytes.Content = detectSizeToString(dwn_items_bytes);
            _mw.ni.ContextMenuStrip.Items[0].Text = String.Format("Downloading progress - {0} ({1:0}%)", detectSizeToString(dwn_items_bytes), 0);

            _mw.labelmsg.Content = "Downloading files...";
            DownloadQueuedItemsAsync();
        }

        private Task PopulateItemsQueueAsync(ClientItemInfo itemResource, string path, DownloadActions action)
        {
            return Task.Factory.StartNew(() => PopulateItemsQueue(itemResource, path, action));
        }

        private void PopulateItemsQueue(ClientItemInfo itemResource, string path, DownloadActions action)
        {
            if (!itemResource.mimeType.Equals("application/vnd.google-apps.folder"))
            {
                var file = Directory.GetFiles(path, itemResource.name, SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

                switch (action)
                {
                    case DownloadActions.Remove:
                        {
                            if (file != null)
                                File.Delete(Path.Combine(path, itemResource.name));
                            break;
                        }
                    default:
                        {
                            if (file == null || !GetFileSize(file).Equals(itemResource.size) && verifyMd5Hash(GetMD5HashFromFile(file), itemResource.md5Checksum) ||
                                !verifyMd5Hash(GetMD5HashFromFile(file), itemResource.md5Checksum))
                            {
                                QueuedFileInfo queuedFileInfo = new QueuedFileInfo();
                                queuedFileInfo.url = DriveService.NormalizedWebContentLink(itemResource.id);
                                queuedFileInfo.localDest = path;
                                queuedFileInfo.name = itemResource.name;
                                queuedFileInfo.extension = itemResource.fullFileExtension;
                                queuedFileInfo.size = itemResource.size;

                                ++dwn_queued_item_counter;

                                dwn_items_bytes += queuedFileInfo.size;

                                downloadQueue.Enqueue(queuedFileInfo);
                            }
                            break;
                        }
                }
            }
            else
            {
                string NewPath = String.IsNullOrEmpty(itemResource.name) ? path : Path.Combine(path, itemResource.name);

                Directory.CreateDirectory(NewPath);

                var childrens = DriveService.GetChildrens(itemResource.id);
                if (childrens != null)
                {
                    foreach (var item in childrens)
                        PopulateItemsQueue(item, NewPath, action);
                }
            }
        }

        private void DownloadQueuedItemsAsync(bool error = false)
        {
            if (!downloadQueue.Any())
            {
                Proceed(false);
                return;
            }

            sw_cf.Restart();

            var dwnItem = error ? _prevQueuedFileInfo : downloadQueue.Dequeue();

            _prevQueuedFileInfo = dwnItem;

            string fileName = dwnItem.name.Length > 24 ? dwnItem.name.Substring(0, 24) + "..." : dwnItem.name;
            _mw.download_progress_files_label.Content = String.Format("({1}/{2}) {0}", fileName, dwn_item_counter, dwn_queued_item_counter);

            webClient.DownloadFileAsync(new Uri(dwnItem.url), Path.Combine(dwnItem.localDest, dwnItem.name));
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _mw.progress_file.Value = e.ProgressPercentage;
            long bytesPerSecond = (long)(e.BytesReceived / sw_cf.Elapsed.TotalSeconds);
            _mw.download_progress_bytes.Content = String.Format("{0} of {1} - {2}/с", detectSizeToString(e.BytesReceived), detectSizeToString(e.TotalBytesToReceive), detectSizeToString(bytesPerSecond));
            dwn_last_bytes = e.BytesReceived;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Proceed(false);
            }
            else if (e.Error != null)
            {
                ++dwn_retryCount;

                if (dwn_retryCount < 10)
                    DownloadQueuedItemsAsync(true);
                else
                {
                    MessageBox.Show("Unable to download the file, try again", "File download error", MessageBoxButton.OK, MessageBoxImage.Error);
                    webClient.CancelAsync();
                }
            }
            else
            {
                dwn_retryCount = 0;
                ++dwn_item_counter;

                dwn_completed_bytes += dwn_last_bytes;

                double progress = dwn_completed_bytes * 100.0 / dwn_items_bytes;
                _mw.progress_total.Value = progress;
                _mw.progress_total_pct.Content = String.Format("{0:0}%", progress);

                long dwn_bytes_left = dwn_items_bytes - dwn_completed_bytes;
                _mw.progress_total_bytes.Content = detectSizeToString(dwn_bytes_left);
                _mw.ni.ContextMenuStrip.Items[0].Text = String.Format("Downloading progress - {1} ({0:0}%)", progress, detectSizeToString(dwn_bytes_left));

                DownloadQueuedItemsAsync();
            }
        }

        public static string detectSizeToString(long value)
        {
            try
            {
                if (value >= 1073741824)
                    return String.Format("{0:0.00}GB", Convert.ToDouble(value) / 1024 / 1024 / 1024);
                else if (value >= 1048576)
                    return String.Format("{0:0.00}MB", Convert.ToDouble(value) / 1024 / 1024);
                else
                    return String.Format("{0:0}KB", Convert.ToDouble(value) / 1024);
            }
            catch
            {
                return "∞ B";
            }
        }
    }
}
