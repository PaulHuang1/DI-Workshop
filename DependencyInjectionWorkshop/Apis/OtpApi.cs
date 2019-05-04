using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Apis
{
    public class OtpApi
    {
        public string GetCurrentOtp(string account)
        {
            var response = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/otps", account).Result;
            response.EnsureSuccessStatusCode();
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}