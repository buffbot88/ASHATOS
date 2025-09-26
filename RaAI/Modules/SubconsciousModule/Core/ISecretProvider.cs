using System;

namespace RaAI.Modules.SubconsciousModule.Storage
{
    public interface ISecretProvider { string? GetPassphrase(); }

    public class EnvSecretProvider(string envName = "BOOKOFRA_PASSPHRASE") : ISecretProvider
    {
        private readonly string _envName = envName;

        public string? GetPassphrase() => Environment.GetEnvironmentVariable(_envName);
    }
}