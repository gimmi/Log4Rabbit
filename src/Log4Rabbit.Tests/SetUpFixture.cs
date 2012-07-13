using NUnit.Framework;
using RabbitMQ.Client;

namespace log4net.Appender
{
	[SetUpFixture]
	public class SetUpFixture
	{
		public const string QueueName = "logs";
		public static ConnectionFactory ConnectionFactory;

		[SetUp]
		public void SetUp()
		{
			Util.LogLog.InternalDebugging = true;

			ConnectionFactory = new ConnectionFactory();
			using(IConnection connection = ConnectionFactory.CreateConnection())
			{
				using(IModel model = connection.CreateModel())
				{
					model.ExchangeDeclare("logs", ExchangeType.Fanout, true);
					model.QueueDeclare(QueueName, true, false, false, null);
					model.QueueBind(QueueName, "logs", "");
				}
			}
		}
	}
}