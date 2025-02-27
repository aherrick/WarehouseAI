using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace WarehouseAI.Core.Helpers;

public static class SkHelper
{
    public static List<(string PluginName, string FunctionName)> GetToolsInovked(
        ChatHistory chatHistory
    )
    {
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
            .SelectMany(toolMsg => toolMsg.Items.OfType<FunctionResultContent>())
            .Select(fr => (fr.PluginName, fr.FunctionName)) // Extract PluginName and FunctionName
            .ToList();

        return toolMessages;
    }
}