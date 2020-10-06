using H3MP.Common.Utils;

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
				.ToMethod(x => new NetManager(x.Kernel.Get<INetEventListener>())
				{
					AutoRecycle = true
				})
				.InSingletonScope();

			var logger = kernel.Get<Logger>();

			logger.Debug("Instantiating network...");
			var server = kernel.Get<NetManager>();

			logger.Debug("Starting network...");
			server.Start(7777);

			logger.Debug("Awaiting clients...");
			while (true) 
			{
				server.PollEvents();
				Thread.Sleep(1);
			}
		}
	}
}
