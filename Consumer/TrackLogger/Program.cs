using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrackLogger.Configuration;
using TrackLogger.Dtos;
using TrackLogger.LogServices;

string FILE_NAME = "config.json";

var builder = new ConfigurationBuilder()
    .AddJsonFile(FILE_NAME)
    .Build();

var logConfiguration = new Logconfiguration();
builder.Bind(Logconfiguration.Position, logConfiguration);

var RabbitMQConfiguration = new RabbitMQConfiguration();
builder.Bind(RabbitMQConfiguration.Position, RabbitMQConfiguration);

Task.Delay(10000).Wait();

var factory = new ConnectionFactory { 
    HostName = RabbitMQConfiguration.HostName, 
    Port=RabbitMQConfiguration.Port, 
    UserName=RabbitMQConfiguration.Username, 
    Password= RabbitMQConfiguration.Password};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: RabbitMQConfiguration.QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    var trackingDetail = JsonSerializer.Deserialize<TrackingDetailsDto>(message);

   await new VisitLogger().LogAsync(logConfiguration.LogFilePath, trackingDetail!);
};

channel.BasicConsume(queue: RabbitMQConfiguration.QueueName,
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();