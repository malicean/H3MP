using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct TransformMessage : ISerializer, IDeltable<TransformMessage, TransformMessage>, ILinearFittable<TransformMessage>, IEquatable<TransformMessage>
	{
		public Option<Vector3> Position;

		public Option<Quaternion> Rotation;

		public TransformMessage InitialDelta => this;

		public void Deserialize(ref BitPackReader reader)
		{
			Position = reader.GetOption((ref BitPackReader r) => r.Bytes.GetVector3());
			Rotation = reader.GetOption<SmallestThreeQuaternionMessage>().Map(x => x.Value);
		}

		public void Serialize(ref BitPackWriter writer)
		{
			writer.Put(Position, (ref BitPackWriter w, Vector3 v) => w.Bytes.Put(v));
			writer.Put(Rotation.Map(x => new SmallestThreeQuaternionMessage(x)));
		}

		public Option<TransformMessage> CreateDelta(TransformMessage baseline)
        {
            var deltas = new DeltaCreator<TransformMessage>(this, baseline);
			var delta = new TransformMessage
			{
				Position = deltas.Create(x => x.Position, x => x.ToDelta()),
				Rotation = deltas.Create(x => x.Rotation, x => x.ToDelta()),
			};

			return delta.Equals(default)
				? Option.None<TransformMessage>()
				: Option.Some(delta);
        }

		public TransformMessage ConsumeDelta(TransformMessage delta)
        {
			var deltas = new DeltaConsumer<TransformMessage>(this, delta);

            return new TransformMessage
			{
				Position = deltas.Consume(x => x.Position, x => x.ToDelta(), x => x),
				Rotation = deltas.Consume(x => x.Rotation, x => x.ToDelta(), x => x)
			};
        }

		public TransformMessage Fit(TransformMessage b, double t)
		{
			var fits = new FitCreator<TransformMessage>(this, b, t);

			return new TransformMessage
			{
				Position = fits.Fit(x => x.Position, x => x.ToFittable(), x => x),
				Rotation = fits.Fit(x => x.Rotation, x => x.ToFittable(), x => x)
			};
		}

		public bool Equals(TransformMessage other)
        {
			return Position.Equals(other.Position, (x, y) => x == y)
				&& Rotation.Equals(other.Rotation, (x, y) => x == y);
        }
    }
}
