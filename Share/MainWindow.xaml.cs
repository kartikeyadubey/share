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
using System.Threading;

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
        private const int IMAGE_WIDTH = 640;
        private const int IMAGE_HEIGHT = 480;
        private const int IMAGE_SIZE = 640*480*4;
        int port = 3000;
        bool test = false;
        bool imageSent = false;
        Socket s;
        bool clientConnected = false;
        bool backgroundSent = false;
        WriteableBitmap clientImage;
        int imageSendCounter;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = System.Windows.WindowState.Maximized;
            greeting = new TextBlock();

            _sensor = new SensorData();
            _sensor.updated += new SensorData.UpdatedEventHandler(_sensor_updated);
            _sensor.imageUpdate += new SensorData.SendImageHandler(_sensor_imageUpdate);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            _worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            _serverThread.DoWork +=new DoWorkEventHandler(_serverThread_DoWork);
            //clientImage = new WriteableBitmap(IMAGE_WIDTH, IMAGE_HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            imageSendCounter = 1;
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
                //Send every sixth frame that we receive
                if (backgroundSent && playerFound && imageSendCounter%2 == 0)
                {
                    //Send the client a flag
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = encoder.GetBytes("playerimage");

                    byte[] completeMessage = new byte[65536];
                    int k = s.Receive(completeMessage);
                    while (k == 0)
                    {
                        k = s.Receive(completeMessage);
                    }
                    Console.WriteLine("Message received: " + encoder.GetString(completeMessage, 0, k));


                    //int imageSize = ((xEnd - xStart) * (yEnd - yStart) * 4);
                    //byte[] playerImage = new byte[(imageSize)];
                    clientImage = b.Resize(320, 240, RewritableBitmap.Interpolation.Bilinear);
                    double tmpXStart = (xStart / 2);
                    double tmpYStart = (yStart / 2);
                    double tmpXEnd = (xEnd / 2);
                    double tmpYEnd = (yEnd / 2);

                    xStart = Convert.ToInt32(Math.Floor(tmpXStart));
                    xEnd = Convert.ToInt32(Math.Floor(tmpXEnd));
                    yStart = Convert.ToInt32(Math.Floor(tmpYStart));
                    yEnd = Convert.ToInt32(Math.Floor(tmpYEnd));


                    int smallWidth = (xEnd - xStart);
                    int smallHeight = (yEnd - yStart);


                    int imgSize = smallWidth * smallHeight* 4;
                    Console.WriteLine("Image size: " + imgSize);
                    byte[] playerImage = new byte[imgSize];


                    clientImage.CopyPixels(new Int32Rect(xStart, yStart, smallWidth, smallHeight), playerImage, (smallWidth * 4), 0);
                    //b.CopyPixels(new Int32Rect(xStart, yStart, (xEnd - xStart), (yEnd - yStart)), playerImage, ((xEnd - xStart) * 4), 0);


                    //Send the actual size of the bounding box
                    byte[] xS = BitConverter.GetBytes(xStart);
                    byte[] xE = BitConverter.GetBytes(xEnd);
                    byte[] yS = BitConverter.GetBytes(yStart);
                    byte[] yE = BitConverter.GetBytes(yEnd);
                    byte[] playerImageSize = BitConverter.GetBytes(playerImage.Length);

                    //Image is too big don't try to send the data
                    if (encoder.GetString(completeMessage, 0 , k) != "rcomplete")
                    {
                        return;
                    }

                    try
                    {
                        //Console.WriteLine("Status of socket: " + s.Blocking);
                        s.Send(buffer);
                        
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Error: " + e.ToString());
                    }

                    s.Send(xS);

                    s.Send(xE);

                    s.Send(yS);

                    s.Send(yE);

                    //WORKING UNCOMMENT AFTER TESTING COMPRESSION
                    s.Send(playerImageSize);
                    imageSent = true;
                    imageSendCounter = 1;
                    //byte[] result = new byte[IMAGE_SIZE]; // ARGB
                    //b.Lock();
                    //Marshal.Copy(b.BackBuffer, result, 0, IMAGE_SIZE);
                    //b.Unlock();
                    //////byte[] compressed = Compressor.Compress(result);
                    //s.Send(result);

                    s.Send(playerImage);
                    Console.WriteLine("Image sent, size of image: " + playerImage.Length + "," + xStart + ", " + xEnd + ", " + yStart + ", " + yEnd);
                }
                else if (!backgroundSent)
                {
                    clientImage = b.Resize(320, 240, RewritableBitmap.Interpolation.Bilinear);
                    byte[] smallBackgroundImage = new byte[IMAGE_SIZE/4];
                    clientImage.CopyPixels(new Int32Rect(0, 0, 320, 240), smallBackgroundImage, 320*4, 0);
                    s.Send(smallBackgroundImage);
                    //s.Send(_sensor.backgroundImage);
                    backgroundSent = true;
                    Console.WriteLine("Background sent");
                }
                else
                {
                    imageSendCounter++;
                    Console.WriteLine("Image not sent");
                }
                
            }
        }



        void _serverThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!clientConnected)
            {
                //Dispatcher.BeginInvoke((Action)delegate
                //{
                    IPAddress ipAddress = IPAddress.Any;
                    TcpListener listener = new TcpListener(ipAddress, port);
                    listener.Start();
                    Console.WriteLine("Server is running");
                    Console.WriteLine("Listening on port " + port);
                    Console.WriteLine("Waiting for connections...");
                    while (!clientConnected)
                    {
                        s = listener.AcceptSocket();
                        s.SendBufferSize = 256000;
                        //s.NoDelay = true;
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
                //});
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
