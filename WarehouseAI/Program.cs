using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using WarehouseAI.Plugins;

var kernel = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName: "", endpoint: "", apiKey: "")
    .Build();

kernel.Plugins.AddFromType<ShippingAgentPlugin>();
kernel.Plugins.AddFromType<InventoryAgentPlugin>();
kernel.Plugins.AddFromType<RobotTaskAgentPlugin>();

var promptExecutionSettings = new PromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

/* Examples:
        Assign a robot to move boxes to Aisle 5
        What’s the status of the robots?
        Check stock for ItemA
        Order 20 ItemB
        Ship ItemB
*/
while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("User > ");
    chatHistory.AddUserMessage(Console.ReadLine());

    var updates = chatService.GetStreamingChatMessageContentsAsync(
        chatHistory,
        promptExecutionSettings,
        kernel
    );

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Assistant > ");
    var sb = new StringBuilder();
    await foreach (var update in updates)
    {
        sb.Append(update.Content);
        Console.Write(update.Content);
    }

    chatHistory.AddAssistantMessage(sb.ToString());

    Console.WriteLine();
}