using System;
using System.Collections.Concurrent;
using System.Linq;
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
			var queue = new ConcurrentQueue<int[]>();
			var go = new AutoResetEvent(false);
			var target = new WorkerThread<int>(TimeSpan.FromSeconds(.75), int.MaxValue, s => {
				queue.Enqueue(s);
				go.Set();
				return true;
			});

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			go.WaitOne();

			target.Enqueue(4);
			target.Enqueue(5);
			target.Enqueue(6);

			go.WaitOne();

			queue.Should().Have.Count.EqualTo(2);
			queue.First().Should().Have.SameSequenceAs(new[] { 1, 2, 3 });
			queue.Last().Should().Have.SameSequenceAs(new[] { 4, 5, 6 });
		}

		[Test]
		public void Should_not_dequeue_when_failing()
		{
			var queue = new ConcurrentQueue<int[]>();
			var go = new AutoResetEvent(false);
			var target = new WorkerThread<int>(TimeSpan.FromSeconds(.75), int.MaxValue, s => {
				queue.Enqueue(s);
				go.Set();
				return false;
			});

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			go.WaitOne();

			target.Enqueue(4);
			target.Enqueue(5);
			target.Enqueue(6);

			go.WaitOne();

			queue.Should().Have.Count.EqualTo(2);
			queue.First().Should().Have.SameSequenceAs(new[] { 1, 2, 3 });
			queue.Last().Should().Have.SameSequenceAs(new[] { 1, 2, 3, 4, 5, 6 });
		}

		[Test]
		public void Should_dequeue_all_when_disposing()
		{
			var queue = new ConcurrentQueue<int[]>();
			var target = new WorkerThread<int>(TimeSpan.FromDays(1), int.MaxValue, s => {
				queue.Enqueue(s);
				return false;
			});

			target.Enqueue(1);
			target.Enqueue(2);
			target.Enqueue(3);

			target.Dispose();

			queue.Should().Have.Count.EqualTo(1);
			queue.First().Should().Have.SameSequenceAs(new[] { 1, 2, 3 });
		}
	}
}