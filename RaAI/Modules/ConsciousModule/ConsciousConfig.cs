using System;

namespace RaAI.Modules.ConsciousModule
{
    public static class ConsciousConfig
    {
        // Default configuration constants
        // These can be adjusted based on performance testing and requirements
        // Thought history limit determines how many past thoughts are retained for context
        // Association limit controls how many related concepts are linked to a thought
        // Feature vector size defines the dimensionality of the thought representation
        // Learning rate affects how quickly the model adapts to new information
        public const int ThoughtHistoryLimit = 200;
        public const int AssociationLimit = 6;
        public const int FeatureVectorSize = 1024;
        public const double LearningRate = 0.05;
        public const int SimulatedLatencyMs = 50;

        // Validation constants
        // These define acceptable ranges for configuration parameters
        // Ensuring parameters stay within these bounds helps maintain system stability
        // and performance
        // Adjust these values based on empirical testing and system capabilities
        // Min and max values for history limit, association limit, etc.
        public const int MinHistoryLimit = 10;
        public const int MaxHistoryLimit = 1000;
        public const int MinAssociationLimit = 1;
        public const int MaxAssociationLimit = 20;
    }
}