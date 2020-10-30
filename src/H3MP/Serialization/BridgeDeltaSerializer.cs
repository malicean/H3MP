using H3MP.Differentiation;
using H3MP.IO;
using H3MP.Utils;

namespace H3MP.Serialization
{
	public static class BridgeDeltaSerializerExtensions
	{
		public static IDeltaSerializer<TValue> ToSerializer<TValue, TDelta>(this IDifferentiator<TValue, TDelta> @this, ISerializer<TDelta> serializer)
		{
			return new BridgeDeltaSerializer<TValue, TDelta>(@this, serializer);
		}
	}

	public class BridgeDeltaSerializer<TValue, TDelta> : IDeltaSerializer<TValue>
	{
		private readonly IDifferentiator<TValue, TDelta> _differentiator;
		private readonly ISerializer<Option<TDelta>> _serializer;

		public BridgeDeltaSerializer(IDifferentiator<TValue, TDelta> differentiator, ISerializer<TDelta> serializer)
		{
			_differentiator = differentiator;
			_serializer = serializer.ToOption();
		}

		public TValue Deserialize(ref BitPackReader reader, Option<TValue> baseline)
		{
			return _serializer.Deserialize(ref reader).MatchSome(out var delta)
				? _differentiator.ConsumeDelta(delta, baseline)
				: baseline.Expect("Received a delta when there was no baseline.");
		}

		public void Serialize(ref BitPackWriter writer, TValue value, Option<TValue> baseline)
		{
			_serializer.Serialize(ref writer, _differentiator.CreateDelta(value, baseline));
		}
	}
}
