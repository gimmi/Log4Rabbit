using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace log4net.Appender
{
	public class Consumer
	{
		private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(Body));

		public static IEnumerable<LogData> GetAllMessages()
		{
			using (IConnection connection = SetUpFixture.ConnectionFactory.CreateConnection())
			{
				using (IModel model = connection.CreateModel())
				{
					var subscription = new Subscription(model, SetUpFixture.QueueName);
					BasicDeliverEventArgs basicDeliveryEventArgs;
					while (subscription.Next(1000, out basicDeliveryEventArgs))
					{
						var logs = (Body) XmlSerializer.Deserialize(new MemoryStream(basicDeliveryEventArgs.Body));
						foreach (var entry in logs.LogDatas)
						{
							yield return entry;
						}
					}
				}
			}
		}

		public class LogData
		{
			[XmlAttribute("domain")]
			public string Domain;

			[XmlAttribute("level")]
			public string Level;

			[XmlAttribute("logger")]
			public string Logger;

			[XmlAttribute("thread")]
			public string Thread;

			[XmlAttribute("timestamp")]
			public DateTime TimeStamp;

			[XmlAttribute("username")]
			public string UserName;

			[XmlElement("message")]
			public string Message;

			[XmlElement("exception")]
			public string Exception;

			[XmlArray("properties")]
			[XmlArrayItem("data")]
			public LogProperty[] Properties;
		}

		public class LogProperty
		{
			[XmlAttribute("name")]
			public string Name;
			
			[XmlAttribute("value")]
			public string Value;
		}

		[XmlRoot("events", Namespace = "http://logging.apache.org/log4net/schemas/log4net-events-1.2")]
		public class Body
		{
			[XmlElement("event")]
			public LogData[] LogDatas { get; set; }
		}
	}
}
