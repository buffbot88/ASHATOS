namespace Abstractions;

public interface IHandlerModule
{
    Task<ModuleResponse> ProcessAsync(string input);
    public string Process(string input);
}
