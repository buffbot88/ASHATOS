using System;

namespace ASHATCore.Modules.Handlers;

/// <summary>
/// Diagnostics and event hooks for Info/Error handlers; for UI/logging.
/// </summary>
public static class HandlerDiagnostics
{
    public static event Action<string, string>? OnInfoHandled;
    public static event Action<string, string>? OnErrorHandled;

    public static void RaiseInfoHandled(string input, string response)
    {
        OnInfoHandled?.Invoke(input, response);
    }

    public static void RaiseErrorHandled(string input, string response)
    {
        OnErrorHandled?.Invoke(input, response);
    }
}
