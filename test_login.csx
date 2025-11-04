#r "ASHATCore/bin/Debug/net9.0/ASHATCore.dll"
#r "ASHATCore/bin/Debug/net9.0/Abstractions.dll"

using ASHATCore.Tests;

Console.WriteLine("Running Login Persistence Tests...\n");
LoginPersistenceTests.RunTests();
Console.WriteLine("\nTests completed!");
