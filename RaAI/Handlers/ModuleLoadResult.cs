using System.Collections.Generic;

namespace RaAI.Handlers 
{ 
    public class ModuleLoadResult
    { 
        public List<ModuleWrapperView> Loaded { get; } = new(); 
        public List<string> Errors { get; } = new(); 
    } 
}