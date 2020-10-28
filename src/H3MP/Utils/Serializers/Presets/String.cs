namespace H3MP.Utils
{
	public static class StringSerializers
	{
		public static StringSerializer ToString(this ISerializer<char> @this, ISerializer<int> length)
		{
			return new StringSerializer(@this, length);
		}
	}
}
