using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Application.Common.Behaviors;

public class TenantValidationBehavior<TRequest, TResponse>(ICurrentTrainerAccessor trainerAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is ITenantScopedRequest)
        {
            if (!trainerAccessor.IsTrainer || string.IsNullOrEmpty(trainerAccessor.TrainerId))
                throw new ForbiddenException("Trainer context required.");
        }
        return next();
    }
}
