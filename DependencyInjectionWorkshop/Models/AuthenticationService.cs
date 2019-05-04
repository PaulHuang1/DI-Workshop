using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

        public AuthenticationService(
            IFailedCounter failedCounter,
            ILogger logger,
            IOtp otp,
            IProfile profile,
            IHash hash)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otp = otp;
            _profile = profile;
            _hash = hash;
        }

        public bool Verify(string account, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(account);

            var hashPassword = _hash.GetHash(password);

            var currentOtp = _otp.GetOtp(account);

            if (passwordFromDb == hashPassword && otp == currentOtp)
            {
                return true;
            }

            var failedCount = _failedCounter.Get(account);
            _logger.Info($"account:{account} verify failed! Current failed times is {failedCount}");

            return false;
        }
    }
}