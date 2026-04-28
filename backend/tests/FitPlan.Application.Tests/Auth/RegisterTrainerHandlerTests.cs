using FluentAssertions;
using Moq;
using FitPlan.Application.Auth.Commands;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using Xunit;

namespace FitPlan.Application.Tests.Auth;

public class RegisterTrainerHandlerTests
{
    private readonly Mock<ITrainerRepository> _trainerRepo = new();
    private readonly Mock<IEmailTokenRepository> _tokenRepo = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RegisterTrainerHandler CreateSut() =>
        new(_trainerRepo.Object, _tokenRepo.Object, _emailService.Object);

    [Fact]
    public async Task Handle_ShouldInsertTrainer_WhenEmailIsNew()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync((Trainer?)null);

        await CreateSut().Handle(
            new RegisterTrainerCommand("Alex", "alex@fitplan.io", "SecurePass1!"),
            default);

        _trainerRepo.Verify(r => r.InsertAsync(
            It.Is<Trainer>(t => t.Email == "alex@fitplan.io" && t.Name == "Alex"),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendConfirmationEmail_AfterInsert()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Trainer?)null);

        await CreateSut().Handle(
            new RegisterTrainerCommand("Alex", "alex@fitplan.io", "SecurePass1!"),
            default);

        _emailService.Verify(e => e.SendConfirmationEmailAsync(
            "alex@fitplan.io", "Alex", It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailAlreadyRegistered()
    {
        var existing = new Trainer("Alex", "alex@fitplan.io", "hash");
        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync(existing);

        var act = () => CreateSut().Handle(
            new RegisterTrainerCommand("Alex", "alex@fitplan.io", "SecurePass1!"),
            default);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_NotStoreItPlaintext()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Trainer?)null);

        await CreateSut().Handle(
            new RegisterTrainerCommand("Alex", "alex@fitplan.io", "SecurePass1!"),
            default);

        _trainerRepo.Verify(r => r.InsertAsync(
            It.Is<Trainer>(t => t.PasswordHash != "SecurePass1!" && t.PasswordHash != null),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeEmailToLowercase()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync((Trainer?)null);

        await CreateSut().Handle(
            new RegisterTrainerCommand("Alex", "ALEX@FITPLAN.IO", "SecurePass1!"),
            default);

        _trainerRepo.Verify(r => r.InsertAsync(
            It.Is<Trainer>(t => t.Email == "alex@fitplan.io"),
            default), Times.Once);
    }
}
