using System;
using System.Drawing;

namespace RaAI.Modules.DigitalFace.Rendering
{
    public sealed class FaceInputs
    {
        public DigitalFaceControl.Mood Mood { get; set; } = DigitalFaceControl.Mood.Neutral;
        public float Activation { get; set; } = 0.5f;
        public float[] Features { get; set; } = Array.Empty<float>();
        public float AudioLevel { get; set; } = 0f;        // 0..1 mic/TTS amplitude
        public PointF? GazeTarget { get; set; }            // optional eye target
        public float ForceNoise { get; set; } = 0.5f;      // noise strength for flow fields
    }

    public interface IDigitalFaceRenderer : IDisposable
    {
        void Initialize(Size size);
        void Resize(Size size);
        void Update(FaceInputs inputs, float dtSeconds);
        void Draw(Graphics g); // GDI phase; replace with Skia/DirectX in alt impls
    }
}