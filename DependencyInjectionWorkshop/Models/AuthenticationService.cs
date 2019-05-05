using System;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class ApiUserDecorator : AuthenticationBaseDecorator
    {
        private readonly IApiUserQuota _apiUserQuota;

        public ApiUserDecorator(IAuthentication authentication, IApiUserQuota apiUserQuota) : base(authentication)
        {
            _apiUserQuota = apiUserQuota;
        }

        private void AddApiUseTimes(string accountId)
        {
            _apiUserQuota.Add(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            AddApiUseTimes(accountId);

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.GetHash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            var isValid = hashedPassword == passwordFromDb && otp == currentOtp;

            //_apiUserDecorator.AddApiUseTimes(accountId);
            //_notificationDecorator.JoeyIsCute();

            return isValid;
        }
    }

    public interface IApiUserQuota
    {
        void Add(string accountId);
    }

    public class ApiUserQuota : IApiUserQuota
    {
        public void Add(string accountId)
        {
            Console.WriteLine($"ApiUserQuota.Add({accountId})");
        }
    }
}