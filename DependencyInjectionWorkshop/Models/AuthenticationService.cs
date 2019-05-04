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
            var httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev") };

            var isLockedResponse = httpClient.PostAsJsonAsync("api/FailedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword",
                        new { Account = account },
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

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

            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/FailedCounter/Reset", account).Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }

            var addFailedResponse = httpClient.PostAsJsonAsync("api/FailedCounter/Add", account).Result;
            addFailedResponse.EnsureSuccessStatusCode();

            var slackClient = new SlackClient("my token");
            var message = $"account: {account} verify failed.";
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}