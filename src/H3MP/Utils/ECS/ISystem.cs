namespace H3MP.Utils
{
	public interface ISystem<TComponents>
	{
		void UpdateLocal(ref TComponents component);

		void UpdateRemote(ref TComponents component);
	}
}
