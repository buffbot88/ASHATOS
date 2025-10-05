using System.Reflection;

namespace RaCore.Modules.Extensions.Language;

internal static class AILanguageModelLoader
{

    public static (object? llama, object? executor, string? error) LoadModel(string modelPath, uint contextSize, Action<string> logError, Action<string>? logInfo = null)
    {
        object? llama = null;
        object? executor = null;
        string? error = null;

        try
        {
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                error = $"Model file not found: {modelPath}";
                logError(error);
                return (null, null, error);
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var llAsm = assemblies.FirstOrDefault(a => a.GetTypes().Any(t => t.Name == "LLamaContext" || t.Name == "LlamaContext"))
                     ?? assemblies.FirstOrDefault(a => a.GetName().Name == "LlamaSharp");

            if (llAsm == null)
            {
                error = "LlamaSharp assembly not found in app domain.";
                logError(error);
                return (null, null, error);
            }

            var typeWeights = AILanguageReflectionUtils.FindTypeByName(llAsm, "LLamaWeights") ?? AILanguageReflectionUtils.FindTypeByName(llAsm, "LLamaSharpWeights");
            if (typeWeights == null)
            {
                error = "LLamaWeights type not found in LlamaSharp assembly.";
                logError(error);
                return (null, null, error);
            }

            var loadFromFile = typeWeights.GetMethod("LoadFromFile", BindingFlags.Public | BindingFlags.Static)
                               ?? typeWeights.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);

            if (loadFromFile == null)
            {
                error = "No static LoadFromFile/Load method found on LLamaWeights.";
                logError(error);
                return (null, null, error);
            }

            var weights = loadFromFile.Invoke(null, [modelPath]);

            object? ctxParamsObj = null;
            var typeModelParams = llAsm.GetTypes().FirstOrDefault(t => t.Name == "ModelParams" || t.Name == "ContextParams");
            if (typeModelParams != null)
            {
                var ctor0 = typeModelParams.GetConstructor(Type.EmptyTypes);
                if (ctor0 != null)
                {
                    ctxParamsObj = ctor0.Invoke([]);
                }

                if (ctxParamsObj != null)
                {
                    var propModelPath = typeModelParams.GetProperty("ModelPath")
                                       ?? typeModelParams.GetProperty("FileName")
                                       ?? typeModelParams.GetProperty("Path");
                    if (propModelPath != null)
                    {
                        propModelPath.SetValue(ctxParamsObj, modelPath);
                    }
                    else
                    {
                        var setModelPathMethod = typeModelParams.GetMethod("SetModelPath", BindingFlags.Public | BindingFlags.Instance);
                        setModelPathMethod?.Invoke(ctxParamsObj, [modelPath]);
                    }

                    var propCtxSize = typeModelParams.GetProperty("ContextSize");
                    if (propCtxSize != null)
                    {
                        if (propCtxSize.PropertyType == typeof(uint) || propCtxSize.PropertyType == typeof(uint?))
                            propCtxSize.SetValue(ctxParamsObj, (uint?)contextSize);
                        else if (propCtxSize.PropertyType == typeof(int) || propCtxSize.PropertyType == typeof(int?))
                            propCtxSize.SetValue(ctxParamsObj, (int)contextSize);
                    }
                }
            }

            var typeLLamaContext = AILanguageReflectionUtils.FindTypeByName(llAsm, "LLamaContext") ?? AILanguageReflectionUtils.FindTypeByName(llAsm, "LlamaContext");
            if (typeLLamaContext == null)
            {
                error = "LLamaContext type not found.";
                logError(error);
                return (null, null, error);
            }

            var typeLLamaParams = llAsm.GetTypes().FirstOrDefault(t => t.Name == "LLamaParams");
            object? llamaContextObj = null;
            if (typeLLamaParams != null)
            {
                object? llParamsObj = null;
                var ctor0 = typeLLamaParams.GetConstructor(Type.EmptyTypes);
                if (ctor0 != null)
                {
                    llParamsObj = ctor0.Invoke([]);
                    var propWeights = typeLLamaParams.GetProperty("Weights");
                    if (propWeights != null && propWeights.PropertyType.IsInstanceOfType(weights))
                        propWeights.SetValue(llParamsObj, weights);

                    var propModelParams = typeLLamaParams.GetProperty("ModelParams") ?? typeLLamaParams.GetProperty("ModelParameters");
                    if (propModelParams != null && ctxParamsObj != null && propModelParams.PropertyType.IsInstanceOfType(ctxParamsObj))
                        propModelParams.SetValue(llParamsObj, ctxParamsObj);
                }

                if (llParamsObj != null)
                {
                    var ctorLLamaContext = typeLLamaContext.GetConstructor([typeLLamaParams]);
                    if (ctorLLamaContext != null)
                    {
                        llamaContextObj = ctorLLamaContext.Invoke([llParamsObj]);
                    }
                }
            }

            if (llamaContextObj == null)
            {
                var ctxParamInterface = llAsm.GetTypes().FirstOrDefault(t => t.Name == "IContextParams" || t.Name == "IModelParams");
                var ctorB = typeLLamaContext.GetConstructors()
                             .FirstOrDefault(c =>
                             {
                                 var ps = c.GetParameters();
                                 return ps.Length >= 2 &&
                                        ps[0].ParameterType.IsInstanceOfType(weights) &&
                                        (ctxParamInterface == null || ps[1].ParameterType.IsAssignableFrom(ctxParamInterface) || (ctxParamsObj != null && ps[1].ParameterType.IsInstanceOfType(ctxParamsObj)));
                             });
                if (ctorB != null)
                {
                    var parms = new System.Collections.Generic.List<object?> { weights };
                    if (ctxParamsObj != null) parms.Add(ctxParamsObj);
                    else
                    {
                        if (ctxParamInterface != null)
                        {
                            try
                            {
                                var concrete = Activator.CreateInstance(ctxParamInterface.Assembly.GetTypes().FirstOrDefault(t => ctxParamInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract) ?? ctxParamInterface);
                                parms.Add(concrete);
                            }
                            catch { parms.Add(null); }
                        }
                        else parms.Add(null);
                    }
                    var ctorParams = ctorB.GetParameters();
                    if (ctorParams.Length > parms.Count)
                        for (int i = parms.Count; i < ctorParams.Length; i++) parms.Add(null);

                    llamaContextObj = ctorB.Invoke([.. parms]);
                }
            }

            if (llamaContextObj == null)
            {
                error = "Unable to construct LLamaContext with detected API surface.";
                logError(error);
                return (null, null, error);
            }

            llama = llamaContextObj;

            var typeExec = llAsm.GetTypes().FirstOrDefault(t => t.Name == "InteractiveExecutor" || t.Name == "Executor" || t.Name == "LlamaExecutor");
            if (typeExec != null)
            {
                var ctorExec = typeExec.GetConstructors().FirstOrDefault(c =>
                {
                    var ps = c.GetParameters();
                    return ps.Length >= 1 && ps[0].ParameterType.IsInstanceOfType(llama);
                });

                if (ctorExec != null)
                {
                    executor = ctorExec.Invoke([llama]);
                }
                else
                {
                    var ctorExecAlt = typeExec.GetConstructors().FirstOrDefault();
                    if (ctorExecAlt != null)
                    {
                        try
                        {
                            executor = ctorExecAlt.GetParameters().Length == 0 ? ctorExecAlt.Invoke(null) : ctorExecAlt.Invoke([llama]);
                        }
                        catch { executor = null; }
                    }
                }
            }
            logInfo?.Invoke($"AILanguageModule initialized. Model loaded from {modelPath}");
            return (llama, executor, null);
        }
        catch (Exception ex)
        {
            error = $"Failed to load model: {ex.GetType().Name}: {ex.Message}";
            logError($"{error}\n{ex}");
            return (null, null, error);
        }
    }
}
