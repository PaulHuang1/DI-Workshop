using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using SlackAPI;

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

    public class FailedCounterApiService
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev/") };

        public void AddFailedCount(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/Add", account).Result;
            response.EnsureSuccessStatusCode();
        }

        public void CheckAccountIsLocked(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            response.EnsureSuccessStatusCode();
            var isLocked = response.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimeException();
            }
        }

        public int GetFailedCount(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            response.EnsureSuccessStatusCode();
            var failedCount = response.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void ResetFailedCount(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimeException : Exception
    {
    }

    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class OtpApiService
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev/") };

        public string GetCurrentOtp(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/otps", account).Result;
            var currentOtp = response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<string>().Result
                : throw new Exception($"Get OTP web api error, accountId:{account}");
            return currentOtp;
        }
    }

    public class ProfileRepository
    {
        public string GetPasswordFromDb(string account)
        {
            string profilePassword;
            var connectionString = "my connection string";
            var spName = "spGetUserPassword";
            using (var connection = new SqlConnection(connectionString))
            {
                profilePassword = connection.Query<string>(spName,
                    new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return profilePassword;
        }
    }

    public class SHA256Adapter
    {
        public string GetHashPassword(string password)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByes in crypto)
            {
                hash.Append(theByes.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }
    }

    public class SlackAdapter
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
        }
    }
}