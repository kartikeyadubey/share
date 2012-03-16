using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;

namespace Share
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker _worker = new BackgroundWorker();
        BackgroundWorker _serverThread = new BackgroundWorker();
        SensorData _sensor;
        TextBlock greeting;
        bool isGreeting = false;
        readonly int IMAGE_WIDTH;
        readonly int IMAGE_HEIGHT;
        readonly int IMAGE_SIZE;
        int port = 3000;
        bool test = false;
        bool imageSent = false;
        Socket s;
        bool clientConnected = false;
        bool backgroundSent = false;
        WriteableBitmap clientImage;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = System.Windows.WindowState.Maximized;
            greeting = new TextBlock();

            Console.WriteLine("Create Sensor Data");
            _sensor = new SensorData();
            _sensor.updated += new SensorData.UpdatedEventHandler(_sensor_updated);
            _sensor.imageUpdate += new SensorData.SendImageHandler(_sensor_imageUpdate);
            IMAGE_HEIGHT = 480;
            IMAGE_WIDTH = 640;
            IMAGE_SIZE = IMAGE_WIDTH * IMAGE_HEIGHT * 4;
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            _worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            _serverThread.DoWork +=new DoWorkEventHandler(_serverThread_DoWork);
            clientImage = new WriteableBitmap(IMAGE_WIDTH, IMAGE_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
        }

        void _sensor_imageUpdate(object sender, WriteableBitmap b, bool playerFound, int xStart, int xEnd, int yStart, int yEnd)
        {
            if (!clientConnected)
            {
                return;
            }
            else
            {
                //Background has been sent and there is a player
                //in the image send a message to the client
                //with the size of the bounding box and the image
                if (backgroundSent && playerFound)
                {
                    //Send the client a flag
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = encoder.GetBytes("playerimage");


                    int imageSize = ((xEnd - xStart) * (yEnd - yStart) * 4);
                    byte[] playerImage = new byte[(imageSize)];

                    b.CopyPixels(new Int32Rect(xStart, yStart, (xEnd - xStart), (yEnd - yStart)), playerImage, ((xEnd - xStart) * 4), 0);


                    s.Send(buffer);

                    //Send the actual size of the bounding box
                    byte[] xS = BitConverter.GetBytes(xStart);
                    byte[] xE = BitConverter.GetBytes(xEnd);
                    byte[] yS = BitConverter.GetBytes(yStart);
                    byte[] yE = BitConverter.GetBytes(yEnd);
                    byte[] playerImageSize = BitConverter.GetBytes(playerImage.Length);

                    s.Send(xS);

                    s.Send(xE);

                    s.Send(yS);

                    s.Send(yE);

                    s.Send(playerImageSize);
                    imageSent = true;
                    //byte[] result = new byte[IMAGE_SIZE]; // ARGB
                    //b.Lock();
                    //Marshal.Copy(b.BackBuffer, result, 0, IMAGE_SIZE);
                    //b.Unlock();
                    //////byte[] compressed = Compressor.Compress(result);
                    //s.Send(result);

                    s.Send(playerImage);
                    //Console.WriteLine("Server sent data, orig data size: " + playerImage.Length + "playerImage length: " + playerImage.Length);
                }
                else if(!backgroundSent)
                {
                    s.Send(_sensor.backgroundImage);
                    backgroundSent = true;
                    Console.WriteLine("Background sent");
                }
                
            }
        }



        void _serverThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!clientConnected)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    IPAddress ipAddress = IPAddress.Any;
                    TcpListener listener = new TcpListener(ipAddress, port);
                    listener.Start();
                    Console.WriteLine("Server is running");
                    Console.WriteLine("Listening on port " + port);
                    Console.WriteLine("Waiting for connections...");
                    while (!clientConnected)
                    {
                        
                        s = listener.AcceptSocket();
                        Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                        byte[] b = new byte[65535];
                        int k = s.Receive(b);
                        Console.WriteLine("Received:");
                        ASCIIEncoding enc = new ASCIIEncoding();
                        //Ensure the client is who we want
                        if (enc.GetString(b, 0, k) == "hello")
                        {
                            clientConnected = true;
                            Console.WriteLine(enc.GetString(b, 0, k));
                        }
                    }
                });
            }
        }

        void _sensor_updated(object sender, OpenNI.Point3D handPoint)
        {
            Console.WriteLine("Point Updated");
            DrawPixels(handPoint.X + (IMAGE_WIDTH / 2), handPoint.Y + (IMAGE_HEIGHT / 2));
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                image1.Source = _sensor.RawImageSource;
                if (_sensor.playerRecognized && !isGreeting)
                {
                    greeting.Text = "Hello";
                    greeting.FontSize = 28;
                    Canvas.SetTop(greeting, 0);
                    Canvas.SetLeft(greeting, 0);
                    canvas1.Children.Add(greeting);
                    isGreeting = true;
                }
                else if(!_sensor.playerRecognized)
                {
                    canvas1.Children.Clear();
                    isGreeting = false;
                }
            });
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
            if (!_serverThread.IsBusy)
            {
                _serverThread.RunWorkerAsync();
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _sensor.Dispose();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.B:
                    Console.WriteLine("B was pressed");
                    s.Close();
                    _sensor.drawBackground = !_sensor.drawBackground;
                    break;
                default: base.OnKeyDown(e);
                    break;
            }
        }
        
        private void DrawPixels(float x, float y)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                // We'll use random colors!


                Ellipse ellipse = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.FromRgb(0,0,0)),
                    Width = 4,
                    Height = 4
                };

                Canvas.SetLeft(ellipse, x);
                Canvas.SetBottom(ellipse, y);

                canvas1.Children.Add(ellipse);
            });
        }
    }
}
