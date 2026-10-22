using Probe = Managed.PGlite.Tests.OptionProbe;

using var first = Probe.__DotCcCreateContext();
using var second = Probe.__DotCcCreateContext();
using (first.Enter())
{
    Probe.Libc.optind = 91;
    if (Probe.dpg_getopt_index() != 1 || Probe.dpg_getopt_probe() != 73 || Probe.dpg_getopt_index() != 3)
        throw new InvalidOperationException("Original getopt globals did not shadow the host cursor.");
    if (Probe.Libc.optind != 91) throw new InvalidOperationException("Original parser overwrote the runtime cursor.");
    Probe.dpg_getopt_set_index(17);
    if (Probe.dpg_static_module_probe() != 0) throw new InvalidOperationException("Static translated symbol provider failed.");
    if (Probe.dpg_static_register_fixture() != 0) throw new InvalidOperationException("Cannot register retained fixture.");
    unsafe
    {
        void* handle = Probe.dpg_static_open_fixture();
        if (handle == null) throw new InvalidOperationException("Cannot open retained fixture.");
        using (second.Enter())
        {
            byte[] name = "twice\0"u8.ToArray();
            fixed (byte* p = name)
                if (Probe.dpg_static_dlsym(handle, p) != null || Probe.Libc.errno != Probe.Libc.EBADF)
                    throw new InvalidOperationException("Foreign owner resolved another module handle.");
            if (Probe.dpg_static_dlclose(handle) != -1 || Probe.Libc.errno != Probe.Libc.EBADF)
                throw new InvalidOperationException("Foreign owner closed another module handle.");
        }
        if (Probe.dpg_static_dlclose(handle) != 0 || Probe.dpg_static_unregister() != 0)
            throw new InvalidOperationException("Foreign lookup changed the retained owner.");
    }
    using (second.Enter())
    {
        if (Probe.Libc.optind != 1 || Probe.dpg_getopt_index() != 1 || Probe.dpg_getopt_probe() != 73)
            throw new InvalidOperationException("Original getopt globals leaked between translated owners.");
        if (Probe.dpg_static_module_probe() != 0) throw new InvalidOperationException("Second owner static provider failed.");
    }
    if (Probe.dpg_getopt_index() != 17 || Probe.Libc.optind != 91)
        throw new InvalidOperationException("Nested translated owner did not restore original globals.");
    Probe.dpg_getopt_set_index(1);
    if (Probe.dpg_getopt_probe() != 73) throw new InvalidOperationException("Original getopt did not restart.");
}
Console.WriteLine("PASS original getopt/getopt_long globals, parser semantics, restart, two owners and static translated symbol transport");
