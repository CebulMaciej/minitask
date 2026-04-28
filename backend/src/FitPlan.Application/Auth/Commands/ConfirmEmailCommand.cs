using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Auth.Commands;

public record ConfirmEmailCommand(string Token) : IRequest;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator() => RuleFor(x => x.Token).NotEmpty();
}

public class ConfirmEmailHandler(
    IEmailTokenRepository tokenRepo,
    ITrainerRepository trainerRepo,
    IClientRepository clientRepo) : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var token = await tokenRepo.GetValidByTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("ConfirmationToken", request.Token);

        token.MarkUsed();
        await tokenRepo.UpdateAsync(token, ct);

        if (token.UserType == UserType.Trainer)
        {
            var trainer = await trainerRepo.GetByIdAsync(token.UserId, ct)
                ?? throw new NotFoundException("Trainer", token.UserId);
            trainer.ConfirmEmail();
            await trainerRepo.UpdateAsync(trainer, ct);
        }
        else
        {
            var client = await clientRepo.GetByIdAsync(token.UserId, ct)
                ?? throw new NotFoundException("Client", token.UserId);
            client.ConfirmEmail();
            await clientRepo.UpdateAsync(client, ct);
        }
    }
}
