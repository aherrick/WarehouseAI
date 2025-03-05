using Microsoft.Extensions.Configuration;
using WarehouseAI.Core;

namespace WarehouseAI.Tests;

public class ChatSessionServiceTests
{
    private readonly ChatSessionService ChatService;
    private const string WarehouseOnlyMessage = "I can only assist with warehouse operations";

    public ChatSessionServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ChatSessionServiceTests>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        ChatService = new ChatSessionService(configuration);
    }

    [Fact]
    public void StartNewSession_ReturnsValidSessionId()
    {
        var sessionId = ChatService.StartNewSession();
        Assert.False(string.IsNullOrEmpty(sessionId));
        Assert.True(Guid.TryParse(sessionId, out _), "Session ID should be a valid GUID");
    }

    [Fact]
    public async Task SendMessageAsync_ValidWarehouseRequest_ReturnsPluginResponse()
    {
        var sessionId = ChatService.StartNewSession();
        var validMessage = "Check inventory for itemA";
        var response = await ChatService.SendMessageAsync(sessionId, validMessage);

        Assert.False(string.IsNullOrEmpty(response));
        Assert.DoesNotContain(WarehouseOnlyMessage, response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendMessageAsync_InvalidRequest_ReturnsRestrictedMessage()
    {
        var sessionId = ChatService.StartNewSession();
        var invalidMessage = "Who's Michael Jordan?";
        var response = await ChatService.SendMessageAsync(sessionId, invalidMessage);

        Assert.Contains(WarehouseOnlyMessage, response, StringComparison.OrdinalIgnoreCase);
    }
}