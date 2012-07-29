using System;

namespace log4net.Appender
{
	public interface IWorker<in T>
	{
		bool Process(T[] logs);
	}
}