using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct SmallestThreeQuaternionMessage : IPackedSerializable, IRef<Quaternion>
	{
		private enum Components : byte
		{
			X = 0b00,
			Y = 0b01,
			Z = 0b10,
			W = 0b11
		}

		private Components _largest;
		private float _a;
		private float _b;
		private float _c;

		private Option<Quaternion> _value; // cached
		public Quaternion Value
		{
			get
			{
				if (_value.MatchSome(out var value))
				{
					return value;
				}

				float x;
				float y;
				float z;
				float w;

				switch (_largest)
				{
					case Components.X:
				}
			}
			set
			{

			}
		}

		public SmallestThreeQuaternionMessage(Quaternion value)
		{

		}

		public void Deserialize(BitPackReader reader)
		{

		}

		public void Serialize(BitPackWriter writer)
		{

		}
	}
}
