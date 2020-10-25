using H3MP.Utils;
using System.Collections.Generic;

namespace H3MP.Extensions
{
	public static class BitPackWriterExtensions
	{
		public delegate void Converter<in T>(ref BitPackWriter writer, T value);

		public static void PutDictionaryWithByteLength<TKey, TValue>(this ref BitPackWriter @this, Dictionary<TKey, TValue> value, Converter<TKey> writeKey, Converter<TValue> writeValue)
		{
			@this.Bytes.Put((byte) value.Count);

			foreach (var pair in value)
			{
				writeKey(ref @this, pair.Key);
				writeValue(ref @this, pair.Value);
			}
		}
	}
}
