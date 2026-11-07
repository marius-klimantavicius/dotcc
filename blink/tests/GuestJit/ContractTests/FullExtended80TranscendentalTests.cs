using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using Managed.Emulation.Execution;
using Raw157=Managed.Emulation.Execution.FullExtended80;
using Op157=Managed.Emulation.Execution.FullExtended80Transcendental;

unsafe partial class Program
{
    // Called ONLY by the ONE R157 family entry. All emission/native calls are
    // AUTHORED_UNRUN. The interval constants are independent Fraction series,
    // not host Math/libm, product kernels, QEMU, or hardware-derived literals.
    readonly record struct Trans157Vector(int Op,Raw157 X,Raw157 Y,string Lo,string Hi,string SecondLo,string SecondHi);
    static readonly Trans157Vector[] trans157Vectors =
    [
        new(0,new(0x8000000000000000UL,0xbfff),new(0x0000000000000000UL,0x0000),"-57896044618658097711785492504343953926634992332820282019728792003956564819969","-57896044618658097711785492504343953926634992332820282019728792003956564819967","0","0"),
        new(0,new(0x8000000000000000UL,0xbffe),new(0x0000000000000000UL,0x0000),"-33914717729852067806019783465708279545762552194397326977635730407156374857451","-33914717729852067806019783465708279545762552194397326977635730407156374857450","0","0"),
        new(0,new(0x8000000000000000UL,0xbffd),new(0x0000000000000000UL,0x0000),"-18422936482918785462985756226923114913051158957521151124807166810542874066279","-18422936482918785462985756226923114913051158957521151124807166810542874066278","0","0"),
        new(0,new(0x8000000000000000UL,0x3ffd),new(0x0000000000000000UL,0x0000),"21908687144730225679043516720728893882093323970913138224094193758313804429023","21908687144730225679043516720728893882093323970913138224094193758313804429024","0","0"),
        new(0,new(0x8000000000000000UL,0x3ffe),new(0x0000000000000000UL,0x0000),"47962653777612059811531418077271348761744880276845910084186123193600379925034","47962653777612059811531418077271348761744880276845910084186123193600379925035","0","0"),
        new(0,new(0x8000000000000000UL,0x3fff),new(0x0000000000000000UL,0x0000),"115792089237316195423570985008687907853269984665640564039457584007913129639935","115792089237316195423570985008687907853269984665640564039457584007913129639937","0","0"),
        new(1,new(0xc000000000000000UL,0x3ffe),new(0xc000000000000000UL,0xbfff),"72087088729992659311765218332069997289781196677204790033217285677820089507085","72087088729992659311765218332069997289781196677204790033217285677820089507086","0","0"),
        new(1,new(0xc000000000000000UL,0x3ffe),new(0x8000000000000000UL,0x3fff),"-48058059153328439541176812221379998193187464451469860022144857118546726338057","-48058059153328439541176812221379998193187464451469860022144857118546726338056","0","0"),
        new(1,new(0xc000000000000000UL,0x3ffe),new(0xc000000000000000UL,0x3fff),"-72087088729992659311765218332069997289781196677204790033217285677820089507086","-72087088729992659311765218332069997289781196677204790033217285677820089507085","0","0"),
        new(1,new(0xc000000000000000UL,0x3fff),new(0xc000000000000000UL,0xbfff),"-101601045125981633823591259180961864490123780321256056025969090334049604952819","-101601045125981633823591259180961864490123780321256056025969090334049604952818","0","0"),
        new(1,new(0xc000000000000000UL,0x3fff),new(0x8000000000000000UL,0x3fff),"67734030083987755882394172787307909660082520214170704017312726889366403301879","67734030083987755882394172787307909660082520214170704017312726889366403301880","0","0"),
        new(1,new(0xc000000000000000UL,0x3fff),new(0xc000000000000000UL,0x3fff),"101601045125981633823591259180961864490123780321256056025969090334049604952818","101601045125981633823591259180961864490123780321256056025969090334049604952819","0","0"),
        new(1,new(0x8000000000000000UL,0x4000),new(0xc000000000000000UL,0xbfff),"-173688133855974293135356477513031861779904976998460846059186376011869694459904","-173688133855974293135356477513031861779904976998460846059186376011869694459904","0","0"),
        new(1,new(0x8000000000000000UL,0x4000),new(0x8000000000000000UL,0x3fff),"115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936","0","0"),
        new(1,new(0x8000000000000000UL,0x4000),new(0xc000000000000000UL,0x3fff),"173688133855974293135356477513031861779904976998460846059186376011869694459904","173688133855974293135356477513031861779904976998460846059186376011869694459904","0","0"),
        new(1,new(0x8000000000000000UL,0x4001),new(0xc000000000000000UL,0xbfff),"-347376267711948586270712955026063723559809953996921692118372752023739388919808","-347376267711948586270712955026063723559809953996921692118372752023739388919808","0","0"),
        new(1,new(0x8000000000000000UL,0x4001),new(0x8000000000000000UL,0x3fff),"231584178474632390847141970017375815706539969331281128078915168015826259279872","231584178474632390847141970017375815706539969331281128078915168015826259279872","0","0"),
        new(1,new(0x8000000000000000UL,0x4001),new(0xc000000000000000UL,0x3fff),"347376267711948586270712955026063723559809953996921692118372752023739388919808","347376267711948586270712955026063723559809953996921692118372752023739388919808","0","0"),
        new(2,new(0x8000000000000000UL,0xbfff),new(0x0000000000000000UL,0x0000),"-180335494232126009299543951459120824879359034990026179555057303577862227023150","-180335494232126009299543951459120824879359034990026179555057303577862227023149","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0xbfff),new(0x0000000000000000UL,0x0000),"-97435683363488292875031139702797626536946356753580186085281608312261259446718","-97435683363488292875031139702797626536946356753580186085281608312261259446717","62562732816211344028933569693394091302293358579475957900908260996539377313530","62562732816211344028933569693394091302293358579475957900908260996539377313531"),
        new(6,new(0x8000000000000000UL,0xbfff),new(0x0000000000000000UL,0x0000),"-97435683363488292875031139702797626536946356753580186085281608312261259446718","-97435683363488292875031139702797626536946356753580186085281608312261259446717","0","0"),
        new(7,new(0x8000000000000000UL,0xbfff),new(0x0000000000000000UL,0x0000),"62562732816211344028933569693394091302293358579475957900908260996539377313530","62562732816211344028933569693394091302293358579475957900908260996539377313531","0","0"),
        new(2,new(0x8000000000000000UL,0xbffe),new(0x0000000000000000UL,0x0000),"-63257506654560215648351813131252583934225991754237942097861021057837391258127","-63257506654560215648351813131252583934225991754237942097861021057837391258126","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0xbffe),new(0x0000000000000000UL,0x0000),"-55513684748706254392157395574451324146997108788015526773113170656738693667656","-55513684748706254392157395574451324146997108788015526773113170656738693667655","101617118319522600545601981648807607350213579319835970884288805016705398675944","101617118319522600545601981648807607350213579319835970884288805016705398675945"),
        new(6,new(0x8000000000000000UL,0xbffe),new(0x0000000000000000UL,0x0000),"-55513684748706254392157395574451324146997108788015526773113170656738693667656","-55513684748706254392157395574451324146997108788015526773113170656738693667655","0","0"),
        new(7,new(0x8000000000000000UL,0xbffe),new(0x0000000000000000UL,0x0000),"101617118319522600545601981648807607350213579319835970884288805016705398675944","101617118319522600545601981648807607350213579319835970884288805016705398675945","0","0"),
        new(2,new(0x8000000000000000UL,0xbffd),new(0x0000000000000000UL,0x0000),"-29566574528053993319636235836078771814458977950106124018258658112477653433647","-29566574528053993319636235836078771814458977950106124018258658112477653433646","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0xbffd),new(0x0000000000000000UL,0x0000),"-28647421327665059059430596260119789787021370826354543144805343654507971817713","-28647421327665059059430596260119789787021370826354543144805343654507971817712","112192393597863122712065585177748900737784171216163716639418346853706594800924","112192393597863122712065585177748900737784171216163716639418346853706594800925"),
        new(6,new(0x8000000000000000UL,0xbffd),new(0x0000000000000000UL,0x0000),"-28647421327665059059430596260119789787021370826354543144805343654507971817713","-28647421327665059059430596260119789787021370826354543144805343654507971817712","0","0"),
        new(7,new(0x8000000000000000UL,0xbffd),new(0x0000000000000000UL,0x0000),"112192393597863122712065585177748900737784171216163716639418346853706594800924","112192393597863122712065585177748900737784171216163716639418346853706594800925","0","0"),
        new(2,new(0x8000000000000000UL,0x3ffd),new(0x0000000000000000UL,0x0000),"29566574528053993319636235836078771814458977950106124018258658112477653433646","29566574528053993319636235836078771814458977950106124018258658112477653433647","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0x3ffd),new(0x0000000000000000UL,0x0000),"28647421327665059059430596260119789787021370826354543144805343654507971817712","28647421327665059059430596260119789787021370826354543144805343654507971817713","112192393597863122712065585177748900737784171216163716639418346853706594800924","112192393597863122712065585177748900737784171216163716639418346853706594800925"),
        new(6,new(0x8000000000000000UL,0x3ffd),new(0x0000000000000000UL,0x0000),"28647421327665059059430596260119789787021370826354543144805343654507971817712","28647421327665059059430596260119789787021370826354543144805343654507971817713","0","0"),
        new(7,new(0x8000000000000000UL,0x3ffd),new(0x0000000000000000UL,0x0000),"112192393597863122712065585177748900737784171216163716639418346853706594800924","112192393597863122712065585177748900737784171216163716639418346853706594800925","0","0"),
        new(2,new(0x8000000000000000UL,0x3ffe),new(0x0000000000000000UL,0x0000),"63257506654560215648351813131252583934225991754237942097861021057837391258126","63257506654560215648351813131252583934225991754237942097861021057837391258127","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0x3ffe),new(0x0000000000000000UL,0x0000),"55513684748706254392157395574451324146997108788015526773113170656738693667655","55513684748706254392157395574451324146997108788015526773113170656738693667656","101617118319522600545601981648807607350213579319835970884288805016705398675944","101617118319522600545601981648807607350213579319835970884288805016705398675945"),
        new(6,new(0x8000000000000000UL,0x3ffe),new(0x0000000000000000UL,0x0000),"55513684748706254392157395574451324146997108788015526773113170656738693667655","55513684748706254392157395574451324146997108788015526773113170656738693667656","0","0"),
        new(7,new(0x8000000000000000UL,0x3ffe),new(0x0000000000000000UL,0x0000),"101617118319522600545601981648807607350213579319835970884288805016705398675944","101617118319522600545601981648807607350213579319835970884288805016705398675945","0","0"),
        new(2,new(0x8000000000000000UL,0x3fff),new(0x0000000000000000UL,0x0000),"180335494232126009299543951459120824879359034990026179555057303577862227023149","180335494232126009299543951459120824879359034990026179555057303577862227023150","115792089237316195423570985008687907853269984665640564039457584007913129639936","115792089237316195423570985008687907853269984665640564039457584007913129639936"),
        new(5,new(0x8000000000000000UL,0x3fff),new(0x0000000000000000UL,0x0000),"97435683363488292875031139702797626536946356753580186085281608312261259446717","97435683363488292875031139702797626536946356753580186085281608312261259446718","62562732816211344028933569693394091302293358579475957900908260996539377313530","62562732816211344028933569693394091302293358579475957900908260996539377313531"),
        new(6,new(0x8000000000000000UL,0x3fff),new(0x0000000000000000UL,0x0000),"97435683363488292875031139702797626536946356753580186085281608312261259446717","97435683363488292875031139702797626536946356753580186085281608312261259446718","0","0"),
        new(7,new(0x8000000000000000UL,0x3fff),new(0x0000000000000000UL,0x0000),"62562732816211344028933569693394091302293358579475957900908260996539377313530","62562732816211344028933569693394091302293358579475957900908260996539377313531","0","0"),
        new(3,new(0x8000000000000000UL,0xbfff),new(0x8000000000000000UL,0xbffd),"-335404985669742047556211389801203614690132364817476154192393747380411922383536","-335404985669742047556211389801203614690132364817476154192393747380411922383535","0","0"),
        new(3,new(0x8000000000000000UL,0xbfff),new(0x8000000000000000UL,0x3ffd),"335404985669742047556211389801203614690132364817476154192393747380411922383535","335404985669742047556211389801203614690132364817476154192393747380411922383536","0","0"),
        new(3,new(0x8000000000000000UL,0x3fff),new(0x8000000000000000UL,0xbffd),"-28366591222024276724023552976526247963261012510916275766378403737526972082650","-28366591222024276724023552976526247963261012510916275766378403737526972082649","0","0"),
        new(3,new(0x8000000000000000UL,0x3fff),new(0x8000000000000000UL,0x3ffd),"28366591222024276724023552976526247963261012510916275766378403737526972082649","28366591222024276724023552976526247963261012510916275766378403737526972082650","0","0"),
        new(3,new(0x8000000000000000UL,0xc000),new(0x8000000000000000UL,0xbffd),"-349372252266101755345560243427462740447374141077446587907753259871061227399581","-349372252266101755345560243427462740447374141077446587907753259871061227399580","0","0"),
        new(3,new(0x8000000000000000UL,0xc000),new(0x8000000000000000UL,0x3ffd),"349372252266101755345560243427462740447374141077446587907753259871061227399580","349372252266101755345560243427462740447374141077446587907753259871061227399581","0","0"),
        new(3,new(0x8000000000000000UL,0x4000),new(0x8000000000000000UL,0xbffd),"-14399324625664568934674699350267122206019236250945842051018891246877667066606","-14399324625664568934674699350267122206019236250945842051018891246877667066605","0","0"),
        new(3,new(0x8000000000000000UL,0x4000),new(0x8000000000000000UL,0x3ffd),"14399324625664568934674699350267122206019236250945842051018891246877667066605","14399324625664568934674699350267122206019236250945842051018891246877667066606","0","0"),
        new(4,new(0x8000000000000000UL,0xbffd),new(0xc000000000000000UL,0xbfff),"72087088729992659311765218332069997289781196677204790033217285677820089507085","72087088729992659311765218332069997289781196677204790033217285677820089507086","0","0"),
        new(4,new(0x8000000000000000UL,0xbffd),new(0x8000000000000000UL,0x3fff),"-48058059153328439541176812221379998193187464451469860022144857118546726338057","-48058059153328439541176812221379998193187464451469860022144857118546726338056","0","0"),
        new(4,new(0x8000000000000000UL,0xbffd),new(0xc000000000000000UL,0x3fff),"-72087088729992659311765218332069997289781196677204790033217285677820089507086","-72087088729992659311765218332069997289781196677204790033217285677820089507085","0","0"),
        new(4,new(0x8000000000000000UL,0xbffc),new(0xc000000000000000UL,0xbfff),"33460164084353458543020921465689033785498546527268115749807481176948392963368","33460164084353458543020921465689033785498546527268115749807481176948392963369","0","0"),
        new(4,new(0x8000000000000000UL,0xbffc),new(0x8000000000000000UL,0x3fff),"-22306776056235639028680614310459355856999031018178743833204987451298928642246","-22306776056235639028680614310459355856999031018178743833204987451298928642245","0","0"),
        new(4,new(0x8000000000000000UL,0xbffc),new(0xc000000000000000UL,0x3fff),"-33460164084353458543020921465689033785498546527268115749807481176948392963369","-33460164084353458543020921465689033785498546527268115749807481176948392963368","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffc),new(0xc000000000000000UL,0xbfff),"-29513956395988974511826040848891867200342583644051265992751804656229515445734","-29513956395988974511826040848891867200342583644051265992751804656229515445733","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffc),new(0x8000000000000000UL,0x3fff),"19675970930659316341217360565927911466895055762700843995167869770819676963822","19675970930659316341217360565927911466895055762700843995167869770819676963823","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffc),new(0xc000000000000000UL,0x3fff),"29513956395988974511826040848891867200342583644051265992751804656229515445733","29513956395988974511826040848891867200342583644051265992751804656229515445734","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffd),new(0xc000000000000000UL,0xbfff),"-55915090036794984957714261111188501405344636789133124093645548026207713173935","-55915090036794984957714261111188501405344636789133124093645548026207713173934","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffd),new(0x8000000000000000UL,0x3fff),"37276726691196656638476174074125667603563091192755416062430365350805142115956","37276726691196656638476174074125667603563091192755416062430365350805142115957","0","0"),
        new(4,new(0x8000000000000000UL,0x3ffd),new(0xc000000000000000UL,0x3fff),"55915090036794984957714261111188501405344636789133124093645548026207713173934","55915090036794984957714261111188501405344636789133124093645548026207713173935","0","0"),
        new(4,new(0xc000000000000000UL,0x3ffd),new(0xc000000000000000UL,0xbfff),"-79797820475541819602383231237939548984107070425414333205900937095987134729121","-79797820475541819602383231237939548984107070425414333205900937095987134729120","0","0"),
        new(4,new(0xc000000000000000UL,0x3ffd),new(0x8000000000000000UL,0x3fff),"53198546983694546401588820825293032656071380283609555470600624730658089819413","53198546983694546401588820825293032656071380283609555470600624730658089819414","0","0"),
        new(4,new(0xc000000000000000UL,0x3ffd),new(0xc000000000000000UL,0x3fff),"79797820475541819602383231237939548984107070425414333205900937095987134729120","79797820475541819602383231237939548984107070425414333205900937095987134729121","0","0"),
    ];
    static bool Trans157Pair(Op157 op)=>op is Op157.Fptan or Op157.Fsincos;
    static bool Trans157Binary(Op157 op)=>op is Op157.Fyl2x or Op157.Fyl2xp1 or Op157.Fpatan;
    static bool Trans157Trig(Op157 op)=>op is Op157.Fptan or Op157.Fsincos or Op157.Fsin or Op157.Fcos;
    static FullExtended80WorkFrame Trans157Frame(Raw157 x,Raw157 y,ushort cw,nint tail=0)=>new()
    {Version=1,Size=384,ControlWord=cw,Left=x,Right=y,Continuation=(ulong)tail};
    static NativeCodeAllocation Trans157Code(Op157 op,bool windows,bool continuation)
    {
        byte[] b=new byte[FullExtended80TranscendentalEmitter.CodeCapacity];int n;
        bool ok=continuation?FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80TranscendentalEmitter.TryEmit(op,b,out n,windows);
        Check(ok&&n>0&&n<=b.Length,"R157 bounded operation-specialized code");return Transform80Publish(b.AsSpan(0,n));
    }
    static void Trans157Near(Raw157 value,string low,string high,int rc,bool multipliedLog)
    {
        int e=(value.SignExponent&0x7fff)-16383;
        Check((value.SignExponent&0x7fff) is >0 and <0x7fff && value.Significand>>63==1,
            "R157 ordinary rational case is finite normalized true80");
        BigInteger actual=new BigInteger(value.Significand)<<(e-63+256);
        if((value.SignExponent&0x8000)!=0)actual=-actual;
        BigInteger lo=BigInteger.Parse(low),hi=BigInteger.Parse(high);
        BigInteger magnitude=BigInteger.Max(BigInteger.Abs(lo),BigInteger.Abs(hi));
        int k=(int)magnitude.GetBitLength()-257;
        BigInteger ulp=BigInteger.One<<(k-63+256);
        BigInteger error=BigInteger.Max(BigInteger.Zero,BigInteger.Max(lo-actual,actual-hi));
        int numerator=rc==0?(multipliedLog?27:20):30;
        Check(error*20<=numerator*ulp,"R157 independent 2^-256 rational interval + Intel093 reduced-domain ulp envelope");
    }
    static void Trans157ComputeSuite()
    {
        Trans157Emission();
        foreach(Op157 op in Enum.GetValues<Op157>())
        foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        {
            using var code=Trans157Code(op,windows,continuation);
            using var ret=Transform80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var adapter=Transform80Adapter(code.Entry,windows,continuation);
            nint tail=continuation?ret.Entry:0;
            foreach(var v in trans157Vectors)
            {
                if(v.Op!=(int)op)continue;
                foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
                foreach(int masks in new[]{0,7,31,47,55,63})
                {
                    ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                    var f=Trans157Frame(v.X,v.Y,cw,tail);Trans157Invoke(adapter.Entry,ref f);
                    Check((uint)f.Admission==0,"R157 no defined-domain256 refusal, including unmasked post events");
                    bool log=op is Op157.Fyl2x or Op157.Fyl2xp1;
                    Trans157Near(f.Normal,v.Lo,v.Hi,rc,log&&v.Y.Significand!=0x8000000000000000UL);
                    if(Trans157Pair(op))Trans157Near(f.Wrapped,v.SecondLo,v.SecondHi,rc,false);
                    uint m=(uint)f.Classification;
                    Check((m&0x3ff)==(uint)((int)op+1|0x100|(Trans157Pair(op)?0x200:0)),"R157 typed kind/valid pair metadata");
                    Check(((m&0x4000)!=0)==((m&0x10000)==0&&(masks&0x38)!=0x38),"R157 any unmasked OE/UE/PE selects nonwaiting actual capture");
                    Check(((m&0x1000)!=0)==((f.WrappedStatus&32)!=0&&(cw&32)==0),"R157 precision completes then deferred trap");
                }
            }
            Trans157Specials(adapter.Entry,op,tail);Trans157Ranges(adapter.Entry,op,tail);
            if(op==Op157.Fyl2xp1)Trans157Log1pEndpoints(adapter.Entry,tail);
            Trans157Protocol(adapter.Entry,tail);Trans157RangeExceptions(adapter.Entry,op,tail);Trans157WrappedLiterals(adapter.Entry,op,tail);
            using var preserve=Trans157HostEnvelope(adapter.Entry);
            foreach(ushort cw in new ushort[]{0x37f,0x340,0x347,0xf47})
            {
                var f=Trans157Frame(new(0x8000000000000000,0x3ffd),new(0x8000000000000000,0x3fff),cw,tail);
                Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)preserve.Entry)(&f)==0,
                    "R157 exact defined FX64 state + RSP restoration, host pendingIE, all XMM/MXCSR, both exits/ABI encodings");
            }
        }
    }
    static uint Trans157Invoke(nint target,ref FullExtended80WorkFrame frame)
    {
        byte* guarded=stackalloc byte[416];new Span<byte>(guarded,416).Fill(0xad);
        var f=(FullExtended80WorkFrame*)(guarded+16);*f=frame;
        new Span<byte>((byte*)f+96,288).Fill(0xa7);
        f->Left.Reserved10=0x1234;f->Left.Reserved12=0x56789abc;
        f->Right.Reserved10=0xdef0;f->Right.Reserved12=0x87654321;
        byte[] before=new ReadOnlySpan<byte>(f,384).ToArray();
        uint admission=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)target)(f);
        Check(admission==(uint)f->Admission,"R157 EAX/frame exact admission");
        Check(new ReadOnlySpan<byte>(f,12).SequenceEqual(before.AsSpan(0,12))&&
            new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(before.AsSpan(16,32))&&
            new ReadOnlySpan<byte>((byte*)f+88,8).SequenceEqual(before.AsSpan(88,8))&&
            new ReadOnlySpan<byte>((byte*)f+256,128).SequenceEqual(before.AsSpan(256,128)),
            "R157 input/header/padding/continuation/bridge/caller storage unchanged");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xad)<0&&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xad)<0,"R157 full Work384 guards");
        if(admission==0)
        {
            Check(f->Normal.Reserved10==0&&f->Normal.Reserved12==0&&f->Wrapped.Reserved10==0&&f->Wrapped.Reserved12==0,
                "R157 canonical raw output padding");
            Check(*(ulong*)((byte*)f+248)==0,"R157 no original host RSP escape");
            if(((uint)f->Classification&0x4000)!=0)
                Check(new ReadOnlySpan<byte>((byte*)f+96,112).IndexOfAnyExcept((byte)0)<0,
                    "R157 nonwaiting capture pointers/empty-register stale data fully scrubbed");
        }
        else Check(new ReadOnlySpan<byte>((byte*)f+48,40).IndexOfAnyExcept((byte)0)<0,"R157 nonzero admission no candidates/status/metadata");
        frame=*f;return admission;
    }
    static void Trans157Specials(nint target,Op157 op,nint tail)
    {
        foreach(var x in new Raw157[]{default,new(0,0x8000),new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff),new(0x8000000000000123,0x7fff),
            new(0xc000000000000123,0xffff),new(1,0x4000),new(1,0x7fff),new(1,0),new(0x8000000000000001,0)})
        foreach(ushort masks in new ushort[]{0,1,2,3,7,31,47,55,62,63})
        {
            var y=new Raw157(0x8000000000000000,0x3fff);
            var f=Trans157Frame(x,y,(ushort)(0x340|masks),tail);Trans157Invoke(target,ref f);
            Check((uint)f.Admission==0,"R157 all input raw classes have a native profile result");
            int cls=Canon154Class(x);uint delta=f.WrappedStatus;
            if(cls is 4 or 5 || cls==2&&Trans157Trig(op))
            {
                Check((f.NormalStatus&0x23f)==1&&delta==1,"R157 unsupported/sNaN IE outranks lower flags");
                Check((((uint)f.Classification&0x400)!=0)==((masks&1)==0),"R157 IE mask completion");
                if((masks&1)!=0)Check(Canon154Class(f.Normal)==3,"R157 masked invalid is quiet NaN");
            }
            if(cls==6&&(masks&2)==0)
            {Check(delta==2&&((uint)f.Classification&0x400)!=0,"R157 unmasked DE suppresses both outputs/stack and lower events");}
            if(((uint)f.Classification&0x400)!=0)
                Check(f.Normal.Significand==0&&f.Normal.SignExponent==0&&f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0,
                    "R157 suppressed completion has no usable numeric image");
        }
        if(Trans157Binary(op))
        foreach(var a in new Raw157[]{new(1,0),new(0x8000000000000001,0),new(0x8000000000000123,0x7fff)})
        {
            var f=Trans157Frame(a,new(0xc000000000000123,0x7fff),0x340,tail);Trans157Invoke(target,ref f);
            bool invalid=Canon154Class(a)==4;
            Check(f.WrappedStatus==(invalid?1:0),"R157 two-input NaN priority suppresses DE, sNaN still IE");
        }
    }
    static void Trans157Ranges(nint target,Op157 op,nint tail)
    {
        if(Trans157Trig(op))
        foreach(ushort e in new ushort[]{0x403e,0xc03e,0x7ffe,0xfffe})
        {
            var f=Trans157Frame(new(0x8000000000000000,e),default,0x340,tail);Trans157Invoke(target,ref f);
            Check((uint)f.Admission==0&&(uint)f.Classification==(uint)((int)op+1|0x800)&&
                *(uint*)((byte*)&f+232)==0x04000400&&f.NormalStatus==0&&f.WrappedStatus==0,
                "R157 finite abs(x)>=2^63 C2-only completion without result/push");
        }
        if(op is Op157.F2xm1 or Op157.Fyl2xp1)
        foreach(var x in new Raw157[]{new(0x8000000000000001,0x3fff),new(0x8000000000000000,0x4000),
            new(0x8000000000000000,0x7fff),new(0x95f619980c4336f8,0xbffd)})
        {
            if(op==Op157.F2xm1&&(x.SignExponent&0x7fff)==0x3ffd)continue;
            var y=new Raw157(0xc000000000000000,0xbfff);var f=Trans157Frame(x,y,0x340,tail);Trans157Invoke(target,ref f);
            var wanted=op==Op157.F2xm1?x:y;
            Check((uint)f.Classification==(uint)((int)op+1|0x8100)&&f.Normal.Significand==wanted.Significand&&
                f.Normal.SignExponent==wanted.SignExponent&&f.NormalStatus==0&&f.WrappedStatus==0,
                "R157 explicitly undefined CopyInput profile, distinct from defined exponent wrap");
        }
    }
    static void Trans157Log1pEndpoints(nint target,nint tail)
    {
        BigInteger scale=BigInteger.One<<65;
        ulong negative=0x95f619980c4336f7,positive=0xd413cccfe7799211;
        Check((scale-negative)*(scale-negative)>=(BigInteger.One<<129)&&
            (scale-negative-1)*(scale-negative-1)<(BigInteger.One<<129),"R157 negative endpoint exact directed integer-square floor proof");
        Check((scale+positive)*(scale+positive)<=(BigInteger.One<<131)&&
            (scale+positive+1)*(scale+positive+1)>(BigInteger.One<<131),"R157 positive endpoint exact directed integer-square floor proof");
        foreach(bool minus in new[]{false,true})foreach(int adjacent in new[]{-1,0,1})
        {
            ulong sig=adjacent<0?(minus?negative:positive)-1:(minus?negative:positive)+(uint)adjacent;
            var f=Trans157Frame(new(sig,minus?(ushort)0xbffd:(ushort)0x3ffd),new(0x8000000000000000,0x3fff),0x37f,tail);
            Trans157Invoke(target,ref f);
            Check((((uint)f.Classification&0x8000)!=0)==(adjacent>0),"R157 asymmetric endpoints: exact neighboring binary80 values");
        }
        foreach(bool minus in new[]{false,true})
        {
            var f=Trans157Frame(new(0xc000000000000000,minus?(ushort)0xbffd:(ushort)0x3ffd),new(0x8000000000000000,0x3fff),0x37f,tail);
            Trans157Invoke(target,ref f);
            Check((((uint)f.Classification&0x8000)!=0)==minus,"R157 +.375 defined native logarithm; -.375 undefined software profile");
            if(!minus)Check(f.Normal.Significand!=0x8000000000000000||f.Normal.SignExponent!=0x3fff,"R157 defined +.375 must not copy y=1");
        }
        foreach(var nan in new Raw157[]{new(0xc000000000000123,0x7fff),new(0x8000000000000123,0x7fff)})
        {
            var f=Trans157Frame(new(0xc000000000000000,0xbffd),nan,0x340,tail);Trans157Invoke(target,ref f);
            Check(((uint)f.Classification&0x8000)==0&&f.WrappedStatus==(Canon154Class(nan)==4?1:0),
                "R157 NaN/sNaN in y outranks -.375 undefined-domain software profile");
        }
    }
    static void Trans157RangeExceptions(nint target,Op157 op,nint tail)
    {
        // All8 ONE table: tiny, exact-tiny boundary, post PE and log overflow.
        var rows=new List<(Raw157 X,Raw157 Y)>
        { (new(1,0),new(0x8000000000000000,0x3fff)),
          (new(0x8000000000000000,1),new(0x8000000000000000,0x3fff)),
          (new(0x8000000000000001,1),new(0x8000000000000000,0x3fff)),
          (new(0x8000000000000000,0x3ffd),new(0x8000000000000000,0x3fff)) };
        if(op==Op157.Fyl2x)
        {rows.Add((new(0x8000000000000000,0x4000),new(ulong.MaxValue,0x7ffe)));
         rows.Add((new(0x8000000000000000,0x7ffe),new(ulong.MaxValue,0x7ffe)));
         rows.Add((new(0x8000000000000001,0x3fff),new(1,0)));}
        if(op==Op157.Fyl2xp1)
        {rows.Add((new(1,0),new(1,0)));rows.Add((new(0x8000000000000000,1),new(0x8000000000000000,1)));}
        if(op==Op157.Fpatan)rows.Add((new(ulong.MaxValue,0x7ffe),new(1,0)));
        foreach(var row in rows)foreach(int rc in new[]{0,1,2,3})
        foreach(int masks in new[]{7,15,23,31,39,47,55,63})
        {
            var f=Trans157Frame(row.X,row.Y,(ushort)(0x340|rc<<10|masks),tail);Trans157Invoke(target,ref f);
            Check((uint)f.Admission==0,"R157 defined unmasked OE/UE/PE/tiny completion is computed, never256");
            if((masks&0x38)!=0x38&&((uint)f.Classification&0x10000)==0)
            {
                Check(((uint)f.Classification&0x4000)!=0&&*(uint*)((byte*)&f+244)==1,"R157 real nonwaiting post-event capture");
                ushort sw=*(ushort*)((byte*)&f+216);
                Check((sw>>11&7)==(Trans157Pair(op)?6:7),"R157 native logical stack completion captured before restore");
                if((f.WrappedStatus&0x18&~masks)!=0)
                    Check((f.Normal.SignExponent&0x7fff) is >0 and <0x7fff,"R157 unmasked range produces finite adjusted-exponent true80");
            }
        }
    }
    static void Trans157WrappedLiterals(nint target,Op157 op,nint tail)
    {
        if(op!=Op157.Fyl2x)return;
        // Integer-exact logarithms and powers of two give independent raw80
        // exponent-wrap anchors, not a masked-result rescaling approximation.
        foreach(int rc in new[]{0,1,2,3})
        {
            var overflow=Trans157Frame(new(0x8000000000000000,0x4001),new(0x8000000000000000,0x7ffe),
                (ushort)(0x377|rc<<10),tail); // x=4,y=2^16383 ->2^16384
            Trans157Invoke(target,ref overflow);
            Check(overflow.Normal.Significand==0x8000000000000000&&overflow.Normal.SignExponent==0x1fff&&
                (overflow.WrappedStatus&8)!=0,"R157 exact OE wrap: 2^16384 /2^24576 =2^-8192");
            var tiny=Trans157Frame(new(0x8000000000000000,0x3ffe),new(1,0),
                (ushort)(0x36f|rc<<10),tail); // x=.5,y=2^-16445 -> -2^-16445
            Trans157Invoke(target,ref tiny);
            Check(tiny.Normal.Significand==0x8000000000000000&&tiny.Normal.SignExponent==0xdfc2&&
                (tiny.WrappedStatus&0x12)==0x12,"R157 exact tiny UE wrap: -2^-16445 *2^24576 =-2^8131, masked DE retained");
        }
    }
    static void Trans157Protocol(nint target,nint tail)
    {
        foreach((uint version,uint size,uint admission) in new[]{(0u,384u,1u),(2u,384u,1u),(1u,0u,2u),(1u,383u,2u),(1u,385u,2u)})
        {
            var f=Trans157Frame(default,default,0x37f,tail);f.Version=version;f.Size=size;
            Check(Trans157Invoke(target,ref f)==admission,"R157 exact version/size rejection before scratch/native FP");
            Check(new ReadOnlySpan<byte>((byte*)&f+96,160).IndexOfAnyExcept((byte)0xa7)<0,"R157 protocol rejection preserves private scratch");
        }
    }
    static void Trans157Emission()
    {
        byte[] b=new byte[FullExtended80TranscendentalEmitter.CodeCapacity+16];
        foreach(Op157 op in Enum.GetValues<Op157>())foreach(bool windows in new[]{false,true})foreach(bool cont in new[]{false,true})
        {
            int n;Check(cont?FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b,out n):
                FullExtended80TranscendentalEmitter.TryEmit(op,b,out n,windows),"R157 all8 both ABI/tail encodings");
            byte[] image=b.AsSpan(0,n).ToArray();
            Check(image.AsSpan(0,4).SequenceEqual(new byte[]{0xf3,0x0f,0x1e,0xfa}),"R157 CET prefix");
            Check(cont?image.AsSpan(n-7).SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}):image[n-1]==0xc3,"R157 restored RET/R11 tail");
            for(int size=0;size<n;++size)
            {
                Array.Fill(b,(byte)0xb7);int m;bool ok=cont?FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b.AsSpan(8,size),out m):
                    FullExtended80TranscendentalEmitter.TryEmit(op,b.AsSpan(8,size),out m,windows);
                Check(!ok&&m==0&&b.AsSpan(0,8).IndexOfAnyExcept((byte)0xb7)<0&&b.AsSpan(8+size).IndexOfAnyExcept((byte)0xb7)<0,
                    "R157 every short span refuses with exact bounds");
            }
            for(int k=0;k<16;++k)if(cont)FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b,out _);else FullExtended80TranscendentalEmitter.TryEmit(op,b,out _,windows);
            long before=GC.GetAllocatedBytesForCurrentThread();bool all=true;
            for(int k=0;k<128;++k)all&=cont?FullExtended80TranscendentalEmitter.TryEmitContinuation(op,b,out _):FullExtended80TranscendentalEmitter.TryEmit(op,b,out _,windows);
            Check(all&&GC.GetAllocatedBytesForCurrentThread()==before,"R157 warm bounded writer no managed allocation");
        }
        Array.Fill(b,(byte)0xb7);
        Check(!FullExtended80TranscendentalEmitter.TryEmit((Op157)255,b,out int bad)&&bad==0&&b.AsSpan().IndexOfAnyExcept((byte)0xb7)<0,
            "R157 unknown selector no writes");
    }
    // Synchronous test shim: seed a full FX64 host state INCLUDING pending IE,
    // nonzero TOP/full tags, 64-bit pointers, MXCSR and all16 XMM patterns; call
    // candidate; capture after; restore outer state before any managed code.
    // It compares defined FX fields only (reserved/padding are unspecified).
    static NativeCodeAllocation Trans157HostEnvelope(nint target)
    {
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa,0x53,0x41,0x54,0x41,0x55,0x41,0x56};
        bool win=OperatingSystem.IsWindows();
        void D(int n){for(int i=0;i<4;++i)b.Add((byte)(n>>(8*i)));}
        void Q(ulong n){for(int i=0;i<8;++i)b.Add((byte)(n>>(8*i)));}
        void StackStore(int reg,int off,int width=64)
        {if(width==16)b.Add(0x66);b.Add((byte)((width==64?0x48:0x40)|(reg>=8?4:0)));b.Add(0x89);b.Add((byte)(0x84|((reg&7)<<3)));b.Add(0x24);D(off);}
        void StackLoad(int reg,int off,int width=64)
        {b.Add((byte)((width==64?0x48:0x40)|(reg>=8?4:0)));if(width==16)b.AddRange(new byte[]{0x0f,0xb7});else b.Add(0x8b);b.Add((byte)(0x84|((reg&7)<<3)));b.Add(0x24);D(off);}
        void Imm(ulong n){b.AddRange(new byte[]{0x48,0xb8});Q(n);}
        void Fx(bool restore,int off)
        {b.AddRange(new byte[]{0x48,0x0f,0xae,(byte)(restore?0x8c:0x84),0x24});D(off);}
        b.AddRange(win?new byte[]{0x49,0x89,0xcc}:new byte[]{0x49,0x89,0xfc}); // R12=frame
        b.AddRange(new byte[]{0x48,0x81,0xec});D(1592); // call-aligned, home32
        Imm(0);for(int p=48;p<1584;p+=8)StackStore(0,p);
        Fx(false,48); // untouched OUTER image
        for(int p=0;p<512;p+=8){StackLoad(0,48+p);StackStore(0,560+p);}
        Imm(0x0c7e);StackStore(0,560,16); // IM unmasked; RC toward zero
        Imm(0xa881);StackStore(0,562,16); // B/ES/IE, TOP5, undefined CC pattern
        Imm(0xff);StackStore(0,564,16);Imm(0x345);StackStore(0,566,16);
        Imm(0x00007ffe12345678);StackStore(0,568);
        Imm(0x00007ffd87654321);StackStore(0,576);
        Imm(0x5f80);StackStore(0,584,32); // masked MXCSR with nondefault RC
        for(int i=0;i<8;++i)
        {Imm(0x8000000000000100UL+(uint)i);StackStore(0,592+16*i);Imm((uint)(0x3fff+i));StackStore(0,600+16*i,16);}
        for(int i=0;i<32;++i){Imm(0x9abcde0100000000UL+(uint)i);StackStore(0,720+8*i);}
        Fx(true,560);Fx(false,560); // canonical actual BEFORE, still pending IE
        b.AddRange(win?new byte[]{0x4c,0x89,0xe1}:new byte[]{0x4c,0x89,0xe7});
        Imm((ulong)target);b.AddRange(new byte[]{0xff,0xd0,0x89,0xc3}); // EBX=admission
        Fx(false,1072);Fx(true,48); // capture AFTER, restore outer BEFORE compare
        // Return bit31 on any mismatch. No FP/MMX/XMM after outer restore.
        var offsets=new List<int>{0,2,6};
        for(int p=8;p<32;p+=2)offsets.Add(p);
        for(int i=0;i<8;++i)for(int p=0;p<10;p+=2)offsets.Add(32+16*i+p);
        for(int p=160;p<416;p+=2)offsets.Add(p);
        // FXSAVE byte4 is FTW; byte5 is reserved and is deliberately excluded.
        StackLoad(0,564,16);StackLoad(1,1076,16);
        b.AddRange(new byte[]{0x31,0xc8,0x25,0xff,0,0,0,0x74,0x06,0x81,0xcb});D(unchecked((int)0x80000000));
        foreach(int p in offsets)
        {
            StackLoad(0,560+p,16);StackLoad(1,1072+p,16);
            b.AddRange(new byte[]{0x39,0xc8,0x74,0x06,0x81,0xcb});D(unchecked((int)0x80000000));
        }
        b.AddRange(new byte[]{0x89,0xd8,0x48,0x81,0xc4});D(1592);
        b.AddRange(new byte[]{0x41,0x5e,0x41,0x5d,0x41,0x5c,0x5b,0xc3});
        return Transform80Publish(b.ToArray());
    }
}
