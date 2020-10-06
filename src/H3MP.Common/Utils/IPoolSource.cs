namespace H3MP.Common.Utils
{
    public interface IPoolSource<T>
    {
        T Create();

        void Clean(T item);
    }
}
