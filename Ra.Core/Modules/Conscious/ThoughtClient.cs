using RaAI.Modules.DigitalFace;

// Fix for IDE0290: Use primary constructor for ThoughtClient
namespace RaAI.Modules.ConsciousModule
{
    /// <summary>
    /// ThoughtClient bridges ConsciousThoughtProcessor output to DigitalFaceControl.
    /// It computes a NeuralState (mood, activation, features) from a thought and pushes it to the face.
    /// </summary>
    public class ThoughtClient(DigitalFaceControl face, ConsciousThoughtProcessor processor)
    {
        private readonly DigitalFaceControl _face = face ?? throw new ArgumentNullException(nameof(face));
        public readonly ConsciousThoughtProcessor _processor = processor ?? throw new ArgumentNullException(nameof(processor));

        /// <summary>
        /// Update the DigitalFaceControl with the latest neural state based on a thought and context signals.
        /// </summary>
        public void UpdateFaceFromThought(Thought thought, string[] memorySignals, string[] subconsciousSignals)
        {
            if (thought == null) throw new ArgumentNullException(nameof(thought));
            memorySignals ??= Array.Empty<string>();
            subconsciousSignals ??= Array.Empty<string>();

            // Run the processor to update thought.Score and produce a textual result
            var result = _processor.Process(thought, memorySignals, subconsciousSignals, associationLimit: 6);

            // Map textual cues and score to face state
            float activation = (float)Math.Min(1.0, Math.Abs(thought.Score));
            var mood = MapMood(result);

            // Derive a feature vector from tokens in the result
            var tokens = result.Split(new[] { ' ', ',', '.', ':', '-', '/', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var maxCount = GetMaxFeatureCount(_face);
            float[] features = tokens
                .Select(t => Math.Abs(t.GetHashCode()) % 1000 / 1000f)
                .Take(maxCount)
                .ToArray();

            _face.UpdateNeuralState(new NeuralState
            {
                Mood = mood,
                Activation = activation,
                Features = features
            });
        }

        private static DigitalFaceControl.Mood MapMood(string result)
        {
            var r = (result ?? string.Empty).ToLowerInvariant();
            if (r.Contains("happy")) return DigitalFaceControl.Mood.Happy;
            if (r.Contains("confused")) return DigitalFaceControl.Mood.Confused;
            if (r.Contains("thinking")) return DigitalFaceControl.Mood.Thinking;
            if (r.Contains("speaking")) return DigitalFaceControl.Mood.Speaking;
            return DigitalFaceControl.Mood.Neutral;
        }

        private static int GetMaxFeatureCount(DigitalFaceControl face)
        {
            // If the control exposes a particle/feature capacity, use it; otherwise default to 100.
            try
            {
                var prop = face.GetType().GetProperty("ParticleCount");
                if (prop?.GetValue(face) is int n && n > 0) return n;
            }
            catch { /* ignore */ }
            return 100;
        }
    }
}