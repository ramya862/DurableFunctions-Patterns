using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace durablefunctionsMonitor
{
  public static class Function1
{
 [FunctionName("MonitorJobStatus")]
 public static async Task Run (
 [OrchestrationTrigger] IDurableOrchestrationContext context)
{
  DateTime expiryTime = DateTime.UtcNow.AddMinutes(5);
  while (context.CurrentUtcDateTime < expiryTime)
 {
 var jobStatus = await context.CallActivityAsync<string>("GetJobStatus",null);
 if (jobStatus == "Completed")
 {
 await context.CallActivityAsync("SendAlert",null);
  break;
}
}
// Perform more work here, or let the orchestration end.
}
[FunctionName("GetJobStatus")] 
public static string GetJobStatus([ActivityTrigger] string name, ILogger log)
{
 return "Completed";
}
[FunctionName("SendAlert")]
public static string SendAlert([ActivityTrigger] string name, ILogger log)
{
return "SendAlert";
}
[FunctionName("Functionl_HttpStart")]
public static async Task<HttpResponseMessage>HttpStart(
[HttpTrigger(AuthorizationLevel. Anonymous,"get", "post")] HttpRequestMessage req,
[DurableClient] IDurableOrchestrationClient starter,ILogger log)
{
// Function input comes from the request content.
string instanceId = await starter.StartNewAsync("MonitorJobStatus",null);
log. LogInformation($"started orchestration with ID = ‘{instanceId}'.");
return starter.CreateCheckStatusResponse(req, instanceId);
}
}

}