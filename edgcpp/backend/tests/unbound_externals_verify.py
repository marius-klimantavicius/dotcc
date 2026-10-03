#!/usr/bin/env python3
"""Verify explicit review declarations fail loudly; default still rejects."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[2]

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=ROOT/'build/bin/cpfe-dot')
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    commands=[]
    def run(label,command,expected=0):
        result=subprocess.run(list(map(str,command)),text=True,capture_output=True)
        (out/f'{label}.stdout.log').write_text(result.stdout);(out/f'{label}.stderr.log').write_text(result.stderr)
        commands.append({'label':label,'command':list(map(str,command)),'exitCode':result.returncode})
        (out/'results.json').write_text(json.dumps(commands,indent=2)+'\n')
        if expected is None:
            assert result.returncode!=0
        else:assert result.returncode==expected, (label,result.stdout[-1000:],result.stderr[-1000:])
        return result
    base=[sys.executable,ROOT/'dotnet.py',ROOT/'backend/tests/unbound_externals.cpp',
          '--compiler',args.compiler.resolve(),'--emit','managedlib','--namespace','Review','--class-name','Core']
    rejected=run('default-reject',[*base,'--output-dir',out/'rejected'],expected=None)
    assert 'unsupported external call' in rejected.stderr or 'address of external function' in rejected.stderr
    for instance in (False,True):
        label='instance' if instance else 'static';directory=out/label
        run(label+'-translate',[*base,'--unbound-externals','declarations','--split','function',
                               *(['--instance-methods'] if instance else []),'--output-dir',directory])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['unboundExternals']['reviewOnly']
        assert {item['symbol'] for item in manifest['unboundExternals']['symbols']}=={'unavailable_function','unavailable_variable'}
        receiver='owner' if instance else 'Review.Core'
        host=('using System; unsafe class Program { static int Main() { '+('var owner=new Review.Core();' if instance else '')+
              f'if ({receiver}.ordinary_function()!=42) return 1; int caught=0;'+
              ''.join(f'try {{ {receiver}.{name}(); return 2; }} catch (PlatformNotSupportedException) {{ caught++; }}'
                      for name in ('call_unavailable','indirect_unavailable','read_unavailable','address_unavailable','read_constant_address'))+
              'return caught==5?0:3; } }')
        (directory/'Host.cs').write_text(host)
        project=directory/'Generated.csproj';text=project.read_text().replace('<OutputType>Library</OutputType>','<OutputType>Exe</OutputType>')
        assert '<OutputType>Exe</OutputType>' in text
        project.write_text(text.replace('</ItemGroup>','<Compile Include="Host.cs" /></ItemGroup>',1))
        run(label+'-build',['dotnet','build',project,'-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(label+'-jit',['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        if args.nativeaot:
            run(label+'-publish',['dotnet','publish',project,'-c','Release','--nologo','-p:PublishAot=true','-o',directory/'aot'])
            run(label+'-aot',[directory/'aot/Generated'])
    print('PASS: default rejects; static/instance external calls, addresses and variables throw explicitly')

if __name__=='__main__':main()
