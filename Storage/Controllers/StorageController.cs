using System;
using System.Threading.Tasks;
using Dapr.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Storage.Models;

namespace Storage.Controllers
{
    [ApiController]
    public class StorageController : ControllerBase
    {
        [HttpGet("add")]
        public async void AddData(Data dataStored, [FromServices] DaprClient client)
        {
            await client.SaveStateAsync("statestore", dataStored.key, dataStored.data);
            foreach(string item in dataStored.data)
            {
                Console.WriteLine($"Data {item} stored at Key {dataStored.key}.");
            }
        }

        [HttpGet("delete")]
        public async void DeleteData(Data dataStored, [FromServices] DaprClient client)
        {    
            dataStored.data = await client.GetStateAsync<string[]>("statestore", dataStored.key);
            if(dataStored.data != null)
            {
                await client.DeleteStateAsync("statestore", dataStored.key);
                foreach(string item in dataStored.data)
                {
                    Console.WriteLine($"Data {item} deleted from Key {dataStored.key}.");
                }
            }
            else{Console.WriteLine($"There is no data at Key {dataStored.key}.");}
        }
        
        [HttpGet("get")]
        public async void GetData(Data dataStored, [FromServices] DaprClient client)
        {  
            dataStored.data = await client.GetStateAsync<string[]>("statestore", dataStored.key);
            if(dataStored.data != null)
            {
                foreach(string item in dataStored.data)
                {
                    Console.WriteLine($"Data {item} recieved from Key {dataStored.key}.");
                }
            }
            else{Console.WriteLine($"There is no data at Key {dataStored.key}.");}
        }
    }
}
