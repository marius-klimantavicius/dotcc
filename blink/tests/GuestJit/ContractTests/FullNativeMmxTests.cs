using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using M = Managed.Emulation.Execution.FullBlockEmitter.FullMmxOperation;

unsafe partial class Program
{
    // R113 ALL AUTHORED_UNRUN. Only the parent builds/emits/executes these.
    // Every real GetOp oracle is inside its fixture's DataScope. Never clear
    // or ResetFull an owned fixture Machine: seed only guest fields instead.
    static void CheckFullNativeMmx(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedMmxLayout() && sizeof(FullBlockFrame) == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007 && sizeof(TranslationBlockHeader) == 160, "R113 unchanged ABI6/frame400/header160 and actual fpu.tw layout");
        CheckFullMmxClassifier();
        Check(!fullOwnerScaffold.IsValid, "R113 standalone scaffold initially clear");
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
            const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
            CheckFullMmxOracle(fixture); CheckFullMmxIndependent(machine); CheckFullMmxMemory(machine); CheckFullMmxSs(machine); CheckFullMmxWaiting(machine); CheckFullMmxIssues(machine);
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try { CheckFullMmxOracle(fixture); CheckFullMmxIndependent(machine); CheckFullMmxMemory(machine); CheckFullMmxSs(machine); CheckFullMmxWaiting(machine); CheckFullMmxIssues(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckFullMmxMmu(); CheckFullMmxController(); CheckFullMmxEmission();
    }

    sealed record MmxForm(string Name, byte Map, byte Opcode, M Operation, int Width = 64,
        byte Prefix = 0, bool Immediate = false, bool MemoryOnly = false, bool RegisterOnly = false, bool Wide = false, byte Group = 0);
    static MmxForm[] MmxForms() => [
        new("movq-load",0,0x6f,M.Move), new("movq-store",0,0x7f,M.Move),
        new("movd-to-mm",0,0x6e,M.ScalarToMmx,32), new("movq-gpr-to-mm",0,0x6e,M.ScalarToMmx,Wide:true),
        new("movd-from-mm",0,0x7e,M.MmxToScalar,32), new("movq-mm-to-gpr",0,0x7e,M.MmxToScalar,Wide:true),
        new("movq2dq",0,0xd6,M.MmxToXmm,Prefix:0xf3,RegisterOnly:true),
        new("movdq2q",0,0xd6,M.XmmToMmx,Prefix:0xf2,RegisterOnly:true),
        new("movntq",0,0xe7,M.NonTemporalStore,MemoryOnly:true),
        new("pmovmskb",0,0xd7,M.MaskByte,RegisterOnly:true),
        new("pinsrw",0,0xc4,M.InsertWord,16,Immediate:true), new("pextrw",0,0xc5,M.ExtractWord,16,Immediate:true,RegisterOnly:true),
        new("pshufw",0,0x70,M.ShuffleWord,Immediate:true), new("emms",0,0x77,M.Emms,0,RegisterOnly:true),
        new("pand",0,0xdb,M.And), new("pandn",0,0xdf,M.AndNot), new("por",0,0xeb,M.Or), new("pxor",0,0xef,M.Xor),
        new("paddb",0,0xfc,M.AddByte), new("paddw",0,0xfd,M.AddWord), new("paddd",0,0xfe,M.AddDword), new("paddq",0,0xd4,M.AddQword),
        new("psubb",0,0xf8,M.SubByte), new("psubw",0,0xf9,M.SubWord), new("psubd",0,0xfa,M.SubDword), new("psubq",0,0xfb,M.SubQword),
        new("paddsb",0,0xec,M.AddSignedByte), new("paddsw",0,0xed,M.AddSignedWord),
        new("paddusb",0,0xdc,M.AddUnsignedByte), new("paddusw",0,0xdd,M.AddUnsignedWord),
        new("psubsb",0,0xe8,M.SubSignedByte), new("psubsw",0,0xe9,M.SubSignedWord),
        new("psubusb",0,0xd8,M.SubUnsignedByte), new("psubusw",0,0xd9,M.SubUnsignedWord),
        new("pcmpeqb",0,0x74,M.EqualByte), new("pcmpeqw",0,0x75,M.EqualWord), new("pcmpeqd",0,0x76,M.EqualDword),
        new("pcmpgtb",0,0x64,M.GreaterByte), new("pcmpgtw",0,0x65,M.GreaterWord), new("pcmpgtd",0,0x66,M.GreaterDword),
        new("pminub",0,0xda,M.MinUnsignedByte), new("pmaxub",0,0xde,M.MaxUnsignedByte),
        new("pminsw",0,0xea,M.MinSignedWord), new("pmaxsw",0,0xee,M.MaxSignedWord),
        new("packsswb",0,0x63,M.PackSignedWord), new("packssdw",0,0x6b,M.PackSignedDword), new("packuswb",0,0x67,M.PackUnsignedWord),
        new("punpcklbw",0,0x60,M.UnpackLowByte), new("punpcklwd",0,0x61,M.UnpackLowWord), new("punpckldq",0,0x62,M.UnpackLowDword),
        new("punpckhbw",0,0x68,M.UnpackHighByte), new("punpckhwd",0,0x69,M.UnpackHighWord), new("punpckhdq",0,0x6a,M.UnpackHighDword),
        new("pmullw",0,0xd5,M.MultiplyLowWord), new("pmulhw",0,0xe5,M.MultiplyHighSignedWord), new("pmulhuw",0,0xe4,M.MultiplyHighUnsignedWord),
        new("pmuludq",0,0xf4,M.MultiplyUnsignedDword), new("pmaddwd",0,0xf5,M.MultiplyAddWord), new("psadbw",0,0xf6,M.SumAbsoluteByte),
        new("pavgb",0,0xe0,M.AverageByte), new("pavgw",0,0xe3,M.AverageWord),
        new("psrlw",0,0xd1,M.ShiftRightWord), new("psrld",0,0xd2,M.ShiftRightDword), new("psrlq",0,0xd3,M.ShiftRightQword),
        new("psraw",0,0xe1,M.ShiftArithmeticWord), new("psrad",0,0xe2,M.ShiftArithmeticDword),
        new("psllw",0,0xf1,M.ShiftLeftWord), new("pslld",0,0xf2,M.ShiftLeftDword), new("psllq",0,0xf3,M.ShiftLeftQword),
        new("psrlw-imm",0,0x71,M.ShiftRightWord,RegisterOnly:true,Group:2),
        new("psraw-imm",0,0x71,M.ShiftArithmeticWord,RegisterOnly:true,Group:4),
        new("psllw-imm",0,0x71,M.ShiftLeftWord,RegisterOnly:true,Group:6),
        new("psrld-imm",0,0x72,M.ShiftRightDword,RegisterOnly:true,Group:2),
        new("psrad-imm",0,0x72,M.ShiftArithmeticDword,RegisterOnly:true,Group:4),
        new("pslld-imm",0,0x72,M.ShiftLeftDword,RegisterOnly:true,Group:6),
        new("psrlq-imm",0,0x73,M.ShiftRightQword,RegisterOnly:true,Group:2),
        new("psllq-imm",0,0x73,M.ShiftLeftQword,RegisterOnly:true,Group:6),
        new("pshufb",0x38,0x00,M.ShuffleByte), new("phaddw",0x38,0x01,M.HorizontalAddWord), new("phaddd",0x38,0x02,M.HorizontalAddDword),
        new("phaddsw",0x38,0x03,M.HorizontalAddSignedWord), new("pmaddubsw",0x38,0x04,M.MultiplyAddUnsignedSignedByte),
        new("phsubw",0x38,0x05,M.HorizontalSubWord), new("phsubd",0x38,0x06,M.HorizontalSubDword), new("phsubsw",0x38,0x07,M.HorizontalSubSignedWord),
        new("psignb",0x38,0x08,M.SignByte), new("psignw",0x38,0x09,M.SignWord), new("psignd",0x38,0x0a,M.SignDword),
        new("pmulhrsw",0x38,0x0b,M.MultiplyRoundedSignedWord), new("pabsb",0x38,0x1c,M.AbsoluteByte),
        new("pabsw",0x38,0x1d,M.AbsoluteWord), new("pabsd",0x38,0x1e,M.AbsoluteDword),
        new("palignr",0x3a,0x0f,M.AlignRightBytes,Immediate:true)
    ];

    static byte[] MmxEncoding(MmxForm form, bool memory = false, bool high = false, bool alias = false, byte control = 0, byte? modrmOverride = null)
    {
        var bytes = new List<byte>(8);
        if (form.Prefix != 0) bytes.Add(form.Prefix);
        int rex = (form.Wide ? 8 : 0) | (high ? 5 : 0);
        if (rex != 0) bytes.Add((byte)(0x40 | rex));
        bytes.Add(0x0f); if (form.Map != 0) bytes.Add(form.Map); bytes.Add(form.Opcode);
        if (form.Operation != M.Emms)
        {
            bytes.Add(modrmOverride ?? (form.Group != 0 ? (byte)(0xc0 | form.Group << 3 | (alias ? 0 : 1)) :
                memory ? (byte)0x03 : alias ? (byte)0xc0 : (byte)0xc1));
            if (form.Immediate || form.Group != 0) bytes.Add(control);
        }
        return bytes.ToArray();
    }
    static JitInstruction MmxClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullMmxLowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R113 classify");
        Check(FullBlockEmitter.CanInlineMmx(i) && FullBlockEmitter.ValidateMmx(i), "R113 typed MMX validator before broad Vector");
        return i;
    }
    static void CheckFullMmxClassifier()
    {
        foreach (var form in MmxForms()) foreach (bool memory in new[] { false, true }) foreach (bool high in new[] { false, true })
        {
            if (memory ? form.RegisterOnly : form.MemoryOnly) continue;
            var i = MmxClassify(MmxEncoding(form, memory, high, control:255));
            Check(FullBlockEmitter.MmxOperation(i) == form.Operation && i.Width == 64 && i.SourceWidth == form.Width,
                "R113 exact selected architectural form/width");
            if (form.Operation == M.Emms) continue;
            bool reverse = form.Opcode is 0x7e or 0x7f or 0xe7;
            Check(i.Destination.IsMemory == (memory && reverse) && i.Source.IsMemory == (memory && !reverse), "R113 load/store direction");
            if (!memory && form.Group == 0)
            {
                int destination = reverse ? 1 : 0, source = reverse ? 0 : 1;
                if (high && form.Operation is M.MaskByte or M.ExtractWord or M.MmxToXmm) destination += 8;
                if (high && form.Operation == M.MmxToScalar) destination += 8;
                if (high && form.Operation is M.ScalarToMmx or M.InsertWord or M.XmmToMmx) source += 8;
                Check(i.Destination.Register == destination && i.Source.Register == source, "R113 MMX REX ignored, GPR/XMM REX effective");
            }
            if (memory) Check(i.Address.Base == (high ? 11 : 3), "R113 MMX ignores REX.R while memory base retains REX.B");
        }
        Check(MmxClassify([0x65,0x67,0x4f,0x0f,0xfc,0x54,0x8b,0xf8]).Address == new JitAddress(11,9,2,-8,false,32,5), "R113 GS/AS32/REX.BX/SIB");
        Check(MmxClassify([0x45,0x67,0x64,0x0f,0xfc,0x54,0x8b,0xf8]).Address == new JitAddress(3,1,2,-8,false,32,4), "R113 legacy prefix cancels prior REX");
        Check(MmxClassify([0x67,0x41,0x0f,0x6f,0x05,1,0,0,0]).Address == new JitAddress(-1,-1,0,1,true,32), "R113 EIP-relative ignored B");
        Check(MmxClassify([0x41,0x0f,0x6f,0x04,0x25,0xf0,0xff,0xff,0xff]).Address == new JitAddress(-1,-1,0,-16), "R113 no-base SIB ignores B");
        Check(MmxClassify([0x64,0x65,0x67,0x67,0x40,0x4f,0x0f,0xfc,0xc1]).Destination.Register == 0, "R113 repeated groups and ignored W/R/X/B");
        Check(MmxClassify([0x66,0x66,0x67,0x4f,0x0f,0x77]).SourceWidth == 0, "R113 EMMS ignored operand/address/REX prefixes");
        foreach (var form in MmxForms())
        {
            var bytes = MmxEncoding(form, form.MemoryOnly);
            for (int n = 0; n < bytes.Length; ++n)
                Check(!FullMmxLowering.TryClassify(Ip,bytes.AsSpan(0,n),out _,out var why) && why != null,"R113 truncation rejects");
            Check(!FullMmxLowering.TryClassify(Ip,bytes.Concat(new byte[]{0x90}).ToArray(),out _,out _),"R113 exact span rejects trailing bytes");
        }
        foreach (byte[] bytes in new byte[][] {
            [0x0f,0x2a,0xc1],[0x66,0x0f,0x2a,0xc1],[0x0f,0x2c,0xc1],[0x66,0x0f,0x2d,0x03],
            [0x0f,0xf7,0xc1],[0x45,0x0f,0xf7,0xc1],[0x67,0x0f,0xf7,0xc1],
            [0x0f,0x6c,0xc1],[0x0f,0x6d,0xc1],[0x0f,0x73,0xd8,1],[0x0f,0x73,0xf8,1],
            [0x0f,0x38,0x40,0xc1],[0x0f,0x3a,0x44,0xc1,0],[0xf0,0x0f,0xfc,0xc1],
            [0xf2,0x0f,0xfc,0xc1],[0x66,0xf3,0x0f,0xd6,0xc1],[0xf3,0x0f,0xd6,0x03],
            [0x0f,0xe7,0xc1],[0x0f,0xc5,0x03,0],[0x0f,0x71,0x13,0],
            [0xc5,0xf9,0xfc,0xc1],[0x62,0xf1,0x7d,0x08,0xfc,0xc1] })
            Check(!FullMmxLowering.TryClassify(Ip,bytes,out _,out var why) && why != null,"R113 explicit FP/masked/SS/selected-only/undefined domain MISSING or UNKNOWN");
        var good = MmxClassify([0x0f,0xfc,0xc1]);
        foreach (var bad in new[] { good with { Width=128 },good with { SourceWidth=128 },good with { Options=0x3000 },
            good with { Options=0x3fff },good with { Options=(uint)M.MaskMove },good with { Options=good.Options|0x10000 },
            good with { Target=1 },good with { Condition=1 },good with { Length=0 },good with { Destination=new(8,0) },
            good with { Source=new(8,0) },good with { Extra=new(0,1,true) },good with { Source=new(0,1) },
            good with { Address=new(3,-1,0,0) },good with { Destination=new(0,0,IsMemory:true) },
            MmxClassify([0x0f,0x70,0xc1,0]) with { Extra=default },
            MmxClassify([0x0f,0x77]) with { SourceWidth=64 } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateMmx(bad); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R113 malformed MMX IR fails before emission");
        }
        Check(!FullBlockEmitter.CanInlineMmx(VectorClassify([0x66,0x0f,0xfc,0xc1])),"R113 namespace disjoint from XMM");
        foreach(byte[] bytes in new byte[][]{
            [0x36,0x0f,0x6f,0x03],[0x0f,0x6f,0x04,0x24],[0x0f,0x6f,0x45,0],
            [0x41,0x0f,0x6f,0x04,0x24],[0x41,0x0f,0x6f,0x45,0]})
            Check(FullMmxLowering.UsesStackSegment(MmxClassify(bytes).Address),"R113 new explicit/default SS MMX domain admitted on parent c22 canonical helper");
        foreach(byte[] bytes in new byte[][]{[0x3e,0x0f,0x6f,0x04,0x24],[0x3e,0x41,0x0f,0x6f,0x45,0]})
            Check(!FullMmxLowering.UsesStackSegment(MmxClassify(bytes).Address),"R113 DS override replaces base-derived SS");
    }

    static void MmxSeed(byte* machine)
    {
        VectorSeed(machine); var m=(Blink.Machine*)machine;
        m->flags &= ~0x40000u; // AC clear: no privileged alignment-check claim
        m->fpu.tw=0x12345678; m->fpu.sw=0x4000; m->fpu.cw=0x37f; m->fpu.op=0x123;
        m->fpu.ip=0x123456; m->fpu.dp=0x654321;
        for(int r=0;r<8;++r) m->fpu.st[r]=r+0.25;
    }
    static void MmxRestoreGuest(byte* machine,byte[] before)
    {
        foreach((int at,int count) in new[]{(0,9),(12,4),(24,128),(160,256),(456,236)})
            before.AsSpan(at,count).CopyTo(new Span<byte>(machine+at,count));
    }
    static ulong MmxU(ReadOnlySpan<byte> b) => BinaryPrimitives.ReadUInt64LittleEndian(b);
    static long MmxLane(ReadOnlySpan<byte> b,int lane,int width,bool signed=false)
    {
        int at=lane*width/8;
        return width switch {
            8 => signed?(sbyte)b[at]:b[at],
            16 => signed?BinaryPrimitives.ReadInt16LittleEndian(b[at..]):BinaryPrimitives.ReadUInt16LittleEndian(b[at..]),
            32 => signed?(long)BinaryPrimitives.ReadInt32LittleEndian(b[at..]):BinaryPrimitives.ReadUInt32LittleEndian(b[at..]),
            _ => unchecked((long)MmxU(b)) };
    }
    static void MmxPut(Span<byte> b,int lane,int width,long value)
    {
        int at=lane*width/8;
        if(width==8)b[at]=unchecked((byte)value);
        else if(width==16)BinaryPrimitives.WriteUInt16LittleEndian(b[at..],unchecked((ushort)value));
        else if(width==32)BinaryPrimitives.WriteUInt32LittleEndian(b[at..],unchecked((uint)value));
        else BinaryPrimitives.WriteUInt64LittleEndian(b,unchecked((ulong)value));
    }
    // Independent scalar lane equations, with mathematical saturation/floor
    // division. No host SIMD intrinsics or copy of product extraction code.
    static byte[] MmxBits(M op,ReadOnlySpan<byte> a,ReadOnlySpan<byte> b,byte control)
    {
        byte[] result=new byte[8];
        if(op is M.And or M.AndNot or M.Or or M.Xor)
        {
            ulong x=MmxU(a),y=MmxU(b);BinaryPrimitives.WriteUInt64LittleEndian(result,op==M.And?x&y:op==M.AndNot?~x&y:op==M.Or?x|y:x^y);
        }
        else if(op is >= M.AddByte and <= M.SubUnsignedWord)
        {
            bool sat=op>=M.AddSignedByte,sign=op is M.AddSignedByte or M.AddSignedWord or M.SubSignedByte or M.SubSignedWord;
            bool sub=op is M.SubByte or M.SubWord or M.SubDword or M.SubQword or M.SubSignedByte or M.SubSignedWord or M.SubUnsignedByte or M.SubUnsignedWord;
            int width=op is M.AddByte or M.SubByte or M.AddSignedByte or M.AddUnsignedByte or M.SubSignedByte or M.SubUnsignedByte?8:
                op is M.AddDword or M.SubDword?32:op is M.AddQword or M.SubQword?64:16;
            for(int lane=0;lane<64/width;++lane)
            {
                long x=MmxLane(a,lane,width,sign),y=MmxLane(b,lane,width,sign),v=unchecked(sub?x-y:x+y);
                if(sat)v=Math.Clamp(v,sign?-(1L<<(width-1)):0,sign?(1L<<(width-1))-1:(1L<<width)-1);
                MmxPut(result,lane,width,v);
            }
        }
        else if(op is >= M.EqualByte and <= M.MaxSignedWord)
        {
            bool compare=op<=M.GreaterDword,sign=op is M.GreaterByte or M.GreaterWord or M.GreaterDword or M.MinSignedWord or M.MaxSignedWord;
            int width=op is M.EqualByte or M.GreaterByte or M.MinUnsignedByte or M.MaxUnsignedByte?8:op is M.EqualDword or M.GreaterDword?32:16;
            for(int lane=0;lane<64/width;++lane)
            {
                long x=MmxLane(a,lane,width,sign),y=MmxLane(b,lane,width,sign);
                long v=compare?(op<=M.EqualDword?(x==y?-1:0):(x>y?-1:0)):
                    op is M.MinUnsignedByte or M.MinSignedWord?Math.Min(x,y):Math.Max(x,y);
                MmxPut(result,lane,width,v);
            }
        }
        else if(op is M.PackSignedWord or M.PackSignedDword or M.PackUnsignedWord)
        {
            int width=op==M.PackSignedDword?32:16,outWidth=width/2,half=64/width;
            for(int lane=0;lane<half*2;++lane)
            {
                long v=MmxLane(lane<half?a:b,lane%half,width,true);
                MmxPut(result,lane,outWidth,Math.Clamp(v,op==M.PackUnsignedWord?0:-(1L<<(outWidth-1)),
                    op==M.PackUnsignedWord?255:(1L<<(outWidth-1))-1));
            }
        }
        else if(op is >= M.UnpackLowByte and <= M.UnpackHighDword)
        {
            int width=op is M.UnpackLowByte or M.UnpackHighByte?8:op is M.UnpackLowWord or M.UnpackHighWord?16:32;
            int half=32/width,start=op>=M.UnpackHighByte?half:0;
            for(int lane=0;lane<half;++lane)
            {MmxPut(result,lane*2,width,MmxLane(a,start+lane,width));MmxPut(result,lane*2+1,width,MmxLane(b,start+lane,width));}
        }
        else if(op is M.MultiplyLowWord or M.MultiplyHighSignedWord or M.MultiplyHighUnsignedWord)
        {
            bool sign=op==M.MultiplyHighSignedWord;
            for(int lane=0;lane<4;++lane)
            {long v=MmxLane(a,lane,16,sign)*MmxLane(b,lane,16,sign);MmxPut(result,lane,16,op==M.MultiplyLowWord?v:v>>16);}
        }
        else if(op==M.MultiplyUnsignedDword)MmxPut(result,0,64,unchecked((long)((ulong)MmxLane(a,0,32)*(ulong)MmxLane(b,0,32))));
        else if(op==M.MultiplyAddWord)
        {
            for(int lane=0;lane<2;++lane)MmxPut(result,lane,32,MmxLane(a,lane*2,16,true)*MmxLane(b,lane*2,16,true)+
                MmxLane(a,lane*2+1,16,true)*MmxLane(b,lane*2+1,16,true));
        }
        else if(op==M.SumAbsoluteByte)
        {long sum=0;for(int lane=0;lane<8;++lane)sum+=Math.Abs(a[lane]-b[lane]);MmxPut(result,0,64,sum);}
        else if(op is M.AverageByte or M.AverageWord)
        {int width=op==M.AverageByte?8:16;for(int lane=0;lane<64/width;++lane)MmxPut(result,lane,width,(MmxLane(a,lane,width)+MmxLane(b,lane,width)+1)/2);}
        else if(op is >= M.ShiftRightWord and <= M.ShiftLeftQword)
        {
            int width=op is M.ShiftRightWord or M.ShiftArithmeticWord or M.ShiftLeftWord?16:op is M.ShiftRightDword or M.ShiftArithmeticDword or M.ShiftLeftDword?32:64;
            bool arithmetic=op is M.ShiftArithmeticWord or M.ShiftArithmeticDword,left=op is M.ShiftLeftWord or M.ShiftLeftDword or M.ShiftLeftQword;
            ulong count=MmxU(b);
            for(int lane=0;lane<64/width;++lane)
            {
                long v=MmxLane(a,lane,width,arithmetic);
                if(arithmetic)v>>=(int)Math.Min(count,(ulong)(width-1));
                else v=count>=(ulong)width?0:left?unchecked(v<<(int)count):unchecked((long)(unchecked((ulong)v)>>(int)count));
                MmxPut(result,lane,width,v);
            }
        }
        else if(op==M.ShuffleWord)
        {for(int lane=0;lane<4;++lane)MmxPut(result,lane,16,MmxLane(b,control>>(lane*2)&3,16));}
        else if(op==M.ShuffleByte)
        {for(int lane=0;lane<8;++lane)result[lane]=(b[lane]&128)!=0?(byte)0:a[b[lane]%8];}
        else if(op is M.HorizontalAddWord or M.HorizontalAddDword or M.HorizontalAddSignedWord or M.HorizontalSubWord or M.HorizontalSubDword or M.HorizontalSubSignedWord)
        {
            bool sub=op is M.HorizontalSubWord or M.HorizontalSubDword or M.HorizontalSubSignedWord,sat=op is M.HorizontalAddSignedWord or M.HorizontalSubSignedWord;
            int width=op is M.HorizontalAddDword or M.HorizontalSubDword?32:16,half=32/width;
            for(int input=0;input<2;++input)for(int pair=0;pair<half;++pair)
            {
                ReadOnlySpan<byte> values=input==0?a:b;long x=MmxLane(values,pair*2,width,sat),y=MmxLane(values,pair*2+1,width,sat),v=sub?x-y:x+y;
                MmxPut(result,input*half+pair,width,sat?Math.Clamp(v,-32768,32767):v);
            }
        }
        else if(op==M.MultiplyAddUnsignedSignedByte)
        {for(int lane=0;lane<4;++lane)MmxPut(result,lane,16,Math.Clamp(a[lane*2]*(sbyte)b[lane*2]+a[lane*2+1]*(sbyte)b[lane*2+1],-32768,32767));}
        else if(op is M.SignByte or M.SignWord or M.SignDword or M.AbsoluteByte or M.AbsoluteWord or M.AbsoluteDword)
        {
            bool abs=op is M.AbsoluteByte or M.AbsoluteWord or M.AbsoluteDword;
            int width=op is M.SignByte or M.AbsoluteByte?8:op is M.SignWord or M.AbsoluteWord?16:32;
            for(int lane=0;lane<64/width;++lane)
            {
                long v=MmxLane(abs?b:a,lane,width,true),sign=MmxLane(b,lane,width,true);
                MmxPut(result,lane,width,abs?Math.Abs(v):sign<0?-v:sign==0?0:v);
            }
        }
        else if(op==M.MultiplyRoundedSignedWord)
        {
            for(int lane=0;lane<4;++lane)
            {
                long dividend=MmxLane(a,lane,16,true)*MmxLane(b,lane,16,true)+16384;
                long v=Math.DivRem(dividend,32768,out long rem);if(rem<0)--v;MmxPut(result,lane,16,v);
            }
        }
        else if(op==M.AlignRightBytes)
        {byte[] concatenation=new byte[24];b[..8].CopyTo(concatenation);a[..8].CopyTo(concatenation.AsSpan(8));concatenation.AsSpan(Math.Min((int)control,16),8).CopyTo(result);}
        else throw new ArgumentException("R113 independent MMX lane model absent");
        return result;
    }

    static byte[] MmxResult(JitInstruction i,byte[] before,ReadOnlySpan<byte> memory)
    {
        var op=FullBlockEmitter.MmxOperation(i);byte[] source=new byte[8];
        if(i.Source.IsImmediate)BinaryPrimitives.WriteUInt64LittleEndian(source,i.Source.Immediate);
        else if(i.Source.IsMemory)memory[..(i.SourceWidth/8)].CopyTo(source);
        else
        {
            int offset=op is M.ScalarToMmx or M.InsertWord?24+i.Source.Register*8:160+i.Source.Register*16;
            before.AsSpan(offset,op==M.InsertWord?2:op==M.ScalarToMmx?i.SourceWidth/8:8).CopyTo(source);
        }
        if(op is M.Move or M.NonTemporalStore or M.ScalarToMmx or M.MmxToScalar or M.MmxToXmm or M.XmmToMmx)return source;
        if(op==M.MaskByte)
        {ulong mask=0;for(int b=0;b<8;++b)mask|=(ulong)(source[b]/128)<<b;BinaryPrimitives.WriteUInt64LittleEndian(source,mask);return source;}
        if(op==M.ExtractWord)
        {ulong value=(ulong)MmxLane(source,(int)i.Extra.Immediate&3,16);BinaryPrimitives.WriteUInt64LittleEndian(source,value);return source;}
        if(op==M.InsertWord)
        {byte[] result=before.AsSpan(160+i.Destination.Register*16,8).ToArray();source.AsSpan(0,2).CopyTo(result.AsSpan(((int)i.Extra.Immediate&3)*2));return result;}
        return MmxBits(op,before.AsSpan(160+i.Destination.Register*16,8),source,(byte)i.Extra.Immediate);
    }
    static byte[] MmxExpected(JitInstruction i,byte[] before,ReadOnlySpan<byte> memory)
    {
        byte[] expected=(byte[])before.Clone();var op=FullBlockEmitter.MmxOperation(i);
        if(op==M.Emms)
        {
            int offset=Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.fpu)).ToInt32()+Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.tw)).ToInt32();
            BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(offset),-1);
        }
        else if(!i.Destination.IsMemory)
        {
            byte[] result=MmxResult(i,before,memory);
            if(op is M.MmxToScalar or M.MaskByte or M.ExtractWord)
            {
                int width=op==M.MmxToScalar?i.SourceWidth:32;
                ulong value=width==64?MmxU(result):BinaryPrimitives.ReadUInt32LittleEndian(result);
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24+i.Destination.Register*8),value);
            }
            else
            {
                result.CopyTo(expected,160+i.Destination.Register*16);
                if(op==M.MmxToXmm)expected.AsSpan(168+i.Destination.Register*16,8).Clear();
            }
        }
        BinaryPrimitives.WriteUInt64LittleEndian(expected,Ip+(uint)i.Length);expected[8]=0;return expected;
    }
    static void MmxRun(byte* machine,JitInstruction i,FullMemoryTlbEntry* tlb=null,ulong fs=0,ulong gs=0,bool hostProbe=false)
    {
        using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=tlb==null?0u:63u;f.FsBase=fs;f.GsBase=gs;
        Check((hostProbe?FloatRunProbe(code,&f):RunFull(code,&f))==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
            f.NativeMemoryComputations==(i.Source.IsMemory||i.Destination.IsMemory?1UL:0UL),"R113 native generated computation, one issue/commit, no FP environment change");
    }

    static void CheckFullMmxOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;byte* machine=(byte*)fixture.Machine(),page=(byte*)fixture.Physical(guest);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach(var form in MmxForms())foreach(bool memory in new[]{false,true})foreach(bool alias in new[]{false,true})
        {
            if(memory?form.RegisterOnly:form.MemoryOnly)continue;
            // These two selected kernels have separately asserted discrepancies.
            if(form.Operation is M.SumAbsoluteByte or M.ExtractWord)continue;
            MmxSeed(machine);if(memory)Write64(machine,48,guest+4088);
            for(int b=0;b<4096;++b)page[b]=(byte)(b*47+0x81);
            byte[] bytes=MmxEncoding(form,memory,alias:alias,control:255);var i=MmxClassify(bytes);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),memoryBefore=new ReadOnlySpan<byte>(page,4096).ToArray();
            byte[] expected=MmxExpected(i,before,new ReadOnlySpan<byte>(page+4088,8));
            byte[] expectedMemory=(byte[])memoryBefore.Clone();
            if(i.Destination.IsMemory)MmxResult(i,before,ReadOnlySpan<byte>.Empty).AsSpan(0,i.SourceWidth/8).CopyTo(expectedMemory.AsSpan(4088));
            fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            VectorCheckArchitecture(machine,expected,$"R113 actual normal GetOp {form.Name}/mem={memory}/alias={alias}");
            Check(new ReadOnlySpan<byte>(page,4096).SequenceEqual(expectedMemory),"R113 oracle exact memory effects");
            MmxRestoreGuest(machine,before);memoryBefore.CopyTo(new Span<byte>(page,4096));
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read|FullMemoryAccess.Write);
            MmxRun(machine,i,memory?tlb:null);
            VectorCheckArchitecture(machine,expected,"R113 oracle agreement outside explicit producer-gap domains");
            Check(new ReadOnlySpan<byte>(page,4096).SequenceEqual(expectedMemory),"R113 exact native memory direction/width");
        }
        // Source-selected portable SAD is two independent 4-byte sums, unlike
        // the architectural sum of eight. Keep this counterexample explicit.
        MmxSeed(machine);new Span<byte>(machine+160,8).Fill(1);new Span<byte>(machine+176,8).Clear();
        fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),new byte[]{0x0f,0xf6,0xc1}));
        Check(MmxU(new ReadOnlySpan<byte>(machine+160,8))==0x400000004UL,"R113 EXPECTED_SELECTED_GAP PSADBW split sums4");
        MmxSeed(machine);Write64(machine,24,ulong.MaxValue);Write64(machine,176,0x1234);
        fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),new byte[]{0x0f,0xc5,0xc1,0}));
        Check(MmxU(new ReadOnlySpan<byte>(machine+24,8))==0xffffffffffff1234UL,"R113 EXPECTED_SELECTED_GAP PEXTRW Put16 retains upper GPR");
        MmxSeed(machine);new Span<byte>(machine+160,8).Fill(1);new Span<byte>(machine+176,8).Fill(2);
        new Span<byte>(machine+288,8).Fill(3);new Span<byte>(machine+304,8).Fill(4);
        fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),new byte[]{0x45,0x0f,0xfc,0xc1}));
        Check(machine[160]==1 && machine[288]==7,"R113 EXPECTED_SELECTED_GAP arithmetic Rexr/Rexb expose XMM8/9 instead of MM0/1");
    }

    static void CheckFullMmxIndependent(byte* machine)
    {
        foreach(var form in MmxForms().Where(x=>!x.MemoryOnly&&x.Operation!=M.Emms))
        foreach(bool high in new[]{false,true})
        foreach(int reg in form.Group==0?Enumerable.Range(0,8):new[]{(int)form.Group})foreach(int rm in Enumerable.Range(0,8))
        {
            MmxSeed(machine);var i=MmxClassify(MmxEncoding(form,high:high,control:0x1b,modrmOverride:(byte)(0xc0|reg<<3|rm)));
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),expected=MmxExpected(i,before,ReadOnlySpan<byte>.Empty);
            MmxRun(machine,i);VectorCheckArchitecture(machine,expected,"R113 every MM0..7, GPR/XMM0..15, distinct and alias binding");
        }
        foreach(var form in MmxForms().Where(x=>!x.MemoryOnly))foreach(bool alias in new[]{false,true})
        for(int sample=0;sample<(form.Immediate||form.Group!=0?256:12);++sample)
        {
            MmxSeed(machine);var i=MmxClassify(MmxEncoding(form,high:true,alias:alias,control:(byte)sample));
            for(int r=0;r<16;++r)for(int b=0;b<16;++b)machine[160+r*16+b]=(byte)(sample*31+r*17+b*23);
            foreach(int r in new[]{i.Destination.Register,i.Source.Register})
            {
                ulong edge=sample switch{0=>0x8000800080008000UL,1=>0x7fff7fff7fff7fffUL,2=>ulong.MaxValue,3=>0UL,_=>0x807f00ff12345678UL^(uint)sample};
                if(r<8)Write64(machine,160+r*16,edge);
            }
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),expected=MmxExpected(i,before,ReadOnlySpan<byte>.Empty);
            MmxRun(machine,i,hostProbe:sample==0);VectorCheckArchitecture(machine,expected,"R113 independent64 lane math/alias/extrema/full imm8/ignored REX/physical high64 preservation");
        }
        foreach(var form in MmxForms().Where(x=>x.Group==0&&x.Operation is >= M.ShiftRightWord and <= M.ShiftLeftQword))
        foreach(bool alias in new[]{false,true})foreach(ulong count in new ulong[]{0,1,15,16,31,32,63,64,255,0x100000000,ulong.MaxValue})
        {
            MmxSeed(machine);var i=MmxClassify(MmxEncoding(form,alias:alias));Write64(machine,160+i.Source.Register*16,count);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();var expected=MmxExpected(i,before,ReadOnlySpan<byte>.Empty);
            MmxRun(machine,i);VectorCheckArchitecture(machine,expected,"R113 variable count uses full64bits including alias and large count");
        }
    }

    static byte[] MmxGather(byte* first,byte* second,int offset,int count)
    {byte[] result=new byte[count];for(int b=0;b<count;++b)result[b]=offset+b<4096?first[offset+b]:second[offset+b-4096];return result;}
    static void CheckFullMmxMemory(byte* machine)
    {
        const ulong guest=0x400000;byte* allocation=(byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            byte* first=allocation,second=allocation+8192;FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var form in MmxForms().Where(x=>!x.RegisterOnly))foreach(int offset in new[]{1,64,4096-form.Width/8,4097-form.Width/8,4095})
            foreach(string mapping in new[]{"ok","missing-first","missing-second","wrong-first","wrong-second"})
            {
                MmxSeed(machine);Write64(machine,48,guest+(uint)offset);
                for(int b=0;b<12288;++b)allocation[b]=(byte)(b*59+0xa5);
                var i=MmxClassify(MmxEncoding(form,true,control:255));bool write=i.Destination.IsMemory,crossing=offset+form.Width/8>4096;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),memoryBefore=new ReadOnlySpan<byte>(allocation,12288).ToArray();
                byte[] source=MmxGather(first,second,offset,form.Width/8),expected=MmxExpected(i,before,source),expectedMemory=(byte[])memoryBefore.Clone();
                if(write)
                {
                    byte[] value=MmxResult(i,before,ReadOnlySpan<byte>.Empty);
                    for(int b=0;b<form.Width/8;++b)expectedMemory[offset+b<4096?offset+b:8192+offset+b-4096]=value[b];
                }
                FullMemoryAccess access=write?FullMemoryAccess.Write:FullMemoryAccess.Read,wrong=write?FullMemoryAccess.Read:FullMemoryAccess.Write;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                if(mapping!="missing-first")SetFullMemoryEntry(tlb,guest,first,mapping=="wrong-first"?wrong:access);
                if(mapping!="missing-second")SetFullMemoryEntry(tlb,guest+4096,second,mapping=="wrong-second"?wrong:access);
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                bool miss=(mapping is "missing-first" or "wrong-first") || crossing&&(mapping is "missing-second" or "wrong-second");
                if(miss)
                {
                    Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.Executed==0&&f.MemoryResume==1&&
                        f.MemoryAddress==guest+(uint)offset&&f.MemoryWidth==form.Width&&f.MemoryAccess==access&&f.NativeMemoryComputations==0&&
                        new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(memoryBefore),
                        "R113 exact mapping/permission and BOTH pages prechecked before source/destination changes");
                    SetFullMemoryEntry(tlb,guest,first,access);SetFullMemoryEntry(tlb,guest+4096,second,access);FullNativeMemory.ResumeSameIssue(&f);
                }
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&f.NativeMemoryComputations==1,
                    "R113 same-reserved-issue resume; exact final-byte access does not require next page");
                VectorCheckArchitecture(machine,expected,"R113 unaligned/page-end/cross-page native exact width");
                Check(new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(expectedMemory),"R113 guarded noncontiguous physical pages and untouched surrounding bytes");
            }
            foreach(var form in MmxForms().Where(x=>!x.RegisterOnly))foreach(byte segment in new byte[]{0x64,0x65})
            {
                MmxSeed(machine);Write64(machine,24+11*8,0x100000010);Write64(machine,24+9*8,0x100000002);
                var encoded=new List<byte>{segment,0x67,(byte)(form.Wide?0x4f:0x47),0x0f};if(form.Map!=0)encoded.Add(form.Map);
                encoded.Add(form.Opcode);encoded.Add(0x44);encoded.Add(0x8b);encoded.Add(0xf8);if(form.Immediate)encoded.Add(255);
                var i=MmxClassify(encoded.ToArray());byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] expected=MmxExpected(i,before,new ReadOnlySpan<byte>(first+16,form.Width/8));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,first,FullMemoryAccess.Read|FullMemoryAccess.Write);
                MmxRun(machine,i,tlb,segment==0x64?guest:0,segment==0x65?guest:0);VectorCheckArchitecture(machine,expected,"R113 actual FS/GS EA32/SIB and high address registers");
            }
            foreach(var form in MmxForms().Where(x=>!x.RegisterOnly))
            foreach(string shape in new[]{"rip64","rip32","sib64","sib32","sib-rex","sib-no-base","rex-cleared"})foreach(int scale in new[]{0,1,2,3})
            {
                MmxSeed(machine);var bytes=new List<byte>();
                if(shape=="rex-cleared")bytes.Add(0x45);
                if(shape is "rip32" or "sib32")bytes.Add(0x67);
                if(shape=="sib-rex")bytes.Add((byte)(form.Wide?0x4f:0x47));
                else if(form.Wide)bytes.Add(0x48);
                // A legacy segment prefix after REX deliberately cancels it.
                if(shape=="rex-cleared")bytes.Add(0x3e);
                bytes.Add(0x0f);if(form.Map!=0)bytes.Add(form.Map);bytes.Add(form.Opcode);
                if(shape is "rip64" or "rip32")
                {
                    bytes.Add(0x05);int length=bytes.Count+4+(form.Immediate?1:0);
                    byte[] disp=new byte[4];BinaryPrimitives.WriteInt32LittleEndian(disp,checked((int)((long)guest+64-(long)Ip-length)));bytes.AddRange(disp);
                }
                else if(shape=="sib-no-base")
                {bytes.Add(0x04);bytes.Add(0x25);byte[] disp=new byte[4];BinaryPrimitives.WriteUInt32LittleEndian(disp,(uint)guest+64);bytes.AddRange(disp);}
                else if(shape=="rex-cleared") {bytes.Add(0x03);Write64(machine,48,guest+64);}
                else
                {
                    bytes.Add(0x44);bytes.Add((byte)(scale<<6|0x0b));bytes.Add(0xf8);
                    bool rex=shape=="sib-rex",a32=shape=="sib32";
                    Write64(machine,24+(rex?11:3)*8,guest+72-(2UL<<scale)+(a32?0x100000000UL:0));
                    Write64(machine,24+(rex?9:1)*8,2+(a32?0x100000000UL:0));
                }
                if(form.Immediate)bytes.Add(0x1b);var i=MmxClassify(bytes.ToArray());
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),expected=MmxExpected(i,before,new ReadOnlySpan<byte>(first+64,form.Width/8));
                byte[] memoryBefore=new ReadOnlySpan<byte>(allocation,12288).ToArray(),expectedMemory=(byte[])memoryBefore.Clone();
                if(i.Destination.IsMemory)MmxResult(i,before,ReadOnlySpan<byte>.Empty).AsSpan(0,i.SourceWidth/8).CopyTo(expectedMemory.AsSpan(64));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,first,FullMemoryAccess.Read|FullMemoryAccess.Write);
                MmxRun(machine,i,tlb);VectorCheckArchitecture(machine,expected,"R113 actual RIP/EIP/SIB scales/no-base/REX cancellation effective addressing");
                Check(new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(expectedMemory),"R113 alternate address shape exact memory effects");
            }
            foreach(var form in MmxForms().Where(x=>!x.RegisterOnly))
            foreach(ulong address in new ulong[]{0x800000000000,0x00007fffffffffff,ulong.MaxValue})
            {
                MmxSeed(machine);Write64(machine,48,address);byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(MmxClassify(MmxEncoding(form,true,control:255)))]);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&f.FaultAddress==0&&issued==1&&completed==0&&
                    f.MemoryResume==0&&f.NativeMemoryComputations==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    "R113 non-SS noncanonical/start/end/wrap #GP before missing MMU and no partial effects");
            }
        }
        finally{NativeMemory.Free(allocation);}
    }

    static void CheckFullMmxIssues(byte* machine)
    {
        foreach(var form in MmxForms())foreach(string boundary in new[]{"stop","attention","budget","approved-attention"})
        {
            MmxSeed(machine);var i=MmxClassify(MmxEncoding(form,form.MemoryOnly,control:255));
            machine[11]=(byte)(boundary.Contains("attention")?1:0);byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=boundary=="approved-attention"?1:0,completed=0;int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL);
            f.FirstPreissued=boundary=="approved-attention"?1u:0u;var exit=RunFull(code,&f);
            if(boundary=="approved-attention")Check(exit==(form.MemoryOnly?FullBlockExit.GuestFault:FullBlockExit.Continue)&&issued==1&&completed==(form.MemoryOnly?0:1),"R113 approved instruction runs before late attention");
            else Check(exit==(boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:FullBlockExit.Budget)&&
                completed==0&&issued==(boundary=="budget"?1:0)&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R113 stop/poll/budget preserves all guest fields and exact issue count");
        }
    }

    static void CheckFullMmxWaiting(byte* machine)
    {
        foreach(var form in MmxForms())foreach(bool memory in new[]{false,true})
        foreach(uint bit in new uint[]{1,2,4,8,16,32})foreach(bool masked in new[]{false,true})
        {
            if(memory?form.RegisterOnly:form.MemoryOnly)continue;
            MmxSeed(machine);var m=(Blink.Machine*)machine;
            m->fpu.sw=0x4000|bit;m->fpu.cw=masked?0x37fu:0x37fu&~bit;
            if(memory)Write64(machine,48,masked?0x400000UL:0x800000000000UL);
            var i=MmxClassify(MmxEncoding(form,memory,high:false,control:0x1b));
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);var exit=FloatRunProbe(code,&f);
            if(!masked)
                Check(exit==FullBlockExit.GuestFault&&f.FaultTrap==16&&f.FaultCode==0&&f.FaultAddress==Ip&&issued==1&&completed==0&&f.Executed==0&&
                    f.MemoryResume==0&&f.MemoryAddress==0&&f.NativeMemoryComputations==0&&f.HostMxcsrSaved==0&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R113 all waiting MMX including EMMS #MF before tag/data/flags/noncanonical EA/MMU");
            else if(memory)
                Check(exit==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryResume==1&&f.NativeMemoryComputations==0&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R113 masked x87 sticky does not cause #MF before exact memory request");
            else
            {
                Check(exit==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1,"R113 masked x87 sticky permits waiting instruction");
                VectorCheckArchitecture(machine,MmxExpected(i,before,ReadOnlySpan<byte>.Empty),"R113 masked waiting leaves CW/SW and ordinary state precise");
            }
        }
    }

    static byte[] MmxSsEncoding(MmxForm form,string shape)
    {
        var bytes=new List<byte>();bool high=shape.EndsWith("r12")||shape.EndsWith("r13");
        bool stack=shape.EndsWith("rsp")||shape.EndsWith("r12");
        if(shape=="explicit")bytes.Add(0x36);else if(shape.StartsWith("ds-"))bytes.Add(0x3e);
        int rex=(form.Wide?8:0)|(high?1:0);if(rex!=0)bytes.Add((byte)(0x40|rex));
        bytes.Add(0x0f);if(form.Map!=0)bytes.Add(form.Map);bytes.Add(form.Opcode);
        if(shape=="explicit")bytes.Add(0x03);
        else if(stack){bytes.Add(0x04);bytes.Add(0x24);}
        else{bytes.Add(0x45);bytes.Add(0);}
        if(form.Immediate)bytes.Add(0x1b);return bytes.ToArray();
    }
    static void CheckFullMmxSs(byte* machine)
    {
        const ulong guest=0x400000;byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var form in MmxForms().Where(x=>!x.RegisterOnly))
            foreach(string shape in new[]{"explicit","rsp","rbp","r12","r13","ds-rsp","ds-rbp","ds-r12","ds-r13"})
            foreach(ulong address in new ulong[]{guest+64,0x800000000000,0x7fffffffffff,ulong.MaxValue})
            {
                MmxSeed(machine);for(int b=0;b<4096;++b)page[b]=(byte)(b*43+0x19);
                var i=MmxClassify(MmxSsEncoding(form,shape));Write64(machine,24+i.Address.Base*8,address);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),memoryBefore=new ReadOnlySpan<byte>(page,4096).ToArray();
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlbMask=63;
                if(address==guest+64)
                {
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read|FullMemoryAccess.Write);f.MemoryTlb=tlb;
                    byte[] expected=MmxExpected(i,before,new ReadOnlySpan<byte>(page+64,i.SourceWidth/8)),expectedMemory=(byte[])memoryBefore.Clone();
                    if(i.Destination.IsMemory)MmxResult(i,before,ReadOnlySpan<byte>.Empty).AsSpan(0,i.SourceWidth/8).CopyTo(expectedMemory.AsSpan(64));
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.NativeMemoryComputations==1,"R113 explicit/default SS computes flat canonical memory precisely");
                    VectorCheckArchitecture(machine,expected,"R113 SS success all MMX memory families");
                    Check(new ReadOnlySpan<byte>(page,4096).SequenceEqual(expectedMemory),"R113 SS memory direction and exact width");
                }
                else
                {
                    f.MemoryTlb=(FullMemoryTlbEntry*)1; // canonical check MUST precede poisoned MMU
                    Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(FullMmxLowering.UsesStackSegment(i.Address)?12:13)&&f.FaultCode==128&&
                        f.FaultAddress==0&&issued==1&&completed==0&&f.Executed==0&&f.MemoryResume==0&&f.NativeMemoryComputations==0&&
                        new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&new ReadOnlySpan<byte>(page,4096).SequenceEqual(memoryBefore),
                        "R113 SS12 versus DSoverride GP13 start/end/wrap before poisoned MMU, no partial effects");
                    var m=(Blink.Machine*)machine;m->fpu.sw=4;m->fpu.cw=0x37b;
                    byte[] waitingBefore=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();issued=completed=0;
                    f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                    Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==16&&f.FaultCode==0&&f.FaultAddress==Ip&&issued==1&&completed==0&&
                        f.MemoryResume==0&&f.NativeMemoryComputations==0&&new ReadOnlySpan<byte>(machine,waitingBefore.Length).SequenceEqual(waitingBefore),
                        "R113 pending #MF precedes both SS12 and GP13 canonicality with poisoned MMU");
                }
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullMmxMmu()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest);fixture.Fill(guest);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        foreach(int width in new[]{16,32,64})
        {
            var f=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4096-(uint)(width/8),MemoryAccess=FullMemoryAccess.Read,MemoryWidth=(uint)width,MemoryResume=1};
            Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.Resolved,"R113 checked exact page-end width needs no next page");
            f.MemoryAddress=guest+4095;f.MemoryResume=1;
            Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14&&f.FaultAddress==guest+4096,"R113 exact cross-page missing-byte fault");
        }
        fixture.Map(guest+4096);fixture.Fill(guest+4096);
        fixture.DataScope(0,()=>Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,(long)(guest+4096),4096,1,false)==0,"R113 actual readonly page"));
        var write=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4095,MemoryAccess=FullMemoryAccess.Write,MemoryWidth=64,MemoryResume=1};
        Check(FillFullMemoryMmu(fixture,&write)==FullMemoryAssistResult.GuestFault&&write.FaultTrap==14&&write.FaultCode==2&&write.FaultAddress==guest+4096,"R113 real checked writable requirement on both pages");
    }
    static void CheckFullMmxController()
    {
        foreach(byte[] operation in new byte[][]{[0x0f,0xfc,0x03],[0x0f,0x7f,0x03],[0x0f,0x38,0x00,0x03]})
        foreach(bool canonical in new[]{false,true})
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;fixture.Map(pc);fixture.Fill(pc);
            byte[] body=new byte[operation.Length+1];body[0]=0x90;operation.CopyTo(body,1);WriteControllerCode(fixture,pc,body);
            var m=fixture.Machine();Write64((byte*)m,48,canonical?pc+8192+4095:0x800000000000);
            byte[] xmm=new ReadOnlySpan<byte>((byte*)m+160,256).ToArray();uint flags=m->flags,mxcsr=m->mxcsr;int tag=m->fpu.tw;
            fixture.Counts(100);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=fixture.StepSynchronousFault(pc);
            Check(signal.Signal==11&&m->trapno==(canonical?14:13)&&m->faultaddr==(canonical?(long)(pc+8192+4095):0)&&m->ip==pc+1&&m->oplen==0&&
                m->flags==flags&&m->mxcsr==mxcsr&&m->fpu.tw==tag&&new ReadOnlySpan<byte>((byte*)m+160,256).SequenceEqual(xmm),"R113 actual controller precise #GP/#PF metadata before any MMX effect");
            Check(fixture.Issued==2&&fixture.Completed==1&&fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0&&
                fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R113 synchronous fault releases native/data/store ownership with no Op trampoline");
        }
        foreach(byte[] operation in new byte[][]{[0x0f,0x77],[0x0f,0xfc,0x03],[0x36,0x0f,0xfc,0x03],[0xf3,0x0f,0xd6,0xc1]})
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;fixture.Map(pc);fixture.Fill(pc);
            WriteControllerCode(fixture,pc,operation);var m=fixture.Machine();
            m->fpu.sw=4;m->fpu.cw=0x37b;m->fpu.tw=0x12345678;Write64((byte*)m,48,0x800000000000);
            byte[] xmm=new ReadOnlySpan<byte>((byte*)m+160,256).ToArray();uint flags=m->flags,mxcsr=m->mxcsr;
            fixture.Counts(100);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var fault=fixture.StepSynchronousFault(pc);
            Check(fault.Signal==8&&fault.Detail?.EndsWith("signal=8 code=3")==true&&m->trapno==16&&m->ip==pc&&m->oplen==0&&m->faultaddr==(long)pc&&
                m->fpu.tw==0x12345678&&m->fpu.sw==4&&m->fpu.cw==0x37b&&m->flags==flags&&m->mxcsr==mxcsr&&
                new ReadOnlySpan<byte>((byte*)m+160,256).SequenceEqual(xmm),"R113 native9 controller #MF priority and SIGFPE divide sticky, including waiting EMMS");
            Check(fixture.Issued==1&&fixture.Completed==0&&fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0&&
                fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R113 #MF controller preserves issue/fault and releases owned epochs/scopes");
        }
        var memoryForm=MmxForms().Single(x=>x.Name=="movq-load");
        foreach(string shape in new[]{"explicit","rsp","rbp","r12","r13","ds-rsp","ds-rbp","ds-r12","ds-r13"})
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;fixture.Map(pc);fixture.Fill(pc);
            byte[] operation=MmxSsEncoding(memoryForm,shape);WriteControllerCode(fixture,pc,operation);
            var i=MmxClassify(operation);var m=fixture.Machine();m->fpu.sw=0;m->fpu.cw=0x37f;Write64((byte*)m,24+i.Address.Base*8,0x800000000000);
            byte[] xmm=new ReadOnlySpan<byte>((byte*)m+160,256).ToArray();uint flags=m->flags,mxcsr=m->mxcsr;
            fixture.Counts(100);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var fault=fixture.StepSynchronousFault(pc);bool ss=FullMmxLowering.UsesStackSegment(i.Address);
            Check(fault.Signal==(ss?7:11)&&fault.Detail?.EndsWith(ss?"signal=7 code=128":"signal=11 code=128")==true&&m->trapno==(ss?12:13)&&m->faultaddr==0&&
                m->ip==pc&&m->oplen==0&&m->flags==flags&&m->mxcsr==mxcsr&&new ReadOnlySpan<byte>((byte*)m+160,256).SequenceEqual(xmm),
                "R113 native9 SS SIGBUS7/code128/trap12 versus DSoverride SIGSEGV11/trap13 precise controller state");
            Check(fixture.Issued==1&&fixture.Completed==0&&fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0&&
                fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R113 scoped MMX SS controller cleanup with no broader old-domain claim");
        }
    }
    static void CheckFullMmxEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],scaffoldBytes=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[]{false,true})foreach(bool shared in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(scaffoldBytes,out var layout,windows);var scaffold=layout.Resolve((nint)0x10000000);
            foreach(var form in MmxForms())foreach(bool memory in new[]{false,true})
            {
                if(memory?form.RegisterOnly:form.MemoryOnly)continue;
                var i=MmxClassify(MmxEncoding(form,memory,high:true,control:255));FullBlockInstruction[] instructions=[new(i)];
                for(int warm=0;warm<8;++warm)Check(shared?FullBlockEmitter.TryEmit(instructions,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(instructions,staging,out _,windows),"R113 both ABI/scaffold warm span emission");
                long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
                for(int repeat=0;repeat<16;++repeat)good&=shared?FullBlockEmitter.TryEmit(instructions,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(instructions,staging,out _,windows);
                Check(good&&GC.GetAllocatedBytesForCurrentThread()==before,"R113 both ABI/scaffold warmed spans allocate zero");
                int tiny;Check(!(shared?FullBlockEmitter.TryEmit(instructions,new byte[7],in scaffold,out tiny,windows):FullBlockEmitter.TryEmit(instructions,new byte[7],out tiny,windows))&&tiny==0,"R113 short buffer cannot publish partial code");
            }
        }
    }
}
