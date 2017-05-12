using System;

namespace CertManager.DnsProviders
{
    class DnsProviderInitializationException: Exception
    {
        public DnsProviderInitializationException(string message) : base(message) { }
    }

    class DnsProviderMissingConfigurationException : DnsProviderInitializationException
    {
        public DnsProviderMissingConfigurationException(string confKey)  
            : base($"The required configuration {confKey} is missing.")
        {

        }
    }
}
