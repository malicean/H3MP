namespace H3MP.Utils
{
	public interface ICopyable<out T> where T : ICopyable<T>
	{
		T Copy();
	}
}
