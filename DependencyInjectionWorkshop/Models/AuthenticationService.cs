using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly INotification _notification;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

        public AuthenticationService(
            IFailedCounter failedCounter,
            ILogger logger,
            IOtp otp,
            IProfile profile,
            IHash hash,
            INotification notification)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otp = otp;
            _profile = profile;
            _hash = hash;
            _notification = notification;
        }

        //public AuthenticationService()
        //{
        //    _failedCounter = new FailedCounter();
        //    _profile = new ProfileRepository();
        //    _hash = new SHA256Adapter();
        //    _otp = new OtpApiService();
        //    _logger = new NLogAdapter();
        //    _notification = new SlackAdapter();
        //}

        public bool Verify(string account, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(account);

            var passwordFromDb = _profile.GetPassword(account);

            var hashPassword = _hash.GetHash(password);

            var currentOtp = _otp.GetOtp(account);

            if (passwordFromDb == hashPassword && otp == currentOtp)
            {
                _failedCounter.Reset(account);

                return true;
            }

            _failedCounter.Add(account);

            var failedCount = _failedCounter.Get(account);
            _logger.Info($"account:{account} verify failed! Current failed times is {failedCount}");

            _notification.PushMessage($"account:{account} verify failed");

            return false;
        }
    }
}