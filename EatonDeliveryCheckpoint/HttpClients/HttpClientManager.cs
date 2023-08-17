using EatonDeliveryCheckpoint.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.HttpClients
{
    public class HttpClientManager
    {
        public HttpClientManager(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private bool test = true;

        public bool PostToTriggerTerminalReader()
        {
            if (test) { return true; }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost/");
            var dto = new TerminalReaderTriggerDto
            {
                DODurationTime = 3,
                DOPortList = new int[] { 1 },
            };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync("api/DOController/", content).Result;
            return response.IsSuccessStatusCode;
        }

        public bool PostToNoTriggeTerminalReader()
        {
            if (test) { return true; }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost/");
            var dto = new TerminalReaderTriggerDto
            {
                DODurationTime = 3,
                DOPortList = new int[] { 0 },
            };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync("api/DOController/", content).Result;
            return response.IsSuccessStatusCode;
        }
    }
}
