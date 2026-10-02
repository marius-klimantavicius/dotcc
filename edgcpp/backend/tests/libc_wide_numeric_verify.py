"""Wide musl numeric conversion: native narrow oracle and generated width-aware cursor."""
import argparse,hashlib,json,shutil,struct,subprocess,sys
from pathlib import Path
from libc_numeric_verify import corpus,SOURCES

def verify_libc_wide_numeric(root,tests,out,nativeaot,run,translate=True,library_source=None):
    out.mkdir(parents=True,exist_ok=True)
    provider=out/'selected-sources';provider.mkdir();library=provider/'strto'
    source=library_source or root/'backend/runtime/musl'
    for component in ['strto','wide_numeric']:shutil.copytree(source/component,provider/component)
    shutil.copytree(root/'backend/runtime/musl/wide',provider/'wide')
    shutil.copy2(tests/'libc_wide_numeric.c',out/'fixture.c')
    for component in ['strto','wide_numeric']:
      folder=provider/component;manifest=json.loads((folder/'source.json').read_text())
      for name,item in manifest['sources'].items():
        original=folder/(name.replace('.c','.original.c') if component=='strto' else 'originals/'+name)
        assert hashlib.sha256(original.read_bytes()).hexdigest()==item['originalSha256'],name
        assert hashlib.sha256((folder/name).read_bytes()).hexdigest()==item['adaptedSha256'],name
      for name,expected in manifest['adapters'].items():assert hashlib.sha256((folder/name).read_bytes()).hexdigest()==expected,name
    original=corpus();narrow_cases=out/'narrow-cases.txt'
    narrow_cases.write_text(''.join(f'{k} {b} {s.encode().hex()}\n' for k,b,s in original))
    old=root/'backend/runtime/musl/strto'
    run(['gcc','-std=c11','-O2','-fno-inline','-fno-builtin','-frounding-math','-I',old,tests/'libc_numeric.c',*[old/n for n in SOURCES],tests/'libc_numeric_native.c','-lm','-o',out/'narrow-native'])
    run([out/'narrow-native',narrow_cases,out/'narrow.bin'])
    baseline=(out/'narrow.bin').read_bytes();rows=[];expected=bytearray()
    for index,(kind,base,text) in enumerate(original):
      if kind not in [0,1,2,3,4,9,10,11,12]:continue
      bits,signexp,error,consumed=struct.unpack_from('<QIiq',baseline,index*24)
      for mode in range(3):
        for localized in ([0,1024] if kind not in [9,10] else [0]):
          rows.append((kind+(mode<<8)+localized,base,text))
          expected.extend(struct.pack('<QIiq',bits,signexp,error,consumed+(2 if mode==1 and consumed else 0)))
    rows.append((255,0,''));expected.extend(bytes(24))
    cases=out/'cases.txt';cases.write_text(''.join(f'{k} {b} {s.encode().hex()}\n' for k,b,s in rows))
    (out/'golden.bin').write_bytes(expected)
    sources=[out/'fixture.c',*[library/n for n in SOURCES],*[provider/'wide_numeric'/n for n in ['wcstol.c','wcstod.c','wide_locale_wrappers.c']],provider/'wide/iswspace.c',provider/'wide/wcschr.c',provider/'wide/wcslen.c']
    bridge=out/'native-locale.c';bridge.write_text('#include <locale.h>\n#include <errno.h>\nint __edg_locale_validate(locale_t locale){if(locale && locale!=LC_GLOBAL_LOCALE)return 1;errno=EINVAL;return 0;}\n')
    run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-inline','-fno-builtin','-frounding-math','-I',library,*sources,bridge,tests/'libc_numeric_native.c','-lm','-o',out/'native'])
    run([out/'native',cases,out/'native.bin'])
    actual=(out/'native.bin').read_bytes();assert len(actual)==len(expected)
    for index,row in enumerate(rows):
      assert actual[index*24:index*24+24]==expected[index*24:index*24+24],(index,row,struct.unpack_from('<QIiq',actual,index*24),struct.unpack_from('<QIiq',expected,index*24))
    checks=[f'{len(rows)} wide native cases equal narrow expected results plus adjusted Unicode-prefix endptr']
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
    try:checks=verify_libc_wide_numeric(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only,library_source=a.source_dir)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
