using System;
using H3MP.IO;

namespace H3MP.Serialization
{
	public static class BranchingSerializerExtensions
	{
		public static ISerializer<T> ToBranching<T>(this ISerializer<T> @this, Func<T, bool> @if, ISerializer<T> then)
		{
			return new BranchingSerializer<T>(@if, then, @this);
		}
	}

	public class BranchingSerializer<T> : ISerializer<T>
	{
		private readonly Func<T, bool> _if;
		private readonly ISerializer<T> _then;
		private readonly ISerializer<T> _else;

		public BranchingSerializer(Func<T, bool> @if, ISerializer<T> then, ISerializer<T> @else)
		{
			_if = @if;
			_then = then;
			_else = @else;
		}

		public T Deserialize(ref BitPackReader reader)
		{
			return reader.Bits.Pop()
				? _then.Deserialize(ref reader)
				: _else.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, T value)
		{
			var conditional = _if(value);
			writer.Bits.Push(conditional);

			if (conditional)
			{
				_then.Serialize(ref writer, value);
			}
			else
			{
				_else.Serialize(ref writer, value);
			}
		}
	}
}
