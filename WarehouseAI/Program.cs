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

var systemPrompt =
    @"You are a warehouse AI assistant.
                     You are ONLY allowed to invoke the provided functions.
                     NEVER respond with free text.
                     If a request does not match a function, reply with:
                     'I can only assist with warehouse operations. Please ask a valid request.'";

var chatHistory = new ChatHistory(systemPrompt);

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