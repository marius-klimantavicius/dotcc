using Probe = Managed.PGlite.Probes.Sigjmp;

internal static unsafe class SigjmpProgram
{
    private static void ForeignJump(void* target)
    {
        using var foreign = new Probe.Libc.RuntimeContext();
        using var binding = foreign.Enter();
        Probe.Libc.siglongjmp((ulong*)target, 7);
    }

    public static int Main()
    {
        using var context = Probe.__DotCcCreateContext();
        using var binding = context.Enter();
        if (Probe.dpg_fixture_sigjmp_conditional(0) != 3) return 12;
        Probe.Libc.RuntimeContext.Current!.EnableGuestSignals();
        if (Probe.dpg_fixture_sigjmp_conditional(1) != 42) return 13;
        if (Probe.dpg_fixture_sigjmp_mask(0) != 1 || Probe.dpg_fixture_sigjmp_mask(1) != 0) return 1;
        if (Probe.dpg_fixture_sigjmp_once() != 0) return 2;
        if (Probe.dpg_fixture_sigjmp_nested() != 0) return 3;
        if (Probe.dpg_fixture_sigjmp_switch() != 0) return 4;
        if (Probe.dpg_fixture_sigjmp_repeated() != 0) return 5;
        if (Probe.dpg_fixture_sigjmp_handler(0) != 0 || Probe.dpg_fixture_sigjmp_handler(1) != 0) return 6;
        if (Probe.dpg_fixture_pending_consume() != 0) return 10;
        Probe.Libc.GuestIntervalTimer timer = new() { ValueMicroseconds = 2000 };
        if (Probe.Libc.GuestSetitimer(0, &timer, null) != 0 || Probe.dpg_fixture_pending_timer() != 0) return 11;
        Probe.dpg_fixture_sigjmp_expire();
        try { Probe.dpg_fixture_sigjmp_stale(); return 7; }
        catch (Probe.Libc.JumpBufferException) { }
        for (int save = 0; save <= 1; save++)
        {
            try { Probe.dpg_fixture_sigjmp_foreign(save, &ForeignJump); return 8; }
            catch (System.InvalidOperationException) { }
        }
        if (Probe.dpg_fixture_sigjmp_invalid_recovery() != 0) return 9;
        System.Console.WriteLine("sigjmp: masks, operands, nested, switch, repeated, handler passed");
        System.Console.WriteLine("signals: blocked pending selection and timer consumption passed");
        System.Console.WriteLine("sigjmp: stale and foreign recovery rejected");
        return 0;
    }
}
