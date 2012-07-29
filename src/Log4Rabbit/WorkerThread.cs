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
		private readonly int _maxSize;
		private readonly Func<T[], bool> _processor;

		public WorkerThread(string name, TimeSpan interval, int maxSize, Func<T[], bool> processor)
		{
			_interval = interval;
			_maxSize = maxSize;
			_processor = processor;
			_queue = new ConcurrentQueue<T>();
			_disposeEvent = new AutoResetEvent(false);
			_thread = new Thread(Loop) { Name = name, IsBackground = true };
			_thread.Start();
		}

		public void Enqueue(T item)
		{
			if(_queue.Count < _maxSize)
			{
				_queue.Enqueue(item);
			}
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
			T[] items = _queue.ToArray();
			if(items.Length > 0 && _processor.Invoke(items))
			{
				Array.ForEach(items, e => _queue.TryDequeue(out e));
			}
		}
	}
}