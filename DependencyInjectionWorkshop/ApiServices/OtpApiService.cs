using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.ApiServices
{
    public class OtpApiService : IOtp
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev/") };

        public string GetOtp(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/otps", account).Result;
            var currentOtp = response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<string>().Result
                : throw new Exception($"Get OTP web api error, accountId:{account}");
            return currentOtp;
        }
    }
}