using Ra.Core.Engine.Manager;
using System;

namespace Ra.Core.Modules;

public class InfoHandlerModule : IRaModule
{
    public string Name => "InfoHandler";
    public void Initialize(ModuleManager manager) { }
    public string Process(string input) => input; // Just echoes the info
    public void Dispose() { }
}