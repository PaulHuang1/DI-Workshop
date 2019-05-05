using DependencyInjectionWorkshop.Adapters;

namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationBaseDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationBaseDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }

    public class LogDecorator : AuthenticationBaseDecorator, IAuthentication
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(
            authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogVerify(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }
}