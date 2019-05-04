using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.ApiServices
{
    public class FailedCounter : IFailedCounter
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://joey.dev/") };

        public void Add(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/Add", account).Result;
            response.EnsureSuccessStatusCode();
        }

        public void CheckAccountIsLocked(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            response.EnsureSuccessStatusCode();
            var isLocked = response.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimeException();
            }
        }

        public int Get(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            response.EnsureSuccessStatusCode();
            var failedCount = response.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void Reset(string account)
        {
            var response = _httpClient
                .PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}