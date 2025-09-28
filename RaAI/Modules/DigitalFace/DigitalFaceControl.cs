using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel; // added
using RaAI.Modules.DigitalFace.Rendering;

namespace RaAI.Modules.DigitalFace
{
    /// <summary>
    /// DigitalFaceControl: A renderer-driven animated AI face.
    /// The face is driven by NeuralState -> FaceInputs and drawn by an IDigitalFaceRenderer.
    /// This control is the UI surface; the actual visuals live in the renderer (e.g., GdiParticleRenderer).
    /// </summary>
    public class DigitalFaceControl : Control
    {
        #region Enums (public API compatibility)

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

        private System.Windows.Forms.Timer _animationTimer = null!;
        private readonly Stopwatch _clock = new();

        private IDigitalFaceRenderer _renderer;
        private FaceInputs _inputs = new();

        // Preserve public API properties/state
        private Mood _currentMood = Mood.Neutral;
        private float _activation = 0.5f;
        private float[] _features = Array.Empty<float>();

        // Extended state for renderer dynamics
        private float _audioLevel = 0f;       // 0..1
        private PointF? _gazeTarget;          // optional absolute coordinates
        private float _gazeX = 0f;            // normalized -1..1
        private float _gazeY = 0f;            // normalized -1..1
        private float _forceNoise = 0.5f;     // 0..1

        private bool _awake;

        #endregion

        #region Ctor

        public DigitalFaceControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;
            BackColor = Color.Black;

            // Default renderer (GDI+ particles). You can swap this at runtime if you have other implementations.
            _renderer = new GdiParticleRenderer();

            InitializeAnimation();
        }

        #endregion

        #region Public Properties (compatibility)

        // Helpful to consumers (historical API)
        [Browsable(false)]
        public int ParticleCapacity => 100; // kept for compatibility; not used by renderer directly

        [Browsable(false)]
        public Mood CurrentMood => _currentMood;

        [Browsable(false)]
        public float ActivationLevel => _activation;

        [Browsable(false)]
        public float[] CurrentFeatures => _features ?? Array.Empty<float>();

        // Extended properties (optional)

