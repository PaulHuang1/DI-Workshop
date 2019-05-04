using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Repositories;
using NLog;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepository _profileRepository = new ProfileRepository();

        public bool Verify(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);

            var passwordFromDb = _profileRepository.GetPasswordFromDb(account);

            var hashedPassword = GetHashedPassword(password);

            var currentOtp = GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                ResetFailedCount(account);

                return true;
            }

            AddFailedCount(account);

            var failedCount = GetFailedCount(account);

            LogInfo($"account: {account} verify failed! failed count: {failedCount}");

            Notify($"account: {account} verify failed.");

            return false;
        }

        private static void AddFailedCount(string account)
        {
            var addFailedResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Add", account).Result;
            addFailedResponse.EnsureSuccessStatusCode();
        }

        private static void CheckAccountIsLocked(string account)
        {
            var isLockedResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }

        private static string GetCurrentOtp(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/otps", account).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static int GetFailedCount(string account)
        {
            var failedCountResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Get", account).Result;
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

        private static void ResetFailedCount(string account)
        {
            var resetResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }
}