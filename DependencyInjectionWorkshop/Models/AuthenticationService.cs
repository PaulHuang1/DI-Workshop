using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using NLog;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev") };

            CheckAccountIsLocked(account, httpClient);

            var passwordFromDb = GetPasswordFromDb(account);

            var hashedPassword = GetHashedPassword(password);

            var currentOtp = GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                ResetFailedCount(account, httpClient);

                return true;
            }

            AddFailedCount(account, httpClient);

            var failedCount = GetFailedCount(account, httpClient);

            LogInfo($"account: {account} verify failed! failed count: {failedCount}");

            Notify($"account: {account} verify failed.");

            return false;
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedResponse = httpClient.PostAsJsonAsync("api/FailedCounter/Add", account).Result;
            addFailedResponse.EnsureSuccessStatusCode();
        }

        private static void CheckAccountIsLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/FailedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }

        private static string GetCurrentOtp(string account, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse = httpClient.PostAsJsonAsync("api/FailedCounter/Get", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static string GetHashedPassword(string password)
        {
            string hashedPassword;
            using (var crypt = new SHA256Managed())
            {
                var builder = new StringBuilder();
                var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (var theByte in crypto)
                {
                    builder.Append(theByte.ToString("x2"));
                }

                hashedPassword = builder.ToString();
            }

            return hashedPassword;
        }

        private static string GetPasswordFromDb(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword",
                        new { Account = account },
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

            return passwordFromDb;
        }

        private static void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my token");
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/FailedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}