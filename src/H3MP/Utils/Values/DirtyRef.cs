using System;

namespace H3MP.Utils
{
	public readonly struct DirtyRef<T> : IDirty<T, DirtyRef<T>>, IRef<T>
	{
		public T Pure { get; }

		public Option<T> Dirty { get; }

		public T Value => Dirty.UnwrapOr(Pure);

		public DirtyRef(T pure, Option<T> dirty)
		{
			Pure = pure;
			Dirty = dirty;
		}

		public DirtyRef<T> Revert()
		{
			return new DirtyRef<T>(Pure, Option.None<T>());
		}

		public DirtyRef<T> Commit()
		{
			return new DirtyRef<T>(Value, Option.None<T>());
		}
	}

	public static class DirtyRef
	{
		public static DirtyRef<T> Push<T>(this DirtyRef<T> @this, T value) where T : IEquatable<T>
		{
			var pure = @this.Value;

			return new DirtyRef<T>(pure, pure.Equals(value) ? Option.None<T>() : Option.Some(value));
		}
	}
}
