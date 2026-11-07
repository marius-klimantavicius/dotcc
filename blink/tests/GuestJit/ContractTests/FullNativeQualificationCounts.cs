using Managed.Emulation.Execution;

unsafe partial class Program
{
    // Test-run observations at existing native return boundaries. These are
    // guest completions, not assertion totals, wrappers, or standalone numeric
    // kernel invocations. Manually invoked reference GetOp calls are excluded.
    static long qualificationLeafCompletions, qualificationLeafFaultReturns,
        qualificationMappingReturns, qualificationHelperBoundaries,
        qualificationControllerNative, qualificationControllerHelpers,
        qualificationControllerInterpreterAttempts;

    internal static void RecordQualificationNative(FullBlockFrame* frame, ulong before)
    {
        if (frame->Executed >= before && frame->Executed - before <= long.MaxValue)
            Interlocked.Add(ref qualificationLeafCompletions, (long)(frame->Executed - before));
        if (frame->Exit == FullBlockExit.GuestFault) Interlocked.Increment(ref qualificationLeafFaultReturns);
        if (frame->Exit == FullBlockExit.NeedMemory) Interlocked.Increment(ref qualificationMappingReturns);
        if (frame->Exit == FullBlockExit.NeedHelper) Interlocked.Increment(ref qualificationHelperBoundaries);
    }
    internal static void RecordQualificationController(GuestJitStatistics statistics)
    {
        Interlocked.Add(ref qualificationControllerNative, statistics.InlineNativeInstructions);
        Interlocked.Add(ref qualificationControllerHelpers, statistics.SemanticHelperInstructions);
        Interlocked.Add(ref qualificationControllerInterpreterAttempts, statistics.InterpreterAttempts);
    }
    static void PrintQualificationCounts(string scope) => Console.WriteLine(
        $"OBSERVED {scope}: synthetic_leaf_native_guest_completions={qualificationLeafCompletions} " +
        $"synthetic_guest_fault_returns={qualificationLeafFaultReturns} mapping_assist_returns={qualificationMappingReturns} " +
        $"helper_boundary_returns={qualificationHelperBoundaries} controller_native_guest_instructions={qualificationControllerNative} " +
        $"controller_semantic_helpers={qualificationControllerHelpers} controller_interpreter_attempts={qualificationControllerInterpreterAttempts}; " +
        "reference GetOp and standalone numeric kernels excluded; these counts do not prove static completeness");
}
