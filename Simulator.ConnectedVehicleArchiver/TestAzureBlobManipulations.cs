// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Azure.Storage.Blobs.Models;
using Econolite.Ode.Cloud.Common.Managers;
using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Simulator.ConnectedVehicleArchiver
{
    public class TestAzureBlobManipulations: IHostedService
    {
        private readonly ILogger<ArchiveMessages> _logger;
        private IConfiguration _configuration;
        private IConnectedVehicleArchiveService _cvArchiveService;
        private IConnectedVehicleLoggerService _cvLogService;

        public TestAzureBlobManipulations(ILogger<ArchiveMessages> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = config;
            _cvArchiveService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleArchiveService>();
            _cvLogService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleLoggerService>();
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            //simple test to push to azure

            //test without tag options
            _logger.LogInformation("Uploading data to Azure Storage Account...");
            var connectionStr = _configuration["ConnectionStrings:AzureBlobCoolStorageConnectionString"] ?? throw new NullReferenceException("ConnectionStrings:AzureBlobCoolStorageConnectionString missing in configuration"); ;
            var containerName = _configuration["Containers:ConnectedVehicleLog"] ?? throw new NullReferenceException("Containers:ConnectedVehicleLog missing in configuration");

            //just dumping some sample data from Mongo; using the same data for all since we aren't using it at the moment
            //var jsonString = "{\"timeStamp\":{\"$date\":{\"$numberLong\":\"1664988345585\"}},\"metaData\":{\"logEntryByteSize\":1837,\"type\":\"BSM\"},\"logEntry\":{\"metadata\":{\"bsmSource\":\"ECONOLITETESTINGBSM\",\"logFileName\":\"rxMsg_BSM_and_TIM.gz\",\"recordType\":\"rxMsg\",\"securityResultCode\":\"success\",\"receivedMessageDetails\":{\"locationData\":{\"latitude\":\"0\",\"longitude\":\"0\",\"elevation\":\"0\",\"speed\":\"0\",\"heading\":\"0\"},\"rxSource\":\"RV\"},\"payloadType\":\"us.dot.its.jpo.ode.model.OdeBsmPayload\",\"serialId\":{\"streamId\":\"6f41e982-239f-4980-b99f-12b46f083aa1\",\"bundleSize\":393,\"bundleId\":15,\"recordId\":390,\"serialNumber\":1112},\"odeReceivedAt\":\"2022-08-03T18:12:49.523Z\",\"schemaVersion\":6,\"maxDurationTime\":0,\"odePacketID\":\"\",\"odeTimStartDateTime\":\"\",\"recordGeneratedAt\":\"2018-12-06T11:18:12.898Z\",\"recordGeneratedBy\":\"OBU\",\"sanitized\":false},\"payload\":{\"dataType\":\"us.dot.its.jpo.ode.plugin.j2735.J2735Bsm\",\"data\":{\"coreData\":{\"msgCnt\":121,\"id\":\"31325442\",\"secMark\":12609,\"position\":{\"latitude\":41.159249,\"longitude\":-104.6614297,\"elevation\":1823.6},\"accelSet\":{\"accelLat\":0,\"accelLong\":0,\"accelVert\":0,\"accelYaw\":0},\"accuracy\":{\"semiMajor\":2.8,\"semiMinor\":5.15},\"transmission\":\"NEUTRAL\",\"speed\":12.8,\"heading\":90.8125,\"brakes\":{\"wheelBrakes\":{\"leftFront\":false,\"rightFront\":false,\"unavailable\":true,\"leftRear\":false,\"rightRear\":false},\"traction\":\"unavailable\",\"abs\":\"unavailable\",\"scs\":\"unavailable\",\"brakeBoost\":\"unavailable\",\"auxBrakes\":\"unavailable\"},\"size\":{\"width\":239,\"length\":726}},\"partII\":[{\"id\":\"VehicleSafetyExtensions\",\"value\":{\"pathHistory\":{\"crumbData\":[{\"elevationOffset\":-0.3,\"latOffset\":-0.0000043,\"lonOffset\":0.0003624,\"timeOffset\":2.38},{\"elevationOffset\":-1.7,\"latOffset\":-0.0000246,\"lonOffset\":0.0028373,\"timeOffset\":19.51},{\"elevationOffset\":3.7,\"latOffset\":-0.000042,\"lonOffset\":0.0053293,\"timeOffset\":37.14}]},\"pathPrediction\":{\"confidence\":100,\"radiusOfCurve\":0}}},{\"id\":\"SpecialVehicleExtensions\",\"value\":{}},{\"id\":\"SupplementalVehicleExtensions\",\"value\":{}}]}}}}";
            var jsonString = "{\"timeStamp\": {\"$date\": {\"$numberLong\": \"1664988346587\"}},\"metaData\": {\"logEntryByteSize\": 2109,\"type\": \"SPAT\"},\"logEntry\": {\"metadata\": {\"payloadType\": \"us.dot.its.jpo.ode.model.OdeAsn1Payload\",\"serialId\": {\"streamId\": \"02a4c72a-7682-4618-9cf9-a045ae4d5131\",\"bundleSize\": \"1\",\"bundleId\": \"0\",\"recordId\": \"0\",\"serialNumber\": \"0\"},\"odeReceivedAt\": \"2022-09-07T18:50:05.378Z\",\"schemaVersion\": \"6\",\"maxDurationTime\": \"0\",\"odePacketID\": [],\"odeTimStartDateTime\": [],\"recordGeneratedAt\": \"1970-01-01T03:25:34.023Z\",\"recordGeneratedBy\": \"OBU\",\"sanitized\": \"false\",\"logFileName\": \"spatTx_test_binary\",\"recordType\": \"spatTx\",\"securityResultCode\": \"success\",\"receivedMessageDetails\": [],\"encodings\": [{\"elementName\": \"root\",\"elementType\": \"Ieee1609Dot2Data\",\"encodingRule\": \"COER\"},{\"elementName\": \"unsecuredData\",\"elementType\": \"MessageFrame\",\"encodingRule\": \"UPER\"}],\"spatSource\": \"V2X\"},\"payload\": {\"dataType\": \"MessageFrame\",\"data\": {\"MessageFrame\": {\"messageId\": \"19\",\"value\": {\"SPAT\": {\"timeStamp\": \"19000\",\"name\": \"07\",\"intersections\": [{\"IntersectionState\": {\"name\": \"07\",\"id\": {\"region\": \"1\",\"id\": \"1\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"02\",\"id\": {\"region\": \"1\",\"id\": \"2\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"03\",\"id\": {\"region\": \"1\",\"id\": \"3\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"04\",\"id\": {\"region\": \"2\",\"id\": \"4\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}}]}}}}}}}";
            byte[] data = Encoding.ASCII.GetBytes(jsonString);

            //test with tag options
            var size = 60;
            var timeStamp = new DateTime(2021, 12, 31, 1, 1, 1, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 600;
            timeStamp = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 666;
            timeStamp = new DateTime(2022, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 700;
            timeStamp = new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 701;
            timeStamp = new DateTime(2022, 1, 10, 1, 1, 1, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 6000;
            timeStamp = new DateTime(2022, 1, 11, 0, 0, 0, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            size = 16000;
            timeStamp = new DateTime(2022, 10, 1, 0, 0, 0, DateTimeKind.Utc);
            uploadBlob(size, timeStamp, connectionStr, containerName, data);

            //pause a bit to make sure azure has time to write the records
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Sleeping for azure to catch up." + Environment.NewLine);

            // Find Blobs by timestamp tag; test the lex sort
            //string query = @"""TimeStamp"" >= '" + $"{startDate:yyyy-MM-dd'T'HH:mm:ss.fffK}" + "' AND ""TimeStamp"" <= '" + $"{endDate:yyyy-MM-dd'T'HH:mm:ss.fffK}" + "'";
            string query = @"""TimeStamp"" >= '2022-01-01T00:00:00.000Z' AND ""TimeStamp"" <= '2022-01-10T00:00:00.000Z'";
            findByTags(connectionStr, containerName, query, false);
            Console.WriteLine("Should have found 3 timestamps" + Environment.NewLine);

            // Find Blobs by byteSize tag;test the lex sort
            //need a string sort on an integer; searching starts at the left; need to pad the numbers so the numeric range search will work
            //terabytes are 12 chars - we'll take our precision out to 32 for good measure
            var startSize = 600;
            var endSize = 700;
            var padStart = $"{startSize.ToString().PadLeft(32, '0')}";
            var padEnd = $"{endSize.ToString().PadLeft(32, '0')}";
            //azure query format wants double quotes around the key and single quotes around the values
            query = String.Format(@"""ByteSize"" >= '{0}' AND ""ByteSize"" <= '{1}'", padStart, padEnd);
            findByTags(connectionStr, containerName, query, false);
            Console.WriteLine("Should have found 3 sizes" + Environment.NewLine);

            var summary = await AzureStorageManager.GetContainerSummaryAsync(connectionStr, containerName, true);
            Console.WriteLine($"Container result: Size= {summary.ContainerByteSize}, " +
                                $"Count= {summary.BlobCount}" + Environment.NewLine);
            Console.WriteLine("Should have found 7 blobs" + Environment.NewLine);

            //Delete blobs by tags
            //first find them and then delete them
            query = @"""DevTesting"">= 'true'";
            findByTags(connectionStr, containerName, query, true);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void uploadBlob(double size, DateTime timeStamp, string connectionStr, string containerName, byte[] data)
        {
            //need a string sort on an integer; searching starts at the left; need to pad the numbers so the numeric range search will work
            //terabytes are 12 chars - we'll take our left padding out to 32 for good measure
            var sizePadded = $"{size.ToString().PadLeft(32, '0')}";

            var options = new BlobUploadOptions();
            options.Tags = new Dictionary<string, string>
            {
                { "ByteSize", $"{sizePadded}" },
                { "TimeStamp", $"{timeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}" },
                { "DevTesting", "true" }
            };
            var id = Guid.NewGuid();
            var filename = $"SimulatorTest_{id:D}_{timeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}.json";
            var blobName = $"{timeStamp.Year}/{timeStamp.Month.ToString().PadLeft(2, '0')}/{timeStamp.Day.ToString().PadLeft(2, '0')}/{filename}";
            await AzureStorageManager.UploadDataAsync(connectionStr, containerName, blobName, data, options);
        }

        private async void findByTags(string connectionStr, string containerName, string query, bool isDelete)
        {
            Console.WriteLine("Find Blob by Tag query: " + query + Environment.NewLine);
            var blobs = await AzureStorageManager.FindBlobsByTagsAsync(connectionStr, containerName, query);
            Console.WriteLine("Found Blob count: " + blobs.Count + Environment.NewLine);
            foreach (var filteredBlob in blobs)
            {
                var tags = String.Join(", ", filteredBlob.Tags.Select(res => "Timestamp with key " + res.Key + ": ID = " + res.Value));
                Console.WriteLine($"BlobIndex result: ContainerName= {filteredBlob.BlobContainerName}, " +
                    $"BlobName= {filteredBlob.BlobName}" + $"BlobTags= {tags}" + Environment.NewLine);
                //delete testing data 
                if(isDelete)
                    await AzureStorageManager.DeleteBlobIfExistsAsync(connectionStr, containerName, filteredBlob.BlobName);
            }
        }
    }
}
