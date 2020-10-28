using System;

namespace H3MP.Utils
{
	public class StringSerializer : ISerializer<string>
	{
		private readonly DynamicArraySerializer<char> _array;

		public StringSerializer(ISerializer<char> @char, ISerializer<int> length)
		{
			_array = @char.ToArrayDynamic(length);
		}

		public string Deserialize(ref BitPackReader reader)
		{
			var chars = _array.Deserialize(ref reader);

			return new string(chars, 0, chars.Length);
		}

		public void Serialize(ref BitPackWriter writer, string value)
		{
			var chars = value.ToCharArray();

			_array.Serialize(ref writer, chars);
		}
	}
}
