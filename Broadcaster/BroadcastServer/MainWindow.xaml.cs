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
        private UdpClient udpClient;
        private IPEndPoint broadcastEndPoint;

        public MainWindow()
        {
            InitializeComponent();
            log = LogManager.GetLogger(typeof(MainWindow));
            XmlConfigurator.Configure(new FileInfo("C:\\Users\\Roman\\source\\repos\\Broadcaster\\BroadcastClient\\log4net.config"));
            screenStreamer = new ScreenStreamer(log);

            string serverIp = GetLocalIPAddress();
            string broadcastIp = GetBroadcastAddress(serverIp, "255.255.255.0"); // Используем сетевую маску /24

            log.Info($"IP сервера: {serverIp}");
            log.Info($"Broadcast IP: {broadcastIp}");

            MessageBox.Show($"IP сервера: {serverIp}\nBroadcast IP: {broadcastIp}", "Информация о сервере");

            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            broadcastEndPoint = new IPEndPoint(IPAddress.Parse(broadcastIp), 5000);
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
                throw new Exception("Не удалось найти IPv4-адрес в системе.");
            return ipAddress.ToString();
        }

        private string GetBroadcastAddress(string ipAddress, string subnetMask)
        {
            var ipBytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            var maskBytes = IPAddress.Parse(subnetMask).GetAddressBytes();

            var broadcastBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastBytes).ToString();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            BroadcastStatus.Content = "Started!";
            log.Info("Сервер начал вещание через UDP...");

            while (true)
            {
                // Отправка данных всем клиентам через broadcast
                await screenStreamer.SendScreenAsync(broadcastEndPoint.Address.ToString(), broadcastEndPoint.Port);
            }
        }
    }
}
