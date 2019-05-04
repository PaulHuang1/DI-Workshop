using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Apis;
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

        [SetUp]
        public void Setup()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otp = Substitute.For<IOtp>();
            _notification = Substitute.For<INotification>();

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
            return _authenticationService.Verify(account, password, otp);
        }
    }
}