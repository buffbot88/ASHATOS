using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SubconsciousModule
{
    /// <summary>
    /// Interface for subconscious AI modules. Defines asynchronous probing, message input, and response output.
    /// Extend this interface with additional methods and properties as needed.
    /// </summary>
    public interface ISubconscious
    {
        /// <summary>
        /// Probe the subconscious module asynchronously with a query and optional cancellation.
        /// </summary>
        Task<string> Probe(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a message to the subconscious module for internal processing or logging.
        /// </summary>
        void ReceiveMessage(string message);

        /// <summary>
        /// Retrieve the latest response (output) from the subconscious module.
        /// </summary>
        string GetResponse();
    }
}