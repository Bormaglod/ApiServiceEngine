namespace ApiServiceEngine
{
    using System;
    using System.Collections.Specialized;
    using System.Net.Http;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    
    class SingleHttpClientInstanceController
    {
        private static readonly HttpClient HttpClient;

        static SingleHttpClientInstanceController()
        {
            HttpClient = new HttpClient();
        }

        public async Task<string> InfoCardLimitSum(string url, object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            try
            {
                serializer.WriteObject(stream, obj);
                string json = Encoding.Default.GetString(stream.ToArray());

                var response = await Request(HttpMethod.Post, $"{url}/RequestCardLimitSum", json, null);
                string responseText = await response.Content.ReadAsStringAsync();

                return await HttpClient.GetStringAsync(url);
            }
            finally
            {
                stream.Close();
            }
        }

        static async Task<HttpResponseMessage> Request(HttpMethod method, string url, string jsonContent, StringDictionary headers)
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = method;
            httpRequestMessage.RequestUri = new Uri(url);
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    httpRequestMessage.Headers.Add(key, headers[key]);
                }
            }

            switch (method.Method)
            {
                case "POST":
                    HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = httpContent;
                    break;

            }

            return await HttpClient.SendAsync(httpRequestMessage);
        }
    }
}
