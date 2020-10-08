namespace H3MP.Utils
{
    public interface IPoolSource<T>
    {
        T Create();

        void Clean(T item);
    }
}
