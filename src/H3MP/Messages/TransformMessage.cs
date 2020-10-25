using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct TransformMessage : IPackedSerializable, IDeltable<TransformMessage, TransformMessage>, ILinearFittable<TransformMessage>, IEquatable<TransformMessage>
	{
		public Option<Vector3> Position { get; private set; }

		public Option<Quaternion> Rotation { get; private set; }

		public TransformMessage InitialDelta => this;

		public TransformMessage(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		public TransformMessage(Transform transform) : this(transform.position, transform.rotation)
		{
		}

		public void Deserialize(BitPackReader reader)
		{
			Position = reader.GetVector3();
			Rotation = reader.GetQuaternion();
		}

		public void Serialize(BitPackWriter writer)
		{
			writer.Put(Position);
			writer.Put(Rotation);
		}
1
		public Option<TransformMessage> CreateDelta(TransformMessage head)
        {
            return Option.Some(new TransformMessage(
				Position - head.Position,
				Rotation * Quaternion.Inverse(head.Rotation)
			));
        }

		public TransformMessage ConsumeDelta(TransformMessage head)
        {
            return new TransformMessage(
				Position + head.Position,
				Rotation * head.Rotation
			);
        }

		public TransformMessage Fit(TransformMessage b, double t)
		{
			return new TransformMessage(
				Position.Fit(b.Position, t),
				Rotation.Fit(b.Rotation, t)
			);
		}

		public bool Equals(TransformMessage other)
        {
            return Position == other.Position && Rotation == other.Rotation;
        }
    }
}
