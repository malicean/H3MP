namespace H3MP.Fitting
{
	public interface IInverseFitter<in T>
	{
		float InverseFit(T a, T b, T value);
	}
}
