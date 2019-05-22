using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace cryptowatcherAI.Misc
{
    public static class HttpHelper
    {
        public static T GetApiData<T>(Uri ApiUri)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = ApiUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                }
                else
                { 
                    return default(T);
                }
            }
        }
    }
}