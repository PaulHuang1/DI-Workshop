﻿using DependencyInjectionWorkshop.ApiServices;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorators
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(
            IAuthentication authentication,
            IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string account, string password, string otp)
        {
            if (_failedCounter.CheckAccountIsLocked(account))
            {
                throw new FailedTooManyTimeException();
            }

            var isValid = _authentication.Verify(account, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(account);
            }
            else
            {
                _failedCounter.Add(account);
            }

            return isValid;
        }
    }
}