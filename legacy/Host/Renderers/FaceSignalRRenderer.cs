using System;
using RaAI.Handlers.Manager;
using RaAI.Modules.DigitalFace;

namespace RaAI.Modules.DigitalFace
{
    [RaModule("WebFaceRenderer")] // ensure discovery
    public sealed class FaceSignalRRendererModule : ModuleBase, IFaceRenderer
    {
        public override string Name => "WebFaceRenderer";
        public string Id => "Web/SignalR";

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            LogInfo("WebFaceRenderer ready (SignalR). Clients connect to /faceHub.");
        }

        public override string Process(string input)
        {
            var t = (input ?? "").Trim();
            if (t.Equals("status", StringComparison.OrdinalIgnoreCase)) return "WebFaceRenderer: available";
            return "WebFaceRenderer: no commands.";
        }

        public void OnWake() => _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.wake", new { atUtc = DateTime.UtcNow });
        public void SetMood(string mood) => _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.mood", new { mood, atUtc = DateTime.UtcNow });
        public void SetAttention(double level) => _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.attention", new { level = Clamp01(level), atUtc = DateTime.UtcNow });
        public void SetAudioLevel(double rms) => _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.audio", new { rms = Clamp01(rms), atUtc = DateTime.UtcNow });
        public void Blink() => _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.blink", new { atUtc = DateTime.UtcNow });

        // NEW: broadcast anchors payload to all web clients
        public void SetAnchors(string anchorsJson)
        {
            _ = Ra.Host.FaceSignalRBridge.BroadcastAsync("face.anchors", new { anchors = anchorsJson, atUtc = DateTime.UtcNow });
        }

        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
    }
}