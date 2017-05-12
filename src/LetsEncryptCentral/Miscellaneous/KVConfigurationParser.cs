using System;
using System.Collections.Generic;
using System.Linq;

namespace LetsEncryptCentral
{
    class KVConfigurationParser
    {
        public static Dictionary<string, string> Parse(string configuration, string[] requiredConfKeys = null)
        {
            var conf = configuration
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Split('='))
                        .Select(parts => new KeyValuePair<string, string>(parts[0], parts[1]))
                        .Aggregate(new Dictionary<string, string>(), (dic, item) => { dic[item.Key] = item.Value; return dic; });

            if (requiredConfKeys != null)
            {
                var missingConf = requiredConfKeys.FirstOrDefault(k => !conf.ContainsKey(k));
                if (missingConf != null)
                {
                    throw new DnsProviderMissingConfigurationException(missingConf);
                }
            }

            return conf;
        }
    }
}
