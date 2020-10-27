using System;

namespace H3MP.Utils
{
	public static class BranchingSerializerExtensions
	{
		// @this is TSerializer2 so the conditional function is next to the conditional result
		public static BranchingSerializer<TValue, TSerializer1, TSerializer2> ToBranching<TValue, TSerializer1, TSerializer2>(this TSerializer2 @this, Func<TValue, bool> @if, TSerializer1 then)
			where TSerializer1 : ISerializer<TValue>
			where TSerializer2 : ISerializer<TValue>
		{
			return new BranchingSerializer<TValue, TSerializer1, TSerializer2>(@if, then, @this);
		}
	}

	public readonly struct BranchingSerializer<TValue, TSerializer1, TSerializer2> : ISerializer<TValue>
		where TSerializer1 : ISerializer<TValue>
		where TSerializer2 : ISerializer<TValue>
	{
		private readonly Func<TValue, bool> _if;
		private readonly TSerializer1 _then;
		private readonly TSerializer2 _else;

		public BranchingSerializer(Func<TValue, bool> @if, TSerializer1 then, TSerializer2 @else)
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
