using NLog;

namespace DependencyInjectionWorkshop.Adapters
{
    public class NLoggerAdapter : ILogger
    {
        public void Info(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}