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
            var otpApiUri = new Uri("http://joey.dev/");
            var httpClient = new HttpClient
            {
                BaseAddress = otpApiUri
            };
            var response = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            response.EnsureSuccessStatusCode();
            var isLocked = response.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimeException();
            }

            string profilePassword;
            var connectionString = "my connection string";
            var spName = "spGetUserPassword";
            using (var connection = new SqlConnection(connectionString))
            {
                profilePassword = connection.Query<string>(spName,
                    new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByes in crypto)
            {
                hash.Append(theByes.ToString("x2"));
            }
            var hashPassword = hash.ToString();

            response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            var currentOtp = response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<string>().Result
                : throw new Exception($"Get OTP web api error, accountId:{account}");

            if (profilePassword == hashPassword && otp == currentOtp)
            {
                response = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
                response.EnsureSuccessStatusCode();

                return true;
            }

            var message = $"account:{account} verify failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");

            response = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            response.EnsureSuccessStatusCode();

            response = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            response.EnsureSuccessStatusCode();
            var failedCount = response.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"account:{account} verify failed! Current failed times is {failedCount}");

            return false;
        }
    }

    public class FailedTooManyTimeException : Exception
    {
    }
}