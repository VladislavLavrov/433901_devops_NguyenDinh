using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace _12_Calculator.Services
{
    public class KafkaProducerHandler : IDisposable
    {
        IProducer<byte[], byte[]> kafkaProducer;

        public KafkaProducerHandler(IConfiguration config)
        {
            var producerConfig = new ProducerConfig();
            config.Bind("Kafka:ProducerSettings", producerConfig);
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
        }

        public Task ProduceAsync(string topic, Message<Null, string> message)
            => kafkaProducer.ProduceAsync(topic, new Message<byte[], byte[]>
            {
                Key = null!,
                Value = System.Text.Encoding.UTF8.GetBytes(message.Value)
            });

        public void Dispose()
        {
            kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            kafkaProducer.Dispose();
        }
    }
}
