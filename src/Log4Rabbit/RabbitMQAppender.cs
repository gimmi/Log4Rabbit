using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private readonly XmlLayout _xmlLayout;
		private RecoverableConnection _connection;

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
			_connection.Dispose();
			_connection = null;
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			var sb = new StringBuilder(@"<?xml version=""1.0"" ?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">");
			using(var sr = new StringWriter(sb))
			{
				foreach(LoggingEvent log in loggingEvents)
				{
					_xmlLayout.Format(sr, log);
				}
			}
			sb.Append("</events>");

			_connection.Publish(Exchange, RoutingKey, "utf-8", "application/xml", Encoding.UTF8.GetBytes(sb.ToString()));
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			Append(new[] { loggingEvent });
		}

		public override void ActivateOptions()
		{
			_connection = RecoverableConnection.Create(new ConnectionFactory {
				HostName = HostName,
				VirtualHost = VirtualHost,
				UserName = UserName,
				Password = Password,
				RequestedHeartbeat = RequestedHeartbeat,
				Port = Port
			}, ReconnectionDelay);
		}
	}
}