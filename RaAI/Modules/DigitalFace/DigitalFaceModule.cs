using System;
using System.Text;
using RaAI.Handlers.Manager;
using RaAI.Modules.DigitalFace.Rendering;

namespace RaAI.Modules.DigitalFace
{
    // [RaModule("DigitalFace")]
    public sealed class DigitalFaceModule : ModuleBase
    {
        public override string Name => "DigitalFace";

        private bool _awake;
        private bool _rendererPresent;

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            // Detect renderer presence
            try
            {
                _rendererPresent = Type.GetType("RaAI.Modules.DigitalFace.Rendering.GdiParticleRenderer") != null;
            }
            catch { _rendererPresent = false; }
            LogInfo("DigitalFaceModule initialized.");
        }

        // System events
        private void OnWake()
        {
            _awake = true;
            LogInfo("DigitalFaceModule awake.");
        }

        public override string Process(string input)
        {
            var t = (input ?? "").Trim().ToLowerInvariant();
            return t switch
            {
                "help" => "DigitalFace commands:\n  status  - face availability and wake state\n  help    - this help",
                "status" => Status(),
                _ => "Unknown command. Try: help"
            };
        }

        private string Status()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DigitalFace status:");
            sb.AppendLine($"- Renderer present: {_rendererPresent}");
            sb.AppendLine($"- Awake: {_awake}");
            return sb.ToString();
        }
    }
}