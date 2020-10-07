using H3MP.Common.Utils;

using System.IO;
using System.Threading;

using LiteNetLib;
using LiteNetLib.Utils;

using Newtonsoft.Json;

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
				.ToMethod(x => new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.WriteTo
					.Console()
					.CreateLogger())
				.InSingletonScope();
			kernel
				.Bind<IConnectionSettings>()
				.ToMethod(x =>
				{
					return File.Exists("settings.json")
						? JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"))
						: new Settings();
				})
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

			logger.Verbose("Loading settings...");
			var settings = kernel.Get<Settings>();

			logger.Verbose("Instantiating network...");
			var server = kernel.Get<NetManager>();

			logger.Verbose("Starting network...");
			server.Start(settings.Port);

			logger.Verbose("Awaiting clients...");
			while (true) 
			{
				server.PollEvents();
				Thread.Sleep(1);
			}
		}
	}
}
