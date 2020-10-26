using H3MP.Extensions;
using H3MP.Models;
using LiteNetLib.Utils;

namespace H3MP.Utils
{
	public struct BitPackReader
	{
		public delegate T Converter<out T>(ref BitPackReader reader);

		public BitQueue Bits;
		public ByteQueue Bytes;

		public BitPackReader(NetDataReader reader)
		{
			Bits = new BitQueue(reader.GetBitArray());
			Bytes = new ByteQueue(reader.GetRemainingBytes());
		}
	}
}
