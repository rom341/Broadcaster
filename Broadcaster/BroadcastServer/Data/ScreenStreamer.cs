using log4net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BroadcastServer.Data
{
    public class ScreenStreamer
    {
        private readonly ILog log;
        private UdpClient udpClient;
        private IPEndPoint clientEndPoint;

        public ScreenStreamer(ILog log)
        {
            this.log = log;
        }

        private Bitmap CaptureScreen()
        {
            var screenSize = new Size(640, 480);
            var bitmap = new Bitmap(screenSize.Width, screenSize.Height, PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, screenSize);
            }
            log.Info($"Captured screen in format: {bitmap.PixelFormat}");
            return bitmap;
        }

        private byte[] GetCompressedByteArray(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                image.Save(ms, jpegEncoder, encoderParameters);
                return ms.ToArray();
            }
        }
        private byte[] GetByteArray(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public async Task SendScreenAsync(string clientIp, int port)
        {
            udpClient = new UdpClient();
            clientEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), port);

            int frameCount = 1;
            var timer = new Timer(state =>
            {
                log.Info($"Frames sent in the last 10 seconds: {frameCount}");
                frameCount = 0;
            }, null, 10000, 10000);

            while (true)
            {
                try
                {
                    Bitmap screen = CaptureScreen();
                    byte[] buffer = GetByteArray(screen);

                    log.Debug($"Size of the image to be sent: {buffer.Length} bytes");

                    await udpClient.SendAsync(buffer, buffer.Length, clientEndPoint);

                    log.Info($"File sent, size: {buffer.Length} bytes");

                    frameCount++;

                    await Task.Delay(30); // Delay to reduce load
                }
                catch (Exception ex)
                {
                    log.Error("Error sending data", ex);
                    break;
                }
                udpClient.Dispose();
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
    }
}
