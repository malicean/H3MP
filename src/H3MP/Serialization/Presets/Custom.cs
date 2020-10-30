using H3MP.Models;

namespace H3MP.Serialization
{
	public static class CustomSerializers
	{
		public static ISerializer<Key32> Key32 { get; } = new Key32Serializer();
	}
}
