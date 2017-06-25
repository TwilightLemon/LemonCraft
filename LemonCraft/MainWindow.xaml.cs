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
using System.Windows.Forms;

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
            Reporter.SetClientName("Lc-1.7.2");
            try {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft") && Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".jre"))
                    Asd.Text = "Minecraft  " + LauncherCore.Create().GetVersions().ToArray()[0].Assets;
            }catch { Asd.Text = "没有检测到MC，点我添加"; }
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
        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tit.Text == "")
            {
                LauncherCore dt;
                if (paths != "sss")
                    dt = LauncherCore.Create(paths);
                else dt = LauncherCore.Create();
                dt.JavaPath = AppDomain.CurrentDomain.BaseDirectory + @"jre8\bin\javaw.exe"; ;
                var ver=dt.GetVersions().ToArray()[0];
                var result = dt.Launch(new LaunchOptions
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
            else { tit.Text = "游戏数据尚未加载或加载失败，无法启动游戏"; await Task.Delay(2000); tit.Text = ""; }
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
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft")&& !Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".jre"))
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "jre.zip")&& File.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip"))
                {
                    tit.Text = "游戏数据加载中...(1/2)";
                    await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + "jre.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                    tit.Text = "游戏数据加载中...(2/2)";
                    await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                    tit.Text = "";
                }else if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "jre.zip")&& !File.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip"))
                {
                    tit.Text = "正在应用数据更改... (1/2)";
                    await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + "jre.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                    var s = new WebClient();
                    s.DownloadFileAsync(new Uri("http://pan.kzwr.com/kzwrfs?fid=dde342eefa164bec8a45408ce25ccfd1bzo1.zip"), AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip");
                    s.DownloadProgressChanged += delegate (object se, DownloadProgressChangedEventArgs es) { tit.Text = "下载游戏数据中...    " + (es.ProgressPercentage) + "%   (2/2)"; };
                    s.DownloadFileCompleted += async delegate
                    {
                        tit.Text = "正在应用数据更改... (2/2)";
                        await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                        s.Dispose();
                    };
                }
                else { tit.Text = "下载游戏数据中...";
                    System.Net.WebClient w = new System.Net.WebClient();
                    w.DownloadFileAsync(new Uri("http://pan.kzwr.com/kzwrfs?fid=c63a5591bce6404198b66e84ca36ecb0317r.zip"), AppDomain.CurrentDomain.BaseDirectory + "jre.zip");
                    w.DownloadProgressChanged += delegate (object se, DownloadProgressChangedEventArgs es) { tit.Text = "下载游戏数据中...    " + (es.ProgressPercentage) + "%   (1/2)"; };
                    w.DownloadFileCompleted += async delegate
                   {
                       tit.Text = "正在应用数据更改... (1/2)";
                       await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + "jre.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                       w.Dispose();
                       var s = new WebClient();
                       s.DownloadFileAsync(new Uri("http://pan.kzwr.com/kzwrfs?fid=dde342eefa164bec8a45408ce25ccfd1bzo1.zip"), AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip");
                       s.DownloadProgressChanged += delegate (object se, DownloadProgressChangedEventArgs es) { tit.Text = "下载游戏数据中...    " + (es.ProgressPercentage) + "%   (2/2)"; };
                       s.DownloadFileCompleted += async delegate
                       {
                           tit.Text = "正在应用数据更改... (2/2)";
                           await Task.Run(new Action(delegate { UnZip(AppDomain.CurrentDomain.BaseDirectory + ".minecraft.zip", AppDomain.CurrentDomain.BaseDirectory, ""); }));
                           s.Dispose();
                       };
                   };
                }
            }
        }
        string paths = "sss";
        private async void Asd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    paths = m_Dialog.SelectedPath;
                    Asd.Text = "Minecraft  " + LauncherCore.Create(m_Dialog.SelectedPath).GetVersions().ToArray()[0].Assets;
                }
                catch { tit.Text = "无效的目录"; await Task.Delay(2000); tit.Text="";}
            }
        }
    }
}
