namespace H3MP.Utils
{
	public readonly struct IntIntConverter : IConverter<int, int>
	{
		public int Convert(int value)
		{
			return value;
		}
	}
}
