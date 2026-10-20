#!/usr/bin/env python3
"""Native-backed raw/processed JIT/AOT qualification of direct intrinsic calls."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys
import xml.etree.ElementTree as ET

FIXTURE = Path(__file__).resolve().parent
REPO = FIXTURE.parents[2]
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import selected_tool, subprocess, tool_manifest


def rules():
    entries = [('read_native16','uint16_t',['const void *'],'load.u16.native'),
        ('read_be32','uint32_t',['const void *'],'load.u32.be'),
        ('read_le64','uint64_t',['const void *'],'load.u64.le'),
        ('write_le16','void',['void *','uint16_t'],'store.u16.le'),
        ('write_be32','void',['void *','uint32_t'],'store.u32.be'),
        ('write_native64','void',['void *','uint64_t'],'store.u64.native'),
        ('swap32','uint32_t',['uint32_t'],'bswap.u32'),('swap64','uint64_t',['uint64_t'],'bswap.u64'),
        ('clz32','unsigned',['uint32_t'],'clz.u32'),('ctz32','unsigned',['uint32_t'],'ctz.u32'),
        ('clz64','unsigned',['uint64_t'],'clz.u64'),('ctz64','unsigned',['uint64_t'],'ctz.u64'),
        ('highbit','unsigned',['uint32_t'],'highbit.u32'),('commonbytes','unsigned',['uint64_t'],'commonbytes.u64'),
        ('little_unsigned','unsigned',[],'is-little-endian.u32'),('little_signed','int',[],'is-little-endian.i32')]
    return [dict(name=name, signature=dict(returnType=result,parameterTypes=parameters,variadic=False),
        target=dict(kind='intrinsic',name=target),declarationFile='selected.h',requireMatch=True)
        for name,result,parameters,target in entries]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def caller_body(text, name):
    # Mask strings/comments without moving offsets, then balance method braces.
    mask = re.sub(r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"', lambda m:' '*len(m.group()), text, flags=re.S)
    match = re.search(r'\b'+re.escape(name)+r'\([^)]*\)\s*\{',mask)
    if not match:
        raise RuntimeError('Missing actual caller '+name)
    start=match.end()-1; depth=0
    for index in range(start,len(mask)):
        depth += (mask[index]=='{')-(mask[index]=='}')
        if depth==0:
            return text[start:index+1]
    raise RuntimeError('Unterminated actual caller '+name)


def inspect(project):
    text='\n'.join(p.read_text() for p in project.parent.glob('*.cs'))
    observations=[]
    for name, required in [('check_memory',['ReadUnaligned<ushort>','ReadUInt32BigEndian','ReadUInt64LittleEndian','WriteUnaligned<ulong>']),
            ('check_conversions',['LeadingZeroCount','TrailingZeroCount','ReverseEndianness','BitConverter.IsLittleEndian']),
            ('check_contexts',['LeadingZeroCount','TrailingZeroCount']),('underscore_probe',['LeadingZeroCount','TrailingZeroCount'])]:
        body=caller_body(text,name)
        if any(token not in body for token in required):
            raise RuntimeError('Missing BCL call in actual caller '+name)
        for rule in rules():
            if re.search(r'\b'+rule['name']+r'\s*\(',body):
                raise RuntimeError('Actual caller still invokes C intrinsic wrapper: '+name+'/'+rule['name'])
        observations.append(dict(method=name,body_sha256=hashlib.sha256(body.encode()).hexdigest(),required=required))
    return observations


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path,required=True)
    parser.add_argument('--rid',default='linux-x64')
    args=parser.parse_args(); output=args.output.resolve()
    if output.exists(): raise RuntimeError('Output must be fresh')
    output.mkdir(parents=True); source=output/'source'; source.mkdir()
    for name in ('operations.h','reference.c','main.c','underscore.c'): shutil.copyfile(FIXTURE/name,source/name)
    shutil.copyfile(source/'operations.h',source/'selected.h')
    (source/'overrides.json').write_text(json.dumps(dict(version=1,functionOverrides=rules()),indent=2)+'\n')
    compiler=selected_tool(); postprocessor=selected_tool('postprocessor')
    report=dict(status='running',commands=[],cells=[],sources={name:sha(source/name) for name in ('operations.h','reference.c','main.c','underscore.c','selected.h','overrides.json')},tools={str(tool):tool_manifest(tool.parent) for tool in (compiler,postprocessor)},structural={})
    report_path=output/'report.json'
    def save(): report_path.write_text(json.dumps(report,indent=2)+'\n')
    def run(label,command,timeout=600):
        command=list(map(str,command)); row=dict(label=label,command=command); report['commands'].append(row);save()
        result=subprocess.run(command,capture_output=True,text=True,timeout=timeout)
        log=output/(label+'.log');log.write_text(result.stdout+result.stderr)
        row.update(actual_command=result.args,exit_code=result.returncode,log=str(log),log_sha256=sha(log));save()
        if result.returncode: raise RuntimeError(label+' failed: '+str(log))
        return result.stdout
    try:
        for case in ('main','underscore'):
            binary=output/('native-'+case)
            run('native-'+case+'-build',['cc','-std=c11','-O2','-DINTRINSIC_NATIVE_MAIN=1',source/'reference.c',source/(case+'.c'),'-o',binary])
            run('native-'+case+'-run',[binary])
        objects=[]
        for name in ('reference','main','underscore'):
            obj=output/(name+'.o');selection=output/(name+'-overrides.jsonl')
            run(name+'-emit',['dotnet',compiler,'-std=c11','--overrides-file',source/'overrides.json','--override-report',selection,source/(name+'.c'),'--emit=obj','-o',obj])
            selected=[json.loads(line) for line in selection.read_text().splitlines() if line]
            selected=[row for row in selected if row.get('event')=='function-override']
            if len(selected)!=16 or {row['name'] for row in selected}!={rule['name'] for rule in rules()}:
                raise RuntimeError('Incomplete fixture selection '+name)
            objects.append(obj)
        raw=output/'raw/IntrinsicProbe'
        run('link',['dotnet',compiler,*objects,'--emit=managedlib','--runtime=c','--nest-types','--class-name','Api','--namespace','Probe.FunctionCalls','--split=function','-o',raw])
        raw_project=next(raw.glob('*.csproj')); products={'raw':raw_project}
        run('raw-build',['dotnet','build',raw_project,'-c','Release','-m:1','--nologo'])
        processed=output/'processed/IntrinsicProbe';shutil.copytree(raw,processed,ignore=shutil.ignore_patterns('bin','obj'))
        processed_project=processed/raw_project.name;products['processed']=processed_project
        run('processed-context-build',['dotnet','build',processed_project,'-c','Release','-m:1','--nologo'])
        run('postprocess',['dotnet',postprocessor,processed_project,'--in-place'],900)
        runner=output/'runner';runner.mkdir()
        runner_project=runner/'IntrinsicTests.csproj'
        runner_project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>14</LangVersion><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><IsAotCompatible>true</IsAotCompatible><PrimitiveProject>MISSING</PrimitiveProject></PropertyGroup><ItemGroup><ProjectReference Include="$(PrimitiveProject)" /></ItemGroup></Project>')
        (runner/'Program.cs').write_text('''using VM=Probe.FunctionCalls.Api;
using var owner=new VM.Libc.RuntimeContext();using var binding=owner.Enter();
int result=VM.probe();if(result!=0)throw new Exception("C fixture failed at line "+result);
if(VM.underscore_probe()!=1)throw new Exception("underscore variable changed");
Console.WriteLine("intrinsic-calls: ok;underscore: preserved99");
''')
        for form,project in products.items():
            report['structural'][form]=inspect(project);save()
            for mode in ('jit','aot'):
                label=form+'-'+mode; binaries=output/label
                common=['-c','Release','-m:1','--nologo','-p:PrimitiveProject='+str(project),'--artifacts-path',output/'intermediates'/label,'-o',binaries]
                if mode=='jit':
                    run(label+'-build',['dotnet','build',runner_project,*common]); command=['dotnet',binaries/'IntrinsicTests.dll']
                else:
                    run(label+'-publish',['dotnet','publish',runner_project,*common,'-r',args.rid,'-p:PublishAot=true'],1800);command=[binaries/'IntrinsicTests']
                transcript=run(label+'-run',command)
                if transcript.strip()!='intrinsic-calls: ok;underscore: preserved99':raise RuntimeError('Unexpected managed fixture output')
                report['cells'].append(dict(form=form,mode=mode,status='passed'));save()
        report['status']='passed'
    except BaseException as error:
        report.update(status='failed',error=repr(error));raise
    finally:save()
    print(report_path)

if __name__=='__main__':main()
