using BroadcastClient.Data;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace BroadcastClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            await screenReceiver.StartReceiving(ServerIpInput.Text);
        }
    }
}
