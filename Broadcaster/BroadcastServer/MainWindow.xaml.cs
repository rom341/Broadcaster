using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
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
using BroadcastServer.Data;
using log4net;
using log4net.Config;
using System.IO;

namespace BroadcastServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            BroadcastStatus.Content = "Started!";
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            log.Info("Сервер запущен и ожидает подключения...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                log.Info("Клиент подключен");
                Thread thread = new Thread(() => screenStreamer.SendScreen(client));
                thread.Start();
            }
        }
    }
}
