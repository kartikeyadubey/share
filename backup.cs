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
        #region Threads
        BackgroundWorker _worker;
        BackgroundWorker _serverThread;
        BackgroundWorker _clientThread;
        BackgroundWorker _imageThread;
        #endregion

        #region Constants
        private const int IMAGE_WIDTH = 640;
        private const int IMAGE_HEIGHT = 480;
        private const int IMAGE_SIZE = 640 * 480 * 4;
        private const int portSend = 3000;
        private const int portReceive = 4000;
        private const int DPI_X = 96;
        private const int DPI_Y = 96;
        private const int TRANSMIT_IMAGE_SIZE = 320 * 240 * 4;
        private const int TRANSMIT_WIDTH = 320;
        private const int TRANSMIT_HEIGHT = 240;
        #endregion

        #region Server Variables
        Socket s;
        bool clientConnected = false;
        bool backgroundSent = false;
        WriteableBitmap clientImage;
        int imageSendCounter;
        Server server;
        #endregion

        #region Client Variables
        //TcpClient client;
        IPEndPoint serverEndPoint;
        NetworkStream clientStream;
        private bool connectedToServer = false;
        private const string ipAddress = "127.0.0.1";
        private bool backgroundReceived = false;
        byte[] backgroundImage;
        byte[] partialPlayerImage;
        byte[] playerImage;

        private int xStart;
        private int xEnd;
        private int yStart;
        private int yEnd;
        WriteableBitmap serverImage;
        private byte[] COMPLETE_MESSAGE;
        private byte[] HELLO_MESSAGE;
        Client client;
        #endregion

        #region Display variables
        SensorData _sensor;
        TextBlock greeting;
        bool isGreeting = false;
        ImageCollection collection;
        int pushCount;
        int imageSelectedIndex;
        int imageSelectedId;
        int canvasSelectedIndex;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            WindowState = System.Windows.WindowState.Maximized;
            greeting = new TextBlock();

            _sensor = new SensorData();
            //_sensor.updated += new SensorData.UpdatedEventHandler(_sensor_updated);
            _sensor.updated += new SensorData.UpdatedEventHandler(_sensor_updated);
            _sensor.imageUpdate += new SensorData.SendImageHandler(_sensor_imageUpdate);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(Worker_DoWork);

            _serverThread = new BackgroundWorker();
            _serverThread.DoWork += new DoWorkEventHandler(_serverThread_DoWork);

            _clientThread = new BackgroundWorker();
            _clientThread.DoWork += new DoWorkEventHandler(_clientThread_DoWork);

            _imageThread = new BackgroundWorker();
            _imageThread.DoWork += new DoWorkEventHandler(_imageThread_DoWork);

            imageSendCounter = 1;
            serverImage = new WriteableBitmap(TRANSMIT_WIDTH, TRANSMIT_HEIGHT, DPI_X, DPI_Y, PixelFormats.Bgra32, null);
            ASCIIEncoding enc = new ASCIIEncoding();
            COMPLETE_MESSAGE = enc.GetBytes("rcomplete");
            HELLO_MESSAGE = enc.GetBytes("hello");

            pushCount = 0;
            canvasSelectedIndex = -1;
            imageSelectedIndex = -1;
            imageSelectedId = -1;

            collection = new ImageCollection();
            server = new Server();
            server.setPort(3000);
            client = new Client();
            client.setPort(4000);
            drawCanvas();
        }

        void _sensor_updated(object sender, OpenNI.Point3D handPoint, string evtName)
        {
            if(evtName.Equals("push"))
            {
                pushCount++;
                if (pushCount == 1)
                {
                    checkAndSelectImage(handPoint);
                }
                if(pushCount == 2)
                {
                    deselectImageAndReset();
                }
            }
            else if (imageSelectedIndex > -1)
            {
                DrawPixels(handPoint.X, handPoint.Y);
                collection.updateImageAtIndex(imageSelectedIndex, handPoint, imageSelectedId);
            }
        }

        public void checkAndSelectImage(OpenNI.Point3D handPoint)
        {
            int imageIndex = collection.findImageAt(handPoint);
            if (imageIndex > -1)
            {
                collection.selectImageAtIndex(imageIndex);
                imageSelectedIndex = imageIndex;
                imageSelectedId = collection.getIdImageAtIndex(imageSelectedIndex);
                Console.WriteLine("Index, Id: " + imageSelectedIndex + "," + imageSelectedId + collection.getImageAtIndex(imageSelectedIndex).getCanvasId());
            }
        }

        public void deselectImageAndReset()
        {
            pushCount = 0;
            collection.deselectImageAtIndex(imageSelectedIndex);
            imageSelectedId = -1;
            imageSelectedIndex = -1;
            canvasSelectedIndex = -1;
            Dispatcher.BeginInvoke((Action)delegate
            {
                canvas1.Children.Clear();
            });
        }
        void _imageThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (imageSelectedIndex > -1)
            {
                drawCanvas();
            }
        }

        /// <summary>
        /// Draw all children on the canvas
        /// </summary>
        private void drawCanvas()
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                canvas2.Children.Clear();
                for (int i = 0; i < collection.getLength(); i++)
                {
                    Image tb = new Image();
                    ImageObject imgObj = collection.getImageAtIndex(i);
                    tb.Source = imgObj.getImage();
                    tb.Width = 100;
                    tb.Height = 100;
                    if (imgObj.isSelected())
                    {
                        tb.Opacity = 0.7;
                    }
                    Canvas.SetLeft(tb, (double)imgObj.getX());
                    Canvas.SetTop(tb, (double)imgObj.getY());
                    canvasSelectedIndex = canvas2.Children.Add(tb);
                    collection.setElementAtIndexCanvasIndex(i, canvasSelectedIndex);
                }
            });
        }

        /// <summary>
        /// Thread responsible for reading data being sent by the server
        /// </summary>
        void _clientThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.connectedToServer)
            {
                client.receiveData();
            }
            else if(server.clientConnected)
            {
                client.connect();
                server.clientStream = client.clientStream;
            }
            //ASCIIEncoding encoder = new ASCIIEncoding();
            //if (!connectedToServer && clientConnected)
            //{
            //    client = new TcpClient();
            //    serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portReceive);

            //    System.Threading.Thread.Sleep(3000);
            //    client.Connect(serverEndPoint);
            //    clientStream = client.GetStream();
            //    server.clientStream = client.GetStream();
            //    clientStream.Write(HELLO_MESSAGE, 0, HELLO_MESSAGE.Length);
            //    clientStream.Flush();
            //    Console.WriteLine("Client said hello");

            //    clientStream.Write(COMPLETE_MESSAGE, 0, COMPLETE_MESSAGE.Length);
            //    clientStream.Flush();
            //    connectedToServer = true;
            //}
            //else if (connectedToServer)
            //{
            //    byte[] backgroundImageData = new byte[TRANSMIT_IMAGE_SIZE];
            //    byte[] serverMessage = new byte[65535];
            //    byte[] xS = new byte[4];
            //    byte[] xE = new byte[4];
            //    byte[] yS = new byte[4];
            //    byte[] yE = new byte[4];
            //    int bytesRead;

            //    while (true)
            //    {
            //        bytesRead = 0;

            //        try
            //        {
            //            if (!client.Connected)
            //            {
            //                client.Connect(serverEndPoint);
            //            }
            //            clientStream.Flush();
            //            //blocks until a server replies with a message
            //            if (!backgroundReceived)
            //            {
            //                bytesRead = clientStream.Read(backgroundImageData, 0, TRANSMIT_IMAGE_SIZE);
            //            }
            //            else
            //            {
            //                bytesRead = clientStream.Read(serverMessage, 0, serverMessage.Length);
            //            }
            //        }
            //        catch
            //        {
            //            //a socket error has occured
            //            Console.WriteLine("Socket error has occured");
            //        }

            //        if (bytesRead == 0)
            //        {
            //            //the client has disconnected from the server
            //            Console.WriteLine("Player has disconnected from the server");
            //            return;
            //        }

            //        //If the background has not been received yet
            //        //Copy it and store it for future use
            //        if (!backgroundReceived)
            //        {
            //            backgroundImage = new byte[backgroundImageData.Length];
            //            Array.Copy(backgroundImageData, backgroundImage, backgroundImageData.Length);
            //            backgroundReceived = true;
            //            Console.WriteLine("Background received");
            //            clientStream.Flush();
            //            drawPlayer();
            //        }
            //        else if (encoder.GetString(serverMessage, 0, bytesRead) == "playerimage")
            //        {
            //            clientStream.Flush();
            //            bytesRead = clientStream.Read(xS, 0, 4);
            //            xStart = BitConverter.ToInt32(xS, 0);

            //            clientStream.Flush();
            //            bytesRead = clientStream.Read(xE, 0, 4);
            //            xEnd = BitConverter.ToInt32(xE, 0);

            //            clientStream.Flush();
            //            bytesRead = clientStream.Read(yS, 0, 4);
            //            yStart = BitConverter.ToInt32(yS, 0);

            //            clientStream.Flush();
            //            bytesRead = clientStream.Read(yE, 0, 4);
            //            yEnd = BitConverter.ToInt32(yE, 0);

            //            clientStream.Flush();
            //            byte[] byteImgSize = new byte[4];
            //            bytesRead = clientStream.Read(byteImgSize, 0, 4);
            //            int imgSize = BitConverter.ToInt32(byteImgSize, 0);

            //            clientStream.Flush();
            //            partialPlayerImage = new byte[imgSize];
            //            playerImage = new byte[imgSize];

            //            bytesRead = 0;
            //            while (bytesRead != imgSize)
            //            {
            //                int tmpBytesRead = clientStream.Read(partialPlayerImage, 0, imgSize - bytesRead);
            //                Buffer.BlockCopy(partialPlayerImage, 0, playerImage, bytesRead, tmpBytesRead);
            //                bytesRead += tmpBytesRead;
            //            }

            //            drawPlayer();

            //            clientStream.Write(COMPLETE_MESSAGE, 0, COMPLETE_MESSAGE.Length);
            //            clientStream.Flush();
            //            //Console.WriteLine("Image received, size of image: " + imgSize + "," + xStart + ", " + xEnd + ", " + yStart + ", " + yEnd);
            //        }
            //    }
            //}
        }
        
        /// <summary>
        /// Function respobsible for drawing the player on the screen
        /// </summary>
        void drawPlayer()
        {
            unsafe
            {
                Dispatcher.Invoke((Action)delegate
                {
                    serverImage.Lock();
                    int dataCounter = 0;
                    //Draw background image
                    for (int y = 0; y < TRANSMIT_HEIGHT; ++y)
                    {
                        byte* pDest = (byte*)serverImage.BackBuffer.ToPointer() + y * serverImage.BackBufferStride;
                        for (int x = 0; x < TRANSMIT_WIDTH; ++x, pDest += 4, dataCounter += 4)
                        {
                            pDest[0] = backgroundImage[dataCounter];
                            pDest[1] = backgroundImage[dataCounter + 1];
                            pDest[2] = backgroundImage[dataCounter + 2];
                            pDest[3] = backgroundImage[dataCounter + 3];
                        }
                    }

                    dataCounter = 0;
                    //WORKING CODE UNCOMMENT AFTER TESTING COMPRESSION
                    if (playerImage != null)
                    {
                        //Draw new updated image
                        for (int y = yStart; y < yEnd; ++y)
                        {
                            byte* pDest = (byte*)serverImage.BackBuffer.ToPointer() + (y * serverImage.BackBufferStride);
                            pDest += 4 * xStart;
                            for (int x = xStart; x < xEnd; ++x, pDest += 4, dataCounter += 4)
                            {
                                pDest[0] = playerImage[dataCounter];
                                pDest[1] = playerImage[dataCounter + 1];
                                pDest[2] = playerImage[dataCounter + 2];
                                pDest[3] = playerImage[dataCounter + 3];
                            }
                        }
                    }

                    serverImage.AddDirtyRect(new Int32Rect(0, 0, TRANSMIT_WIDTH, TRANSMIT_HEIGHT));
                    //Console.WriteLine("Draw Image now");
                    image2.Source = serverImage;
                    //imageDrawn = true;
                    serverImage.Unlock();
                });
            }
        }

        /// <summary>
        /// Function responsible for connecting to the client
        /// and sending data across
        /// </summary>
        void _sensor_imageUpdate(object sender, WriteableBitmap b, bool playerFound, int xStart, int xEnd, int yStart, int yEnd)
        {
            server.sendData(b, playerFound, xStart, xEnd, yStart, yEnd);
            //if (!clientConnected)
            //{
            //    return;
            //}
            //else
            //{
            //    //Background has been sent and there is a player
            //    //in the image send a message to the client
            //    //with the size of the bounding box and the image
            //    //Send every sixth frame that we receive
            //    if (backgroundSent && playerFound)
            //    {
            //        //Send the client a flag
            //        ASCIIEncoding encoder = new ASCIIEncoding();
            //        byte[] buffer = encoder.GetBytes("playerimage");

            //        byte[] readyToReceive = encoder.GetBytes("rcomplete");
            //        clientStream.Write(readyToReceive, 0, readyToReceive.Length);
            //        clientStream.Flush();
            //        byte[] completeMessage = new byte[65536];

            //        int k = s.Receive(completeMessage);
            //        while (k == 0)
            //        {
            //            k = s.Receive(completeMessage);
            //        }
            //        //Get the string from the first 9 bytes since
            //        //rcomplete may have been appended to ensure there is no deadlock
            //        string tempMessage = encoder.GetString(completeMessage, 0, 9);
            //        if (!tempMessage.Equals("rcomplete"))
            //        {
            //            Console.WriteLine("Message received: " + encoder.GetString(completeMessage, 0, k));
            //            return;
            //        }

            //        clientImage = b.Resize(320, 240, RewritableBitmap.Interpolation.Bilinear);
            //        double tmpXStart = (xStart / 2);
            //        double tmpYStart = (yStart / 2);
            //        double tmpXEnd = (xEnd / 2);
            //        double tmpYEnd = (yEnd / 2);

            //        xStart = Convert.ToInt32(Math.Floor(tmpXStart));
            //        xEnd = Convert.ToInt32(Math.Floor(tmpXEnd));
            //        yStart = Convert.ToInt32(Math.Floor(tmpYStart));
            //        yEnd = Convert.ToInt32(Math.Floor(tmpYEnd));


            //        int smallWidth = (xEnd - xStart);
            //        int smallHeight = (yEnd - yStart);


            //        int imgSize = smallWidth * smallHeight * 4;
            //        //Console.WriteLine("Image size: " + imgSize);
            //        byte[] transmitPlayerImage = new byte[imgSize];


            //        clientImage.CopyPixels(new Int32Rect(xStart, yStart, smallWidth, smallHeight), transmitPlayerImage, (smallWidth * 4), 0);
            //        //b.CopyPixels(new Int32Rect(xStart, yStart, (xEnd - xStart), (yEnd - yStart)), playerImage, ((xEnd - xStart) * 4), 0);


            //        //Send the actual size of the bounding box
            //        byte[] xS = BitConverter.GetBytes(xStart);
            //        byte[] xE = BitConverter.GetBytes(xEnd);
            //        byte[] yS = BitConverter.GetBytes(yStart);
            //        byte[] yE = BitConverter.GetBytes(yEnd);
            //        byte[] playerImageSize = BitConverter.GetBytes(transmitPlayerImage.Length);


            //        try
            //        {
            //            //Console.WriteLine("Status of socket: " + s.Blocking);
            //            s.Send(buffer);

            //        }
            //        catch (SocketException e)
            //        {
            //            Console.WriteLine("Error: " + e.ToString());
            //        }

            //        s.Send(xS);

            //        s.Send(xE);

            //        s.Send(yS);

            //        s.Send(yE);

            //        s.Send(playerImageSize);
            //        imageSendCounter = 1;
            //        s.Send(transmitPlayerImage);
            //        //Console.WriteLine("Image sent, size of image: " + transmitPlayerImage.Length + "," + xStart + ", " + xEnd + ", " + yStart + ", " + yEnd);
            //    }
            //    else if (!backgroundSent)
            //    {
            //        clientImage = b.Resize(320, 240, RewritableBitmap.Interpolation.Bilinear);
            //        byte[] smallBackgroundImage = new byte[TRANSMIT_IMAGE_SIZE];
            //        clientImage.CopyPixels(new Int32Rect(0, 0, 320, 240), smallBackgroundImage, 320 * 4, 0);
            //        s.Send(smallBackgroundImage);
            //        backgroundSent = true;
            //        Console.WriteLine("Background sent");
            //    }
            //    else
            //    {
            //        imageSendCounter++;
            //    }

            //}
        }

        void _serverThread_DoWork(object sender, DoWorkEventArgs e)
        {
            server.connect();
            //if (!clientConnected)
            //{
            //    IPAddress ipAddress = IPAddress.Any;
            //    TcpListener listener = new TcpListener(ipAddress, portSend);
            //    listener.Start();
            //    Console.WriteLine("Server is running");
            //    Console.WriteLine("Listening on port " + portSend);
            //    Console.WriteLine("Waiting for connections...");
            //    while (!clientConnected)
            //    {
            //        s = listener.AcceptSocket();
            //        s.SendBufferSize = 256000;
            //        Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
            //        byte[] b = new byte[65535];
            //        int k = s.Receive(b);
            //        ASCIIEncoding enc = new ASCIIEncoding();
            //        Console.WriteLine("Received:" + enc.GetString(b, 0, k) + "..");
            //        //Ensure the client is who we want
            //        if (enc.GetString(b, 0, k) == "hello" || enc.GetString(b, 0, k) == "hellorcomplete")
            //        {
            //            clientConnected = true;
            //            Console.WriteLine(enc.GetString(b, 0, k));
            //        }
            //    }
            //}
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
                else if (!_sensor.playerRecognized)
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
            if (!_clientThread.IsBusy)
            {
                _clientThread.RunWorkerAsync();
            }
            if (!_imageThread.IsBusy)
            {
                _imageThread.RunWorkerAsync();
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _sensor.Dispose();
        }

        private void DrawPixels(float x, float y)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                // We'll use random colors!


                Ellipse ellipse = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Width = 4,
                    Height = 4
                };

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);

                canvas1.Children.Add(ellipse);
            });
        }
    }
}
