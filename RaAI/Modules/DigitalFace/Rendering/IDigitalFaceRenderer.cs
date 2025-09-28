using System;
using System.Drawing;

namespace RaAI.Modules.DigitalFace.Rendering
{
    public sealed class FaceInputs
    {
        // Overall affect/state chosen by higher-level modules (e.g., Conscious)
        public DigitalFaceControl.Mood Mood { get; set; } = DigitalFaceControl.Mood.Neutral;

        // General activation/energy level (0..1)
        public float Activation { get; set; } = 0.5f;

        // Optional feature vector for renderer-specific effects
        public float[] Features { get; set; } = Array.Empty<float>();

        // Microphone/TTS amplitude (0..1) driving mouth/halo dynamics
        public float AudioLevel { get; set; } = 0f;

        // Optional absolute gaze target (in control/surface coordinates)
        public PointF? GazeTarget { get; set; }

        // Normalized gaze offsets (-1..1) used by renderers; can be set directly
        // or computed from GazeTarget via helper below.
        public float GazeX { get; set; } = 0f;
        public float GazeY { get; set; } = 0f;

        // Noise strength for flow/particle fields (0..1)
        public float ForceNoise { get; set; } = 0.5f;

        // Helper: update normalized gaze from absolute target given current surface size.
        // Produces approximately -1..1 offsets relative to center.
        public void UpdateGazeFromTarget(Size surfaceSize)
        {
            if (GazeTarget == null || surfaceSize.Width <= 0 || surfaceSize.Height <= 0) return;

            var cx = surfaceSize.Width * 0.5f;
            var cy = surfaceSize.Height * 0.5f;
            var dx = GazeTarget.Value.X - cx;
            var dy = GazeTarget.Value.Y - cy;

            // Normalize by half-dimensions so that edges are near +/-1
            var nx = (surfaceSize.Width > 0) ? dx / Math.Max(1f, cx) : 0f;
            var ny = (surfaceSize.Height > 0) ? dy / Math.Max(1f, cy) : 0f;

            GazeX = Math.Clamp(nx, -1f, 1f);
            GazeY = Math.Clamp(ny, -1f, 1f);
        }
    }

    public interface IDigitalFaceRenderer : IDisposable
    {
        void Initialize(Size size);
        void Resize(Size size);
        void Update(FaceInputs inputs, float dtSeconds);
        void Draw(Graphics g); // GDI phase; replace with Skia/DirectX in alt impls
    }
}