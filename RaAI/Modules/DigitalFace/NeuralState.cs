using System;

namespace RaAI.Modules.DigitalFace
{
    /// <summary>
    /// Neural network output used to drive the DigitalFace animation.
    /// </summary>
    public sealed class NeuralState
    {
        public DigitalFaceControl.Mood Mood { get; set; } = DigitalFaceControl.Mood.Neutral;

        // 0.0 - 1.0 activation level (overall energy/intensity)
        public float Activation { get; set; }

        // Arbitrary feature vector from the processor (used to modulate particle behavior)
        public float[] Features { get; set; } = Array.Empty<float>();
    }
}