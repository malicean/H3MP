using System;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct MoveMessage : IPackedSerializable, IDeltable<MoveMessage, MoveMessage>, IEquatable<MoveMessage>, ILinearFittable<MoveMessage>
	{
		public Option<TransformMessage> Head;
		public Option<TransformMessage> HandLeft;
		public Option<TransformMessage> HandRight;

		public MoveMessage InitialDelta => this;

		public void Deserialize(ref BitPackReader reader)
		{
			Head = reader.GetOption<TransformMessage>();
			HandLeft = reader.GetOption<TransformMessage>();
			HandRight = reader.GetOption<TransformMessage>();
		}

		public void Serialize(ref BitPackWriter writer)
		{
			writer.Put(Head);
			writer.Put(HandLeft);
			writer.Put(HandRight);
		}

		public Option<MoveMessage> CreateDelta(MoveMessage baseline)
        {
			var deltas = new DeltaCreator<MoveMessage>(this, baseline);
			var delta = new MoveMessage
			{
				Head = deltas.Create(x => x.Head),
				HandLeft = deltas.Create(x => x.HandLeft),
				HandRight = deltas.Create(x => x.HandRight)
			};

			return delta.Equals(default)
				? Option.None<MoveMessage>()
				: Option.Some(delta);
        }

		public MoveMessage ConsumeDelta(MoveMessage delta)
        {
			var deltas = new DeltaConsumer<MoveMessage>(this, delta);

            return new MoveMessage
			{
				Head = deltas.Consume(x => x.Head),
				HandLeft = deltas.Consume(x => x.HandLeft),
				HandRight = deltas.Consume(x => x.HandRight)
			};
        }

		public bool Equals(MoveMessage other)
        {
            return Head.Equals(other.Head)
				&& HandLeft.Equals(other.HandLeft)
				&& HandRight.Equals(other.HandRight);
        }

		public MoveMessage Fit(MoveMessage b, double t)
		{
			var fits = new FitCreator<MoveMessage>(this, b, t);

			return new MoveMessage
			{
				Head = fits.Fit(x => x.Head),
				HandLeft = fits.Fit(x => x.HandLeft),
				HandRight = fits.Fit(x => x.HandRight)
			};
		}
    }
}
