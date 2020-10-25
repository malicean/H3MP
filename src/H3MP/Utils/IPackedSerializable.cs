using LiteNetLib.Utils;

namespace H3MP.Utils
{
	public interface IPackedSerializable
	{
		void Deserialize(ref BitPackReader reader);

		void Serialize(ref BitPackWriter writer);
	}
}
