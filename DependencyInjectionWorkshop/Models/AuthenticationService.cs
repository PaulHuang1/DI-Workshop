using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        private void NotificationVerify(string accountId)
        {
            _notification.PushMessage($"accountId:{accountId} verify failed");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                NotificationVerify(accountId);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;
        private readonly ILogger _logger;

        public AuthenticationService(IFailedCounter failedCounter, IProfile profile, IHash hash, IOtp otpService, ILogger logger)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.GetHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");

                return false;
            }
        }
    }
}