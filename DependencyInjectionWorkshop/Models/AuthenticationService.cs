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

            var otpApiUri = new Uri("http://joey.com/");
            var httpClient = new HttpClient
            {
                BaseAddress = otpApiUri
            };
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            var currentOtp = response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<string>().Result
                : throw new Exception($"web api error, accountId:{account}");

            if (profilePassword != hashPassword || otp != currentOtp)
            {
                var message = $"account:{account} verify failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
                return false;
            }

            return true;
        }
    }
}