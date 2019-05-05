using System;

namespace DependencyInjectionWorkshop.ApiServices
{
    public class ApiUserQuota : IApiUserQuota
    {
        public void AddUseTimes(string account)
        {
            Console.WriteLine($"AddUseTimes({account})");
        }
    }
}