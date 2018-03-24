﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Olive
{
    public class Context
    {
        static Context current;
        public IServiceProvider ServiceProvider { get; private set; }
        public readonly IServiceCollection Services;

        public static Context Current => current
            ?? throw new InvalidOperationException("Olive.Context is not initialized!");

        Context(IServiceCollection services) => Services = services;

        public static void Initialize(IServiceCollection services) => current = new Context(services);

        public Context Configure(IServiceProvider provider)
        {
            ServiceProvider = provider;
            return this;
        }

        public TService GetService<TService>() => ServiceProvider.GetRequiredService<TService>();

        public TService GetOptionalService<TService>() where TService : class
        {
            var result = Current.ServiceProvider.GetService<TService>();
            if (result == null)
                Debug.WriteLine(typeof(TService).FullName + " service is not configured.");
            return result;
        }

        public IEnumerable<TService> GetServices<TService>() => Current.ServiceProvider.GetServices<TService>();

        public void AddService(Type serviceType, object serviceInstance)
            => Services.AddSingleton(serviceType, serviceInstance);
    }
}