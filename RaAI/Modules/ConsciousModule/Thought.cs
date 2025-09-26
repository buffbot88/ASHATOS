using System;
using RaAI;

namespace RaAI.Modules.ConsciousModule 
{ 
    public class Thought
    { 
        public int Id { get; set; } 
        public string Content { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "user"; 
        public double Score { get; set; } = 0.0; 
        public override string ToString() => $"[{Timestamp:HH:mm:ss}] (#{Id}) {Content} (score={Score:F2})"; 
    } 
}