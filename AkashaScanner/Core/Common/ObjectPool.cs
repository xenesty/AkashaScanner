using System.Collections.Concurrent;

namespace AkashaScanner.Core
{
    public class ObjectPool<T> : IDisposable
    {
        private readonly ConcurrentBag<Object> Objects = new();

        private readonly Func<T> Factory;
        private bool Disposed;

        public ObjectPool(Func<T> factory)
        {
            Factory = factory;
        }

        public IObject Get()
        {
            if (Objects.TryTake(out Object? obj))
            {
                obj.IsInUse = true;
                return obj;
            }

            var newObj = new Object(this)
            {
                Data = Factory(),
                IsInUse = true,
            };

            return newObj;
        }
        private void Release(Object obj)
        {
            if (Disposed)
            {
                if (obj.Data is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                obj.IsInUse = false;
                Objects.Add(obj);
            }
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                foreach (var obj in Objects)
                {
                    if (obj.Data is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                Objects.Clear();
            }
            GC.SuppressFinalize(this);
        }

        ~ObjectPool()
        {
            Dispose();
        }

        public interface IObject : IDisposable
        {
            public T Data { get; }
        }

        private class Object : IObject
        {
            private readonly ObjectPool<T> Pool;
            public T Data = default!;
            public volatile bool IsInUse;

            T ObjectPool<T>.IObject.Data { get => Data; }

            public Object(ObjectPool<T> pool)
            {
                Pool = pool;
            }

            public void Dispose()
            {
                if (IsInUse)
                    Pool.Release(this);
            }
        }
    }

    public class ObjectPool<T, K> : IDisposable where K : IEquatable<K>
    {
        private readonly ConcurrentDictionary<K, ObjectPool<T>> Pools = new();

        private readonly Func<K, T> Factory;
        private bool Disposed;

        public ObjectPool(Func<K, T> factory)
        {
            Factory = factory;
        }

        public IObject Get(K key)
        {
            if (!Pools.ContainsKey(key))
                Pools[key] = new ObjectPool<T>(() => Factory(key));

            var newObj = new Object()
            {
                Key = key,
                Obj = Pools[key].Get(),
            };

            return newObj;
        }

        ~ObjectPool()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                foreach (var item in Pools)
                {
                    item.Value.Dispose();
                }
                Pools.Clear();
            }
            GC.SuppressFinalize(this);
        }

        public interface IObject : IDisposable
        {
            public K Key { get; }
            public T Data { get; }
        }

        private class Object : IObject
        {
            public K Key = default!;
            public ObjectPool<T>.IObject Obj = default!;

            K ObjectPool<T, K>.IObject.Key { get => Key; }
            T ObjectPool<T, K>.IObject.Data { get => Obj.Data; }

            public void Dispose()
            {
                Obj.Dispose();
            }
        }
    }
}
