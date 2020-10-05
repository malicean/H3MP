using System;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Client
{
    public static class Program 
    {
        public static void Main() 
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager client = new NetManager(listener);
            client.Start();

            Console.Write("key: ");
            var key = Console.ReadLine();
            client.Connect("73.111.168.150", 9099, key);

            var quit = false;
            client.NetworkReceiveEvent += (peer, reader, method) =>
            {
                Console.WriteLine($"data received: {reader.GetString(100)}");
                reader.Recycle();

                quit = true;
            };

            while (!quit || Console.ReadKey().Key != ConsoleKey.Q) 
            {
                client.PollEvents();
                Thread.Sleep(10);
            }

            client.Stop();
        }
    }
}