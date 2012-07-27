using System;
using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;

namespace log4net.Appender
{
	public class RabbitWorker : IWorker<LoggingEvent>
	{
		private readonly ConnectionFactory _connectionFactory;
		private readonly string _exchange;
		private readonly string _routingKey;
		private readonly XmlLayout _xmlLayout;
		private IConnection _connection;
		private IModel _model;

		public RabbitWorker(ConnectionFactory connectionFactory, string exchange, string routingKey)
		{
			_connectionFactory = connectionFactory;
			_exchange = exchange;
			_routingKey = routingKey;
			_xmlLayout = new XmlLayout();
			Connect();
		}

		public bool Process(LoggingEvent[] logs)
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
			var body = Encoding.UTF8.GetBytes(sb.ToString());

			try
			{
				Publish(body);
			}
			catch
			{
				try
				{
					Disconnect();
					Connect();
					Publish(body);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}

		private void Publish(byte[] body)
		{
			IBasicProperties basicProperties = _model.CreateBasicProperties();
			basicProperties.ContentEncoding = "utf-8";
			basicProperties.ContentType = "application/xml";
			basicProperties.DeliveryMode = 2;
			_model.BasicPublish(_exchange, _routingKey, basicProperties, body);
		}

		public void Dispose()
		{
			Disconnect();
		}

		public void Connect()
		{
			_connection = _connectionFactory.CreateConnection();
			_model = _connection.CreateModel();
		}

		public void Disconnect()
		{
			_model.Abort();
			_connection.Abort(0);
		}
	}
}