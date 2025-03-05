using Microsoft.Extensions.Configuration;
using WarehouseAI.Core;

namespace WarehouseAI.Tests;

public class ChatSessionServiceTests
{
    private readonly ChatSessionService ChatService;

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
        string sessionId = ChatService.StartNewSession();
        Assert.False(string.IsNullOrEmpty(sessionId));
        Assert.True(Guid.TryParse(sessionId, out _), "Session ID should be a valid GUID");
    }

    [Fact]
    public async Task SendMessageAsync_ValidWarehouseRequest_ReturnsPluginResponse()
    {
        string sessionId = ChatService.StartNewSession();
        string validMessage = "Check inventory for itemA";
        string response = await ChatService.SendMessageAsync(sessionId, validMessage);

        Assert.False(string.IsNullOrEmpty(response));
        Assert.DoesNotContain(
            "I can only assist with warehouse operations",
            response,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task SendMessageAsync_InvalidRequest_ReturnsRestrictedMessage()
    {
        string sessionId = ChatService.StartNewSession();
        string invalidMessage = "Who's Michael Jordan?";
        string response = await ChatService.SendMessageAsync(sessionId, invalidMessage);

        Assert.Contains(
            "I can only assist with warehouse operations",
            response,
            StringComparison.OrdinalIgnoreCase
        );
    }
}