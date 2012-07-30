using System;
using System.Collections.Concurrent;
using System.Threading;

namespace log4net.Appender
{
	public class WorkerThread<T> : IDisposable
	{
		private readonly ConcurrentQueue<T> _queue;
		private readonly AutoResetEvent _disposeEvent;
		private readonly Thread _thread;
		private readonly TimeSpan _interval;
		private readonly Action<T[]> _processor;

		public WorkerThread(string name, TimeSpan interval, Action<T[]> processor)
		{
			_interval = interval;
			_processor = processor;
			_queue = new ConcurrentQueue<T>();
			_disposeEvent = new AutoResetEvent(false);
			_thread = new Thread(Loop) { Name = name, IsBackground = true };
			_thread.Start();
		}

		public void Enqueue(T item)
		{
			_queue.Enqueue(item);
		}

		public void Dispose()
		{
			_disposeEvent.Set();
			_thread.Join();
		}

		private void Loop()
		{
			while(true)
			{
				if(_disposeEvent.WaitOne(_interval))
				{
					Dequeue();
					return;
				}
				Dequeue();
			}
		}

		private void Dequeue()
		{
			int count = _queue.Count;
			if(count <= 0)
			{
				return;
			}
			var items = new T[count];
			for(int i = 0; i < count; i++)
			{
				_queue.TryDequeue(out items[i]);
			}
			_processor.Invoke(items);
		}
	}
}