using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RaAI.Handlers.Manager;

namespace RaAI.Modules.DigitalFace
{
    [RaModule("Face")] // ensure discovery
    public sealed class FaceBusModule : ModuleBase
    {
        public override string Name => "Face";

        private ModuleManager? _manager;
        private readonly List<IFaceRenderer> _renderers = new();
        private readonly object _lock = new();

        private FaceState _state = new()
        {
            Mood = "Idle",
            Attention = 0.2,
            AudioLevel = 0.0,
            Awake = false,
            UpdatedAtUtc = DateTime.UtcNow
        };

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _manager = manager;

            foreach (var w in manager.Modules)
                if (w.Instance is IFaceRenderer r)
                    _renderers.Add(r);

            LogInfo($"FaceBus initialized. Renderers: {(_renderers.Count == 0 ? "(none)" : string.Join(", ", _renderers.Select(r => r.Id)))}");
        }

        public override string Process(string input)
        {
            var t = (input ?? "").Trim();
            if (string.IsNullOrEmpty(t) || t.Equals("help", StringComparison.OrdinalIgnoreCase))
                return Help();

            if (t.Equals("status", StringComparison.OrdinalIgnoreCase))
                return $"FaceBus: renderers={_renderers.Count}, mood={_state.Mood}, attention={_state.Attention:F2}, audio={_state.AudioLevel:F2}, awake={_state.Awake}";

            if (t.Equals("face state", StringComparison.OrdinalIgnoreCase) || t.Equals("state", StringComparison.OrdinalIgnoreCase))
                return JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });

            if (t.Equals("face wake", StringComparison.OrdinalIgnoreCase) || t.Equals("wake", StringComparison.OrdinalIgnoreCase))
            {
                lock (_lock)
                {
                    _state.Awake = true;
                    _state.UpdatedAtUtc = DateTime.UtcNow;
                    Broadcast(r => r.OnWake());
                }
                return "Face: wake signaled.";
            }

            if (t.StartsWith("face set mood ", StringComparison.OrdinalIgnoreCase) || t.StartsWith("set mood ", StringComparison.OrdinalIgnoreCase))
            {
                var idx = t.LastIndexOf("mood", StringComparison.OrdinalIgnoreCase);
                var mood = idx >= 0 ? t[(idx + 4)..].Trim() : "Neutral";
                if (string.IsNullOrWhiteSpace(mood)) mood = "Neutral";
                lock (_lock)
                {
                    _state.Mood = mood;
                    _state.UpdatedAtUtc = DateTime.UtcNow;
                    Broadcast(r => r.SetMood(mood));
                }
                return $"Face: mood={mood}";
            }

            if (t.StartsWith("face set attention ", StringComparison.OrdinalIgnoreCase) || t.StartsWith("set attention ", StringComparison.OrdinalIgnoreCase))
            {
                var val = ParseTailDouble(t);
                var clamped = Clamp01(val);
                lock (_lock)
                {
                    _state.Attention = clamped;
                    _state.UpdatedAtUtc = DateTime.UtcNow;
                    Broadcast(r => r.SetAttention(clamped));
                }
                return $"Face: attention={clamped:F2}";
            }

            if (t.StartsWith("face set audio ", StringComparison.OrdinalIgnoreCase) || t.StartsWith("set audio ", StringComparison.OrdinalIgnoreCase))
            {
                var val = ParseTailDouble(t);
                var clamped = Clamp01(val);
                lock (_lock)
                {
                    _state.AudioLevel = clamped;
                    _state.UpdatedAtUtc = DateTime.UtcNow;
                    Broadcast(r => r.SetAudioLevel(clamped));
                }
                return $"Face: audio={clamped:F2}";
            }

            if (t.Equals("face blink", StringComparison.OrdinalIgnoreCase) || t.Equals("blink", StringComparison.OrdinalIgnoreCase))
            {
                lock (_lock)
                {
                    _state.UpdatedAtUtc = DateTime.UtcNow;
                    Broadcast(r => r.Blink());
                }
                return "Face: blink";
            }

            // NEW: pass arbitrary anchors JSON to renderers
            if (t.StartsWith("face anchors ", StringComparison.OrdinalIgnoreCase))
            {
                var json = t.Substring("face anchors ".Length).Trim();
                if (string.IsNullOrWhiteSpace(json))
                    return "(anchors missing)";

                // Lightweight validation: must start with { or [
                if (!(json.StartsWith("{") || json.StartsWith("[")))
                    return "(anchors must be JSON)";

                Broadcast(r => r.SetAnchors(json));
                return "Face: anchors broadcast.";
            }

            if (t.Equals("face bind", StringComparison.OrdinalIgnoreCase))
            {
                var added = 0;
                if (_manager != null)
                {
                    foreach (var w in _manager.Modules)
                    {
                        if (w.Instance is IFaceRenderer r && !_renderers.Contains(r))
                        {
                            _renderers.Add(r);
                            added++;
                        }
                    }
                }
                return $"Face: bound {added} renderer(s). Total={_renderers.Count}.";
            }

            return "FaceBus: unknown command. Try: help";
        }

        private void Broadcast(Action<IFaceRenderer> action)
        {
            foreach (var r in _renderers)
            {
                try { action(r); } catch (Exception ex) { LogWarn($"Renderer '{r.Id}' error: {ex.Message}"); }
            }
        }

        private static double ParseTailDouble(string s)
        {
            var parts = s.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return 0;
            return double.TryParse(parts[^1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        private static string Help() => @"
Face commands:
  - status                       : FaceBus availability and state
  - face state                   : JSON state snapshot
  - face wake                    : notify renderers that system is awake
  - face set mood <mood>         : set mood
  - face set attention <0..1>    : set attention level
  - face set audio <0..1>        : set audio/rms level
  - face blink                   : blink once
  - face anchors {json}          : send an anchor 'sketch' JSON to renderers
  - face bind                    : re-scan for renderers
".Trim();

        private sealed class FaceState
        {
            public string Mood { get; set; } = "Idle";
            public double Attention { get; set; }
            public double AudioLevel { get; set; }
            public bool Awake { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    }
}