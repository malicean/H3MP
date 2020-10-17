using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct TransformMessage : INetSerializable, ILinearFittable<TransformMessage>
	{
		public Vector3 Position { get; private set; }

		public Quaternion Rotation { get; private set; }

		public TransformMessage(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		public TransformMessage(Transform transform) : this(transform.position, transform.rotation)
		{
		}

		public TransformMessage Fit(TransformMessage newer, double t)
		{
			return new TransformMessage(
				Position.Fit(newer.Position, t), 
				Rotation.Fit(newer.Rotation, t)
			);
		}

		public void Apply(Transform transform)
		{
			transform.position = Position;
			transform.rotation = Rotation;
		}

		public void Deserialize(NetDataReader reader)
		{
			Position = reader.GetVector3();
			Rotation = reader.GetQuaternion();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Position);
			writer.Put(Rotation);
		}
	}
}