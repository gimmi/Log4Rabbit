using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private readonly XmlLayout _xmlLayout = new XmlLayout();
		private readonly ModelHandler _modelHandler = new ModelHandler();

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public string HostName { get; set; }

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public string VirtualHost { get; set; }

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public ushort? RequestedHeartbeat { get; set; }

		/// <summary>
		/// Optional, default to RabbitMQ.Client.ConnectionFactory default
		/// </summary>
		public int? Port { get; set; }

		/// <summary>
		/// Optional, Default to "logs"
		/// </summary>
		public string Exchange { get; set; }

		/// <summary>
		/// Optional, Default to ""
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
			var factory = new ConnectionFactory();
			if(!string.IsNullOrWhiteSpace(HostName))
			{
				factory.HostName = HostName;
			}
			if(!string.IsNullOrWhiteSpace(VirtualHost))
			{
				factory.VirtualHost = VirtualHost;
			}
			if(!string.IsNullOrWhiteSpace(UserName))
			{
				factory.UserName = UserName;
			}
			if(!string.IsNullOrWhiteSpace(Password))
			{
				factory.Password = Password;
			}
			if(RequestedHeartbeat.HasValue)
			{
				factory.RequestedHeartbeat = RequestedHeartbeat.Value;
			}
			if(Port.HasValue)
			{
				factory.Port = Port.Value;
			}
			_modelHandler.ActivateOptions(factory, Exchange ?? "logs", RoutingKey ?? "", ErrorHandler);
		}
	}
}