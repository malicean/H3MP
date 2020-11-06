using System.Collections.Generic;
using H3MP.Utils;

namespace H3MP.Extensions
{
	public static class ListExtensions
	{
		public static Option<T> LastOrNone<T>(this List<T> @this)
		{
			return @this.Count > 0
				? Option.Some(@this[@this.Count - 1])
				: Option.None<T>();
		}

		public static IEnumerable<T> FastReverse<T>(this List<T> @this)
		{
			for (var i = @this.Count - 1; i >= 0; --i)
			{
				yield return @this[i];
			}
		}
	}
}
