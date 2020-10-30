namespace H3MP.Serialization
{
	public static class ArraySerializers
	{
		public static ISerializer<TValue[]> ToArrayFixed<TValue>(this ISerializer<TValue> @this, int length)
		{
			return new FixedArraySerializer<TValue>(@this, length);
		}

		public static ISerializer<TValue[]> ToArrayDynamic<TValue>(this ISerializer<TValue> @this, ISerializer<int> length)
		{
			return new DynamicArraySerializer<TValue>(@this, length);
		}
	}
}
