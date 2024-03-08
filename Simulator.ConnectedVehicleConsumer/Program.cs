// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace Simulator.ConnectedVehicleConsumer
{
    /// <summary>
    /// Bare bones consumer that will consume messages from a kafka server and topic; didn't want to hook in our typical consumer because i wanted to make sure we could work with the jpo ode and its producer in isolation
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //bare bones consumer listening to a specified server and a topic

                var topic = "topic.OdeBsmJson";
                //var topic = "topic.OdeTimJson";
                //var topic = "topic.OdeSpatJson";
                //var topic = "topic.OdeSrmJson";


                var config = new ConsumerConfig
                {
                    BootstrapServers = "kafka.cosysdev.com:9092",
                    //BootstrapServers="localhost:9092"
                    GroupId = "connectedVehicle.logging",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                Console.WriteLine("Starting Connected Vehicle Consumer");
                Console.WriteLine($"Listening to '{topic}' ");

                //TODO:  ko - string seems to work here. doesn't even have to be "string?" and it doesn't have to be "Ignore" 
                using (var consumer = new ConsumerBuilder<string, string>(config).Build())
                {
                   consumer.Subscribe(topic);

                    var cancelled = false;
                    CancellationTokenSource cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true; // prevent the process from terminating.
                        cancelled = true;
                        cts.Cancel();
                    };

                    while (!cancelled)
                    {
                        var consumeResult = consumer.Consume(cts.Token);

                        // handle consumed message.
                        Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' ");

                    }

                    consumer.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0}" , ex));
            }
        }
    }
}
