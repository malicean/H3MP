using System;
using LiteNetLib.Utils;

namespace H3MP.IO
{
	public readonly struct BitArray
	{
		public const byte BITS_PER_ELEMENT = sizeof(byte) * 8;

		public static BitArray Deserialize(NetDataReader reader)
        {
            var byteLength = reader.GetByte();
			var trailingBits = reader.GetByte();

			var buffer = new byte[byteLength];
			reader.GetBytes(buffer, 0, buffer.Length);

			return new BitArray(buffer, trailingBits);
        }

        public void Serialize(NetDataWriter writer, BitArray value)
        {
			writer.Put((byte) value.Buffer.Length);
			writer.Put((byte) value.TrailingBits);
			writer.Put(value.Buffer);
        }

		public readonly byte[] Buffer;
		public readonly byte TrailingBits;
		public readonly int BitLength;

		public BitArray(byte[] buffer, byte trailingBits)
		{
			Buffer = buffer;
			TrailingBits = trailingBits;
			BitLength = Buffer.Length * BITS_PER_ELEMENT - TrailingBits;
		}

		private ref byte GetBitHolder(int index)
		{
			if (index < 0 && BitLength <= index)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be a non-negative integer that is less than the size of the buffer.");
			}

			return ref Buffer[index / BITS_PER_ELEMENT];
		}

		public bool this[int index]
		{
			get
			{
				var bits = GetBitHolder(index);
				var bit = 1 << (index % BITS_PER_ELEMENT);

				return (bits & bit) == bit;
			}
			set
			{
				ref var bits = ref GetBitHolder(index);
				var bit = (byte) (1 << (index % BITS_PER_ELEMENT));

				bits |= bit;
			}
		}
	}
}
