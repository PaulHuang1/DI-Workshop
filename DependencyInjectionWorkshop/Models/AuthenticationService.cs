using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounterApiService _failedCounterApiService = new FailedCounterApiService();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly OtpApiService _otpApiService = new OtpApiService();
        private readonly ProfileRepository _profileRepository = new ProfileRepository();
        private readonly SHA256Adapter _sha256Adapter = new SHA256Adapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string account, string password, string otp)
        {
            _failedCounterApiService.CheckAccountIsLocked(account);

            var passwordFromDb = _profileRepository.GetPasswordFromDb(account);

            var hashPassword = _sha256Adapter.GetHashPassword(password);

            var currentOtp = _otpApiService.GetCurrentOtp(account);

            if (passwordFromDb == hashPassword && otp == currentOtp)
            {
                _failedCounterApiService.ResetFailedCount(account);

                return true;
            }

            _failedCounterApiService.AddFailedCount(account);

            var failedCount = _failedCounterApiService.GetFailedCount(account);
            _nLogAdapter.LogFailedCount($"account:{account} verify failed! Current failed times is {failedCount}");

            _slackAdapter.Notify($"account:{account} verify failed");

            return false;
        }
    }
}