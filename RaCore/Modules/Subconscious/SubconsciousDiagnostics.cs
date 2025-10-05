namespace RaCore.Modules.Subconscious;

public static class SubconsciousDiagnostics
{
    public static event Action<string, string>? OnProbe; // (query, response)
    public static event Action<string, string>? OnReceiveMessage; // (message, response)
    public static event Action<string>? OnGetResponse; // (response)
    public static event Action<Exception>? OnError;

    public static void RaiseProbe(string query, string response)
    {
        OnProbe?.Invoke(query, response);
    }

    public static void RaiseReceiveMessage(string message, string response)
    {
        OnReceiveMessage?.Invoke(message, response);
    }

    public static void RaiseGetResponse(string response)
    {
        OnGetResponse?.Invoke(response);
    }

    public static void RaiseError(Exception ex)
    {
        OnError?.Invoke(ex);
    }
}
