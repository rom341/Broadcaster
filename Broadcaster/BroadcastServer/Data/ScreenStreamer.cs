using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BroadcastServer.Data
{
    public class ScreenStreamer
    {
        private readonly ILog log;
        private int frameCount = 0;
        private readonly object lockObj = new object();

        public ScreenStreamer(ILog log)
        {
            this.log = log;
            StartLoggingTimer();
        }

        private Bitmap CaptureScreen()
        {
            var screenSize = new Size(1920, 1080);
            var bitmap = new Bitmap(screenSize.Width, screenSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, screenSize);
            }
            return bitmap;
        }

        public async void SendScreen(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            try
            {
                while (true)
                {
                    Bitmap screen = CaptureScreen();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
                        ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                        screen.Save(ms, jpegEncoder, encoderParameters);

                        byte[] buffer = ms.ToArray();
                        log.Debug($"Размер файла для отправки: {buffer.Length} байт");

                        await stream.WriteAsync(BitConverter.GetBytes(buffer.Length), 0, sizeof(int));
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        await stream.FlushAsync();

                        log.Info($"Файл отправлен, размер: {buffer.Length} байт");

                        lock (lockObj)
                        {
                            ++frameCount;
                        }
                        await Task.Delay(50); // Задержка для уменьшения нагрузки
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Ошибка при отправке данных", ex);
            }
            finally
            {
                client.Close();
                log.Info("Клиент отключен");
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        private void StartLoggingTimer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000); // 10 секунд

                    int framesSent;
                    lock (lockObj)
                    {
                        framesSent = frameCount;
                        frameCount = 0; // Сброс счетчика после записи
                    }

                    log.Info($"Отправлено кадров за последние 10 секунд: {framesSent}");
                }
            });
        }
    }
}

