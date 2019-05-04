using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Apis;
using DependencyInjectionWorkshop.Repositories;
using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounterApi _failedCounterApi = new FailedCounterApi();
        private readonly OtpApi _otpApi = new OtpApi();
        private readonly ProfileRepository _profileRepository = new ProfileRepository();
        private readonly SHA256Adapter _sha256Adapter = new SHA256Adapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string account, string password, string otp)
        {
            _failedCounterApi.CheckAccountIsLocked(account);

            var passwordFromDb = _profileRepository.GetPasswordFromDb(account);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpApi.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounterApi.ResetFailedCount(account);

                return true;
            }

            _failedCounterApi.AddFailedCount(account);

            var failedCount = _failedCounterApi.GetFailedCount(account);

            LogInfo($"account: {account} verify failed! failed count: {failedCount}");

            _slackAdapter.Notify($"account: {account} verify failed.");

            return false;
        }

        private static void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}