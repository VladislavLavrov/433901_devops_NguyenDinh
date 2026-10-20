using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace _12_Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        private readonly string? _topic;
        private readonly IProducer<K, V> _kafkaHandle;

        public KafkaProducerService(IConfiguration config)
        {
            var producerConfig = new ProducerConfig();
            config.Bind("Kafka:ProducerSettings", producerConfig);
            var handle = new ProducerBuilder<K, V>(producerConfig).Build();
            _kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
        }

        public Task ProduceAsync(string topic, Message<K, V> message)
            => _kafkaHandle.ProduceAsync(topic, message);
    }
}
