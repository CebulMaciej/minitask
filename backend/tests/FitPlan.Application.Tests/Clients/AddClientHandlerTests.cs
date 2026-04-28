using FluentAssertions;
using Moq;
using FitPlan.Application.Clients.Commands;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using Xunit;

namespace FitPlan.Application.Tests.Clients;

public class AddClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepo = new();
    private readonly Mock<IEmailTokenRepository> _tokenRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ITrainerRepository> _trainerRepo = new();

    private AddClientHandler CreateSut() =>
        new(_clientRepo.Object, _tokenRepo.Object, _emailService.Object, _trainerRepo.Object);

    private const string TrainerId = "trainer-1";

    private void SetupTrainer() =>
        _trainerRepo.Setup(r => r.GetByIdAsync(TrainerId, default))
            .ReturnsAsync(new Trainer("Coach", "coach@gym.io", "hash"));

    [Fact]
    public async Task Handle_ShouldInsertClient_WhenEmailIsNew()
    {
        SetupTrainer();
        _clientRepo.Setup(r => r.GetByEmailAndTrainerAsync(
            "bob@fitplan.io", TrainerId, default))
            .ReturnsAsync((Client?)null);

        var result = await CreateSut().Handle(
            new AddClientCommand(TrainerId, "Bob", "bob@fitplan.io"), default);

        result.Name.Should().Be("Bob");
        result.Email.Should().Be("bob@fitplan.io");
        _clientRepo.Verify(r => r.InsertAsync(
            It.Is<Client>(c => c.Name == "Bob" && c.TrainerId == TrainerId),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendInvitationEmail_AfterInsert()
    {
        SetupTrainer();
        _clientRepo.Setup(r => r.GetByEmailAndTrainerAsync(
            It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync((Client?)null);

        await CreateSut().Handle(
            new AddClientCommand(TrainerId, "Bob", "bob@fitplan.io"), default);

        _emailService.Verify(e => e.SendClientInvitationAsync(
            "bob@fitplan.io", "Bob", "Coach",
            It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenClientEmailAlreadyExistsForTrainer()
    {
        SetupTrainer();
        _clientRepo.Setup(r => r.GetByEmailAndTrainerAsync(
            "bob@fitplan.io", TrainerId, default))
            .ReturnsAsync(new Client(TrainerId, "Bob", "bob@fitplan.io"));

        var act = () => CreateSut().Handle(
            new AddClientCommand(TrainerId, "Bob", "bob@fitplan.io"), default);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenTrainerDoesNotExist()
    {
        _trainerRepo.Setup(r => r.GetByIdAsync(TrainerId, default))
            .ReturnsAsync((Trainer?)null);
        _clientRepo.Setup(r => r.GetByEmailAndTrainerAsync(
            It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync((Client?)null);

        var act = () => CreateSut().Handle(
            new AddClientCommand(TrainerId, "Bob", "bob@fitplan.io"), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCreateEmailToken_ForClientInvitation()
    {
        SetupTrainer();
        _clientRepo.Setup(r => r.GetByEmailAndTrainerAsync(
            It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync((Client?)null);

        await CreateSut().Handle(
            new AddClientCommand(TrainerId, "Bob", "bob@fitplan.io"), default);

        _tokenRepo.Verify(r => r.InsertAsync(
            It.IsAny<EmailConfirmationToken>(), default), Times.Once);
    }
}
