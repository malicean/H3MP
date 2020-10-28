using System;
using H3MP.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct TransformMessage : IEquatable<TransformMessage>
	{
		public static TransformMessage Identity { get; } = new TransformMessage
		{
			Position = Vector3.zero,
			Rotation = Quaternion.identity
		};

		public Vector3 Position;
		public Quaternion Rotation;

		public bool Equals(TransformMessage other)
        {
			return Position == other.Position && Rotation == other.Rotation;
        }
    }

	public class TransformMessageFitter : IFitter<TransformMessage>
	{
		private readonly IFitter<Vector3> _position;
		private readonly IFitter<Quaternion> _rotation;

		public TransformMessageFitter(IFitter<Vector3> position, IFitter<Quaternion> rotation)
		{
			_position = position;
			_rotation = rotation;
		}

		public TransformMessage Fit(TransformMessage a, TransformMessage b, float t)
		{
			var fitter = new SuperFitter<TransformMessage>(a, b, t);

			fitter.Include(x => x.Position, (ref TransformMessage body, Vector3 value) => body.Position = value, _position);
			fitter.Include(x => x.Rotation, (ref TransformMessage body, Quaternion value) => body.Rotation = value, _rotation);

			return fitter.Body;
		}
	}

	public class TransformMessageDifferentiator : IDifferentiator<TransformMessage, TransformMessage>
	{
		public Option<TransformMessage> CreateDelta(TransformMessage now, Option<TransformMessage> baseline)
		{
			if (baseline.MatchSome(out var value))
			{
				var delta = new TransformMessage
				{
					Position = now.Position - value.Position,
					Rotation = now.Rotation * Quaternion.Inverse(value.Rotation)
				};

				return delta.Equals(TransformMessage.Identity)
					? Option.None<TransformMessage>()
					: Option.Some(delta);
			}

			return Option.Some(now);
		}

		public TransformMessage ConsumeDelta(TransformMessage delta, Option<TransformMessage> now)
		{
			return now.MatchSome(out var value)
				? new TransformMessage
				{
					Position = delta.Position + value.Position,
					Rotation = delta.Rotation * value.Rotation
				}
				: delta;
		}
	}

	public class TransformMessageSerializer : ISerializer<TransformMessage>
	{
		private readonly ISerializer<Vector3> _position;
		private readonly ISerializer<Quaternion> _rotation;

		public TransformMessageSerializer(ISerializer<Vector3> position, ISerializer<Quaternion> rotation)
		{
			_position = position;
			_rotation = rotation;
		}

		public TransformMessage Deserialize(ref BitPackReader reader)
		{
			return new TransformMessage
			{
				Position = _position.Deserialize(ref reader),
				Rotation = _rotation.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, TransformMessage value)
		{
			_position.Serialize(ref writer, value.Position);
			_rotation.Serialize(ref writer, value.Rotation);
		}
	}
}
