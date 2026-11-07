using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class FullTranslationJit
    {
        internal void TestX87KernelOwner(Action<bool, string> check)
        {
            var frame = new FullBlockFrame();
            lock (Authority.Gate) RequireX87KernelAdmission(in frame);
            frame.X87 = (FullX87RegisterFile*)1; // admission never dereferences data
            bool unavailable = false;
            try { lock (Authority.Gate) RequireX87KernelAdmission(in frame); }
            catch (InvalidOperationException) { unavailable = true; }
            check(unavailable, "canonical entry cannot run without a completely published catalog");
            long before = Store.GetStatistics().ChargedBytes;
            InitializeX87Kernels(true);
            check(Store.Publications == 0 && x87Kernels!.Count == FullX87KernelCatalog.MaximumKernels &&
                Store.GetStatistics().ChargedBytes > before,
                "catalog is eagerly published and charged before the first retained TB");
            lock (Authority.Gate)
            {
                RequireX87KernelAdmission(in frame);
                var binding = GetX87Kernel(FullX87KernelKey.Divide);
                check(binding.Entry != 0 && binding.CodeBytes > 0,
                    "owner authenticates the immutable native binding under admission gate");
                x87Kernels!.CloseAdmission();
                bool closed = false;
                try { RequireX87KernelAdmission(in frame); }
                catch (InvalidOperationException) { closed = true; }
                check(closed, "closed catalog rejects cached canonical entries before enrollment");
            }
        }
    }
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal static void TestFullX87KernelOwner(Action<bool, string> check)
        {
            using var fixture = new FullControllerFixture();
            var jit = fixture.Owner.fullJit!;
            jit.TestX87KernelOwner(check);
            fixture.Dispose();
            check(jit.IsDisposed && jit.Store.NativeLifetimeClosed,
                "owner closes TB lifetime before unmapping independent x87 code");
            fixture.Dispose();
        }
    }
}

unsafe partial class Program
{
    static void CheckFullX87KernelOwner() => ThreadedGuestExecution.TestFullX87KernelOwner(Check);
}
