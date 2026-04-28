using FluentAssertions;
using Moq;
using FitPlan.Application.Auth.Queries;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using FitPlan.Domain.Entities;
using FitPlan.Domain.Enums;
using Xunit;

namespace FitPlan.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<ITrainerRepository> _trainerRepo = new();
    private readonly Mock<IClientRepository> _clientRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IRefreshTokenService> _refreshService = new();

    private LoginHandler CreateSut() =>
        new(_trainerRepo.Object, _clientRepo.Object, _jwtService.Object, _refreshService.Object);

    private static Trainer ConfirmedTrainer(string password = "Secret123!")
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var t = new Trainer("Alex", "alex@fitplan.io", hash);
        t.ConfirmEmail();
        return t;
    }

    // ── Trainer login ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldReturnAuthResult_WhenTrainerCredentialsValid()
    {
        var trainer = ConfirmedTrainer();
        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync(trainer);
        _jwtService.Setup(j => j.GenerateAccessToken(trainer.Id, UserType.Trainer, null))
            .Returns("access-token");
        _refreshService.Setup(r => r.GenerateAndStoreAsync(trainer.Id, UserType.Trainer, default))
            .ReturnsAsync("refresh-token");

        var result = await CreateSut().Handle(
            new LoginQuery("alex@fitplan.io", "Secret123!", UserType.Trainer), default);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserType.Should().Be(UserType.Trainer);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTrainerEmailNotFound()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Trainer?)null);

        var act = () => CreateSut().Handle(
            new LoginQuery("unknown@fitplan.io", "pass", UserType.Trainer), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordWrong()
    {
        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync(ConfirmedTrainer("CorrectPassword!"));

        var act = () => CreateSut().Handle(
            new LoginQuery("alex@fitplan.io", "WrongPassword!", UserType.Trainer), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenEmailNotConfirmed()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Secret123!");
        var unconfirmed = new Trainer("Alex", "alex@fitplan.io", hash); // EmailConfirmed = false

        _trainerRepo.Setup(r => r.GetByEmailAsync("alex@fitplan.io", default))
            .ReturnsAsync(unconfirmed);

        var act = () => CreateSut().Handle(
            new LoginQuery("alex@fitplan.io", "Secret123!", UserType.Trainer), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*confirm*");
    }

    // ── Client login ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldReturnClientAuthResult_WithTrainerId_WhenClientCredentialsValid()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("ClientPass1!");
        var client = new Client("trainer-1", "Bob", "bob@fitplan.io", hash);
        client.ConfirmEmail();

        _clientRepo.Setup(r => r.FindOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Client, bool>>>(), default))
            .ReturnsAsync(client);
        _jwtService.Setup(j => j.GenerateAccessToken(client.Id, UserType.Client, "trainer-1"))
            .Returns("client-access-token");
        _refreshService.Setup(r => r.GenerateAndStoreAsync(client.Id, UserType.Client, default))
            .ReturnsAsync("client-refresh-token");

        var result = await CreateSut().Handle(
            new LoginQuery("bob@fitplan.io", "ClientPass1!", UserType.Client), default);

        result.AccessToken.Should().Be("client-access-token");
        result.UserType.Should().Be(UserType.Client);
    }
}
