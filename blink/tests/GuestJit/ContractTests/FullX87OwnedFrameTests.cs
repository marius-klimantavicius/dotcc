using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal static void TestFullX87OwnedFrame(Action<bool, string> check)
        {
            using var fixture = new FullControllerFixture();
            var owner = fixture.Owner; var worker = owner.workers[0];
            var native = worker.FullNativeState!; var file = native.X87Pointer;
            var frame = new FullBlockFrame { Machine = worker.Machine, OwnerCookie = worker.X87OwnerToken };
            ValidateOwnedX87KernelFrame(worker, in frame);
            check(file->ProfileActive == 0 && frame.X87 == null,
                "inactive exact owner retains null native compatibility");
            // Test-only isolated activation; no guest instruction, transfer or
            // native entry occurs before restoring the inactive profile.
            file->ProfileActive = 1;
            try
            {
                frame.X87 = file;
                ValidateOwnedX87KernelFrame(worker, in frame);
                ValidateX87KernelReturn(worker, in frame);
                check((byte*)native.X87WorkPointer == (byte*)file + 192,
                    "admitted canonical file has its exact exclusive owned384-byte work tail");
                FullX87RegisterFile bare = *file;
                foreach (string mutation in new[] { "null", "bare192", "machine", "owner" })
                {
                    frame = new() { Machine = worker.Machine, X87 = file, OwnerCookie = worker.X87OwnerToken };
                    if (mutation == "null") frame.X87 = null;
                    if (mutation == "bare192") frame.X87 = &bare;
                    if (mutation == "machine") frame.Machine = null;
                    if (mutation == "owner") ++frame.OwnerCookie;
                    bool denied = false;
                    try { ValidateOwnedX87KernelFrame(worker, in frame); }
                    catch (InvalidOperationException) { denied = true; }
                    check(denied, "prepare hooks cannot substitute a null, bare192, foreign-machine or foreign-owner work capability");
                }
                frame = new() { Machine = worker.Machine, X87 = file, OwnerCookie = worker.X87OwnerToken };
                byte* work = (byte*)native.X87WorkPointer;
                *(uint*)(work + 280) = 0x58383746; *(uint*)(work + 284) = 3;
                bool fatal = false;
                try { ValidateX87KernelReturn(worker, in frame); }
                catch (InvalidOperationException e) { fatal = e.Message.Contains("failed internally", StringComparison.Ordinal); }
                check(fatal && !worker.X87NativeInFlight,
                    "post-RET protocol marker is internal fatal and cannot become a guest fault or retry");
            }
            finally { file->ProfileActive = 0; native.ClearX87Work(); }
        }
    }
}

unsafe partial class Program
{
    static void CheckFullX87OwnedFrame() => ThreadedGuestExecution.TestFullX87OwnedFrame(Check);
}
