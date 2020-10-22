using System;

namespace H3MP.Utils
{
	public readonly struct FunctionRef<T> : IRef<T>
	{
		private readonly Func<T> _value;

		public T Value => _value();

		public FunctionRef(Func<T> value)
		{
			_value = value;
		}
	}
}
