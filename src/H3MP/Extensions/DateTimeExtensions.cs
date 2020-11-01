using System;

namespace H3MP.Extensions
{
	public static class DateTimeExtensions
	{
		public static int ToUnixTimestamp(this DateTime @this)
		{
			return (int) @this.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}
	}
}
