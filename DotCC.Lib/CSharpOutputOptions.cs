namespace DotCC;

/// <summary>Runtime source included in final C# output. Auto follows source/object language provenance.</summary>
public enum RuntimeProfile { All, C, Auto }

/// <summary>Final output layout; set at link time when using object fragments.</summary>
public sealed record CSharpOutputOptions(bool NestTypes = false, RuntimeProfile Runtime = RuntimeProfile.All);
