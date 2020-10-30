using LiteNetLib.Utils;

namespace H3MP.IO
{
	public struct BitPackReader
	{
		public delegate T Converter<out T>(ref BitPackReader reader);

		public BitQueue Bits;
		public ByteQueue Bytes;

		public BitPackReader(NetDataReader reader)
		{
			Bits = new BitQueue(BitArray.Deserialize(reader));
			Bytes = new ByteQueue(reader.GetRemainingBytes());
		}
	}
}
