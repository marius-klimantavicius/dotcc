namespace Managed.Emulation;

/// <summary>Resolves only explicit or application-local worker assets. It never
/// changes execution mode or searches PATH for an arbitrary Blink worker.</summary>
public static class WorkerDiscovery
{
    public static WorkerLaunch Discover(string? directory = null)
    {
        string root = Path.GetFullPath(directory ?? Path.Combine(AppContext.BaseDirectory, "blink-worker"));
        string native = Path.Combine(root, OperatingSystem.IsWindows() ? "Managed.Emulation.Worker.exe" : "Managed.Emulation.Worker");
        if (File.Exists(native)) return new(native, []);
        string assembly = Path.Combine(root, "Managed.Emulation.Worker.dll");
        if (File.Exists(assembly) && File.Exists(Path.Combine(root, "Managed.Emulation.Worker.runtimeconfig.json")))
            return new("dotnet", [assembly]);
        throw new FileNotFoundException("Separate-process execution requires the published Managed.Emulation.Worker assets in 'blink-worker' beside the application, or an explicit MachineOptions.Worker. No execution-mode fallback is performed.");
    }
}
