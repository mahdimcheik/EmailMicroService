using Confluent.Kafka;
using EmailMicroService.Services;
using EmailMicroService.Utilities;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly EmailService _emailService;
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    public KafkaConsumerHostedService(
        EmailService emailService,
        ILogger<KafkaConsumerHostedService> logger
    )
    {
        _emailService = emailService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = EnvironmentVaraibles.KafkaBootstrapServers,
            GroupId = EnvironmentVaraibles.KafkaGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        var topic = EnvironmentVaraibles.KafkaTopic;

        return Task.Run(
            () =>
            {
                using var consumer = new ConsumerBuilder<string, string>(config).Build();
                consumer.Subscribe(topic);
                _logger.LogInformation(
                    "Subscribed to Kafka topic '{Topic}' on {BootstrapServers}",
                    topic,
                    config.BootstrapServers
                );

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = consumer.Consume(stoppingToken);
                            if (result?.Message is null)
                            {
                                continue;
                            }

                            _logger.LogInformation(
                                "Consumed message from {Topic} [{Partition}] at offset {Offset}",
                                topic,
                                result.Partition.Value,
                                result.Offset.Value
                            );

                            _logger.LogInformation(
                                "Consumed message: {Message}",
                                result.Message.Value
                            );
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Error consuming Kafka message");
                        }
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    consumer.Close();
                }
            },
            stoppingToken
        );
    }
}
