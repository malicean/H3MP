using H3MP.IO;
using H3MP.Utils;

namespace H3MP.Serialization
{
	public static class OptionSerializerExtensions
	{
		public static ISerializer<Option<TValue>> ToOption<TValue>(this ISerializer<TValue> @this)
		{
			return new OptionSerializer<TValue>(@this);
		}
	}

	public class OptionSerializer<T> : ISerializer<Option<T>>
	{
		private readonly ISerializer<T> _serializer;

		public OptionSerializer(ISerializer<T> serializer)
		{
			_serializer = serializer;
		}

		public Option<T> Deserialize(ref BitPackReader reader)
		{
			return reader.Bits.Pop()
				? Option.Some(_serializer.Deserialize(ref reader))
				: Option.None<T>();
		}

		public void Serialize(ref BitPackWriter writer, Option<T> value)
		{
			var isSome = value.MatchSome(out var inner);

			writer.Bits.Push(isSome);
			if (isSome)
			{
				_serializer.Serialize(ref writer, inner);
			}
		}
	}
}
