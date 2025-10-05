namespace Abstractions;

public interface ISubconscious
{
    Task<string> Probe(string query, CancellationToken cancellationToken = default);
    void ReceiveMessage(string message);
    string GetResponse();
}
