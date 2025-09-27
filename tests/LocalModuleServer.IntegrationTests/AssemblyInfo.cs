using Xunit;

// Disable test parallelization for integration tests that use a shared port
[assembly: CollectionBehavior(DisableTestParallelization = true)]