using NLog;

namespace DependencyInjectionWorkshop.Adapters
{
    public class NLoggerAdapter
    {
        public void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}