﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTcpServer
{
    class Program
    {
        public class InnerSendingThreadsStructure
        {
            public InnerSendingThreadsStructure(string thread, bool isSendingThread)
            {
                this.thread = thread;
                this.isSendingThread = isSendingThread;
            }

            public InnerSendingThreadsStructure()
            {
            }

            public string thread { get; set; }
            public bool isSendingThread { get; set; }
        }

        private static Dictionary<string, Dictionary<string, List<InnerSendingThreadsStructure>>> _history =
            new Dictionary<string, Dictionary<string, List<InnerSendingThreadsStructure>>>();

        private static int port = 8005; // incoming port


        static void Main(string[] args)
        {
            using (Socket listenSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {

                    var token = cts.Token;
                    token.Register(() => { Console.WriteLine("Server Cancelled."); });
                
                    // get addressess for starting socket
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

                    // create socket


                    // bind socket with local incoming data point
                    listenSocket.Bind(ipPoint);

                    // starting listening
                    listenSocket.Listen(10);

                    Console.WriteLine("Server started. Waiting connections...");

                    while (!token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                            cts.Cancel();

                        Task.Run(() => { ServerReadMessages(listenSocket, token); }, token);

                    }
                }
            }

            Console.ReadKey();
        }

        private static void ServerReadMessages(Socket listenSocket, CancellationToken token)
        {
            try
            {
                    if (token.IsCancellationRequested)
                        return;
                    using (Socket handler = listenSocket.Accept())
                    {
                        // get message
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0; // bytes count
                        byte[] data = new byte[10000]; // buffer

                        do
                        {
                            bytes = handler.Receive(data);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        } while (handler.Available > 0 && !token.IsCancellationRequested);

                        String[] transferredArgs = builder.ToString().Split('|');
                        Console.WriteLine(DateTime.Now.ToLongTimeString() + " thread number : " + transferredArgs[0] +
                                          " sent message " + transferredArgs[1]);
                        Dictionary<string, List<InnerSendingThreadsStructure>> helpingDictionary =
                            new Dictionary<string, List<InnerSendingThreadsStructure>>();
                        List<InnerSendingThreadsStructure> helpingList = new List<InnerSendingThreadsStructure>();
                        helpingList.Add(new InnerSendingThreadsStructure(transferredArgs[0], true));

                        helpingDictionary.Add(transferredArgs[1], helpingList);

                        _history.Add(Guid.NewGuid().ToString(), helpingDictionary);
                        // send response
                        SendMessagesToClients(transferredArgs[0], handler);
                    }
            }
            catch (Exception ex){}

        }

        private static void SendMessagesToClients(string numberThread, Socket handler)
        {
            try
            {
                byte[] data = new byte[10000]; // buffer
                string sendingData = string.Empty;
                foreach (string key in _history.Keys)
                {
                    foreach (var innerDictionary in _history[key])
                    {
                        if (!innerDictionary.Value.Any(x => x.thread == numberThread))
                        {
                            innerDictionary.Value.Add(new InnerSendingThreadsStructure(numberThread, false));
                            string sendingThread = innerDictionary.Value.Single(x => x.isSendingThread).thread;
                            sendingData += "Other thread number : " + sendingThread + " sent message " + innerDictionary.Key + Environment.NewLine;
                        }
                    }
                }
                handler.Send(Encoding.Unicode.GetBytes(sendingData));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
