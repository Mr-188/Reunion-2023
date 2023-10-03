using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientCore;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Rampastring.Tools;
using ReunionShareModel;

namespace DTAClient.Online;

internal class ReunionApi
{
    private static readonly HttpClient _httpClient = new HttpClient();

    [ItemCanBeNull]
    public static async Task<TResponse> SendRequest<TResponse>(ApiRequest<TResponse> request)
        where TResponse : ApiResponse
    {
       // var host = ClientConfiguration.Instance.ReunionApiHost;
        var host = "https://raa2022.top/api/";
        try
        {
            if (host.EndsWith('/') && request.Path.StartsWith('/'))
            {
                host = host[..^1];
            }

            var uri = new Uri(host + request.Path);
           
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var json = JsonConvert.DeserializeObject<TResponse>(jsonString);

                return json;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            Logger.Log("Api Error", e);
        }

        return null;
    }
}