        [Category("DigitalFace")]
        [Description("Microphone/TTS amplitude (0..1) driving mouth/halo dynamics.")]
        [DefaultValue(0f)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public float AudioLevel
        {
            get => _audioLevel;
            set
            {
                var v = Math.Clamp(value, 0f, 1f);
                if (Math.Abs(v - _audioLevel) < 1e-6f) return;
                _audioLevel = v;
                Invalidate();
            }
        }

        // WinForms designer helpers so it can persist/reset the property
        public bool ShouldSerializeAudioLevel() => Math.Abs(_audioLevel - 0f) > 1e-6f;
        public void ResetAudioLevel() => AudioLevel = 0f;

        [Category("DigitalFace")]
        [Description("Optional absolute gaze target in surface coordinates.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PointF? GazeTarget
        {
            get => _gazeTarget;
            set { _gazeTarget = value; Invalidate(); }
        }

        [Category("DigitalFace")]
        [Description("Normalized horizontal gaze offset (-1..1).")]
        [DefaultValue(0f)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public float GazeX
        {
            get => _gazeX;
            set
            {
                var v = Math.Clamp(value, -1f, 1f);
                if (Math.Abs(v - _gazeX) < 1e-6f) return;
                _gazeX = v;
                Invalidate();
            }
        }
        public bool ShouldSerializeGazeX() => Math.Abs(_gazeX - 0f) > 1e-6f;
        public void ResetGazeX() => GazeX = 0f;

        [Category("DigitalFace")]
        [Description("Normalized vertical gaze offset (-1..1).")]
        [DefaultValue(0f)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public float GazeY
        {
            get => _gazeY;
            set
            {
                var v = Math.Clamp(value, -1f, 1f);
                if (Math.Abs(v - _gazeY) < 1e-6f) return;
                _gazeY = v;
                Invalidate();
            }
        }
        public bool ShouldSerializeGazeY() => Math.Abs(_gazeY - 0f) > 1e-6f;
        public void ResetGazeY() => GazeY = 0f;

        [Category("DigitalFace")]
        [Description("Noise strength (0..1) for particle/flow dynamics.")]
        [DefaultValue(0.5f)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public float ForceNoise
        {
            get => _forceNoise;
            set
            {
                var v = Math.Clamp(value, 0f, 1f);
                if (Math.Abs(v - _forceNoise) < 1e-6f) return;
                _forceNoise = v;
                Invalidate();
            }
        }
        public bool ShouldSerializeForceNoise() => Math.Abs(_forceNoise - 0.5f) > 1e-6f;
        public void ResetForceNoise() => ForceNoise = 0.5f;

        #endregion

        #region Public Methods (compatibility)

        public void SetMood(Mood mood)
        {
            _currentMood = mood;
            Invalidate();
        }

        public void UpdateNeuralState(NeuralState state)
        {
            if (state == null) return;

            // Map NeuralState into control state; renderer inputs are built each frame
            _currentMood = (Mood)state.Mood; // same enum values expected
            _activation = Math.Clamp(state.Activation, 0f, 1f);
            _features = state.Features ?? Array.Empty<float>();

            // Optional extended fields if provided
            AudioLevel = state.AudioLevel; // go through property to keep designer helpers consistent
            _gazeTarget = state.GazeTarget;
            GazeX = state.GazeX;
            GazeY = state.GazeY;
            ForceNoise = state.ForceNoise;

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

        // Allow swapping the renderer at runtime (optional)
        public void SetRenderer(IDigitalFaceRenderer renderer)
        {
            if (renderer == null) return;
            _renderer?.Dispose();
            _renderer = renderer;
            try { _renderer.Initialize(ClientSize); } catch { /* ignore */ }
            Invalidate();
        }

        // Ordered wake hook (Memory -> Subconscious -> Conscious -> Speech -> DigitalFace)
        // External orchestrator can call this to indicate the face should be active.
        public void OnWake()
        {
            _awake = true;

            // Best-effort: if the renderer exposes OnWake(), call it reflectively (optional)
            try
            {
                var mi = _renderer.GetType().GetMethod("OnWake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                mi?.Invoke(_renderer, null);
            }
            catch { /* ignore */ }
        }

        #endregion

        #region Animation Loop

        private void InitializeAnimation()
        {
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
            _animationTimer.Tick += (sender, e) => Animate();
            try
            {
                _renderer.Initialize(ClientSize);
            }
            catch { /* ignore */ }

            _clock.Restart();
            _animationTimer.Start();
        }

        private void Animate()
        {
            if (!Visible || Width <= 0 || Height <= 0) return;

            // Compute dt
            float dt = Math.Min(0.1f, (float)_clock.Elapsed.TotalSeconds); // clamp to avoid spikes
            _clock.Restart();

            // Build FaceInputs from current control state
            _inputs = new FaceInputs
            {
                Mood = (DigitalFaceControl.Mood)_currentMood,
                Activation = _activation,
                Features = _features ?? Array.Empty<float>(),
                AudioLevel = _audioLevel,
                GazeTarget = _gazeTarget,
                GazeX = _gazeX,
                GazeY = _gazeY,
                ForceNoise = _forceNoise
            };

            // If absolute gaze target provided, compute normalized gaze relative to surface
            if (_gazeTarget.HasValue)
                _inputs.UpdateGazeFromTarget(ClientSize);

            try
            {
                _renderer.Update(_inputs, dt);
            }
            catch { /* ignore to keep UI responsive */ }

            Invalidate();
        }

        #endregion

        #region Paint/Resize/Dispose

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            try
            {
                _renderer.Draw(e.Graphics);
            }
            catch
            {
                // As a minimal fallback, clear the background if renderer failed
                e.Graphics.Clear(BackColor);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            try { _renderer.Resize(ClientSize); } catch { /* ignore */ }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _animationTimer?.Stop(); } catch { }
                _animationTimer?.Dispose();
                _renderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}