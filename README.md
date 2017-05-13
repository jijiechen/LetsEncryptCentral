LetsEncryptCentral
--------

LetsEncryptCentral(**lec.exe**) is a utility that helps apply domain validation certificates from the open [Let's encrypt](https://letsencrypt.org/) CA. LetsEncryptCentral uses the [DNS-01 challenge](https://letsencrypt.org/how-it-works/) to prove your ownership of the domain name to Let's Encrypt CA.

LetsEncryptCentral can be used to apply certificates for non-webserver service endpoints, it also can be used as a centralized certificate management tool for requesting and renewing Let's Encrypt certificates.

## Usage
This project is in early development, hence not ready for production usage. Use at your own risk.

1. Clone this project, run the `.\Build.bat` to compile;
1. Run the **lec.exe** from the `release` output directory;
1. Create a new registration profile if this is your first running;
1. Apply a certificate using your registration profile (Currently, only DnsPod dynamic DNS service is supported, more DNS service provider is on the way).

Here are the sample commands:

```powershell
cd release
.\lec.exe reg --contact user@example.com --out-reg C:\Users\MyUserName\Desktop\reg.json --out-signer C:\Users\MyUserName\Desktop\signer.key
.\lec.exe apply name.ciznx.com -out C:\Users\MyUserName\Desktop\cert.pfx --out-type pfx --reg C:\Users\MyUserName\Desktop\reg.json --signer C:\Users\MyUserName\Desktop\signer.key --dns DnsPod --dns-conf "token_id=1234;token=822321668afcefe;domain=example.com"
```

## Contributing and license
Pull requests are welcome. The next step is to support more dynamic DNS providers, such as Azure DNS, AWS Router and Google DNS services. The future plan is wrap the core as a web service to provide out-of-box centralized certificate manageability.

The source code of this project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

