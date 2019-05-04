using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Apis
{
    public class FailedCounterApi
    {
        public void AddFailedCount(string account)
        {
            var addFailedResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Add", account).Result;
            addFailedResponse.EnsureSuccessStatusCode();
        }

        public void CheckAccountIsLocked(string account)
        {
            var isLockedResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Get", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void ResetFailedCount(string account)
        {
            var resetResponse = new HttpClient { BaseAddress = new Uri("http://joey.dev") }.PostAsJsonAsync("api/FailedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }
}