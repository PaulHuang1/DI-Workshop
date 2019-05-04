using DependencyInjectionWorkshop.Adapters;

namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogVerify(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }
}