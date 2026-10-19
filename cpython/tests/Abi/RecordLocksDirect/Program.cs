using DotCC.Tests;

if (!OperatingSystem.IsLinux() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
    throw new PlatformNotSupportedException("Linux x64 LP64 record-lock control requires its real OS ABI");
Console.WriteLine("record-lock direct passed checks=" + CPythonR9RecordLockChecks.RunAll());
