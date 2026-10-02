"""Selected musl numeric scanners with native and generated C/C++ acceptance."""
import argparse,hashlib,json,random,shutil,struct,subprocess,sys
from pathlib import Path
SOURCES=['intscan.c','floatscan.c','strtol.c','strtod.c','atoi.c','atol.c','atoll.c','atof.c']

def corpus():
    rows=[]
    integer=['',' ',' +',' -','x','0','-0','00','08','09','0x','0xg','0xffx','-0X10','0123','0b101','123tail',' \t\r\n\v\f-99z',
             '9223372036854775807','9223372036854775808','-9223372036854775808','-9223372036854775809',
             '18446744073709551615','18446744073709551616','-18446744073709551615','-18446744073709551616','zZ9','+000009','9'*300]
    for kind in [0,1,9,10,11,12]:
        for base in [0,2,8,10,16,36,-1,1,37]:
            for s in integer:rows.append((kind,base,s))
    for kind in [5,6,7]:
        for s in ['','abc','  +123x','-2147483648','2147483647','-0',' \t-900z']:rows.append((kind,10,s))
    floating=['',' ','+','-.','0','-0','1','-1','.5','5.','1e','1e+','1e-','1e-2x','0x','0xg','0x.','0x1','0x1p','0x1p+',
        '0x.8p1','-0x1.8p+2x','INF','-infinity','infinite','nan','-nan','nan()','nan(payload)','nan(12_Ab)','nan(bad!)',
        '1,5',' \t\n-3.1415926535897932384626433832795028841971693993751tail',
        '1.000000059604644775390625','1.00000000000000011102230246251565404236316680908203125',
        '0x1p-149','0x1p-150','0x1.000002p-150','0x1.fffffep127','0x1.ffffffp127','0x1.fffffe8p127','0x1p128','-0x1p128',
        '0x1p-1074','0x1p-1075','0x1.0000000000001p-1075','0x1.fffffffffffffp1023','0x1.fffffffffffff7p1023','0x1.fffffffffffff8p1023','0x1p1024','-0x1p1024',
        '0x1p-16445','0x1p-16446','0x1.0000000000000002p-16446','0x1.fffffffffffffffep16383','0x1.fffffffffffffffe8p16383','0x1.ffffffffffffffffp16383','0x1p16384','-0x1p16384',
        '1e-999999999999999999999','-1e99999999999999999999','0e99999999999999999999',
        '1'+('0'*19000)+'e-19000','0.'+('0'*19000)+'1e19000',
        '1'+'2'*19000+'e-19000','9'*19000+'e-19000',
        '1.00000000000000011102230246251565404236316680908203125'+'0'*19000+'1']
    rng=random.Random(77651)
    for i in range(180):floating.append(('-' if i&1 else '')+str(rng.randrange(1,10**30))+'e'+str(rng.randrange(-5100,5100)))
    for kind in [2,3,4,8]:
        for s in floating:rows.append((kind,0,s))
    return rows

def validate_golden(rows,data):
    records={(kind,base,text):struct.unpack_from('<QIiq',data,index*24) for index,(kind,base,text) in enumerate(rows)}
    for kind,tiny,overflow in [(2,'0x1p-149','0x1p128'),(3,'0x1p-1074','0x1p1024'),(4,'0x1p-16445','0x1p16384'),(8,'0x1p-1074','0x1p1024')]:
        r=records[kind,0,tiny];assert r[0]==1 and r[2]==34,(kind,tiny,r)
        for text in [overflow,'-'+overflow]:assert records[kind,0,text][2]==34,(kind,text)
        for text in ['INF','-infinity','nan','-nan','nan(payload)']:assert records[kind,0,text][2]==71,(kind,text)
        for text in ['',' ','+','-.']:assert records[kind,0,text][2]==22,(kind,text)
    for kind in [5,6,7]:
        for text in ['','abc']:assert records[kind,10,text]==(0,0,71,0),(kind,text,records[kind,10,text])
    for kind in [0,9,11]:
        assert records[kind,10,'-9223372036854775808']==(1<<63,0,71,20)
        assert records[kind,10,'9223372036854775808']==((1<<63)-1,0,34,19)
    for kind in [1,10,12]:
        assert records[kind,10,'18446744073709551615']==((1<<64)-1,0,71,20)
        assert records[kind,10,'18446744073709551616']==((1<<64)-1,0,34,20)

