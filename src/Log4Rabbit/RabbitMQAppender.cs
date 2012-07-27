using System;
using System.IO;
using System.Text;
using RabbitMQ.Client;
using log4net.Core;
using log4net.Layout;

namespace log4net.Appender
{
	public class RabbitMQAppender : AppenderSkeleton
	{
		private WorkerThread<LoggingEvent> _worker; 

		public RabbitMQAppender()
		{
			HostName = "localhost";
			VirtualHost = "/";
			UserName = "guest";
			Password = "guest";
			RequestedHeartbeat = 0;
			Port = 5672;
			Exchange = "logs";
			RoutingKey = "";
			ReconnectionDelay = 5;
		}

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

		/// <summary>
		/// Seconds to wait between reconnection attempts, if the connection die. Specify 0 to reconnect immediately. Default to 5 seconds
		/// </summary>
		public int ReconnectionDelay { get; set; }

		protected override void OnClose()
		{
			_worker.Dispose();
			_worker = null;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			loggingEvent.Fix = FixFlags.All;
			_worker.Enqueue(loggingEvent);
		}

		public override void ActivateOptions()
		{
			var connectionFactory = new ConnectionFactory {
				HostName = HostName, 
				VirtualHost = VirtualHost, 
				UserName = UserName, 
				Password = Password, 
				RequestedHeartbeat = RequestedHeartbeat, 
				Port = Port
			};
			var worker = new RabbitWorker(connectionFactory, Exchange, RoutingKey);
			_worker = new WorkerThread<LoggingEvent>(string.Concat(GetType().Name, " ", Name), TimeSpan.FromSeconds(5), 1000, worker);
		}
	}
}