using System;
using System.Drawing;
using RaAI.Modules.SubconsciousModule.Core;

namespace RaAI.Modules.DigitalFace.Rendering
{
    public sealed class GdiParticleRenderer : IDigitalFaceRenderer
    {
        private Size _size;
        private FaceInputs _inputs = new();

        public void Initialize(Size size) { _size = size; }
        public void Resize(Size size) { _size = size; }

        public void Update(FaceInputs inputs, float dtSeconds)
        {
            _inputs = inputs ?? new FaceInputs();
            // Here you can apply additional effects (e.g., AudioLevel -> mouth curve)
        }

        public void Draw(Graphics g)
        {
            // For now, you can call into your DigitalFaceControl’s existing drawing pipeline
            // or move that logic here modularly over time.
            // This stub intentionally minimal to avoid large refactors in one step.
        }

        public void Dispose() { }
    }
}