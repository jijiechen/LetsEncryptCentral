using System;

namespace CertManager.DnsProviders
{
    class DnsPodProvider : IDnsProvider
    {
        public DnsPodProvider(string login, string apiKey, string domainName)
        {

        }

        public void AddTxtRecord(string name, string[] values)
        {
            throw new NotImplementedException();
        }

        public void RemoveTxtRecord(string name)
        {
            throw new NotImplementedException();
        }
    }
}
