# DaprRedisStore
Using Dapr in conjunction with the built in redis store on docker to set up a NoSQL database.

# Prerequisites
### Installation of Docker 
![Docker](https://github.com/AlexanderAzzopardi/UnitConvertor/blob/main/Saved%20Pictures/DockerLogo.jfif)
> <https://docs.docker.com/engine/install/>

### Installation of Dapr 
![Dapr](https://github.com/AlexanderAzzopardi/UnitConvertor/blob/main/Saved%20Pictures/DaprLogo.jfif)
> <https://docs.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/getting-started>

### Installation of RestClient
Search **RestClient** in the extension tab and install. 

# Dapr Storage Microservice
You will need to set up the dapr microservice storage with the commands

> dotnet new webapi -o storage

> dotnet add package Dapr.AspNetCore --version 0.12.0-preview01

In *Startup.cs* add;

    services.AddControllers().AddDapr();

Add *StorageController.cs* with the following content;

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
                Console.WriteLine($"Data {dataStored.data} stored at Key {dataStored.key}.");
            }

            [HttpGet("delete")]
            public async void DeleteData(Data dataStored, [FromServices] DaprClient client)
            {    
                dataStored.data = await client.GetStateAsync<string>("statestore", dataStored.key);
                if(dataStored.data != null)
                {
                    await client.DeleteStateAsync("statestore", dataStored.key);
                    Console.WriteLine($"Data {dataStored.data} deleted from Key {dataStored.key}.");
                }
                else{Console.WriteLine($"There is no data at Key {dataStored.key}.");}
            }

            [HttpGet("get")]
            public async void GetData(Data dataStored, [FromServices] DaprClient client)
            {  
                dataStored.data = await client.GetStateAsync<string>("statestore", dataStored.key);
                if(dataStored.data != null)
                {
                    Console.WriteLine($"Data {dataStored.data} recieved from Key {dataStored.key}.");
                }
                else{Console.WriteLine($"There is no data at Key {dataStored.key}.");}
            }
        }
    }

Also add *Data.cs* inside a *Models* folder and fill with the following content;

    using System;
    using System.Collections.Generic;

    namespace Storage.Models
    {
        public class Data
        {
            public string key {get; set;}
            public string data {get; set;}
        }
    }

# Redis Store
When setting up a redis store you need to create a .yaml file with the following content;

    apiVersion: dapr.io/v1alpha1
    kind: Component
    metadata:
      name: statestore
      namespace: default
    spec:
      type: state.redis
      version: v1
      metadata:
      - name: redisHost
        value: localhost:6379
      - name: redisPassword
        value: ""
      - name: enableTLS
        value: true # Optional. Allowed: true, false.
      - name: failover
        value: true # Optional. Allowed: true, false.
        
# Redis Commands

## Via C#
This method allows you to invoke functions wich interact with the redis store.
You need to create a *StateStore.http* and fill with the following content;

    GET http://localhost:5010/v1.0/invoke/storage/method/add HTTP/1.1
    content-type: application/json

    {
        "Data": "Alex",
        "Key": "1"
    }

    ###
    GET http://localhost:5010/v1.0/invoke/storage/method/delete HTTP/1.1
    content-type: application/json

    {
        "Key": "1"
    }

    ###
    GET http://localhost:5010/v1.0/invoke/storage/method/get HTTP/1.1
    content-type: application/json

    {
        "Key": "1"
    }

Inside your C# code you need to access the darp client using one of the following lines.
Import a client from Dapr into a function
    [FromServices] DaprClient client

or Build a new Dapr client
    var client = new DaprClientBuilder().Build();

### Storing a value
    await client.SaveStateAsync(statestore, Key, Data);

### Getting a value
    var data = await client.GetStateAsync<DataType>(statestore, Key);
  
### Deleting a value
    await client.DeleteStateAsync(statestore, Key);
  
## Via Http Requests

This method means the http request communicates directly with the redis store so the controller in the *storage* dapr microsrvice is not needed.
Create a *StateStore.http* file and fill with the following content;

    ### Storing a single value
    POST http://localhost:5010/v1.0/state/statestore HTTP/1.1
    content-type: application/json

    [
        {
            "key": "1", 
            "value": "value1"
        }
    ]

    ### Retrieving a single value
    GET http://localhost:5010/v1.0/state/statestore/1

    ### Storing multiple values under one Key
    POST http://localhost:5010/v1.0/state/statestore HTTP/1.1
    content-type: application/json

    [
        {
            "key": "1", 
            "value": ["value1a","value1b"]
        }
    ]
    
    ### Storing multiple values under many Key
    POST http://localhost:5010/v1.0/state/statestore HTTP/1.1
    content-type: application/json

    [
        {
            "key": "1", 
            "value": "value1"
        }, 
        { 
            "key": "2", 
            "value": "value2"
        },
        { 
            "key": "3", 
            "value": "value3"
        }
    ]

    ### Retrieving mulitples value
    POST http://localhost:5010/v1.0/state/statestore/bulk
    content-type: application/json

    {
        "keys":
        [
            "1", 
            "3"
        ]
    }

    ### Deleting values
    DELETE  http://localhost:5010/v1.0/state/statestore/1

  
