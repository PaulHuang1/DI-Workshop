using System;
using Autofac;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Decorators;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;

namespace MyConsole
{
    internal class FakeFailedCounter : IFailedCounter
    {
        public void Add(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({account})");
        }

        public bool CheckAccountIsLocked(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(CheckAccountIsLocked)}({account})");
            return false;
        }

        public int Get(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({account})");
            return 91;
        }

        public void Reset(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({account})");
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHash(string password)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHash)}({password})");
            return "my hashed password";
        }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"{nameof(FakeLogger)}.{nameof(Info)}({message})");
        }
    }

    internal class FakeNotification : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine($"{nameof(FakeNotification)}.{nameof(PushMessage)}({message})");
        }
    }

    internal class FakeOtp : IOtp
    {
        public string GetOtp(string account)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetOtp)}({account})");
            return "123456";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string account)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({account})");
            return "my hashed password";
        }
    }

    internal class Program
    {
        private static IContainer _container;

        private static void Main(string[] args)
        {
            //IOtp otp = new FakeOtp();
            //IProfile profile = new FakeProfile();
            //IHash hash = new FakeHash();
            //INotification notification = new FakeNotification();
            //IFailedCounter failedCounter = new FakeFailedCounter();
            //ILogger logger = new FakeLogger();

            //var authenticationService = new AuthenticationService(otp, profile, hash);
            //var notificationDecorator = new NotificationDecorator(authenticationService, notification);
            //var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator, failedCounter);
            //var logDecorator = new LogDecorator(failedCounterDecorator, logger, failedCounter);

            //var authentication = logDecorator;

            RegisterContainer();

            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "pw", "123456");

            Console.WriteLine($"authentication.Verify return {isValid}");
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeNotification>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeLogger>().As<ILogger>();

            //builder.RegisterType<FailedCounterDecorator>().As<IAuthentication>();
            //builder.RegisterType<LogDecorator>().As<IAuthentication>();
            //builder.RegisterType<NotificationDecorator>().As<IAuthentication>();
            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }
}