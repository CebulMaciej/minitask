using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;

namespace FitPlan.Application.Auth.Commands;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ResetPasswordHandler(
    IEmailTokenRepository tokenRepo,
    ITrainerRepository trainerRepo) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var token = await tokenRepo.GetValidByTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("ResetToken", request.Token);

        var trainer = await trainerRepo.GetByIdAsync(token.UserId, ct)
            ?? throw new NotFoundException("Trainer", token.UserId);

        trainer.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await trainerRepo.UpdateAsync(trainer, ct);

        token.MarkUsed();
        await tokenRepo.UpdateAsync(token, ct);
    }
}
