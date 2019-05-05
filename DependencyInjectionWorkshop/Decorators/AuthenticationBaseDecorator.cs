using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public abstract class AuthenticationBaseDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationBaseDecorator(
            IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }
}