def verify_libc_numeric(root,tests,out,nativeaot,run,translate=True,library_source=None):
    out.mkdir(parents=True,exist_ok=True);library=out/'selected-sources';shutil.copytree(library_source or root/'backend/runtime/musl/strto',library);rows=corpus();cases=out/'cases.txt'
    manifest=json.loads((library/'source.json').read_text())
    for name,item in manifest['sources'].items():
        original=library/name.replace('.c','.original.c');adapted=library/name
        assert hashlib.sha256(original.read_bytes()).hexdigest()==item['originalSha256'],name
        assert hashlib.sha256(adapted.read_bytes()).hexdigest()==item['adaptedSha256'],name
        replay=library/name.replace('.c','.replayed.c')
        patch=library/name.replace('.c','.patch')
        if patch.stat().st_size:run(['patch','--silent','--output',replay,original,patch])
        else:shutil.copy2(original,replay)
        assert replay.read_bytes()==adapted.read_bytes(),name
    for name,expected in manifest['adapters'].items():assert hashlib.sha256((library/name).read_bytes()).hexdigest()==expected,name
    cases.write_text(''.join(f'{k} {b} {s.encode().hex()}\n' for k,b,s in rows));sources=[tests/'libc_numeric.c',*[library/n for n in SOURCES]]
    run(['gcc','-std=c11','-O2','-fno-inline','-fno-builtin','-frounding-math','-I',library,*sources,tests/'libc_numeric_native.c','-lm','-o',out/'native'])
    run([out/'native',cases,out/'golden.bin']);assert len((out/'golden.bin').read_bytes())==len(rows)*24;validate_golden(rows,(out/'golden.bin').read_bytes())
    checks=[f'{len(rows)} selected musl native cases']
    if not translate:return checks
    for language in ['c','c++']:
      for instance in [False,True]:
        directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
        run([sys.executable,root/'dotnet.py',*sources,'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc',
             '-I',library,'--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','30000','--output-dir',directory,
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text());method=next(x['method'] for x in manifest['exports'] if x['symbol']=='number_case');host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text('''using System;using System.IO;using System.Runtime.InteropServices;
unsafe class Program {static void Main(string[] args) {
OWNER
byte[] golden=File.ReadAllBytes(args[1]);int index=0;byte* actual=(byte*)NativeMemory.Alloc(24);
try {foreach(string line in File.ReadLines(args[0])) {var p=line.Split(' ');byte[] input=Convert.FromHexString(p[2]);byte* text=(byte*)NativeMemory.Alloc((nuint)input.Length+1);
try {input.CopyTo(new Span<byte>(text,input.Length));text[input.Length]=0;new Span<byte>(actual,24).Fill(0xa5);
CALL(int.Parse(p[0]),(sbyte*)text,int.Parse(p[1]),actual);
if(!new ReadOnlySpan<byte>(actual,24).SequenceEqual(golden.AsSpan(index*24,24)))throw new Exception($"case {index}: {line}: actual={Convert.ToHexString(new ReadOnlySpan<byte>(actual,24))}, expected={Convert.ToHexString(golden.AsSpan(index*24,24))}");
} finally {NativeMemory.Free(text);}++index;}
} finally {NativeMemory.Free(actual);SHUTDOWN}
Console.WriteLine($"{index} selected musl numeric cases passed: exact results, endptr and errno");}}
'''.replace('OWNER','using var owner=new Core();' if instance else '').replace('CALL',('owner.' if instance else 'Core.')+method).replace('SHUTDOWN',('owner' if instance else 'Core')+'.Shutdown();'))
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll',cases,out/'golden.bin']);checks.append(f'{directory.name}: JIT {len(rows)} exact numeric cases')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            run([host/'aot/Host',cases,out/'golden.bin']);checks.append(f'{directory.name}: NativeAOT {len(rows)} exact numeric cases')
    return checks

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',required=True,type=Path);parser.add_argument('--nativeaot',action='store_true');parser.add_argument('--native-only',action='store_true');parser.add_argument('--source-dir',type=Path);a=parser.parse_args()
    out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
    def run(command):
        nonlocal index
        command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,text=True,capture_output=True,timeout=300)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
        if r.returncode:print(r.stdout+r.stderr)
        r.check_returncode()
    try:checks=verify_libc_numeric(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only,library_source=a.source_dir)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
