namespace H3MP.Utils
{
	public interface IMutRef<T> : IRef<T>
	{
		new T Value { get; set; }
	}
}
