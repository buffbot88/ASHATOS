using System;

namespace RaAI.Modules.SubconsciousModule
{
    public interface ISubconscious
    {
        Task<string> Probe(string query, CancellationToken cancellationToken = default);
        void ReceiveMessage(string message);
        string GetResponse();
        // Add more methods and properties as needed
    }
}