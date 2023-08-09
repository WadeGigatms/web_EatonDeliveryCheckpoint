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
    public static class HttpClientManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static bool PostToTerminalReader(int duration, int[] ports)
        {
            _httpClient.BaseAddress = new Uri("http://localhost/");
            var dto = new TerminalReaderTriggerDto
            {
                DODurationTime = duration,
                DOPortList = ports,
            };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync("api/DOController/", content).Result;
            return response.IsSuccessStatusCode;
        }
    }
}
