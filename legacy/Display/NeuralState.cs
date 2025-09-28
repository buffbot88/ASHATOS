using System;
using System.Drawing;

namespace RaAI.Modules.DigitalFace
{
    /// <summary>
    /// Neural network output used to drive the DigitalFace animation.
    /// This is the bridge between higher-level cognition output and the renderer-facing FaceInputs.
    /// </summary>
    public sealed class NeuralState
    {
        // Affective state selected by higher-level modules
        public DigitalFaceControl.Mood Mood { get; set; } = DigitalFaceControl.Mood.Neutral;

        // Overall energy/intensity (0..1)
        public float Activation { get; set; } = 0.5f;

        // Arbitrary feature vector from the processor (used to modulate particle behavior, etc.)
        public float[] Features { get; set; } = Array.Empty<float>();

        // Optional audio amplitude (0..1) for mouth/halo dynamics
        public float AudioLevel { get; set; } = 0f;

        // Optional absolute gaze target (in surface coordinates); if set, normalized GazeX/Y will be computed.
        public PointF? GazeTarget { get; set; }

        // Normalized gaze offsets (-1..1). If GazeTarget is provided, these are computed from it.
        public float GazeX { get; set; } = 0f;
        public float GazeY { get; set; } = 0f;

        // Noise strength (0..1) for flow/particle fields
        public float ForceNoise { get; set; } = 0.5f;

        // Optional metadata about when this state was produced
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Convert this neural state into renderer-facing FaceInputs.
        /// If a GazeTarget is present, normalized gaze (GazeX/Y) is computed from the provided surface size.
        /// </summary>
        public FaceInputs ToFaceInputs(Size surfaceSize)
        {
            var inputs = new FaceInputs
            {
                Mood = Mood,
                Activation = Activation,
                Features = Features ?? Array.Empty<float>(),
                AudioLevel = Math.Clamp(AudioLevel, 0f, 1f),
                GazeTarget = GazeTarget,
                GazeX = Math.Clamp(GazeX, -1f, 1f),
                GazeY = Math.Clamp(GazeY, -1f, 1f),
                ForceNoise = Math.Clamp(ForceNoise, 0f, 1f)
            };

            // If absolute gaze is provided, derive normalized gaze offsets for renderers.
            if (GazeTarget.HasValue)
                inputs.UpdateGazeFromTarget(surfaceSize);

            return inputs;
        }
    }
}