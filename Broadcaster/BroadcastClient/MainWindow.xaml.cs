using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using log4net;
using log4net.Config;
using System.IO;
using BroadcastClient.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;

namespace BroadcastClient
{
    public partial class MainWindow : Window
    {
        private readonly ILog log;
        private readonly ScreenReceiver screenReceiver;
        private WriteableBitmap writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();
            log = LogManager.GetLogger(typeof(MainWindow));
            XmlConfigurator.Configure(new FileInfo("C:\\Users\\Roman\\source\\repos\\Broadcaster\\BroadcastClient\\log4net.config"));

            screenReceiver = new ScreenReceiver(log, this.Dispatcher, DisplayImage);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            screenReceiver.StopReceiving();
            await screenReceiver.StartReceiving(int.Parse(ServerIpInput.Text));
        }
    }
}
