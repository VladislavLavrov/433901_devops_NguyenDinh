using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace _12_Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        private readonly IProducer<K, V> _kafkaHandle;

        public KafkaProducerService(IConfiguration config)
        {
            var producerConfig = new ProducerConfig();
            config.Bind("Kafka:ProducerSettings", producerConfig);
            _kafkaHandle = new ProducerBuilder<K, V>(producerConfig).Build();
        }

        public Task ProduceAsync(string topic, Message<K, V> message)
            => _kafkaHandle.ProduceAsync(topic, message);
    }
}