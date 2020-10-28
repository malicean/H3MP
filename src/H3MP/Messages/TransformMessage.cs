using System;
using H3MP.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct TransformMessage : IEquatable<TransformMessage>
	{
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
			var fits = new SuperFitter<TransformMessage>(a, b, t);

			return new TransformMessage
			{
				Position = fits.Fit(x => x.Position, _position),
				Rotation = fits.Fit(x => x.Rotation, _rotation)
			};
		}
	}

	public class TransformMessageDeltaSerializer : IDeltaSerializer<TransformMessage>
	{
		private readonly IDeltaSerializer<Vector3> _position;
		private readonly IDeltaSerializer<Quaternion> _rotation;

		public TransformMessageDeltaSerializer(IDeltaSerializer<Vector3> position, IDeltaSerializer<Quaternion> rotation)
		{
			_position = position;
			_rotation = rotation;
		}

		public TransformMessage Deserialize(ref BitPackReader reader, Option<TransformMessage> baseline)
		{
			return new TransformMessage
			{
				Position = _position.Deserialize(ref reader, baseline.Map(x => x.Position)),
				Rotation = _rotation.Deserialize(ref reader, baseline.Map(x => x.Rotation))
			};
		}

		public void Serialize(ref BitPackWriter writer, TransformMessage now, Option<TransformMessage> baseline)
		{
			_position.Serialize(ref writer, now.Position, baseline.Map(x => x.Position));
			_rotation.Serialize(ref writer, now.Rotation, baseline.Map(x => x.Rotation));
		}
	}
}
