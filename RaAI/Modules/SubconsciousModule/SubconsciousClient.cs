using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaAI.Modules.SubconsciousModule
{
    public class SubconsciousClient(ISubconscious subconsciousModule)
    {
        private readonly ISubconscious _subconsciousModule = subconsciousModule ?? throw new ArgumentNullException(nameof(subconsciousModule));
        private readonly SubconsciousModule? subconsciousModule;

        public SubconsciousClient(SubconsciousModule subconsciousModule)
            : this(subconsciousModule as ISubconscious ?? throw new ArgumentNullException(nameof(subconsciousModule)))
        {
            this.subconsciousModule = subconsciousModule;
        }

        public async Task<string> ProbeAsync(string query, CancellationToken cancellationToken = default)
        {
            // Call the Probe method of the SubconsciousModule
            return await _subconsciousModule.Probe(query, cancellationToken);
        }

        internal string Probe(string content)
        {
            throw new NotImplementedException();
        }
        public void SendMessage(string message)
        {
            _subconsciousModule.ReceiveMessage(message);
        }

        public string GetResponse()
        {
            return _subconsciousModule.GetResponse();
        }
    }
}