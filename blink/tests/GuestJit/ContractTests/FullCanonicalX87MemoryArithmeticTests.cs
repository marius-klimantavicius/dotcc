using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using E80 = Managed.Emulation.Execution.FullExtended80;
using E80Op = Managed.Emulation.Execution.FullExtended80Operation;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R155 ALL_AUTHORED_UNRUN. Parent's single coherent x87 batch owns all
    // build/emission/native/runtime checks. Production Active0 stays unchanged.
    static void CheckFullCanonicalX87MemoryArithmetic(byte* machine)
    {
        _ = machine;
        Check(FullBlockFrame.AbiVersion == 0x46420007 && sizeof(FullBlockFrame) == 416 &&
            sizeof(FullX87NativeStorage) == 576 && sizeof(FullExtended80WorkFrame) == 384 &&
            sizeof(FullX87RegisterFile) == 192 && FullX87KernelCatalog.MaximumKernels >= 66 &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<FullX87KernelCall>(), "R155 coherent66 catalog, unchanged ABI7/576 raw owner");
        MemoryCXDecode(); MemoryCXLiterals(); MemoryCXNumeric();
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        var calls = new FullX87KernelCall[FullX87KernelCatalog.MaximumKernels];
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _) &&
                catalog.Count == FullX87KernelCatalog.MaximumKernels, "R155 whole coherent catalog initialized before TB publication");
            foreach (var form in MemoryCXForms())
            {
                var i = MemoryCXClassify([form.Opcode, (byte)(form.Group << 3 | 3)]);
                var key = FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(i); var binding = catalog.Find(key);
                Check(catalog.IsValid(in binding), "R155 same-Gate exact typed RX binding authentication");
                calls[(int)key] = new((byte)key, binding.Entry, 73, binding.CodeBytes);
            }
        }
        var token = RetainedPublish(store, 0x500000);
        Check(store.TryAcquireToken(token, out var root), "R155 original TB root spans kernel and continuation");
        Check(store.TryBeginNativeEpoch(out var epochLease), "R155 original TB epoch spans kernel and continuation");
        bool enrolled; lock (authority.Gate) enrolled = catalog.AdmissionOpen && authority.TryEnterNative(authority.Epoch);
        Check(enrolled, "R155 original authority enrollment spans kernel/continuation/RET");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine) + 15) & ~15), 16);
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(576, 16);
        byte* pages = (byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            Check(fake != null && state != null && pages != null && !fullOwnerScaffold.IsValid, "R155 isolated synthetic storage");
            MemoryCXSuite(fake, state, pages, calls);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try { MemoryCXSuite(fake, state, pages, calls); } finally { fullOwnerScaffold = default; }
            }
            MemoryCXEncodings(calls);
        }
        finally
        {
            NativeMemory.Free(pages); NativeMemory.AlignedFree(state); NativeMemory.AlignedFree(fake);
            if (enrolled) authority.ExitNative(); epochLease.Dispose(); root.Dispose();
        }
        lock (authority.Gate) catalog.CloseAdmission();
        store.Dispose(); catalog.Dispose();
        Check(catalog.ChargedBytes == 0, "R155 catalog cleanup after original TB/native barriers");
    }

    readonly record struct MemoryCXForm(byte Opcode, int Group, int Width)
    {
        internal bool Bcd => Opcode == 0xdf;
        internal bool Store => Bcd && Group == 6;
        internal bool Load => !Store; // read memory, not necessarily a stack push
        internal bool Compare => !Bcd && Group is 2 or 3;
        internal bool Integer => Opcode is 0xda or 0xde;
        internal bool Pop => Store || Compare && Group == 3;
    }
    static IEnumerable<MemoryCXForm> MemoryCXForms()
    {
        foreach (byte op in new byte[] {0xd8,0xdc,0xda,0xde})
        foreach (int group in new[] {0,1,2,3,4,5,6,7})
            yield return new(op,group,op==0xdc?64:op==0xde?16:32);
        yield return new(0xdf,4,80); yield return new(0xdf,6,80);
    }
    static JitInstruction MemoryCXClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullCanonicalX87MemoryArithmeticLowering.TryClassify(Ip, bytes, out var i, out string? why), why ?? "R155 exact authoritative span");
        Check(FullBlockEmitter.ValidateCanonicalX87MemoryArithmetic(i), "R155 typed34-row family IR"); return i;
    }
    static FullX87KernelCall MemoryCXCall(JitInstruction i,FullX87KernelCall[] calls) =>
        calls[(int)FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(i)];

    static void MemoryCXDecode()
    {
        int rows = 0; var keys = new HashSet<byte>();
        foreach (var form in MemoryCXForms())
        {
            ++rows;
            foreach (byte[] prefix in new byte[][] {[],[0x66],[0x67],[0x65,0x4b],[0x4b,0x66],[0x64,0x67,0x4b]})
            {
                byte[] b = [..prefix,form.Opcode,(byte)(form.Group<<3|4),0x24];
                var i = MemoryCXClassify(b); keys.Add((byte)FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(i));
                Check(i.Width==80&&i.SourceWidth==form.Width&&i.Length==b.Length&&
                    i.Extra.Immediate==(ulong)((form.Opcode&7)<<8|form.Group<<3|4)&&
                    FullBlockEmitter.CanonicalX87MemoryArithmeticRequiresPrecision(i)==(!form.Bcd&&!form.Compare),
                    "R155 independent34 opcode/FOP/width/PC domains");
                foreach (var bad in new[] {i with {Width=64},i with {Options=i.Options|0x10000},i with {SourceWidth=8},
                    i with {Length=1},i with {Length=16},i with {Extra=new(0,0x800,IsImmediate:true)},
                    i with {Source=new(7,0)},i with {Target=1},i with {Condition=1},i with {Address=i.Address with {Scale=4}}})
                {
                    bool reject=false; try { FullBlockEmitter.ValidateCanonicalX87MemoryArithmetic(bad); } catch(ArgumentException){reject=true;}
                    Check(reject,"R155 malformed recognized memory-computation IR denied");
                }
                var good = new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(i),(nint)0x1000,73,32);
                foreach(var bad in new[]{default(FullX87KernelCall),good with{Entry=0},good with{OwnerCookie=0},good with{CodeBytes=FullX87KernelCatalog.StagingBytes+1},good with{Key=(byte)(good.Key^1)}})
                {
                    bool reject=false;try{FullBlockEmitter.ValidateCanonicalX87MemoryArithmetic(i,in bad);}catch(ArgumentException){reject=true;}
                    Check(reject,"R155 typed key/raw call shape validation before parent same-Gate binding authentication");
                }
            }
        }
        Check(rows==34&&keys.Count==34&&keys.SetEquals(System.Linq.Enumerable.Select(System.Linq.Enumerable.Range(32,34),x=>(byte)x)),
            "R155 exact independent34 documented rows and complete proposed keys32..65");
        foreach(byte[] b in new byte[][] {[],[0xd8],[0xd8,0x00,0],[0xd8,0xc0],[0xde,0xd9],[0xdf,0xc0],
            [0xf0,0xd8,0x00],[0xf2,0xd8,0x00],[0xf3,0xdf,0x20],[0x9b,0xd8,0x00],[0xd9,0x20],[0xdd,0x30]})
            Check(!FullCanonicalX87MemoryArithmeticLowering.TryClassify(Ip,b,out _,out _),"R155 other families/aliases/REP/LOCK/WAIT stay excluded");
    }

    // Independent exact IEEE/integer rational lift; no product decoder or FLD
    // oracle. Original memory denormal classification is carried separately.
    static (E80 Value,bool Denormal) MemoryCXLift(MemoryCXForm form,ulong raw)
    {
        if(form.Integer)
        {
            BigInteger n=form.Width==16?(short)raw:(int)raw;
            return (E80PackExact(BigInteger.Abs(n),0,n.Sign<0),false);
        }
        int f=form.Width==32?23:52,bias=form.Width==32?127:1023;
        ulong fraction=raw&((1UL<<f)-1),exp=(raw>>f)&(form.Width==32?255UL:2047UL);
        bool negative=(raw>>(form.Width-1)&1)!=0;
        if(exp==(ulong)(bias*2+1)) return(new(0x8000000000000000UL|(fraction<<(63-f)),(ushort)(0x7fff|(negative?0x8000:0))),false);
        BigInteger sig=exp==0?fraction:fraction|(1UL<<f);
        return(E80PackExact(sig,(exp==0?1:(int)exp)-bias-f,negative),exp==0&&fraction!=0);
    }
    static E80Expected MemoryCXArithmeticReference(MemoryCXForm form,E80 a,ulong raw,ushort cw)
    {
        var b=MemoryCXLift(form,raw);
        E80Op op=form.Group switch{0=>E80Op.Add,1=>E80Op.Multiply,4 or 5=>E80Op.Subtract,_=>E80Op.Divide};
        bool reverse=form.Group is 5 or 7;
        var x=ReferenceE80(op,reverse?b.Value:a,reverse?a:b.Value,cw);
        // Lifting does NOT erase original IEEE denormal class. The rational
        // numeric oracle sees exact value; add DE only after higher classes.
        if(b.Denormal&&x.Admission==0&&(x.NormalStatus&5)==0&&(x.Classification&64)==0)
            x=x with{NormalStatus=(ushort)(x.NormalStatus|2),WrappedStatus=(ushort)(x.WrappedStatus|((x.Classification&2)!=0?2:0)),Classification=x.Classification|32};
        return x;
    }
    static Control80Expected MemoryCXCompareReference(MemoryCXForm form,E80 a,ulong raw,ushort cw)
    {
        var b=MemoryCXLift(form,raw);var op=form.Pop?FullExtended80Control.Fcomp:FullExtended80Control.Fcom;
        var x=Control80Reference(op,a,b.Value,cw);
        if(b.Denormal&&(x.Metadata&8)==0)
            x=x with{Masked=2,Events=2,Metadata=x.Metadata|16|1024|(form.Pop&&(cw&2)==0?2048u:0u)};
        return x;
    }

    static (uint Delta,bool Write) MemoryCXExpected(MemoryCXForm form,JitInstruction i,byte[] input,byte[] m,byte[] raw,
        byte[] pages,int offset,ulong address)
    {
        uint sw=CanonicalX87U32(m,648),tw=CanonicalX87U32(m,652);ushort cw=(ushort)CanonicalX87U32(m,672);
        int top=(int)(sw>>11)&7,d=form.Bcd&&!form.Store?(top+7)&7:top;
        bool stack=((tw>>(d*2))&3)==(form.Bcd&&!form.Store?0u:3u);
        if(form.Bcd&&!form.Store)stack=((tw>>(d*2))&3)!=3;
        uint delta=0;bool write=true,pop=false;E80 result=default;
        if(stack)
        {
            delta=form.Bcd&&!form.Store?0x241u:0x41u;write=(cw&1)!=0;
            result=new(0xc000000000000000,0xffff);pop=write&&form.Pop;
            if(write&&form.Compare)sw=(sw&~0x4700u)|0x4500;
        }
        else if(form.Bcd)
        {
            var value=new E80(BinaryPrimitives.ReadUInt64LittleEndian(input),BinaryPrimitives.ReadUInt16LittleEndian(input.AsSpan(8)));
            var b=Bcd80Reference(form.Store?FullExtended80BcdOperation.StorePackedBcd:FullExtended80BcdOperation.LoadPackedBcd,value,cw);
            if(b.Admission==3){result=new(0xc000000000000000,0xffff);delta=0;}
            else{result=b.Value;delta=b.Events;write=(b.Metadata&512)==0;}
            pop=write&&form.Store;
        }
        else
        {
            var a=ArithmeticRead(raw,top);ulong bits=form.Width==16?BinaryPrimitives.ReadUInt16LittleEndian(input):
                form.Width==32?BinaryPrimitives.ReadUInt32LittleEndian(input):BinaryPrimitives.ReadUInt64LittleEndian(input);
            if(form.Compare)
            {
                var b=MemoryCXCompareReference(form,a,bits,cw);delta=b.Events;
                var relation=b.Image;var view=(FullExtended80CompareResult*)&relation;
                sw=(sw&~(uint)view->StatusWriteMask)|((uint)view->StatusBits&view->StatusWriteMask);
                write=false;pop=form.Pop&&(b.Metadata&2048)==0;
            }
            else
            {
                var b=MemoryCXArithmeticReference(form,a,bits,cw);
                uint flags=(uint)b.NormalStatus&7,stopping=(flags&~cw&1)!=0?1u:(flags&~cw&4)!=0?4u:(flags&~cw&2)!=0?2u:0u;
                write=stopping==0;
                if(!write)delta=stopping;
                else if((b.Classification&8)!=0&&(cw&8)==0||(b.Classification&4)!=0&&(cw&16)==0)
                {result=b.Wrapped;delta=b.WrappedStatus;}
                else{result=b.Normal;delta=b.NormalStatus;}
            }
        }
        CanonicalX87W64(m,664,i.Ip);CanonicalX87W32(m,656,(uint)i.Extra.Immediate);CanonicalX87W64(m,680,address);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        int segment=i.Address.Segment>=0?i.Address.Segment:FullScalarMiscLowering.UsesStackSegment(i.Address)?2:3;
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(42),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(456+segment*16)));
        sw=(sw&~0x200u)|delta;if((delta&~cw&63)!=0)sw|=0x8080;
        if(write)
        {
            if(form.Store){BinaryPrimitives.WriteUInt64LittleEndian(pages.AsSpan(offset),result.Significand);BinaryPrimitives.WriteUInt16LittleEndian(pages.AsSpan(offset+8),result.SignExponent);}
            else if(!form.Compare)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(raw.AsSpan(64+d*16),result.Significand);
                BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(72+d*16),result.SignExponent);
                tw=(tw&~(3u<<(d*2)))|(uint)ArithmeticResultTag(result)<<(d*2);
                if(form.Bcd)sw=(sw&~0x3800u)|(uint)d<<11;
            }
        }
        if(pop){tw|=3u<<(top*2);sw=(sw&~0x3800u)|(uint)((top+1)&7)<<11;}
        CanonicalX87W32(m,648,sw);CanonicalX87W32(m,652,tw);CanonicalX87W64(m,0,i.Ip+(uint)i.Length);m[8]=0;
        return(delta,write);
    }

    static IEnumerable<ulong> MemoryCXBits(MemoryCXForm form)
    {
        if(form.Integer)
        {
            foreach(ulong x in new ulong[]{0,1,2,3,7,0x7fff,0x8000,0xffff,0x7fffffff,0x80000000,0xffffffff})
                yield return form.Width==16?x&65535:x;
        }
        else if(form.Width==32)
        {
            foreach(uint x in new uint[]{0,0x80000000,1,0x80000001,0x007fffff,0x00800000,0x3f800000,0x3f800001,
                0xbf800000,0x40400000,0x7f7fffff,0x7f800000,0xff800000,0x7f800001,0xff800001,0x7fc00001,0xffc12345})yield return x;
        }
        else
        {
            foreach(ulong x in new ulong[]{0,0x8000000000000000,1,0x8000000000000001,0xfffffffffffff,0x10000000000000,
                0x3ff0000000000000,0x3ff0000000000001,0xbff0000000000000,0x4008000000000000,0x7fefffffffffffff,
                0x7ff0000000000000,0xfff0000000000000,0x7ff0000000000001,0xfff0000000000001,0x7ff8000000000001,0xfff8123456789abc})yield return x;
        }
    }
    static IEnumerable<E80> MemoryCXLeft()
    {
        yield return default;yield return new(0,0x8000);yield return new(0x8000000000000000,0x3fff);
        yield return new(0x8000000000000001,0x3fff);yield return new(0x8000000000000000,0xbfff);
        yield return new(0x8000000000000000,0x4000);yield return new(0xc000000000000000,0x4000);
        yield return new(1,0);yield return new(0x8000000000000000,0);yield return new(ulong.MaxValue,0x7ffe);
        yield return new(0x8000000000000000,0x0001);yield return new(0x8000000000000000,0x7fff);
        yield return new(0x8000000000000000,0xffff);yield return new(0xc123456789abcdef,0x7fff);
        yield return new(0x8123456789abcdef,0xffff);yield return new(0x7fffffffffffffff,0x4000);
    }
    static bool MemoryCXEmit(MemoryCXForm form,Span<byte> bytes,out int n,bool windows,bool continuation)
    {
        var format=form.Opcode switch{0xd8=>FullExtended80MemoryArithmeticFormat.Binary32,0xdc=>FullExtended80MemoryArithmeticFormat.Binary64,
            0xda=>FullExtended80MemoryArithmeticFormat.Int32,_=>FullExtended80MemoryArithmeticFormat.Int16};
        if(form.Compare)return continuation?FullExtended80MemoryCompareEmitter.TryEmitContinuation(format,form.Pop,bytes,out n):
            FullExtended80MemoryCompareEmitter.TryEmit(format,form.Pop,bytes,out n,windows);
        var op=form.Group switch{0=>FullExtended80MemoryArithmeticOperation.Add,1=>FullExtended80MemoryArithmeticOperation.Multiply,
            4=>FullExtended80MemoryArithmeticOperation.Subtract,5=>FullExtended80MemoryArithmeticOperation.SubtractReverse,
            6=>FullExtended80MemoryArithmeticOperation.Divide,_=>FullExtended80MemoryArithmeticOperation.DivideReverse};
        return continuation?FullExtended80MemoryArithmeticEmitter.TryEmitContinuation(op,format,bytes,out n):
            FullExtended80MemoryArithmeticEmitter.TryEmit(op,format,bytes,out n,windows);
    }
    static void MemoryCXNumeric()
    {
        byte[] staging=new byte[32768];byte* guard=stackalloc byte[416];
        foreach(var form in MemoryCXForms())
        {
            if(form.Bcd)continue; // qualified R152 foundation, wrapper tested below
            foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
            {
                Check(MemoryCXEmit(form,staging,out int n,windows,continuation)&&n>0,"R155 all32 new specialized native lifts both RET ABIs/continuation");
                using var code=Bcd80Publish(staging.AsSpan(0,n));using var adapter=Bcd80Adapter(code.Entry,windows,continuation);
                using var tail=Bcd80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
                using var preservation=Bcd80PreservationProbe(adapter.Entry);
                var warmFrame=E80Frame(new(0x8000000000000000,0x3fff),new(form.Integer?1UL:form.Width==32?0x3f800000UL:0x3ff0000000000000UL,0),0x37f);
                warmFrame.Continuation=continuation?(ulong)tail.Entry:0;
                Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)preservation.Entry)(&warmFrame)==0,
                    "R155 SysV/Win64 union nonvolatile preservation through numeric kernel and native continuation");
                for(int warm=0;warm<16;++warm)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)adapter.Entry)(&warmFrame);
                long nativeAllocated=GC.GetAllocatedBytesForCurrentThread();uint admissionBits=0;
                for(int warm=0;warm<256;++warm)admissionBits|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)adapter.Entry)(&warmFrame);
                Check(admissionBits==0&&nativeAllocated==GC.GetAllocatedBytesForCurrentThread(),"R155 native lift/compute invocation allocates zero");
                foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{63,0,62,61,59,55,47,31})
                foreach(E80 a in MemoryCXLeft())foreach(ulong raw in MemoryCXBits(form))
                {
                    ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                    new Span<byte>(guard,416).Fill(0xa5);
                    var f=(FullExtended80WorkFrame*)(guard+16);*f=E80Frame(a,new(raw,0x1234),cw);
                    new Span<byte>((byte*)f+96,288).Fill(0xa5);f->Continuation=continuation?(ulong)tail.Entry:0;
                    f->Left.Reserved10=0x1234;f->Right.Reserved12=0x98765432;
                    byte[] inputs=CanonicalX87Bytes((byte*)f+16,32);
                    uint admission=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)adapter.Entry)(f);
                    if(form.Compare)
                    {
                        var e=MemoryCXCompareReference(form,a,raw,cw);
                        Check(admission==0&&f->Admission==0&&E80Equal(f->Normal,e.Image)&&f->NormalStatus==e.Masked&&
                            f->WrappedStatus==e.Events&&(uint)f->Classification==e.Metadata&&E80Equal(f->Wrapped,default),
                            "R155 compare direct raw IEEE/unquieted NaN/original denormal independent rational relation/status/completion");
                    }
                    else
                    {
                        var e=MemoryCXArithmeticReference(form,a,raw,cw);
                        Check(admission==(uint)e.Admission&&f->Admission==e.Admission,"R155 arithmetic all-PC software admission is not a guest opcode branch");
                        if(admission==0)Check(E80Equal(f->Normal,e.Normal)&&E80Equal(f->Wrapped,e.Wrapped)&&f->NormalStatus==e.NormalStatus&&
                            f->WrappedStatus==e.WrappedStatus&&(uint)f->Classification==e.Classification,
                            "R155 independent exact rational arithmetic, PC/RC/masks/reverse/class priority/wrapped completion");
                    }
                    Check(new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(inputs)&&
                        new ReadOnlySpan<byte>((byte*)f+256,128).IndexOfAnyExcept((byte)0xa5)<0&&
                        new ReadOnlySpan<byte>(guard,16).IndexOfAnyExcept((byte)0xa5)<0&&
                        new ReadOnlySpan<byte>(guard+400,16).IndexOfAnyExcept((byte)0xa5)<0,
                        "R155 inputs/caller384 protocol bounds immutable under all numeric classes");
                }
                foreach(int cut in new[]{0,1,17,n-1})Check(!MemoryCXEmit(form,staging.AsSpan(0,cut),out int shortN,windows,continuation)&&shortN==0,
                    "R155 bounded numeric emission refuses partial publication");
                for(int warm=0;warm<8;++warm)MemoryCXEmit(form,staging,out _,windows,continuation);
                long before=GC.GetAllocatedBytesForCurrentThread();for(int warm=0;warm<16;++warm)MemoryCXEmit(form,staging,out _,windows,continuation);
                Check(before==GC.GetAllocatedBytesForCurrentThread(),"R155 numeric direct lift zero-allocation emission");
            }
        }
    }

    static void MemoryCXLiterals()
    {
        var add32=new MemoryCXForm(0xd8,0,32);var divr32=new MemoryCXForm(0xd8,7,32);var div32=new MemoryCXForm(0xd8,6,32);
        E80 one=new(0x8000000000000000,0x3fff);E80 qnan=new(0xc123456789abcdef,0x7fff);
        var x=MemoryCXArithmeticReference(add32,one,0x7f800001,0x37f);
        Check(E80Equal(x.Normal,new(0xc000010000000000,0x7fff))&&x.NormalStatus==1&&x.Classification==97,
            "R155 literal IEEE32 sNaN retains original signaling class and all payload until arithmetic IE/quieting");
        x=MemoryCXArithmeticReference(add32,qnan,1,0x37d);
        Check(E80Equal(x.Normal,qnan)&&x.NormalStatus==0&&x.Classification==65,"R155 literal QNaN wins memory denormal even DMclear");
        x=MemoryCXArithmeticReference(divr32,default,1,0x37f);
        Check(E80Equal(x.Normal,new(0x8000000000000000,0x7fff))&&x.NormalStatus==4&&x.Classification==161,
            "R155 literal memory denormal divided by ST0zero reports ZE alone before DE");
        x=MemoryCXArithmeticReference(div32,one,1,0x37f);
        Check(E80Equal(x.Normal,new(0x8000000000000000,0x4094))&&x.NormalStatus==2&&x.Classification==49,
            "R155 literal exact IEEE32 least-denormal divide retains DE despite normalized internal value");
        x=MemoryCXArithmeticReference(new(0xde,0,16),default,0x8000,0x37f);
        Check(E80Equal(x.Normal,new(0x8000000000000000,0xc00e))&&x.NormalStatus==0,"R155 literal signed integer minimum lift exact");
        var c=MemoryCXCompareReference(new(0xd8,3,32),one,1,0x37d);var image=c.Image;
        Check(c.Events==2&&(c.Metadata&2048)!=0&&((FullExtended80CompareResult*)&image)->StatusBits==0,
            "R155 compare DE updates greater relation while suppressing unmasked pop");
    }

    static void MemoryCXSuite(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        MemoryCXNumbers(fake,state,pages,calls);MemoryCXAddresses(fake,state,pages,calls);
        MemoryCXMisses(fake,state,pages,calls);MemoryCXCanonical(fake,state,calls);
        MemoryCXBoundaries(fake,state,pages,calls);MemoryCXDeferred(fake,state,pages,calls);
        MemoryCXBadProtocol(fake,state,pages);MemoryCXProofFailures(fake,state,calls);MemoryCXUndefinedBcd(fake,state,pages,calls);
    }
    static void MemoryCXNumbers(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach(var form in MemoryCXForms())
        {
            var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryCXCall(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{63,0,62,61,59,55,47,31})
            {
                ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                foreach(E80 left in MemoryCXLeft())
                {
                    if(form.Bcd)
                    {
                        foreach(E80 input in Bcd80Cases(form.Store?FullExtended80BcdOperation.StorePackedBcd:FullExtended80BcdOperation.LoadPackedBcd))
                        {
                            MemoryCXSetup(form,fake,state,pages,4095,cw,form.Store?input:left);
                            if(!form.Store)MemoryCXWriteInput(pages,4095,input);
                            MemoryCXOne(code,form,i,fake,state,pages,4095);
                        }
                        break; // BCD doesn't use both operands
                    }
                    foreach(ulong bits in MemoryCXBits(form))
                    {
                        MemoryCXSetup(form,fake,state,pages,4095,cw,left);MemoryCXWriteBits(pages,4095,bits,form.Width);
                        MemoryCXOne(code,form,i,fake,state,pages,4095);
                    }
                }
                foreach(bool stackFault in new[]{false,true})
                {
                    MemoryCXSetup(form,fake,state,pages,4095,cw,new(0x8000000000000000,0x3fff));
                    if(stackFault)MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?0:3);
                    MemoryCXOne(code,form,i,fake,state,pages,4095);
                }
            }
        }
    }
    static void MemoryCXSetup(MemoryCXForm form,byte* fake,FullX87RegisterFile* state,byte* pages,int offset,ushort cw,E80 left)
    {
        MemoryX87Seed(fake,state,7,cw);new Span<byte>(pages,12288).Fill(0xa5);Write64(fake,48,0x100000+(uint)offset);
        MemoryX87Tag(fake,state,0,ArithmeticResultTag(left),MemoryX87Raw(left.Significand,left.SignExponent));
        if(form.Bcd&&!form.Store)MemoryX87Tag(fake,state,7,3);
        if(form.Bcd&&!form.Store)MemoryCXWriteInput(pages,offset,new(0x1234567890123456,0x8012));
    }
    static void MemoryCXWriteInput(byte* pages,int offset,E80 value)
    {
        byte[] b=MemoryX87Raw(value.Significand,value.SignExponent);
        for(int p=0;p<10;++p){int at=offset+p;pages[at<4096?at:at+4096]=b[p];}
    }
    static void MemoryCXWriteBits(byte* pages,int offset,ulong value,int width)
    {for(int p=0;p<width/8;++p){int at=offset+p;pages[at<4096?at:at+4096]=(byte)(value>>(p*8));}}


    static void MemoryCXOne(NativeCodeAllocation code,MemoryCXForm form,JitInstruction i,byte* fake,
        FullX87RegisterFile* state,byte* pages,int offset,ulong fs=0,ulong gs=0,bool ticket=false)
    {
        ulong address=MemoryIntegerAddress(i,fake,fs,gs);
        Check((address&~4095UL)==0x100000 || (address&~4095UL)==0x101000,$"R155 fixture guest span EA={address:x16}, offset={offset}, fs={fs:x16}, gs={gs:x16}, IR={i}");
        byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
        byte[] expected=new byte[8192]; new ReadOnlySpan<byte>(pages,4096).CopyTo(expected);
        new ReadOnlySpan<byte>(pages+8192,4096).CopyTo(expected.AsSpan(4096));
        byte[] input;
        if (form.Load) input=expected.AsSpan(offset,form.Width/8).ToArray();
        else
        {
            int top=(int)(((Blink.Machine*)fake)->fpu.sw>>11)&7;
            input=new ReadOnlySpan<byte>(FullX87RegisterFile.Physical(state,top),10).ToArray();
        }
        if (input.Length<10) Array.Resize(ref input,10);
        var completion=MemoryCXExpected(form,i,input,m,raw,expected,offset,address);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(i.Ip,code.Entry);
        var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state; f.FsBase=fs; f.GsBase=gs;
        FullStackMemoryDescriptor descriptor=default; f.StackMemory=&descriptor;
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb=tlb; f.MemoryTlbMask=63;
        if (!ticket)
        {
            SetFullMemoryEntry(tlb,0x100000,pages); SetFullMemoryEntry(tlb,0x101000,pages+8192);
            if(form.Load) { tlb[FullNativeMemory.TlbIndex(0x100000,63)].Permissions=FullMemoryAccess.Read;
                tlb[FullNativeMemory.TlbIndex(0x101000,63)].Permissions=FullMemoryAccess.Read; }
        }
        else
        {
            byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),beforeRaw=CanonicalX87Bytes((byte*)state,192);
            bool accessNeeded=form.Load||completion.Write;
            if (accessNeeded)
            {
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&
                    f.MemoryAddress==address&&f.MemoryAccess==(form.Load?FullMemoryAccess.Read:FullMemoryAccess.Write),
                    "R155 exact fullspan mapping request before guest effects");
                CanonicalX87Same(fake,state,before,beforeRaw);
                int first=Math.Min(form.Width/8,4096-(int)(address&4095));
                byte* firstPointer=offset<4096?pages+offset:pages+8192+offset-4096;
                FullNativeMemory.PublishBarrierTicket(&f,firstPointer,first<form.Width/8?pages+8192:null,(uint)first,epoch,123);
                FullNativeMemory.ResumeSameIssue(&f);
            }
        }
        if (!form.Load&&!completion.Write)
        {
            byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),beforeRaw=CanonicalX87Bytes((byte*)state,192);
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&
                f.MemoryAddress==address&&descriptor.Version==FullCanonicalX87MemoryLowering.MemoryProbeTag&&
                descriptor.Count==1&&descriptor.State==FullStackMemoryState.Request&&descriptor.Ticket==0&&
                descriptor.Request0.Host0==null&&descriptor.Request0.Host1==null,
                "R155 suppressed store still requests fullspan RW proof, no writer pointer/ticket");
            CanonicalX87Same(fake,state,before,beforeRaw);
            // Isolated proof publication models parent mapping-only resolver.
            // Parent integration tests must prove real PTE/EOF/authority checks.
            descriptor.Epoch=epoch; descriptor.State=FullStackMemoryState.Ready;
            FullNativeMemory.ResumeSameIssue(&f);
        }
        Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
            f.HelperIndex==FullBlockFrame.NoHelperIndex&&f.HostMxcsrSaved==0&&f.FaultTrap==0,
            "R155 single native issue/complete, all nonvolatile GPRs, no semantic callback");
        CanonicalX87Same(fake,state,m,raw);
        Check(new ReadOnlySpan<byte>(pages,4096).SequenceEqual(expected.AsSpan(0,4096))&&
            new ReadOnlySpan<byte>(pages+8192,4096).SequenceEqual(expected.AsSpan(4096)),
            "R155 whole pages exactly match independent numeric+instruction completion oracle");
        Check(new ReadOnlySpan<byte>(pages+4096,4096).IndexOfAnyExcept((byte)0xa5)<0,"R155 cross-page transfer never touches unmapped host gap");
        Check(((uint*)((byte*)MemoryX87Work(state)+280))[0]==0&&MemoryX87Work(state)->Continuation==0,
            "R155 no stale live continuation or protocol failure survives RET");
        Check(new ReadOnlySpan<byte>((byte*)MemoryX87Work(state)+292,92).IndexOfAnyExcept((byte)0xa5)<0,
            "R155 kernel/bridge/wrapper preserve unused caller292..383");
        Check(f.NativeMemoryComputations==(form.Load||completion.Write?1UL:0UL),"R155 suppressed memory store performs no data access");
    }

    static void MemoryCXAddresses(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryCXForms())
        foreach (byte[] addressBytes in new byte[][] { [0], [4,0x24], [0x45,0], [0x84,0xcc,0x80,0,0,0],
            [5,0,0,0,0], [4,0x25,0,0,0,0] })
        foreach (byte[] prefix in new byte[][] { [],[0x67],[0x64],[0x65,0x67],[0x4b],[0x65,0x67,0x4b] })
        {
            byte[] encoding=[..prefix,form.Opcode,(byte)(addressBytes[0]|form.Group<<3),..addressBytes.AsSpan(1).ToArray()];
            var i=MemoryCXClassify(encoding); var call=MemoryCXCall(i,calls);
            var original=i;
            foreach (int offset in new[] {0,1,4096-form.Width/8,4095,4097})
            {
                i=original;
                MemoryX87Seed(fake,state,7,0x37f); new Span<byte>(pages,12288).Fill(0xa5);
                for (int r=0;r<16;++r) Write64(fake,24+r*8,0);
                ulong fs=0,gs=0; var a=i.Address;
                ulong target=0x100000+(uint)offset;
                ulong current=MemoryIntegerAddress(i,fake,0,0);
                if (a.Base>=0)
                {
                    ulong b=unchecked(target-current);
                    if (a.Index==a.Base)
                    {
                        // REX.X can make the SIB no-index code a real R12
                        // index, equal to the base. Solve BOTH occurrences;
                        // offsets outside this finite encoding's lattice are
                        // unrepresentable, rather than a native failure.
                        ulong factor=1UL+(1UL<<a.Scale);
                        if (b%factor!=0) continue;
                        b/=factor;
                    }
                    if (a.AddressWidth==32) b|=0xfedcba9800000000;
                    Write64(fake,24+a.Base*8,b);
                }
                else if (a.Segment==4) fs=unchecked(target-current);
                else if (a.Segment==5) gs=unchecked(target-current);
                else
                {
                    // RIP/EIP and no-base displacement adjusted at translation.
                    long disp=unchecked((long)target-(a.RipRelative?(long)(i.Ip+(uint)i.Length):0));
                    if (disp is < int.MinValue or > int.MaxValue) continue;
                    i=i with { Address=a with { Displacement=disp } };
                    // Body with updated compile-time displacement belongs to a
                    // separate allocation; ownership stays synchronous.
                    using var adjusted=CreateFullBlock([new(i,X87Kernel:call)]);
                    MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?3:0,form.Bcd&&!form.Store?null:MemoryX87Raw(0x8000000000000001,0x3fff));
                    MemoryCXOne(adjusted,form,i,fake,state,pages,offset); continue;
                }
                using var code=CreateFullBlock([new(i,X87Kernel:call)]);
                MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?3:0,form.Bcd&&!form.Store?null:MemoryX87Raw(0x8000000000000001,0x3fff));
                MemoryCXOne(code,form,i,fake,state,pages,offset,fs,gs,ticket:offset==4095);
            }
        }
    }

    static void MemoryCXMisses(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach (var form in MemoryCXForms())
        {
            var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryCXCall(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string failure in new[] {"first","second","permission","eof","epoch","barrier"})
            {
                MemoryX87Seed(fake,state,0,0x37f); new Span<byte>(pages,12288).Fill(0xa5); Write64(fake,48,0x100fff);
                MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?3:0,form.Bcd&&!form.Store?null:MemoryX87Raw(0x8000000000000000,0x3fff));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,0x100000,pages);SetFullMemoryEntry(tlb,0x101000,pages+8192);
                ref var e=ref tlb[FullNativeMemory.TlbIndex(failure=="first"?0x100000UL:0x101000UL,63)];
                if (failure is "first" or "second") e=default;
                if (failure=="permission") e.Permissions=form.Load?FullMemoryAccess.Write:FullMemoryAccess.Read;
                if (failure=="eof") e.ValidEnd=4095;
                if (failure=="epoch") e.AuthorityEpoch=10;
                if (failure=="barrier") { if (form.Load) continue; e.Flags=FullMemoryFlags.WriteBarrier; }
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),data=CanonicalX87Bytes(pages,12288);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&f.MemoryResume==1,
                    "R155 first/second/permission/EOF/epoch/barrier miss before any architectural effect");
                CanonicalX87Same(fake,state,before,raw);
                Check(new ReadOnlySpan<byte>(pages,12288).SequenceEqual(data),"R155 both pages unchanged on partial-span miss");
                Check(FullNativeMemory.Fault(&f,f.MemoryAddress,failure=="eof"?unchecked((uint)-2):2)==FullMemoryAssistResult.GuestFault,
                    "R155 mapping assist delivers precise guest fault, no arithmetic fallback");
                CanonicalX87Same(fake,state,before,raw);
                Check(completed==0&&f.Executed==0,"R155 fault preserves RSP and withholds completion");
            }
        }
    }

    static void MemoryCXCanonical(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryCXForms())
        foreach (int register in new[] {3,4,5,12,13})
        foreach (int segment in new[] {-1,2,3,4,5})
        {
            var bytes=new List<byte>();
            if (segment>=0) bytes.Add(segment switch {2=>(byte)0x36,3=>(byte)0x3e,4=>(byte)0x64,_=>(byte)0x65});
            if (register>=8) bytes.Add(0x41);
            bytes.Add(form.Opcode);int rm=register&7;
            bytes.Add((byte)((rm==5?0x40:0)|form.Group<<3|rm));if (rm==4) bytes.Add(0x24);if (rm==5) bytes.Add(0);
            var i=MemoryCXClassify(bytes.ToArray());var call=MemoryCXCall(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (int masks in new[] {0,63})
            foreach (ulong address in new[] {0x0100000000000000UL,0x7fffffffffffUL,ulong.MaxValue})
            {
                MemoryX87Seed(fake,state,4,(ushort)(0x300|masks));Write64(fake,24+register*8,address);
                MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?0:3); // even suppressed empty/full cannot win GP/SS
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(FullScalarMiscLowering.UsesStackSegment(i.Address)?12u:13u)&&
                    f.FaultCode==128&&f.FaultAddress==0&&issued==1&&completed==0&&f.Executed==0,
                    "R155 canonical endpoint/wrap SS versus GP precedes memory, stack fault and status even no-write");
                CanonicalX87Same(fake,state,before,raw);
            }
        }
    }

    static void MemoryCXBoundaries(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryCXForms())
        {
            var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]); var call=MemoryCXCall(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string boundary in new[] {"stop","attention","budget","epoch","null","owner","mf1","mf2","mf4","mf8","mf16","mf32"})
            {
                MemoryX87Seed(fake,state,3,0x37f); Write64(fake,48,0x800000000000); // poison noncanonical EA
                bool pending=boundary.StartsWith("mf",StringComparison.Ordinal);
                if (pending) { uint bit=uint.Parse(boundary.AsSpan(2));((Blink.Machine*)fake)->fpu.sw|=bit;((Blink.Machine*)fake)->fpu.cw&=~bit; }
                if (boundary=="attention") ((Blink.Machine*)fake)->attention=true;
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),work=CanonicalX87Bytes((byte*)MemoryX87Work(state),384);
                ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL,limit:1);
                f.X87=boundary=="null"?null:state;
                if (boundary=="owner") { h.OwnerCookie=f.OwnerCookie=74;state->OwnerToken=74;raw=CanonicalX87Bytes((byte*)state,192); }
                if (boundary=="epoch") ++epoch;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
                var want=boundary switch {"stop"=>FullBlockExit.Stop,"attention"=>FullBlockExit.Attention,
                    "budget"=>FullBlockExit.Budget,_=>pending?FullBlockExit.GuestFault:FullBlockExit.Invalidated};
                Check(RunFull(code,&f)==want&&completed==0&&f.Executed==0&&issued==(pending||boundary=="budget"?1:0),
                    "R155 pre-Issue/previous-MF priority before poisoned memory/kernel");
                CanonicalX87Same(fake,state,before,raw);
                Check(new ReadOnlySpan<byte>(MemoryX87Work(state),384).SequenceEqual(work),"R155 denied/prior-MF never enters Work or kernel");
                if (pending) Check(f.FaultTrap==16,"R155 exactly six pending masks, prior #MF wins noncanonical memory");
            }
            foreach (byte[] raw in new[] {MemoryX87Raw(0xc123456789abcdef,0x7fff),MemoryX87Raw(0x8000000000000000,0x7ffe)})
            {
                if (form.Load) continue;
                MemoryX87Seed(fake,state,3,0x340); new Span<byte>(pages,12288).Fill(0xa5);Write64(fake,48,0x100fff);
                MemoryX87Tag(fake,state,0,MemoryX87TagFor(raw),raw);
                // Suppressed writes require mapping-only RW proof even though
                // no writable data pointer or revocation ticket is consumed.
                MemoryCXOne(code,form,i,fake,state,pages,4095,ticket:true);
            }
        }
    }

    static void MemoryCXDeferred(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryCXForms())
        {
            var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryCXCall(i,calls);
            var wait=new JitInstruction(i.Ip+(uint)i.Length,1,JitOperation.X87,80,default,default,
                Extra:new(0,0,IsImmediate:true),Options:(uint)FullCanonicalX87StackOperation.Fwait);
            using var code=CreateFullBlock([new(i,X87Kernel:call),new(wait)]);
            MemoryX87Seed(fake,state,7,0x37e);new Span<byte>(pages,12288).Fill(0xa5);Write64(fake,48,0x100080);
            MemoryX87Tag(fake,state,form.Bcd&&!form.Store?7:0,form.Bcd&&!form.Store?0:3);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,pages);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop);f.X87=state;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
            FullStackMemoryDescriptor descriptor=default;f.StackMemory=&descriptor;
            if (!form.Load)
            {
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,
                    "R155 newly unmasked empty store first requests RW proof");
                descriptor.Epoch=epoch;descriptor.State=FullStackMemoryState.Ready;FullNativeMemory.ResumeSameIssue(&f);
            }
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==16&&issued==2&&completed==1&&f.Executed==1&&
                ((Blink.Machine*)fake)->ip==wait.Ip&&(((Blink.Machine*)fake)->fpu.sw&0x8081)==0x8081,
                "R155 newly unmasked stack exception completes its instruction; following waiting instruction raises MF");
        }
    }

    static void MemoryCXEncodings(FullX87KernelCall[] calls)
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],common=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] {false,true})
        foreach (bool shared in new[] {false,true})
        {
            FullBlockEmitter.EmitScaffold(common,out var layout,windows);var scaffold=layout.Resolve((nint)0x10000000);
            foreach (var form in MemoryCXForms())
            {
                byte[] bytes=[0x65,0x67,0x4b,form.Opcode,(byte)(0x44|form.Group<<3),0xcc,0xf8];
                var i=MemoryCXClassify(bytes);var call=MemoryCXCall(i,calls);FullBlockInstruction[] body=[new(i,X87Kernel:call)];
                int n; bool good=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out n,windows):FullBlockEmitter.TryEmit(body,staging,out n,windows);
                Check(good&&n>0,"R155 both ABIs/scaffolds/all34 actual memory native bodies");
                int continuationCount=0;
                for (int p=0;p+7<n;++p)
                {
                    if (!staging.AsSpan(p,3).SequenceEqual(new byte[] {0x48,0x8d,0x05})) continue;
                    int target=p+7+BinaryPrimitives.ReadInt32LittleEndian(staging.AsSpan(p+3));
                    if (target<0||target+7>n||!staging.AsSpan(target,7).SequenceEqual(new byte[] {0xf3,0x0f,0x1e,0xfa,0x4d,0x89,0xd9})) continue;
                    ++continuationCount;
                    Check(BinaryPrimitives.ReadUInt64LittleEndian(staging.AsSpan(target-10))==(ulong)call.Entry,
                        "R155 literal typed catalog Entry, exact continuation with restored frame+Machine");
                }
                Check(continuationCount==1,"R155 one specialized memory computation continuation, no runtime selector");
                foreach (int cut in new[] {0,1,7,n-1})
                {
                    int size; bool partial=shared?FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),in scaffold,out size,windows):
                        FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),out size,windows);
                    Check(!partial&&size==0,"R155 short span cannot publish partial native body");
                }
                for (int warm=0;warm<12;++warm)
                {
                    FullCanonicalX87MemoryArithmeticLowering.TryClassify(Ip,bytes,out _,out _);
                    if (shared) FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows);else FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                long allocated=GC.GetAllocatedBytesForCurrentThread();
                for (int warm=0;warm<16;++warm)
                {
                    good&=FullCanonicalX87MemoryArithmeticLowering.TryClassify(Ip,bytes,out _,out _);
                    good&=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good&&allocated==GC.GetAllocatedBytesForCurrentThread(),"R155 warm exact parser/caller-owned emission allocates zero");
            }
        }
    }

    static void MemoryCXBadProtocol(byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach((uint field,uint returned,uint metadata) in new(uint,uint,uint)[]{(1,1,0),(0,1,0),(1,0,0),(0,0,0),(3,3,0)})
        {
            var bytes=new List<byte>{0xf3,0x0f,0x1e,0xfa};
            void Put(int offset,uint value)
            {
                bytes.AddRange([0x41,0xc7,0x83]);Bcd80I32(bytes,offset);Bcd80I32(bytes,unchecked((int)value));
            }
            Put(12,field);for(int p=48;p<88;p+=4)Put(p,0);Put(84,metadata);
            bytes.AddRange([0x49,0xba,0,0,0,0,0,0,0,0,0xb8]);Bcd80I32(bytes,unchecked((int)returned));
            bytes.AddRange([0x41,0xff,0x63,88]);using var kernel=Bcd80Publish(bytes.ToArray());
            foreach(var form in MemoryCXForms())
            {
                var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]);
                var call=new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87MemoryArithmeticKernel(i),kernel.Entry,73,bytes.Count);
                using var code=CreateFullBlock([new(i,X87Kernel:call)]);
                MemoryCXSetup(form,fake,state,pages,128,0x37f,new(0x8000000000000000,0x3fff));
                // Even admission3 must be fatal on a DEFINED FBLD digit input;
                // undefined digits from the real foundation have separate tests.
                if(form.Bcd&&!form.Store)MemoryCXWriteInput(pages,128,default);
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),data=CanonicalX87Bytes(pages,12288);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,pages);
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);f.X87=state;f.FirstPreissued=1;
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;FullStackMemoryDescriptor descriptor=default;f.StackMemory=&descriptor;
                Check(FloatRunProbe(code,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.Executed==0&&
                    f.FirstPreissued==1&&f.FaultTrap==0&&((uint*)((byte*)MemoryX87Work(state)+280))[0]==0x58383746&&
                    ((uint*)((byte*)MemoryX87Work(state)+284))[0]!=0&&MemoryX87Work(state)->Continuation==0,
                    "R155 malformed response is internal fatal before all effects, original issue retained, caller restored");
                CanonicalX87Same(fake,state,before,raw);
                Check(new ReadOnlySpan<byte>(pages,12288).SequenceEqual(data),"R155 malformed kernel cannot write guest memory");
            }
        }
    }
    static void MemoryCXProofFailures(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryCXForms())
        {
            if (form.Load) continue;
            var i=MemoryCXClassify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryCXCall(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string mutation in new[] {"fault","site","address","width","access","host","length","reserved","request1","ticket","epoch","stop","attention"})
            {
                MemoryX87Seed(fake,state,0,0x37e);Write64(fake,48,0x100fff);MemoryX87Tag(fake,state,0,3);
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                FullStackMemoryDescriptor descriptor=default;f.StackMemory=&descriptor;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,
                    "R155 unmasked empty store stages permission proof before flags/metadata");
                Check(FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(&f,in i,(Blink.Machine*)fake),
                    "R155 parent mapping-only request authenticated against exact current typed store");
                CanonicalX87Same(fake,state,before,raw);
                if (mutation=="fault")
                {
                    Check(FullNativeMemory.Fault(&f,0x101000,2)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14,
                        "R155 readonly/unmapped suppressed destination still gets PF");
                    CanonicalX87Same(fake,state,before,raw);continue;
                }
                descriptor.Epoch=epoch;descriptor.State=FullStackMemoryState.Ready;
                switch (mutation)
                {
                    case "site": ++descriptor.Site;break;case "address":++descriptor.Request0.Address;break;
                    case "width":descriptor.Request0.Width=8;break;case "access":descriptor.Request0.Access=FullMemoryAccess.Read;break;
                    case "host":descriptor.Request0.Host0=(byte*)1;break;case "length":descriptor.Request0.Length0=1;break;
                    case "reserved":descriptor.Request0.Reserved40=1;break;case "request1":descriptor.Request1.Width=16;break;
                    case "ticket":descriptor.Ticket=1;break;case "epoch":descriptor.Epoch=10;break;
                }
                FullNativeMemory.ResumeSameIssue(&f);
                if (mutation is "stop" or "attention")
                {
                    if (mutation=="attention")
                    {
                        ((Blink.Machine*)fake)->attention=true;f.Budget=0;
                        // Existing Poll deliberately honors the approved first
                        // issue before attention. This SAME memory instruction
                        // completes; pending attention remains for the next one.
                        Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                            f.Executed==1&&f.FirstPreissued==0&&descriptor.State==FullStackMemoryState.Consumed&&
                            (int)((Blink.Machine*)fake)->attention!=0&&f.MemoryAssists==0,
                            "R155 approved memory resume completes once before attention without new issue or mapping");
                        continue;
                    }
                    stop=1;
                    byte[] pending=CanonicalX87Bytes(fake,sizeof(Blink.Machine));
                    Check(RunFull(code,&f)==FullBlockExit.Stop&&
                        issued==1&&completed==0&&f.FirstPreissued==1&&descriptor.State==FullStackMemoryState.Ready,
                        "R155 stop preserves proof and SAME preissued instruction");
                    CanonicalX87Same(fake,state,pending,raw);
                    stop=0;((Blink.Machine*)fake)->attention=false;f.Budget=0;
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.FirstPreissued==0&&
                        descriptor.State==FullStackMemoryState.Consumed,"R155 resumed approved issue completes once despite exhausted later budget");
                    continue;
                }
                Check(RunFull(code,&f)==(mutation=="epoch"?FullBlockExit.NeedMemory:FullBlockExit.Invalidated)&&
                    issued==1&&completed==0&&f.Executed==0,
                    "R155 proof is bound to exact site/EA/width/zero pointer payload and current epoch");
                CanonicalX87Same(fake,state,before,raw);
                if (mutation=="epoch") Check(descriptor.State==FullStackMemoryState.Request&&f.MemoryResume==1,
                    "R155 stale proof restages exact Request before returning NeedMemory");
            }
        }
    }


    static void MemoryCXUndefinedBcd(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        var form=new MemoryCXForm(0xdf,4,80);var i=MemoryCXClassify([0xdf,0x23]);var call=MemoryCXCall(i,calls);
        using var code=CreateFullBlock([new(i,X87Kernel:call)]);
        foreach(int position in System.Linq.Enumerable.Range(0,18))foreach(int digit in new[]{10,11,12,13,14,15})
        foreach(bool negative in new[]{false,true})foreach(ushort cw in new ushort[]{0x140,0x17f,0x37f,0xf40})
        {
            BigInteger packed=(BigInteger)digit<<(position*4);
            E80 input=new((ulong)(packed&ulong.MaxValue),(ushort)((ulong)(packed>>64)|(negative?0x8000UL:0)));
            MemoryCXSetup(form,fake,state,pages,4095,cw,default);
            ((Blink.Machine*)fake)->fpu.sw&=~0x7fu; // no prior SF/IE can hide a new contribution
            MemoryCXWriteInput(pages,4095,input);MemoryCXOne(code,form,i,fake,state,pages,4095);
            int top=(int)(((Blink.Machine*)fake)->fpu.sw>>11)&7;var physical=FullX87RegisterFile.Physical(state,top);E80 output=new(physical->Significand,physical->SignExponent);
            Check(E80Equal(output,new(0xc000000000000000,0xffff))&&(((Blink.Machine*)fake)->fpu.sw&0x27f)==0&&
                ((uint*)((byte*)MemoryX87Work(state)+280))[0]==0,
                "R155 every A..F digit position is explicit undefined-software-policy completion, no IE/SF/C1/fatal/fallback");
        }
    }

}
