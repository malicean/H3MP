using FistVR;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct PlayerTransformsMessage : INetSerializable, ILinearFittable<PlayerTransformsMessage>
	{
		public TransformMessage Head { get; private set; }

		public TransformMessage HandLeft { get; private set; }

		public TransformMessage HandRight { get; private set; }

		public PlayerTransformsMessage(TransformMessage head, TransformMessage handLeft, TransformMessage handRight)
		{
			Head = head;
			HandLeft = handLeft;
			HandRight = handRight;
		}

		public PlayerTransformsMessage(Transform head, Transform handLeft, Transform handRight) : this(new TransformMessage(head), new TransformMessage(handLeft), new TransformMessage(handRight))
		{
		}

		public PlayerTransformsMessage Fit(PlayerTransformsMessage other, double t)
		{
			return new PlayerTransformsMessage(
				Head.Fit(other.Head, t),
				HandLeft.Fit(other.HandLeft, t),
				HandRight.Fit(other.HandRight, t)
			);
		}

		public void Deserialize(NetDataReader reader)
		{
			Head = reader.Get<TransformMessage>();
			HandLeft = reader.Get<TransformMessage>();
			HandRight = reader.Get<TransformMessage>();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Head);
			writer.Put(HandLeft);
			writer.Put(HandRight);
		}
	}
}
