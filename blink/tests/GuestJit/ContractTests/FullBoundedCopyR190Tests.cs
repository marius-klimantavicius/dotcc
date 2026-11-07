using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Copy190Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckFullBoundedCopiesR190()
    {
        foreach (bool hostBuffer in new[] { false, true })
        {
            using var f = new Copy190Fixture(); ulong pc=Copy190Fixture.Base, data=pc+8192;
            f.Map(pc); f.Fill(pc); f.Map(data,8192);
            var token=RetainedWarm(f,pc);
            byte* host=(byte*)NativeMemory.AlignedAlloc(8192,4096);
            try
            {
                nint address=hostBuffer ? (nint)(host+4095) : f.Physical(data)+64;
                ulong code=f.Jit.Authority.CodeEpoch, runtime=f.Jit.Authority.Epoch;
                f.At(pc); var copy=(IHostGuestMemoryCopies)f.Owner;
                copy.BeginGuestWrite(address,2);
                try
                {
                    Check(f.Jit.Authority.CopyWriters>0 && f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)),
                        "R190 exact cross-page copy retains actual write exclusion");
                    lock(f.Jit.Authority.Gate)
                    {
                        bool entered=f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch);
                        if(entered) f.Jit.Authority.ExitNative(); // fixture always balances an unexpected success
                        Check(!entered,"R190 live bounded copy excludes foreign native execution");
                    }
                    ((byte*)address)[0]=0x5a;
                    // The host allocation covers both pages; the guest case
                    // stays within one independently mapped host page.
                    ((byte*)address)[1]=0xa5;
                }
                finally { f.At(pc); copy.EndGuestWrite(); }
                Check(f.Jit.Authority.CodeEpoch==code && f.Jit.Authority.Epoch>runtime,
                    "R190 unregistered finite data/host buffers do not pulse unrelated code freshness");
                var before=f.Statistics;
                var hit=f.R185Acquire(pc,token,0);
                Check(hit.Acquired && hit.Retained && hit.Token==token && !hit.FetchBorrow &&
                    AuthorityCount(f.Statistics,"ByteValidations")==AuthorityCount(before,"ByteValidations"),
                    "R190 unrelated trusted body reuses authority without fetch/byte revalidation");
                RetainedClean(f);
            }
            finally { NativeMemory.AlignedFree(host); }
        }
        foreach (string kind in new[] { "same", "changed", "alias", "during-copy" })
        {
            using var f=new Copy190Fixture(); ulong pc=Copy190Fixture.Base, alias=pc+0x10000;
            if (kind=="alias")
            {
                var file=f.Io.CreateMemoryFile("r190-shared-code",0);
                Check(file.Succeeded && f.Io.WriteAt(file.Value,RetainedPage(),0).Value==4096,"R190 owned shared alias bytes");
                f.MapShared(file.Value,pc);f.MapShared(file.Value,alias);
            }
            else { f.Map(pc); f.Fill(pc); }
            var token=RetainedWarm(f,pc); nint physical=f.Physical(kind=="alias" ? alias : pc);
            ulong code=f.Jit.Authority.CodeEpoch;
            Check(f.Jit.Store.TryAcquireToken(token,out var held),"R190 actual source retained lease");
            using(held)
            {
                var old=f.Jit.Store.GetHeaderCopy(in held);
                f.At(pc);var copy=(IHostGuestMemoryCopies)f.Owner;
                copy.BeginGuestWrite(physical+1,1);
                try
                {
                    var revoked=f.Jit.Store.GetHeaderCopy(in held);
                    Check(revoked.Live==0 && revoked.CodeEpoch!=old.CodeEpoch && f.Jit.Authority.CodeEpoch==code,
                        "R190 actual physical aliases revoke body/version before first write without global code pulse");
                    if(kind=="during-copy")
                    {
                        Check(f.Acquire(pc,1,out var transient,out _,out _),"R190 checked validation during actual bounded writer");
                        using(transient)
                            Check(f.Jit.Store.GetHeaderCopy(in transient).ValidatedGlobalEpoch==0,
                                "R190 an image sampled during copy cannot gain a current executable stamp");
                    }
                    if(kind is "changed" or "during-copy") ((byte*)physical)[1]=7;
                }
                finally { f.At(pc); copy.EndGuestWrite(); }
            }
            var refreshed=RetainedWarm(f,pc);
            Check((refreshed==token)==(kind is "same" or "alias"),
                "R190 identical bytes retain emission; changed bytes retire and compile a new generation");
            f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
            f.Counts(8);
            Check(f.Step(pc) && f.Completed==1 && FullRax(f)==(kind is "changed" or "during-copy" ? 7UL : 1UL),
                "R190 real controller executes exact final bytes after bounded physical copy");
            RetainedClean(f);
        }
        using(var f=new Copy190Fixture())
        {
            ulong pc=Copy190Fixture.Base;f.Map(pc);f.Fill(pc);RetainedWarm(f,pc);
            bool denied=false;
            try { f.Jit.RevokeBoundedCopy(f.WorkerId(0),f.Physical(pc),0); }
            catch(InvalidOperationException) { denied=true; }
            Check(denied,"R190 exact range cannot fabricate actual copy writer admission");
            RetainedClean(f);
        }
    }
}
