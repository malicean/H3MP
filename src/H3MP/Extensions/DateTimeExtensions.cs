using System;

namespace H3MP.Extensions
{
	public static class DateTimeExtensions
	{
		public static long ToUnixTimestamp(this DateTime @this)
		{
			return (long) @this.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}
	}
}
