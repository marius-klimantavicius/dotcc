#!/usr/bin/env python3
"""Run the native frontend and preserve its versioned graph and source bytes."""
import argparse,hashlib,json,os,pathlib,subprocess,sys
p=argparse.ArgumentParser();p.add_argument('--compiler',required=True,type=pathlib.Path);p.add_argument('--output',required=True,type=pathlib.Path);p.add_argument('source',type=pathlib.Path);p.add_argument('flags',nargs=argparse.REMAINDER);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[1];exe=a.compiler.resolve();source=a.source.resolve();out=a.output.resolve()
out.mkdir(parents=True,exist_ok=False);(out/'sources').mkdir()
sha=lambda b:hashlib.sha256(b).hexdigest()
flags=a.flags[1:] if a.flags[:1]==['--'] else a.flags
if not any(x.startswith('--c') for x in flags):flags=['--c11' if source.suffix=='.c' else '--c++17',*flags]
isCpp=any(x.startswith('--c++') for x in flags)
instantiation=['--instantiate','used'] if isCpp else []
command=[str(exe),'--clear_flag','use_predefined_macro_file',*instantiation,*flags,str(source)]
env=dict(os.environ,EDG_SEMANTIC_OUTPUT=str(out/'graph.json'))
invocation={'executable':str(exe),'sha256':sha(exe.read_bytes()),'arguments':command[1:],'cwd':str(pathlib.Path.cwd())}
(out/'invocation.json').write_text(json.dumps(invocation,indent=2)+'\n')
with (out/'stdout.log').open('w') as stdout,(out/'stderr.log').open('w') as stderr:
    result=subprocess.run(command,env=env,stdout=stdout,stderr=stderr)
if result.returncode:
    print((out/'stderr.log').read_text(),file=sys.stderr);raise SystemExit(result.returncode)
data=(out/'graph.json').read_bytes();graph=json.loads(data);sources=[];diagnostics=[]
for node in graph['nodes']:
    if node['kind']!='source-file':continue
    spelling=node.get('fullPath') or node.get('path');entry={'node':node['id'],'originalPath':spelling,'encoding':'bytes'}
    candidate=pathlib.Path(spelling)
    if not candidate.is_absolute():candidate=pathlib.Path.cwd()/candidate
    if candidate.is_file():
        content=candidate.read_bytes();name=f"sources/{node['id']}-{candidate.name}";(out/name).write_bytes(content)
        entry.update(path=name,sha256=sha(content),byteLength=len(content),available=True)
    else:
        entry['available']=False;diagnostics.append({'code':'SEM_SOURCE_UNAVAILABLE','source':node['id'],'path':spelling})
    sources.append(entry)
producerPath=exe.parent/'cpfe-semantic.producer.json'
producer=json.loads(producerPath.read_text())
if producer['binarySha256']!=invocation['sha256']:raise SystemExit('Compiler provenance hash mismatch')
(out/'producer.json').write_text(json.dumps(producer,indent=2)+'\n')
inputs=[]
for item in [root/'upstream/src/il_def.h',root/'upstream/src/basics.h',root/'upstream/src/host_envir.h',root/'semantic-export/build.py',root/'semantic-export/native/semantic_export_be.c',root/'semantic-export/native/generate_names.py']:
    inputs.append({'path':str(item.relative_to(root)),'sha256':sha(item.read_bytes())})
manifest={'schema':'edg-semantic-package/1','graph':{'path':'graph.json','sha256':sha(data)},'invocation':invocation,'adapterSourceInputsAtPackaging':inputs,'producer':{'path':'producer.json','sha256':sha((out/'producer.json').read_bytes())},'sources':sources,'diagnostics':diagnostics,'capabilities':{'unlowered':True,'sourceOrder':True,'fullSourcePositions':True,'macroDefinitions':True,'macroInvocations':True,'macroArguments':True,'templateInstantiationPolicy':'used' if isCpp else 'not-applicable','semanticAdapterCoverage':'partial-explicit-diagnostics','codeEmissionReady':False},'sourceSnapshotPolicy':'read-after-frontend; caller must keep inputs immutable during export','locationContract':'EDG sequence/column plus presumed/physical paths, not byte offsets'}
(out/'package.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(f"{out}/package.json ({len(graph['nodes'])} graph nodes, {len(sources)} source files, {len(graph['diagnostics'])} adapter diagnostics)")
