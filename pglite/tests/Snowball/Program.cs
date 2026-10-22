using Core = Managed.PGlite.Probes.SnowballProbe;

for (int iteration = 0; iteration < 2; iteration++)
{
    using var owner = Core.__DotCcCreateContext();
    using var binding = owner.Enter();
    if (Core.dsb_owner_sequence() != 1)
        throw new InvalidOperationException("Snowball generated globals leaked from the previous owner");
    int result = Core.dsb_run();
    if (result != 0)
        throw new InvalidOperationException("Original Snowball contract failed at probe.c:" + result);
    if (Core.dsb_owner_sequence() != 2)
        throw new InvalidOperationException("Snowball owner did not retain its own initialized state");
}
