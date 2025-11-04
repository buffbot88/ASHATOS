using System;
using System.Threading.Tasks;
using ASHATCore.Tests;

// Test the CMS Settings persistence
await CMSSettingsPersistenceTests.RunAllTests();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
