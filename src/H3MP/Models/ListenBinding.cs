using System.Net;

namespace H3MP.Models
{
	public readonly struct ListenBinding
	{
		public readonly IPAddress IPv4;
		public readonly IPAddress IPv6;
		public readonly ushort Port;

		public ListenBinding(IPAddress ipv4, IPAddress ipv6, ushort port)
		{
			IPv4 = ipv4;
			IPv6 = ipv6;
			Port = port;
		}
	}
}
