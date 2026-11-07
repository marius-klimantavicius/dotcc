using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using X = Managed.Emulation.Execution.FullBlockEmitter.FullX87Operation;

unsafe partial class Program
{
    // R116: ALL AUTHORED_UNRUN. The parent alone builds/emits/executes these.
    // A selected GetOp oracle ALWAYS runs inside the fixture's DataScope.
    // Seed/restore guest fields only; never ResetFull an owned Machine.
    static void CheckFullNativeX87(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedX87Layout(), "R116 actual f64 stack/layout ABI6/frame400/header160");
        CheckFullX87Classifier();
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
            fixture.Map(ThreadedGuestExecution.FullControllerFixture.Base, 8192);
            fixture.Fill(ThreadedGuestExecution.FullControllerFixture.Base, 8192);
            Check(!fullOwnerScaffold.IsValid, "R116 standalone starts clear");
            CheckFullX87Stack(machine); CheckFullX87Math(machine); CheckFullX87Compare(machine);
            CheckFullX87Memory(machine); CheckFullX87Pending(machine); CheckFullX87Oracle(fixture);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try
                {
                    CheckFullX87Stack(machine); CheckFullX87Math(machine); CheckFullX87Compare(machine);
                    CheckFullX87Memory(machine); CheckFullX87Pending(machine); CheckFullX87Oracle(fixture);
                }
                finally { fullOwnerScaffold = default; }
            }
        }
        CheckFullX87Controller(); CheckFullX87Emission();
    }

    static JitInstruction X87Classify(ReadOnlySpan<byte> bytes)
    {
        Check(FullX87Lowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R116 classify");
        Check(FullBlockEmitter.CanInlineX87(i) && FullBlockEmitter.ValidateX87(i), "R116 typed x87 validation"); return i;
    }
    static byte[] X87Encoding(byte opcode, byte modrm, params byte[] prefixes)
    {
        var b = new List<byte>(15); b.AddRange(prefixes); b.Add(opcode); b.Add(modrm);
        if (modrm < 0xc0)
        {
            int mod = modrm >> 6, rm = modrm & 7;
            if (rm == 4) b.Add(0x24);
            if (mod == 1) b.Add(0);
            if (mod == 2 || mod == 0 && rm == 5) b.AddRange(new byte[4]);
        }
        return b.ToArray();
    }
    // Independent sparse-domain table. Selected-only aliases are deliberately
    // tested separately from the architecture-only DAE9 FUCOMPP addition.
    static bool X87SelectedOrdinary(byte opcode, byte modrm)
    {
        int g = modrm >> 3 & 7, r = modrm & 7;
        if (modrm < 0xc0) return opcode switch
        {
            0xd8 or 0xda or 0xdc or 0xde => true, 0xd9 => g != 1,
            0xdb => g is 0 or 1 or 2 or 3 or 5 or 7, 0xdd => g != 5,
            0xdf => g is 0 or 1 or 2 or 3 or 5 or 7, _ => false
        };
        return opcode switch
        {
            0xd8 or 0xdc or 0xde => true,
            0xd9 => g <= 3 || g == 4 && r is 0 or 1 or 4 or 5 || g == 5 && r < 7 ||
                g == 6 && r >= 4 || g == 7 && r is 0 or 2 or 4 or 5,
            0xda => g <= 3,
            0xdb => g <= 3 || g == 4 && r is 2 or 3 || g is 5 or 6,
            0xdd => g <= 5, 0xdf => g <= 6, _ => false
        };
    }
    static void CheckFullX87Classifier()
    {
        int tuples = 0;
        for (int op = 0xd8; op <= 0xdf; ++op) for (int mr = 0; mr < 256; ++mr)
        {
            byte[] bytes = X87Encoding((byte)op, (byte)mr);
            bool expected = X87SelectedOrdinary((byte)op, (byte)mr) || op == 0xda && mr == 0xe9;
            bool found = FullX87Lowering.TryClassify(Ip, bytes, out var i, out var reason);
            Check(found == expected && (found || reason != null), "R116 all2048 selector tuples, including sparse holes, complex and disabled BCD");
            if (found)
            {
                Check(FullBlockEmitter.ValidateX87(i) && i.Extra.Immediate == (ulong)((op & 7) << 8 | mr), "R116 exact FOP is compile metadata only");
                foreach (byte[] prefix in new byte[][] { [0x66], [0x67], [0x4f], [0x64, 0x65, 0x66, 0x67, 0x4f], [0x4f, 0x66, 0x67], [0xf2, 0xf3] })
                {
                    var v = X87Classify(X87Encoding((byte)op, (byte)mr, prefix));
                    Check(FullBlockEmitter.X87Operation(v) == FullBlockEmitter.X87Operation(i), "R116 effective/repeated prefixes, ignored ST REX extensions");
                }
                Check(!FullX87Lowering.TryClassify(Ip, bytes.Concat(new byte[] { 0x90 }).ToArray(), out _, out _), "R116 exact decoder span");
            }
            ++tuples;
        }
        Check(tuples == 2048, "R116 selector inventory count is NOT execution coverage");
        Check(X87Classify([0x9b]).Extra == default, "R116 standalone FWAIT has no invented FOP");
        Check(X87Classify([0x65,0x67,0x4b,0xdb,0x6c,0x8b,0xf8]).Address == new JitAddress(11,9,2,-8,false,32,5), "R116 m80 GS/EA32/high SIB");
        Check(X87Classify([0x4f,0x67,0x64,0xdd,0x44,0x8b,0xf8]).Address == new JitAddress(3,1,2,-8,false,32,4), "R116 legacy cancels prior REX");
        Check(X87Classify([0x67,0x41,0xdb,0x2d,0,0,0,0]).Address == new JitAddress(-1,-1,0,0,true,32), "R116 EIP-relative special REX.B ignored");
        Check(X87Classify([0x41,0xdb,0x2c,0x25,0,0,0,0]).Address.Base == -1, "R116 no-base SIB ignores REX.B");
        foreach (byte[] b in new byte[][] { [0xd9,0xf0],[0xd9,0xf1],[0xd9,0xf2],[0xd9,0xf3],[0xd9,0xf9],[0xd9,0xfb],[0xd9,0xfe],[0xd9,0xff] })
            Check(!FullX87Lowering.TryClassify(Ip,b,out _,out var why) && why == "NativeX87Complex" + FullX87Lowering.ComplexName(b[0],b[1]) + "Missing", "R116 each complex operation visibly MISSING");
        var good = X87Classify([0xd8,0xc1]);
        foreach (var bad in new[] { good with { Width=80 }, good with { Length=0 }, good with { Target=1 }, good with { Condition=1 },
            good with { Extra=default }, good with { Extra=new(0,0x800,true) }, good with { Options=good.Options|0x100000 },
            good with { Source=new(8,0) }, good with { SourceWidth=80 }, good with { Address=new(3,-1,0,0) },
            good with { Options=good.Options|3u<<16 }, good with { Options=good.Options|FullBlockEmitter.X87IntegerMemory } })
        {
            bool rejected = false; try { FullBlockEmitter.ValidateX87(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected, "R116 malformed IR rejected before emission");
        }
    }
    static void X87Seed(byte* machine, int top = 0)
    {
        var m = (Blink.Machine*)machine;
        m->ip=Ip; m->oplen=0; m->attention=false; m->stashaddr=0;
        m->mode.omode=Blink.XED_MODE_LONG; m->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        m->flags=0xa5020e15u & ~0x50100u; m->mxcsr=0x1f80;
        for(int r=0;r<16;++r) Write64(machine,24+r*8,0x1234000000000000UL+(uint)r);
        m->fpu.sw=(uint)(top<<11); m->fpu.cw=0x37f; m->fpu.tw=0;
        m->fpu.ip=0x123456; m->fpu.dp=0x654321; m->fpu.op=0x123;
        for(int r=0;r<8;++r)m->fpu.st[r]=r+1;
    }
    static int X87Top(byte* machine) => (int)(((Blink.Machine*)machine)->fpu.sw>>11&7);
    static ulong X87StBits(byte* machine,int logical) => (ulong)BitConverter.DoubleToInt64Bits(((Blink.Machine*)machine)->fpu.st[(X87Top(machine)+logical)&7]);
    static void X87Set(byte* machine,int logical,ulong bits,int tag=0)
    {
        var m=(Blink.Machine*)machine;int physical=(X87Top(machine)+logical)&7;
        m->fpu.st[physical]=BitConverter.Int64BitsToDouble((long)bits);
        m->fpu.tw=(m->fpu.tw&~(3<<(physical*2)))|tag<<(physical*2);
    }
    static void X87Run(byte* machine,JitInstruction i,FullMemoryTlbEntry* tlb=null,bool probe=false)
    {
        using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=tlb==null?0u:63u;
        Check((probe?FloatRunProbe(code,&f):RunFull(code,&f))==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
            f.NativeMemoryComputations==(i.Source.IsMemory||i.Destination.IsMemory?1UL:0UL),"R116 generated leaf one issue/commit and exact memory accounting");
        Check(f.HostMxcsrDirty==0,"R116 every native return restores host MXCSR");
    }
    static void CheckFullX87Stack(byte* machine)
    {
        for(int top=0;top<8;++top)for(int r=0;r<8;++r)
        {
            X87Seed(machine,top);ulong a=X87StBits(machine,0),b=X87StBits(machine,r);
            X87Run(machine,X87Classify([(byte)0xd9,(byte)(0xc8+r)]),probe:top==0&&r==0);
            Check(X87StBits(machine,0)==b&&X87StBits(machine,r)==a&&X87Top(machine)==top,"R116 FXCH logical aliases/allTOP");
            X87Seed(machine,top);a=X87StBits(machine,0);
            X87Run(machine,X87Classify([0xdd,(byte)(0xd8+r)]));
            Check(X87Top(machine)==((top+1)&7) && (ulong)BitConverter.DoubleToInt64Bits(((Blink.Machine*)machine)->fpu.st[(top+r)&7])==a &&
                (((Blink.Machine*)machine)->fpu.tw>>(top*2)&3)==3,"R116 FSTP destination written before pop including ST0 alias");
            X87Seed(machine,top);X87Run(machine,X87Classify([0xdd,(byte)(0xc0+r)]));
            Check((((Blink.Machine*)machine)->fpu.tw>>(((top+r)&7)*2)&3)==3 && X87Top(machine)==top,"R116 FFREE exact physical tag only");
            if(r<7)
            {
                X87Seed(machine,top);b=X87StBits(machine,r);X87Set(machine,7,X87StBits(machine,7),3);
                X87Run(machine,X87Classify([0xd9,(byte)(0xc0+r)]));
                Check(X87Top(machine)==((top+7)&7)&&X87StBits(machine,0)==b,"R116 FLD reads old logical STi before TOP decrement");
            }
        }
        foreach((byte rm,ulong bits) in new (byte,ulong)[]{(0,0x3ff0000000000000),(1,0x400a934f0979a371),(2,0x3ff71547652b82fe),
            (3,0x400921fb54442d18),(4,0x3fd34413509f79ff),(5,0x3fe62e42fefa39ef),(6,0)})
        {
            X87Seed(machine);X87Set(machine,7,0,3);X87Run(machine,X87Classify([0xd9,(byte)(0xe8+rm)]));
            Check(X87StBits(machine,0)==bits&&X87Top(machine)==7,"R116 constants in inherited double model (not 80bit constants qualification)");
        }
        foreach(byte modrm in new byte[]{0xf6,0xf7})
        {
            X87Seed(machine,7);int tags=((Blink.Machine*)machine)->fpu.tw;X87Run(machine,X87Classify([0xd9,modrm]));
            Check(X87Top(machine)==(modrm==0xf6?6:0)&&((Blink.Machine*)machine)->fpu.tw==tags,"R116 TOP wraps without rotation/tag changes");
        }
        X87Seed(machine);X87Run(machine,X87Classify([0xd9,0xe8]));
        Check(X87StBits(machine,0)==0xfff8000000000000 && (((Blink.Machine*)machine)->fpu.sw&0x241)==0x241,"R116 masked stack overflow indefinite/SF/IE/C1");
        X87Seed(machine);((Blink.Machine*)machine)->fpu.cw&=~1u;ulong old=X87StBits(machine,7);X87Run(machine,X87Classify([0xd9,0xe8]));
        Check(X87Top(machine)==0&&X87StBits(machine,7)==old&&(((Blink.Machine*)machine)->fpu.sw&0x80c1)==0x80c1,"R116 newly unmasked stack overflow deferred with no push");
        foreach(ulong bits in new ulong[]{0,0x8000000000000000,1,0x3ff0000000000000,0x7ff0000000000000,0xfff0000000000000,0x7ff8000000000001})
        {
            X87Seed(machine);X87Set(machine,0,bits);X87Run(machine,X87Classify([0xd9,0xe0]));Check(X87StBits(machine,0)==(bits^0x8000000000000000),"R116 FCHS preserves payload");
            X87Seed(machine);X87Set(machine,0,bits);X87Run(machine,X87Classify([0xd9,0xe1]));Check(X87StBits(machine,0)==(bits&0x7fffffffffffffff),"R116 FABS preserves payload");
            X87Seed(machine);X87Set(machine,0,bits);X87Run(machine,X87Classify([0xd9,0xe5]));
            uint cc=bits<<1==0?0x4000u:(bits&0x7fffffffffffffff)>0x7ff0000000000000?0x100u:(bits&0x7fffffffffffffff)==0x7ff0000000000000?0x500u:
                (bits&0x7ff0000000000000)==0?0x4400u:0x400u;
            Check((((Blink.Machine*)machine)->fpu.sw&0x4700)==(cc|(uint)(bits>>63<<9)),"R116 FXAM f64 classes/sign");
        }
    }

    static void CheckFullX87Math(byte* machine)
    {
        foreach(byte opcode in new byte[]{0xd8,0xdc,0xde})foreach(int group in new[]{0,1,4,5,6,7})for(int r=0;r<8;++r)
        {
            X87Seed(machine,3);X87Set(machine,0,0x4028000000000000);if(r!=0)X87Set(machine,r,0x4008000000000000);
            double a=12,b=r==0?12:3;
            double expected=group==0?a+b:group==1?a*b:opcode==0xd8?(group==4?a-b:group==5?b-a:group==6?a/b:b/a):
                group==4?a-b:group==5?b-a:group==6?a/b:b/a;
            int physical=(3+(opcode==0xd8?0:r))&7;
            X87Run(machine,X87Classify([opcode,(byte)(0xc0|group<<3|r)]));
            Check(((Blink.Machine*)machine)->fpu.st[physical]==expected && X87Top(machine)==(opcode==0xde?4:3),"R116 all arithmetic directions/pops/logical aliases independent equations");
        }
        foreach(int rc in new[]{0,1,2,3})
        {
            X87Seed(machine);((Blink.Machine*)machine)->fpu.cw=(uint)(0x37f|rc<<10);
            X87Set(machine,0,0x3ff0000000000000);X87Set(machine,1,0x3ca0000000000000);X87Run(machine,X87Classify([0xd8,0xc1]));
            Check(X87StBits(machine,0)==(rc==2?0x3ff0000000000001UL:0x3ff0000000000000UL)&&(((Blink.Machine*)machine)->fpu.sw&32)!=0,"R116 guest RC and precision sticky for halfway add");
            foreach(double x in new[]{-2.5,-1.5,-0.1,-0.0,0.0,0.1,1.5,2.5,4503599627370496.0})
            {
                X87Seed(machine);((Blink.Machine*)machine)->fpu.cw=(uint)(0x37f|rc<<10);X87Set(machine,0,(ulong)BitConverter.DoubleToInt64Bits(x));
                X87Run(machine,X87Classify([0xd9,0xfc]));double expected=rc==0?Math.Round(x,MidpointRounding.ToEven):rc==1?Math.Floor(x):rc==2?Math.Ceiling(x):Math.Truncate(x);
                if(expected==0)expected=Math.CopySign(0,x);
                Check(X87StBits(machine,0)==(ulong)BitConverter.DoubleToInt64Bits(expected),"R116 FRNDINT ties/directed/signed zero/large integral");
            }
        }
        foreach((double a,double b) in new[]{(7.0,2.0),(5.0,2.0),(-7.0,2.0),(7.0,-2.0),(1.5,1.0),(0.75,1.0),(0.5,1.0),(0.0,2.0)})
        foreach(bool nearest in new[]{false,true})
        {
            X87Seed(machine);X87Set(machine,0,(ulong)BitConverter.DoubleToInt64Bits(a));X87Set(machine,1,(ulong)BitConverter.DoubleToInt64Bits(b));
            X87Run(machine,X87Classify([0xd9,nearest?(byte)0xf5:(byte)0xf8]));
            double expected=nearest?Math.IEEERemainder(a,b):a%b;long q=(long)(nearest?Math.Round(a/b,MidpointRounding.ToEven):Math.Truncate(a/b));
            uint cc=(uint)((q&1)<<9|(q&2)<<13|(q&4)<<6);
            Check(X87StBits(machine,0)==(ulong)BitConverter.DoubleToInt64Bits(expected)&&(((Blink.Machine*)machine)->fpu.sw&0x4700)==cc,"R116 exact remainder/nearest ties/quotient low3/sign");
        }
        X87Seed(machine);X87Set(machine,0,0x7e30000000000000);X87Set(machine,1,0x3ff0000000000000);X87Run(machine,X87Classify([0xd9,0xf8]));
        Check((((Blink.Machine*)machine)->fpu.sw&0x400)!=0,"R116 partial remainder sets C2 when exponent distance>=64");
        foreach(double x in new[]{-8.0,-0.0,0.5,1.0,1.5,double.Epsilon})foreach(int power in new[]{-1075,-4,0,4,1024})
        {
            X87Seed(machine);X87Set(machine,0,(ulong)BitConverter.DoubleToInt64Bits(x));X87Set(machine,1,(ulong)BitConverter.DoubleToInt64Bits((double)power));
            X87Run(machine,X87Classify([0xd9,0xfd]));Check(X87StBits(machine,0)==(ulong)BitConverter.DoubleToInt64Bits(Math.ScaleB(x,power)),"R116 FSCALE exact exponent and one f64 narrowing");
        }
        foreach(double x in new[]{-8.0,0.5,1.5,double.Epsilon})
        {
            X87Seed(machine);X87Set(machine,0,(ulong)BitConverter.DoubleToInt64Bits(x));X87Set(machine,7,0,3);X87Run(machine,X87Classify([0xd9,0xf4]));
            int e=Math.ILogB(x);Check(((Blink.Machine*)machine)->fpu.st[(X87Top(machine)+1)&7]==e&&
                ((Blink.Machine*)machine)->fpu.st[X87Top(machine)]==Math.ScaleB(x,-e),"R116 FXTRACT exponent/normalized signed significand including f64 subnormal");
        }
        foreach(byte modrm in new byte[]{0xe1,0xf1})
        {
            X87Seed(machine);X87Set(machine,0,0x7ff0000000000000);X87Set(machine,1,modrm==0xe1?0x3ff0000000000000:0x8000000000000000);
            X87Run(machine,X87Classify([0xd8,modrm]));
            Check(X87StBits(machine,0)==(modrm==0xe1?0x7ff0000000000000UL:0xfff0000000000000UL),"R116 source infinity-subtraction/signed-zero-divide defects intentionally corrected");
        }
    }

    static void CheckFullX87Compare(byte* machine)
    {
        foreach ((ulong a, ulong b, uint cc) in new (ulong, ulong, uint)[] {
            (0x3ff0000000000000,0x4000000000000000,0x100),
            (0x4000000000000000,0x3ff0000000000000,0),
            (0x3ff0000000000000,0x3ff0000000000000,0x4000),
            (0x7ff8000000000001,0x3ff0000000000000,0x4500) })
        foreach (bool unordered in new[] { false, true })
        {
            X87Seed(machine); X87Set(machine,0,a); X87Set(machine,1,b);
            X87Run(machine,X87Classify(unordered ? [0xdd,0xe1] : [0xd8,0xd1]));
            uint sw=((Blink.Machine*)machine)->fpu.sw;
            Check((sw&0x4700)==cc && ((sw&1)!=0)==(!unordered && cc==0x4500),
                "R117 ordered/unordered compare qNaN invalid and condition codes");
        }
        // In this backing profile a binary64 subnormal already held in ST is
        // a normal binary80 value; it must not inherit SSE's operand DE.
        X87Seed(machine); X87Set(machine,0,1); X87Set(machine,1,0x3ff0000000000000);
        X87Run(machine,X87Classify([0xd8,0xc1]));
        Check((((Blink.Machine*)machine)->fpu.sw&2)==0,"R117 register backing subnormal does not invent x87 DE");
    }

    static void CheckFullX87Memory(byte* machine)
    {
        const ulong guest=0x200000;
        byte* allocation=(byte*)NativeMemory.Alloc(12288), first=allocation, second=allocation+8192;
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach (bool single in new[] { false, true })
            foreach (int offset in new[] { 32,4095 })
            {
                new Span<byte>(allocation,12288).Clear(); new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,guest,first); SetFullMemoryEntry(tlb,guest+4096,second);
                (offset<4096?first+offset:second+offset-4096)[0]=1;
                X87Seed(machine); X87Set(machine,7,0,3); Write64(machine,48,guest+(uint)offset);
                X87Run(machine,X87Classify(single ? [0xd9,0x03] : [0xdd,0x03]),tlb);
                Check((((Blink.Machine*)machine)->fpu.sw&2)!=0 && X87Top(machine)==7,
                    "R117 m32/m64 denormal source records DE before widening across either mapping");
            }
            new Span<byte>(allocation,12288).Clear(); new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            SetFullMemoryEntry(tlb,guest,first); SetFullMemoryEntry(tlb,guest+4096,second);
            first[64]=1; X87Seed(machine); X87Set(machine,7,0,3); Write64(machine,48,guest+64);
            ((Blink.Machine*)machine)->fpu.cw &= ~2u;
            X87Run(machine,X87Classify([0xdd,0x03]),tlb);
            Check(X87Top(machine)==0 && (((Blink.Machine*)machine)->fpu.sw&0x8082)==0x8082,
                "R117 newly unmasked m64 DE stages exception and suppresses load");
            new Span<byte>(first+64,10).Clear(); first[64]=1;
            X87Seed(machine); X87Set(machine,7,0,3); Write64(machine,48,guest+64);
            X87Run(machine,X87Classify([0xdb,0x2b]),tlb);
            Check((((Blink.Machine*)machine)->fpu.sw&2)!=0,"R117 genuine binary80 memory denormal records DE before narrowing");
            foreach(int rc in new[] { 0,1,2,3 }) foreach(int width in new[] { 16,32,64 })
            {
                new Span<byte>(first+64,8).Fill(0xa5); X87Seed(machine); Write64(machine,48,guest+64);
                var m=(Blink.Machine*)machine; m->fpu.cw=(uint)(0x37f|rc<<10); X87Set(machine,0,0xbff8000000000000);
                byte[] encoding=width==16 ? [0xdf,0x13] : width==32 ? [0xdb,0x13] : [0xdf,0x3b];
                X87Run(machine,X87Classify(encoding),tlb);
                long expected=rc is 0 or 1 ? -2 : -1;
                long actual=width==16 ? BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(first+64,2)) :
                    width==32 ? BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(first+64,4)) :
                    BinaryPrimitives.ReadInt64LittleEndian(new ReadOnlySpan<byte>(first+64,8));
                Check(actual==expected && X87Top(machine)==(width==64?1:0),"R117 FIST widths, signed directed rounding and pop");
            }
            X87Seed(machine); X87Set(machine,0,0x3ff8000000000000); Write64(machine,48,guest+4095);
            X87Run(machine,X87Classify([0xdb,0x3b]),tlb);
            byte[] encoded=[0,0,0,0,0,0,0,0xc0,0xff,0x3f];
            Check(first[4095]==encoded[0] && new ReadOnlySpan<byte>(second,9).SequenceEqual(encoded.AsSpan(1)) &&
                X87Top(machine)==1,"R117 FSTP80 explicit significand and exponent across pages");
            X87Seed(machine); X87Set(machine,7,0,3); Write64(machine,48,guest+4095);
            X87Run(machine,X87Classify([0xdb,0x2b]),tlb);
            Check(X87StBits(machine,0)==0x3ff8000000000000 && X87Top(machine)==7,
                "R117 FLD80 exact representable binary64 across pages");
            foreach(bool narrow in new[] { false,true })
            {
                new Span<byte>(allocation,12288).Clear(); X87Seed(machine,3); Write64(machine,48,guest+4095);
                var m=(Blink.Machine*)machine; m->fpu.cw=0x27f; m->fpu.tw=0x1b1b; m->fpu.op=0x321;
                m->fpu.ip=0x1234; m->fpu.dp=0x5678;
                X87Run(machine,X87Classify(narrow ? [0x66,0xd9,0x33] : [0xd9,0x33]),tlb);
                byte[] env=new byte[narrow?14:28]; env[0]=first[4095]; new ReadOnlySpan<byte>(second,env.Length-1).CopyTo(env.AsSpan(1));
                Check(BinaryPrimitives.ReadUInt16LittleEndian(env)==0x27f &&
                    BinaryPrimitives.ReadUInt16LittleEndian(env.AsSpan(narrow?2:4))==3<<11 &&
                    BinaryPrimitives.ReadUInt16LittleEndian(env.AsSpan(narrow?4:8))==0x1b1b && (m->fpu.cw&63)==63,
                    "R117 FNSTENV width/layout and post-store exception masks");
                m->ip=Ip; X87Run(machine,X87Classify(narrow ? [0x66,0xd9,0x23] : [0xd9,0x23]),tlb);
                Check(m->fpu.cw==0x27f && (m->fpu.sw&0xffff)==3<<11 && (m->fpu.tw&0xffff)==0x1b1b &&
                    m->fpu.ip==0x1234 && m->fpu.dp==0x5678 && (narrow || m->fpu.op==0x321),
                    "R117 FLDENV restores literal environment including 16-bit format");
            }
            // A missing second page must precede FOP/IP/DP, TOP and every store.
            foreach (byte[] encoding in new byte[][] { [0xdb,0x2b], [0xdb,0x3b], [0xdd,0x33], [0x66,0xdd,0x33] })
            {
                new Span<byte>(allocation,12288).Fill(0x5a); new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,guest,first); X87Seed(machine); Write64(machine,48,guest+4095);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(X87Classify(encoding))]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
                    f.NativeMemoryComputations==0 && new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before) &&
                    new ReadOnlySpan<byte>(allocation,12288).IndexOfAnyExcept((byte)0x5a)<0,
                    "R117 wide full-span precheck precedes all state and bytes");
            }
        }
        finally { NativeMemory.Free(allocation); }
    }

    static void CheckFullX87Pending(byte* machine)
    {
        foreach(byte[] encoding in new byte[][] { [0x9b], [0xd8,0xc1], [0xdb,0x2b] })
        {
            X87Seed(machine); var m=(Blink.Machine*)machine; m->fpu.sw=4; m->fpu.cw=0x37b;
            Write64(machine,48,0x800000000000); byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            using var code=CreateFullBlock([new(X87Classify(encoding))]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=(FullMemoryTlbEntry*)1; f.MemoryTlbMask=63;
            Check(FloatRunProbe(code,&f)==FullBlockExit.GuestFault && f.FaultTrap==16 && f.FaultCode==0 && f.FaultAddress==Ip &&
                issued==1 && completed==0 && f.Executed==0 && new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "R117 pending #MF precedes canonicality, MMU and x87 effects");
        }
    }

    static void CheckFullX87Oracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        foreach(byte[] encoding in new byte[][] { [0xd8,0xc1], [0xd8,0xc9], [0xd8,0xe1], [0xd8,0xf1] })
        {
            byte* machine=(byte*)fixture.Machine(); X87Seed(machine); X87Set(machine,0,0x4028000000000000); X87Set(machine,1,0x4008000000000000);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),encoding));
            ulong expected=X87StBits(machine,0); int top=X87Top(machine);
            before.CopyTo(new Span<byte>(machine,before.Length)); X87Run(machine,X87Classify(encoding));
            Check(X87StBits(machine,0)==expected && X87Top(machine)==top,
                "R117 selected GetOp comparison restricted to matching exact finite arithmetic");
        }
    }

    static void CheckFullX87Controller()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture(); const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc); fixture.Fill(pc); WriteControllerCode(fixture,pc,[0x9b]);
        var m=fixture.Machine(); m->fpu.sw=4; m->fpu.cw=0x37b; fixture.Counts(100);
        var fault=fixture.StepSynchronousFault(pc);
        Check(fault.Signal==8 && fault.Detail?.EndsWith("signal=8 code=3")==true && m->trapno==16 && m->ip==pc &&
            fixture.Issued==1 && fixture.Completed==0 && fixture.Statistics.SemanticHelperInstructions==0 &&
            fixture.Statistics.InterpreterAttempts==0 && fixture.Statistics.ActiveDataBorrows==0 &&
            fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0 && fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
            "R117 controller delivers #MF after native/store/data ownership unwinds");
    }

    static void CheckFullX87Emission()
    {
        byte[] buffer=new byte[FullBlockEmitter.CodeCapacity], slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[] { false,true }) foreach(bool shared in new[] { false,true })
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows); var addresses=layout.Resolve((nint)0x10000000);
            foreach(byte[] bytes in new byte[][] { [0xd8,0xc1], [0xdb,0x2b], [0x66,0xdd,0x33], [0xd9,0xf8], [0x9b] })
            {
                FullBlockInstruction[] instructions=[new(X87Classify(bytes))];
                for(int warm=0;warm<8;++warm)
                    Check(shared ? FullBlockEmitter.TryEmit(instructions,buffer,in addresses,out _,windows) :
                        FullBlockEmitter.TryEmit(instructions,buffer,out _,windows),"R117 warm x87 emission");
                long before=GC.GetAllocatedBytesForCurrentThread(); bool good=true;
                for(int n=0;n<16;++n) good &= shared ? FullBlockEmitter.TryEmit(instructions,buffer,in addresses,out _,windows) :
                    FullBlockEmitter.TryEmit(instructions,buffer,out _,windows);
                Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R117 both ABI/scaffold warmed span emission");
            }
        }
    }
}
