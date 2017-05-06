using ACMESharp;
using ACMESharp.ACME;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CertManager
{
    class DnsAuthorizer
    {
        public static void Authorize(AcmeClient client, string hostName, List<string> alternativeNames = null)
        {
            var dnsIdentifiers = new List<string>() { hostName };
            if (alternativeNames != null)
            {
                dnsIdentifiers.AddRange(alternativeNames);
            }

            var authResults = new List<AuthorizationState>();

            foreach (var dnsIdentifier in dnsIdentifiers)
            {
                var authzState = client.AuthorizeIdentifier(dnsIdentifier);
                var challenge = client.DecodeChallenge(authzState, AcmeProtocol.CHALLENGE_TYPE_DNS);

                var dnsChallenge = challenge.Challenge as DnsChallenge;

                // todo: Add the authorization info into DNS

                try
                {
                    authzState.Challenges = new AuthorizeChallenge[] { challenge };
                    client.SubmitChallengeAnswer(authzState, AcmeProtocol.CHALLENGE_TYPE_DNS, true);

                    // have to loop to wait for server to stop being pending.
                    // todo: put timeout/retry limit in this loop
                    while (authzState.Status == "pending")
                    {
                        Thread.Sleep(4000); // this has to be here to give ACME server a chance to think
                        var newAuthzState = client.RefreshIdentifierAuthorization(authzState);
                        if (newAuthzState.Status != "pending")
                            authzState = newAuthzState;
                    }

                    authResults.Add(authzState);
                }
                finally
                {
                    // todo:Remove the authorization info from DNS
                }
            }

            if (authResults.Any(result => result.Status != "valid"))
            {
                throw new AuthorizationFailedException(authResults);
            }
        }

    }
}
