using ACMESharp.PKI;
using System.IO;

namespace CertManager
{
    class CertificateExporter
    {
        public static void Export(CertificateProvider certProvider, IssuedCertificate certificate, CertOutputType outType, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                certProvider.ExportArchive(
                    certificate.PrivateKey,
                    new[] { certificate.PublicKey, certificate.CAPublicKey },
                    ArchiveFormat.PKCS12,
                    fileStream);
            }
        }

    }


    enum CertOutputType
    {
        Pfx,
        Pem
    }
}
