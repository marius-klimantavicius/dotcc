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
