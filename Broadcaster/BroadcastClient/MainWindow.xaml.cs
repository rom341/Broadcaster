using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using log4net;
using log4net.Config;
using System.IO;

namespace BroadcastClient
{
    public partial class MainWindow : Window
    {
        private readonly ILog log;
        private readonly ScreenReceiver screenReceiver;

        public MainWindow()
        {
            InitializeComponent();
            log = LogManager.GetLogger(typeof(MainWindow));
            XmlConfigurator.Configure(new FileInfo("C:\\Users\\Roman\\source\\repos\\Broadcaster\\BroadcastClient\\log4net.config"));

            screenReceiver = new ScreenReceiver(log, this.Dispatcher, DisplayImage);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string serverIP = ServerIpInput.Text;
            int serverPort = int.Parse(ServerPortInput.Text);
            int clientPort = serverPort;//serverPort + 1;

            screenReceiver.StopReceiving();
            screenReceiver.StartReceiving(serverIP, serverPort, clientPort);
        }
    }
}
