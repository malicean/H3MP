namespace H3MP.Models
{
	public interface IStamped<out TStamp, out TContent>
	{
		TStamp Stamp { get; }
		TContent Content { get; }
	}
}
