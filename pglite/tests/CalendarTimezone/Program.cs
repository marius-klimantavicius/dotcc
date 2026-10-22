using System.Runtime.InteropServices;
using PG = Managed.PGlite.Probes.CalendarTimezone;

internal static unsafe class Program
{
    public static int Main()
    {
        try
        {
            nint saved;
            string expected;
            using (var owner = new PG.Libc.RuntimeContext())
            {
                using var binding = owner.Enter();
                int status = PG.dtz_run();
                if (status != 0) throw new InvalidOperationException("timezone assertion at timezone.c:" + status);
                long epoch = 1721044800;
                PG.Libc.tm local;
                if (PG.Libc.localtime_r(&epoch, &local) == null) throw new InvalidOperationException("localtime_r lifetime setup");
                saved = (nint)local.tm_zone;
                expected = Marshal.PtrToStringUTF8(saved)!;
            }
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            if (Marshal.PtrToStringUTF8(saved) != expected)
                throw new InvalidOperationException("tm_zone must survive owner disposal and GC");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
