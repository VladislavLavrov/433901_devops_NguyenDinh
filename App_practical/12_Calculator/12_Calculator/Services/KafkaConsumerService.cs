using Confluent.Kafka;
using _12_Calculator.Data;
using _12_Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace _12_Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string? _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var consumerConfig = new ConsumerConfig();
            config.Bind("Kafka:ConsumerSettings", consumerConfig);
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _topic = config["Kafka:TopicName"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _kafkaConsumer.Consume(stoppingToken);
                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(consumeResult.Message.Value);

                    if (inputData != null)
                    {
                       
                        var num1 = double.Parse(inputData.Operand_1 ?? "0");
                        var num2 = double.Parse(inputData.Operand_2 ?? "0");
                        double result = inputData.Type_operation switch
                        {
                            "Add" => num1 + num2,
                            "Subtract" => num1 - num2,
                            "Multiply" => num1 * num2,
                            "Divide" => num1 / num2,
                            _ => 0
                        };
                        inputData.Result = result.ToString();

                        
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                        context.DataInputVariants.Add(inputData);
                        await context.SaveChangesAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}
