using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Decorators;

namespace DependencyInjectionWorkshop.Models
{
    public class ApiUserDecorator : AuthenticationBaseDecorator
    {
        private readonly IApiUserQuota _apiUserQuota;

        public ApiUserDecorator(IAuthentication authentication, IApiUserQuota apiUserQuota)
            : base(authentication)
        {
            _apiUserQuota = apiUserQuota;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);

            _apiUserQuota.AddUseTimes(account);

            return isValid;
        }
    }
}