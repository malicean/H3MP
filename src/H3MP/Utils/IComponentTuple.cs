namespace H3MP.Utils
{
	public interface IComponentTuple<in TEntity>
	{
		void Take(TEntity entity);

		void Store(TEntity entity);
	}
}
