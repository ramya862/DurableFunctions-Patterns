using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
public static class HumanInteractionFunction
{
    [FunctionName("HumanInteractionFunction")]
    public static async Task<object> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        // Get the input from the initial trigger
        var input = context.GetInput<string>();

        // Send a message to the user
        string message = "Hello, please respond with 'yes' or 'no'.";
        await context.CallActivityAsync("SendMessageActivity", message);

        // Wait for a response from the user
        string response = await context.WaitForExternalEvent<string>("UserResponseEvent");

        // Process the user's response
        if (response == "yes")
        {
            message = "You said yes!";
        }
        else if (response == "no")
        {
            message = "You said no!";
        }
        else
        {
            message = "Sorry, I didn't understand your response.";
        }

        // Send a final message to the user
        await context.CallActivityAsync("SendMessageActivity", message);

        return message;
    }

    [FunctionName("SendMessageActivity")]
    public static void SendMessageActivity(
        [ActivityTrigger] string message,
        ILogger log)
    {
        // Send the message to the user through some communication channel (e.g. email, SMS, chat, etc.)
        log.LogInformation(message);
    }

    [FunctionName("UserResponseWebhook")]
    public static async Task UserResponseWebhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] string requestBody,
        [DurableClient] IDurableOrchestrationClient client,
        ILogger log)
    {
        // Get the response from the request body
        string response = requestBody;

        // Get the instance ID and raise an event to the orchestration
        string instanceId = await client.GetInstanceIdAsync(Request);
        await client.RaiseEventAsync(instanceId, "UserResponseEvent", response);
    }
}
