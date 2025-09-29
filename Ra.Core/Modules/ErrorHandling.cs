using Ra.Core.Engine;
using Ra.Core.Engine.Manager;
using System;

namespace Ra.Core.Modules;

public class ErrorHandlerModule : IRaModule
{
    public string Name => "ErrorHandler";

    private ModuleManager _manager;

    public void Initialize(ModuleManager manager)
    {
        _manager = manager;
        // Optionally subscribe to events here if your manager supports error events
    }

    public string Process(string input)
    {
        // This module can be called explicitly to log errors, or you can have other modules call it when they catch errors.
        return $"ERROR: {input}";
    }

    public void Dispose() { }
}