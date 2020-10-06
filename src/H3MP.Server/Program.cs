using H3MP.Common;
using H3MP.Common.Utils;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using LiteNetLib;
using LiteNetLib.Utils;

using Ninject;

using Serilog;
using Serilog.Core;

namespace H3MP.Server
{
	public static class Program
	{
		public static void Main()
		{
			var kernel = new StandardKernel();
			kernel
				.Bind<Logger>()
				.ToMethod(x => new LoggerConfiguration().WriteTo.Console().CreateLogger())
				.InSingletonScope();
			kernel
				.Bind<IConnectionSettings>()
				.To<Settings>()
				.InSingletonScope();
			kernel
				.Bind<Pool<NetDataWriter>>()
				.ToMethod(x => new Pool<NetDataWriter>(new NetDataWriterPoolSource()))
				.InSingletonScope();
			kernel
				.Bind<INetEventListener>()
				.To<NetEventListener>()
				.InSingletonScope();
			kernel
				.Bind<NetManager>()
				.ToMethod(x => new NetManager(x.Kernel.Get<INetEventListener>()))
				.InSingletonScope();

			// construct bindings
			var server = kernel.Get<NetManager>();
			server.Start(9009);

			while (true) 
			{
				server.PollEvents();
				Thread.Sleep(1);
			}
		}
	}
}
