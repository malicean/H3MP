using System;
using H3MP.Utils;

namespace H3MP.Extensions
{
	public static class ValueExtensions
	{
		public static ConstantRef<T> ToValue<T>(this T @this)
		{
			// This normally wouldn't work, but because we have the method return type, it implicitly converts.
			return @this;
		}

		public static bool HasFlag(this byte @this, byte flag)
		{
			return (@this & flag) == flag;
		}
	}
}
