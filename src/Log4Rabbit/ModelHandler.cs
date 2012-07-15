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

		public void ActivateOptions(ConnectionFactory connectionFactory, string exchange, string routingKey)
		{
			_routingKey = routingKey;
			_exchange = exchange;
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
			_model.BasicPublish(_exchange, _routingKey, basicProperties, body);
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

		public void EnsureConnected()
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