using System;
using System.Threading.Tasks;
using ASHATCore.Tests;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting CMS Settings Persistence Tests...\n");
        await CMSSettingsPersistenceTests.RunAllTests();
        Console.WriteLine("\nTests completed.");
    }
}
