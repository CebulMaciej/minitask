using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Auth.Commands;

public record RegisterTrainerCommand(string Name, string Email, string Password) : IRequest;

public class RegisterTrainerValidator : AbstractValidator<RegisterTrainerCommand>
{
    public RegisterTrainerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterTrainerHandler(
    ITrainerRepository trainerRepo,
    IEmailTokenRepository tokenRepo,
    IEmailService emailService) : IRequestHandler<RegisterTrainerCommand>
{
    public async Task Handle(RegisterTrainerCommand request, CancellationToken ct)
    {
        var existing = await trainerRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (existing != null) throw new ConflictException("An account with this email already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var trainer = new Trainer(request.Name, request.Email, hash);
        await trainerRepo.InsertAsync(trainer, ct);

        var token = new EmailConfirmationToken(trainer.Id, UserType.Trainer,
            Guid.NewGuid().ToString("N"), DateTime.UtcNow.AddHours(24));
        await tokenRepo.InsertAsync(token, ct);

        await emailService.SendConfirmationEmailAsync(trainer.Email, trainer.Name, token.Token, ct);
    }
}
