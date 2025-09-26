using RaAI.Modules.SubconsciousModule.Storage;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RaAI.Modules.SubconsciousModule.Security
{
    // Simple ECDSA P-256 signer that can load PEM or auto-generate a keypair.
    public class DefaultECDsaSigner : ISigner { private ECDsa _ec; private bool _ownsKey = true;

    public DefaultECDsaSigner(string? privatePemPath = null, string? publicPemPath = null, bool autoGenerateIfMissing = true)
    {
        if (!string.IsNullOrEmpty(privatePemPath) && System.IO.File.Exists(privatePemPath))
        {
            var pem = System.IO.File.ReadAllText(privatePemPath);
            _ec = ECDsa.Create();
            try { _ec.ImportFromPem(pem.ToCharArray()); }
            catch
            {
                // fallback to new key
                _ec.Dispose();
                _ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            }
        }
        else if (!string.IsNullOrEmpty(publicPemPath) && System.IO.File.Exists(publicPemPath))
        {
            var pem = System.IO.File.ReadAllText(publicPemPath);
            _ec = ECDsa.Create();
            try { _ec.ImportFromPem(pem.ToCharArray()); }
            catch
            {
                _ec.Dispose();
                _ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            }
        }
        else
        {
            if (autoGenerateIfMissing)
                _ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            else
                throw new InvalidOperationException("No keys present and auto-generate disabled.");
        }
    }

    public byte[] Sign(byte[] data)
    {
        // data expected to be a precomputed hash (SHA256), but to be forgiving we hash if length != 32
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != 32)
        {
            using var sha = SHA256.Create();
            data = sha.ComputeHash(data);
        }
        return _ec.SignHash(data);
    }

    public string? ExportPublicKeyPem()
    {
        try
        {
            var pub = _ec.ExportSubjectPublicKeyInfo();
            var b64 = Convert.ToBase64String(pub);
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN PUBLIC KEY-----");
            for (int i = 0; i < b64.Length; i += 64)
                sb.AppendLine(b64.Substring(i, Math.Min(64, b64.Length - i)));
            sb.AppendLine("-----END PUBLIC KEY-----");
            return sb.ToString();
        }
        catch { return null; }
    }

    public void LoadKeysFromPem(string? privatePem, string? publicPem = null)
    {
        _ec?.Dispose();
        _ec = ECDsa.Create();
        if (!string.IsNullOrEmpty(privatePem))
        {
            _ec.ImportFromPem(privatePem.ToCharArray());
        }
        else if (!string.IsNullOrEmpty(publicPem))
        {
            _ec.ImportFromPem(publicPem.ToCharArray());
        }
    }

    public bool HasPrivateKey
    {
        get
        {
            try
            {
                // Try to export private key — if it fails it's public-only.
                _ec.ExportPkcs8PrivateKey();
                return true;
            }
            catch { return false; }
        }
    }

    public void Dispose()
    {
        _ec?.Dispose();
    }
}
}