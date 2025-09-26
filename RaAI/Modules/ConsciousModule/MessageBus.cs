using System;

namespace RaAI.Modules.ConsciousModule
{ 
    // Tiny in-process bus for events internal to Conscious; allows UI or other modules (if they get a reference) to subscribe.
    public class MessageBus
    { 
        public event Action<string, string>? MessageSent; // (topic, payload)

        public void Publish(string topic, string payload)
        {
            try { MessageSent?.Invoke(topic, payload); }
            catch { /* swallow to avoid propagating */ }
        }
    }
}