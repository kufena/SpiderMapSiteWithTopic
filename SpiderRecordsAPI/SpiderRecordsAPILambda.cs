using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Util;
using System.Net;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;
using Contracts;
using Models;
using Amazon.Runtime.Internal.Util;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpiderRecordsAPI;

public class SpiderRecordsAPILambda
{
    AmazonSimpleNotificationServiceClient SNSClient;
    AmazonDynamoDBClient DBClient;

    string SNSTopicEnvironmentName = "TopicArn";
    string DBTableName = "TableName";
    string TopicArn = "";
    string TableName = "";

    public SpiderRecordsAPILambda()
    {
        SNSClient = new AmazonSimpleNotificationServiceClient();
        DBClient = new AmazonDynamoDBClient();

        var s = Environment.GetEnvironmentVariable(SNSTopicEnvironmentName);
        if (s == null)
        {
            throw new Exception("No environment variable set for TopicArn");
        }

        TopicArn = s;

        s = Environment.GetEnvironmentVariable(DBTableName);
        if (s == null)
        {
            throw new Exception("No environment variable set for DBTableName");
        }

        TableName = s;
    }

    public async Task<APIGatewayProxyResponse> GetHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var logger = context.Logger;
        string guid = input.PathParameters["guid"];

        if (guid is null)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "No GUID presented in path parameters."
            };
        }

        try
        {
            var value = await RetrieveFromDB(guid, logger);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(value)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = ""
            };
        }
    }

    public async Task<APIGatewayProxyResponse> PutHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = ""
        };
    }

    /// <summary>
    /// 
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
            Body = $"{newId.ToString()}"
        };
    }

    public async Task<APIGatewayProxyResponse> DeleteHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = ""
        };
    }

    private async Task<int> PushToTopic(NewSpeciesRecord record) {
        var body = JsonSerializer.Serialize(record);
        PublishRequest request = new PublishRequest(TopicArn, body);
        var response = await SNSClient.PublishAsync(request);
        return (int)response.HttpStatusCode;
    }

    private async Task<SpeciesRecordWithId> RetrieveFromDB(string guid, ILambdaLogger logger) 
    {
        var key = new Dictionary<string, AttributeValue>();
        key.Add("Guid", new AttributeValue(guid));
        GetItemResponse p = await DBClient.GetItemAsync(TableName, key);
        if (p.HttpStatusCode == HttpStatusCode.OK) {
            if (!p.IsItemSet)
            {
                logger.LogError("No Item Set returned from get item???");
                logger.LogError($"{p.ToString()}");
                throw new Exception("No item set returned from get item");
            }
            logger.LogError($"No of items = {p.Item.Count}");
            var items = p.Item;
            foreach (var (k, v) in items)
            {
                logger.LogError($"{k} == {v.S}");
            }
            return new SpeciesRecordWithId(guid, new SpeciesRecord(
                items["DateRecorded"].S,
                items["DateAdded"].S,
                items["SpeciesId"].S,
                items["Species"].S,
                items["User"].S,
                double.Parse(items["Latitude"].N),
                double.Parse(items["Longitude"].N)
            ));
        }

        throw new Exception("Not Found");
        
    }
}
