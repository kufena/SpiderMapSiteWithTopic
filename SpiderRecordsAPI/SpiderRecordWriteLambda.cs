using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS.Model;
using Amazon.SQS;
using System.Text.Json;
using Contracts;
using Models;
using Polly;
using Polly.Retry;

namespace SpiderRecordsAPI
{
    public  class SpiderRecordWriteLambda
    {

        AmazonDynamoDBClient DBClient;
        string DBName;
        string QueueURL;

        public SpiderRecordWriteLambda()
        {
            DBClient = new AmazonDynamoDBClient();
            var dbenv = Environment.GetEnvironmentVariable("SpiderRecordsDBName");
            if (dbenv == null)
                throw new Exception("No SpiderRecordsDBName environment variable set.");
            DBName = dbenv;
            var qenv = Environment.GetEnvironmentVariable("QueueURL");
            if (qenv == null)
                throw new Exception("No Queue URL given in environment for lambda function");
            QueueURL = qenv;
        }

        public async Task<SQSBatchResponse> BatchHandler(SQSEvent messages, ILambdaContext context)
        {
            var logger = context.Logger;
            logger.LogError($"We're in the Handler of SpiderRecordWriteLambda - with {messages.Records.Count} messages.");

            var response = new SQSBatchResponse()
            {
                BatchItemFailures = new List<SQSBatchResponse.BatchItemFailure>()
            };

            int numMessages = messages.Records.Count;
            int completed = 0;
            foreach (var message in messages.Records)
            {
                var done = await HandleMessage(message, logger);
                if (done)
                {
                    logger.LogError($"Event source: {message.EventSource}");
                    logger.LogError($"Event source Arn: {message.EventSourceArn}");
                    completed++;
                }
                else
                {
                    response.BatchItemFailures.Add(new SQSBatchResponse.BatchItemFailure() { ItemIdentifier = message.MessageId });
                }
            }

            logger.LogInformation($"Passed {numMessages} messages, completed {completed}");
            return response;
        }

        public async Task<Boolean> HandleMessage(SQSEvent.SQSMessage message, ILambdaLogger logger)
        {
            var attrs = new Dictionary<string, AttributeValue>();
            var speciesRecordContract = JsonSerializer.Deserialize<NewSpeciesRecord>(message.Body);
            if (speciesRecordContract == null) {
                logger.LogError("Null Species Record presented in message - ignoring.");
                return true;
            }

            var speciesRecord = speciesRecordContract.Record;
            attrs.Add("Guid", new AttributeValue(speciesRecord.Id));
            attrs.Add("User", new AttributeValue(speciesRecord.Record.Recorder));
            attrs.Add("Latitude", new AttributeValue() { N = $"{speciesRecord.Record.latitude}" });
            attrs.Add("Longitude", new AttributeValue() { N = $"{speciesRecord.Record.longitude}" });
            attrs.Add("Species", new AttributeValue(speciesRecord.Record.SpeciesName));
            attrs.Add("SpeciesId", new AttributeValue(speciesRecord.Record.SpeciesId));
            attrs.Add("DateAdded", new AttributeValue(speciesRecord.Record.DateAdded));
            attrs.Add("DateRecorded", new AttributeValue(speciesRecord.Record.DateRecorded));

            // I made this global at first - but it kind of documents what's happening in the line beloe
            // so moved it into the method.  
            Policy DBPolicy = Policy.Handle<ProvisionedThroughputExceededException>()
                                    .Or<RequestLimitExceededException>()
                                    .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            try
            {
                var dbResponse = await DBPolicy.Execute( () => DBClient.PutItemAsync(DBName, attrs));
            }
            catch (Exception ex) {
                logger.LogError(ex.ToString());
                return false;
            }

            // I think, if we get this far, without an exception, we're ok.
            // Of course, the lambda may have timed out by now.
            return true;
        }
    }
}
