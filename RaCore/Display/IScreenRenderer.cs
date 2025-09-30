using System.Threading;

namespace RaCore.Display;

// Rendering-agnostic face contract extended to accept arbitrary anchor "sketches"
public interface IFaceRenderer
{
    string Id { get; }

    void OnWake();
    void SetMood(string mood);
    void SetAttention(double level); // 0..1
    void SetAudioLevel(double rms);  // 0..1
    void Blink();

    // NEW: Arbitrary anchor sketch for the particle canvas (JSON string, schema-agnostic at core level)
    // Renderer decides how to interpret and visualize.
    void SetAnchors(string anchorsJson);
}
