# Game Telemetry Cost Estimation

It can be laborious to estimate exactly how much a cloud infrastructure will cost. This guide will hopefully simplify how the settings in the Azure Pricing Calculator correlate with the implementation of telemetry in your game title.

## Components

The system uses Functions, Cosmos DB, and Events Hubs. Each of these will be explained within the in-game telemetry context below. For someone implementing this system, these are the important pieces you will need to consider:

+ Number of concurrent users
+ Number of events per second
+ Number of seconds per upload (i.e. how often events are batched together before uploading)
+ Average size (in bytes) of each event

## Open the calculator

To start, open the [Azure Calculator](https://azure.com/e/3ed9f111bd674a05b974b4eeb2ede5ce)

Set the **Region** in each section to the region you expect to run this from

## Azure Functions
For this system, a function execution is any time there is a send or receive request to the server. That means any query run or event (or batch of events) is posted, it will be a single execution plus a second execution to pass the data to the database. While this second execution will only occur a single time for multiple requests in a small period of time, we will assume the worst case scenario that this runs with every upload. Executions take about 250ms, so the best estimate is to **set the exeuction time at the minimum of 1s and only account for 1/4 of the executions**.

**(Adjusted) Executions per month = (# of users * (total playtime / # of seconds per upload)) / 2**

Ex: 100 users playing 40 hours a week while uploading every 5 seconds
```
100 * (604,800 / 5) / 2 = 12,096,000 adjusted executions per month
```

**Cost saving tips:**
+ Batch your events as much as possible to reduce executions

## Azure Cosmos DB
Cosmos will be hosting our database. For this area we care about the number of requests to the database and storage. Each event, query, and deletion is a request.

**Provisioned RUs (RU/sec) = # of users * # of events per second**

Ex: 100 users sending 4 events per second
```
100 * 4 = 400 RU/sec
```

Storage depends heavily on how long you wish to keep your data. For many telemetry systems, long term retention holds little value and thus can be exported and archived or just deleted.

**Data generated (per second) = # of users * size of event * # of events per second**

Ex: 100 users sending 2 events per second averaging 512 bytes per event
```
100 * 2 * 512 = 100 kB/sec
100 kB/sec * 40 hours a week for a month = ~60 GB
```
**Cost saving tips:**
+ Delete or archive data at a regular cadence to keep storage capacity down

## Azure Event Hubs
The Event Hubs will manage the events as they enter the system. What we care about here is the **Thoughput units**, which is the total size of the events that are sent out every second in MB. The system compresses all data sent to minimze the cost.

**Throughput units = # of users * size of event (in MB) * # of events per second * 25% (average compression ratio)**

Ex: 100 users sending 2 events per second averaging 512 bytes per event
```
100 * 512 bytes * 2 * .25 = 25 kB/sec
Rounded up to MB = 1 MB/sec = 1 Throughput unit
```
