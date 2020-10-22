using System;
using H3MP.Utils;

namespace H3MP.Extensions
{
	public static class DirtyExtensions
	{
		public static T CreateDelta<T>(this T @this)
		{
			return new ConstantRef<T>(@this);
		}
	}
}
