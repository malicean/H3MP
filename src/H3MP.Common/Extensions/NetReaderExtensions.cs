using LiteNetLib.Utils;

namespace H3MP.Common.Extensions
{
	public static class NetReaderExtensions
	{
		public static bool TryCatchGet<T>(this NetDataReader @this, out T value) where T : INetSerializable, new()
		{
			try
			{
				value = @this.Get<T>();
			}
			catch
			{
				value = default;
				return false;
			}

			return true;
		}
	}
}
