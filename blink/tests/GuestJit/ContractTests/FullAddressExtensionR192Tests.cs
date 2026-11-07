using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using Address192Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE_AUTHORED_UNRUN. ONE family entry for the parent's coalesced gate.
    // Both ABI encodings, both scaffold modes; execute only the actual host ABI.
    static void CheckFullAddressExtensionsR192(byte* machine)
    {
        Address192Admission();
        Address192AbiEncoding();
        {
        using var oracle = new Address192Fixture(interpreterOracle: true);
        foreach (bool shared in new[] { false, true })
        {
            using var slab = CreateFullScaffold(out var addresses);
            Check(!fullOwnerScaffold.IsValid, "R192 scaffold fixture begins isolated");
            fullOwnerScaffold = shared ? addresses : default;
            try
            {
                Address192LeaNative(oracle.Program, machine);
                Address192RegisterNative(oracle.Program, machine);
                Address192MemoryNative(machine);
                Address192Polls(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
        } // Release the current producer/barrier binding before another controller fixture.
        Address192SelectedMemoryOracle();
        Address192RealMmu();
        Address192Controller();
    }

    readonly record struct Lea192Row(byte[] Bytes, int Width, int Destination, JitAddress Address, bool Oracle = true);
    static IEnumerable<Lea192Row> Address192LeaRows()
    {
        // Every destination/base/index 0..15, every SIB scale, both address
        // sizes, all six real operand/address width cells. mod1 forces a base.
        foreach (int width in new[] { 16, 32, 64 })
        foreach (bool a32 in new[] { false, true })
        for (int r = 0; r < 16; ++r)
        for (int scale = 0; scale < 4; ++scale)
        {
            int index = (r + 1) & 15, destination = scale % 2 == 0 ? r : index;
            // raw index4/no X is the architectural no-index sentinel.
            int actualIndex = index == 4 ? -1 : index;
            int rex = 0x40 | (width == 64 ? 8 : 0) | (destination >> 3 << 2) |
                (index >> 3 << 1) | (r >> 3);
            byte[] bytes = Address192Bytes(width, a32, -1, rex,
                [0x8d, (byte)(0x44 | (destination & 7) << 3),
                (byte)(scale << 6 | (index & 7) << 3 | (r & 7)), 0x80]);
            yield return new(bytes, width, destination, new(r, actualIndex, scale, -128, false, a32 ? 32 : 64));
        }
        foreach (int segment in new[] { 0x26, 0x2e, 0x36, 0x3e, 0x64, 0x65 })
            yield return new([(byte)segment,0x67,0x66,0x8d,0x43,0xff],16,0,
                new(3,-1,0,-1,false,32,segment switch {0x26=>0,0x2e=>1,0x36=>2,0x3e=>3,0x64=>4,_=>5}));
        yield return new([0x48,0x8d,0x05,0xff,0xff,0xff,0xff],64,0,new(-1,-1,0,-1,true));
        yield return new([0x67,0x48,0x8d,0x05,0xfe,0xff,0xff,0xff],64,0,new(-1,-1,0,-2,true,32));
        // REX.B cannot create R13 in mod0/rm5 or mod0/SIB.base5.
        yield return new([0x49,0x8d,0x05,0,0,0,0x80],64,0,new(-1,-1,0,int.MinValue,true));
        yield return new([0x4b,0x8d,0x04,0xa5,0xff,0xff,0xff,0xff],64,0,new(-1,12,2,-1));
        yield return new([0x48,0x8d,0x04,0x25,0xff,0xff,0xff,0x7f],64,0,new(-1,-1,0,int.MaxValue));
        // R159 aliases are a named software profile, separate from ISA syntax.
        yield return new([0x48,0x66,0x67,0x8d,0x43,0],16,0,new(3,-1,0,0,false,32),false);
        yield return new([0x66,0x66,0x67,0x67,0x4f,0x40,0x8d,0x43,0],16,0,new(3,-1,0,0,false,32),false);
        yield return new([0x66,0x48,0x8d,0x43,0],64,0,new(3,-1,0,0));
    }

    static byte[] Address192Bytes(int width, bool a32, int segment, int rex, byte[] body)
    {
        var result = new List<byte>();
        if (segment >= 0) result.Add((byte)segment);
        if (a32) result.Add(0x67);
        if (width == 16) result.Add(0x66);
        if (rex != 0) result.Add((byte)rex);
        result.AddRange(body); return result.ToArray();
    }
    static JitInstruction Address192Classify(byte[] bytes, ulong ip = Ip)
    {
        Check(FullAddressExtensionLowering.TryClassify(ip,bytes,out var i,out var reason), reason ?? "R192 classification");
        Check(FullTranslationJit.TryClassifyNative(ip,bytes,out var actual) && actual == i,
            "R192 production classification precedes the legacy core and preserves original identity");
        return i;
    }
    static void Address192Admission()
    {
        foreach (var row in Address192LeaRows())
        {
            var i=Address192Classify(row.Bytes);
            Check(i.Operation==JitOperation.Lea && i.Width==row.Width && i.Destination==new JitOperand(row.Destination,0) &&
                i.Address==row.Address && i.Source==default && i.Extra==default && i.Options==0 && i.SourceWidth==0 &&
                FullBlockEmitter.ValidateAddressExtension(i), "R192 exact LEA independent encoding table");
        }
        foreach(byte[] bad in new byte[][] { [0x8d,0xc0], [0x66,0x0f,0xb7,0xc0], [0x66,0x0f,0xbf,0x00],
            [0xf0,0x8d,0x00], [0xf3,0x63,0xc0], [0xf2,0x0f,0xb6,0x00], [0x48,0x8d,0x04],
            [0x67,0x8d,0x05,0,0,0], [0x8d,0x00,0x90], [0x64,0x65,0x8d,0x00] })
            Check(!FullTranslationJit.TryClassifyNative(Ip,bad,out _), "R192 invalid/undefined/policy cell cannot fall through legacy core");
        var lea=Address192Classify([0x66,0x8d,0x03]);
        var ext=Address192Classify([0x0f,0xb6,0xc4]);
        foreach(var bad in new[] {lea with {Source=new(1,0)},lea with {Extra=new(1,0)},lea with {Options=1},
            lea with {Target=1},lea with {SourceWidth=16},lea with {Address=lea.Address with {AddressWidth=16}},
            lea with {Destination=new(0,1)},ext with {SourceWidth=32},ext with {Width=8},
            ext with {Source=new(4,0,ByteOffset:1)},ext with {Condition=1},ext with {Address=new(-1,-1,0,0)}})
        {
            bool rejected=false;
            try { FullBlockEmitter.ValidateAddressExtension(bad); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R192 strict validator rejects all unused/contradictory operands");
        }
    }

    static void Address192AbiEncoding()
    {
        byte[] storage=new byte[FullBlockEmitter.CodeCapacity], slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows);
            var shared=layout.Resolve((nint)0x700000000000);
            foreach(byte[] bytes in new byte[][] { [0x67,0x66,0x8d,0x03], [0x67,0x48,0x8d,0x03],
                [0x66,0x63,0x03], [0x63,0x03], [0x48,0x63,0x03], [0x48,0x0f,0xbf,0x03], [0x0f,0xbe,0xc4] })
            {
                FullBlockInstruction[] sites=[new(Address192Classify(bytes))];
                Check(FullBlockEmitter.Emit(sites,storage,windows)>0 &&
                    FullBlockEmitter.Emit(sites,storage,in shared,windows)>0,"R192 both ABIs and scaffolds encode family through real writer");
            }
        }
    }
    static void Address192Seed(byte* machine, ulong ip=Ip)
    {
        IntegerSeed(machine,0xa52c0e15); Write64(machine,0,ip);
        for(int r=0;r<16;++r)Write64(machine,24+r*8,r%2==0?0xfedcba988000ff80UL:0xfedcba987654007fUL);
        new Span<byte>(machine+160,256).Fill(0x5a); // XMM/media must remain byte-identical
    }
    static ulong Address192Read(byte[] machine,int r) => BinaryPrimitives.ReadUInt64LittleEndian(machine.AsSpan(24+r*8,8));
    static ulong Address192Offset(JitAddress a,byte[] before,ulong ip,int length)
    {
        ulong value=unchecked((ulong)a.Displacement+(a.RipRelative?ip+(uint)length:0));
        if(a.Base>=0)value=unchecked(value+Address192Read(before,a.Base));
        if(a.Index>=0)value=unchecked(value+(Address192Read(before,a.Index)<<a.Scale));
        return a.AddressWidth==32?(uint)value:value;
    }
    static byte[] Address192Expected(byte[] before,JitInstruction i,ulong value)
    {
        byte[] expected=(byte[])before.Clone();
        BinaryPrimitives.WriteUInt64LittleEndian(expected,unchecked(i.Ip+(uint)i.Length));
        if(i.Width==16)BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(24+i.Destination.Register*8),unchecked((ushort)value));
        else BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24+i.Destination.Register*8),i.Width==32?(uint)value:value);
        expected[8]=0;return expected;
    }
    static void Address192Run(byte* machine,JitInstruction i,byte[] expected,bool poison=false)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
        if(poison)
        {
            f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
            f.FsBase=ulong.MaxValue;f.GsBase=0x800000000000;
        }
        Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
            f.NativeMemoryComputations==0&&f.MemoryResume==0&&f.MemoryAssists==0,
            "R192 no data access for LEA/register extension, even with poisoned memory authority");
        Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(expected),
            "R192 independent math: entire Machine, flags, media, aliases, partial/zero-extension and original next-IP");
    }
    static void Address192LeaNative(Blink oracle,byte* machine)
    {
        foreach(var row in Address192LeaRows())
        {
            // Include EIP and RIP arithmetic wrap without fetching at synthetic PCs.
            ulong pc=row.Address.RipRelative?ulong.MaxValue-3:Ip;
            var i=Address192Classify(row.Bytes,pc);Address192Seed(machine,pc);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong value=Address192Offset(row.Address,before,pc,row.Bytes.Length);
            if(row.Oracle)
            {
                IntegerReference(oracle,machine,row.Bytes);
                byte[] math=Address192Expected(before,i,value);
                Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(math),
                    "R192 actual decoded GetOp LEA common-domain whole Machine oracle");
                before.AsSpan().CopyTo(new Span<byte>(machine,before.Length));
            }
            Address192Run(machine,i,Address192Expected(before,i,value),poison:true);
        }
    }

    readonly record struct Extension192Row(byte[] Bytes,int Width,int SourceWidth,int Destination,int Source,int ByteOffset,bool Signed,bool Oracle=true);
    static IEnumerable<Extension192Row> Address192RegisterRows()
    {
        foreach(int width in new[]{16,32,64})
        foreach(int sw in new[]{8,16,32})
        foreach(bool signed in new[]{false,true})
        {
            if(sw>=width||sw==32&&(!signed||width!=64))continue;
            for(int r=0;r<16;++r)
            {
                int source=(r+7)&15,rex=0x40|(width==64?8:0)|(r>>3<<2)|(source>>3);
                byte[] body=sw==32?[(byte)0x63,(byte)(0xc0|(r&7)<<3|(source&7))]:
                    [(byte)0x0f,(byte)((signed?0xbe:0xb6)+(sw==16?1:0)),(byte)(0xc0|(r&7)<<3|(source&7))];
                yield return new(Address192Bytes(width,true,0x65,rex,body),width,sw,r,source,0,signed);
            }
        }
        yield return new([0x0f,0xbe,0xc4],32,8,0,0,1,true); // AL<-AH alias
        yield return new([0x66,0x0f,0xb6,0xe4],16,8,4,0,1,false); // SP<-AH, destination is not AH
        yield return new([0x66,0x63,0xc0],16,16,0,0,0,false);
        yield return new([0x63,0xc0],32,32,0,0,0,false);
        yield return new([0x66,0x48,0x63,0xc0],64,32,0,0,0,true);
        for(int high=0;high<4;++high)
            yield return new([0x0f,0xbe,(byte)(0xd8|4|high)],32,8,3,high,1,true);
        yield return new([0x40,0x66,0x0f,0xbe,0xc4],16,8,0,0,1,true,false);
        yield return new([0x66,0x40,0x40,0x0f,0xbe,0xc4],16,8,0,4,0,true,false);
    }
    static ulong Address192Extend(ulong value,int width,bool signed)
    {
        ulong mask=width==32?uint.MaxValue:width==16?ushort.MaxValue:byte.MaxValue;
        value&=mask;return signed&&(value&(1UL<<(width-1)))!=0?value|~mask:value;
    }
    static void Address192RegisterNative(Blink oracle,byte* machine)
    {
        foreach(var row in Address192RegisterRows())
        {
            var i=Address192Classify(row.Bytes);Address192Seed(machine);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong value=Address192Extend(Address192Read(before,row.Source)>>(8*row.ByteOffset),row.SourceWidth,row.Signed);
            byte[] expected=Address192Expected(before,i,value);
            Check(i.Width==row.Width&&i.Destination.Register==row.Destination&&i.Source.Register==row.Source&&
                i.Source.ByteOffset==row.ByteOffset,"R192 independent register/high-byte table");
            if(row.Oracle)
            {
                IntegerReference(oracle,machine,row.Bytes);
                Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(expected),
                    "R192 actual decoded extension GetOp agrees on register common-domain whole Machine");
            }
            before.AsSpan().CopyTo(new Span<byte>(machine,before.Length));Address192Run(machine,i,expected,poison:true);
        }
    }

    static IEnumerable<byte[]> Address192MemoryRows()
    {
        (byte[] Bytes,int Width,int SourceWidth)[] ordinary = [([0x66,0x63,0x03],16,16),([0x63,0x03],32,32),([0x48,0x63,0x03],64,32),
            ([0x66,0x0f,0xb6,0x03],16,8),([0x0f,0xb6,0x03],32,8),([0x48,0x0f,0xb6,0x03],64,8),
            ([0x0f,0xb7,0x03],32,16),([0x48,0x0f,0xb7,0x03],64,16),([0x66,0x0f,0xbe,0x03],16,8),
            ([0x0f,0xbe,0x03],32,8),([0x48,0x0f,0xbe,0x03],64,8),([0x0f,0xbf,0x03],32,16),([0x48,0x0f,0xbf,0x03],64,16)];
        foreach(var row in ordinary)
        foreach(int segment in new[]{-1,0x26,0x2e,0x36,0x3e,0x64,0x65})
        foreach(bool a32 in new[]{false,true})
        {
            byte[] bytes=segment<0?(a32?[0x67,..row.Bytes]:row.Bytes):a32?[(byte)segment,0x67,..row.Bytes]:[(byte)segment,..row.Bytes];
            var i=Address192Classify(bytes);
            Check(i.Width==row.Width&&i.SourceWidth==row.SourceWidth&&i.Address.AddressWidth==(a32?32:64)&&
                i.Source==new JitOperand(0,0,IsMemory:true),"R192 independent finite13 width cells x address-size x segment-prefix table");
            yield return bytes;
        }
    }

    static void Address192MemoryNative(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(8192);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(byte[] bytes in Address192MemoryRows())
            foreach(int offset in new[]{64,4095})
            {
                var i=Address192Classify(bytes);using var code=CreateFullBlock([new(i)]);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();new Span<byte>(data,8192).Fill(0x85);
                SetFullMemoryEntry(tlb,Page,data);SetFullMemoryEntry(tlb,Page+4096,data+4096);
                Address192Seed(machine);Write64(machine,48,Page+(uint)offset);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] media=new ReadOnlySpan<byte>(data,8192).ToArray();
                ulong value=Address192Extend(0x85858585,i.SourceWidth,i.Operation==JitOperation.Movsx);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.NativeMemoryComputations==1&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(Address192Expected(before,i,value))&&
                    new ReadOnlySpan<byte>(data,8192).SequenceEqual(media),"R192 exact-width native extension, page crossing, flags/media/whole Machine");
            }
            // A two-byte architectural66/63 load at page end succeeds with no
            // following page. Selected GetOp reads4; do not use it as oracle.
            var shortCopy=Address192Classify([0x66,0x63,0x03]);using(var code=CreateFullBlock([new(shortCopy)]))
            {
                Address192Seed(machine);Write64(machine,48,Page+4094);new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,Page,data);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(shortCopy.SourceWidth==16&&RunFull(code,&f)==FullBlockExit.Continue&&completed==1,
                    "R192 Intel architectural66/63 reads2, selected-source read4 discrepancy explicit");
            }
            foreach(byte segment in new byte[]{0x64,0x65})
            {
                var fs=Address192Classify([segment,0x67,0x48,0x0f,0xbf,0x03]);using var code=CreateFullBlock([new(fs)]);
                const ulong linear=0x200000fff;Address192Seed(machine);Write64(machine,48,0x12345678ffffffff);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,linear&~4095UL,data);
                SetFullMemoryEntry(tlb,(linear&~4095UL)+4096,data+4096);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                if(segment==0x64)f.FsBase=0x100001000;else f.GsBase=0x100001000;
                Check(RunFull(code,&f)==FullBlockExit.Continue&&f.MemoryAddress==linear&&*(ulong*)(machine+24)==0xffffffffffff8585,
                    "R192 AS32 offset wrap then full FS/GS base then linear byte addressing across4GiB boundary");
            }
            Address192MemoryAddresses(machine,tlb,data);
            Address192Faults(machine,tlb,data);
        }
        finally {NativeMemory.Free(data);}
    }

    static void Address192MemoryAddresses(byte* machine,FullMemoryTlbEntry* tlb,byte* data)
    {
        foreach(bool a32 in new[]{false,true})
        foreach(bool relative in new[]{false,true})
        {
            ulong pc=a32&&relative?0xfffffffcUL:Ip,ea=Page+4095;
            byte[] bytes=relative?Address192Bytes(64,a32,-1,0x48,[0x63,0x05,0,0,0,0]):
                Address192Bytes(64,a32,-1,0x4b,[0x63,0x44,0xa5,0x80]); // R13+R12*4-128
            if(relative)
            {
                uint next=unchecked((uint)(pc+(uint)bytes.Length));
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(bytes.Length-4),unchecked((uint)ea-next));
            }
            var i=Address192Classify(bytes,pc);using var code=CreateFullBlock([new(i)]);
            Address192Seed(machine,pc);Write64(machine,24+13*8,ea+128-12);Write64(machine,24+12*8,3);
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,Page,data);SetFullMemoryEntry(tlb,Page+4096,data+4096);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(pc,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&f.MemoryAddress==ea&&issued==1&&completed==1&&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(Address192Expected(before,i,0xffffffff85858585)),
                "R192 memory RIP/EIP wrap and extended SIB base/index feed exact-width native sign extension");
        }
    }

    static void Address192Faults(byte* machine,FullMemoryTlbEntry* tlb,byte* data)
    {
        foreach(byte[] bytes in new byte[][]{[0x36,0x48,0x63,0x03],[0x48,0x63,0x45,0],
            [0x3e,0x0f,0xbf,0x45,0],[0x48,0x0f,0xb6,0x03],[0x64,0x67,0x0f,0xb7,0x03]})
        {
            var i=Address192Classify(bytes);using var code=CreateFullBlock([new(i)]);
            Address192Seed(machine);Write64(machine,24+i.Address.Base*8,0x800000000000);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
            if(i.Address.AddressWidth==32)f.FsBase=0x800000000000;
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(FullScalarMiscLowering.UsesStackSegment(i.Address)?12u:13u)&&
                f.FaultCode==128&&f.FaultAddress==0&&issued==1&&completed==0&&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R192 #GP/#SS precedes poisoned TLB, no register/flag/media commit");
        }
        foreach(string failure in new[]{"miss","rights","cross-page","epoch"})
        {
            var i=Address192Classify([0x48,0x63,0x03]);using var code=CreateFullBlock([new(i)]);
            Address192Seed(machine);Write64(machine,48,Page+4095);new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            SetFullMemoryEntry(tlb,Page,data);SetFullMemoryEntry(tlb,Page+4096,data+4096);
            ref var entry=ref tlb[FullNativeMemory.TlbIndex(Page,63)];
            if(failure=="miss")entry=default;if(failure=="rights")entry.Permissions=FullMemoryAccess.Write;
            if(failure=="epoch")entry.AuthorityEpoch=10;
            if(failure=="cross-page")tlb[FullNativeMemory.TlbIndex(Page+4096,63)]=default;
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==32&&f.MemoryResume==1&&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R192 missing capability cannot partially commit extension");
            SetFullMemoryEntry(tlb,Page,data);SetFullMemoryEntry(tlb,Page+4096,data+4096);FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R192 same-issue resume performs native computation once");
        }
    }

    static void Address192Polls(byte* machine)
    {
        foreach(byte[] bytes in new byte[][]{[0x67,0x66,0x8d,0x03],[0x48,0x63,0x03]})
        foreach(bool budget in new[]{false,true})
        {
            var i=Address192Classify(bytes);using var code=CreateFullBlock([new(i)]);Address192Seed(machine);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=budget?0:1;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget?0UL:10UL);f.MemoryTlb=(FullMemoryTlbEntry*)1;
            Check(RunFull(code,&f)==(budget?FullBlockExit.Budget:FullBlockExit.Stop)&&issued==(budget?1:0)&&completed==0&&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R192 stop/budget before any operand or memory effect");
        }
    }

    static void Address192SelectedMemoryOracle()
    {
        using var f=new Address192Fixture(interpreterOracle:true);ulong page=Address192Fixture.Base;f.Map(page);f.Fill(page);
        foreach(var bytes in Address192MemoryRows())
        {
            var i=Address192Classify(bytes);if(i.Operation==JitOperation.Mov&&i.Width==16)continue; // read-width discrepancy
            f.DataScope(0,()=>
            {
                byte* m=(byte*)f.Machine();f.Machine()->ip=Ip;f.Machine()->oplen=0;Write64(m,48,page+72);Write64(m,24,ulong.MaxValue);
                byte[] media=new ReadOnlySpan<byte>(m+160,256).ToArray();uint flags=f.Machine()->flags;
                ulong value=Address192Extend(0xcccccccc,i.SourceWidth,i.Operation==JitOperation.Movsx);
                IntegerReference(f.Program,m,bytes);
                ulong expected=i.Width==16?0xffffffffffff0000UL|(ushort)value:i.Width==32?(uint)value:value;
                Check(*(ulong*)(m+24)==expected&&f.Machine()->flags==flags&&f.Machine()->ip==Ip+(uint)bytes.Length&&
                    new ReadOnlySpan<byte>(m+160,256).SequenceEqual(media),"R192 actual decoded GetOp memory oracle on explicit common domain");
            });
        }
    }
    static void Address192RealMmu()
    {
        using var f=new Address192Fixture();ulong page=Address192Fixture.Base;f.Map(page);f.Fill(page);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var frame=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=page+4094,MemoryAccess=FullMemoryAccess.Read,MemoryWidth=16,MemoryResume=1};
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.Resolved,"R192 actual MMU reads2 at last mapped word");
        new Span<FullMemoryTlbEntry>(tlb,64).Clear();frame.MemoryWidth=32;
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&frame.FaultAddress==page+4096,
            "R192 actual MMU four-byte selected-source access would fault; architectural width difference is observable");
    }
    static void Address192Controller()
    {
        foreach(bool warm in new[]{false,true})
        {
            using var f=new Address192Fixture();ulong pc=Address192Fixture.Base;f.Map(pc);f.Fill(pc);
            // Native loop: ordinary LEA16, AS32 LEA64, MOVSXD, MOVZX, MOVSX, JMP.
            byte[] body=[0x66,0x8d,0x43,1,0x67,0x48,0x8d,0x4b,2,0x48,0x63,0xd1,
                0x0f,0xb6,0xf0,0x48,0x0f,0xbe,0xfe,0xeb,0xeb];
            WriteControllerCode(f,pc,body);Write64((byte*)f.Machine(),48,0x123400000010);
            if(warm)
            {
                f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
                f.Counts(1000);
                Check(f.Step(pc)&&f.Completed==1,"R192 bounded real warm publication does not terminate owner budget");
            }
            f.Counts(18);
            Check(f.Step(pc),"R192 actual controller starts cold/warm native family");
            // First code-page admission may require cap1; the controller is
            // allowed to return before the loop reaches the owner budget.
            for(int n=0;n<8&&f.Completed<18;++n)
                Check(f.Continue(),"R192 cold admission resumes native family");
            Check(f.Completed==18&&f.Issued==19&&f.Statistics.InlineNativeInstructions>=18&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0&&f.Statistics.NativeMemoryAssists==0,
                $"R192 cold/warm={warm}: exact native family budget, completed={f.Completed}, issued={f.Issued}, helpers={f.Statistics.SemanticHelperInstructions}");
            var store=f.Jit.Store.GetStatistics();Check(store.ActiveNativeEpochs==0&&store.ActiveManagedLeases==0&&f.Statistics.ActiveDataBorrows==0,
                "R192 controller releases native/mapping authority");
        }
    }
}
