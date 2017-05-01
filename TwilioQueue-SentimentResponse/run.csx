using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public static async Task Run(string myQueueItem, TraceWriter log)
{
    
    log.Info(myQueueItem);
    var formValues = myQueueItem.Split('&')
        .Select(value => value.Split('='))
        .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "), 
                      pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

    var phoneNumber = formValues["From"];
    var fromCity = formValues["FromCity"];
    var fromZip = formValues["FromZip"];
    var fromState = formValues["FromState"];
    var body = formValues["Body"];
    
    log.Info($"Incoming SMS from {fromCity},{phoneNumber}. Message: {body}");
    
    var sentiment = await GetSentiment(body);
    log.Info(sentiment);
    SendResponse(phoneNumber, sentiment);

}

static async Task<string> GetSentiment(string message)
{
    using(var client = new HttpClient())
    {
        var response = await client.GetStringAsync($"https://codecampdemo.azurewebsites.net/api/HttpTriggerCSharp/text/\"{message}\"?code=ML3rB719Dm0uugiAGc7ulRS38hqaqqNaSQHjuZq5a44Xqg03JtuB6A==");
        return response;
    }
    
}

static void SendResponse(string phone, string text)
{
    TwilioClient.Init(Environment.GetEnvironmentVariable("ACCOUNT_SID"),
     Environment.GetEnvironmentVariable("AUTH_TOKEN"));
    var message = MessageResource.Create(
        to: new PhoneNumber(phone),
                from: new PhoneNumber("+13123130075"),
                body: text);
    
}