using System.Text;
using RabbitMQ.Client;

namespace PRODUCER.Services;

public class RabbitMQProducerService : IDisposable
{
	private static IConnection? _connection;
	private static IModel? _channel;
	private readonly ConnectionFactory _connectionFactory;

	public RabbitMQProducerService(string hostname, string username, string password, IConnection? connection,
		IModel? channel)
	{
		_connection = connection;
		_channel = channel;
		_connectionFactory = new ConnectionFactory
		{
			HostName = hostname,
			UserName = username,
			Password = password
		};

		Task.Run(Connect).Wait();
	}

	public void Dispose()
	{
		_channel?.Close();
		_connection?.Close();
	}

	private void Connect()
	{
		_connection = _connectionFactory.CreateConnection();
		_channel = _connection.CreateModel();
	}

	public static async Task SendMessageAsync(string queueName, string message)
	{
		await Task.Run(() =>
		{
			_channel?.QueueDeclare(queueName, false, false, false,
				null);
			var body = Encoding.UTF8.GetBytes(message);
			_channel?.BasicPublish("", queueName, null, body);
		});
	}
}