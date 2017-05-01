#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
 
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO; 
using System.Text;
 

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string text, TraceWriter log)
{    
    log.Info("C# HTTP trigger function processed a request.");
    if (String.IsNullOrEmpty(text))
    {
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
    
    string result = await CallSentimentAPI(text, log); 

    if (String.IsNullOrEmpty(result))
    {
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
    return req.CreateResponse(HttpStatusCode.OK, result);
}

public static async Task<string> CallSentimentAPI(string text, TraceWriter log)
{
    log.Info($"Calling Sentiment API for {text}.");
    
    using (var client = new HttpClient())
    {
        var url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("Text_Analytics_Key"));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        var sentimentInput = new BatchInput {
			Documents = new List<DocumentInput> {
				new DocumentInput {
					Id = 1,
					Text = text,
				}
			}
		};
        var json = JsonConvert.SerializeObject(sentimentInput);
		var sentimentPost = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
		var sentimentRawResponse = await sentimentPost.Content.ReadAsStringAsync();
        log.Info(sentimentRawResponse);
		var sentimentJsonResponse = JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
		var sentimentScore = sentimentJsonResponse?.Documents?.FirstOrDefault()?.Score ?? 0;
		string message;
		if (sentimentScore > 0.7)
		{
			message = $"That's great to hear!";
		}
		else if (sentimentScore < 0.3)
		{
			message = $"I'm sorry to hear that, we aim to improve.";
		}
		else
		{
			message = $"You seem to have mixed feelings...";
		}
		log.Info($"Message is {message}");
        return message;

    }
}

public class BatchInput
{
    public List<DocumentInput> Documents { get; set; }
}
public class DocumentInput
{
    public double Id { get; set; }
    public string Text { get; set; }
}

// Classes to store the result from the sentiment analysis
public class BatchResult
{
    public List<DocumentResult> Documents { get; set; }
}
public class DocumentResult
{
    public double Score { get; set; }
    public string Id { get; set; }
}