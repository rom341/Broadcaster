using log4net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace BroadcastServer.Data
{
    public class ScreenStreamer
    {
        private ILog logger;
        private UdpClient udpClient;
        private IPEndPoint broadcastEndPoint;
        FrequencyCounter frequencyCounter;
        private Thread streamingThread;
        private bool isRunning = false;

        public int port { get; private set; } = 5000;
        public int fps { get; private set; } = 60;
        public int interval { get; private set; } = 1000 / 60;  // Интервал для достижения 60 FPS
        public Rectangle screenBounds { get; set; } = Screen.PrimaryScreen.Bounds;

        private const int MaxPacketSize = 65000 - 8; // Учитываем размер заголовка
        private const int HeaderSize = 8; // 4 байта для ID пакета и 4 байта для общего числа пакетов

        public ScreenStreamer(ILog logger)
        {
            udpClient = new UdpClient();
            broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            frequencyCounter = new FrequencyCounter(1, () => 
            {
                logger.Info($"FPS: {frequencyCounter.GetFrequency()}");
            });
            udpClient.EnableBroadcast = true;
            this.logger = logger;
        }

        public void StartStreaming()
        {
            if (!isRunning)
            {
                logger.Info("Starting screen streaming.");
                isRunning = true;
                streamingThread = new Thread(StreamScreen);
                streamingThread.IsBackground = true;
                streamingThread.Start();
                frequencyCounter.Start();
            }
        }

        public void StopStreaming()
        {
            if (isRunning)
            {
                logger.Info("Stopping screen streaming.");
                isRunning = false;
                udpClient.Close();
                streamingThread?.Join();
                frequencyCounter.Stop();
            }
        }

        private void StreamScreen()
        {
            while (isRunning)
            {
                try
                {
                    var screenshot = CaptureScreen();
                    var byteArrayFromImage = ImageToByte(screenshot);

                    SendImageInChunks(byteArrayFromImage);

                    frequencyCounter.IncrementCounter();
                    //Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    logger.Error("Error during screen streaming.", ex);
                    if (!isRunning)
                        break;
                }
            }
        }

        private Bitmap CaptureScreen()
        {
            Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, screenBounds.Size);
            }
            return bitmap;
        }

        private byte[] ImageToByte(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private void SendImageInChunks(byte[] byteArray)
        {
            int totalPackets = (int)Math.Ceiling((double)byteArray.Length / (MaxPacketSize - HeaderSize));
            logger.Info($"Sending image by chunks. Total size: {byteArray.Length}");
            for (int i = 0; i < totalPackets; i++)
            {
                int offset = i * (MaxPacketSize - HeaderSize);
                int packetSize = Math.Min(MaxPacketSize - HeaderSize, byteArray.Length - offset);

                byte[] packet = new byte[packetSize + HeaderSize];
                Array.Copy(BitConverter.GetBytes(i), 0, packet, 0, 4);
                Array.Copy(BitConverter.GetBytes(totalPackets), 0, packet, 4, 4);
                Array.Copy(byteArray, offset, packet, HeaderSize, packetSize);

                udpClient.Send(packet, packet.Length, broadcastEndPoint);
            }
        }
    }
}
