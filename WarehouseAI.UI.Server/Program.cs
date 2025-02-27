using WarehouseAI.Core;
using WarehouseAI.UI.Server.Helpers;
using WarehouseAI.UI.Shared;

var builder = WebApplication.CreateBuilder(args);

// Register ChatSessionService as a singleton
builder.Services.AddSingleton<ChatSessionService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

// ---------------------- SESSION MANAGEMENT ----------------------

app.MapPost(
    "/sessions",
    (ChatSessionService chatService) =>
    {
        var sessionId = chatService.StartNewSession();

        return Results.Ok(new SessionIdResponse(sessionId));
    }
);

// Retrieve all active session IDs
app.MapGet(
    "/sessions",
    (ChatSessionService chatService) =>
    {
        var sessionIds = chatService.GetAllSessionIds();

        return Results.Ok(new SessionIdsResponse(sessionIds));
    }
);

// ---------------------- MESSAGE HANDLING ----------------------

app.MapPost(
    "/sessions/{sessionId}/messages",
    async (string sessionId, ChatSessionService chatService, MessageRequest messageRequest) =>
    {
        var response = await chatService.SendMessageAsync(sessionId, messageRequest.UserMessage);

        return Results.Ok(new MessageResponse(response));
    }
);

// ---------------------- SESSION HISTORY ----------------------

app.MapGet(
    "/sessions/{sessionId}/history",
    (string sessionId, ChatSessionService chatService) =>
    {
        var history = chatService.GetChatHistoryById(sessionId);

        return Results.Ok(new ChatHistoryResponse([.. history]));
    }
);

// ---------------------- SESSION DELETION ----------------------

app.MapPost(
    "/sessions/{sessionId}",
    (string sessionId, ChatSessionService chatService) =>
    {
        var deleted = chatService.DeleteSession(sessionId);

        return Results.Ok(deleted);
    }
);

app.Run();