using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules;
using System;

namespace RaCore;
    class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: GenerateLicenseKey <userId> <instanceName> <licenseType> <durationYears>");
            return;
        }

        var userId = Guid.Parse(args[0]);
        var instanceName = args[1];
        var licenseType = (LicenseType)Enum.Parse(typeof(LicenseType), args[2], true);
        var durationYears = int.Parse(args[3]);

        var moduleManager = new ModuleManager();
        var licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;

        var license = licenseModule.CreateAndAssignLicense(userId, instanceName, licenseType, durationYears);

        Console.WriteLine($"License Key: {license.LicenseKey}");
        Console.WriteLine($"License Type: {license.Type}");
        Console.WriteLine($"Expires At: {license.ExpiresAtUtc}");
    }
}