using System;

namespace H3MP.Utils
{
	public interface IDirty<T, TSelf> : IRef<T> where TSelf : IDirty<T, TSelf>
	{
		T Pure { get; }

		Option<T> Dirty { get; }

		TSelf Revert();

		TSelf Commit();
	}
}
