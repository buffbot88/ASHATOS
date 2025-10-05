using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Subconscious;

public class SubconsciousProxy : ISubconscious
{
    private readonly object _impl;
    private readonly Type _implType;
    private readonly IAILanguageModule? _aiLang;

    public SubconsciousProxy(ModuleManager manager)
    {
        _impl = FindAndCreateSubconsciousInstance(manager) ?? throw new InvalidOperationException("No SubconsciousModule implementation found.");
        _implType = _impl.GetType();
        _aiLang = manager.GetModuleInstanceByName("AILanguage") as IAILanguageModule;
    }

    private static object? FindAndCreateSubconsciousInstance(ModuleManager manager)
    {
        return manager.GetModuleInstanceByName("Subconscious")
            ?? manager.GetModuleInstanceByName("SubconsciousModule");
    }

    public async Task<string> Probe(string query, CancellationToken cancellationToken = default)
    {
        var m = _implType.GetMethod("Probe", [typeof(string), typeof(CancellationToken)]) ?? throw new MissingMethodException("Probe(string, CancellationToken) not found.");
        var taskObj = m.Invoke(_impl, [query, cancellationToken]);
        if (taskObj is Task<string> ts)
        {
            var raw = await ts.ConfigureAwait(false);
            if (_aiLang != null)
            {
                var resp = await _aiLang.GenerateAsync("subconscious.probe", raw, "en", null);
                return resp.Text;
            }
            return raw;
        }
        throw new InvalidOperationException("Probe did not return Task<string>.");
    }

    public void ReceiveMessage(string message)
    {
        var m = _implType.GetMethod("ReceiveMessage", [typeof(string)]);
        m?.Invoke(_impl, [message]);
    }

    public string GetResponse()
    {
        var m = _implType.GetMethod("GetResponse", Type.EmptyTypes);
        var raw = m?.Invoke(_impl, [])?.ToString() ?? string.Empty;
        if (_aiLang != null)
        {
            var resp = _aiLang.GenerateAsync("subconscious.response", raw, "en", null).GetAwaiter().GetResult();
            return resp.Text;
        }
        return raw;
    }
}
