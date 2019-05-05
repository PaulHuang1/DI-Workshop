using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class LogDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(
            IAuthentication authentication,
            ILogger logger,
            IFailedCounter failedCounter) : base(authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);

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