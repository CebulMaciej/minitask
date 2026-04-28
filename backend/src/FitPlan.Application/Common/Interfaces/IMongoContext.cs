using MongoDB.Driver;
using FitPlan.Domain.Common;

namespace FitPlan.Application.Common.Interfaces;

public interface IMongoContext
{
    IMongoCollection<T> GetCollection<T>(string name) where T : BaseEntity;
}
