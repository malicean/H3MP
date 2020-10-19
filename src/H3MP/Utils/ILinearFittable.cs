namespace H3MP.Utils
{
	public interface ILinearFittable<TSelf> where TSelf : ILinearFittable<TSelf>
	{
		TSelf Fit(TSelf other, double t);
	}
}
