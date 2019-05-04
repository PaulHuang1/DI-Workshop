using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class NotificationDecorator: IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(
            IAuthentication authentication,
            INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = _authentication.Verify(account, password, otp);

            if (!isValid)
            {
                _notification.PushMessage($"account:{account} verify failed");
            }

            return isValid;
        }
    }
}