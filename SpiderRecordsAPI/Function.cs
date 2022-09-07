using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Util;
using System.Net;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;
using Contracts;
using Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpiderRecordsAPI;

public class Function
{
    AmazonSimpleNotificationServiceClient SNSClient;
    string SNSTopicEnvironmentName = "TopicArn";
    string TopicArn = "";

    public Function()
    {
        SNSClient = new AmazonSimpleNotificationServiceClient();
        var s = Environment.GetEnvironmentVariable(SNSTopicEnvironmentName);
        if (s == null)
        {
            throw new Exception("No environment variable set for TopicArn");
        }

        TopicArn = s;
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> PostHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var speciesRecord = JsonSerializer.Deserialize<SpeciesRecord>(input.Body);

        if (speciesRecord == null)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Null JSON in body or doesn't match what we expect for a new species record."
            };
        }

        string newId = Guid.NewGuid().ToString();

        SpeciesRecordWithId srwi = new SpeciesRecordWithId(newId, speciesRecord);
        var contract = new NewSpeciesRecord(srwi);
        int responseCode = await PushToTopic(contract);

        if (responseCode != (int)HttpStatusCode.OK)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Failed to distribute message"
            };
        }

        return new APIGatewayProxyResponse()
        {
            StatusCode = (int) HttpStatusCode.OK,
            Body = ""
        };
    }

    private async Task<int> PushToTopic(NewSpeciesRecord record) {
        var body = JsonSerializer.Serialize(record);
        PublishRequest request = new PublishRequest(TopicArn, body);
        var response = await SNSClient.PublishAsync(request);
        return (int)response.HttpStatusCode;
    }
}
