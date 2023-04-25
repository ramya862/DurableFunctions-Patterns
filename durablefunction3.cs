using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace durablefunction3

{


public static class Functionl

{

[FunctionName("FanOutFanIn")]

public static async Task<int> Run(
[OrchestrationTrigger] IDurableOrchestrationContext context)
{
var parallelTasks = new List<Task<int>>();

// Get a list of N work items to process in parallel.
 int[] workBatch = await context.CallActivityAsync<int[]>("F1", null);
 for (int i = 0; i < workBatch.Length; i++)
{
  Task<int> task = context.CallActivityAsync<int>("F2", workBatch[i]);
 parallelTasks.Add(task);

}

await Task.WhenAll(parallelTasks);

// Aggregate all N outputs and send the result to F3.

int sum = parallelTasks.Sum(t => t.Result);

int result = await context.CallActivityAsync<int>("F3", sum);
return result;


}


[FunctionName("F1")]

public static int[] F1([ActivityTrigger] string name, ILogger log)

{

return new int[] {1, 2, 3};
}

[FunctionName("F2")]

public static async Task<int> F2([ActivityTrigger] int value, ILogger log)

{
  return await Task.FromResult(value);

}

[FunctionName("F3")]

public static int F3([ActivityTrigger] int value, ILogger log)

{
    return value;

}


[FunctionName("Functionl_HttpStart")]

public static async Task<HttpResponseMessage> HttpStart(
[HttpTrigger(AuthorizationLevel. Anonymous, "get", "post")] HttpRequestMessage req,
[DurableClient] IDurableOrchestrationClient starter,
ILogger log)
{
// Function input comes from the request content.
string instanceId = await starter.StartNewAsync("FanOutFanIn", null);

log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

return starter.CreateCheckStatusResponse(req, instanceId);
}
}
}
