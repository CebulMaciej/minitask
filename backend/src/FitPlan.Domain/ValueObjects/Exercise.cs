namespace FitPlan.Domain.ValueObjects;

public class Exercise
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Name { get; private set; }
    public int Order { get; private set; }
    public int Sets { get; private set; }
    public int Reps { get; private set; }
    public double? TargetWeight { get; private set; }
    public int? ActualSets { get; private set; }
    public int? ActualReps { get; private set; }
    public double? ActualWeight { get; private set; }
    public bool UnexpectedProgress { get; private set; }
    public string? Notes { get; private set; }

    private Exercise() { Name = string.Empty; }

    public Exercise(string name, int order, int sets, int reps, double? targetWeight = null)
    {
        Name = name;
        Order = order;
        Sets = sets;
        Reps = reps;
        TargetWeight = targetWeight;
    }

    public Exercise LogActuals(int? actualSets, int? actualReps, double? actualWeight, string? notes = null)
    {
        var updated = Clone();
        updated.ActualSets = actualSets;
        updated.ActualReps = actualReps;
        updated.ActualWeight = actualWeight;
        updated.Notes = notes;
        updated.UnexpectedProgress = ComputeUnexpectedProgress(actualSets, actualReps, actualWeight);
        return updated;
    }

    private bool ComputeUnexpectedProgress(int? actualSets, int? actualReps, double? actualWeight)
    {
        if (actualWeight.HasValue && TargetWeight.HasValue && actualWeight > TargetWeight) return true;
        if (actualReps.HasValue && actualReps > Reps) return true;
        if (actualSets.HasValue && actualSets > Sets) return true;
        return false;
    }

    private Exercise Clone() => new()
    {
        Id = Id,
        Name = Name,
        Order = Order,
        Sets = Sets,
        Reps = Reps,
        TargetWeight = TargetWeight,
        ActualSets = ActualSets,
        ActualReps = ActualReps,
        ActualWeight = ActualWeight,
        UnexpectedProgress = UnexpectedProgress,
        Notes = Notes
    };
}
