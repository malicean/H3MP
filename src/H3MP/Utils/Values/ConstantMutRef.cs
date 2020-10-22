using System;

namespace H3MP.Utils
{
	public struct ConstantMutRef<T> : IMutRef<T>
	{
		public T Value { get; set; }

		public ConstantMutRef(T value)
		{
			Value = value;
		}

		public static implicit operator T(ConstantMutRef<T> @this)
		{
			return @this.Value;
		}
	}
}
