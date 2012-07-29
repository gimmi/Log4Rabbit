using System;
using System.Diagnostics;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private ConnectionFactory _connectionFactory;
		private XmlMessageBuilder _messageBuilder;
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
			SendInterval = 5;
			MaxQueuedLogs = 10000;
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
		/// Seconds to wait between message send. Default to 5 seconds
		/// </summary>
		public int SendInterval { get; set; }

		/// <summary>
		/// Max number of log queued for sending. Logs exceeding will be discarded. Default to 10.000
		/// </summary>
		public int MaxQueuedLogs { get; set; }

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
			_messageBuilder = new XmlMessageBuilder();
			_messageBuilder.ActivateOptions();
			_connectionFactory = new ConnectionFactory {
				HostName = HostName, 
				VirtualHost = VirtualHost, 
				UserName = UserName, 
				Password = Password, 
				RequestedHeartbeat = RequestedHeartbeat, 
				Port = Port
			};
			_worker = new WorkerThread<LoggingEvent>(string.Concat("Worker for log4net appender '", Name, "'"), TimeSpan.FromSeconds(SendInterval), MaxQueuedLogs, Process);
		}

		public bool Process(LoggingEvent[] logs)
		{
			Stopwatch sw = Stopwatch.StartNew();
			try
			{
				LogLog.Debug(typeof(RabbitMQAppender), string.Concat("publishing ", logs.Length, " logs"));
				byte[] body = _messageBuilder.Build(logs);
				using (IConnection connection = _connectionFactory.CreateConnection())
				{
					using (IModel model = connection.CreateModel())
					{
						IBasicProperties basicProperties = model.CreateBasicProperties();
						basicProperties.ContentEncoding = _messageBuilder.ContentEncoding;
						basicProperties.ContentType = _messageBuilder.ContentType;
						basicProperties.DeliveryMode = 2;
						model.BasicPublish(Exchange, RoutingKey, basicProperties, body);
					}
				}
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