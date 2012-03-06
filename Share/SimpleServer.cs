using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Share
{
    class SimpleServer
    {
        private static int port = 3000;
        public bool set = false;

        public SimpleServer()
        {
            IPAddress ipAddress = IPAddress.Any;
            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("Waiting for connections...");
            while (true)
            {
                Socket s = listener.AcceptSocket();
                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                byte[] b = new byte[65535];
                int k = s.Receive(b);
                Console.WriteLine("Received:");
                for (int i = 0; i < k; i++)
                    Console.Write(Convert.ToChar(b[i]));
                ASCIIEncoding enc = new ASCIIEncoding();
                s.Send(enc.GetBytes("Server responded"));
                Console.WriteLine("\nSent Response");
                s.Close();
            }
        }
    }
}
