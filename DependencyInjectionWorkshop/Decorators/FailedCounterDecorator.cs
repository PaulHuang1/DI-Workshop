using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(
            IAuthentication authentication,
            IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            if (_failedCounter.CheckAccountIsLocked(account))
            {
                throw new FailedTooManyTimeException();
            }

            var isValid = base.Verify(account, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(account);
            }
            else
            {
                _failedCounter.Add(account);
            }

            return isValid;
        }
    }
}