
namespace CertManager.DnsProviders
{
    interface IDnsProvider
    {
        string AddTxtRecord(string name, string values);
        void RemoveTxtRecord(string recordRef);
    }
}
