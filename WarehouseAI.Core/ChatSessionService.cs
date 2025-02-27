using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using WarehouseAI.Core.Plugins;
using WarehouseAI.UI.Shared;

namespace WarehouseAI.Core;

public class ChatSessionService(IConfiguration config)
{
    private readonly ConcurrentDictionary<
        string,
        (Kernel Kernel, ChatHistory ChatHistory, List<ChatData> Messages)
    > _sessions = new();

    private const string AI_UNSUPPORTED_RESPONSE =
        "I can only assist with warehouse operations. Please ask a valid request.";

    public string StartNewSession()
    {
        var newSessionId = Guid.NewGuid().ToString();
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

        _sessions[newSessionId] = (kernel, new ChatHistory(systemPrompt), new List<ChatData>());
        return newSessionId;
    }

    public async Task<string> SendMessageAsync(string sessionId, string userMessage)
    {
        var (kernel, chatHistory, messages) = _sessions[sessionId];

        var userTimestamp = DateTime.UtcNow; // Capture user message timestamp
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

        var toolMessages = ExtractToolMessages(chatHistory);

        var assistantMessage =
            result.Content
            + (toolMessages.Count > 0 ? "\n\n" + string.Join("\n", toolMessages) : "");
        chatHistory.AddAssistantMessage(result.Content);
        messages.Add(
            new ChatData
            {
                User = userMessage,
                Assistant = assistantMessage,
                UserTimestamp = userTimestamp,
                AssistantTimestamp = DateTime.UtcNow,
            }
        );

        return assistantMessage;
    }

    private static List<string> ExtractToolMessages(ChatHistory chatHistory)
    {
        int lastUserMessageIndex = -1;
        for (int i = chatHistory.Count - 1; i >= 0; i--)
        {
            if (chatHistory[i].Role == AuthorRole.User)
            {
                lastUserMessageIndex = i;
                break;
            }
        }

        return
        [
            .. chatHistory
                .Skip(lastUserMessageIndex + 1) // Start from the message after the user's last input
                .Where(msg => msg.Role == AuthorRole.Tool)
                .SelectMany(toolMsg =>
                    toolMsg
                        .Items.OfType<FunctionResultContent>()
                        .Select(x => $"({x.PluginName}: {x.FunctionName})")
                ),
        ];
    }

    public List<string> GetAllSessionIds()
    {
        return [.. _sessions.Keys];
    }

    public IEnumerable<ChatData> GetChatHistoryById(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return session.Messages;
        }
        return [];
    }

    public bool DeleteSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}