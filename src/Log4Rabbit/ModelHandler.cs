using System;
using RabbitMQ.Client;
using log4net.Core;

namespace log4net.Appender
{
	public class ModelHandler
	{
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _model;
		private string _exchange;
		private string _routingKey;
		private IErrorHandler _errorHandler;

		public void ActivateOptions(ConnectionFactory connectionFactory, string exchange, string routingKey, IErrorHandler errorHandler)
		{
			_errorHandler = errorHandler;
			_routingKey = routingKey;
			_exchange = exchange;
			_connectionFactory = connectionFactory;
			Connect();
		}

		public void Publish(string contentEncoding, string contentType, byte[] body)
		{
			try
			{
				InternalPublish(contentEncoding, contentType, body);
			}
			catch(Exception e)
			{
				_errorHandler.Error("Cannot publish log. Will try to recover", e);
				Connect();
				InternalPublish(contentEncoding, contentType, body);
			}
		}

		private void InternalPublish(string contentEncoding, string contentType, byte[] body)
		{
			IBasicProperties basicProperties = _model.CreateBasicProperties();
			basicProperties.ContentEncoding = contentEncoding;
			basicProperties.ContentType = contentType;
			basicProperties.DeliveryMode = 2;
			_model.BasicPublish(_exchange, _routingKey, basicProperties, body);
		}

		private void Connect()
		{
			ShutDown();
			_connection = _connectionFactory.CreateConnection();
			_model = _connection.CreateModel();
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
	}
}
