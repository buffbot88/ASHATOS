using System;

[AttributeUsage(AttributeTargets.Class)]
public class RaModuleTypeAttribute : Attribute
{
    public string Type { get; }
    public RaModuleTypeAttribute(string type) => Type = type;
}
