﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ControlClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static Socket server;  //数据源服务器
        static string msg = "hold";  //默认发送数据
        static int  rowNum = 4;
        static int cloumnNum = 4;
        string[,] gamePath = new string[rowNum, cloumnNum];
        public MainWindow()
        {
            InitializeComponent();
            initGame(this);
            startServer();
        }

        private void topTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            Point position = e.GetPosition(topTitle);

            // 如果鼠标位置在标题栏内，允许拖动  
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (position.X >= 0 && position.X < topTitle.ActualWidth && position.Y >= 0 && position.Y < topTitle.ActualHeight)
                {
                    this.DragMove();
                }
            }  
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(-1);
        }

        private void Scale_click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void zuixiaohua(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int niconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);  
        private void AddGame(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            String fileName = openFile.FileName;
            if (fileName != null && !"".Equals(fileName)) {
                if (!fileName.EndsWith(".exe"))
                {
                    MessageBox.Show("文件格式错误", "出错了");
                }
                else { 
                doUpdate(this, fileName);
                }
            }
        }
        private void startGame(object sender, RoutedEventArgs e)
        {
           Image img =  (Image)sender;
            int row = Grid.GetRow(img);
            int column = Grid.GetColumn(img);
            Process.Start(@gamePath[row, row]); 
    
        }
        //初始化游戏
        public static void initGame(MainWindow main)
        {
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (!key.Contains("IP"))
                {
                    String path = config.AppSettings.Settings[key].Value.ToString();
                    doUpdate(main, path);
                }
            }
  
        }
        //更新游戏
        public static void doUpdate(MainWindow main,String fileName)
        {
            IntPtr[] largeIcons, smallIcons;  //存放大/小图标的指针数组  
            string appPath = @fileName;
            //第一步：获取程序中的图标数         
            int IconCount = ExtractIconEx(appPath, -1, null, null, 0);
            //第二步：创建存放大/小图标的空间    
            largeIcons = new IntPtr[IconCount];
            smallIcons = new IntPtr[IconCount];
            //第三步：抽取所有的大小图标保存到largeIcons和smallIcons中        
            ExtractIconEx(appPath, 0, largeIcons, smallIcons, IconCount);
            //  for (int i = 0; i < IconCount; i++)
            //    {
            System.Drawing.Icon newIcon = System.Drawing.Icon.FromHandle(largeIcons[0]);
            System.Drawing.Bitmap bmp = newIcon.ToBitmap();
            BitmapSource returnSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Image img = new Image();
            img.Source = returnSource;
            int nowColumn = Grid.GetColumn(main.addGame);
            int nowRow = Grid.GetRow(main.addGame);
            if (nowRow == cloumnNum - 1 && nowColumn == rowNum - 1)
            {
                MessageBox.Show("游戏数目已达上限", "出错了");
            }
            else if (nowColumn == cloumnNum - 1)  //换行
            {
                Grid g = new Grid();
                RowDefinition rw1 = new RowDefinition();
                RowDefinition rw2 = new RowDefinition();
                g.RowDefinitions.Add(rw1);
                g.RowDefinitions.Add(rw2);
                g.Children.Add(img);
                Grid.SetRow(img, 0);
                int subStart = fileName.LastIndexOf("\\");
                int subEnd = fileName.LastIndexOf(".exe");
                String gameName = fileName.Substring(subStart + 1, subEnd - subStart - 1);
                Label l = new Label();
                l.Content = gameName;
                l.HorizontalAlignment = HorizontalAlignment.Center;
                g.Children.Add(l);
                Grid.SetRow(l, 1);
                main.gameContainer.Children.Add(g);
                Grid.SetColumn(g, nowColumn);
                Grid.SetRow(g, nowRow);
                main.gamePath[nowRow, nowColumn] = fileName;
                img.MouseLeftButtonUp += main.startGame;  //添加鼠标事件                        
                Grid.SetColumn(main.addGame, 0);
                Grid.SetRow(main.addGame, nowRow + 1);
                //写配置文件
                Utils.UpdateAppConfig(nowRow.ToString() + nowColumn, fileName);
            }
            else
            {
                Grid g = new Grid();
                RowDefinition rw1 = new RowDefinition();
                RowDefinition rw2 = new RowDefinition();
                g.RowDefinitions.Add(rw1);
                g.RowDefinitions.Add(rw2);
                g.Children.Add(img);
                Grid.SetRow(img, 0);
                int subStart = fileName.LastIndexOf("\\");
                int subEnd = fileName.LastIndexOf(".exe");
                String gameName = fileName.Substring(subStart + 1, subEnd - subStart-1);
                Label l = new Label();
                l.Content = gameName;
                l.HorizontalAlignment = HorizontalAlignment.Center;
                g.Children.Add(l);
                Grid.SetRow(l, 1);
                main.gameContainer.Children.Add(g);
                Grid.SetColumn(g, nowColumn);
                Grid.SetRow(g, nowRow);
                main.gamePath[nowRow, nowColumn] = fileName;
                img.MouseLeftButtonUp += main.startGame;
                Grid.SetColumn(main.addGame, nowColumn + 1);
                //写配置文件
                Utils.UpdateAppConfig(nowRow.ToString() + nowColumn, fileName);
            }
        }
        //清空游戏
        private void ClearGame(object sender, MouseButtonEventArgs e)
        {
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {     
                if(!key.Contains("IP"))
                config.AppSettings.Settings.Remove(key);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            gameContainer.Children.Clear();
            gameContainer.Children.Add(addGame);
            Grid.SetColumn(addGame, 0);
            Grid.SetRow(addGame, 0);
        }

        //登录窗口
        private void Login(object sender, RoutedEventArgs e)
        {
            Login login =  new Login();
            login.Owner = this;
            login.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            login.ShowDialog();
        }

        private void ShowData_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                showData.Content = "hold";
                msg = "hold";
            }
            if (e.Key == Key.Left)
            {
                showData.Content = "hold";
                msg = "hold";
            }
        }
       private void ShowData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                msg = "right";
                showData.Content = "right";
            }
            if (e.Key == Key.Left)
            {
                msg = "left";
                showData.Content = "left";
            }
        }
    
       static void startServer()
       {
           server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
           String localIP = Utils.getConfig("localIP");
           if (localIP != null)
           {
               try
               {
                   server.Bind(new IPEndPoint(IPAddress.Parse(localIP), 6001));//绑定端口号和IP
                   Thread t = new Thread(sendMsg);//开启发送消息线程
                   t.IsBackground = true;
                   t.Start();
               }catch(Exception){
                   MessageBox.Show("本机IP地址无效", "出错了");
               }
           }else
           {
               MessageBox.Show("本机IP地址不能为空", "出错了");
           }
       }
 
        //发送数据
        static void sendMsg()
        {
            String targetIP = Utils.getConfig("targetIP");
            if (targetIP != null) {
                try
                {
                    EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000);
                    while (true)
                    {
                        server.SendTo(Encoding.UTF8.GetBytes(msg), point);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("目标IP地址无效", "出错了");
                }
            }
            else
            {
                MessageBox.Show("目标IP地址不能为空","出错了");
            }
        }

    }
}