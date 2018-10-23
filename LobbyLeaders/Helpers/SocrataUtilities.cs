using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LobbyLeaders.Helpers
{
    public static class SocrataUtilities
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };


        public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, Uri url, object body = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Add("Accept", "application/json");
            if (body != null)
            {
                HttpContent content = body as HttpContent;
                if (content != null)
                {
                    request.Content = content;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(body, _settings);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result;

            if (typeof(T) == typeof(HttpResponseMessage))
            {
                return (T)(object)response;
            }

            string responseBody = "{}";
            if (response.Content != null)
            {
                // sometimes returning new HttpResponseMessage() may yield no body. 
                responseBody = await response.Content.ReadAsStringAsync();
            }

            ThrowIfFailed(method, url, response, responseBody);

            var value = JsonConvert.DeserializeObject<T>(responseBody);
            return value;
        }

        private static void ThrowIfFailed(HttpMethod method, Uri url, HttpResponseMessage response, string body)
        {
            if (response.IsSuccessStatusCode)
                return;

            throw new InvalidOperationException($"Call to {method} {url} failed with {response.StatusCode}. Body={body}");
        }
    }
}
