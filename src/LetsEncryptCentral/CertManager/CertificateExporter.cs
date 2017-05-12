using ACMESharp.PKI;
using System.IO;

namespace LetsEncryptCentral.CertManager
{
    class CertificateExporter
    {
        public static void Export(CertificateProvider certProvider, IssuedCertificate certificate, CertOutputType outType, string filePath)
        {
            switch (outType)
            {
                case CertOutputType.Pfx:
                    ExportPfx(certProvider, certificate, filePath);
                    break;
                case CertOutputType.Pem:
                    ExportPem(certProvider, certificate, filePath);
                    break;
            }
        }

        static void ExportPfx(CertificateProvider certProvider, IssuedCertificate certificate, string filePath)
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

        static void ExportPem(CertificateProvider certProvider, IssuedCertificate certificate, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                certProvider.ExportCertificate(certificate.PublicKey, EncodingFormat.PEM, fileStream);
                certProvider.ExportPrivateKey(certificate.PrivateKey, EncodingFormat.PEM, fileStream);
            }
        }

    }


    enum CertOutputType
    {
        Pfx,
        Pem
    }
}
