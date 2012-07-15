using System;
using RabbitMQ.Client;
using log4net.Util;

namespace log4net.Appender
{
	public class ModelHandler
	{
		private readonly RabbitMQAppender _appender;
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _model;

		public ModelHandler(RabbitMQAppender appender)
		{
			_appender = appender;
		}

		public void ActivateOptions(ConnectionFactory connectionFactory)
		{
			_connectionFactory = connectionFactory;
			EnsureConnected();
		}

		public void Publish(string contentEncoding, string contentType, byte[] body)
		{
			EnsureConnected();
			IBasicProperties basicProperties = _model.CreateBasicProperties();
			basicProperties.ContentEncoding = contentEncoding;
			basicProperties.ContentType = contentType;
			basicProperties.DeliveryMode = 2;
			_model.BasicPublish(_appender.Exchange, _appender.RoutingKey, basicProperties, body);
		}

		public void ShutDown()
		{
			if(_model != null)
			{
				_model.Abort();
				_model = null;
			}
			if(_connection != null)
			{
				_connection.Abort(0);
				_connection = null;
			}
		}

		private void EnsureConnected()
		{
			if(_connection == null || _model == null || !_connection.IsOpen || !_model.IsOpen)
			{
				ShutDown();
				_connection = _connectionFactory.CreateConnection();
				_model = _connection.CreateModel();
			}
		}
	}
}