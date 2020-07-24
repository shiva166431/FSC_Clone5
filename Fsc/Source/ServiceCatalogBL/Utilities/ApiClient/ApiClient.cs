using System;
using System.Collections.Generic;
using System.Text;
using ServiceCatalog.BL;
using ServiceCatalog.BL.Models;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using PCAT.Common.Models;
using PCAT.Common.Models.Debugging;
using System.Xml.Serialization;
using PCAT.Common.Utilities;
using ServiceCatalog.BL.Utilities;

namespace ServiceCatalog.BL
{
    public class ApiClient
    {
        // reuse the same client to minimize connections to increase performance

        private static Dictionary<HeaderType, HttpClient> _httpClients;
        public static bool LogResults = true;

        #region Types
        public enum ContentType
        {
            Json,
            Xml
        }
        public enum HeaderType
        {
            FSCMediation,
            Standard
        }
        #endregion

        #region Constructor
        static ApiClient()
        {
            _httpClients = new Dictionary<ServiceCatalog.BL.ApiClient.HeaderType, HttpClient>();
        }
        #endregion

        #region Get/Post
        public static TResult Get<TResult>(HttpTargetType apiName, string apiMethod, string url, HeaderType headerType = HeaderType.Standard, int maxSecsToSendEmail = 30)
            where TResult : class
        {
            return CallApi<TResult, Object>(apiName, apiMethod, url, null, headerType, maxSecsToSendEmail, (client, uri, x) =>
            {
                return client.GetAsync(uri);
            });
        }

        public static TResult Post<TResult, TData>(HttpTargetType apiName, string apiMethod, string url, TData data, ContentType dataType = ContentType.Json, HeaderType headerType = HeaderType.Standard, int maxSecsToSendEmail = 30)
            where TResult : class
        {
            return CallApi<TResult, TData>(apiName, apiMethod, url, data, headerType, maxSecsToSendEmail, (client, uri, content) =>
            {
                switch (dataType)
                {
                    case ContentType.Json:
                        return client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));
                    case ContentType.Xml:
                        var stringWriter = new System.IO.StringWriter();
                        XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, ConformanceLevel = ConformanceLevel.Auto });
                        var serializer = new XmlSerializer(typeof(TData));
                        serializer.Serialize(xmlWriter, data);
                        xmlWriter.Close();

                        return client.PostAsync(uri, new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml"));
                }
                return null;
            });
        }
        #endregion

        #region CallApi
        private static TResult CallApi<TResult, TData>(HttpTargetType apiName, string apiMethod, string url, TData data, HeaderType headerType, int maxSecsToSendEmail,
            Func<HttpClient, string, TData, Task<HttpResponseMessage>> asyncCall)
            where TResult : class
        {
            TResult result = null;
            string json = null;
            var entry = LogResults ? new HttpCallLogEntry(apiName, apiMethod, url, data, maxSecsToSendEmail) : null;

            try
            {
                if (url == null || url.Length < 10)
                    FscApplication.Current.FireAddLogEntry("ApiClient", "ApiClient url problem? Url(" + url + "), Stacktrace: " + Environment.StackTrace, true);

                var async = asyncCall(GetClient(headerType), Uri.EscapeUriString(url), data);
                async.Wait();

                var task = async.Result.Content.ReadAsStringAsync();
                task.Wait();

                json = task.Result;
                if (entry != null)
                    entry.End(json);
                result = !string.IsNullOrEmpty(json)
                            ? JsonConvert.DeserializeObject<TResult>(json)
                            : null;
            }
            catch (Exception e)
            {
                if (entry != null)
                    entry.Exception(e);

                throw e;
            }
            finally
            {
                entry?.Save();
            }

            return result;
        }
        #endregion

        #region GetClient
        public static HttpClient GetClient(HeaderType headerType)
        {
            HttpClient client = null;
            if (_httpClients.ContainsKey(headerType))
                client = _httpClients[headerType];
            else
            {
                lock (_httpClients)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;
                    client = new HttpClient(handler);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    switch (headerType)
                    {
                        case HeaderType.FSCMediation:
                            var digest = new HmacDigest();
                            client.DefaultRequestHeaders.Add("X-Level3-Application-Key", digest.PublicKey);
                            client.DefaultRequestHeaders.Add("X-Level3-Digest", digest.HashedStringBase64);
                            client.DefaultRequestHeaders.Add("X-Level3-Digest-Time", digest.EpochTime.ToString());
                            break;
                    }

                    // double check to make sure it wasn't added by another thread
                    if (!_httpClients.ContainsKey(headerType))
                        _httpClients.Add(headerType, client);
                }
            }
            return client;
        }
        #endregion
    }
}
