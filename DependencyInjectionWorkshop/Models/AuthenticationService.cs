using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Repositories;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

        public AuthenticationService(
            IOtp otp,
            IProfile profile,
            IHash hash)
        {
            _otp = otp;
            _profile = profile;
            _hash = hash;
        }

        public bool Verify(string account, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(account);

            var hashPassword = _hash.GetHash(password);

            var currentOtp = _otp.GetOtp(account);

            return passwordFromDb == hashPassword && otp == currentOtp;
        }
    }
}