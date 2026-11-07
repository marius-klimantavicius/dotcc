using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. No emitter/frame/controller API dependency.
    // Parent invokes this only in the existing initialized contract fixture.
    static void CheckFullRepLowering(Blink program)
    {
        foreach(byte opcode in new byte[]{0xa4,0xa6,0xaa,0xac,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool address32 in new[]{false,true})
        foreach(byte segment in new byte[]{0,0x26,0x2e,0x3e,0x64,0x65})
        foreach(byte repeat in new byte[]{0xf2,0xf3})
        {
            bool compare=opcode is 0xa6 or 0xae;
            byte[] b=RepLoweringBytes(opcode,width,address32,segment,repeat);
            bool ok=FullRepLowering.TryClassify(Ip,b,out var i,out var reason);
            if(repeat==0xf2&&!compare)
            {Check(!ok&&reason?.Contains("F2")==true,"R100 names noncomparison F2 gap");continue;}
            Blink.XedDecodedInst decoded=default;
            fixed(byte* p=b)Check(program.DecodeInstruction(&decoded,p,(ulong)b.Length,Blink.XED_MODE_LONG)==0&&
                decoded.length==b.Length,"R100 independent authoritative XED length");
            Check(ok&&reason==null&&FullRepLowering.IsCanonical(in i),"R100 canonical REP lowering");
            var expected=opcode switch{0xa4=>FullRepOperation.Move,0xa6=>FullRepOperation.Compare,
                0xaa=>FullRepOperation.Store,0xac=>FullRepOperation.Load,_=>FullRepOperation.Scan};
            Check(i.Operation==expected&&i.Ip==Ip&&i.Length==b.Length&&i.Width==width&&
                i.AddressWidth==(address32?32:64)&&i.SourceSegment==
                (i.HasSource?(segment==0x64?4:segment==0x65?5:-1):-1)&&
                i.Repeat==(!compare?FullRepRepeat.Count:repeat==0xf3?FullRepRepeat.WhileEqual:FullRepRepeat.WhileNotEqual),
                "R100 independent opcode/repeat/width/address/segment expectations");
            if(i.HasDestination)Check(FullRepLowering.DestinationAccess(in i)==
                (opcode is 0xa4 or 0xaa?FullMemoryAccess.Write:FullMemoryAccess.Read),"R100 RR versus RW destination");
            for(int cut=0;cut<b.Length;++cut)
                Check(!FullRepLowering.TryClassify(Ip,b.AsSpan(0,cut),out _,out _),"R100 exact-span truncation");
            Check(!FullRepLowering.TryClassify(Ip,[..b,0x90],out _,out _),"R100 trailing opcode rejects");
        }
        foreach(byte opcode in new byte[]{0xa4,0xa5,0xa6,0xa7,0xaa,0xab,0xac,0xad,0xae,0xaf})
        for(byte rex=0x40;rex<=0x4f;++rex)
        {
            Check(FullRepLowering.TryClassify(Ip,[0x67,0xf3,0x66,rex,opcode],out var i,out _),"R100 REX grammar");
            Check(i.Width==((opcode&1)==0?8:(rex&8)!=0?64:16)&&i.AddressWidth==32,
                "R100 REX.W priority and ignored implicit register R/X/B bits");
        }
        foreach(byte[] prefix in RepPrefixPermutations([0xf3,0x66,0x67,0x64]))
            Check(FullRepLowering.TryClassify(Ip,[..prefix,0x48,0xa7],out var i,out _)&&i.Width==64&&
                i.AddressWidth==32&&i.SourceSegment==4&&i.Repeat==FullRepRepeat.WhileEqual,
                "R100 all24 independent legacy groups before REX");
        foreach(byte opcode in new byte[]{0xa4,0xa6,0xac})
            Check(FullRepLowering.TryClassify(Ip,[0x36,0xf3,opcode],out var i,out _) && i.SourceSegment==2 &&
                FullRepLowering.IsCanonical(in i),"R162 SS source retains typed #SS12 identity");
        foreach(byte opcode in new byte[]{0xaa,0xae})
            Check(FullRepLowering.TryClassify(Ip,[0x36,0xf3,opcode],out var i,out _)&&i.SourceSegment==-1,
                "R100 destination-only SS prefix stays ignored");
        foreach(byte[] bad in new byte[][]{[],[0xf3],[0xf3,0x48],[0xf3,0xf3,0xa4],[0xf2,0xf3,0xa6],
            [0xf3,0x66,0x66,0xa5],[0xf3,0x67,0x67,0xa5],[0xf3,0x64,0x65,0xa5],
            [0xf0,0xf3,0xa4],[0xf3,0xf0,0xa4],[0xf3,0x48,0x66,0xa5],[0xf3,0x40,0x48,0xa5],
            [0xa4],[0x66,0xa5],[0xf3,0x6c],[0xf3,0x6d],[0xf3,0x6e],[0xf3,0x6f],
            [0xf3,0x0f,0xa4,0xc0,1],new byte[16]})
            Check(!FullRepLowering.TryClassify(Ip,bad,out var i,out var reason)&&i==default&&reason!=null,
                "R100 malformed/missing/LOCK/port encodings cannot enter REP backend");
        var good=new FullRepInstruction(Ip,2,FullRepOperation.Compare,8,64,-1,FullRepRepeat.WhileEqual);
        foreach(var bad in new[]{good with{Length=1},good with{Length=16},good with{Width=128},
            good with{AddressWidth=16},good with{SourceSegment=3},good with{Repeat=FullRepRepeat.Count},
            good with{Repeat=(FullRepRepeat)4},good with{Operation=(FullRepOperation)5},
            good with{Operation=FullRepOperation.Store,SourceSegment=4,Repeat=FullRepRepeat.Count}})
            Check(!FullRepLowering.IsCanonical(in bad),"R100 forged IR rejects before emission");
    }

    static byte[] RepLoweringBytes(byte opcode,int width,bool address32,byte segment,byte repeat)
    {
        var b=new List<byte>(6){repeat};if(segment!=0)b.Add(segment);
        if(width==16)b.Add(0x66);if(address32)b.Add(0x67);if(width==64)b.Add(0x48);
        b.Add((byte)(opcode+(width==8?0:1)));return b.ToArray();
    }

    static IEnumerable<byte[]> RepPrefixPermutations(byte[] b)
    {
        if(b.Length==0){yield return [];yield break;}
        for(int k=0;k<b.Length;++k)
        {
            byte[] rest=[..b.AsSpan(0,k),..b.AsSpan(k+1)];
            foreach(var suffix in RepPrefixPermutations(rest))yield return [b[k],..suffix];
        }
    }
}
