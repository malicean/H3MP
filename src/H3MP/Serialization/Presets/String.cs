namespace H3MP.Serialization
{
	public static class StringSerializers
	{
		public static ISerializer<string> ToString(this ISerializer<char> @this, ISerializer<int> length)
		{
			return new StringSerializer(@this, length);
		}
	}
}
