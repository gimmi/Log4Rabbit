using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private ConnectionFactory _connectionFactory;
		private XmlLayout _xmlLayout;
		private WorkerThread<LoggingEvent> _worker;

		public RabbitMQAppender()
		{
			HostName = "localhost";
			VirtualHost = "/";
			UserName = "guest";
			Password = "guest";
			RequestedHeartbeat = 0;
			Port = 5672;
			Exchange = "logs";
			RoutingKey = "";
			ReconnectionDelay = 5;
		}

		/// <summary>
		/// Default to "localhost"
		/// </summary>
		public string HostName { get; set; }

		/// <summary>
		/// Default to "/"
		/// </summary>
		public string VirtualHost { get; set; }

		/// <summary>
		/// Default to "guest"
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Default to "guest"
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Value in seconds, default to 0 that mean no heartbeat
		/// </summary>
		public ushort RequestedHeartbeat { get; set; }

		/// <summary>
		/// Default to 5672
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Default to "logs"
		/// </summary>
		public string Exchange { get; set; }

		/// <summary>
		/// Default to ""
		/// </summary>
		public string RoutingKey { get; set; }

		/// <summary>
		/// Seconds to wait between reconnection attempts, if the connection die. Specify 0 to reconnect immediately. Default to 5 seconds
		/// </summary>
		public int ReconnectionDelay { get; set; }

		protected override void OnClose()
		{
			_worker.Dispose();
			_worker = null;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			loggingEvent.Fix = FixFlags.All;
			_worker.Enqueue(loggingEvent);
		}

		public override void ActivateOptions()
		{
			_xmlLayout = new XmlLayout{ Prefix = null };
			_xmlLayout.ActivateOptions();
			_connectionFactory = new ConnectionFactory {
				HostName = HostName, 
				VirtualHost = VirtualHost, 
				UserName = UserName, 
				Password = Password, 
				RequestedHeartbeat = RequestedHeartbeat, 
				Port = Port
			};
			_worker = new WorkerThread<LoggingEvent>(string.Concat("log4net worker for appender '", Name, "'"), TimeSpan.FromSeconds(5), 1000, Process);
		}

		public bool Process(LoggingEvent[] logs)
		{
			Stopwatch sw = Stopwatch.StartNew();
			try
			{
				var sb = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">");
				using (var sr = new StringWriter(sb))
				{
					foreach (LoggingEvent log in logs)
					{
						_xmlLayout.Format(sr, log);
					}
				}
				sb.Append("</events>");
				byte[] body = Encoding.UTF8.GetBytes(sb.ToString());

				LogLog.Debug(typeof(RabbitMQAppender), string.Concat("publishing ", logs.Length, " logs"));

				using (IConnection connection = _connectionFactory.CreateConnection())
				{
					LogLog.Debug(typeof(RabbitMQAppender), string.Concat("connection created ", sw.Elapsed));
					using (IModel model = connection.CreateModel())
					{
						LogLog.Debug(typeof(RabbitMQAppender), string.Concat("model created ", sw.Elapsed));
						IBasicProperties basicProperties = model.CreateBasicProperties();
						basicProperties.ContentEncoding = "utf-8";
						basicProperties.ContentType = _xmlLayout.ContentType;
						basicProperties.DeliveryMode = 2;
						model.BasicPublish(Exchange, RoutingKey, basicProperties, body);
						LogLog.Debug(typeof(RabbitMQAppender), string.Concat("message sent ", sw.Elapsed));
					}
					LogLog.Debug(typeof(RabbitMQAppender), string.Concat("model disposed ", sw.Elapsed));
				}
				LogLog.Debug(typeof(RabbitMQAppender), string.Concat("connection disposed ", sw.Elapsed));
				return true;
			}
			catch (Exception e)
			{
				LogLog.Debug(typeof(RabbitMQAppender), "Exception comunicating with rabbitmq", e);
				return false;
			}
			finally
			{
				LogLog.Debug(typeof(RabbitMQAppender), string.Concat("process completed, took ", sw.Elapsed));
			}
		}
	}
}