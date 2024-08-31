using log4net;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows;

namespace BroadcastClient
{
    public class ScreenReceiver
    {
        private readonly ILog logger;
        private UdpClient udpClient;
        private const int port = 5000;
        private Thread receiveThread;
        private readonly Image imageDisplay;
        private bool isRunning = false;

        private Dictionary<int, byte[]> receivedPackets = new Dictionary<int, byte[]>();
        private int totalPackets = -1;

        public Dispatcher Dispatcher { get; private set; }

        public ScreenReceiver(ILog logger, Dispatcher dispatcher, Image imageDisplay)
        {
            this.logger = logger;
            Dispatcher = dispatcher;
            this.imageDisplay = imageDisplay;
        }

        public void StartReceiving(string serverIpAddress, int serverPort = 8888, int clientPort = 8889)
        {
            if (!isRunning)
            {
                try
                {
                    isRunning = true;
                    udpClient = new UdpClient(clientPort);

                    receiveThread = new Thread(() => ReceiveFrames(serverIpAddress, serverPort));
                    receiveThread.IsBackground = true;

                    logger.Info($"Starting screen receiving from {serverIpAddress}:{serverPort}.");
                    receiveThread.Start();
                    logger.Info($"Screen receiving from {serverIpAddress}:{serverPort} started successfully.");
                }
                catch (SocketException e)
                {
                    MessageBox.Show($"Port error: one of the used ports (serverPort:{serverPort} or clientPort:{clientPort}) is already in use.");
                }
            }
        }

        public void StopReceiving()
        {
            if (isRunning)
            {
                logger.Info("Stopping screen receiving.");
                isRunning = false;
                udpClient.Close();
                receiveThread?.Join();
            }
        }

        private async void ReceiveFrames(string serverIpAddress, int port)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIpAddress), port);
            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    int packetId = BitConverter.ToInt32(data, 0);
                    totalPackets = BitConverter.ToInt32(data, 4);

                    if (!receivedPackets.ContainsKey(packetId))
                    {
                        byte[] imageData = new byte[data.Length - 8];
                        Array.Copy(data, 8, imageData, 0, imageData.Length);
                        receivedPackets[packetId] = imageData;

                        if (receivedPackets.Count == totalPackets)
                        {
                            logger.Info($"Displaying image of {receivedPackets.Count} bytes");
                            DisplayImage();
                            receivedPackets.Clear();
                        }
                    }
                }
                catch (SocketException sex)
                {
                    logger.Error("Error while receiving data.", sex);
                }
                catch (Exception ex)
                {
                    logger.Error("Error while receiving data. Stoping", ex);
                    StopReceiving();
                    if (!isRunning)
                        break;
                }
            }
        }

        private void DisplayImage()
        {
            using (var ms = new MemoryStream())
            {
                for (int i = 0; i < totalPackets; i++)
                {
                    ms.Write(receivedPackets[i], 0, receivedPackets[i].Length);
                }

                ms.Seek(0, SeekOrigin.Begin);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);

                Dispatcher.Invoke(() =>
                {
                    Stream bitmapStream = new MemoryStream();
                    bitmap.Save(bitmapStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    bitmapStream.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = bitmapStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    imageDisplay.Source = bitmapImage;
                });
            }
        }
    }
}
