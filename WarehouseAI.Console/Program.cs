using Microsoft.Extensions.Configuration;
using WarehouseAI.Core;

// Load configuration from User Secrets
IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>() // Ensure this matches the UserSecretsId in your .csproj
    .Build();

// Initialize ChatSessionService with valid config
var chatSessionService = new ChatSessionService(config);

// Start a single session automatically
var sessionId = chatSessionService.StartNewSession();

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("User > ");
    string userMessage = Console.ReadLine();

    string response = await chatSessionService.SendMessageAsync(sessionId, userMessage);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Agent > {response}\n");
}