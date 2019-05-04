using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Reset(string accountId);
        void Add(string accountId);
        int Get(string accountId);
        void CheckAccountIsLocked(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            var addFailedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void CheckAccountIsLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")}
                .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}