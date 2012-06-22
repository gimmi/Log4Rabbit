using RabbitMQ.Client;

namespace log4net.Appender
{
	public class ModelHandler
	{
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _model;

		public void ActivateOptions(ConnectionFactory connectionFactory)
		{
			_connectionFactory = connectionFactory;
		}

		public IModel GetModel()
		{
			if (_model == null || !_model.IsOpen)
			{
				ShutDown();
				_connection = _connectionFactory.CreateConnection();
				_model = _connection.CreateModel();
			}
			return _model;
		}

		public void ShutDown()
		{
			if (_model != null)
			{
				_model.Dispose();
				_model = null;
			}
			if (_connection != null)
			{
				_connection.Dispose();
				_connection = null;
			}
		}
	}
}
