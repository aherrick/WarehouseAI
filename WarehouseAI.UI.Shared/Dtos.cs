using System;
using System.Collections.Generic;

namespace WarehouseAI.UI.Shared;

// request
public record MessageRequest(string UserMessage);

// response
public record SessionIdResponse(string SessionId);

public record SessionIdsResponse(List<string> SessionIds);

public record MessageResponse(string AssistantMessage);

public record ChatHistoryResponse(List<ChatData> History);

// dto
public class ChatData
{
    public string User { get; set; }
    public string Assistant { get; set; }
    public DateTime UserTimestamp { get; set; }
    public DateTime AssistantTimestamp { get; set; }
}

public class ApiEx
{
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public string StatusPhrase { get; set; }
}