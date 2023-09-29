using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Launcher
{
    public class GameClientTools
    {
        #region Variables
        #region Dll's
        // Post message to process
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        #endregion
        #endregion

        public static bool ValidateClienBuild(string path)
        {
            try
            {
                int[] ClientBuilds = {
                    Properties.Settings.Default.clientBuildVanilla,
                    Properties.Settings.Default.clientBuildBC,
                    Properties.Settings.Default.clientBuildWotlk,
                    Properties.Settings.Default.clientBuildCATA,
                    Properties.Settings.Default.clientBuildMop,
                    Properties.Settings.Default.clientBuildWOD,
                    Properties.Settings.Default.clientBuildLegion,
                    Properties.Settings.Default.clientBuildBFA
                };
                string[] clientVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(path, Properties.Settings.Default.binaryFileName)).FileVersion.Split(',');
                return ClientBuilds.Contains(int.Parse(clientVersionInfo[3]));
            }
            catch
            {
                // TODO: add exception message
                return false;
            }
        }

        public static long GetClientSize()
        {
            return Directory.GetFiles(Properties.Settings.Default.clientPath, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
        }

        public static string GetClientVersionByName(string server)
        {
            switch (server)
            {
                case "vanilla":
                    return String.Format("WoW Vanilla {0}({1})", Properties.Settings.Default.clientVersionVanilla, Properties.Settings.Default.clientBuildVanilla);
                case "bc":
                    return String.Format("WoW BC {0}({1})", Properties.Settings.Default.clientVersionBC, Properties.Settings.Default.clientBuildBC);
                case "wotlk":
                    return String.Format("WoW WoTLK {0}({1})", Properties.Settings.Default.clientVersionWotlk, Properties.Settings.Default.clientBuildWotlk);
                case "cata":
                    return String.Format("WoW Cataclysm {0}({1})", Properties.Settings.Default.clientVersionCATA, Properties.Settings.Default.clientBuildCATA);
                case "mop":
                    return String.Format("WoW MoP {0}({1})", Properties.Settings.Default.clientVersionMop, Properties.Settings.Default.clientBuildMop);
                case "wod":
                    return String.Format("WoW WoD {0}({1})", Properties.Settings.Default.clientVersionWOD, Properties.Settings.Default.clientBuildWOD);
                case "legion":
                    return String.Format("WoW Legion {0}({1})", Properties.Settings.Default.clientVersionLegion, Properties.Settings.Default.clientBuildLegion);
                case "bfa":
                    return String.Format("WoW BFA {0}({1})", Properties.Settings.Default.clientVersionBFA, Properties.Settings.Default.clientBuildBFA);
                default:
                    return null;
            }
        }

        public static string GetClientVersionByPath(string path)
        {
            string server = GetClientExpansion(path);
            switch (server)
            {
                case "vanilla":
                    return String.Format("WoW Vanilla {0}({1})", Properties.Settings.Default.clientVersionVanilla, Properties.Settings.Default.clientBuildVanilla);
                case "bc":
                    return String.Format("WoW BC {0}({1})", Properties.Settings.Default.clientVersionBC, Properties.Settings.Default.clientBuildBC);
                case "wotlk":
                    return String.Format("WoW WoTLK {0}({1})", Properties.Settings.Default.clientVersionWotlk, Properties.Settings.Default.clientBuildWotlk);
                case "cata":
                    return String.Format("WoW Cataclysm {0}({1})", Properties.Settings.Default.clientVersionCATA, Properties.Settings.Default.clientBuildCATA);
                case "mop":
                    return String.Format("WoW MoP {0}({1})", Properties.Settings.Default.clientVersionMop, Properties.Settings.Default.clientBuildMop);
                case "wod":
                    return String.Format("WoW WoD {0}({1})", Properties.Settings.Default.clientVersionWOD, Properties.Settings.Default.clientBuildWOD);
                case "legion":
                    return String.Format("WoW Legion {0}({1})", Properties.Settings.Default.clientVersionLegion, Properties.Settings.Default.clientBuildLegion);
                case "bfa":
                    return String.Format("WoW BFA {0}({1})", Properties.Settings.Default.clientVersionBFA, Properties.Settings.Default.clientBuildBFA);
                default:
                    return null;
            }
        }

        public static string GetClientExpansion(string path)
        {
            try
            {
                string[] clientVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(path, Properties.Settings.Default.binaryFileName)).FileVersion.Split(',');
                int v = int.Parse(clientVersionInfo[3]);
                switch (v)
                {
                    // reference https://wowwiki.fandom.com/wiki/Public_client_builds
                    case 5875:
                        return "vanilla";
                    case 8606:
                        return "bc";
                    case 12340:
                        return "wotlk";
                    case 13205:
                    case 13329:
                    case 13596:
                    case 13623:
                    case 14007:
                    case 14480:
                    case 14545:
                    case 15211:
                    case 15354:
                    case 15595:
                        return "cata";
                    case 16057:
                    case 16135:
                    case 16309:
                    case 16357:
                    case 16826:
                    case 17128:
                    case 17359:
                    case 17371:
                    case 17399:
                    case 17538:
                    case 17658:
                    case 17688:
                    case 17956:
                    case 18019:
                    case 18291:
                    case 18414:
                        return "mop";
                    case 20886:
                    case 21315:
                    case 21336:
                    case 21348:
                    case 21355:
                    case 21463:
                    case 21676:
                    case 21742:
                        return "wod";
                    case 22522:
                    case 22566:
                    case 22594:
                    case 22624:
                    case 22747:
                    case 22810:
                    case 22900:
                    case 22908:
                    case 22950:
                    case 22989:
                    case 22995:
                    case 22996:
                    case 23171:
                    case 23222:
                    case 23360:
                    case 23420:
                    case 23826:
                    case 23835:
                    case 23836:
                    case 23846:
                    case 23852:
                    case 23857:
                    case 23877:
                    case 23911:
                    case 23937:
                    case 24015:
                    case 24330:
                    case 24367:
                    case 24415:
                    case 24430:
                    case 24461:
                    case 24742:
                    case 24920:
                    case 24931:
                    case 25021:
                    case 25383:
                    case 25442:
                    case 25455:
                    case 25480:
                    case 25497:
                    case 25549:
                    case 25848:
                    case 25860:
                    case 25864:
                    case 25875:
                    case 25881:
                    case 25901:
                    case 25928:
                    case 25937:
                    case 25961:
                    case 25996:
                    case 26124:
                    case 26365:
                    case 26654:
                    case 26822:
                    case 26899:
                    case 26972:
                        return "legion";
                    case 27355:
                    case 27366:
                    case 27377:
                    case 27404:
                    case 27481:
                    case 27547:
                    case 27602:
                    case 27843:
                    case 27980:
                    case 28153:
                    case 28724:
                    case 28768:
                    case 28807:
                    case 28822:
                    case 28833:
                    case 29088:
                    case 29139:
                    case 29235:
                    case 29297:
                    case 29482:
                    case 29600:
                    case 29621:
                    case 29683:
                    case 29701:
                    case 29718:
                    case 29732:
                    case 29737:
                    case 29814:
                    case 29869:
                    case 29896:
                    case 29981:
                    case 30477:
                    case 30706:
                    case 30920:
                    case 30948:
                    case 30993:
                    case 31229:
                    case 31429:
                    case 31478:
                        return "bfa";
                    default:
                        return "unknown";
                }
            }
            catch
            {
                // TODO: add exception message
                return null;
            }
        }

        public static bool FindClient(string path)
        {
            return File.Exists(Path.Combine(path, Properties.Settings.Default.binaryFileName));
        }

        public static bool ClientValidation(string path)
        {
            return FindClient(path) && ValidateClienBuild(path);
        }

        public static string GetClientLocale()
        {
            List<string> clientLocales = GetClientLocales();
            string result = null;
            foreach (var locale in clientLocales)
            {
                result += locale;
            }
            return String.IsNullOrEmpty(result) ? "UNKNOWN" : Regex.Replace(result, "[a-zA-Z]{4}", "$0 ").Trim();
        }

        public static List<string> GetClientLocales()
        {
            List<string> clientLocales = new List<string>();
            string[] locales = Properties.Settings.Default.clientLocales.Split(',');
            foreach (var locale in locales)
            {
                if (Directory.Exists(Path.Combine(Properties.Settings.Default.clientPath, "Data", locale)))
                    clientLocales.Add(locale);
            }
            return clientLocales;
        }

        public static void DeleteClientCache(string path)
        {
            string cpath = Path.Combine(path, "Cache");
            if (Directory.Exists(cpath))
                Directory.Delete(cpath, true);
        }

        public static void UpdateClientRealmlist(string path)
        {
            string expansion = GetClientExpansion(path);
            string realmlist = GetRelmlistByClientExpansion(expansion);
            if (String.IsNullOrEmpty(realmlist))
                return;

            switch (expansion)
            {
                case "vanilla":
                case "bc":
                {
                    string cpath = Path.Combine(path, Properties.Settings.Default.realmlistFileName);
                    if (!File.Exists(cpath))
                    {
                        StreamWriter writer = new StreamWriter(cpath);
                        writer.WriteLine(realmlist);
                        writer.Flush();
                        writer.Close();
                    }
                    break;
                }
                default:
                {
                    List<string> clientLocales = GetClientLocales();
                    foreach (var locale in clientLocales)
                    {
                        string cpath = Path.Combine(path, "Data", locale, Properties.Settings.Default.realmlistFileName);
                        if (!File.Exists(cpath))
                            continue;

                        StreamWriter writer = new StreamWriter(cpath);
                        writer.WriteLine(realmlist);
                        writer.Flush();
                        writer.Close();
                    }
                    break;
                }
            }
        }

        public static string GetRelmlistByClientExpansion(string expansion)
        {
            switch (expansion)
            {
                case "vanilla":
                    return Properties.Settings.Default.realmlistVanilla;
                case "bc":
                    return Properties.Settings.Default.realmlistBC;
                case "wotlk":
                    return Properties.Settings.Default.realmlistWotlk;
                case "cata":
                    return Properties.Settings.Default.realmlistCATA;
                case "mop":
                    return Properties.Settings.Default.realmlistMop;
                case "wod":
                    return Properties.Settings.Default.realmlistWOD;
                case "legion":
                    return Properties.Settings.Default.realmlistLegion;
                case "bfa":
                    return Properties.Settings.Default.realmlistBFA;
                default:
                    return null;
            }
        }

        public static void RunGame()
        {
            string cpath = Properties.Settings.Default.clientPath;
            if (!ClientValidation(cpath))
            {
                MessageBox.Show("The World of Warcraft game launch file is not found!", "Game startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DeleteClientCache(cpath);
            UpdateClientRealmlist(cpath);
            StartClientProcess(cpath);
        }

        public static void SelectGameFolder(out string selectedPath)
        {
            selectedPath = null;
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            folder.Description = "Select a game client folder";
            folder.RootFolder = Environment.SpecialFolder.MyComputer;
            folder.ShowNewFolderButton = false;
            System.Windows.Forms.DialogResult result = folder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(Path.Combine(folder.SelectedPath, Properties.Settings.Default.binaryFileName)))
                    selectedPath = folder.SelectedPath;
                else
                    MessageBox.Show(String.Format("File \"{0}\" not found!\nPlease select the correct game folder!", Properties.Settings.Default.binaryFileName), "Folder selection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void StartClientProcess(string cpath)
        {
            try
            {
                var process = Process.Start(Path.Combine(cpath, Properties.Settings.Default.binaryFileName));
                Thread.Sleep(1000);

                if (Properties.Settings.Default.autologin)
                {
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;

                            // Set keycodes
                            uint WM_KEYDOWN = 0x0100;
                            uint WM_KEYUP = 0x0101;
                            uint WM_CHAR = 0x0102;
                            uint VK_RETURN = 0x0D;
                            uint VK_TAB = 0x09;

                            do // Keep repeating till window is idle
                            {
                                process.WaitForInputIdle();
                                process.Refresh();
                            } while (process.MainWindowHandle.ToInt32() == 0);

                            // Sleep for a little to give the insides time to load
                            Thread.Sleep(1000);

                            foreach (char accNameLetter in Properties.Settings.Default.username)
                            {
                                SendMessage(process.MainWindowHandle, WM_CHAR, new IntPtr(accNameLetter), IntPtr.Zero);
                                Thread.Sleep(30);
                            }

                            SendMessage(process.MainWindowHandle, WM_KEYUP, new IntPtr(VK_TAB), IntPtr.Zero);
                            SendMessage(process.MainWindowHandle, WM_KEYDOWN, new IntPtr(VK_TAB), IntPtr.Zero);

                            // Send the password one key at a time
                            foreach (char accPassLetter in Properties.Settings.Default.password)
                            {
                                SendMessage(process.MainWindowHandle, WM_CHAR, new IntPtr(accPassLetter), IntPtr.Zero);
                                Thread.Sleep(30);
                            }

                            // Hit enter to log in
                            SendMessage(process.MainWindowHandle, WM_KEYUP, new IntPtr(VK_RETURN), IntPtr.Zero);
                            SendMessage(process.MainWindowHandle, WM_KEYDOWN, new IntPtr(VK_RETURN), IntPtr.Zero);

                            Thread.CurrentThread.Abort();
                        }
                        catch
                        {
                            Thread.CurrentThread.Abort();
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error of process initialization", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
