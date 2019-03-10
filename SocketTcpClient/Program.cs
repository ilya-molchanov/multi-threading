using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTcpClient
{
    class Program
    {
        //Function to get random number
        private static readonly Random _getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (_getrandom) // synchronize
            {
                return _getrandom.Next(min, max);
            }
        }

        private static readonly int _port = 8005; // server _port
        private static readonly string _address = "127.0.0.1"; // server _address

        private static List<string> _clientNamesList = new List<string>() {"first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eigth", "nineth", "tenth" };

        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(state => ClientSendMessages(Thread.CurrentThread.ManagedThreadId));
            ThreadPool.QueueUserWorkItem(state => ClientSendMessages(Thread.CurrentThread.ManagedThreadId));
            ThreadPool.QueueUserWorkItem(state => ClientSendMessages(Thread.CurrentThread.ManagedThreadId));

            Console.Read();
        }
        
        private static async Task ClientSendMessages(int numberClient)
        {
            try
            {
                for (int i = 0; i < GetRandomNumber(2, 3); i++)
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_address), _port);

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    // connect to the remote host
                    socket.Connect(ipPoint);
                    string sendingMessage = _clientNamesList[GetRandomNumber(0, 9)];
                    byte[] data = Encoding.Unicode.GetBytes(numberClient + "|" + sendingMessage);
                    socket.Send(data);

                    // getting response
                    data = new byte[10000]; // response buffer
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // received bytes count

                    do
                    {
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (socket.Available > 0);

                    if (builder.ToString() != string.Empty)
                    {
                        Console.WriteLine("client number: " + numberClient + " got server response: " +
                                          builder.ToString());
                    }

                    // close socket
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await Task.Delay(2000);
        }
    }
}
