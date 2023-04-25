using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
namespace MyAggregatorfunctionss{
 public static class Myclass{
[FunctionName("OrchestrationFunction")]
public static async Task<List<string>> RunOrchestrator(
    [OrchestrationTrigger] IDurableOrchestrationContext context)
{
    var tasks = new List<Task<string>>();
    
    // Start multiple parallel workflows
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(context.CallActivityAsync<string>("ActivityFunction", i.ToString()));
    }

    // Wait for all workflows to complete
    await Task.WhenAll(tasks);

    // Aggregate results from all workflows
    var results = new List<string>();
    foreach (var task in tasks)
    {
        if (task.Status == TaskStatus.RanToCompletion)
        {
            results.Add(task.Result);
        }
    }

    // Return the final result
    return results;
}

[FunctionName("ActivityFunction")]
public static string RunActivity([ActivityTrigger] string input)
{
    // Process input and return result
    return $"Result for input {input}";
}
[FunctionName("Function1_HttpStart")]
public static async Task<HttpResponseMessage>HttpStart(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
[DurableClient] IDurableOrchestrationClient starter,ILogger log)
{string instanceId = await starter.StartNewAsync("OrchestrationFunction",null);

log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

return starter.CreateCheckStatusResponse(req,instanceId);
}

}
}