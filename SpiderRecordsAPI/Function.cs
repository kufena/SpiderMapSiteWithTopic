using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpiderRecordsAPI;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> PostHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int) HttpStatusCode.OK,
            Body = ""
        };
    }
}
