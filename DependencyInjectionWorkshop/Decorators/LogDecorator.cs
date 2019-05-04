using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class LogDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(
            IAuthentication authentication,
            ILogger logger,
            IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = _authentication.Verify(account, password, otp);

            if (isValid)
            {
                return true;
            }

            var failedCount = _failedCounter.Get(account);
            _logger.Info($"account:{account} verify failed! Current failed times is {failedCount}");
            return false;
        }
    }
}