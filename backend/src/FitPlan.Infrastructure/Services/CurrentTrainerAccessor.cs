using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Enums;

namespace FitPlan.Infrastructure.Services;

public class CurrentTrainerAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentTrainerAccessor
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public bool IsTrainer => User?.FindFirst("userType")?.Value == UserType.Trainer.ToString();
    public bool IsClient => User?.FindFirst("userType")?.Value == UserType.Client.ToString();
    public string? TrainerId => IsTrainer ? UserId : null;
    public string? ClientTrainerId => User?.FindFirst("trainerId")?.Value;
}
