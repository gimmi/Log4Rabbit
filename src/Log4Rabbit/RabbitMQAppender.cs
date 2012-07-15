using System;
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
		private readonly XmlLayout _xmlLayout = new XmlLayout();
		private readonly ModelHandler _modelHandler = new ModelHandler();

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

		protected override void OnClose()
		{
			_modelHandler.ShutDown();
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

			_modelHandler.Publish("utf-8", "application/xml", Encoding.UTF8.GetBytes(sb.ToString()));
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			Append(new[] { loggingEvent });
		}

		public override void ActivateOptions()
		{
			var factory = new ConnectionFactory {
				HostName = HostName ?? "localhost", 
				VirtualHost = VirtualHost ?? "/", 
				UserName = UserName ?? "guest", 
				Password = Password ?? "guest",
				RequestedHeartbeat = (RequestedHeartbeat == default(ushort) ? (ushort)0 : RequestedHeartbeat),
				Port = (Port == default(int) ? 5672 : Port)
			};
			_modelHandler.ActivateOptions(factory, Exchange ?? "logs", RoutingKey ?? "");
		}
	}
}