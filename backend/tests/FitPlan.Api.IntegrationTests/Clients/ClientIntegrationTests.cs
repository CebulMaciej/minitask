using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitPlan.Api.IntegrationTests.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace FitPlan.Api.IntegrationTests.Clients;

public class ClientIntegrationTests : IClassFixture<FitPlanWebFactory>
{
    private readonly FitPlanWebFactory _factory;

    public ClientIntegrationTests(FitPlanWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient AuthenticatedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private void SetupEmailMocks()
    {
        _factory.EmailServiceMock
            .Setup(e => e.SendConfirmationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _factory.EmailServiceMock
            .Setup(e => e.SendClientInvitationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task AddClient_ShouldReturn201_WithTrainerToken()
    {
        SetupEmailMocks();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(
            _factory.CreateClient(), _factory.EmailServiceMock, "Trainer A");
        var client = AuthenticatedClient(token);

        var res = await client.PostAsJsonAsync("/api/clients",
            new { name = "Client X", email = $"cx-{Guid.NewGuid():N}@test.fitplan" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddClient_ShouldReturn401_WithNoToken()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/clients",
            new { name = "Anon Client", email = "anon@test.fitplan" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetClients_ShouldOnlyReturnCurrentTrainersClients()
    {
        SetupEmailMocks();
        var http = _factory.CreateClient();

        // Trainer A adds client
        var (tokenA, _) = await TestHelpers.RegisterAndLoginAsync(
            http, _factory.EmailServiceMock, "Trainer A isolation");
        var clientA = AuthenticatedClient(tokenA);
        await clientA.PostAsJsonAsync("/api/clients",
            new { name = "Client of A", email = $"client-a-{Guid.NewGuid():N}@test.fitplan" });

        // Trainer B should not see Trainer A's client
        var (tokenB, _) = await TestHelpers.RegisterAndLoginAsync(
            http, _factory.EmailServiceMock, "Trainer B isolation");
        var clientB = AuthenticatedClient(tokenB);
        var res = await clientB.GetAsync("/api/clients");
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<List<object>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClient_ShouldReturn404_WhenClientBelongsToOtherTrainer()
    {
        SetupEmailMocks();
        var http = _factory.CreateClient();

        var (tokenA, _) = await TestHelpers.RegisterAndLoginAsync(
            http, _factory.EmailServiceMock, "Owner Trainer");
        var clientA = AuthenticatedClient(tokenA);

        var addRes = await clientA.PostAsJsonAsync("/api/clients",
            new { name = "Owned Client", email = $"owned-{Guid.NewGuid():N}@test.fitplan" });
        addRes.EnsureSuccessStatusCode();
        var owned = await addRes.Content.ReadFromJsonAsync<ClientResponse>();

        var (tokenB, _) = await TestHelpers.RegisterAndLoginAsync(
            http, _factory.EmailServiceMock, "Stealing Trainer");
        var clientB = AuthenticatedClient(tokenB);
        var res = await clientB.GetAsync($"/api/clients/{owned!.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddClient_ShouldReturn409_WhenDuplicateEmailForSameTrainer()
    {
        SetupEmailMocks();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(
            _factory.CreateClient(), _factory.EmailServiceMock, "Dup Trainer");
        var client = AuthenticatedClient(token);
        var email = $"dup-client-{Guid.NewGuid():N}@test.fitplan";

        await client.PostAsJsonAsync("/api/clients", new { name = "Client", email });
        var res2 = await client.PostAsJsonAsync("/api/clients", new { name = "Client2", email });

        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private record ClientResponse(string Id, string Name, string Email);
}
