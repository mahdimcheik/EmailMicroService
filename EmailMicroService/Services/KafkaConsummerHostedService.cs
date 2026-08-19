using System.Text.Json;
using Confluent.Kafka;
using EmailMicroService.Models;
using EmailMicroService.Services;
using EmailMicroService.Utilities;

public class KafkaConsumerHostedService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

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
            EnableAutoCommit = false,
        };

        var topic = EnvironmentVaraibles.KafkaTopic;

        return Task.Run(
            async () =>
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
                        ConsumeResult<string, string>? result = null;
                        try
                        {
                            result = consumer.Consume(stoppingToken);
                            if (result?.Message is null)
                            {
                                continue;
                            }

                            MailInfos? mailInfos;
                            try
                            {
                                mailInfos = JsonSerializer.Deserialize<MailInfos>(
                                    result.Message.Value,
                                    JsonOptions
                                );
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(
                                    ex,
                                    "Could not parse Kafka message to MailInfos at offset {Offset}: {Message}",
                                    result.Offset.Value,
                                    result.Message.Value
                                );
                                continue;
                            }

                            if (mailInfos is null)
                            {
                                _logger.LogError(
                                    "Kafka message parsed to null MailInfos at offset {Offset}: {Message}",
                                    result.Offset.Value,
                                    result.Message.Value
                                );
                                continue;
                            }

                            await _emailService.SendMailAsync(mailInfos);

                            consumer.Commit(result);
                            _logger.LogInformation(
                                "Mail sent and offset committed for {Topic} [{Partition}] at offset {Offset}",
                                topic,
                                result.Partition.Value,
                                result.Offset.Value
                            );
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Error consuming Kafka message");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error sending mail for Kafka message at offset {Offset}",
                                result?.Offset.Value
                            );
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
