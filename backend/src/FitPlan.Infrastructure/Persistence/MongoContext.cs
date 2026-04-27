using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Common;
using FitPlan.Domain.Entities;

namespace FitPlan.Infrastructure.Persistence;

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _db;

    static MongoContext() => RegisterBsonClassMaps();

    public MongoContext(IConfiguration config)
    {
        var connStr = config["MongoDB:ConnectionString"] ?? throw new InvalidOperationException("MongoDB:ConnectionString not configured.");
        var dbName = config["MongoDB:DatabaseName"] ?? "fitplan";
        var client = new MongoClient(connStr);
        _db = client.GetDatabase(dbName);
    }

    private static readonly object _classMapLock = new();

    private static void RegisterBsonClassMaps()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(WorkoutSession))) return;
        lock (_classMapLock)
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(WorkoutSession))) return;

            // AutoMap() on WorkoutSession would detect the 4-arg public constructor, create a creator
            // map for it, and fail at Freeze() because the 'exercises' parameter has no matching BSON
            // member (_exercises is a private field). We skip AutoMap and map everything explicitly so
            // that only the 0-arg private constructor is used as the creator.
            BsonClassMap.RegisterClassMap<WorkoutSession>(cm =>
            {
                cm.MapMember(ws => ws.TrainerId);
                cm.MapMember(ws => ws.ClientId);
                cm.MapMember(ws => ws.ScheduledAt);
                cm.MapMember(ws => ws.Status);
                cm.MapMember(ws => ws.StartedAt);
                cm.MapMember(ws => ws.CompletedAt);
                cm.MapField("_exercises").SetElementName("exercises");
                cm.MapCreator(() => (WorkoutSession)Activator.CreateInstance(typeof(WorkoutSession), nonPublic: true)!);
            });
        }
    }

    public IMongoCollection<T> GetCollection<T>(string name) where T : BaseEntity
        => _db.GetCollection<T>(name);
}
