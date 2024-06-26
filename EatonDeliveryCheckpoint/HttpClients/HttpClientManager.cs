﻿using EatonDeliveryCheckpoint.Dtos;
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

        public bool PostToTriggerTerminalReader()
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                try
                {
                    //httpClient.BaseAddress = new Uri("http://localhost/"); // test
                    //httpClient.BaseAddress = new Uri("http://10.10.10.18:5000/"); // beta
                    httpClient.BaseAddress = new Uri("http://192.168.0.110:5000/"); // standard
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var dto = new TerminalReaderTriggerDto
                    {
                        DODurationTime = "10",
                        DOPortList = new string[] { "1", "2", "4" },
                    };
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    var result1 = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    var result2 = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    return true;
                }
                catch (Exception exp)
                {
                    return false;
                }
            }
        }

        public bool PostToDismissTriggerTerminalReader()
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                try
                {
                    //httpClient.BaseAddress = new Uri("http://localhost/"); // test
                    //httpClient.BaseAddress = new Uri("http://10.10.10.18:5000/"); // beta
                    httpClient.BaseAddress = new Uri("http://192.168.0.110:5000/"); // standard
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var dto = new TerminalReaderTriggerDto
                    {
                        DODurationTime = "0",
                        DOPortList = new string[] { "1", "2", "4" },
                    };
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    var result1 = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    var result2 = httpClient.PostAsync("api/DOController", content).ConfigureAwait(false);
                    return true;
                }
                catch (Exception exp)
                {
                    return false;
                }
            }
        }
    }
}
