#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#r "System.Runtime"

#load "QueueUtils.csx"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Twilio.TwiML;


public static async Task<object> Run(HttpRequestMessage req, IAsyncCollector<string> outputQueueItem, TraceWriter log)
{
    //Twilio sends a sms in the format of
    //ToCountry=US&ToState=IL&SmsMessageSid=SM0c8bbd5359b1eb0fbc826b7b03509841&NumMedia=0&ToCity=CHICAGO&FromZip=60517&SmsSid=SM0c8bbd5359b1eb0fbc826b7b03509841&FromState=IL
    //&SmsStatus=received&FromCity=NORTHBROOK&Body=Did+I+break+it%3F&FromCountry=US&To=%2B13123130075&ToZip=60661&NumSegments=1&MessageSid=SM0c8bbd5359b1eb0fbc826b7b03509841&AccountSid=AC807691e9fc06bcb7da45f49bbbbc88a5&From=%2B16306739392&ApiVersion=2010-04-01
    //whenenver an sms is sent to 312-313-0075
   var data = await req.Content.ReadAsStringAsync();
    log.Info(data);
    var formValues = data.Split('&')
        .Select(value => value.Split('='))
        .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "), 
                      pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

    var phoneNumber = formValues["From"];
    var fromCity = formValues["FromCity"];
    var fromZip = formValues["FromZip"];
    var fromState = formValues["FromState"];
    var body = formValues["Body"];
    log.Info($"Incoming SMS from {fromCity},{phoneNumber}. Message: {body}");
    await outputQueueItem.AddAsync(data);
    //AddToQueue(data);
    return RespondToSms("Welcome to Code Camp. I am analyzing your sentiment.");

}

static HttpResponseMessage RespondToSms(string message)
{

    var response = new MessagingResponse()
        .Message(message);
    var twiml = response.ToString();
    twiml = twiml.Replace("utf-16", "utf-8");

    return new HttpResponseMessage
    {
        Content = new StringContent(twiml, Encoding.UTF8, "application/xml")
    };
}





