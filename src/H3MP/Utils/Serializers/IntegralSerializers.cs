namespace H3MP.Utils
{
	public readonly struct ByteSerializer : ISerializer<byte>
	{
		public byte Deserialize(ref BitPackReader reader)
		{
			return reader.Bytes.Pop();
		}

		public void Serialize(ref BitPackWriter writer, byte value)
		{
			writer.Bytes.Push(value);
		}
	}

	public readonly struct SByteSerializer<TByteSerializer> : ISerializer<sbyte> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public SByteSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public sbyte Deserialize(ref BitPackReader reader)
		{
			return (sbyte) _byte.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, sbyte value)
		{
			_byte.Serialize(ref writer, (byte) value);
		}
	}

	public readonly struct UShortSerializer<TByteSerializer> : ISerializer<ushort> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public UShortSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public ushort Deserialize(ref BitPackReader reader)
		{
			ushort value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (ushort) (_byte.Deserialize(ref reader) << 8);

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ushort value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
		}
	}

	public readonly struct ShortSerializer<TUShortSerializer> : ISerializer<short> where TUShortSerializer : ISerializer<ushort>
	{
		private readonly TUShortSerializer _ushort;

		public ShortSerializer(TUShortSerializer @ushort)
		{
			_ushort = @ushort;
		}

		public short Deserialize(ref BitPackReader reader)
		{
			return (short) _ushort.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, short value)
		{
			_ushort.Serialize(ref writer, (ushort) value);
		}
	}

	public readonly struct UIntSerializer<TByteSerializer> : ISerializer<uint> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public UIntSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public uint Deserialize(ref BitPackReader reader)
		{
			uint value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (uint) _byte.Deserialize(ref reader) << 8;
			value |= (uint) _byte.Deserialize(ref reader) << 16;
			value |= (uint) _byte.Deserialize(ref reader) << 24;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, uint value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
			_byte.Serialize(ref writer, (byte) (value >> 16));
			_byte.Serialize(ref writer, (byte) (value >> 24));
		}
	}

	public readonly struct IntSerializer<TUIntSerializer> : ISerializer<int> where TUIntSerializer : ISerializer<uint>
	{
		private readonly TUIntSerializer _uint;

		public IntSerializer(TUIntSerializer @uint)
		{
			_uint = @uint;
		}

		public int Deserialize(ref BitPackReader reader)
		{
			return (int) _uint.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, int value)
		{
			_uint.Serialize(ref writer, (uint) value);
		}
	}

	public readonly struct ULongSerializer<TByteSerializer> : ISerializer<ulong> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public ULongSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public ulong Deserialize(ref BitPackReader reader)
		{
			ulong value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (ulong) _byte.Deserialize(ref reader) << 8;
			value |= (ulong) _byte.Deserialize(ref reader) << 16;
			value |= (ulong) _byte.Deserialize(ref reader) << 24;
			value |= (ulong) _byte.Deserialize(ref reader) << 32;
			value |= (ulong) _byte.Deserialize(ref reader) << 40;
			value |= (ulong) _byte.Deserialize(ref reader) << 48;
			value |= (ulong) _byte.Deserialize(ref reader) << 56;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ulong value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
			_byte.Serialize(ref writer, (byte) (value >> 16));
			_byte.Serialize(ref writer, (byte) (value >> 24));
			_byte.Serialize(ref writer, (byte) (value >> 32));
			_byte.Serialize(ref writer, (byte) (value >> 40));
			_byte.Serialize(ref writer, (byte) (value >> 48));
			_byte.Serialize(ref writer, (byte) (value >> 56));
		}
	}

	public readonly struct LongSerializer<TULongSerializer> : ISerializer<long> where TULongSerializer : ISerializer<ulong>
	{
		private readonly TULongSerializer _ulong;

		public LongSerializer(TULongSerializer @ulong)
		{
			_ulong = @ulong;
		}

		public long Deserialize(ref BitPackReader reader)
		{
			return (long) _ulong.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, long value)
		{
			_ulong.Serialize(ref writer, (ulong) value);
		}
	}
}
