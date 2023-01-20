using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public byte[] SaveImage { get; set; } = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string ip = "127.0.0.1";
            IPAddress.TryParse(ip, out var iP);
            byte[] buffer = new byte[ushort.MaxValue];
            EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 27001);

            client.SendTo(Encoding.Default.GetBytes("get"), SocketFlags.None, endPoint);

            var len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

            string response = Encoding.Default.GetString(buffer, 0, len);

            if (response.ToLower() != "start")
                return;

            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int parts = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int lenght = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            byte[] responseImage = new byte[lenght];
            var received = 0;
            for (int i = 0; i < parts; i++)
            {
                len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

                for (int j = received, k = 0; k < len; j++, k++)
                {
                    responseImage[j] = buffer[k];
                }

                client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

                received += len;
            }
            var image = ByteToImage(responseImage);
            screenImage.Stretch = Stretch.Fill;
            screenImage.Source = image;
            SaveImage = responseImage;
        }


        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg as ImageSource;
            return imgSrc;
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SaveImage != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Title = "Save picture as ";
                save.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                var temp = save.ShowDialog();

                if (temp == true)
                    if (save.FileName != String.Empty)
                    {
                        File.WriteAllBytes(System.IO.Path.Combine(save.FileName), SaveImage);
                        screenImage.Source = new BitmapImage(new Uri(@"/Images/image.jpg", UriKind.Relative));
                        screenImage.Stretch = Stretch.Uniform;
                    }
            }

        }
    }
}
