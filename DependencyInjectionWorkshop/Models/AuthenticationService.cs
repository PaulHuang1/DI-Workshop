using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;
        private readonly ILogger _logger;
        private readonly INotification _notification;

        public AuthenticationService(IFailedCounter failedCounter, IProfile profile, IHash hash, IOtp otpService,
            ILogger logger, INotification notification)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _notification = notification;
        }

        //public AuthenticationService()
        //{
        //    _failedCounter = new FailedCounter();
        //    _profile = new ProfileRepo();
        //    _hash = new Sha256Adapter();
        //    _otpService = new OtpService();
        //    _nLogAdapter = new NLogAdapter();
        //    _slackAdapter = new SlackAdapter();
        //}

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

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

                _notification.PushMessage($"accountId:{accountId} verify failed");

                return false;
            }
        }
    }
}