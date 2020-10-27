using System.Runtime.InteropServices;

namespace H3MP.Utils
{
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
