using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Op165=Managed.Emulation.Execution.FullExtended80Transcendental;
using Raw165=Managed.Emulation.Execution.FullExtended80;

unsafe partial class Program
{
    // Deliberately NOT wired to ordinary in-process tests. A general outer x87
    // save/restore shim has the same open proof as the product. Parent must run
    // this ONE family suite only in an externally established disposable host
    // control thread with known reset x87 state. Its outer FX restore preserves
    // ABI media/CW but makes NO exact outer x87/pointer/full-tag claim.
    // No hardware/control operation in this suite has been executed for R165.
    sealed record Trans165HostReceipt(string Operation,string Fixture,string Path,
        uint Admission,bool SeedEstablished,bool LegacyEqual,bool MediaEqual,bool ExactRsp,
        bool HighPointersObservable,bool HighPointersEqual,bool LaterUnmaskEqual,
        string Status,byte[] BeforeLegacy,byte[] AfterLegacy,byte[] BeforeFx,byte[] AfterFx,byte[] AfterUnmaskFx);

    static List<Trans165HostReceipt> Trans165HostStateSuite(bool disposableKnownResetHostThread,bool includeUnqualifiedHostPath)
    {
        if(!disposableKnownResetHostThread)
            throw new InvalidOperationException("R165 host control requires parent-owned disposable thread with known reset outer x87; no universal outer restore shim exists.");
        var receipts=new List<Trans165HostReceipt>();
        foreach(Op165 op in Enum.GetValues<Op165>())foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        foreach(bool legacySeed in new[]{false,true})foreach(int top in new[]{0,3,7})
        foreach(string state in new[]{"ES0-clean","ES0-masked-sticky","ES1-unmasked-IE","ES1-unmasked-PE","MMX-alias"})
        foreach(string path in includeUnqualifiedHostPath?
            new[]{"integer","version1","size2","host-candidate","host-pre-suppress","host-actual","host-completion257"}:
            new[]{"integer","version1","size2"})
        {
            // Literal tags are deliberately inconsistent with several raw80
            // classes in legacy mode. FX mode uses actual reconstructed tags;
            // this separates full-tag loss from high-pointer omission.
            byte[] legacy=new byte[108],actual=new byte[512],armed=new byte[512];
            ushort cw=state=="ES1-unmasked-IE"?(ushort)0x37e:state=="ES1-unmasked-PE"?(ushort)0x35f:(ushort)0x37f;
            ushort flags=state=="ES0-masked-sticky"?(ushort)0x3f:state=="ES1-unmasked-IE"?(ushort)1:
                state=="ES1-unmasked-PE"?(ushort)32:(ushort)0;
            ushort sw=(ushort)(top<<11|0x4500|flags|((state.StartsWith("ES1",StringComparison.Ordinal))?0x8080:0));
            ushort tw=state=="MMX-alias"?(ushort)0:(ushort)0x6c93; // 3,0,1,2,0,3,2,1 physical
            ulong fip=0x00007ffe12345678UL,fdp=0x00007ffd87654321UL;ushort fop=0x765;
            void W16(byte[] v,int p,ushort n)=>BinaryPrimitives.WriteUInt16LittleEndian(v.AsSpan(p),n);
            void W32(byte[] v,int p,uint n)=>BinaryPrimitives.WriteUInt32LittleEndian(v.AsSpan(p),n);
            void W64(byte[] v,int p,ulong n)=>BinaryPrimitives.WriteUInt64LittleEndian(v.AsSpan(p),n);
            W16(legacy,0,cw);W16(legacy,4,sw);W16(legacy,8,tw);
            W32(legacy,12,(uint)fip);W16(legacy,18,fop);W32(legacy,20,(uint)fdp);
            W16(actual,0,cw);W16(actual,2,sw);actual[4]=0xff;W16(actual,6,fop);W64(actual,8,fip);W64(actual,16,fdp);
            W32(actual,24,0x3fbf); // all flags, all masks, RC down, no DAZ/FTZ
            // Logical register slots include empty payloads, pseudo-denormal,
            // unsupported, qNaN and MMX-like exponentFFFF aliases. No arithmetic
            // consumes any caller x87 register while a fixture is installed.
            ulong[] sigs=[0x8000000000000001UL,0,1,0xfedcba9876543210UL,
                0xc000000000000123UL,0x123456789abcdef0UL,0x8000000000000000UL,0xffffffffffffffffUL];
            ushort[] exps=[0x3fff,0x8000,0,0x6000,0x7fff,0x1234,0,0xffff];
            for(int j=0;j<8;++j)
            {
                ushort se=state=="MMX-alias"?(ushort)0xffff:exps[j];
                W64(legacy,28+10*j,sigs[j]);W16(legacy,36+10*j,se);
                W64(actual,32+16*j,sigs[j]);W16(actual,40+16*j,se);
            }
            for(int j=0;j<32;++j)W64(actual,160+8*j,0x9abcdef001234500UL+(uint)j);
            actual.CopyTo(armed,0);
            if(legacySeed)
            { W16(armed,0,0x37f);W16(armed,2,0); } // clean media seed before FRSTOR
            else
            { W16(armed,0,0x37e);W16(armed,2,(ushort)(0x8081|top<<11)); } // force pointer install before ES0 actual image
            using var kernel=Trans165HostTarget(op,windows,continuation,path=="host-completion257");
            using var ret=Transform80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var adapter=Transform80Adapter(kernel.Entry,windows,continuation);
            bool later=state=="ES0-masked-sticky";
            using var envelope=Trans165IndependentEnvelope(adapter.Entry,legacySeed,later);
            byte* images=(byte*)NativeMemory.AlignedAlloc(4096,16);
            try
            {
                if(images==null)throw new OutOfMemoryException();new Span<byte>(images,4096).Fill(0xa5);
                armed.AsSpan().CopyTo(new Span<byte>(images+512,512));actual.AsSpan().CopyTo(new Span<byte>(images+1024,512));legacy.AsSpan().CopyTo(new Span<byte>(images+1536,108));
                var x=op is Op165.Fyl2x or Op165.Fpatan?new Raw165(0x8000000000000000,0x3fff):default;
                var f=Trans157Frame(x,default,0x37f,continuation?ret.Entry:0);
                if(path.StartsWith("host-",StringComparison.Ordinal))
                {f.Left=new(0xc000000000000000,0x3ffd);f.Right=new(0x8000000000000000,0x3fff);}
                if(path=="host-pre-suppress"){f.Left=new(0x8000000000000123,0x7fff);f.ControlWord=0x37e;}
                if(path is "host-actual" or "host-completion257")f.ControlWord=0x347;
                if(path=="version1")f.Version=0;if(path=="size2")f.Size=383;
                // Context: outerFX, armedFX, actualFX, seedLegacy, beforeFX,
                // beforeLegacy, afterFX, afterLegacy, beforeRSP, afterRSP,
                // admission, laterLegacy, unmaskCW. Pointers never escape frame.
                nint* c=stackalloc nint[14];
                c[0]=(nint)images;c[1]=(nint)(images+512);c[2]=(nint)(images+1024);c[3]=(nint)(images+1536);
                c[4]=(nint)(images+1664);c[5]=(nint)(images+2176);c[6]=(nint)(images+2304);c[7]=(nint)(images+2816);
                c[8]=c[9]=c[10]=0;c[11]=(nint)(images+2944);c[12]=0x340;c[13]=(nint)(images+3072);
                // Work pointer is passed separately, stored in envelope's home.
                ((delegate* unmanaged<nint*,FullExtended80WorkFrame*,void>)envelope.Entry)(c,&f);
                byte[] bf=new ReadOnlySpan<byte>(images+1664,512).ToArray(),af=new ReadOnlySpan<byte>(images+2304,512).ToArray();
                byte[] bl=new ReadOnlySpan<byte>(images+2176,108).ToArray(),al=new ReadOnlySpan<byte>(images+2816,108).ToArray();
                byte[] ul=new ReadOnlySpan<byte>(images+2944,108).ToArray();
                byte[] uf=new ReadOnlySpan<byte>(images+3072,512).ToArray();
                bool seed=Trans165LegacySeedEstablished(bl,legacySeed?legacy:null,actual,top);
                bool same=Trans165LegacyDefinedEqual(bl,al);
                bool media=bf.AsSpan(24,4).SequenceEqual(af.AsSpan(24,4))&&bf.AsSpan(160,256).SequenceEqual(af.AsSpan(160,256));
                bool beforeHigh=BinaryPrimitives.ReadUInt64LittleEndian(bf.AsSpan(8))==fip&&BinaryPrimitives.ReadUInt64LittleEndian(bf.AsSpan(16))==fdp;
                bool high=beforeHigh||!legacySeed&&later;
                bool highSame=beforeHigh?bf.AsSpan(8,16).SequenceEqual(af.AsSpan(8,16)):
                    !legacySeed&&later&&BinaryPrimitives.ReadUInt64LittleEndian(uf.AsSpan(8))==fip&&BinaryPrimitives.ReadUInt64LittleEndian(uf.AsSpan(16))==fdp;
                bool unmask=!later||Trans165AfterUnmaskEqual(al,ul);
                bool rsp=c[8]==c[9];uint admission=unchecked((uint)c[10]);
                uint expectedAdmission=path=="version1"?1u:path=="size2"?2u:path=="host-completion257"?257u:0u;
                string status=!seed?"MISSING_SEED_NOT_ESTABLISHED":!same||!media||!rsp||!unmask||admission!=expectedAdmission?
                    "FAIL_OBSERVABLE_STATE":!legacySeed&&!high?"MISSING_HIGH_POINTER_OBSERVATION":!legacySeed&&!highSame?
                    "FAIL_HIGH_POINTER_STATE":"FINITE_ONLY_NOT_UNIVERSAL_CLEARANCE";
                receipts.Add(new(op.ToString(),$"{(legacySeed?"legacy108":"FX64-armed")}/{state}/TOP{top}/{windows}/{continuation}",path,
                    admission,seed,same,media,rsp,high,highSame,unmask,status,bl,al,bf,af,uf));
            }
            finally{NativeMemory.AlignedFree(images);}
        }
        return receipts;
    }
    static bool Trans165LegacySeedEstablished(byte[] observed,byte[]? literal,byte[] fx,int top)
    {
        static ushort U16(byte[] v,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(v.AsSpan(p));
        if(literal!=null)return Trans165LegacyDefinedEqual(observed,literal);
        if(U16(observed,0)!=U16(fx,0)||U16(observed,4)!=U16(fx,2)||(U16(observed,18)&0x7ff)!=(U16(fx,6)&0x7ff)||
            !observed.AsSpan(12,4).SequenceEqual(fx.AsSpan(8,4))||!observed.AsSpan(20,4).SequenceEqual(fx.AsSpan(16,4)))return false;
        // Independently derive FX full tags from literals plus PHYSICAL TOP,
        // while checking all raw80 aliases against seed bytes, including empty.
        uint tw=0;
        for(int logical=0;logical<8;++logical)
        {
            if(!observed.AsSpan(28+10*logical,10).SequenceEqual(fx.AsSpan(32+16*logical,10)))return false;
            ulong sig=BinaryPrimitives.ReadUInt64LittleEndian(fx.AsSpan(32+16*logical));int e=U16(fx,40+16*logical)&0x7fff;
            uint tag=e==0&&sig==0?1u:e>0&&e<0x7fff&&(sig>>63)!=0?0u:2u;
            tw|=tag<<(2*((logical+top)&7));
        }
        return U16(observed,8)==tw;
    }
    static bool Trans165LegacyDefinedEqual(byte[] a,byte[] b)
    {
        // Reserved words, FCS/FDS selectors in long mode and FOP reserved high5
        // are not oracle bytes. CW/SW/fullTW/FIP32/FDP32/FOP11/raw80 ARE.
        foreach(int p in new[]{0,4,8})if(!a.AsSpan(p,2).SequenceEqual(b.AsSpan(p,2)))return false;
        if(!a.AsSpan(12,4).SequenceEqual(b.AsSpan(12,4))||!a.AsSpan(20,4).SequenceEqual(b.AsSpan(20,4)))return false;
        if((BinaryPrimitives.ReadUInt16LittleEndian(a.AsSpan(18))&0x7ff)!=(BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(18))&0x7ff))return false;
        return a.AsSpan(28,80).SequenceEqual(b.AsSpan(28,80));
    }
    static bool Trans165AfterUnmaskEqual(byte[] after,byte[] unmasked)
    {
        // Unmask six stored sticky flags AFTER return. No waiting instruction
        // follows FLDCW; immediate nonwaiting FNSAVE captures pending summary.
        // Legacy pointers/fullTW/raw80 must stay exact and flags must persist.
        byte[] expected=(byte[])after.Clone();BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(0),0x340);
        ushort sw=BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(4));
        if((sw&63)!=0)sw|=0x8080;
        // Intel093 FLDCW explicitly makes C0/C1/C2/C3 undefined. This mask
        // applies ONLY after this test's deliberate later FLDCW, never to the
        // exact caller state comparison at the original kernel return.
        sw&=unchecked((ushort)~0x4700);BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(4),sw);
        byte[] actual=(byte[])unmasked.Clone();ushort actualSw=BinaryPrimitives.ReadUInt16LittleEndian(actual.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(actual.AsSpan(4),(ushort)(actualSw&~0x4700));
        return Trans165LegacyDefinedEqual(expected,actual);
    }
    static NativeCodeAllocation Trans165HostTarget(Op165 op,bool windows,bool continuation,bool force257)
    {
        byte[] b=new byte[FullExtended80TranscendentalEmitter.CodeCapacity];int n;
        Check(continuation?FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80TranscendentalEmitter.TryEmit(op,b,out n,windows),"R165 family control target");
        if(force257)
        {
            // Test-only fault AFTER original FX/RSP restoration and private
            // scrub: change the unique captured-TOP comparator, never opcode,
            // guest arithmetic, host masks, private interval or save format.
            int found=0;byte expected=Trans157Pair(op)?(byte)6:(byte)7;
            for(int p=0;p+12<=n;++p)
                if(b.AsSpan(p,8).SequenceEqual(new byte[]{0x81,0xf9,expected,0,0,0,0x0f,0x85}))
                {b[p+2]=5;++found;}
            Check(found==1,"R165 exact one forced host-completion257 seam");
        }
        return Transform80Publish(b.AsSpan(0,n));
    }
    static NativeCodeAllocation Trans165IndependentEnvelope(nint target,bool legacy,bool laterUnmask)
    {
        bool win=OperatingSystem.IsWindows();var b=new List<byte>{0xf3,0x0f,0x1e,0xfa,0x53,0x48,0x83,0xec,0x30};
        void D(int v){for(int n=0;n<4;++n)b.Add((byte)(v>>(n*8)));}
        void Q(ulong v){for(int n=0;n<8;++n)b.Add((byte)(v>>(n*8)));}
        void Load(int reg,int slot){b.Add((byte)(0x48|(reg>=8?4:0)));b.Add(0x8b);b.Add((byte)(0x83|((reg&7)<<3)));D(slot*8);}
        void Store(int reg,int slot){b.Add((byte)(0x48|(reg>=8?4:0)));b.Add(0x89);b.Add((byte)(0x83|((reg&7)<<3)));D(slot*8);}
        void Fx(bool restore,int slot){Load(0,slot);b.AddRange([0x48,0x0f,0xae,(byte)(restore?0x08:0x00)]);}
        void Legacy(int ext,int slot){Load(0,slot);b.AddRange([0xdd,(byte)(ext<<3)]);}
        void Seed()
        {
            b.AddRange([0xdb,0xe3]);Fx(true,1);
            if(legacy)Legacy(4,3);else Fx(true,2);
        }
        b.AddRange(win?new byte[]{0x48,0x89,0xcb,0x48,0x89,0x54,0x24,0x28}:
            new byte[]{0x48,0x89,0xfb,0x48,0x89,0x74,0x24,0x28}); // RBX=context, home40=Work
        Fx(false,0);Seed();Fx(false,4);Legacy(6,5); // before FX plus independent full legacy/raw80 oracle
        Seed();b.AddRange([0x48,0x89,0xe0]);Store(0,8);
        b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x28}:new byte[]{0x48,0x8b,0x7c,0x24,0x28});
        b.AddRange([0x48,0xb8]);Q((ulong)target);b.AddRange([0xff,0xd0,0x89,0xc0]);Store(0,10);
        b.AddRange([0x48,0x89,0xe0]);Store(0,9);Fx(false,6);Legacy(6,7);
        if(laterUnmask)
        {
            // A fresh identical trial gives an independent observation channel:
            // never truncate pointers by FRSTOR before later unmask. The first
            // trial's complete AFTER legacy image remains the raw/full-tag oracle.
            Seed();
            b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x28}:new byte[]{0x48,0x8b,0x7c,0x24,0x28});
            b.AddRange([0x48,0xb8]);Q((ulong)target);b.AddRange([0xff,0xd0]);
            b.AddRange([0xd9,0xab]);D(12*8); // FLDCW: original ES0, flags masked
            Fx(false,13);Legacy(6,11); // immediate nonwaiting observation, no FWAIT
        }
        Fx(true,0);b.AddRange([0x48,0x83,0xc4,0x30,0x5b,0xc3]);return Transform80Publish(b.ToArray());
    }
}
