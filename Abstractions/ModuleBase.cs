namespace Abstractions
{

    /// <summary>
    /// Base class for RaCore modules. Supports hot-drop events and manager reference.
    /// </summary>
    public abstract class ModuleBase : IRaModule, IDisposable
    {
        protected object? Manager { get; private set; }

        public virtual string Name => GetType().Name.Replace("Module", "");

        public virtual void Initialize(object? manager)
        {
            Manager = manager;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public virtual void OnSystemEvent(string name, object? payload) { }

        public abstract string Process(string input);

        protected void LogInfo(string msg) => Console.WriteLine($"[Module:{Name}] INFO: {msg}");
        protected void LogWarn(string msg) => Console.WriteLine($"[Module:{Name}] WARN: {msg}");
        protected void LogError(string msg) => Console.WriteLine($"[Module:{Name}] ERROR: {msg}");
    }
}