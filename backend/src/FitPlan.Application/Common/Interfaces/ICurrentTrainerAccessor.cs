namespace FitPlan.Application.Common.Interfaces;

public interface ICurrentTrainerAccessor
{
    string? TrainerId { get; }
    string? UserId { get; }
    bool IsTrainer { get; }
    bool IsClient { get; }
    string? ClientTrainerId { get; }
}
