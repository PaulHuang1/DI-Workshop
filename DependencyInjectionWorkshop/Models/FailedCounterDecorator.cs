using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger, IOtp otp) :
            base(authentication)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otp = otp;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        private readonly ILogger _logger;
        private readonly IOtp _otp;

        public override bool Verify(string accountId, string password, string otp)
        {
            var currentOtp = _otp.GetCurrentOtp(accountId);
            _logger.Info($"joey otp is {currentOtp}");
            CheckAccountIsLocked(accountId);
            var isValid = base.Verify(accountId, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(accountId);
            }
            else
            {
                _failedCounter.Add(accountId);
            }

            return isValid;
        }
    }
}