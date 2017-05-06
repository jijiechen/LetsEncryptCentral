
namespace CertManager.DnsProviders
{
    interface IDnsProvider
    {
        void AddTxtRecord(string name, string[] values);
        void RemoveTxtRecord(string name);
    }
}
