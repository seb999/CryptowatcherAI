using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace cryptowatcherAI.Misc
{
    public static class HttpHelper
    {
        public static string GetApiData(Uri ApiUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = ApiUri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "";
        }
    }
}