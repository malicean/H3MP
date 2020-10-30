using System;

namespace H3MP.Utils
{
	public static class Option
	{
		public static Option<T> Some<T>(T value)
		{
			return new Option<T>(true, value);
		}

		// Would be a property, but then generics can't be used unless the whole class is generic (would conflict with the actual Option<T> struct).
		public static Option<T> None<T>()
		{
			return new Option<T>(false, default);
		}
	}

	public static class OptionExtensions
	{
		public static Option<T> Flatten<T>(this Option<Option<T>> @this)
		{
			return @this.MatchSome(out var option) ? option : Option.None<T>();
		}

		public static bool Equals<T>(this Option<T> @this, Option<T> other) where T : IEquatable<T>
		{
			return @this.Equals(other, (x, y) => x.Equals(y));
		}

		public static bool Equals<T>(this Option<T> @this, Option<T> other, Func<T, T, bool> equals)
		{
			return @this.MatchSome(out var thisValue) && other.MatchSome(out var otherValue) && equals(thisValue, otherValue);
		}
	}

	/// <summary>
	///		Represents an optional, non-nullable value. No, this is not a reimplementation of <see cref="Nullable{T}" />, but a replacement.
	/// </summary>
	public readonly struct Option<T>
	{
		private readonly bool _isSome;
		private readonly T _value;

		public bool IsSome => _isSome;
		public bool IsNone => !IsSome;

		public Option(bool isSome, T value)
		{
			_isSome = isSome;
			_value = value;
		}

		public bool MatchSome(out T value)
		{
			value = _value;
			return IsSome;
		}

		public T Expect(string message)
		{
			if (MatchSome(out var some))
			{
				return some;
			}

			throw new InvalidOperationException(message);
		}

		public void ExpectNone(string message)
		{
			if (!IsNone)
			{
				return;
			}

			throw new InvalidOperationException(message);
		}

		public T Unwrap()
		{
			return Expect("Expected a Some option when unwrapping " + typeof(Option<T>) + ".");
		}

		public void UnwrapNone()
		{
			ExpectNone("Expected a None option when unwrapping " + typeof(Option<T>) + ".");
		}

		public T UnwrapOr(T @default)
		{
			return MatchSome(out var value) ? value : @default;
		}

		public Option<T> Or(Option<T> other)
		{
			return MatchSome(out var value) ? Option.Some(value) : other;
		}

		public Option<T> And(Option<T> other)
		{
			return IsNone ? this : other;
		}

		public Option<T> Xor(Option<T> other)
		{
			T value;

			if (MatchSome(out value))
			{
				if (other.IsNone)
				{
					return Option.Some(value);
				}
			}
			else if (other.MatchSome(out value))
			{
				return Option.Some(value);
			}

			return Option.None<T>();
		}

		public Option<TMapped> Map<TMapped>(Func<T, TMapped> map)
		{
			return MatchSome(out var value) ? Option.Some(map(value)) : Option.None<TMapped>();
		}

		public override string ToString()
		{
			return MatchSome(out var value)
				? "Some(" + value + ")"
				: "None";
		}
	}
}
