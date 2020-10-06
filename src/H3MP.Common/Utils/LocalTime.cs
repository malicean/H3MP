using System.Diagnostics;

namespace H3MP.Common.Utils
{
	public static class LocalTime
	{
		private readonly static Stopwatch _watch = Stopwatch.StartNew();

		public static double Now => _watch.Elapsed.TotalSeconds;
	}
}
