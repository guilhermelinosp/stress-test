﻿using CONSUMER.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", false, true)
	.AddUserSecrets<Program>()
	.Build();

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection, configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();
var rabbitMQConsumerService = serviceProvider.GetService<RabbitMQConsumerService>();
rabbitMQConsumerService!.StartConsumingAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
	services.AddSingleton(configuration);
	services.AddSingleton<MongoDBService>();
	services.AddSingleton<RabbitMQConsumerService>();
}