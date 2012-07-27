using System;

namespace log4net.Appender
{
	public interface IWorker<in T> : IDisposable
	{
		bool Process(T[] items);
	}
}