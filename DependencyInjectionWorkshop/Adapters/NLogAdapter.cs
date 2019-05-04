using System;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class NLogAdapter : ILogger
    {
        public virtual void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }


    public class ConsoleAdapter : NLogAdapter
    {
        public override void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}