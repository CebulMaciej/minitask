using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ForgotPasswordHandler(
    ITrainerRepository trainerRepo,
    IEmailTokenRepository tokenRepo,
    IEmailService emailService) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var trainer = await trainerRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (trainer == null) return; // Always return 200 — no email enumeration

        var token = new EmailConfirmationToken(trainer.Id, UserType.Trainer,
            Guid.NewGuid().ToString("N"), DateTime.UtcNow.AddHours(1));
        await tokenRepo.InsertAsync(token, ct);

        await emailService.SendPasswordResetEmailAsync(trainer.Email, trainer.Name, token.Token, ct);
    }
}
