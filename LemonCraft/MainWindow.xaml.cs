using ICSharpCode.SharpZipLib.Zip;
using KMCCC.Authentication;
using KMCCC.Launcher;
using LemonCraft.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace LemonCraft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserName.Text = Settings.Default.UserName;
        }
        /// <summary>   
        /// 解压功能   
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>   
        /// <param name="password">密码</param>   
        /// <returns>解压结果</returns>   
        private bool UnZip(string fileToUnZip, string zipedFolder, string password)
        {
            bool result = true;
            FileStream fs = null;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            string fileName;

            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                zipStream = new ZipInputStream(File.OpenRead(fileToUnZip.Trim()));
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = System.IO.Path.Combine(zipedFolder, ent.Name);
                        fileName = fileName.Replace('/', '\\');

                        if (fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        using (fs = File.Create(fileName))
                        {
                            int size = 2048;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = zipStream.Read(data, 0, data.Length);
                                if (size > 0)
                                    fs.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tit.Text == "")
            {
                LauncherCore Core = LauncherCore.Create();
                Reporter.SetClientName("Lc-1.7.2");
                var versions = Core.GetVersions().ToArray();
                Core.JavaPath = AppDomain.CurrentDomain.BaseDirectory + @"jre8\bin\javaw.exe";
                var ver = versions[0];
                var result = Core.Launch(new LaunchOptions
                {
                    Version = ver,
                    MaxMemory = 514,
                    Authenticator = new OfflineAuthenticator(UserName.Text),
                });
                Settings.Default.UserName = UserName.Text;
                Settings.Default.Save();
                var d = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
                d.Completed += delegate { Environment.Exit(0); };
                this.BeginAnimation(OpacityProperty, d);
            }
            else { tit.Text = "游戏数据尚未加载或加载失败，无法启动游戏"; }
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft"))
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data.zip"))
                {
                    tit.Text = "游戏数据加载中...";
                    await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + "Data.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                    tit.Text = "";
                }
                else { tit.Text = "下载游戏数据中...";
                    System.Net.WebClient w = new System.Net.WebClient();
                    w.DownloadFileAsync(new Uri("http://api.lemonapp.tk/Mc/Data.zip"), AppDomain.CurrentDomain.BaseDirectory + "Data.zip");
                    w.DownloadProgressChanged += delegate (object se, DownloadProgressChangedEventArgs es) { tit.Text = "下载游戏数据中...    " + (es.ProgressPercentage) + "%"; };
                    w.DownloadFileCompleted += async delegate
                    {
                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data.zip"))
                        {
                            tit.Text = "游戏数据加载中...";
                            await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + "Data.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                            tit.Text = "";
                        }
                        else { tit.Text = "游戏数据下载失败"; }
                    };
                }
            }
        }
    }
}
