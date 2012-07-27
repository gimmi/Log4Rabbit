using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;

namespace log4net.Appender
{
	[TestFixture]
	public class WorkerThreadTest
	{
		[Test]
		public void Should_dequeue_in_batch()
		{
			var worker = new LogWorker();
			var target = new WorkerThread<int>("test", TimeSpan.FromSeconds(.75), int.MaxValue, worker);

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			worker.Go.WaitOne();

			target.Enqueue(4);
			target.Enqueue(5);
			target.Enqueue(6);

			worker.Go.WaitOne();

			worker.Logs.Should().Have.SameSequenceAs(new[] {
				"1, 2, 3",
				"4, 5, 6"
			});
		}

		[Test]
		public void Should_not_dequeue_when_failing()
		{
			var worker = new LogWorker { ReturnValue = false };
			var target = new WorkerThread<int>("test", TimeSpan.FromSeconds(.75), int.MaxValue, worker);

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			worker.Go.WaitOne();

			target.Enqueue(4);
			target.Enqueue(5);
			target.Enqueue(6);

			worker.Go.WaitOne();

			worker.Logs.Should().Have.SameSequenceAs(new[] {
				"1, 2, 3",
				"1, 2, 3, 4, 5, 6"
			});
		}

		[Test]
		public void Should_dequeue_all_when_disposing()
		{
			var worker = new LogWorker();
			var target = new WorkerThread<int>("test", TimeSpan.FromDays(1), int.MaxValue, worker);

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			target.Dispose();

			worker.Logs.Should().Have.SameSequenceAs(new[] {
				"1, 2, 3",
				"Disposed"
			});
		}

		public class LogWorker : IWorker<int>
		{
			public List<string> Logs = new List<string>();
			public AutoResetEvent Go = new AutoResetEvent(false);
			public bool ReturnValue = true;

			public bool Process(int[] items)
			{
				Logs.Add(string.Join(", ", items));
				Go.Set();
				return ReturnValue;
			}

			public void Dispose()
			{
				Logs.Add("Disposed");
			}
		}
	}
}