using System;

namespace RaAI.Handlers 
{ 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)] 
    public sealed class RaModuleAttribute : Attribute { public string? Category { get; set; } 
        public RaModuleAttribute() { } 
        public RaModuleAttribute(string category) { Category = category; } 
    } 
}