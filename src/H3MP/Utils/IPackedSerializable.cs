using LiteNetLib.Utils;

namespace H3MP.Utils
{
	public interface IPackedSerializable
	{
		void Deserialize(BitPackReader reader);

		void Serialize(BitPackWriter writer);
	}
}
