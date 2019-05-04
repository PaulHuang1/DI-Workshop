using DependencyInjectionWorkshop.Apis;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(
            IAuthentication authentication,
            IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string account, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(account);

            var isValid = _authentication.Verify(account, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(account);
            }

            _failedCounter.Add(account);
            return isValid;
        }
    }
}