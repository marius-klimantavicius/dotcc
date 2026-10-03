#!/usr/bin/env python3
"""Check bulk-format preservation, ordinary formatting, and JIT/AOT behavior."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT))
from backend.source_format import format_sources

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    bulk=''.join(f'total = unchecked(total + {index}); // synthetic bulk initialization\n' for index in range(6000))
    source=('using System; public static class Program { static int total; static Program(){\n'+bulk+
            '} static ReadOnlySpan<byte> Data => ['+','.join(str(i%256) for i in range(5000))+'];'+
            'public static int Work(int n){int result=0;for(int i=0;i<n;i++){result+=i;}return result;}'+
            'public static int Main(){int sum=0;foreach(byte value in Data)sum+=value;'+
            f'return total=={sum(range(6000))}&&sum=={sum(i%256 for i in range(5000))}&&Work(10)==45?0:1;}}}}\n')
    (out/'Original.cs').write_text(source)
    formatted,metadata=format_sources(ROOT,{'Program.cs':source});text=formatted['Program.cs']
    (out/'Formatted.cs').write_text(text)
    assert bulk in text, 'synthetic bulk body changed internally'
    assert ','.join(str(i%256) for i in range(5000)) in text, 'numeric token sequence changed'
    assert '    public static int Work(int n)\n    {' in text, 'ordinary method was not formatted'
    verify=out/'verify';verify.mkdir()
    (verify/'Verify.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup><Reference Include="Microsoft.CodeAnalysis"><HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.dll</HintPath></Reference><Reference Include="Microsoft.CodeAnalysis.CSharp"><HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll</HintPath></Reference></ItemGroup></Project>''')
    (verify/'Program.cs').write_text('''using Microsoft.CodeAnalysis; using Microsoft.CodeAnalysis.CSharp;
var before=CSharpSyntaxTree.ParseText(File.ReadAllText(args[0]));var after=CSharpSyntaxTree.ParseText(File.ReadAllText(args[1]));
if(before.GetDiagnostics().Any(x=>x.Severity==DiagnosticSeverity.Error)||after.GetDiagnostics().Any(x=>x.Severity==DiagnosticSeverity.Error))return 1;
return before.GetRoot().DescendantTokens().Select(t=>(t.RawKind,t.Text)).SequenceEqual(after.GetRoot().DescendantTokens().Select(t=>(t.RawKind,t.Text)))?0:2;
''')
    app=out/'app';app.mkdir();(app/'Program.cs').write_text(text)
    (app/'App.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
    records=[]
    def run(label,command):
        result=subprocess.run(list(map(str,command)),capture_output=True,text=True)
        (out/(label+'.stdout.log')).write_text(result.stdout);(out/(label+'.stderr.log')).write_text(result.stderr)
        records.append({'label':label,'command':list(map(str,command)),'exitCode':result.returncode})
        (out/'results.json').write_text(json.dumps({'formatter':metadata,'runs':records},indent=2)+'\n')
        assert result.returncode==0,(label,result.stdout[-1000:],result.stderr[-1000:])
    run('token-identity',['dotnet','run','--project',verify/'Verify.csproj','-c','Release','--',out/'Original.cs',out/'Formatted.cs'])
    run('jit',['dotnet','run','--project',app/'App.csproj','-c','Release'])
    if args.nativeaot:
        run('publish',['dotnet','publish',app/'App.csproj','-c','Release','-p:PublishAot=true','-o',app/'aot'])
        run('aot',[app/'aot/App'])
    print('PASS: exact tokens, compact bulk initialization, formatted source method, runtime behavior')

if __name__=='__main__':main()
