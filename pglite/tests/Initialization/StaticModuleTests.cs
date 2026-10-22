using Probe = Managed.PGlite.Tests.StaticModules;

unsafe
{
    using var first = Probe.__DotCcCreateContext();
    using var second = Probe.__DotCcCreateContext();
    using (first.Enter())
    {
        if (Probe.dpg_static_module_probe() != 0) throw new InvalidOperationException("Static module contracts failed.");
        if (Probe.dpg_static_register_fixture() != 0) throw new InvalidOperationException("First registration failed.");
        void* handle = Probe.dpg_static_open_fixture();
        if (handle == null) throw new InvalidOperationException("First open failed.");
        using (second.Enter())
        {
            if (Probe.dpg_static_open_fixture() != null) throw new InvalidOperationException("A static registry leaked to another owner.");
            if (Probe.dpg_static_module_probe() != 0) throw new InvalidOperationException("Second static registry contracts failed.");
        }
        if (Probe.dpg_static_dlclose(handle) != 0 || Probe.dpg_static_unregister() != 0)
            throw new InvalidOperationException("Owner binding did not restore the original static registry.");
    }
}
Console.WriteLine("PASS native/emitted static symbol contracts, managed function-pointer invocation, errors/refcounts, two owner registries");
