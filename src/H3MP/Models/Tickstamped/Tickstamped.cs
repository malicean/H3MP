namespace H3MP.Models
{
	public struct Tickstamped<T>
	{
		public uint Tick;
		public T Content;
	}
}
