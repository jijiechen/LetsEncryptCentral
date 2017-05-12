﻿
using System;

namespace LetsEncryptCentral.DnsProviders
{
    interface IDnsProvider: IDisposable
    {
        void Initialize(string configuration);
        string AddTxtRecord(string name, string values);
        void RemoveTxtRecord(string recordRef);
    }
}