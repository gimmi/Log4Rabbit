using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private readonly XmlLayout _xmlLayout;
		private ConnectionFactory _connectionFactory;
		private ConcurrentQueue<byte[]> _queue;
		private Thread _thread;
		private bool _running;

		public RabbitMQAppender()
		{
			_xmlLayout = new XmlLayout();
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
			if(_running)
			{
				_running = false;
				_thread.Join();
			}
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			var sb = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">");
			using(var sr = new StringWriter(sb))
			{
				foreach(LoggingEvent log in loggingEvents)
				{
					_xmlLayout.Format(sr, log);
				}
			}
			sb.Append("</events>");

			_queue.Enqueue(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			Append(new[] { loggingEvent });
		}

		public override void ActivateOptions()
		{
			_connectionFactory = new ConnectionFactory {
				HostName = HostName,
				VirtualHost = VirtualHost,
				UserName = UserName,
				Password = Password,
				RequestedHeartbeat = RequestedHeartbeat,
				Port = Port
			};
			_queue = new ConcurrentQueue<byte[]>();
			_thread = new Thread(Loop) { Name = string.Concat(typeof(RabbitMQAppender).Name, " ", Name) };
			_running = true;
			_thread.Start();
		}

		private void Loop()
		{
			while(_running)
			{
				try
				{
					using(IConnection connection = _connectionFactory.CreateConnection())
					{
						using(IModel model = connection.CreateModel())
						{
							while(_running)
							{
								byte[] body;
								if(_queue.TryPeek(out body))
								{
									IBasicProperties basicProperties = model.CreateBasicProperties();
									basicProperties.ContentEncoding = "utf-8";
									basicProperties.ContentType = "application/xml";
									basicProperties.DeliveryMode = 2;
									model.BasicPublish(Exchange, RoutingKey, basicProperties, body);
									_queue.TryDequeue(out body);
									LogLog.Debug(typeof(RabbitMQAppender), "dequeued");
								}
								else
								{
									Sleep();
								}
							}
						}
					}
				}
				catch
				{
					LogLog.Debug(typeof(RabbitMQAppender), "Exception");
					Sleep();
				}
			}
		}

		private void Sleep()
		{
			for(int i = 0; i < (ReconnectionDelay*1000) && _running; i += 100)
			{
				Thread.Sleep(100);
			}
		}
	}
}