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
        public bool Verify(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);

            var passwordFromDb = GetPasswordFromDb(account);

            var hashPassword = GetHashPassword(password);

            var currentOtp = GetCurrentOtp(account);

            if (passwordFromDb == hashPassword && otp == currentOtp)
            {
                ResetFailedCount(account);

                return true;
            }

            AddFailedCount(account);

            var failedCount = GetFailedCount(account);

            LogFailedCount($"account:{account} verify failed! Current failed times is {failedCount}");

            Notify($"account:{account} verify failed");

            return false;
        }

        private static void AddFailedCount(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            response.EnsureSuccessStatusCode();
        }

        private static void CheckAccountIsLocked(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            response.EnsureSuccessStatusCode();
            var isLocked = response.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimeException();
            }
        }

        private static string GetCurrentOtp(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/otps", account).Result;
            var currentOtp = response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<string>().Result
                : throw new Exception($"Get OTP web api error, accountId:{account}");
            return currentOtp;
        }

        private static int GetFailedCount(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            response.EnsureSuccessStatusCode();
            var failedCount = response.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static string GetHashPassword(string password)
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

        private static string GetPasswordFromDb(string account)
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

        private static void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
        }

        private static void ResetFailedCount(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimeException : Exception
    {
    }
}