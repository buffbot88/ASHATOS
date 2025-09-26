using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Collections.Generic;
using RaAI.Modules.DigitalFace;

namespace RaAI.Modules.DigitalFace
{
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
        private const int ParticleCount = 100;
        private const int BackgroundParticleCount = 50;

        // Fix: Initialize all non-nullable fields in the constructor to satisfy CS8618

        private readonly Bitmap _faceBitmap = new(FaceWidth, FaceHeight);
        private System.Windows.Forms.Timer _animationTimer = null!;
        private Mood _currentMood = Mood.Neutral;
        private readonly Random _random = new();
        private List<Particle> _particles = null!;
        private List<BackgroundParticle> _backgroundParticles = null!;

        public DigitalFaceControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.Opaque |
                     ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;
            Size = new Size(FaceWidth, FaceHeight);
            BackColor = Color.Black;

            _animationTimer = null!; // Will be initialized in InitializeAnimation
            _particles = [];
            _backgroundParticles = [];

            InitializeParticles();
            InitializeAnimation();
        }

        #endregion

        #region Public Methods

        public void SetMood(Mood mood)
        {
            _currentMood = mood;
            Invalidate();
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
            // Animate particles
            foreach (var particle in _particles)
            {
                particle.Update();
            }

            // Animate background particles
            foreach (var particle in _backgroundParticles)
            {
                particle.Update();
            }

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

            // Draw face outline
            var faceRect = new Rectangle((Width - FaceWidth) / 2, (Height - FaceHeight) / 2, FaceWidth, FaceHeight);
            using (var pen = new Pen(Color.FromArgb(60, 100, 180), 2))
            {
                g.DrawArc(pen, faceRect.X + 10, faceRect.Y + 20, faceRect.Width - 20, faceRect.Height - 40, 0, 180); // Head
                g.DrawArc(pen, faceRect.X + 25, faceRect.Y + 80, 50, 70, 180, 180); // Left ear
                g.DrawArc(pen, faceRect.X + 125, faceRect.Y + 80, 50, 70, 0, 180); // Right ear
                g.DrawArc(pen, faceRect.X + 35, faceRect.Y + 120, 130, 90, 0, 180); // Jawline

                // Eyes
                g.DrawEllipse(pen, faceRect.X + 45, faceRect.Y + 50, 30, 20); // Left eye
                g.DrawEllipse(pen, faceRect.X + 125, faceRect.Y + 50, 30, 20); // Right eye

                // Nose
                g.DrawLine(pen, faceRect.X + 75, faceRect.Y + 85, faceRect.X + 75, faceRect.Y + 105);

                // Mouth
                switch (_currentMood)
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

            // Draw glowing eyes
            var leftEyeCenter = new Point(faceRect.X + 60, faceRect.Y + 60);
            var rightEyeCenter = new Point(faceRect.X + 140, faceRect.Y + 60);

            // Eye glow effect
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

            // Particles around head
            foreach (var particle in _particles)
            {
                using var brush = new SolidBrush(particle.Color);
                g.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
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
                Size = _random.Next(1, 3);
                Color = Color.FromArgb(_random.Next(50, 150), 100, 150, 255);
                _speed = _random.Next(1, 5) * 0.1f;
                _angle = (float)(_random.NextDouble() * Math.PI * 2);
                _distance = _random.Next(30, 80);
            }

            public void Update()
            {
                _angle += _speed;
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
                Size = _random.Next(1, 2);
                Color = Color.FromArgb(_random.Next(20, 80), 100, 150, 255);
            }

            public void Update()
            {
                // Slowly fade and reappear randomly
                if (_random.Next(100) == 0)
                {
                    Reset();
                }
            }
        }

        #endregion
    }
}