using Managed.Emulation.Execution;
using RepOracleFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Actual initialized native8 normal GetOp route.
    // Only bounded successful mappings supply an oracle. Faults, reversed SCAS,
    // LODS32 high halves and identity MOVSB have independent native assertions.
    static void CheckFullRepSelectedOracles()
    {
        using var f=new RepOracleFixture(interpreterOracle:true);
        const ulong src=RepOracleFixture.Base+0x10000,dst=src+0x10000;
        f.Map(src);f.Fill(src);f.Map(dst);f.Fill(dst);
        foreach(byte opcode in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool as32 in new[]{false,true})
        foreach(bool df in new[]{false,true})
        foreach(int count in new[]{0,1,3})
        foreach(byte repeat in new byte[]{0xf2,0xf3})
        {
            bool compares=opcode is 0xa6 or 0xae;if(repeat==0xf2&&!compares)continue;
            int n=width/8;ulong si=src+128,di=dst+128;
            if(as32){si|=0xabcd123400000000UL;di|=0x8765432100000000UL;}
            ulong cx=(ulong)count|(as32?0x7766554400000000UL:0);
            uint flags=(0xa52c0e15u&~0x10540u)|(df?1024u:0u);
            f.DataScope(0,()=>
            {
                for(int k=0;k<3;++k)
                {
                    long offset=128+(df?-k:k)*n;
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+offset,0,0),width,5);
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)dst+offset,0,0),width,5);
                }
                RepSeed(f,si,di,5,cx,flags);f.Machine()->ip=Ip;
                IntegerReference(f.Program,(byte*)f.Machine(),RepLoweringBytes(opcode,width,as32,0,repeat));
            });
            int iterations=compares&&repeat==0xf2?Math.Min(1,count):count;
            ulong mask=as32?uint.MaxValue:ulong.MaxValue,delta=(ulong)(iterations*n);
            ulong nextSi=iterations==0||opcode is 0xaa or 0xae?si:unchecked(si+(df?0UL-delta:delta))&mask;
            ulong nextDi=iterations==0||opcode==0xac?di:unchecked(di+(df?0UL-delta:delta))&mask;
            Check(RepRegister(f,72)==nextSi&&RepRegister(f,80)==nextDi&&RepRegister(f,24)==5&&
                RepRegister(f,32)==(iterations==0?cx:(ulong)(count-iterations))&&
                f.Machine()->flags==(compares&&iterations!=0?(flags&~0xff0008d1u)|64u:flags),
                "R100 actual selected bounded normal oracle where architectural results agree");
        }

        uint initial=0xa52c0e15u&~0x10500u;
        f.DataScope(0,()=>
        {
            RepSeed(f,src+128,src+128,5,3,initial);f.Machine()->ip=Ip;
            IntegerReference(f.Program,(byte*)f.Machine(),[0xf3,0xa4]);
        });
        Check(RepRegister(f,32)==3&&RepRegister(f,72)==src+128&&RepRegister(f,80)==src+128,
            "R94-S4 retained selected identity REP MOVSB skips count/index work; NOT a native oracle");
        f.DataScope(0,()=>
        {
            for(int k=0;k<3;++k)RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+128+4*k,0,0),32,5);
            RepSeed(f,src+128,dst+128,0xaabbccddffffffffUL,3,initial);f.Machine()->ip=Ip;
            IntegerReference(f.Program,(byte*)f.Machine(),[0xf3,0xad]);
        });
        Check(RepRegister(f,24)==0xaabbccdd00000005UL,
            "R94-S2 retained selected REP LODS32 high-half defect; native expected EAX zero-extension");
        f.DataScope(0,()=>
        {
            f.Program.LookupAddress2(f.Machine(),(long)dst+128,0,0)[0]=2;
            RepSeed(f,src+128,dst+128,5,3,initial);f.Machine()->ip=Ip;
            IntegerReference(f.Program,(byte*)f.Machine(),[0xf3,0xae]);
        });
        // 2-5 = 0xfd: CF, AF, SF and lazy parity byte 0xfd, no OF/ZF.
        Check(f.Machine()->flags==((initial&~0xff0008d1u)|0xfd000091u)&&RepRegister(f,32)==2,
            "R94-S1 retained selected REP SCAS computes memory-accumulator; native expected 5-2");
    }
}
