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
}
