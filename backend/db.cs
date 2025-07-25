using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Email { get; set; }
    public string Name { get; set; }
}

public class MongoDbService
{
    private readonly IMongoCollection<User> _users;

    public MongoDbService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:Database"]);
        _users = database.GetCollection<User>("users");
    }

    public async Task<User> GetOrCreateUserAsync(string email, string name)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User { Email = email, Name = name };
            await _users.InsertOneAsync(user);
        }
        return user;
    }
}
