using H3MP.Utils;
using System.Collections.Generic;

namespace H3MP.Extensions
{
	public static class BitPackReaderExtensions
	{
		public delegate T Converter<out T>(ref BitPackReader reader);

		public static Dictionary<TKey, TValue> GetDictionaryWithByteCount<TKey, TValue>(this ref BitPackReader @this, Converter<TKey> readKey, Converter<TValue> readValue)
		{
			var count = @this.Bytes.GetByte();
			var value = new Dictionary<TKey, TValue>(count);

			for (var i = 0; i < count; ++i)
			{
				value.Add(readKey(ref @this), readValue(ref @this));
			}

			return value;
		}
	}
}
