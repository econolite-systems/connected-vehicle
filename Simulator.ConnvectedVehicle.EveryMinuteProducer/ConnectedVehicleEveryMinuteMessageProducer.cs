// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Econolite.Ode.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Simulator.ConnectedVehicleProducer
{
    public class ConnectedVehicleEveryMinuteMessageProducer : BackgroundService
    {

        private readonly ILogger<ConnectedVehicleEveryMinuteMessageProducer> _logger;
        private readonly ClientConfig pConfig;
        private readonly IConfiguration _configuration;
        private readonly string _caLocation;
        private readonly string _clientLocation;
        private Timer? _timer;

        /// <summary>
        /// Build a bare bones producer that doesn't use our shared producer so we can do string, string message key and value
        /// Sends a message to Kafka every minute
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public ConnectedVehicleEveryMinuteMessageProducer(
            ILogger<ConnectedVehicleEveryMinuteMessageProducer> logger,
            IConfiguration config
        )
        {
            _logger = logger;
            _configuration = config;

            _caLocation = "./ca.crt";
            _clientLocation = "./client.crt";

            var ca = _configuration["Kafka:ssl:ca"];
            if (ca != null) SaveCert(_caLocation, ca);

            var client = _configuration["Kafka:ssl:certificate"];
            if (client != null) SaveCert(_clientLocation, client);

            pConfig = BuildClientConfig();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(1);

                _timer = new Timer(async (e) =>
                {
                    await PublishMessages(stoppingToken);
                }, null, startTimeSpan, periodTimeSpan);
                return Task.CompletedTask;
            }, stoppingToken);
        }


        public async Task PublishMessages(CancellationToken cancellationToken)
        {
            try
            {
                //Timer consumer message - doesn't matter just need a string value
                var now = DateTime.UtcNow;
                var _everyMinTopic = _configuration[Consts.TOPICS_CV_EVERY_MIN];
                _logger.LogInformation("Publishing to topic {@}", _everyMinTopic);
                _logger.LogInformation("\"Producing a message\"");

                //Note:  not using our IProducer class, want our own key value definition
                using (var producer = new ProducerBuilder<Null, string>(pConfig).Build())
                {
                    await producer.ProduceAsync(_everyMinTopic, new Message<Null, string> { Value = now.ToString() }, cancellationToken);
                }
            }
            finally
            {
                _logger.LogInformation("\"Finished producing every minute message\"");
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
                result.SslCaLocation = _caLocation;
                result.SslCertificateLocation = _clientLocation;
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

        private void SaveCert(string path, string cert)
        {
            File.WriteAllText(path, Base64Decode(cert));
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
