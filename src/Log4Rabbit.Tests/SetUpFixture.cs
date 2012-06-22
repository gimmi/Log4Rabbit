using NUnit.Framework;
using RabbitMQ.Client;
using log4net.Config;

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
			//LogLog.InternalDebugging = true; // For capturing log4net internal logging. See http://logging.apache.org/log4net/release/faq.html
			XmlConfigurator.Configure();

			ConnectionFactory = new ConnectionFactory();
			using (IConnection connection = ConnectionFactory.CreateConnection())
			{
				using (IModel model = connection.CreateModel())
				{
					model.ExchangeDeclare("logs", ExchangeType.Fanout, true);
					model.QueueDeclare(QueueName, true, false, false, null);
					model.QueueBind(QueueName, "logs", "");
				}
			}
		}
	}
}
