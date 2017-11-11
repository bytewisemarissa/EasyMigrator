using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Utility
{
    public static class ServiceProviderManager
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                if (_serviceProvider == null)
                {
                    _serviceProvider = value;
                }
                else
                {
                    throw new ApplicationException("Can not set the global service provider twitce.");
                }
            }
        }
    }
}
