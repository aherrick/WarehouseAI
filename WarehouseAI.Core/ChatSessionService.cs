using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using WarehouseAI.Core.Plugins;

namespace WarehouseAI.Core;

public class ChatSessionService(IConfiguration config)
{
    private readonly ConcurrentDictionary<
        Guid,
        (
            Kernel Kernel,
            ChatHistory ChatHistory,
            List<(string User, string Assistant, DateTime Timestamp)> Messages
        )
    > _sessions = new();

    private const string AI_UNSUPPORTED_RESPONSE =
        "I can only assist with warehouse operations. Please ask a valid request.";

    public Guid StartNewSession()
    {
        var newSessionId = Guid.NewGuid();
        var kernel = Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: config["AzureOpenAI:DeploymentName"],
                endpoint: config["AzureOpenAI:Endpoint"],
                apiKey: config["AzureOpenAI:ApiKey"]
            )
            .Build();
        kernel.Plugins.AddFromType<ShippingAgentPlugin>();
        kernel.Plugins.AddFromType<InventoryAgentPlugin>();
        kernel.Plugins.AddFromType<RobotTaskAgentPlugin>();

        var systemPrompt =
            @$"
            You are a warehouse AI assistant.
            You are ONLY allowed to invoke the provided functions.
            NEVER respond with free text.
            If a request does not match a function, reply with:
            {AI_UNSUPPORTED_RESPONSE}";

        _sessions[newSessionId] = (
            kernel,
            new ChatHistory(systemPrompt),
            new List<(string User, string Assistant, DateTime Timestamp)>()
        );
        return newSessionId;
    }

    public async Task<string> SendMessageAsync(Guid sessionId, string userMessage)
    {
        var (kernel, chatHistory, messages) = _sessions[sessionId];
        chatHistory.AddUserMessage(userMessage);

        var promptExecutionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var result = await chatService.GetChatMessageContentAsync(
            chatHistory,
            promptExecutionSettings,
            kernel
        );

        // Find the index of the most recent user message
        int lastUserMessageIndex = -1;
        for (int i = chatHistory.Count - 1; i >= 0; i--)
        {
            if (chatHistory[i].Role == AuthorRole.User)
            {
                lastUserMessageIndex = i;
                break;
            }
        }

        // Extract only tool messages that appear AFTER the last user message
        var toolMessages = chatHistory
            .Skip(lastUserMessageIndex + 1) // Start from the message after the user's last input
            .Where(msg => msg.Role == AuthorRole.Tool)
            .SelectMany(toolMsg =>
                toolMsg
                    .Items.OfType<FunctionResultContent>()
                    .Select(x => $"({x.PluginName}: {x.FunctionName})")
            )
            .ToList();

        var pluginDetails = toolMessages.Count > 0 ? "\n\n" + string.Join("\n", toolMessages) : "";

        var assistantMessage = result.Content + pluginDetails;
        chatHistory.AddMessage(result.Role, result.Content);
        messages.Add((userMessage, assistantMessage, DateTime.UtcNow));

        return assistantMessage;
    }

    public List<Guid> GetAllSessionIds()
    {
        return [.. _sessions.Keys];
    }

    public IEnumerable<(string User, string Assistant, DateTime Timestamp)> GetChatHistoryById(
        Guid sessionId
    )
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return session.Messages;
        }
        return [];
    }

    public bool DeleteSession(Guid sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}