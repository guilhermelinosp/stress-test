using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using PRODUCER.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var host = builder.Host;
var services = builder.Services;
var configuration = builder.Configuration;
var web = builder.WebHost;

// Configure Kestrel from appsettings.json with validation
web.ConfigureKestrel((context, serverOptions) =>
{
	serverOptions.Configure(context.Configuration.GetSection("Kestrel"));
});

services.AddControllers();
services.AddHealthChecks();

services.AddSingleton(new RabbitMQProducerService(configuration["RabbitMQ:Hostname"]!,
	configuration["RabbitMQ:Username"]!, configuration["RabbitMQ:Password"]!, null, null));

// HOST
host.UseSerilog((host, services, logging) =>
{
	logging
		.MinimumLevel.Warning()
		.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
		.MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Information)
		.WriteTo.Async(write =>
		{
			write.Console(
				outputTemplate:
				"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {ClientIp}  {ThreadId} {Message:lj} <p:{SourceContext}>{NewLine}{Exception}",
				restrictedToMinimumLevel: LogEventLevel.Information);
			write.File("logs/.log", rollingInterval: RollingInterval.Day,
				outputTemplate:
				"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {ClientIp}  {ThreadId} {Message:lj} <p:{SourceContext}>{NewLine}{Exception}",
				restrictedToMinimumLevel: LogEventLevel.Warning);
		})
		.ReadFrom.Configuration(host.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext();
});
// MIDDLEWARE
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
} else
{
	app.UseExceptionHandler("/error");
	app.UseHsts();
}
// Map controllers
app.UseRouting();
app.MapControllers();
app.UseSerilogRequestLogging();

app.MapHealthChecks("/health", new HealthCheckOptions
{
	Predicate = _ => true,
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/", async context =>
{
	try
	{
		await context.Response.WriteAsync("Client is running!"); // Sample endpoint
	}
	catch (Exception ex)
	{
		Log.Error(ex, "Error processing request at /");
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("An error occurred processing your request.");
	}
});

app.MapGet("/api/client", async context =>
{
	try
	{
		var client = new
		{
			context.Connection.Id,
			RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "IP não encontrado",
			context.Connection.RemotePort,
			LocalIpAddress = context.Connection.LocalIpAddress?.ToString() ?? "IP não encontrado",
			context.Connection.LocalPort,
			context.Request.Query,
			Headers = context.Request.Headers.ToDictionary(header => header.Key, header => header.Value.ToString()),
			context.Request.Method,
			context.Request.Path,
			context.Request.Protocol
		};

		var clientJson = JsonConvert.SerializeObject(client, Formatting.Indented);
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsync(clientJson);

		await RabbitMQProducerService.SendMessageAsync(configuration["RabbitMQ:Queue"]!, clientJson);
	}
	catch (Exception ex)
	{
		Log.Error(ex, "Error processing request at /api/client");
		context.Response.StatusCode = 500;
		await context.Response.WriteAsync("An error occurred processing your request.");
	}
});

await app.RunAsync();