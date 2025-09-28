using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using RaAI.Modules.SubconsciousModule.Core;

namespace RaAI.Modules.DigitalFace.Rendering
{
    public sealed class GdiParticleRenderer : IDigitalFaceRenderer, IDisposable
    {
        private Size _size;
        private FaceInputs _inputs = new();

        // Optional wake state (DigitalFace module can call OnWake reflectively)
        private bool _awake;
        private float _time; // seconds accumulator for simple animation
        private readonly Random _rnd = new(1337);

        // Cached pens/brushes
        private Pen? _accentPen;
        private Pen? _dimPen;
        private Brush? _accentBrush;
        private Brush? _dimBrush;
        private Color _accent = Color.MediumSpringGreen;
        private Color _dim = Color.FromArgb(50, 50, 50);

        public void Initialize(Size size)
        {
            _size = size;
            EnsureResources();
        }

        public void Resize(Size size)
        {
            _size = size;
        }

        public void Update(FaceInputs inputs, float dtSeconds)
        {
            _inputs = inputs ?? new FaceInputs();
            _time += Math.Max(0, dtSeconds);
            if (_time > 1_000_000f) _time = 0f; // keep it bounded
        }

        public void Draw(Graphics g)
        {
            if (g == null) return;
            EnsureResources();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // Canvas metrics
            float w = _size.Width <= 0 ? g.VisibleClipBounds.Width : _size.Width;
            float h = _size.Height <= 0 ? g.VisibleClipBounds.Height : _size.Height;
            var cx = w * 0.5f;
            var cy = h * 0.5f;
            var scale = Math.Min(w, h);

            // Pulse (breathing) influenced by AudioLevel if available
            float audio = GetFloatInput(nameof(FaceInputs.AudioLevel), defaultValue: 0f);
            audio = Math.Clamp(audio, 0f, 1f);
            float pulse = (float)(0.5 + 0.5 * Math.Sin(_time * (1.5 + audio * 4.0))); // 0..1
            float haloR = scale * (0.22f + 0.02f * pulse);

            // 1) Halo particles around the face center
            int particles = 64;
            for (int i = 0; i < particles; i++)
            {
                float t = (i / (float)particles) * (float)(Math.PI * 2.0);
                float jitter = (float)(_rnd.NextDouble() * 0.12 - 0.06); // slight angular jitter
                float a = t + _time * (0.2f + audio * 0.8f) + jitter;

                float pr = haloR * (0.85f + 0.25f * (float)Math.Sin(a * 3.0));
                float px = cx + (float)Math.Cos(a) * pr;
                float py = cy + (float)Math.Sin(a) * pr;

                int alpha = (int)(80 + 100 * pulse);
                using var pBrush = new SolidBrush(Color.FromArgb(Math.Clamp(alpha, 20, 200), _accent));
                float sz = 1.5f + 2.5f * pulse;
                g.FillEllipse(pBrush, px - sz * 0.5f, py - sz * 0.5f, sz, sz);
            }

            // 2) Eyes (positions respond slightly to optional GazeX/GazeY if present)
            float gazeX = GetFloatInput("GazeX", 0f); // -1..1 expected
            float gazeY = GetFloatInput("GazeY", 0f); // -1..1 expected
            gazeX = Math.Clamp(gazeX, -1f, 1f);
            gazeY = Math.Clamp(gazeY, -1f, 1f);

            float eyeOffsetX = scale * 0.10f;
            float eyeOffsetY = scale * -0.03f;
            float eyeR = scale * 0.012f + scale * 0.006f * pulse;

            var leftEye = new PointF(cx - eyeOffsetX + gazeX * scale * 0.01f, cy + eyeOffsetY + gazeY * scale * 0.01f);
            var rightEye = new PointF(cx + eyeOffsetX + gazeX * scale * 0.01f, cy + eyeOffsetY + gazeY * scale * 0.01f);

            g.FillEllipse(_accentBrush!, leftEye.X - eyeR, leftEye.Y - eyeR, eyeR * 2, eyeR * 2);
            g.FillEllipse(_accentBrush!, rightEye.X - eyeR, rightEye.Y - eyeR, eyeR * 2, eyeR * 2);

            // 3) Mouth (arc curvature reacts to AudioLevel)
            float mouthW = scale * 0.14f;
            float mouthH = scale * (0.015f + 0.02f * audio);
            var mouthRect = new RectangleF(cx - mouthW * 0.5f, cy + scale * 0.06f - mouthH * 0.5f, mouthW, mouthH);
            float startAngle = 10f + 10f * pulse;
            float sweepAngle = 160f - 20f * pulse;
            g.DrawArc(_accentPen!, mouthRect, 180f - startAngle, sweepAngle);

            // 4) Subtle center nucleus
            float nucleusR = scale * (0.01f + 0.008f * pulse);
            var nucleusAlpha = (int)(170 + 60 * pulse);
            using var nucleusBrush = new SolidBrush(Color.FromArgb(Math.Clamp(nucleusAlpha, 0, 230), _accent));
            g.FillEllipse(nucleusBrush, cx - nucleusR, cy - nucleusR, nucleusR * 2, nucleusR * 2);
        }

        // Optional hook – DigitalFace module may call this after ordered wake
        public void OnWake()
        {
            _awake = true;
        }

        public void Dispose()
        {
            _accentPen?.Dispose();
            _dimPen?.Dispose();
            _accentBrush?.Dispose();
            _dimBrush?.Dispose();
        }

        // ----------------- helpers -----------------

        private void EnsureResources()
        {
            _accentPen ??= new Pen(_accent, 2.0f);
            _dimPen ??= new Pen(_dim, 1.0f);
            _accentBrush ??= new SolidBrush(_accent);
            _dimBrush ??= new SolidBrush(_dim);
        }

        private float GetFloatInput(string propertyName, float defaultValue)
        {
            try
            {
                var pi = typeof(FaceInputs).GetProperty(propertyName);
                if (pi == null) return defaultValue;
                var val = pi.GetValue(_inputs);
                if (val == null) return defaultValue;

                // Handle common numeric types
                if (val is float f) return f;
                if (val is double d) return (float)d;
                if (val is int i) return i;
                if (float.TryParse(val.ToString(), out var parsed)) return parsed;
            }
            catch
            {
                // ignore and fall through
            }
            return defaultValue;
        }
    }
}