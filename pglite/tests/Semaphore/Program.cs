using Core = Managed.PGlite.Probes.SemaphoreProbe;
for (int iteration = 0; iteration < 2; iteration++)
{
    using var owner = Core.__DotCcCreateContext();
    using var binding = owner.Enter();
    int result = Core.dpg_semaphore_probe();
    if (result != 0) throw new System.Exception($"Semaphore native-contract mismatch {result}");
}
System.Console.WriteLine("semaphore two-owner contracts pass");
