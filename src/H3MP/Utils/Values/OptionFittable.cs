namespace H3MP.Utils
{
	public static class OptionFittableExtension
	{
		public static OptionFittable<T> ToFittable<T>(this Option<T> @this) where T : ILinearFittable<T>
		{
			return new OptionFittable<T>(@this);
		}
	}

    public readonly struct OptionFittable<T> : ILinearFittable<OptionFittable<T>>, IRef<Option<T>> where T : ILinearFittable<T>
	{
		private readonly Option<T> _value;

		public Option<T> InitialDelta => _value;

		public Option<T> Value => _value;

		public OptionFittable(Option<T> value)
        {
            _value = value;
        }

		public OptionFittable<T> Fit(OptionFittable<T> other, double t)
		{
			var value = _value.MatchSome(out var thisValue) && other._value.MatchSome(out var otherValue)
				? Option.Some(thisValue.Fit(otherValue, t))
				: Option.None<T>();

			return value.ToFittable<T>();
		}

		public static implicit operator Option<T>(OptionFittable<T> @this)
		{
			return @this._value;
		}
	}
}
