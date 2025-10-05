using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RaCore.Modules.Extensions.Language;

internal static class AILanguageReflectionUtils
{
    public static Type? FindTypeByName(Assembly asm, string simpleName)
    {
        try { return asm.GetTypes().FirstOrDefault(t => string.Equals(t.Name, simpleName, StringComparison.OrdinalIgnoreCase)); }
        catch { return null; }
    }

    public static void TryDispose(object? obj)
    {
        if (obj is null) return;
        if (obj is IDisposable d)
        {
            try { d.Dispose(); } catch { }
            return;
        }
        var mi = obj.GetType().GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
        if (mi != null)
        {
            try { mi.Invoke(obj, null); } catch { }
        }
    }

    public static string CorrectFileLocationInContext(string context)
    {
        if (string.IsNullOrEmpty(context)) return context;
        var tokens = context.Split(' ');
        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token.Contains(".") || token.Contains("/") || token.Contains("\\"))
            {
                var normalized = token.Replace("\"", "").Replace("'", "").Trim();
                try
                {
                    if (File.Exists(normalized))
                        tokens[i] = Path.GetFullPath(normalized);
                    else if (Directory.Exists(normalized))
                        tokens[i] = Path.GetFullPath(normalized);
                }
                catch { }
            }
        }
        return string.Join(' ', tokens);
    }
}
