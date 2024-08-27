using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using log4net;

namespace BroadcastClient.Data
{
    public class ScreenReceiver
    {
        private readonly ILog log;
        private readonly Dispatcher dispatcher;
        private readonly Image display;
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;

        public ScreenReceiver(ILog log, Dispatcher dispatcher, Image display)
        {
            this.log = log;
            this.dispatcher = dispatcher;
            this.display = display;
        }

        public async Task StartReceiving(int port)
        {
            udpClient = new UdpClient(port);
            serverEndPoint = new IPEndPoint(IPAddress.Any, port);

            int frameCount = 0;
            var timer = new Timer(state =>
            {
                log.Info($"Кадров получено за 10 секунд: {frameCount}");
                frameCount = 0;
            }, null, 10000, 10000);

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    byte[] buffer = result.Buffer;

                    log.Debug($"Получен файл размером: {buffer.Length} байт");

                    await dispatcher.InvokeAsync(() => 
                    {
                        MemoryStream ms = new MemoryStream(buffer);
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        display.Source = bitmap;
                    });
                    frameCount++;
                }
                catch (Exception ex)
                {
                    log.Error("Ошибка при получении данных", ex);
                    await dispatcher.InvokeAsync(() => MessageBox.Show("Ошибка получения данных"));
                }
            }
        }
        public void StopReceiving()
        {
            udpClient?.Dispose();
        }
    }
}
