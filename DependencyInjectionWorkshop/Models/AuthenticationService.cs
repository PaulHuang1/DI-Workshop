using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileRepo
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }

    public class Sha256Adapter
    {
        public string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }

    public class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/otps", accountId).Result;
            string currentOtp;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return currentOtp;
        }
    }

    public class FailedCounter
    {
        public void ResetFailedCounter(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class SlackAdapter
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, "my channel", message, "my bot name");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var passwordFromDb = _profileRepo.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.LogFailedCount($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify($"accountId:{accountId} verify failed");

                return false;
            }
        }

        private static void CheckAccountIsLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}