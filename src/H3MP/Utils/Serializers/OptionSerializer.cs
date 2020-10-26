namespace H3MP.Utils
{
	public static class OptionSerializerExtensions
	{
		public static OptionSerializer<TValue, TSerializer> ToOption<TValue, TSerializer>(this TSerializer @this) where TSerializer : ISerializer<TValue>
		{
			return new OptionSerializer<TValue, TSerializer>(@this);
		}
	}

	public readonly struct OptionSerializer<TValue, TSerializer> : ISerializer<Option<TValue>> where TSerializer : ISerializer<TValue>
	{
		private readonly TSerializer _serializer;

		public OptionSerializer(TSerializer serializer)
		{
			_serializer = serializer;
		}

		public Option<TValue> Deserialize(ref BitPackReader reader)
		{
			return reader.Bits.Pop()
				? Option.Some(_serializer.Deserialize(ref reader))
				: Option.None<TValue>();
		}

		public void Serialize(ref BitPackWriter writer, Option<TValue> value)
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
