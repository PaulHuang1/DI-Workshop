using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Apis;
using DependencyInjectionWorkshop.Decorators;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccount = "joey";
        private const int DefaultFailedCount = 91;
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultOtp = "123456";
        private const string DefaultPassword = "pw";
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;
        private IProfile _profile;

        [Test]
        public void account_is_locked()
        {
            _failedCounter
                .When(call => call.CheckAccountIsLocked(DefaultAccount))
                .Do(call => throw new FailedTooManyTimesException());

            TestDelegate action = () => WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);

            Assert.Throws<FailedTooManyTimesException>(action);
        }

        [Test]
        public void add_failed_count_when_verify_invalid()
        {
            WhenInvalid();
            ShouldBeAddFailedCount();
        }

        [Test]
        public void is_invalid_when_wrong_otp()
        {
            _profile.GetPassword(DefaultAccount).ReturnsForAnyArgs(DefaultHashedPassword);
            _hash.GetHash(DefaultPassword).ReturnsForAnyArgs(DefaultHashedPassword);
            _otp.GetOtp(DefaultAccount).ReturnsForAnyArgs(DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void is_valid()
        {
            _profile.GetPassword(DefaultAccount).ReturnsForAnyArgs(DefaultHashedPassword);
            _hash.GetHash(DefaultPassword).ReturnsForAnyArgs(DefaultHashedPassword);
            _otp.GetOtp(DefaultAccount).ReturnsForAnyArgs(DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void log_account_failed_count_when_verify_invalid()
        {
            GivenFailedCount();

            WhenInvalid();

            ShouldBeLogAndContains(DefaultAccount, DefaultFailedCount);
        }

        [Test]
        public void notify_user_when_verify_invalid()
        {
            WhenInvalid();
            ShouldBeNotifyUser();
        }

        [Test]
        public void reset_failed_count_when_verify_valid()
        {
            WhenValid();
            ShouldBeResetFailedCount();
        }

        [SetUp]
        public void Setup()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otp = Substitute.For<IOtp>();
            _notification = Substitute.For<INotification>();

            var authenticationService = new AuthenticationService(_failedCounter, _logger, _otp, _profile, _hash);
            var notificationDecorator = new NotificationDecorator(authenticationService, _notification);

            _authentication = notificationDecorator;
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void GivenFailedCount()
        {
            _failedCounter.Get(DefaultAccount).ReturnsForAnyArgs(DefaultFailedCount);
        }

        private void ShouldBeAddFailedCount()
        {
            _failedCounter.Received(1).Add(DefaultAccount);
        }

        private void ShouldBeLogAndContains(string account, int failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(message =>
                message.Contains(account) && message.Contains(failedCount.ToString())));
        }

        private void ShouldBeNotifyUser()
        {
            _notification.Received(1).PushMessage(Arg.Any<string>());
        }

        private void ShouldBeResetFailedCount()
        {
            _failedCounter.Received(1).Reset(DefaultAccount);
        }

        private bool WhenInvalid()
        {
            _profile.GetPassword(DefaultAccount).ReturnsForAnyArgs(DefaultHashedPassword);
            _hash.GetHash(DefaultPassword).ReturnsForAnyArgs(DefaultHashedPassword);
            _otp.GetOtp(DefaultAccount).ReturnsForAnyArgs(DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");
            return isValid;
        }

        private bool WhenValid()
        {
            _profile.GetPassword(DefaultAccount).ReturnsForAnyArgs(DefaultHashedPassword);
            _hash.GetHash(DefaultPassword).ReturnsForAnyArgs(DefaultHashedPassword);
            _otp.GetOtp(DefaultAccount).ReturnsForAnyArgs(DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenVerify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }
}