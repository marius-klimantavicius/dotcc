using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // One cross-family prefix contract: raw stack, arithmetic, comparisons,
    // conversions, memory computation, state and waiting instructions.
    static void CheckFullCanonicalX87Prefixes(byte* machine)
    {
        _=machine;
        using(var decoder=new Blink())
        using(var binding=decoder.__DotCcEnter())
        {
            var forms=CanonicalX87Forms().Concat(ArithmeticForms()).Concat(Canon154Forms())
                .Concat(MemoryX87Forms().Select(f=>new byte[]{f.Opcode,(byte)(f.Group*8+3)}))
                .Concat(MemoryCXForms().Select(f=>new byte[]{f.Opcode,(byte)(f.Group*8+3)}))
                .Concat(canon157Forms.Select(m=>new byte[]{0xd9,m}))
                .Concat(new byte[][]{[0xd9,0x2b],[0xd9,0x3b],[0xd9,0x33],[0xd9,0x23],
                    [0xdd,0x33],[0xdd,0x23],[0xdd,0x3b]})
                .GroupBy(b=>Convert.ToHexString(b)).Select(g=>g.First()).ToArray();
            int admitted=0;
            foreach(var form in forms)
            foreach(byte repeat in new byte[]{0xf2,0xf3})
            foreach(byte[] order in new byte[][]{[],[0x40],[0x66],[0x67],[0x66,0x40]})
            {
                byte[] plain=[..order,..form],original=[repeat,..order,..form];
                Check(FullTranslationJit.R159Classify(Ip,plain,out var expected),
                    "x87 prefix table baseline must be an existing documented native form "+Convert.ToHexString(plain));
                Blink.XedDecodedInst decoded=default;
                fixed(byte* p=original)Check(decoder.DecodeInstruction(&decoded,p,(ulong)original.Length,Blink.XED_MODE_LONG)==0&&
                    decoded.length==original.Length,"single ignored x87 REP keeps authoritative whole length");
                Check(FullTranslationJit.R159Classify(Ip,original,out var actual)&&
                    (actual with{Length=expected.Length})==expected&&actual.Length==original.Length,
                    "single F2/F3 preserves x87 family/operands/width/address/FOP and original nextPC: "+Convert.ToHexString(original));
                ++admitted;
            }
            Check(admitted>3000,"cross-family ignored-prefix domain is explicitly table-enumerated");
            Span<byte> normalized=stackalloc byte[15];
            foreach(byte repeat in new byte[]{0xf2,0xf3})
            {
                foreach(byte[] bad in new byte[][]{[repeat,repeat,0xd8,0xc1],[0xf0,repeat,0xd8,0xc1],
                    [0xf2,0xf3,0xd9,0xe8]})
                    Check(FullCanonicalEncoding.TryNormalize(bad,normalized,out _,out _) == !bad.Contains((byte)0xf0),
                        "R179r1 repeated/mixed ignored x87 prefixes use explicit software policy; LOCK stays invalid, no hardware coverage claim");
                foreach((byte op,byte modrm,int width) in new[]{((byte)0xd9,(byte)0x33,224),((byte)0xdd,(byte)0x33,864),
                    ((byte)0xd9,(byte)0x23,224),((byte)0xdd,(byte)0x23,864)})
                {
                    byte[] original=[repeat,0x66,0x48,op,modrm];
                    Check(FullCanonicalEncoding.TryNormalize(original,normalized,out int n,out _)&&
                        normalized[..n].SequenceEqual(new byte[]{0x66,0x48,op,modrm})&&
                        FullTranslationJit.R159Fallback(Ip,original,out var i)&&i.SourceWidth==width&&i.Length==original.Length,
                        "normalizer retains REX.W over66 for large 28/108-byte legacy state format");
                }
                foreach((byte op,int width) in new[]{((byte)0xd9,112),((byte)0xdd,752)})
                {
                    byte[] original=[repeat,0x66,op,0x23];
                    Check(FullTranslationJit.R159Classify(Ip,original,out var small)&&small.SourceWidth==width&&
                        (small.Options&FullCanonicalX87ControlLowering.StateEnvironment16)!=0,
                        "R163r1 protected16 restore uses the authorized deterministic software FOP0 profile");
                }
            }
        }
        CheckCanonicalX87PrefixController();
    }

    static void CheckCanonicalX87PrefixController()
    {
        foreach(byte repeat in new byte[]{0xf2,0xf3})
        foreach(string kind in new[]{"wait","clear","push","add","load","store","memory-add","bcd"})
        {
            using var f=new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;const ulong data=pc+8192;
            f.Map(pc,12288);f.Fill(pc,12288);
            byte[] body=kind switch{"wait"=>[0x9b],"clear"=>[0xdb,0xe2],"push"=>[0xd9,0xe8],
                "add"=>[0xd8,0xc1],"load"=>[0xd9,0x03],"store"=>[0xd9,0x13],
                "memory-add"=>[0xd8,0x03],_=>[0xdf,0x23]};
            byte[] bytes=[repeat,..body];WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            f.DataScope(0,()=>{
                byte* p=f.Program.LookupAddress2(f.Machine(),(long)data,0,0);
                new Span<byte>(p,108).Clear();
                if(kind=="bcd")p[0]=1;
                else BinaryPrimitives.WriteUInt32LittleEndian(new Span<byte>(p,4),0x3f800000);
            });
            f.StartCanonicalFamily();
            try
            {
                var m=f.Machine();var file=f.CanonicalFile();
                m->fpu.cw=0x37f;m->fpu.sw=0;m->fpu.tw=0xfff0;
                *FullX87RegisterFile.Physical(file,0)=new(0x8000000000000000,0x3fff);
                *FullX87RegisterFile.Physical(file,1)=new(0x8000000000000000,0x4000);
                Write64((byte*)m,48,data);f.Counts(100);
                Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
                f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
                Check(f.Step(pc),"actual compiler starts ignored x87 prefix family");
                for(int n=0;f.Completed<1&&n<16;++n)Check(f.Continue(),"same issued prefixed x87 mapping continuation");
                Check(f.Issued==1&&f.Completed==1&&m->ip==pc+(uint)bytes.Length&&m->oplen==0&&
                    f.Statistics.InlineNativeInstructions==1&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                    "prefixed x87 compiles and executes actual generated body with original identity, no fallback: "+kind);
                if(kind is "load" or "push" or "bcd")
                    Check(((m->fpu.sw>>11)&7)==7&&FullX87RegisterFile.Physical(file,7)->Significand==0x8000000000000000&&
                        FullX87RegisterFile.Physical(file,7)->SignExponent==0x3fff,"prefixed raw80 load/push/BCD exact one");
                else if(kind is "add" or "memory-add")
                    Check(FullX87RegisterFile.Physical(file,0)->SignExponent==0x4000&&
                        FullX87RegisterFile.Physical(file,0)->Significand==(kind=="add"?0xc000000000000000UL:0x8000000000000000UL),
                        "prefixed integer-native true80 arithmetic result");
                else if(kind=="store")
                    f.DataScope(0,()=>Check(BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(
                        f.Program.LookupAddress2(f.Machine(),(long)data,0,0),4))==0x3f800000,"prefixed native store exact bytes"));
            }
            finally { f.NativePrepared=null;f.NativeReturned=null;f.EndCanonicalFamily(); }
        }
    }
}
