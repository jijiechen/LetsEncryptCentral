using System;
using System.Collections.Generic;
using System.Linq;

namespace LetsEncryptCentral
{
    class KVConfigurationParser
    {
        public static Dictionary<string, string> Parse(string configuration, string[] requiredConfKeys = null)
        {
            var semicolonReplacement = string.Format("${0}$", Guid.NewGuid().ToString("N").Substring(28));
            var equalSignReplacement = string.Format("${0}$", Guid.NewGuid().ToString("N").Substring(28));

            var conf = configuration
                        .Replace(";;", semicolonReplacement)
                        .Replace("==", equalSignReplacement)
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Replace(semicolonReplacement, ";").Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(parts => parts.Length > 1)
                        .Select(parts => new KeyValuePair<string, string>(parts[0], parts[1].Replace(equalSignReplacement, "=")))
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
