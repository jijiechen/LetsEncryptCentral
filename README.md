LetsEncryptCentral
--------


[![Build status](https://ci.appveyor.com/api/projects/status/3pkc96dqc597i133?svg=true)](https://ci.appveyor.com/project/jijiechen/letsencryptcentral)

LetsEncryptCentral(**lec.exe**) is a utility that helps apply domain validation certificates from the open [Let's encrypt](https://letsencrypt.org/) CA. LetsEncryptCentral uses the [DNS-01 challenge](https://letsencrypt.org/how-it-works/) to prove your ownership of the domain name to Let's Encrypt CA.

LetsEncryptCentral can be used to apply certificates for non-webserver service endpoints, it also can be used as a centralized certificate management tool for requesting and renewing Let's Encrypt certificates.

## Usage
This tool currently only supports Windows operating systems, including server systems. You can set it as a regular task every 80 days in the Windows Task Scheduler to keep your certificates updated.

1. Download the latest binaries from the [releases](https://github.com/jijiechen/LetsEncryptCentral/releases);
1. Create a new registration profile if this is your first running;
1. Apply a certificate using your registration profile (Currently, Azure DNS service and DnsPod DNS service are supported, more DNS service providers are on the way. You can also implement your own DNSProvider type.).

Here are the sample commands:

```powershell
.\lec.exe reg --accept-tos --contact user@domain.com --out-reg reg.json --out-signer signer.key
.\lec.exe apply some.domain.com --out cert.pem --out-type pem --reg reg.json --signer signer.key --dns Azure --dns-conf "client_id=1234;client_secret=822321668a;subscription_id=9837549;zone_name=domain.com"
```

## Development plan
Possible plans are web API/UI and Linux compatibility. Push requests to these features are warmly welcome.

## Contributing and license
Pull requests are welcome. The next step is to support more dynamic DNS providers, AWS Router and Google DNS services. The future plan is wrap the core as a web service to provide out-of-box centralized certificate manageability.

The source code of this project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

