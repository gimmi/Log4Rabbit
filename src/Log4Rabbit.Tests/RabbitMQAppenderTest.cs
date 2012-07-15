using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using log4net.Config;

namespace log4net.Appender
{
	[TestFixture]
	public class RabbitMQAppenderTest
	{
		private ILog _target;

		[SetUp]
		public void SetUp()
		{
			_target = LogManager.GetLogger("Test");
		}

		[Test]
		public void Should_use_default_values()
		{
			ConfigureWithDefault();

			var appender = (RabbitMQAppender)LogManager.GetRepository().GetAppenders().First();
			appender.HostName.Should().Be.EqualTo("localhost");
			appender.VirtualHost.Should().Be.EqualTo("/");
			appender.UserName.Should().Be.EqualTo("guest");
			appender.Password.Should().Be.EqualTo("guest");
			appender.RequestedHeartbeat.Should().Be.EqualTo(0);
			appender.Port.Should().Be.EqualTo(5672);
			appender.Exchange.Should().Be.EqualTo("logs");
			appender.RoutingKey.Should().Be.EqualTo("");
		}

		[Test]
		public void Should_use_configured_values()
		{
			XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
<log4net>
	<appender name='RabbitMQAppender' type='log4net.Appender.RabbitMQAppender, Log4Rabbit'>
		<HostName value='host'/>
		<VirtualHost value='vh'/>
		<UserName value='usr'/>
		<Password value='pwd'/>
		<RequestedHeartbeat value='5'/>
		<Port value='8080'/>
		<Exchange value='xch'/>
		<RoutingKey value='rk'/>
	</appender>
	<root>
		<level value='ALL' />
		<appender-ref ref='RabbitMQAppender' />
	</root>
</log4net>
")));

			var appender = (RabbitMQAppender)LogManager.GetRepository().GetAppenders().First();
			appender.HostName.Should().Be.EqualTo("host");
			appender.VirtualHost.Should().Be.EqualTo("vh");
			appender.UserName.Should().Be.EqualTo("usr");
			appender.Password.Should().Be.EqualTo("pwd");
			appender.RequestedHeartbeat.Should().Be.EqualTo(5);
			appender.Port.Should().Be.EqualTo(8080);
			appender.Exchange.Should().Be.EqualTo("xch");
			appender.RoutingKey.Should().Be.EqualTo("rk");
		}

		[Test]
		public void Should_log_all_events()
		{
			ConfigureWithDefault();

			for(int i = 0; i < 1000; i++)
			{
				_target.Info(i);
			}

			Consumer.LogData[] msgs = Consumer.GetAllMessages().ToArray();
			msgs.Should().Have.Count.EqualTo(1000);
		}

		[Test, Ignore("for testing connection/model recovery. Need to manually close/shutdown rabbit")]
		public void Should_recover_from_connection_errors()
		{
			ConfigureWithDefault();

			int i = 1;
			while(true)
			{
				_target.Info(i);
				Debug.WriteLine("Published " + i);
				i++;
				Thread.Sleep(100);
			}
		}

		[Test]
		public void Should_log_basic_information()
		{
			ConfigureWithDefault();

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
			ConfigureWithDefault();

			try
			{
				throw new ApplicationException("exc msg");
			}
			catch(Exception e)
			{
				_target.Info("a log", e);
			}
			Consumer.LogData ev = Consumer.GetAllMessages().First();
			ev.Exception.Should().Contain("exc msg");
		}

		[Test]
		public void Should_log_context_properties()
		{
			ConfigureWithDefault();

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
			ConfigureWithDefault();

			GlobalContext.Properties.Clear();
			ThreadContext.Properties.Clear();
			GlobalContext.Properties["globalContextProperty"] = "value";

			_target.Info("a log");

			Consumer.LogData ev = Consumer.GetAllMessages().First();

			ev.Properties.Should().Have.Count.EqualTo(1);
			ev.Properties.First().Name.Should().Be.EqualTo("globalContextProperty");
			ev.Properties.First().Value.Should().Be.EqualTo("value");
		}

		private void ConfigureWithDefault()
		{
			XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
<log4net>
	<appender name='RabbitMQAppender' type='log4net.Appender.RabbitMQAppender, Log4Rabbit' />
	<root>
		<level value='ALL' />
		<appender-ref ref='RabbitMQAppender' />
	</root>
</log4net>
")));
		}
	}
}