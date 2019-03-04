using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketTcpServer
{
    class Program
    {
        static int port = 8005; // incoming port
        static void Main(string[] args)
        {
            // get addressess for starting socket
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // create socket
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // bind socket with local incoming data point
                listenSocket.Bind(ipPoint);

                // starting listening
                listenSocket.Listen(10);
                Console.WriteLine("Server started. Waiting connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // get message
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // bytes count
                    byte[] data = new byte[256]; // buffer

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    // send response
                    string message = "message delivered";
                    data = Encoding.Unicode.GetBytes(message);
                    handler.Send(data);
                    // close socket
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
