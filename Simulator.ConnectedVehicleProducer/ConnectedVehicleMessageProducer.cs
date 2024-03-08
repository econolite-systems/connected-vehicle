// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Econolite.Ode.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics;

namespace Simulator.ConnectedVehicleProducer
{
    public class ConnectedVehicleMessageProducer : BackgroundService
    {
        private readonly ILogger<ConnectedVehicleMessageProducer> _logger;
        private readonly ClientConfig _pConfig;
        private readonly IConfiguration _configuration;
        private readonly string _caLocation;
        private readonly string _clientLocation;
        private Timer? _timer;
        private readonly IMetricsCounter _loopCounter;
        private readonly IMetricsCounter _totalVehicleCounter;
        private readonly UserEventFactory _userEventFactory;

        /// <summary>
        /// Build a bare bones producer that doesn't use our shared producer because we need to send messages with no Key to kafka to simulate what we'll get from the jpo-ode
        /// Runs every minute and generates 4 messages; one of each type
        /// </summary>
        public ConnectedVehicleMessageProducer(
            ILogger<ConnectedVehicleMessageProducer> logger,
            IConfiguration config,
            IMetricsFactory metricsFactory,
            UserEventFactory userEventFactory
        )
        {
            _logger = logger;
            _configuration = config;

            _caLocation = "./ca.crt";
            _clientLocation = "./client.crt";

            //var ca = _configuration["Kafka:ssl:ca"];
            //if (ca != null) SaveCert(_caLocation, ca);

            //var client = _configuration["Kafka:ssl:certificate"];
            //if (client != null) SaveCert(_clientLocation, client);

            _pConfig = BuildClientConfig();

            _loopCounter = metricsFactory.GetMetricsCounter("Simulator Producer");
            _totalVehicleCounter = metricsFactory.GetMetricsCounter("Simulator Producer Vehicles");
            _userEventFactory = userEventFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(1);

                _timer = new Timer(async _ =>
                {
                    await PublishMessagesAsync(stoppingToken);
                    _loopCounter.Increment();
                }, null, startTimeSpan, periodTimeSpan);
                return Task.CompletedTask;
            });
        }


        public async Task PublishMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
               //BSM message
                var bsmJsonMessage = "{\"metadata\":{\"bsmSource\":\"ECONOLITE TESTING BSM\",\"logFileName\":\"rxMsg_BSM_and_TIM.gz\",\"recordType\":\"rxMsg\",\"securityResultCode\":\"success\",\"receivedMessageDetails\":{\"locationData\":{\"latitude\":\"0\",\"longitude\":\"0\",\"elevation\":\"0\",\"speed\":\"0\",\"heading\":\"0\"},\"rxSource\":\"RV\"},\"payloadType\":\"us.dot.its.jpo.ode.model.OdeBsmPayload\",\"serialId\":{\"streamId\":\"6f41e982-239f-4980-b99f-12b46f083aa1\",\"bundleSize\":393,\"bundleId\":15,\"recordId\":390,\"serialNumber\":1112},\"odeReceivedAt\":\"2022-08-03T18:12:49.523Z\",\"schemaVersion\":6,\"maxDurationTime\":0,\"odePacketID\":\"\",\"odeTimStartDateTime\":\"\",\"recordGeneratedAt\":\"2018-12-06T11:18:12.898Z\",\"recordGeneratedBy\":\"OBU\",\"sanitized\":false},\"payload\":{\"dataType\":\"us.dot.its.jpo.ode.plugin.j2735.J2735Bsm\",\"data\":{\"coreData\":{\"msgCnt\":121,\"id\":\"31325442\",\"secMark\":12609,\"position\":{\"latitude\":41.1592490,\"longitude\":-104.6614297,\"elevation\":1823.6},\"accelSet\":{\"accelLat\":0.00,\"accelLong\":0.00,\"accelVert\":0.00,\"accelYaw\":0.00},\"accuracy\":{\"semiMajor\":2.80,\"semiMinor\":5.15},\"transmission\":\"NEUTRAL\",\"speed\":12.80,\"heading\":90.8125,\"brakes\":{\"wheelBrakes\":{\"leftFront\":false,\"rightFront\":false,\"unavailable\":true,\"leftRear\":false,\"rightRear\":false},\"traction\":\"unavailable\",\"abs\":\"unavailable\",\"scs\":\"unavailable\",\"brakeBoost\":\"unavailable\",\"auxBrakes\":\"unavailable\"},\"size\":{\"width\":239,\"length\":726}},\"partII\":[{\"id\":\"VehicleSafetyExtensions\",\"value\":{\"pathHistory\":{\"crumbData\":[{\"elevationOffset\":-0.3,\"latOffset\":-0.0000043,\"lonOffset\":0.0003624,\"timeOffset\":2.38},{\"elevationOffset\":-1.7,\"latOffset\":-0.0000246,\"lonOffset\":0.0028373,\"timeOffset\":19.51},{\"elevationOffset\":3.7,\"latOffset\":-0.0000420,\"lonOffset\":0.0053293,\"timeOffset\":37.14}]},\"pathPrediction\":{\"confidence\":100.0,\"radiusOfCurve\":0.0}}},{\"id\":\"SpecialVehicleExtensions\",\"value\":{}},{\"id\":\"SupplementalVehicleExtensions\",\"value\":{}}]}}}";
                var bsmStatusTopic = _configuration[Consts.TOPICS_CV_BSM];
                _logger.LogInformation("Publishing to topic {@}", bsmStatusTopic);
                _logger.LogInformation("\"Producing pavement condition status BSM message\"");

                //SPAT message - don't have real message format yet
                //var spatJsonMessage = "{\"metadata\":{\"bsmSource\":\"ECONOLITE TESTING SPAT\",\"logFileName\":\"rxMsg_BSM_and_TIM.gz\",\"recordType\":\"rxMsg\",\"securityResultCode\":\"success\",\"receivedMessageDetails\":{\"locationData\":{\"latitude\":\"0\",\"longitude\":\"0\",\"elevation\":\"0\",\"speed\":\"0\",\"heading\":\"0\"},\"rxSource\":\"RV\"},\"payloadType\":\"us.dot.its.jpo.ode.model.OdeBsmPayload\",\"serialId\":{\"streamId\":\"6f41e982-239f-4980-b99f-12b46f083aa1\",\"bundleSize\":393,\"bundleId\":15,\"recordId\":390,\"serialNumber\":1112},\"odeReceivedAt\":\"2022-08-03T18:12:49.523Z\",\"schemaVersion\":6,\"maxDurationTime\":0,\"odePacketID\":\"\",\"odeTimStartDateTime\":\"\",\"recordGeneratedAt\":\"2018-12-06T11:18:12.898Z\",\"recordGeneratedBy\":\"OBU\",\"sanitized\":false},\"payload\":{\"dataType\":\"us.dot.its.jpo.ode.plugin.j2735.J2735Bsm\",\"data\":{\"coreData\":{\"msgCnt\":121,\"id\":\"31325442\",\"secMark\":12609,\"position\":{\"latitude\":41.1592490,\"longitude\":-104.6614297,\"elevation\":1823.6},\"accelSet\":{\"accelLat\":0.00,\"accelLong\":0.00,\"accelVert\":0.00,\"accelYaw\":0.00},\"accuracy\":{\"semiMajor\":2.80,\"semiMinor\":5.15},\"transmission\":\"NEUTRAL\",\"speed\":12.80,\"heading\":90.8125,\"brakes\":{\"wheelBrakes\":{\"leftFront\":false,\"rightFront\":false,\"unavailable\":true,\"leftRear\":false,\"rightRear\":false},\"traction\":\"unavailable\",\"abs\":\"unavailable\",\"scs\":\"unavailable\",\"brakeBoost\":\"unavailable\",\"auxBrakes\":\"unavailable\"},\"size\":{\"width\":239,\"length\":726}},\"partII\":[{\"id\":\"VehicleSafetyExtensions\",\"value\":{\"pathHistory\":{\"crumbData\":[{\"elevationOffset\":-0.3,\"latOffset\":-0.0000043,\"lonOffset\":0.0003624,\"timeOffset\":2.38},{\"elevationOffset\":-1.7,\"latOffset\":-0.0000246,\"lonOffset\":0.0028373,\"timeOffset\":19.51},{\"elevationOffset\":3.7,\"latOffset\":-0.0000420,\"lonOffset\":0.0053293,\"timeOffset\":37.14}]},\"pathPrediction\":{\"confidence\":100.0,\"radiusOfCurve\":0.0}}},{\"id\":\"SpecialVehicleExtensions\",\"value\":{}},{\"id\":\"SupplementalVehicleExtensions\",\"value\":{}}]}}}";
                //took from the jpo-ode doc: https://github.com/usdot-jpo-ode/jpo-ode/wiki/SPaT-JSON-Schema
                //var spatJsonMessage = "{\"$schema\": \"http://json-schema.org/draft-04/schema#\",\"type\": \"object\",\"properties\": {\"metadata\": {\"type\": \"object\",\"properties\": {\"spatSource\": {\"type\": \"string\"},\"isCertPresent\": {\"type\": \"boolean\"},\"originIp\": {\"type\": \"string\"},\"logFileName\": {\"type\": \"string\"},\"recordType\": {\"type\": \"string\"},\"securityResultCode\": {\"type\": \"string\"},\"receivedMessageDetails\": {\"type\": \"object\",\"properties\": {\"locationData\": {\"type\": \"null\"},\"rxSource\": {\"type\": \"string\"}},\"required\": [\"locationData\",\"rxSource\"]},\"encodings\": {\"type\": \"null\"},\"payloadType\": {\"type\": \"string\"},\"serialId\": {\"type\": \"object\",\"properties\": {\"streamId\": {\"type\": \"string\"},\"bundleSize\": {\"type\": \"integer\"},\"bundleId\": {\"type\": \"integer\"},\"recordId\": {\"type\": \"integer\"},\"serialNumber\": {\"type\": \"integer\"}},\"required\": [\"streamId\",\"bundleSize\",\"bundleId\",\"recordId\",\"serialNumber\"]},\"odeReceivedAt\": {\"type\": \"string\"},\"schemaVersion\": {\"type\": \"integer\"},\"maxDurationTime\": {\"type\": \"integer\"},\"odePacketID\": {\"type\": \"string\"},\"odeTimStartDateTime\": {\"type\": \"string\"},\"recordGeneratedAt\": {\"type\": \"string\"},\"recordGeneratedBy\": {\"type\": \"null\"},\"sanitized\": {\"type\": \"boolean\"}},\"required\": [\"spatSource\",\"isCertPresent\",\"originIp\",\"logFileName\",\"recordType\",\"securityResultCode\",\"receivedMessageDetails\",\"encodings\",\"payloadType\",\"serialId\",\"odeReceivedAt\",\"schemaVersion\",\"maxDurationTime\",\"odePacketID\",\"odeTimStartDateTime\",\"recordGeneratedAt\",\"recordGeneratedBy\",\"sanitized\"]},\"payload\": {\"type\": \"object\",\"properties\": {\"dataType\": {\"type\": \"string\"},\"data\": {\"type\": \"object\",\"properties\": {\"timeStamp\": {\"type\": \"null\"},\"name\": {\"type\": \"null\"},\"intersectionStateList\": {\"type\": \"object\",\"properties\": {\"intersectionStatelist\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"name\": {\"type\": \"null\"},\"id\": {\"type\": \"object\",\"properties\": {\"region\": {\"type\": \"null\"},\"id\": {\"type\": \"integer\"}},\"required\": [\"region\",\"id\"]},\"revision\": {\"type\": \"integer\"},\"status\": {\"type\": \"string\"},\"moy\": {\"type\": \"null\"},\"timeStamp\": {\"type\": \"integer\"},\"enabledLanes\": {\"type\": \"null\"},\"states\": {\"type\": \"object\",\"properties\": {\"movementList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]},{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]},{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]},{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]},{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]},{\"type\": \"object\",\"properties\": {\"movementName\": {\"type\": \"null\"},\"signalGroup\": {\"type\": \"integer\"},\"state_time_speed\": {\"type\": \"object\",\"properties\": {\"movementEventList\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"eventState\": {\"type\": \"string\"},\"timing\": {\"type\": \"object\",\"properties\": {\"startTime\": {\"type\": \"null\"},\"minEndTime\": {\"type\": \"integer\"},\"maxEndTime\": {\"type\": \"integer\"},\"likelyTime\": {\"type\": \"null\"},\"confidence\": {\"type\": \"null\"},\"nextTime\": {\"type\": \"null\"}},\"required\": [\"startTime\",\"minEndTime\",\"maxEndTime\",\"likelyTime\",\"confidence\",\"nextTime\"]},\"speeds\": {\"type\": \"null\"}},\"required\": [\"eventState\",\"timing\",\"speeds\"]}]}},\"required\": [\"movementEventList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"movementName\",\"signalGroup\",\"state_time_speed\",\"maneuverAssistList\"]}]}},\"required\": [\"movementList\"]},\"maneuverAssistList\": {\"type\": \"null\"}},\"required\": [\"name\",\"id\",\"revision\",\"status\",\"moy\",\"timeStamp\",\"enabledLanes\",\"states\",\"maneuverAssistList\"]}]}},\"required\": [\"intersectionStatelist\"]}},\"required\": [\"timeStamp\",\"name\",\"intersectionStateList\"]}},\"required\": [\"dataType\",\"data\"]}},\"required\": [\"metadata\",\"payload\"]}";
                //copied from Asn1DecoderOutput Kafka topic
                //var spatJsonMessage = "{\"metadata\": {    \"payloadType\": \"us.dot.its.jpo.ode.model.OdeAsn1Payload\",    \"serialId\": {      \"streamId\": \"02a4c72a-7682-4618-9cf9-a045ae4d5131\",      \"bundleSize\": \"1\",      \"bundleId\": \"0\",      \"recordId\": \"0\",      \"serialNumber\": \"0\"    },    \"odeReceivedAt\": \"2022-09-07T18:50:05.378Z\",    \"schemaVersion\": \"6\",    \"maxDurationTime\": \"0\",    \"odePacketID\": [],    \"odeTimStartDateTime\": [],    \"recordGeneratedAt\": \"1970-01-01T03:25:34.023Z\",    \"recordGeneratedBy\": \"OBU\",    \"sanitized\": \"false\",    \"logFileName\": \"spatTx_test_binary\",    \"recordType\": \"spatTx\",    \"securityResultCode\": \"success\",    \"receivedMessageDetails\": [],    \"encodings\": [      {        \"elementName\": \"root\",        \"elementType\": \"Ieee1609Dot2Data\",        \"encodingRule\": \"COER\"      },      {        \"elementName\": \"unsecuredData\",        \"elementType\": \"MessageFrame\",        \"encodingRule\": \"UPER\"      }    ],    \"spatSource\": \"V2X\"  },  \"payload\": {    \"dataType\": \"MessageFrame\",    \"data\": {      \"MessageFrame\": {        \"messageId\": \"19\",        \"value\": {          \"SPAT\": {            \"timeStamp\": \"19000\",            \"name\": \"07\",            \"intersections\": {              \"IntersectionState\": {                \"name\": \"07\",                \"id\": {                  \"region\": \"1\",                  \"id\": \"1\"                },                \"revision\": \"1\",                \"status\": \"0000000000000000\",                \"states\": {                  \"MovementState\": {                    \"movementName\": \"07\",                    \"signalGroup\": \"5\",                    \"state-time-speed\": {                      \"MovementEvent\": {                        \"eventState\": {                          \"stop-And-Remain\": []                        },                        \"timing\": {                          \"minEndTime\": \"1200\"                        }                      }                    }                  }                }              }            }          }        }      }    }  }}";
                //copied from Asn1DecoderOutput Kafka topic; tweaked for multiple intersections
                var spatJsonMessage = "{\"metadata\": {\"payloadType\": \"us.dot.its.jpo.ode.model.OdeAsn1Payload\",\"serialId\": {\"streamId\": \"02a4c72a-7682-4618-9cf9-a045ae4d5131\",\"bundleSize\": \"1\",\"bundleId\": \"0\",\"recordId\": \"0\",\"serialNumber\": \"0\"},\"odeReceivedAt\": \"2022-09-07T18:50:05.378Z\",\"schemaVersion\": \"6\",\"maxDurationTime\": \"0\",\"odePacketID\": [],\"odeTimStartDateTime\": [],\"recordGeneratedAt\": \"1970-01-01T03:25:34.023Z\",\"recordGeneratedBy\": \"OBU\",\"sanitized\": \"false\",\"logFileName\": \"spatTx_test_binary\",\"recordType\": \"spatTx\",\"securityResultCode\": \"success\",\"receivedMessageDetails\": [],\"encodings\": [{\"elementName\": \"root\",\"elementType\": \"Ieee1609Dot2Data\",\"encodingRule\": \"COER\"},{\"elementName\": \"unsecuredData\",\"elementType\": \"MessageFrame\",\"encodingRule\": \"UPER\"}],\"spatSource\": \"V2X\"},\"payload\": {\"dataType\": \"MessageFrame\",\"data\": {\"MessageFrame\": {\"messageId\": \"19\",\"value\": {\"SPAT\": {\"timeStamp\": \"19000\",\"name\": \"07\",\"intersections\": [{\"IntersectionState\": {\"name\": \"07\",\"id\": {\"region\": \"1\",\"id\": \"1\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"02\",\"id\": {\"region\": \"1\",\"id\": \"2\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"03\",\"id\": {\"region\": \"1\",\"id\": \"3\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"04\",\"id\": {\"region\": \"2\",\"id\": \"4\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}}]}}}}}}";
                var spatStatusTopic = _configuration[Consts.TOPICS_CV_SPAT];
                _logger.LogInformation("Publishing to topic {@}", spatStatusTopic);
                _logger.LogInformation("\"Producing pavement condition status SPAT message\"");


                //SRM message - don't have real wessage format yet
                //var srmJsonMessage = "{\"metadata\":{\"bsmSource\":\"ECONOLITE TESTING SRM\",\"logFileName\":\"rxMsg_BSM_and_TIM.gz\",\"recordType\":\"rxMsg\",\"securityResultCode\":\"success\",\"receivedMessageDetails\":{\"locationData\":{\"latitude\":\"0\",\"longitude\":\"0\",\"elevation\":\"0\",\"speed\":\"0\",\"heading\":\"0\"},\"rxSource\":\"RV\"},\"payloadType\":\"us.dot.its.jpo.ode.model.OdeBsmPayload\",\"serialId\":{\"streamId\":\"6f41e982-239f-4980-b99f-12b46f083aa1\",\"bundleSize\":393,\"bundleId\":15,\"recordId\":390,\"serialNumber\":1112},\"odeReceivedAt\":\"2022-08-03T18:12:49.523Z\",\"schemaVersion\":6,\"maxDurationTime\":0,\"odePacketID\":\"\",\"odeTimStartDateTime\":\"\",\"recordGeneratedAt\":\"2018-12-06T11:18:12.898Z\",\"recordGeneratedBy\":\"OBU\",\"sanitized\":false},\"payload\":{\"dataType\":\"us.dot.its.jpo.ode.plugin.j2735.J2735Bsm\",\"data\":{\"coreData\":{\"msgCnt\":121,\"id\":\"31325442\",\"secMark\":12609,\"position\":{\"latitude\":41.1592490,\"longitude\":-104.6614297,\"elevation\":1823.6},\"accelSet\":{\"accelLat\":0.00,\"accelLong\":0.00,\"accelVert\":0.00,\"accelYaw\":0.00},\"accuracy\":{\"semiMajor\":2.80,\"semiMinor\":5.15},\"transmission\":\"NEUTRAL\",\"speed\":12.80,\"heading\":90.8125,\"brakes\":{\"wheelBrakes\":{\"leftFront\":false,\"rightFront\":false,\"unavailable\":true,\"leftRear\":false,\"rightRear\":false},\"traction\":\"unavailable\",\"abs\":\"unavailable\",\"scs\":\"unavailable\",\"brakeBoost\":\"unavailable\",\"auxBrakes\":\"unavailable\"},\"size\":{\"width\":239,\"length\":726}},\"partII\":[{\"id\":\"VehicleSafetyExtensions\",\"value\":{\"pathHistory\":{\"crumbData\":[{\"elevationOffset\":-0.3,\"latOffset\":-0.0000043,\"lonOffset\":0.0003624,\"timeOffset\":2.38},{\"elevationOffset\":-1.7,\"latOffset\":-0.0000246,\"lonOffset\":0.0028373,\"timeOffset\":19.51},{\"elevationOffset\":3.7,\"latOffset\":-0.0000420,\"lonOffset\":0.0053293,\"timeOffset\":37.14}]},\"pathPrediction\":{\"confidence\":100.0,\"radiusOfCurve\":0.0}}},{\"id\":\"SpecialVehicleExtensions\",\"value\":{}},{\"id\":\"SupplementalVehicleExtensions\",\"value\":{}}]}}}";
                //took from the jpo-ode doc: https://github.com/usdot-jpo-ode/jpo-ode/wiki/SRM-JSON-Schema
                //var srmJsonMessage = "{\"$schema\": \"http://json-schema.org/draft-04/schema#\",\"type\": \"object\",\"properties\": {\"metadata\": {\"type\": \"object\",\"properties\": {\"originIp\": {\"type\": \"string\"},\"srmSource\": {\"type\": \"string\"},\"logFileName\": {\"type\": \"string\"},\"recordType\": {\"type\": \"string\"},\"securityResultCode\": {\"type\": \"string\"},\"receivedMessageDetails\": {\"type\": \"object\",\"properties\": {\"locationData\": {\"type\": \"null\"},\"rxSource\": {\"type\": \"string\"}},\"required\": [\"locationData\",\"rxSource\"]},\"encodings\": {\"type\": \"null\"},\"payloadType\": {\"type\": \"string\"},\"serialId\": {\"type\": \"object\",\"properties\": {\"streamId\": {\"type\": \"string\"},\"bundleSize\": {\"type\": \"integer\"},\"bundleId\": {\"type\": \"integer\"},\"recordId\": {\"type\": \"integer\"},\"serialNumber\": {\"type\": \"integer\"}},\"required\": [\"streamId\",\"bundleSize\",\"bundleId\",\"recordId\",\"serialNumber\"]},\"odeReceivedAt\": {\"type\": \"string\"},\"schemaVersion\": {\"type\": \"integer\"},\"maxDurationTime\": {\"type\": \"integer\"},\"odePacketID\": {\"type\": \"string\"},\"odeTimStartDateTime\": {\"type\": \"string\"},\"recordGeneratedAt\": {\"type\": \"string\"},\"recordGeneratedBy\": {\"type\": \"null\"},\"sanitized\": {\"type\": \"boolean\"}},\"required\": [\"originIp\",\"srmSource\",\"logFileName\",\"recordType\",\"securityResultCode\",\"receivedMessageDetails\",\"encodings\",\"payloadType\",\"serialId\",\"odeReceivedAt\",\"schemaVersion\",\"maxDurationTime\",\"odePacketID\",\"odeTimStartDateTime\",\"recordGeneratedAt\",\"recordGeneratedBy\",\"sanitized\"]},\"payload\": {\"type\": \"object\",\"properties\": {\"dataType\": {\"type\": \"string\"},\"data\": {\"type\": \"object\",\"properties\": {\"timeStamp\": {\"type\": \"integer\"},\"second\": {\"type\": \"integer\"},\"sequenceNumber\": {\"type\": \"integer\"},\"requests\": {\"type\": \"object\",\"properties\": {\"signalRequestPackage\": {\"type\": \"array\",\"items\": [{\"type\": \"object\",\"properties\": {\"request\": {\"type\": \"object\",\"properties\": {\"id\": {\"type\": \"object\",\"properties\": {\"region\": {\"type\": \"integer\"},\"id\": {\"type\": \"integer\"}},\"required\": [\"region\",\"id\"]},\"requestID\": {\"type\": \"integer\"},\"requestType\": {\"type\": \"string\"},\"inBoundLane\": {\"type\": \"object\",\"properties\": {\"lane\": {\"type\": \"integer\"},\"approach\": {\"type\": \"integer\"},\"connection\": {\"type\": \"integer\"}},\"required\": [\"lane\",\"approach\",\"connection\"]},\"outBoundLane\": {\"type\": \"object\",\"properties\": {\"lane\": {\"type\": \"integer\"},\"approach\": {\"type\": \"integer\"},\"connection\": {\"type\": \"integer\"}},\"required\": [\"lane\",\"approach\",\"connection\"]}},\"required\": [\"id\",\"requestID\",\"requestType\",\"inBoundLane\"]},\"minute\": {\"type\": \"null\"},\"second\": {\"type\": \"null\"},\"duration\": {\"type\": \"integer\"}},\"required\": [\"request\"]}]}},\"required\": [\"signalRequestPackage\"]},\"requestor\": {\"type\": \"object\",\"properties\": {\"id\": {\"type\": \"object\",\"properties\": {\"entityID\": {\"type\": \"string\"},\"stationID\": {\"type\": \"integer\"}},\"required\": [\"entityID\",\"stationID\"]},\"type\": {\"type\": \"object\",\"properties\": {\"role\": {\"type\": \"string\"},\"subrole\": {\"type\": \"string\"},\"request\": {\"type\": \"string\"},\"iso3883\": {\"type\": \"integer\"},\"hpmsType\": {\"type\": \"string\"}},\"required\": [\"role\"]},\"position\": {\"type\": \"object\",\"properties\": {\"position\": {\"type\": \"object\",\"properties\": {\"lat\": {\"type\": \"integer\"},\"lon\": {\"type\": \"integer\"},\"elevation\": {\"type\": \"integer\"}},\"required\": [\"lat\",\"lon\"]},\"heading\": {\"type\": \"integer\"},\"speed\": {\"type\": \"object\",					\"properties\": {					\"transmisson\": {						\"type\": \"string\"						},					\"speed\": {					\"type\": \"integer\"					}					},					\"required\": [\"transmisson\",\"speed\"]}},\"required\": [\"position\"]},\"name\": {\"type\": \"string\"},\"routeName\": {\"type\": \"string\"},\"transitStatus\": {\"type\": \"string\"},\"transitOccupancy\": {\"type\": \"string\"},\"transitSchedule\": {\"type\": \"integer\"}},\"required\": [\"id\"]}},\"required\": [\"requestor\"]}},\"required\": [\"dataType\",\"data\"]}},\"required\": [\"metadata\",\"payload\"]}";
                //copied from SPAT's Asn1DecoderOutput Kafka topic; tweaked for multiple intersections
                var srmJsonMessage = "{\"metadata\": {\"payloadType\": \"us.dot.its.jpo.ode.model.OdeAsn1Payload\",\"serialId\": {\"streamId\": \"02a4c72a-7682-4618-9cf9-a045ae4d5131\",\"bundleSize\": \"1\",\"bundleId\": \"0\",\"recordId\": \"0\",\"serialNumber\": \"0\"},\"odeReceivedAt\": \"2022-09-07T18:50:05.378Z\",\"schemaVersion\": \"6\",\"maxDurationTime\": \"0\",\"odePacketID\": [],\"odeTimStartDateTime\": [],\"recordGeneratedAt\": \"1970-01-01T03:25:34.023Z\",\"recordGeneratedBy\": \"OBU\",\"sanitized\": \"false\",\"logFileName\": \"spatTx_test_binary\",\"recordType\": \"spatTx\",\"securityResultCode\": \"success\",\"receivedMessageDetails\": [],\"encodings\": [{\"elementName\": \"root\",\"elementType\": \"Ieee1609Dot2Data\",\"encodingRule\": \"COER\"},{\"elementName\": \"unsecuredData\",\"elementType\": \"MessageFrame\",\"encodingRule\": \"UPER\"}],\"spatSource\": \"V2X\"},\"payload\": {\"dataType\": \"MessageFrame\",\"data\": {\"MessageFrame\": {\"messageId\": \"19\",\"value\": {\"SPAT\": {\"timeStamp\": \"19000\",\"name\": \"07\",\"intersections\": [{\"IntersectionState\": {\"name\": \"07\",\"id\": {\"region\": \"1\",\"id\": \"1\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"02\",\"id\": {\"region\": \"1\",\"id\": \"2\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"03\",\"id\": {\"region\": \"1\",\"id\": \"3\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}},{\"IntersectionState\": {\"name\": \"04\",\"id\": {\"region\": \"2\",\"id\": \"4\"},\"revision\": \"1\",\"status\": \"0000000000000000\",\"states\": {\"MovementState\": {\"movementName\": \"07\",\"signalGroup\": \"5\",\"state-time-speed\": {\"MovementEvent\": {\"eventState\": {\"stop-And-Remain\": []},\"timing\": {\"minEndTime\": \"1200\"}}}}}}}]}}}}}}";
                var srmStatusTopic = _configuration[Consts.TOPICS_CV_SRM];
                _logger.LogInformation("Publishing to topic {@}", srmStatusTopic);
                _logger.LogInformation("\"Producing pavement condition status SRM message\"");


                //TIM message
                var timJsonMessage = "{\"metadata\":{\"securityResultCode\":\"success\",\"recordGeneratedBy\":\"RSU\",\"schemaVersion\":6,\"odePacketID\":\"\",\"sanitized\":false,\"recordGeneratedAt\":\"2018-11-14T16:00:45.588Z\",\"recordType\":\"rxMsg\",\"maxDurationTime\":0,\"odeTimStartDateTime\":\"\",\"receivedMessageDetails\":{\"locationData\":{\"elevation\":1810,\"heading\":297.8,\"latitude\":41.1519349,\"speed\":0.02,\"longitude\":-104.656723},\"rxSource\":\"RSU\"},\"payloadType\":\"us.dot.its.jpo.ode.model.OdeTimPayload\",\"serialId\":{\"recordId\":63,\"serialNumber\":1178,\"streamId\":\"6f41e982-239f-4980-b99f-12b46f083aa1\",\"bundleSize\":243,\"bundleId\":16},\"logFileName\":\"rxMsg_TIM_GeneratedBy_RSU.gz\",\"odeReceivedAt\":\"2022-08-03T18:51:59.793Z\"},\"payload\":{\"data\":{\"MessageFrame\":{\"messageId\":31,\"value\":{\"TravelerInformation\":{\"timeStamp\":449088,\"packetID\":\"000000000000086B2C\",\"urlB\":null,\"dataFrames\":{\"TravelerDataFrame\":{\"regions\":{\"GeographicalPath\":{\"closedPath\":{\"false\":\"\"},\"anchor\":{\"lat\":411492107,\"long\":-1046778055},\"name\":\"westbound_I-80_369.0_368.0_RSU-10.145.1.100_RW_4453\",\"laneWidth\":32700,\"directionality\":{\"both\":\"\"},\"description\":{\"path\":{\"offset\":{\"xy\":{\"nodes\":{\"NodeXY\":[{\"delta\":{\"node-LatLon\":{\"lon\":-1046793931,\"lat\":411483939}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046809732,\"lat\":411475785}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046825568,\"lat\":411467670}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046841365,\"lat\":411459513}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046857165,\"lat\":411451359}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046872977,\"lat\":411443219}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046888778,\"lat\":411435066}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046904584,\"lat\":411426919}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046920416,\"lat\":411418801}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046936213,\"lat\":411410646}}},{\"delta\":{\"node-LatLon\":{\"lon\":-1046952038,\"lat\":411402520}}}]}}},\"scale\":0}},\"id\":{\"id\":0,\"region\":0},\"direction\":\"0000000000100000\"}},\"duratonTime\":1440,\"sspMsgRights1\":1,\"sspMsgRights2\":1,\"startYear\":2018,\"msgId\":{\"roadSignID\":{\"viewAngle\":1111111111111111,\"mutcdCode\":{\"warning\":\"\"},\"position\":{\"lat\":411492107,\"long\":-1046778055}}},\"priority\":5,\"content\":{\"advisory\":{\"SEQUENCE\":[{\"item\":{\"itis\":777}},{\"item\":{\"itis\":13580}}]}},\"url\":null,\"sspTimRights\":1,\"sspLocationRights\":1,\"frameType\":{\"advisory\":\"\"},\"startTime\":448260}},\"msgCnt\":1}}}},\"dataType\":\"TravelerInformation\"}}";
                var timStatusTopic = _configuration[Consts.TOPICS_CV_TIM];
                _logger.LogInformation("Publishing to topic {@}", timStatusTopic);
                _logger.LogInformation("\"Producing pavement condition status TIM message\"");


                //Note:  not using our IProducer class because we don't want the message header "type"; we need that to come through as "unspecified"
                using (var producer = new ProducerBuilder<Null, string>(_pConfig).Build())
                {
                    await producer.ProduceAsync(bsmStatusTopic, new Message<Null, string> { Value = bsmJsonMessage }, cancellationToken);
                    _totalVehicleCounter.Increment();
                    await producer.ProduceAsync(spatStatusTopic, new Message<Null, string> { Value = spatJsonMessage }, cancellationToken);
                    _totalVehicleCounter.Increment();
                    await producer.ProduceAsync(srmStatusTopic, new Message<Null, string> { Value = srmJsonMessage }, cancellationToken);
                    _totalVehicleCounter.Increment();
                    await producer.ProduceAsync(timStatusTopic, new Message<Null, string> { Value = timJsonMessage }, cancellationToken);
                    _totalVehicleCounter.Increment();
                    
                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Sent {0} simulated vehicle messages.", 4)));
                }
            }
            finally
            {
                _logger.LogInformation("\"Finished producing connected vehicle messages\"");
            }
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }

        //Copied supporting stuff out of Econolite.Ode.Messaging.Producer.cs
        private ClientConfig BuildClientConfig(IEnumerable<KeyValuePair<string, string>>? config = null)
        {
            var result = new ClientConfig
            {
                BootstrapServers = _configuration[Consts.KAFKA_BOOTSTRAP_SERVERS]
            };

            // Sets the Security Protocol
            if (_configuration[Consts.KAFKA_SECURITY_PROTOCOL] == "SASL_SSL")
            {
                result.SecurityProtocol = SecurityProtocol.SaslSsl;
                //result.SslCaLocation = _caLocation;
                //result.SslCertificateLocation = _clientLocation;
            }
            else if (_configuration[Consts.KAFKA_SECURITY_PROTOCOL] == "SASL_PLAIN")
            {
                result.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            }
            else
            {
                result.SecurityProtocol = SecurityProtocol.Plaintext;
            }

            // Sets the Sasl Mechanism
            if (_configuration[Consts.KAFKA_SASL_MECHANISM] == "SCRAM-SHA-512")
            {
                result.SaslMechanism = SaslMechanism.ScramSha512;
                result.SaslUsername = _configuration[Consts.KAFKA_SASL_USERNAME];
                result.SaslPassword = _configuration[Consts.KAFKA_SASL_PASSWORD];
            }

            if (config?.Any() ?? false)
                foreach (var item in config)
                    result.Set(item.Key, item.Value);

            return result;
        }

        //private void SaveCert(string path, string cert)
        //{
        //    File.WriteAllText(path, Base64Decode(cert));
        //}

        //private string Base64Decode(string base64EncodedData)
        //{
        //    var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        //    return Encoding.UTF8.GetString(base64EncodedBytes);
        //}
    }
}
