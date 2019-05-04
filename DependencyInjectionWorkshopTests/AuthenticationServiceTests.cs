using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
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
        private AuthenticationService _authenticationService;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;
        private IProfile _profile;

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();

            ShouldBeAddFailedCount();
        }

        [Test]
        public void is_invalid_when_wrong_otp()
        {
            GivenPassword(DefaultAccount, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccount, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void log_account_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);

            WhenInvalid();

            LogShouldBeContains(DefaultAccount, DefaultFailedCount);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();

            ShouldBeNotifyUser();
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();

            ShouldBeResetFailedCount();
        }

        [Test]
        public void account_is_locked()
        {
            _failedCounter.CheckAccountIsLocked(DefaultAccount).ReturnsForAnyArgs(true);

            TestDelegate action = () => _authenticationService.Verify(DefaultAccount, DefaultPassword, DefaultOtp);

            Assert.Throws<FailedTooManyTimeException>(action);
        }

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _otp = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();

            _authenticationService = new AuthenticationService(_failedCounter, _logger, _otp, _profile, _hash, _notification);
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.Get(DefaultAccount).ReturnsForAnyArgs(failedCount);
        }

        private void GivenHash(string password, string hashedPassword)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenOtp(string account, string otp)
        {
            _otp.GetOtp(account).ReturnsForAnyArgs(otp);
        }

        private void GivenPassword(string account, string hashedPassword)
        {
            _profile.GetPassword(account).ReturnsForAnyArgs(hashedPassword);
        }

        private void LogShouldBeContains(string account, int failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(account) &&
                                                         m.Contains(failedCount.ToString())));
        }

        private void ShouldBeAddFailedCount()
        {
            _failedCounter.Received(1).Add(DefaultAccount);
        }

        private void ShouldBeNotifyUser()
        {
            _notification.Received(1).PushMessage(Arg.Any<string>());
        }

        private void ShouldBeResetFailedCount()
        {
            _failedCounter.Received(1).Reset(Arg.Any<string>());
        }

        private void WhenInvalid()
        {
            GivenPassword(DefaultAccount, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccount, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenVerify(string account, string password, string otp)
        {
            return _authenticationService.Verify(account, password, otp);
        }
    }
}