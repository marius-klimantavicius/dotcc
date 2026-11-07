using Managed.Emulation.Execution;
using LinkFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckFullStaticExitMetadata()
    {
        foreach (bool helpersOnly in new[] { false, true })
        {
            using var f = new LinkFixture(helpersOnly: helpersOnly);
            ulong pc = LinkFixture.Base;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, Enumerable.Repeat((byte)0x90, 512).ToArray());
            var token = WarmLastExit(f, pc);
            Check(f.Jit.Store.TryAcquireToken(token, out var lease), "FEL1 resident metadata lease");
            using (lease)
            {
                Check(f.Jit.FullTestStaticExit(in lease, out var exit) && exit.Count == 512 &&
                    exit.LastHelperPlusOne == (helpersOnly ? 512 : 0) && exit.LastLength == 1 &&
                    exit.Edge0 == !helpersOnly && !exit.Edge1 && !exit.TakenInternal &&
                    exit.Taken == 0 && exit.Fallthrough == pc + 512 && exit.SitePc == pc + 511,
                    "FEL1 retains 512-site bounds and helper suffix without a runtime plan scan");
                var h = (TranslationBlockHeader*)f.Jit.Store.GetHeader(in lease);
                var saved = h[0];
                ulong[] invalid = [0, saved.Reserved152 ^ (1UL << 63),
                    saved.Reserved152 ^ 8, saved.Reserved152 | (1UL << 26),
                    saved.Reserved152 & ~(15UL << 22),
                    (saved.Reserved152 & ~(1023UL << 12)) | (513UL << 12)];
                try
                {
                    foreach (ulong word in invalid)
                    {
                        h->Reserved152 = word;
                        Check(!f.Jit.FullTestStaticExit(in lease, out _),
                            "FEL1 unknown tags and malformed count/length/helper bounds fail closed");
                    }
                    h[0] = saved;
                    h->Reserved152 |= helpersOnly ? 1UL : 4UL;
                    Check(!f.Jit.FullTestStaticExit(in lease, out _),
                        "FEL1 helper terminal cannot claim an edge; internal needs a taken edge");
                    h[0] = saved;
                    h->Reserved144 = pc;
                    Check(!f.Jit.FullTestStaticExit(in lease, out _),
                        "FEL1 no-taken-edge cannot carry a taken PC");
                }
                finally { h[0] = saved; }
                Check(f.Jit.FullTestStaticExit(in lease, out _), "FEL1 restoration retains exact publication metadata");
            }
            Check(f.Issued == 0 && f.Completed == 0, "FEL1 compilation does not issue guest work");
            LastExitClean(f);
        }

        using (var f = new LinkFixture())
        {
            ulong pc = LinkFixture.Base, targetPc = pc + 64;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, [0x9c, 0x90, .. LastExitJump(pc + 2, targetPc)]);
            var token = WarmLastExit(f, pc);
            Check(f.Jit.Store.TryAcquireToken(token, out var lease), "FEL1 mixed helper/native lease");
            using (lease)
                Check(f.Jit.FullTestStaticExit(in lease, out var exit) && exit.Count == 3 &&
                    exit.LastHelperPlusOne == 1 && exit.LastLength == 5 && !exit.Edge0 &&
                    exit.Edge1 && !exit.TakenInternal && exit.Taken == targetPc && exit.SitePc == pc + 2,
                    "FEL1 records the final typed-helper index and native terminal separately");
            LastExitClean(f);
        }

        using (var f = new LinkFixture())
        {
            ulong pc = LinkFixture.Base, targetPc = pc + 64;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, LastExitJump(pc, targetPc));
            WriteControllerCode(f, targetPc, LastExitMarker(7));
            var token = WarmLastExit(f, pc);
            f.Counts(101); Check(f.Step(pc), "FEL1 captures real generated static exit");
            var candidate = f.Owner.FullTestLastExit;
            Check(f.Acquire(targetPc, 0, out var target, out bool multiple, out _) && multiple,
                "FEL1 resolves the actual pending target");
            using (target)
            {
                lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in target, multiple);
                Check(f.Jit.Store.TryAcquireToken(token, out var source), "FEL1 authentic source lease");
                using (source)
                {
                    var h = (TranslationBlockHeader*)f.Jit.Store.GetHeader(in source);
                    ulong word = h->Reserved152;
                    try
                    {
                        h->Reserved152 = 0;
                        Check(!CompleteLastExit(f, in candidate, in target) && h->Link1TargetHeader == 0,
                            "FEL1 unrecognized metadata cannot backpatch even an otherwise authentic token");
                    }
                    finally { h->Reserved152 = word; }
                    Check(CompleteLastExit(f, in candidate, in target) && h->Link1Pc == targetPc,
                        "FEL1 immutable publication metadata completes the authentic edge");
                }
            }
            LastExitClean(f);
        }
    }
}

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class FullTranslationJit
    {
        internal bool FullTestStaticExit(in TranslationLease lease, out (int Count, int LastHelperPlusOne,
            int LastLength, bool Edge0, bool Edge1, bool TakenInternal, ulong Taken, ulong Fallthrough, ulong SitePc) view)
        {
            var h = Store.GetHeaderCopy(in lease);
            view = default;
            if (!TryReadStaticExit(in h, out var exit)) return false;
            view = (exit.Count, exit.LastHelperPlusOne, exit.LastLength, exit.Edge0, exit.Edge1,
                exit.TakenInternal, exit.Taken, exit.Fallthrough, exit.SitePc);
            return true;
        }
    }
}
