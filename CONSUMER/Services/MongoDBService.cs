using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CONSUMER.Services;

internal class MongoDBService
{
	private readonly IMongoCollection<BsonDocument> _collection;

	public MongoDBService(IConfiguration configuration)
	{
		var mongoSettings = configuration.GetSection("MongoDB");
		var connectionString =
			$"mongodb://{mongoSettings["Username"]}:{mongoSettings["Password"]}@{mongoSettings["Hostname"]}";
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(mongoSettings["Database"]); // Replace with your actual database name
		_collection =
			database.GetCollection<BsonDocument>(
				mongoSettings["Collection"]); // Replace with your actual collection name
	}

	public void InsertDocument(string json)
	{
		_collection.InsertOne(BsonDocument.Parse(json));
	}
}