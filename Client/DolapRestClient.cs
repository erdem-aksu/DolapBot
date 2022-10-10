using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DolapBot.Client.Exceptions;
using DolapBot.Client.Extensions;
using DolapBot.Client.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DolapBot.Client
{
    public class DolapRestClient
    {
        protected IRestClient RestClient { get; }
        protected ProxyData ProxyData { get; set; }

        protected bool IsLoggedIn { get; set; }

        private string AccessToken { get; set; }

        private static readonly Dictionary<string, string> RequestHeaders =
            new Dictionary<string, string>()
            {
                {
                    "Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
                },
                {"Accept-Language", "en-US,en;q=0.5"}
            };

        private static readonly Dictionary<string, string> RequestHeadersApi =
            new Dictionary<string, string>()
            {
                {"Accept", "application/json, text/javascript, */*; q=0.01"},
                {"Accept-Language", "en-US,en;q=0.5"},
                {"X-Requested-With", "XMLHttpRequest"},
            };

        public DolapRestClient(ProxyData proxyData = null)
        {
            ProxyData = proxyData;

            RestClient = new RestClient
            {
                BaseUrl = new Uri(DolapClientConstants.BaseUrl),
                CookieContainer = new CookieContainer(),
                UserAgent = DolapClientConstants.UserAgent,
                Timeout = (int) TimeSpan.FromMinutes(2).TotalMilliseconds,
                ReadWriteTimeout = (int) TimeSpan.FromMinutes(2).TotalMilliseconds,
                Proxy = ProxyData != null
                    ? new WebProxy(
                        ProxyData.Address,
                        false,
                        Array.Empty<string>(),
                        new NetworkCredential(ProxyData.Username, ProxyData.Password)
                    )
                    : null
            };

            HttpWebRequest.DefaultMaximumErrorResponseLength = 1048576;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public void UseApiHeaders()
        {
            RestClient.AddDefaultHeaders(RequestHeadersApi);
        }

        public void UseBotHeaders()
        {
            RestClient.AddDefaultHeaders(RequestHeaders);
        }

        public async Task<IRestResponse<T>> GetAsync<T>(
            string requestUri,
            object options = null,
            IDictionary<string, string> urlSegments = null,
            IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(requestUri, Method.GET);

            if (urlSegments != null)
            {
                foreach (var pair in urlSegments)
                {
                    request.AddUrlSegment(pair.Key, pair.Value);
                }
            }

            if (options != null)
            {
                foreach (var pair in options.ToKeyValueString())
                {
                    request.AddQueryParameter(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return await SendAsync<T>(request);
        }

        public async Task<IRestResponse<T>> DeleteAsync<T>(
            string requestUri,
            IDictionary<string, string> urlSegments = null,
            object options = null,
            IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(requestUri, Method.DELETE);

            if (urlSegments != null)
            {
                foreach (var pair in urlSegments)
                {
                    request.AddUrlSegment(pair.Key, pair.Value);
                }
            }

            if (options != null)
            {
                foreach (var pair in options.ToKeyValueString())
                {
                    request.AddQueryParameter(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return await SendAsync<T>(request);
        }

        public async Task<IRestResponse<T>> PostAsync<T>(
            string requestUri,
            object value,
            IDictionary<string, string> urlSegments = null,
            bool isJson = true,
            object options = null,
            IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(requestUri, Method.POST);

            if (isJson)
            {
                request.AddJsonBody(value);
            }
            else
            {
                request.AlwaysMultipartFormData = true;

                foreach (var pair in value.ToKeyValueString())
                {
                    request.AddParameter(pair.Key, pair.Value);
                }
            }

            if (urlSegments != null)
            {
                foreach (var pair in urlSegments)
                {
                    request.AddUrlSegment(pair.Key, pair.Value);
                }
            }

            if (options != null)
            {
                foreach (var pair in options.ToKeyValueString())
                {
                    request.AddQueryParameter(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return await SendAsync<T>(request);
        }

        public async Task<IRestResponse<T>> PutAsync<T>(
            string requestUri,
            object value,
            IDictionary<string, string> urlSegments = null,
            bool isJson = true,
            object options = null,
            IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(requestUri, Method.PUT);

            if (isJson)
            {
                request.AddJsonBody(value);
            }
            else
            {
                request.AlwaysMultipartFormData = true;

                foreach (var pair in value.ToKeyValueString())
                {
                    request.AddParameter(pair.Key, pair.Value);
                }
            }

            if (urlSegments != null)
            {
                foreach (var pair in urlSegments)
                {
                    request.AddUrlSegment(pair.Key, pair.Value);
                }
            }

            if (options != null)
            {
                foreach (var pair in options.ToKeyValueString())
                {
                    request.AddQueryParameter(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return await SendAsync<T>(request);
        }

        public async Task<IRestResponse<T>> PatchAsync<T>(
            string requestUri,
            object value,
            IDictionary<string, string> urlSegments = null,
            bool isJson = true,
            object options = null,
            IDictionary<string, string> headers = null)
        {
            var request = new RestRequest(requestUri, Method.PATCH);

            if (isJson)
            {
                request.AddJsonBody(value);
            }
            else
            {
                request.AlwaysMultipartFormData = true;

                foreach (var pair in value.ToKeyValueString())
                {
                    request.AddParameter(pair.Key, pair.Value);
                }
            }

            if (urlSegments != null)
            {
                foreach (var pair in urlSegments)
                {
                    request.AddUrlSegment(pair.Key, pair.Value);
                }
            }

            if (options != null)
            {
                foreach (var pair in options.ToKeyValueString())
                {
                    request.AddQueryParameter(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return await SendAsync<T>(request);
        }

        public async Task<IRestResponse<T>> SendAsync<T>(IRestRequest request)
        {
            if (typeof(T) == typeof(string))
            {
                UseBotHeaders();

                RestClient.FailOnDeserializationError = false;
                RestClient.ThrowOnDeserializationError = false;
            }
            else
            {
                UseApiHeaders();

                RestClient.UseSerializer<NewtonsoftJsonSerializer>();
                RestClient.FailOnDeserializationError = true;
                RestClient.ThrowOnDeserializationError = true;
            }

            var response = await RestClient.ExecuteAsync<T>(request);

            if (!response.IsSuccessful)
            {
                throw CreateException(response);
            }

            return response;
        }

        private void Logout()
        {
            IsLoggedIn = false;
            RestClient.CookieContainer = new CookieContainer();
        }

        private static string BuildPath(string basePath, object options = null)
        {
            if (options == null) return basePath;

            var optionsPairs = options.ToKeyValueString();

            return optionsPairs.Aggregate(
                basePath,
                (current, pair) => current.AddQueryParam(pair.Key, pair.Value)
            );
        }

        private Exception CreateException(IRestResponse response)
        {
            var url = response.ResponseUri.ToString();
            var content = response.Content;

            string jsonMessage = null;
            try
            {
                var errorResponse = RestClient.Deserialize<JToken>(response);

                jsonMessage = errorResponse.Data.Value<string>("message");
            }
            catch
            {
                // ignored
            }

            var message = jsonMessage ?? response.StatusDescription;
            var status = (int) response.StatusCode;
            switch (status)
            {
                case 400:
                    return DolapException.Create<DolapBadRequestException>(
                        message,
                        url,
                        content
                    );
                case 401:
                    Logout();
                    return DolapException.Create<DolapAuthorizationException>(
                        message,
                        url,
                        content
                    );
                case 403:
                    return DolapException.Create<DolapForbiddenException>(
                        message,
                        url,
                        content
                    );
                case 404:
                    return DolapException.Create<DolapNotFoundException>(
                        message,
                        url,
                        content
                    );
                case 408:
                    return DolapException.Create<DolapTimeoutException>(
                        message,
                        url,
                        content
                    );
                case 429:
                    return DolapException.Create<DolapRateLimitExceededException>(
                        message,
                        url,
                        content,
                        429
                    );
                case 500:
                case 502:
                case 599:
                    return DolapException.Create<DolapServerErrorException>(
                        message,
                        url,
                        content,
                        status
                    );
                default:
                    return new DolapException(message)
                    {
                        RequestUrl = url,
                        ResponseContent = content,
                        HttpStatusCode = status
                    };
            }
        }
    }
}