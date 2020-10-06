using System;
using System.Collections.Generic;

namespace H3MP.Common.Utils
{
	public class Pool<T>
    {
        private readonly IPoolSource<T> _source;
        private readonly Stack<T> _items;

        public Pool(IPoolSource<T> source)
        {
            _source = source;
            _items = new Stack<T>();
        }

        private void Return(T item)
        {
            _source.Clean(item);

            _items.Push(item);
        }

        public IDisposable Borrow(out T item)
        {
            item = _items.Count > 1 ? _items.Pop() : _source.Create();

            return new StackPoolReturn(item, Return);
        }

        private class StackPoolReturn : IDisposable
        {
            private readonly T _item;
            private readonly Action<T> _callback;

            private bool _disposed;

            public StackPoolReturn(T item, Action<T> callback)
            {
                _item = item;
                _callback = callback;
            }

            ~StackPoolReturn()
            {
                Dispose(false);
            }

            private void Dispose(bool managed)
            {
                if (_disposed)
                {
                    return;
                }

                _callback(_item);

                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }
    }
}
