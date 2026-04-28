using FluentValidation;
using MediatR;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Application.Common.Models;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;

namespace FitPlan.Application.Clients.Commands;

public record AddClientCommand(string TrainerId, string Name, string Email) : IRequest<ClientDetailDto>, ITenantScopedRequest;

public class AddClientValidator : AbstractValidator<AddClientCommand>
{
    public AddClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class AddClientHandler(
    IClientRepository clientRepo,
    IEmailTokenRepository tokenRepo,
    IEmailService emailService,
    ITrainerRepository trainerRepo) : IRequestHandler<AddClientCommand, ClientDetailDto>
{
    public async Task<ClientDetailDto> Handle(AddClientCommand request, CancellationToken ct)
    {
        var existing = await clientRepo.GetByEmailAndTrainerAsync(
            request.Email.ToLowerInvariant(), request.TrainerId, ct);
        if (existing != null) throw new ConflictException("A client with this email already exists.");

        var trainer = await trainerRepo.GetByIdAsync(request.TrainerId, ct)
            ?? throw new NotFoundException("Trainer", request.TrainerId);

        var tempPassword = Guid.NewGuid().ToString("N")[..12];
        var client = new Client(request.TrainerId, request.Name, request.Email,
            BCrypt.Net.BCrypt.HashPassword(tempPassword));
        await clientRepo.InsertAsync(client, ct);

        var token = new EmailConfirmationToken(client.Id, UserType.Client,
            Guid.NewGuid().ToString("N"), DateTime.UtcNow.AddDays(7));
        await tokenRepo.InsertAsync(token, ct);

        await emailService.SendClientInvitationAsync(client.Email, client.Name, trainer.Name, tempPassword, ct);

        return new ClientDetailDto(client.Id, client.Name, client.Email, client.CreatedAt, 0, null);
    }
}
