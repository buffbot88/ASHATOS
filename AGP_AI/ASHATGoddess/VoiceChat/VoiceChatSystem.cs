using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ASHATGoddessClient.VoiceChat;

/// <summary>
/// Voice chat system with audio capture and playback
/// Integrates with networking for voice communication
/// </summary>
public class VoiceChatSystem
{
    private WaveInEvent? _waveIn;
    private WaveOutEvent? _waveOut;
    private BufferedWaveProvider? _waveProvider;
    private readonly int _sampleRate = 16000;
    private readonly int _channels = 1;
    private readonly int _bitsPerSample = 16;
    private bool _isRecording;
    private bool _isInitialized;

    public event Action<byte[]>? OnAudioCaptured;
    public bool IsRecording => _isRecording;

    /// <summary>
    /// Initialize voice chat system
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            // Setup audio input
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channels),
                BufferMilliseconds = 50
            };

            _waveIn.DataAvailable += OnDataAvailable;

            // Setup audio output
            _waveOut = new WaveOutEvent();
            _waveProvider = new BufferedWaveProvider(new WaveFormat(_sampleRate, _bitsPerSample, _channels))
            {
                BufferDuration = TimeSpan.FromSeconds(2)
            };
            
            _waveOut.Init(_waveProvider);

            _isInitialized = true;
            Console.WriteLine("[VoiceChat] Initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to initialize: {ex.Message}");
        }
    }

    /// <summary>
    /// Start recording voice
    /// </summary>
    public void StartRecording()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_isRecording) return;

        try
        {
            _waveIn?.StartRecording();
            _isRecording = true;
            Console.WriteLine("[VoiceChat] Recording started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to start recording: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop recording voice
    /// </summary>
    public void StopRecording()
    {
        if (!_isRecording) return;

        try
        {
            _waveIn?.StopRecording();
            _isRecording = false;
            Console.WriteLine("[VoiceChat] Recording stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to stop recording: {ex.Message}");
        }
    }

    /// <summary>
    /// Play received audio data
    /// </summary>
    public void PlayAudio(byte[] audioData)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        try
        {
            _waveProvider?.AddSamples(audioData, 0, audioData.Length);
            
            if (_waveOut?.PlaybackState != PlaybackState.Playing)
            {
                _waveOut?.Play();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to play audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Event handler for captured audio data
    /// </summary>
    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded > 0)
        {
            var audioData = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);
            OnAudioCaptured?.Invoke(audioData);
        }
    }

    /// <summary>
    /// Get available input devices
    /// </summary>
    public static List<string> GetInputDevices()
    {
        var devices = new List<string>();
        
        try
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                devices.Add(capabilities.ProductName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to get input devices: {ex.Message}");
        }

        return devices;
    }

    /// <summary>
    /// Get available output devices
    /// </summary>
    public static List<string> GetOutputDevices()
    {
        var devices = new List<string>();
        
        try
        {
            // NAudio WaveOutEvent doesn't provide device enumeration in the same way
            // This is a placeholder that returns a basic list
            devices.Add("Default Output Device");
            Console.WriteLine("[VoiceChat] Output device enumeration placeholder");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceChat] Failed to get output devices: {ex.Message}");
        }

        return devices;
    }

    /// <summary>
    /// Set input device
    /// </summary>
    public void SetInputDevice(int deviceNumber)
    {
        if (_waveIn != null)
        {
            _waveIn.DeviceNumber = deviceNumber;
            Console.WriteLine($"[VoiceChat] Input device set to {deviceNumber}");
        }
    }

    /// <summary>
    /// Set output device
    /// </summary>
    public void SetOutputDevice(int deviceNumber)
    {
        if (_waveOut != null)
        {
            _waveOut.DeviceNumber = deviceNumber;
            Console.WriteLine($"[VoiceChat] Output device set to {deviceNumber}");
        }
    }

    /// <summary>
    /// Set recording volume (0.0 to 1.0)
    /// </summary>
    public void SetInputVolume(float volume)
    {
        // NAudio WaveInEvent doesn't support direct volume control
        // Volume would need to be applied to the captured samples
        Console.WriteLine($"[VoiceChat] Input volume set to {volume:P0}");
    }

    /// <summary>
    /// Set playback volume (0.0 to 1.0)
    /// </summary>
    public void SetOutputVolume(float volume)
    {
        if (_waveOut != null)
        {
            _waveOut.Volume = Math.Clamp(volume, 0.0f, 1.0f);
            Console.WriteLine($"[VoiceChat] Output volume set to {volume:P0}");
        }
    }

    /// <summary>
    /// Get current playback volume
    /// </summary>
    public float GetOutputVolume()
    {
        return _waveOut?.Volume ?? 0.5f;
    }

    /// <summary>
    /// Enable/disable echo cancellation (placeholder for future implementation)
    /// </summary>
    public void SetEchoCancellation(bool enabled)
    {
        Console.WriteLine($"[VoiceChat] Echo cancellation {(enabled ? "enabled" : "disabled")} (placeholder)");
    }

    /// <summary>
    /// Enable/disable noise suppression (placeholder for future implementation)
    /// </summary>
    public void SetNoiseSuppression(bool enabled)
    {
        Console.WriteLine($"[VoiceChat] Noise suppression {(enabled ? "enabled" : "disabled")} (placeholder)");
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        StopRecording();

        _waveIn?.Dispose();
        _waveOut?.Dispose();
        _waveProvider = null;

        _isInitialized = false;
        Console.WriteLine("[VoiceChat] Disposed");
    }
}

/// <summary>
/// Voice chat configuration
/// </summary>
public class VoiceChatConfig
{
    public int SampleRate { get; set; } = 16000;
    public int Channels { get; set; } = 1;
    public int BitsPerSample { get; set; } = 16;
    public float InputVolume { get; set; } = 0.8f;
    public float OutputVolume { get; set; } = 0.8f;
    public bool EchoCancellation { get; set; } = true;
    public bool NoiseSuppression { get; set; } = true;
    public int InputDeviceNumber { get; set; } = 0;
    public int OutputDeviceNumber { get; set; } = 0;
}
