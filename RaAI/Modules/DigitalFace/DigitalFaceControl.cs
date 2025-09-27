using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RaAI.Modules.DigitalFace
{
    /// <summary>
    /// DigitalFaceControl: A particle-based animated AI face.
    /// Blue particles form the head/face and reflect mood, with glowing eyes.
    /// Designed for extensibility and neural network integration.
    /// </summary>
    public class DigitalFaceControl : Control
    {
        #region Enums

        public enum Mood
        {
            Neutral,
            Thinking,
            Speaking,
            Confused,
            Happy
        }

        #endregion

        #region Fields

        private const int FaceWidth = 200;
        private const int FaceHeight = 250;
        private const int EyeGlowRadius = 15;
        public const int ParticleCount = 100;
        private const int BackgroundParticleCount = 50;

        private System.Windows.Forms.Timer _animationTimer = null!;
        private Mood _currentMood = Mood.Neutral;
        private float _neuralActivation = 0.5f;
        private float[] _neuralFeatures = Array.Empty<float>();
        private readonly Random _random = new();
        private List<Particle> _particles = null!;
        private List<BackgroundParticle> _backgroundParticles = null!;

        #endregion

        #region Ctor

        public DigitalFaceControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.Opaque |
                     ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;
            Size = new Size(FaceWidth, FaceHeight);
            BackColor = Color.Black;

            _particles = [];
            _backgroundParticles = [];

            InitializeParticles();
            InitializeAnimation();
        }

        #endregion

        #region Public Properties

        // Helpful to consumers (e.g., ThoughtClient) for sizing feature vectors
        public int ParticleCapacity => ParticleCount;

        public Mood CurrentMood => _currentMood;

        public float ActivationLevel => _neuralActivation;

        public float[] CurrentFeatures => _neuralFeatures ?? Array.Empty<float>();

        #endregion

        #region Public Methods

        public void SetMood(Mood mood)
        {
            _currentMood = mood;
            Invalidate();
        }

        public void UpdateNeuralState(NeuralState state)
        {
            if (state == null) return;

            _currentMood = state.Mood;
            _neuralActivation = Math.Clamp(state.Activation, 0.0f, 1.0f);
            _neuralFeatures = state.Features ?? Array.Empty<float>();
            Invalidate();
        }

        // Safe call from non-UI threads
        public void UpdateNeuralStateSafe(NeuralState state)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => UpdateNeuralState(state))); } catch { /* ignore */ }
            }
            else
            {
                UpdateNeuralState(state);
            }
        }

        #endregion

        #region Private Methods

        private void InitializeParticles()
        {
            _particles = [];
            for (int i = 0; i < ParticleCount; i++)
            {
                _particles.Add(new Particle(_random, Width, Height));
            }

            _backgroundParticles = [];
            for (int i = 0; i < BackgroundParticleCount; i++)
            {
                _backgroundParticles.Add(new BackgroundParticle(_random, Width, Height));
            }
        }

        private void InitializeAnimation()
        {
            _animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animationTimer.Tick += (sender, e) => Animate();
            _animationTimer.Start();
        }

        private void Animate()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                float feature = (_neuralFeatures.Length > i) ? _neuralFeatures[i] : _neuralActivation;
                _particles[i].Update(_currentMood, _neuralActivation, feature);
            }

            foreach (var particle in _backgroundParticles)
                particle.Update();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            // Draw background particles
            foreach (var particle in _backgroundParticles)
            {
                using var brush = new SolidBrush(particle.Color);
                g.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
            }

            // Draw face using particles: head/face area
            foreach (var particle in _particles)
            {
                using var brush = new SolidBrush(particle.Color);
                g.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
            }

            // Draw glowing eyes (fixed to head center)
            var faceRect = new Rectangle((Width - FaceWidth) / 2, (Height - FaceHeight) / 2, FaceWidth, FaceHeight);
            var leftEyeCenter = new Point(faceRect.X + 60, faceRect.Y + 60);
            var rightEyeCenter = new Point(faceRect.X + 140, faceRect.Y + 60);

            using (var glowBrush = new SolidBrush(Color.FromArgb(150, 0, 150, 255)))
            {
                for (int radius = EyeGlowRadius; radius > 0; radius--)
                {
                    float alpha = (float)(radius * 0.05);
                    using var brush = new SolidBrush(Color.FromArgb((int)(alpha * 255), 0, 150, 255));
                    g.FillEllipse(brush,
                                  leftEyeCenter.X - radius,
                                  leftEyeCenter.Y - radius,
                                  radius * 2, radius * 2);

                    g.FillEllipse(brush,
                                  rightEyeCenter.X - radius,
                                  rightEyeCenter.Y - radius,
                                  radius * 2, radius * 2);
                }
            }

            // Eye pupils
            using (var pupilBrush = new SolidBrush(Color.FromArgb(255, 100, 255, 255)))
            {
                g.FillEllipse(pupilBrush, leftEyeCenter.X - 3, leftEyeCenter.Y - 3, 6, 6);
                g.FillEllipse(pupilBrush, rightEyeCenter.X - 3, rightEyeCenter.Y - 3, 6, 6);
            }

            // Draw mouth by mood (particle smile/frown, or direct arc)
            DrawMouth(g, faceRect, _currentMood);
        }

        private void DrawMouth(Graphics g, Rectangle faceRect, Mood mood)
        {
            using var pen = new Pen(Color.FromArgb(60, 100, 180), 2);
            switch (mood)
            {
                case Mood.Neutral:
                    g.DrawArc(pen, faceRect.X + 55, faceRect.Y + 110, 40, 20, 0, 180);
                    break;
                case Mood.Thinking:
                    g.DrawString("?", new Font("Arial", 24, FontStyle.Bold), Brushes.White,
                                 faceRect.X + 67, faceRect.Y + 115);
                    break;
                case Mood.Speaking:
                    g.DrawArc(pen, faceRect.X + 55, faceRect.Y + 110, 40, 20, 0, -180);
                    break;
                case Mood.Confused:
                    g.DrawArc(pen, faceRect.X + 55, faceRect.Y + 110, 40, 20, 0, 180);
                    break;
                case Mood.Happy:
                    g.DrawArc(pen, faceRect.X + 55, faceRect.Y + 115, 40, 15, 0, -180);
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InitializeParticles();
        }

        #endregion

        #region Helper Classes

        private class Particle
        {
            private readonly Random _random;
            private readonly int _maxX;
            private readonly int _maxY;
            private float _speed;
            private float _angle;
            private float _distance;

            public float X { get; private set; }
            public float Y { get; private set; }
            public float Size { get; private set; }
            public Color Color { get; private set; }

            public Particle(Random random, int maxX, int maxY)
            {
                _random = random;
                _maxX = maxX;
                _maxY = maxY;
                Reset();
            }

            public void Reset()
            {
                X = _random.Next(_maxX);
                Y = _random.Next(_maxY);
                Size = _random.Next(2, 5);
                Color = Color.FromArgb(_random.Next(120, 200), 0, 120 + _random.Next(60), 255);
                _speed = _random.Next(1, 5) * 0.1f;
                _angle = (float)(_random.NextDouble() * Math.PI * 2);
                _distance = _random.Next(60, 90);
            }

            public void Update(Mood mood, float activation, float feature)
            {
                // Mood/activation/feature can affect particle dynamics and color
                float moodFactor = mood switch
                {
                    Mood.Happy => 1.2f + activation,
                    Mood.Thinking => 0.8f + feature,
                    Mood.Speaking => 1.0f + feature,
                    Mood.Confused => 0.7f + activation,
                    _ => 1.0f + activation
                };

                int blue = (int)(150 + 105 * activation);
                int alpha = (int)(120 + 120 * feature);
                Color = Color.FromArgb(alpha, 0, blue, 255);

                _angle += _speed * moodFactor;
                _distance += _random.Next(-1, 2) * moodFactor;
                X = (float)(_maxX / 2 + Math.Cos(_angle) * _distance);
                Y = (float)(_maxY / 2 + Math.Sin(_angle) * _distance);

                if (X < 0 || X > _maxX || Y < 0 || Y > _maxY)
                {
                    Reset();
                }
            }
        }

        private class BackgroundParticle
        {
            private readonly Random _random;
            private readonly int _maxX;
            private readonly int _maxY;

            public float X { get; private set; }
            public float Y { get; private set; }
            public float Size { get; private set; }
            public Color Color { get; private set; }

            public BackgroundParticle(Random random, int maxX, int maxY)
            {
                _random = random;
                _maxX = maxX;
                _maxY = maxY;
                Reset();
            }

            public void Reset()
            {
                X = _random.Next(_maxX);
                Y = _random.Next(_maxY);
                Size = _random.Next(1, 3);
                Color = Color.FromArgb(_random.Next(40, 90), 100, 150, 255);
            }

            public void Update()
            {
                if (_random.Next(100) == 0)
                {
                    Reset();
                }
            }
        }

        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _animationTimer?.Stop(); } catch { }
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}