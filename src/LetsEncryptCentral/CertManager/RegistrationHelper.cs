using ACMESharp;
using ACMESharp.JOSE;
using System.IO;

namespace LetsEncryptCentral.CertManager
{
    class RegistrationHelper
    {
        public static AcmeRegistration CreateNew(AcmeClient client, string contactEmail)
        {
            var contacts = new[] { "mailto:" + contactEmail };
            var registration = client.Register(contacts);
            client.UpdateRegistration(true, true);

            return registration;
        }

        public static AcmeRegistration LoadFromFile(string filePath)
        {
            using (var registrationStream = File.OpenRead(filePath))
                return AcmeRegistration.Load(registrationStream);
        }

        public static void SaveToFile(AcmeRegistration registration, string filePath)
        {
            using (var registrationStream = File.OpenWrite(filePath))
                registration.Save(registrationStream);
        }

    }
}
