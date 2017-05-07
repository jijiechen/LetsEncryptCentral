using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CertManager.DnsProviders
{
    class DnsPodProvider : IDnsProvider
    {
        readonly HttpClient _httpClient = new HttpClient();
        readonly int _tokenId;
        readonly string _tokenValue;
        readonly string _domainName;

        const string DnsPodBaseUri = "https://dnsapi.cn/";
        const string RecordCreateAPI = "Record.Create";
        const string RecordRemoveAPI = "Record.Remove";

        public DnsPodProvider(int tokenId, string token, string domain)
        {
            _tokenId = tokenId;
            _tokenValue = token;
            _domainName = domain;
        }

        public string AddTxtRecord(string name, string value)
        {
            var parameters = new Dictionary<string, string>
            {
                { "domain", _domainName },
                { "record_type", "TXT" },
                { "record_line_id", "0"},  // Record.Line: https://www.dnspod.cn/docs/domains.html#record-line
                { "value", $"{name}={value}"}
            };

            var uri = new Uri(new Uri(DnsPodBaseUri), new Uri(RecordCreateAPI));
            var content = GetRequestContent(parameters);

            var recordCreateResult = InvokeDnsPodAPI<DnsPodCreateRecordResponseObject>(uri, content);
            return recordCreateResult.Record.Id;
        }

        public void RemoveTxtRecord(string recordRef)
        {
            if (string.IsNullOrEmpty(recordRef))
            {
                return;
            }

            var parameters = new Dictionary<string, string>
            {
                { "domain", _domainName },
                { "record_id", recordRef}
            };

            var uri = new Uri(new Uri(DnsPodBaseUri), new Uri(RecordRemoveAPI));
            var content = GetRequestContent(parameters);
            InvokeDnsPodAPI<DnsPodResponseObject>(uri, content);
        }

        T InvokeDnsPodAPI<T>(Uri uri, StringContent content) where T: DnsPodResponseObject
        {
            HttpResponseMessage response = null;
            string responseContent = null;
            try
            {
                response = _httpClient.PostAsync(uri, content).Result;
                responseContent = response.Content.ReadAsStringAsync().Result;
                var apiResponse = JsonConvert.DeserializeObject<T>(responseContent);

                // Common resposne: https://www.dnspod.cn/docs/info.html#common-response
                // Non 1 values are errors
                if (!string.Equals(apiResponse.Status.Code, "1"))
                {
                    throw new DnsPodResponseException(RecordCreateAPI, apiResponse, responseContent, response.StatusCode);
                }
                return apiResponse;
            }
            catch (HttpRequestException httpException)
            {
                throw new DnsPodResponseException(RecordCreateAPI, httpException, responseContent, (response == null ? HttpStatusCode.SwitchingProtocols : response.StatusCode));
            }
            catch (JsonException jsonException)
            {
                throw new DnsPodResponseException(RecordCreateAPI, jsonException, responseContent, response.StatusCode);
            }
        }

        StringContent GetRequestContent(Dictionary<string, string> extraParameters)
        {
            // Common parameters defined by DNSPod. See https://www.dnspod.cn/docs/info.html#common-parameters
            var allParameters = new Dictionary<string, string>()
            {
                { "login_token", _tokenValue },
                { "format", "json" },
                { "lang", "en" },
                { "error_on_empty", "yes" }
            };
            foreach (var key in extraParameters.Keys)
            {
                allParameters[key] = extraParameters[key];
            }

            var query = string.Join("&", allParameters.Keys.Select(key => string.Format("{0}={1}", key, Uri.EscapeDataString(allParameters[key]))));
            var content = new StringContent(query);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            return content;
        }

    }

    class DnsPodResponseException : Exception
    {
        public DnsPodResponseObject ApiResponse { get; }
        public string ResponseContent { get; }
        public HttpStatusCode ResponseStatusCode { get; }

        public DnsPodResponseException(string apiPath, DnsPodResponseObject responseObject, string responseContent, HttpStatusCode statusCode)
            : base("An error occured when trying to invoke DNSPod API " + apiPath)
        {
            this.ApiResponse = responseObject;
            this.ResponseContent = responseContent;
            this.ResponseStatusCode = statusCode;
        }

        public DnsPodResponseException(string apiPath, HttpRequestException httpException, string responseContent, HttpStatusCode statusCode)
             : base("An http error occured while invoking DNSPod API " + apiPath, httpException)
        {
            this.ResponseContent = responseContent;
            this.ResponseStatusCode = statusCode;
        }

        public DnsPodResponseException(string apiPath, JsonException jsonException, string responseContent, HttpStatusCode statusCode)
            : base("Could not read response object from API response returned from DNSPod API " + apiPath, jsonException)
        {
            this.ResponseContent = responseContent;
            this.ResponseStatusCode = statusCode;
        }
    }

    class DnsPodResponseObject
    {
        public DnsPodResponseStatus Status { get; set; }



        public class DnsPodResponseStatus
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }


    class DnsPodCreateRecordResponseObject: DnsPodResponseObject
    {
        public DnsPodDnsRecord Record { get; set; }

        public class DnsPodDnsRecord
        {
            public string Id { get; set; }
        }
    }
}