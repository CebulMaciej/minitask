using FluentAssertions;
using MediatR;
using Moq;
using FitPlan.Application.Common.Behaviors;
using FitPlan.Application.Common.Exceptions;
using FitPlan.Application.Common.Interfaces;
using Xunit;

namespace FitPlan.Application.Tests.Behaviors;

public class TenantValidationBehaviorTests
{
    private readonly Mock<ICurrentTrainerAccessor> _accessor = new();

    private TenantValidationBehavior<TRequest, TResponse> CreateSut<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        => new(_accessor.Object);

    private record UnscopedRequest : IRequest<string>;
    private record ScopedRequest(string TrainerId) : IRequest<string>, ITenantScopedRequest;

    [Fact]
    public async Task Handle_ShouldCallNext_WhenRequestIsNotTenantScoped()
    {
        _accessor.Setup(a => a.IsTrainer).Returns(false);
        _accessor.Setup(a => a.TrainerId).Returns((string?)null);

        var behavior = new TenantValidationBehavior<UnscopedRequest, string>(_accessor.Object);
        var next = new RequestHandlerDelegate<string>(_ => Task.FromResult("ok"));

        var result = await behavior.Handle(new UnscopedRequest(), next, default);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenRequestIsScopedAndTrainerContextPresent()
    {
        _accessor.Setup(a => a.IsTrainer).Returns(true);
        _accessor.Setup(a => a.TrainerId).Returns("trainer-1");

        var behavior = new TenantValidationBehavior<ScopedRequest, string>(_accessor.Object);
        var next = new RequestHandlerDelegate<string>(_ => Task.FromResult("ok"));

        var result = await behavior.Handle(new ScopedRequest("trainer-1"), next, default);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenRequestIsScopedButNotTrainer()
    {
        _accessor.Setup(a => a.IsTrainer).Returns(false);
        _accessor.Setup(a => a.TrainerId).Returns((string?)null);

        var behavior = new TenantValidationBehavior<ScopedRequest, string>(_accessor.Object);
        var next = new RequestHandlerDelegate<string>(_ => Task.FromResult("ok"));

        var act = async () => await behavior.Handle(new ScopedRequest("trainer-1"), next, default);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenTrainerIdIsEmpty()
    {
        _accessor.Setup(a => a.IsTrainer).Returns(true);
        _accessor.Setup(a => a.TrainerId).Returns(string.Empty);

        var behavior = new TenantValidationBehavior<ScopedRequest, string>(_accessor.Object);
        var next = new RequestHandlerDelegate<string>(_ => Task.FromResult("ok"));

        var act = async () => await behavior.Handle(new ScopedRequest("trainer-1"), next, default);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
