using System;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Server
{
	public static class Program
	{
		public static void Main()
		{
			var listener = new EventBasedNetListener();
			var server = new NetManager(listener);

			Console.Write("key: ");
			var key = Console.ReadLine();

			listener.ConnectionRequestEvent += request => 
			{
				request.AcceptIfKey(key);
			};

			listener.PeerConnectedEvent += peer => 
			{
				Console.WriteLine($"Peer connected: {peer.EndPoint}");

				Console.Write("Type a response: ");
				var response = Console.ReadLine();

				var writer = new NetDataWriter();
				writer.Put(response);
				peer.Send(writer, DeliveryMethod.ReliableSequenced);
			};

			server.Start(9099);

			while (!Console.KeyAvailable && Console.ReadKey().Key != ConsoleKey.Q) 
			{
				server.PollEvents();
				Thread.Sleep(10);
			}
		}
	}
}
