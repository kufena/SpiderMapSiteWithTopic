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
    string SNSTopicEnvironmentName = "";
    string TopicArn = "";

    public Function()
    {
        SNSClient = new AmazonSimpleNotificationServiceClient();
        Environment.GetEnvironmentVariable(SNSTopicEnvironmentName);
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

        int responseCode = await PushToTopic(srwi);

        return new APIGatewayProxyResponse()
        {
            StatusCode = (int) HttpStatusCode.OK,
            Body = ""
        };
    }

    private async Task<int> PushToTopic(SpeciesRecordWithId record) {
        var body = JsonSerializer.Serialize(record);
        PublishRequest request = new PublishRequest(TopicArn, body);
        var response = await SNSClient.PublishAsync(request);
        return (int)response.HttpStatusCode;
    }
}
