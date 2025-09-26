namespace RaAI.Handlers
{
    public interface IModuleRegistry
    {
        string[] GetModuleNames();
        IRaModule? GetModuleByName(string name);
    }
}