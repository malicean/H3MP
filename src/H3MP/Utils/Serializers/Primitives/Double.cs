using System.Runtime.InteropServices;

namespace H3MP.Utils
{
	public readonly struct DoubleSerializer<TULongSerializer> : ISerializer<double> where TULongSerializer : ISerializer<ulong>
	{
		private readonly TULongSerializer _ulong;

		public DoubleSerializer(TULongSerializer @ulong)
		{
			_ulong = @ulong;
		}

		public double Deserialize(ref BitPackReader reader)
		{
			return new DoubleToULong
			{
				Integral = _ulong.Deserialize(ref reader)
			}.Floating;
		}

		public void Serialize(ref BitPackWriter writer, double value)
		{
			_ulong.Serialize(ref writer, new DoubleToULong
			{
				Floating = value
			}.Integral);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleToULong
		{
			[FieldOffset(0)]
			public double Floating;

			[FieldOffset(0)]
			public ulong Integral;
		}
	}
}
