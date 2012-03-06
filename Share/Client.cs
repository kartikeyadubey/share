using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Share
{
    class Client
    {
        public Client()
        {
            TcpClient client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);

            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("Hello Server!");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
            Console.WriteLine("Client said Hello Server!");

            byte[] message = new byte[4096];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a server replies with a message
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
                    break;
                }

                //message has successfully been received
                //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));

                if (encoder.GetString(message, 0, bytesRead) == "Hello Client!")
                {
                    Console.WriteLine("Client received Hello Client!");
                    buffer = encoder.GetBytes("Close connection");
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }

            }
        }
    }
}
