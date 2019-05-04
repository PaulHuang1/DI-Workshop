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
        [Test]
        public void is_invalid_when_wrong_otp()
        {
            var failedCounter = Substitute.For<IFailedCounter>();
            var logger = Substitute.For<ILogger>();
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otp = Substitute.For<IOtp>();
            var notification = Substitute.For<INotification>();

            var authenticationService =
                new AuthenticationService(failedCounter, logger, otp, profile, hash, notification);

            profile.GetPassword("joey").ReturnsForAnyArgs("my hashed password");
            hash.GetHash("pw").ReturnsForAnyArgs("my hashed password");
            otp.GetOtp("joey").ReturnsForAnyArgs("123456");

            var isValid = authenticationService.Verify("joey", "pw", "wrong otp");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void is_valid()
        {
            var failedCounter = Substitute.For<IFailedCounter>();
            var logger = Substitute.For<ILogger>();
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otp = Substitute.For<IOtp>();
            var notification = Substitute.For<INotification>();

            var authenticationService =
                new AuthenticationService(failedCounter, logger, otp, profile, hash, notification);

            profile.GetPassword("joey").ReturnsForAnyArgs("my hashed password");
            hash.GetHash("pw").ReturnsForAnyArgs("my hashed password");
            otp.GetOtp("joey").ReturnsForAnyArgs("123456");

            var isValid = authenticationService.Verify("joey", "pw", "123456");

            Assert.IsTrue(isValid);
        }
    }
}