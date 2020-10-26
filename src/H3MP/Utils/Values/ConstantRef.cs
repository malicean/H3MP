namespace H3MP.Utils
{
	/// <summary>
	///		Efficient, immutable representation of <see cref="IRef{T}" />.
	///		This should <code>never</code> be directly constructed.
	///		To discourage you from using it directly, it has purposefully been made hard to access <see cref="IRef{T}.Value" />.
	/// </summary>
	public readonly struct ConstantRef<T> : IRef<T>
	{
		private readonly T _value;

		T IRef<T>.Value => _value;

		public ConstantRef(T value)
		{
			_value = value;
		}

		public static implicit operator ConstantRef<T>(T value)
		{
			return new ConstantRef<T>(value);
		}

		public static implicit operator T(ConstantRef<T> @this)
		{
			return @this._value;
		}
	}
}
