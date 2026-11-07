using RepSuiteFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Parent adds this ONE group to the real runner
    // after ABI6/core/controller/kernel-pool integration. No alternate executor.
    static void CheckFullRepContracts(byte* machine)
    {
        using(var fixture=new RepSuiteFixture(interpreterOracle:true))CheckFullRepLowering(fixture.Program);
        CheckFullRepKernels(machine);
        CheckFullRepController();
        CheckFullRepManagedPauseChanges();
        CheckFullRepAttemptIdentities();
        CheckFullRepSelectedOracles();
    }
}
