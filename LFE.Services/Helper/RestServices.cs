using System;
using RestSharp;

namespace LFE.Application.Services.Helper
{
    public class RestServices
    {
        public string GetCfSignedUrl(string path)
        {
            var client = new RestClient {BaseUrl = new Uri("http://localhost:8081") };
            var request = new RestRequest("getSignedUrl", Method.POST);
            request.AddParameter("path", path);

            var response = client.Execute(request);

            return response.Content;

        }
    }
}
