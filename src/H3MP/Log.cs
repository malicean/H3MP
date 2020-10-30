using BepInEx.Logging;
using H3MP.Utils;

namespace H3MP
{
	public class Log
	{
		public const string DELIMITER = "-";

		public readonly ManualLogSource Common;
		public readonly Option<ManualLogSource> Sensitive;

		public Log(string name, bool sensitive)
		{
			Common = Logger.CreateLogSource(name);
			Sensitive = sensitive
				? Option.Some(Logger.CreateLogSource(name + DELIMITER + "SENSITIVE"))
				: Option.None<ManualLogSource>();
		}
	}
}
