namespace AkashaScanner.Core
{
    public class TaskQueue
    {
        private readonly object CounterMutex = new();
        private readonly ManualResetEvent Empty = new(true);
        private readonly ManualResetEvent NotFull = new(true);
        private volatile int Counter = 0;

        private readonly object ThreadMutex = new();
        private volatile int NumberOfThreads = 0;
        private readonly Queue<(Action<object?>, object?, ManualResetEvent)> Queue = new();

        private readonly int MaxThreads;
        private readonly int FullAt;

        public TaskQueue(int? maxThreads = null)
        {
            MaxThreads = maxThreads ?? Environment.ProcessorCount;
            FullAt = (int)(MaxThreads * 1.5);
        }

        public void WaitAll()
        {
            Empty.WaitOne();
        }

        public Action Add<T>(Action<T> action, T arg)
        {
            lock (CounterMutex)
            {
                Empty.Reset();
                ++Counter;
                if (Counter >= FullAt)
                {
                    NotFull.Reset();
                }
            }
            ManualResetEvent notify = new(false);
            lock (ThreadMutex)
            {
                Queue.Enqueue((x => action((T)x!), arg, notify));
                if (NumberOfThreads < MaxThreads)
                {
                    ++NumberOfThreads;
                    ThreadPool.QueueUserWorkItem(Work);
                }
            }
            NotFull.WaitOne();
            return () => notify.WaitOne();
        }

        public Action Add(Action action)
        {
            return Add<object?>(_ => action(), null);
        }

        private void Work(object? _)
        {
            (Action<object?>, object?, ManualResetEvent) item;
            while (true)
            {
                lock (ThreadMutex)
                {
                    if (!Queue.TryDequeue(out item))
                    {
                        --NumberOfThreads;
                        break;
                    }
                }
                var (action, arg, notify) = item;
                action(arg);
                notify.Set();
                lock (CounterMutex)
                {
                    --Counter;
                    if (Counter == 0)
                        Empty.Set();

                    if (Counter < FullAt)
                        NotFull.Set();
                }
            }
        }
    }
}
