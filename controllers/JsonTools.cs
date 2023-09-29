using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [CollectionDataContract]
    public class ServerStats : List<ServerStatsInfo>
    {
    }

    [DataContract]
    public struct ServerStatsInfo
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int realmid { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string server { get; set; }
        [DataMember]
        public int maxlvl { get; set; }
        [DataMember]
        public int online { get; set; }
        [DataMember]
        public string status { get; set; }
    }

    [CollectionDataContract]
    public class LauncherVideos : List<LauncherVideosInfo>
    {
    }

    [DataContract]
    public struct LauncherVideosInfo
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string source_url { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string server { get; set; }
        [DataMember]
        public string description { get; set; }
    }

    [CollectionDataContract]
    public class News : LinkedList<NewsInfo>
    {
    }

    [DataContract]
    public struct NewsInfo
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string body { get; set; }
        [DataMember]
        public string thumbnail_url { get; set; }
        [DataMember]
        public string server { get; set; }
    }

    [CollectionDataContract]
    public class ClientItems : List<ClientItemInfo>
    {
    }

    [DataContract]
    public struct ClientItemInfo
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string[] parents { get; set; }
        [DataMember]
        public string mimeType { get; set; }
        [DataMember]
        public string originalFilename { get; set; }
        [DataMember]
        public string fullFileExtension { get; set; }
        [DataMember]
        public string md5Checksum { get; set; }
        [DataMember]
        public string webContentLink { get; set; }
        [DataMember]
        public long size { get; set; }
    }

    [DataContract]
    public struct LauncherVersionInfo
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string version_hash { get; set; }
        [DataMember]
        public string version { get; set; }
    }

    [DataContract]
    public struct LauncherChangelogInfo
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string descr { get; set; }
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public string updated_at { get; set; }
    }

    public class JsonTools
    {
        private static T DeserializeJSON<T>(string json)
        {
            if (String.IsNullOrEmpty(json))
                return default(T);

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(ms);
            }
        }

        public static ServerStats DeserializeServerStatsJSON()
        {
            return DeserializeJSON<ServerStats>(GetServerStatusJSON());
        }

        public static ClientItems DeserializeClientItemsJSON(string url)
        {
            return DeserializeJSON<ClientItems>(GetClientItemsJSON(url));
        }

        public static News DeserializeNewsJSON()
        {
            return DeserializeJSON<News>(GetNewsJSON());
        }

        public static LauncherVersionInfo DeserializeLauncherVersionJSON()
        {
            return DeserializeJSON<LauncherVersionInfo>(GetLauncherVersionJSON());
        }

        public static LauncherChangelogInfo DeserializeLauncherChangelogJSON(string version)
        {
            return DeserializeJSON<LauncherChangelogInfo>(GetLauncherChangelogJSON(version));
        }

        public static LauncherVideos DeserializeLauncherVideosJSON()
        {
            return DeserializeJSON<LauncherVideos>(GetLauncherVideosJSON());
        }

        private static string GetClientItemsJSON(string url)
        {
            try
            {
                string result = new WebClient().DownloadString(url);

                if (result.Contains("files"))
                {
                    result = result.Substring(result.IndexOf('['));
                    result = result.Substring(0, result.LastIndexOf(']') + 1);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static Task<ServerStats> GetServersStatsAsync(string server)
        {
            return Task.Factory.StartNew(() =>
            {
                var json_obj = DeserializeServerStatsJSON();
                return FindServerStatsByServer(json_obj, server);
            });
        }

        public static Task<LauncherVideos> GetLauncherVideosAsync(string server)
        {
            return Task.Factory.StartNew(() =>
            {
                var json_obj = DeserializeLauncherVideosJSON();
                return FindLauncherVideosByServer(json_obj, server);
            });
        }

        public static Task<News> GetNewsAsync(string server)
        {
            return Task.Factory.StartNew(() =>
            {
                var json_obj = DeserializeNewsJSON();
                return FindNewsByServer(json_obj, server);
            });
        }

        public static Task<LauncherVersionInfo> GetLauncherVersionAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return DeserializeLauncherVersionJSON();
            });
        }

        public static Task<LauncherChangelogInfo> GetLauncherChangelogAsync(string version)
        {
            return Task.Factory.StartNew(() =>
            {
                return DeserializeLauncherChangelogJSON(version);
            });
        }

        private static string GetServerStatusJSON()
        {
            try
            {
                return new WebClient().DownloadString(Properties.Settings.Default.serverStatsURL);
            }
            catch
            {
                return null;
            }
        }

        private static string GetLauncherVideosJSON()
        {
            try
            {
                return new WebClient().DownloadString(Properties.Settings.Default.videosURL);
            }
            catch
            {
                return null;
            }
        }

        private static string GetNewsJSON()
        {
            try
            {
                return new WebClient().DownloadString(Properties.Settings.Default.newsURL);
            }
            catch
            {
                return null;
            }
        }

        private static string GetLauncherVersionJSON()
        {
            try
            {
                return new WebClient().DownloadString(Properties.Settings.Default.launcherVersionURL);
            }
            catch
            {
                return null;
            }
        }

        private static string GetLauncherChangelogJSON(string version)
        {
            try
            {
                return new WebClient().DownloadString(Properties.Settings.Default.launcherChangelogsURL + version);
            }
            catch
            {
                return null;
            }
        }

        private static LauncherVideos FindLauncherVideosByServer(LauncherVideos json_obj, string server)
        {
            if (json_obj == null)
                return null;

            LauncherVideos launcherVideInfoList = new LauncherVideos();
            foreach (var item in json_obj)
            {
                if (string.Equals(item.server, server))
                    launcherVideInfoList.Add(item);
            }
            return launcherVideInfoList;
        }

        private static ServerStats FindServerStatsByServer(ServerStats json_obj, string server)
        {
            if (json_obj == null)
                return null;

            ServerStats serverStatsInfoList = new ServerStats();
            foreach (var item in json_obj)
            {
                if (string.Equals(item.server, server))
                    serverStatsInfoList.Add(item);
            }
            return serverStatsInfoList;
        }

        private static News FindNewsByServer(News json_obj, string server)
        {
            if (json_obj == null)
                return null;

            News newsInfoList = new News();
            foreach (var item in json_obj)
            {
                if (string.Equals(item.server, server))
                    newsInfoList.AddLast(item);
            }
            return newsInfoList;
        }
    }
}
