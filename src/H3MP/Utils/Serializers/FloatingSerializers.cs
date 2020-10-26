using System.Runtime.InteropServices;

namespace H3MP.Utils
{
	public readonly struct FloatSerializer<TUIntSerializer> : ISerializer<float> where TUIntSerializer : ISerializer<uint>
	{
		private readonly TUIntSerializer _uint;

		public FloatSerializer(TUIntSerializer @uint)
		{
			_uint = @uint;
		}

		public float Deserialize(ref BitPackReader reader)
		{
			return new FloatToUInt
			{
				Integral = _uint.Deserialize(ref reader)
			}.Floating;
		}

		public void Serialize(ref BitPackWriter writer, float value)
		{
			_uint.Serialize(ref writer, new FloatToUInt
			{
				Floating = value
			}.Integral);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatToUInt
		{
			[FieldOffset(0)]
			public float Floating;

			[FieldOffset(0)]
			public uint Integral;
		}
	}

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

	public readonly struct DecimalSerializer<TULongSerializer> : ISerializer<decimal> where TULongSerializer : ISerializer<ulong>
	{
		private readonly TULongSerializer _ulong;

		public DecimalSerializer(TULongSerializer @ulong)
		{
			_ulong = @ulong;
		}

		public decimal Deserialize(ref BitPackReader reader)
		{
			return new DecimalToULongs
			{
				Integral1 = _ulong.Deserialize(ref reader),
				Integral2 = _ulong.Deserialize(ref reader)
			}.Floating;
		}

		public void Serialize(ref BitPackWriter writer, decimal value)
		{
			var ulongs = new DecimalToULongs
			{
				Floating = value
			};

			_ulong.Serialize(ref writer, ulongs.Integral1);
			_ulong.Serialize(ref writer, ulongs.Integral2);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalToULongs
		{
			[FieldOffset(0)]
			public decimal Floating;

			[FieldOffset(0)]
			public ulong Integral1;

			[FieldOffset(sizeof(ulong))]
			public ulong Integral2;
		}
	}
}
