using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;

namespace log4net.Appender
{
	[TestFixture]
	public class RabbitMQAppenderTest
	{
		[SetUp]
		public void SetUp()
		{
			_target = LogManager.GetLogger("Test");
		}

		private ILog _target;

		[Test]
		public void Should_log_all_events()
		{
			for (int i = 0; i < 1000; i++)
			{
				_target.Info(i);
			}

			Consumer.LogData[] msgs = Consumer.GetAllMessages().ToArray();
			msgs.Should().Have.Count.EqualTo(1000);
		}

		[Test]
		public void Should_log_basic_information()
		{
			_target.Info("a log");

			Consumer.LogData ev = Consumer.GetAllMessages().First();
			ev.Domain.Should().Be.EqualTo(AppDomain.CurrentDomain.FriendlyName);
			ev.Level.Should().Be.EqualTo("INFO");
			ev.Logger.Should().Be.EqualTo("Test");
			ev.Thread.Should().Be.EqualTo(Thread.CurrentThread.Name);
			ev.UserName.Should().Be.EqualTo(WindowsIdentity.GetCurrent().Name);
			ev.Message.Should().Be.EqualTo("a log");
			ev.TimeStamp.Should().Be.IncludedIn(DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(10));
		}

		[Test]
		public void Should_log_exception()
		{
			try
			{
				throw new ApplicationException("exc msg");
			}
			catch (Exception e)
			{
				_target.Info("a log", e);
			}
			Consumer.LogData ev = Consumer.GetAllMessages().First();
			ev.Exception.Should().Contain("exc msg");
		}

		[Test]
		public void Should_log_context_properties()
		{
			GlobalContext.Properties.Clear();
			ThreadContext.Properties.Clear();
			ThreadContext.Properties["threadContextProperty"] = "value";

			_target.Info("a log");

			Consumer.LogData ev = Consumer.GetAllMessages().First();

			ev.Properties.Should().Have.Count.EqualTo(1);
			ev.Properties.First().Name.Should().Be.EqualTo("threadContextProperty");
			ev.Properties.First().Value.Should().Be.EqualTo("value");
		}

		[Test]
		public void Should_log_globalcontext_properties()
		{
			GlobalContext.Properties.Clear();
			ThreadContext.Properties.Clear();
			GlobalContext.Properties["globalContextProperty"] = "value";

			_target.Info("a log");

			Consumer.LogData ev = Consumer.GetAllMessages().First();

			ev.Properties.Should().Have.Count.EqualTo(1);
			ev.Properties.First().Name.Should().Be.EqualTo("globalContextProperty");
			ev.Properties.First().Value.Should().Be.EqualTo("value");
		}
	}
}
