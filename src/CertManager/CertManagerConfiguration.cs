namespace CertManager
{
    class CertManagerConfiguration
    {
        public string AcmeServerBaseUri { get; set; } = "https://acme-staging.api.letsencrypt.org/";
        public string ProxyUri { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }

        public short RSAKeyBits { get; set; } = 4096;
    }
}