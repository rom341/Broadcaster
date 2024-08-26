using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BroadcastClient.Data
{
    public class ScreenReceiver
    {
        private readonly ILog log;
        private readonly Dispatcher dispatcher;
        private readonly Image display;

        public ScreenReceiver(ILog log, Dispatcher dispatcher, Image display)
        {
            this.log = log;
            this.dispatcher = dispatcher;
            this.display = display;
        }

        private async Task ReceiveScreen(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            try
            {
                while (true)
                {
                    byte[] sizeBuffer = new byte[sizeof(int)];
                    int bytesRead = await stream.ReadAsync(sizeBuffer, 0, sizeof(int));

                    if (bytesRead == 0)
                    {
                        log.Info("Соединение закрыто сервером");
                        break;
                    }

                    int size = BitConverter.ToInt32(sizeBuffer, 0);
                    log.Debug($"Ожидаемый размер файла: {size} байт");

                    byte[] buffer = new byte[size];
                    int totalRead = 0;
                    while (totalRead < size)
                    {
                        int read = await stream.ReadAsync(buffer, totalRead, size - totalRead);
                        if (read == 0)
                        {
                            log.Info("Соединение закрыто сервером");
                            break;
                        }
                        totalRead += read;
                    }

                    log.Info($"Файл получен, размер: {totalRead} байт");

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        await dispatcher.InvokeAsync(() => display.Source = bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Ошибка при получении данных", ex);
                await dispatcher.InvokeAsync(() => MessageBox.Show("Ошибка получения данных"));
            }
            finally
            {
                client.Close();
                log.Info("Отключение от сервера");
            }
        }


        public async Task StartReceiving(string serverIp)
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(serverIp, 5000);
            log.Info("Подключение к серверу...");
            await Task.Run(() => ReceiveScreen(client));
        }
    }
}
