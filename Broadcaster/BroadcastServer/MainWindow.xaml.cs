using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using log4net;
using log4net.Config;
using System.IO;
using BroadcastServer.Data;

namespace BroadcastServer
{
    public partial class MainWindow : Window
    {
        private readonly ILog log;
        private readonly ScreenStreamer screenStreamer;

        public MainWindow()
        {
            InitializeComponent();
            log = LogManager.GetLogger(typeof(MainWindow));
            XmlConfigurator.Configure(new FileInfo("C:\\Users\\Roman\\source\\repos\\Broadcaster\\BroadcastClient\\log4net.config"));
            screenStreamer = new ScreenStreamer(log);
            //screenStreamer.screenBounds = new System.Drawing.Rectangle(0, 0, 640, 480);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            BroadcastStatus.Content = "Started!";
            log.Info("Сервер начал вещание через UDP...");

            screenStreamer.StartStreaming();
        }
    }
}
