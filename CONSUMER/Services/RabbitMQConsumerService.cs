using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CONSUMER.Services;

internal class RabbitMQConsumerService(IConfiguration configuration, MongoDBService mongoDBService)
{
	public void StartConsumingAsync()
	{
		var rabbitSettings = configuration.GetSection("RabbitMQ");
		var factory = new ConnectionFactory
		{
			HostName = rabbitSettings["Hostname"],
			UserName = rabbitSettings["Username"],
			Password = rabbitSettings["Password"]
		};
		using var connection = factory.CreateConnection();
		using var channel = connection.CreateModel();

		channel.QueueDeclare(rabbitSettings["Queue"], false, false, false,
			null);

		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (model, ea) =>
		{
			var body = ea.Body.ToArray();
			var message = Encoding.UTF8.GetString(body);
			mongoDBService.InsertDocument(message);
		};

		channel.BasicConsume(rabbitSettings["Queue"], true, consumer);

		Console.WriteLine("Press [enter] to exit.");
		Console.ReadLine();
	}
}