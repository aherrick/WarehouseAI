using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace WarehouseAI.Core.Helpers;

public static class SkHelper
{
    public static List<(string PluginName, string FunctionName)> GetToolsInvoked(
        ChatHistory chatHistory
    )
    {
        // Find the last user message index and process tool messages in one pass
        return
        [
            .. chatHistory
                .Select((msg, index) => (Message: msg, Index: index))
                .Reverse() // Start from the end for efficiency
                .TakeWhile(x => x.Message.Role != AuthorRole.User) // Take messages until the last user message
                .Where(x => x.Message.Role == AuthorRole.Tool) // Filter for tool messages
                .SelectMany(x => x.Message.Items.OfType<FunctionResultContent>()) // Extract function results
                .Select(fr => (fr.PluginName, fr.FunctionName)),
        ];
    }
}