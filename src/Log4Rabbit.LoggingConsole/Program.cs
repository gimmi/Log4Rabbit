using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Util;

namespace Log4Rabbit.LoggingConsole
{
	public class Program
	{
		private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(100);
		private static int _count;

		public static void Main()
		{
			LogLog.InternalDebugging = true;
			XmlConfigurator.Configure();

			ILog log = LogManager.GetLogger(typeof(Program));
			while (!Console.KeyAvailable)
			{
				log.Info(++_count);
				Thread.Sleep(Interval);
			}

			LogManager.Shutdown();
		}
	}
}