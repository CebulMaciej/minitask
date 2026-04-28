namespace FitPlan.Application.Common.Interfaces;

public interface ITenantScopedRequest
{
    string TrainerId { get; }
}
