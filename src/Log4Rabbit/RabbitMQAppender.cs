using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;

namespace log4net.Appender
{
	public class RabbitMQAppender : IBulkAppender, IOptionHandler
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

		public string Name { get; set; }

		public void Close()
		{
			_modelHandler.ShutDown();
		}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			DoAppend(new[] {loggingEvent});
		}

		public void DoAppend(LoggingEvent[] logs)
		{
			var model = _modelHandler.GetModel();
			var basicProperties = model.CreateBasicProperties();
			basicProperties.ContentEncoding = "utf-8";
			basicProperties.ContentType = "application/xml";

			var sb = new StringBuilder(@"<?xml version=""1.0"" ?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">");
			using (var sr = new StringWriter(sb))
			{
				foreach (var log in logs)
				{
					_xmlLayout.Format(sr, log);
				}
			}
			sb.Append("</events>");

			model.BasicPublish(Exchange ?? "logs", RoutingKey ?? "", basicProperties, Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public void ActivateOptions()
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
