namespace H3MP.Utils
{
	public static class TruncatedSerializer
	{
		public static ConverterSerializer<float, ushort, UShortFloatConverter, TSerializer> FloatWithMaxAbsolute<TSerializer>(TSerializer serializer, float maxAbs)
			where TSerializer : ISerializer<ushort>
		{
			return new ConverterSerializer<float, ushort, UShortFloatConverter, TSerializer>(serializer, new UShortFloatConverter(maxAbs));
		}
	}

}
