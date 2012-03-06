using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;


/*
 * Class that handles Tcp server side initialization and communication
 */

namespace Share
{
    class TcpServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        public WriteableBitmap img;
        public bool set = false;

        public TcpServer()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }


        //Start listening for client side connection
        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        //Handle client communication
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client disconnected");
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));

                if (encoder.GetString(message, 0, bytesRead) == "Hello Server!" && set)
                {
                    Console.WriteLine("Server received Hello Server!");
                    int count = 640*480;
                    int len = count*4;
                    byte[] result = new byte[len]; // ARGB
                    img.Lock();
                    Marshal.Copy(img.BackBuffer, result, 0, count);
                    img.Unlock();                    
                    clientStream.Write(result, 0, result.Length);
                    clientStream.Flush();
                    Console.WriteLine("Server sent image data");
                }
                else if (encoder.GetString(message, 0, bytesRead) == "Close connection")
                {
                    break;
                }
            }

            tcpClient.Close();
        }
    }
}
