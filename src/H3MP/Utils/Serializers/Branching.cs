using System;

namespace H3MP.Utils
{
	public static class BranchingSerializerExtensions
	{
		public static ISerializer<TValue> ToBranching<TValue>(this ISerializer<TValue> @this, Func<TValue, bool> @if, ISerializer<TValue> then)
		{
			return new BranchingSerializer<TValue>(@if, then, @this);
		}
	}

	public class BranchingSerializer<TValue> : ISerializer<TValue>
	{
		private readonly Func<TValue, bool> _if;
		private readonly ISerializer<TValue> _then;
		private readonly ISerializer<TValue> _else;

		public BranchingSerializer(Func<TValue, bool> @if, ISerializer<TValue> then, ISerializer<TValue> @else)
		{
			_if = @if;
			_then = then;
			_else = @else;
		}

		public TValue Deserialize(ref BitPackReader reader)
		{
			return reader.Bits.Pop()
				? _then.Deserialize(ref reader)
				: _else.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, TValue value)
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
