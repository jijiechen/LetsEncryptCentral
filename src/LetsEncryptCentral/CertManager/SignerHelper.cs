using ACMESharp.JOSE;
using System.IO;

namespace LetsEncryptCentral.CertManager
{
    class SignerHelper
    {
        public static RS256Signer LoadFromFile(string signerPath)
        {
            var signer = new RS256Signer();
            signer.Init();

            using (var signerStream = File.OpenRead(signerPath))
                signer.Load(signerStream);

            return signer;
        }


        public static void SaveToFile(RS256Signer signer, string signerPath)
        {
            using (var signerStream = File.OpenWrite(signerPath))
                signer.Save(signerStream);
        }
    }
}
