using System;
using System.Threading;
using RabbitMQ.Client;
using log4net.Util;

namespace log4net.Appender
{
	public class RecoverableConnection : IDisposable
	{
		private readonly ConnectionFactory _connectionFactory;
		private readonly int _reconnectionDelay;
		private IConnection _connection;
		private IModel _model;

		private RecoverableConnection(ConnectionFactory connectionFactory, int reconnectionDelay)
		{
			_connectionFactory = connectionFactory;
			_reconnectionDelay = reconnectionDelay;
		}

		public static RecoverableConnection Create(ConnectionFactory connectionFactory, int reconnectionDelaySeconds)
		{
			var ret = new RecoverableConnection(connectionFactory, reconnectionDelaySeconds);
			ret.Connect();
			return ret;
		}

		public void Publish(string exchange, string routingKey, string contentEncoding, string contentType, byte[] body)
		{
			lock(this)
			{
				if(_connection.IsOpen)
				{
					IBasicProperties basicProperties = _model.CreateBasicProperties();
					basicProperties.ContentEncoding = contentEncoding;
					basicProperties.ContentType = contentType;
					basicProperties.DeliveryMode = 2;
					_model.BasicPublish(exchange, routingKey, basicProperties, body);
				}
				else
				{
					LogLog.Debug(typeof(RecoverableConnection), "Connection closed, message lost");
				}
			}
		}

		private void Connect()
		{
			LogLog.Debug(typeof(RecoverableConnection), "Connecting");
			IConnection connection = _connectionFactory.CreateConnection();
			IModel model = connection.CreateModel();
			Replace(connection, model);
			LogLog.Debug(typeof(RecoverableConnection), "Connection established");
		}

		private void OnConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
		{
			LogLog.Debug(typeof(RecoverableConnection), "Retrying to connect");
			new Timer(t => {
				((Timer)t).Dispose();
				lock(this)
				{
					if(!ReferenceEquals(_connection, connection))
					{
						return;
					}
				}
				try
				{
					Connect();
				}
				catch(Exception e)
				{
					LogLog.Debug(typeof(RecoverableConnection), "Failed reconnecting", e);
					OnConnectionShutdown(connection, null);
				}
			}).Change(_reconnectionDelay*1000, Timeout.Infinite);
		}

		public void Dispose()
		{
			Replace(null, null);
		}

		private void Replace(IConnection connection, IModel model)
		{
			lock(this)
			{
				if(_model != null)
				{
					_model.Abort();
				}
				if(_connection != null)
				{
					_connection.ConnectionShutdown -= OnConnectionShutdown;
					_connection.Abort(0);
				}
				_model = model;
				_connection = connection;
				if (_connection != null)
				{
					_connection.ConnectionShutdown += OnConnectionShutdown;
				}
			}
		}
	}
}