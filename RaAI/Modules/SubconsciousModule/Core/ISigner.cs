using System;

namespace RaAI.Modules.SubconsciousModule.Storage
{
    public interface ISigner : IDisposable
    {
        // Sign raw bytes (preferably a 32-byte SHA256 hash). Returns raw signature bytes.
        byte[] Sign(byte[] data);

        // Export the public key in PEM format (or null on failure).
        string? ExportPublicKeyPem();

        // Load keys from PEM string(s). privatePem can be null to set only a public key.
        void LoadKeysFromPem(string? privatePem, string? publicPem = null);

        // Whether a private key is available for signing.
        bool HasPrivateKey { get; }
    }
}