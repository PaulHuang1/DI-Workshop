using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Apis
{
    public class OtpApi
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev") };

        public string GetCurrentOtp(string account)
        {
            var response = _httpClient.PostAsJsonAsync("api/otps", account).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}