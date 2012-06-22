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
		/// Default to 60 seconds
		/// </summary>
		public ushort? RequestedHeartbeat { get; set; }

		/// <summary>
		/// Default to 5672
		/// </summary>
		public int? Port { get; set; }

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
			IModel model = _modelHandler.GetModel();
			IBasicProperties basicProperties = model.CreateBasicProperties();
			basicProperties.ContentEncoding = "utf-8";
			basicProperties.ContentType = "application/xml";

			var sb = new StringBuilder(@"<?xml version=""1.0"" ?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">");
			using(var sr = new StringWriter(sb))
			{
				foreach(LoggingEvent log in loggingEvents)
				{
					_xmlLayout.Format(sr, log);
				}
			}
			sb.Append("</events>");

			model.BasicPublish(Exchange ?? "logs", RoutingKey ?? "", basicProperties, Encoding.UTF8.GetBytes(sb.ToString()));
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			Append(new[] { loggingEvent });
		}

		public override void ActivateOptions()
		{
			_modelHandler.ActivateOptions(new ConnectionFactory {
				HostName = HostName ?? "localhost",
				VirtualHost = VirtualHost ?? "/",
				UserName = UserName ?? "guest",
				Password = Password ?? "guest",
				RequestedHeartbeat = RequestedHeartbeat ?? 60,
				Port = Port ?? 5672
			});
		}
	}
}