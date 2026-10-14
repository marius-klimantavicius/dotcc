#!/usr/bin/env python3
"""Adapt a GNU/Linux x86-64 compilation database and export independent semantic objects.

Unsupported semantic options remain explicit; --allow-semantic-delta permits a
best-effort attempt without claiming equivalent metadata. No source is rewritten.
"""
import argparse, concurrent.futures, hashlib, json, os, pathlib, re, shlex, sqlite3, subprocess, time, shutil, resource, signal, sys
ATTEMPT=SOURCE_ROOT=COMPILE_DB=WRAPPER=COMPILER=None
CONTRACT=''
FORMAT=(2,0)
RUN_SNAPSHOT={}
STOP_REQUESTED=False
ENV_KEYS=('CPATH','C_INCLUDE_PATH','CPLUS_INCLUDE_PATH','CFLAGS','CXXFLAGS','CPPFLAGS','CC','CXX')
PREFIXES=('__SSE','__AVX','__MMX','__FMA','__BMI','__LZCNT','__POPCNT','__AES','__PCLMUL','__GCC_HAVE_SYNC_COMPARE_AND_SWAP','__CRC32','__FXSR','__XSAVE','__x86_64','__amd64','__tune_','__INT','__UINT','__FLT','__DBL','__LDBL','__DEC','__SIZEOF','__ATOMIC','__GCC_ATOMIC','__LONG','__WCHAR','__WINT','__SIG_ATOMIC','__PTRDIFF','__SIZE_MAX','__SIZE_WIDTH','__SHRT','__SSSE3','__FP_FAST_FMA')
NAMES={'__CHAR_BIT__','__SCHAR_MAX__','__OPTIMIZE__','__PIC__','__pic__','__PIE__','__pie__','__STRICT_ANSI__','__MOVBE__','__LAHF_SAHF__','__F16C__','__BIGGEST_ALIGNMENT__'}
def write(p,obj):
 tmp=p.with_suffix(p.suffix+'.new');tmp.write_text(json.dumps(obj,indent=2)+'\n');tmp.replace(p)
def sha(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for x in iter(lambda:f.read(4*1024*1024),b''):h.update(x)
 return h.hexdigest()
def category(e):
 f=e['file'];o=e.get('output','')
 if '/test/' in f or '/test/' in o:return 'tests'
 if '/tools/shell/' in f:return 'shell'
 if '/extension/' in f or o.startswith('extension/'):return 'bundled_extensions'
 if '/third_party/' in f:return 'third_party'
 if '/src/' in f:return 'library'
 return 'tools_or_generated'
def adapt(e,d):
 raw=e.get('arguments') or shlex.split(e['command']);lang='c' if pathlib.Path(e['file']).suffix=='.c' else 'c++'
 host=[raw[0]];forward=[];inc=[];std=None;unsupported=[];host_only=[];i=1
 while i<len(raw):
  a=raw[i]
  if a in ('-o','-c'):
   i+=2 if a=='-o' else 1;continue
  if a==e['file']:i+=1;continue
  host.append(a)
  if a in ('-I','-D','-U'):
   i+=1;host.append(raw[i]);forward.extend([a,raw[i]])
   if a=='-I':inc.append(raw[i])
  elif a.startswith(('-D','-U','-I')):
   forward.append(a)
   if a.startswith('-I'):inc.append(a[2:])
  elif a.startswith('-std='):std=a[5:]
  elif a in ('-fno-exceptions','-fexceptions','-fno-rtti','-frtti'):forward.append({'-fno-exceptions':'--no_exceptions','-fexceptions':'--exceptions','-fno-rtti':'--no_rtti','-frtti':'--rtti'}[a])
  elif a.startswith('-fvisibility'):unsupported.append(a)
  elif a.startswith(('-O','-W','-fPIC','-fPIE','-ffunction-sections','-fdata-sections','-march=','-mtune=')) or a=='-w':host_only.append(a)
  else:unsupported.append(a)
  i+=1
 macro_cmd=[*host,'-dM','-E','-x',lang,'-'];p=subprocess.run(macro_cmd,input='',capture_output=True,text=True,cwd=e['directory'],check=True)
 (d/'host-predefines.txt').write_text(p.stdout)
 include_cmd=[*host,'-E','-v','-x',lang,'-'];v=subprocess.run(include_cmd,input='',capture_output=True,text=True,cwd=e['directory'],check=True)
 (d/'host-include-search.txt').write_text(v.stderr)
 sysinc=[];inside=False
 for line in v.stderr.splitlines():
  if '#include <...> search starts here:' in line:inside=True;continue
  if 'End of search list.' in line:inside=False
  if inside and line.startswith(' ') and line.strip() not in inc:sysinc.append(line.strip())
 macros={};extra=[]
 for line in p.stdout.splitlines():
  m=re.match(r'#define (\w+) (.*)',line)
  if m:
   n,value=m.groups();macros[n]=value
   if n in NAMES or n.startswith(PREFIXES):extra.append('-D'+n+'='+value)
 # Capture every actual -march/-mtune predefined-macro delta, not guessed ISA names.
 arch_cmd=[x for x in macro_cmd if not x.startswith(('-march=','-mtune='))]
 arch_probe=subprocess.run(arch_cmd,input='',capture_output=True,text=True,cwd=e['directory'],check=True)
 (d/'host-architecture-baseline.txt').write_text(arch_probe.stdout)
 base=dict(re.findall(r'^#define (\w+) (.*)$',arch_probe.stdout,re.M))
 extra_names={x[2:].split('=',1)[0] for x in extra}
 for n,v in macros.items():
  if base.get(n)!=v and n not in extra_names:extra.append('-D'+n+'='+v)
 for n in base:
  if n not in macros:extra.append('-U'+n)
 if '__x86_64__' not in macros or macros.get('__SIZEOF_POINTER__')!='8':unsupported.append('unmapped host ABI; supported adapter is GNU/Linux x86-64')
 if '__clang__' in macros:unsupported.append('Clang emulation is outside this GNU adapter')
 gnu_version=10000*int(macros['__GNUC__'])+100*int(macros['__GNUC_MINOR__'])+int(macros['__GNUC_PATCHLEVEL__'])
 if lang=='c++':
  if std not in ('c++17','gnu++17'):unsupported.append('unsupported C++ dialect '+str(std))
  mode=['--g++','--c++17']
  if std=='gnu++17':mode.append('--no_strict_gnu')
 else:
  if std not in ('gnu99',None):unsupported.append('unsupported C dialect '+str(std))
  if std is None and macros.get('__STDC_VERSION__')!='201710L':unsupported.append('unsupported default C language version '+str(macros.get('__STDC_VERSION__')))
  mode=['--gcc','--no_strict_gnu','--c99' if std=='gnu99' else '--c17']
 argv=['python3',str(WRAPPER),'-o',str(d/'unit.roslynobj'),'--timing',*mode,'--gnu_version',str(gnu_version),*forward,*extra]
 for x in sysinc:argv.extend(['--sys_include',x])
 argv.append(e['file'])
 unity=[]
 if pathlib.Path(e['file']).name.startswith('ub_'):
  unity=re.findall(r'^#include\s+"([^"]+)"',pathlib.Path(e['file']).read_text(),re.M)
 return {'configured_entry':e,'language':lang,'effective_host_standard':std or 'GNU default '+str(macros.get('__STDC_VERSION__')),'host_macro_command':macro_cmd,'host_include_command':include_cmd,'host_compiler_only_flags':host_only,'unsupported_semantic_flags':unsupported,'forwarded_host_predefines':extra,'architecture':'actual host -march/-mtune predefined delta; EDG Linux x86-64 target','architecture_adaptation_version':2,'architecture_baseline_command':arch_cmd,'argv':argv,'cwd':e['directory'],'unity_included_sources':unity}
def prepare(indices=None,provenance=None):
 out=ATTEMPT
 if (out/'manifest.json').exists():raise RuntimeError('Attempt already prepared; use run/resume, or choose a new attempt root')
 out.mkdir(parents=True,exist_ok=True)
 entries=json.loads(COMPILE_DB.read_text());unique={};items=[]
 env={k:os.environ.get(k) for k in ENV_KEYS}
 for idx,e in enumerate(entries):
  if indices is not None and idx not in indices:continue
  tokens=e.get('arguments') or shlex.split(e['command']);key=json.dumps([e['directory'],tokens,env],sort_keys=True)
  if key in unique:
   items[unique[key]]['original_entry_indices'].append(idx);continue
  pos=len(items);unique[key]=pos;d=out/f'{idx:04d}';d.mkdir(exist_ok=True)
  recipe=adapt(e,d);write(d/'recipe.json',recipe)
  items.append({'entry_index':idx,'original_entry_indices':[idx],'source':e['file'],'source_sha256':sha(pathlib.Path(e['file'])),'recipe_sha256':sha(d/'recipe.json'),'directory':e['directory'],'target_output':e.get('output'),'category':category(e),'language':recipe['language'],'recipe':str(d/'recipe.json'),'result':str(d/'result.json'),'artifact':str(d/'unit.roslynobj'),'unsupported_semantic_flags':recipe['unsupported_semantic_flags']})
  if idx%100==0:print('PREPARED',idx,flush=True)
 manifest={'source_revision':subprocess.check_output(['git','-C',str(SOURCE_ROOT),'rev-parse','HEAD'],text=True).strip(),'configured_entries':len(entries),'distinct_sources':len({e['file'] for e in entries}),'distinct_invocations':len(items),'deduplication':'exact directory + full tokenized configured command + relevant compiler environment only','compiler_environment':env,'compile_commands_sha256':sha(COMPILE_DB),'driver_sha256':sha(pathlib.Path(__file__)),'contract':CONTRACT,'format_major':FORMAT[0],'format_minor':FORMAT[1],'compile_database':str(COMPILE_DB),'source_root':str(SOURCE_ROOT),'project_provenance':json.loads(provenance.read_text()) if provenance else None,'items':items}
 write(out/'manifest.json',manifest);return manifest
def run_one(item,allow_delta=False):
 p=pathlib.Path(item['recipe']);d=p.parent
 if sha(p)!=item['recipe_sha256']:raise RuntimeError('Prepared recipe changed')
 if sha(pathlib.Path(item['source']))!=item['source_sha256']:raise RuntimeError('Prepared translation unit changed')
 recipe=json.loads(p.read_text());unsupported=recipe['unsupported_semantic_flags'];start=time.monotonic()
 result={'producer_sha256':RUN_SNAPSHOT['compiler_sha256'],'process_stack_limit_bytes':resource.getrlimit(resource.RLIMIT_STACK)[0],'recipe_sha256':sha(p),'entry_index':item['entry_index'],'source':item['source'],'category':item['category'],'semantic_equivalent':not unsupported,'unsupported_semantic_flags':unsupported}
 if unsupported and not allow_delta:
  result.update(status='frontend_configuration_unsupported',exit_code=None,reason='No supported EDG mapping for configured semantic options; no compiler invocation performed.')
 else:
  with (d/'compiler.log').open('w') as log:
   r=subprocess.run(['/usr/bin/time','-v','-o',str(d/'time.txt'),*recipe['argv']],stdout=log,stderr=subprocess.STDOUT,cwd=recipe['cwd'],env={**os.environ,'ROSLYNOBJ_COMPILER':RUN_SNAPSHOT['compiler']})
  log=(d/'compiler.log').read_text(errors='replace');result.update(exit_code=r.returncode,combined_wall_seconds=time.monotonic()-start,log=str(d/'compiler.log'),time_log=str(d/'time.txt'))
  timing={}
  for name,cpu,elapsed in re.findall(r'(Front end|Back end|Total compilation) time\s+([\d.]+) \(CPU\)\s+([\d.]+) \(elapsed\)',log):timing[name]={'cpu_seconds':float(cpu),'elapsed_seconds_rounded_stock':float(elapsed)}
  result['stock_timing']=timing
  if r.returncode==245:result['native_signal_candidate']=-11
  t=(d/'time.txt').read_text();m=re.search(r'Maximum resident set size \(kbytes\): (\d+)',t);result['max_rss_kib']=int(m.group(1)) if m else None
  artifact=d/'unit.roslynobj'
  if r.returncode==0 and artifact.is_file():
   try:
    c=sqlite3.connect('file:'+str(artifact)+'?mode=ro',uri=True)
    header=c.execute('select complete,format_major,format_minor from object_header').fetchone()
    complete=header[:1]
    contract=c.execute("select v.value from build_capability b join text_atom n on n.id=b.name_id join text_atom v on v.id=b.value_id where n.value='normalized_semantic_schema_v1'").fetchone()
    if complete!=(1,) or header[1:]!=FORMAT or contract!=(CONTRACT,):raise RuntimeError('complete/contract admission mismatch')
    result.update(status='published' if not unsupported else 'published_with_semantic_delta',artifact=str(artifact),bytes=artifact.stat().st_size,sha256=sha(artifact),complete=1,contract=contract[0],format_major=header[1],format_minor=header[2]);c.close()
   except Exception as exc:result.update(status='publication_admission_failure',reason=str(exc))
  else:
   status='backend_extraction_or_publication_failure' if 'roslynobj export error' in log or ('Front end time' in log and not re.search(r'\d+ errors? detected',log)) else 'frontend_configuration_or_source_failure'
   if r.returncode==245:status='backend_crash_after_frontend' if 'Front end' in timing else 'compiler_crash_unlocated'
   result.update(status=status,diagnostic_tail=log[-4000:])
 write(d/'result.json',result);return result
def initialize_snapshot():
 path=ATTEMPT/'producer-snapshot.json'
 if path.exists():
  snapshot=json.loads(path.read_text())
  if sha(pathlib.Path(snapshot['compiler']))!=snapshot['compiler_sha256']:raise RuntimeError('Frozen compiler changed')
  if snapshot['contract']!=CONTRACT or tuple(snapshot['format'])!=FORMAT:raise RuntimeError('Attempt format/contract mismatch')
  return snapshot
 producer=ATTEMPT/'producer';producer.mkdir(exist_ok=True)
 digest=sha(COMPILER);frozen=producer/(digest+'-roslyn-cpfe')
 if frozen.exists() and sha(frozen)!=digest:raise RuntimeError('Content-addressed compiler mismatch')
 if not frozen.exists():shutil.copy2(COMPILER,frozen)
 snapshot={'compiler':str(frozen),'compiler_sha256':digest,'contract':CONTRACT,'format':list(FORMAT),'driver_sha256':sha(pathlib.Path(__file__))}
 write(path,snapshot);return snapshot

def run(manifest,jobs,allow_delta):
 global RUN_SNAPSHOT,STOP_REQUESTED
 RUN_SNAPSHOT=initialize_snapshot()
 if sha(COMPILE_DB)!=manifest['compile_commands_sha256']:raise RuntimeError('Compilation database changed')
 if {k:os.environ.get(k) for k in ENV_KEYS}!=manifest['compiler_environment']:raise RuntimeError('Compiler environment changed')
 write(ATTEMPT/('run-config-'+str(time.time_ns())+'.json'),{'jobs':jobs,'process_stack_limit_bytes':resource.getrlimit(resource.RLIMIT_STACK)[0],'driver_sha256':sha(pathlib.Path(__file__)),'allow_semantic_delta':allow_delta})
 if manifest['contract']!=CONTRACT or (manifest['format_major'],manifest['format_minor'])!=FORMAT:raise RuntimeError('Prepared manifest format/contract mismatch')
 results=[];pending=[]
 for item in manifest['items']:
  p=pathlib.Path(item['result'])
  if p.exists():
   r=json.loads(p.read_text())
   if r['status'].startswith('published') and sha(pathlib.Path(r['artifact']))!=r['sha256']:raise RuntimeError('Existing published artifact changed')
   results.append(r)
  else:pending.append(item)
 def stop(signum,frame):
  global STOP_REQUESTED
  STOP_REQUESTED=True;print('Stop requested: finish active outputs before exiting',flush=True)
 signal.signal(signal.SIGTERM,stop);signal.signal(signal.SIGINT,stop)
 iterator=iter(pending);active={};pool=concurrent.futures.ThreadPoolExecutor(max_workers=jobs)
 def dispatch():
  if STOP_REQUESTED:return
  try:item=next(iterator)
  except StopIteration:return
  active[pool.submit(run_one,item,allow_delta)]=item
 for i in range(jobs):dispatch()
 while active:
  done,_=concurrent.futures.wait(active,timeout=1,return_when=concurrent.futures.FIRST_COMPLETED)
  for future in done:
   item=active.pop(future)
   try:r=future.result()
   except Exception as exc:
    r={'entry_index':item['entry_index'],'status':'driver_failure','reason':repr(exc)};write(pathlib.Path(item['result']),r)
   results.append(r);write(ATTEMPT/'results-progress.json',sorted(results,key=lambda x:x['entry_index']))
   print('DONE',len(results),'/',len(manifest['items']),r['entry_index'],r['status'],flush=True);dispatch()
 pool.shutdown(wait=True)
 write(ATTEMPT/('results-stopped.json' if STOP_REQUESTED else 'results-final.json'),sorted(results,key=lambda x:x['entry_index']))
 write(ATTEMPT/'dispatch-final.json',{'selected_indices':[x['entry_index'] for x in manifest['items']],'completed_indices':[x['entry_index'] for x in results],'stopped':STOP_REQUESTED,'jobs':jobs,'process_stack_limit_bytes':resource.getrlimit(resource.RLIMIT_STACK)[0]})

def select_attempts(manifest_paths, legacy_indices, output):
 manifests=[json.loads(p.read_text()) for p in manifest_paths]
 configured=manifests[0]['configured_entries']
 if any(m['configured_entries']!=configured or m['compile_commands_sha256']!=manifests[0]['compile_commands_sha256'] for m in manifests):raise RuntimeError('Attempt compilation databases differ')
 maps=[{index:x for x in m['items'] for index in x['original_entry_indices']} for m in manifests]
 indices=range(configured);published=[];failed=[];pending=[]
 for idx in sorted(indices):
  chosen=None
  for position in reversed(range(len(manifests))):
   if position==0 and len(manifests)>1 and idx not in legacy_indices:continue
   item=maps[position].get(idx)
   if not item:continue
   path=pathlib.Path(item['result'])
   if path.exists():chosen=(position,item,json.loads(path.read_text()));break
  if chosen is None:pending.append(idx);continue
  position,item,r=chosen
  if not r['status'].startswith('published'):
   failed.append({'entry_index':idx,'status':r['status'],'result':item['result'],'log':r.get('log'),'exit_code':r.get('exit_code')});continue
  artifact=pathlib.Path(r['artifact'])
  if artifact.stat().st_size!=r['bytes']:raise RuntimeError('Published artifact size changed')
  c=sqlite3.connect(artifact.resolve().as_uri()+'?mode=ro',uri=True)
  header=c.execute('select complete,format_major,format_minor from object_header').fetchone()
  cap=c.execute("select v.value from build_capability b join text_atom n on n.id=b.name_id join text_atom v on v.id=b.value_id where n.value='normalized_semantic_schema_v1'").fetchone();c.close()
  manifest=manifests[position]
  if 'format_major' in manifest:expected_format=(manifest['format_major'],manifest['format_minor'])
  elif manifest['contract']=='4bb94e5feda94e85fe4b6fe4e8eb5f3494f3b112d14a56dc71e6b069861a2559':expected_format=(1,0)
  else:raise RuntimeError('Unknown legacy manifest format')
  if header[0]!=1 or header[1:]!=expected_format or cap!=(manifest['contract'],):raise RuntimeError('Published numeric format/header/contract mismatch')
  recipe=json.loads(pathlib.Path(item['recipe']).read_text())
  published.append({'entry_index':idx,'original_entry_indices':item['original_entry_indices'],'path':str(artifact),'source':item['source'],'source_sha256':item['source_sha256'],'category':item['category'],'format_major':header[1],'format_minor':header[2],'contract':cap[0],'bytes':r['bytes'],'sha256':r['sha256'],'result':item['result'],'recipe':item['recipe'],'semantic_equivalent':not recipe['unsupported_semantic_flags'],'unsupported_semantic_flags':recipe['unsupported_semantic_flags'],'selected_attempt_manifest':str(manifest_paths[position])})
 result={'configured_entries':configured,'distinct_sources':manifests[0]['distinct_sources'],'selected_published':len(published),'unique_objects':len({x['path'] for x in published}),'failed':len(failed),'pending':len(pending),'published_objects':published,'failures':failed,'pending_indices':pending,'selection':'Newest completed attempt wins; legacy attempt eligible only for explicit approved reuse indices. No FK or graph rescans.'}
 write(output,result);print(json.dumps({k:v for k,v in result.items() if k not in ('published_objects','failures','pending_indices')}))

if __name__=='__main__':
 if len(sys.argv)>1 and sys.argv[1]=='select':
  a=argparse.ArgumentParser(description='Select immutable completed objects and retain failures/pending entries')
  a.add_argument('--manifest',type=pathlib.Path,action='append',required=True);a.add_argument('--legacy-reuse',type=pathlib.Path,required=True);a.add_argument('--output',type=pathlib.Path,required=True)
  args=a.parse_args(sys.argv[2:]);reuse=json.loads(args.legacy_reuse.read_text())['legacy_reuse_indices'];select_attempts(args.manifest,set(reuse),args.output);sys.exit(0)
 a=argparse.ArgumentParser(description=__doc__)
 a.add_argument('--compile-db',type=pathlib.Path,required=True);a.add_argument('--source-root',type=pathlib.Path,required=True);a.add_argument('--attempt-root',type=pathlib.Path,required=True)
 a.add_argument('--compiler',type=pathlib.Path,required=True);a.add_argument('--wrapper',type=pathlib.Path,default=pathlib.Path(__file__).with_name('roslyn-cpfe.py'))
 a.add_argument('--contract',required=True);a.add_argument('--format-major',type=int,default=2);a.add_argument('--format-minor',type=int,default=0)
 a.add_argument('--prepare',action='store_true');a.add_argument('--run',action='store_true');a.add_argument('--indices',type=int,nargs='+');a.add_argument('--jobs',type=int,choices=range(1,9),default=1);a.add_argument('--stack-mib',type=int,default=64)
 a.add_argument('--allow-semantic-delta',action='store_true');a.add_argument('--project-provenance',type=pathlib.Path)
 args=a.parse_args();ATTEMPT=args.attempt_root.resolve();SOURCE_ROOT=args.source_root.resolve();COMPILE_DB=args.compile_db.resolve();COMPILER=args.compiler.resolve();WRAPPER=args.wrapper.resolve();CONTRACT=args.contract;FORMAT=(args.format_major,args.format_minor)
 if not args.prepare and not args.run:a.error('Choose --prepare and/or --run')
 if args.stack_mib<=0:a.error('Stack limit must be positive')
 resource.setrlimit(resource.RLIMIT_STACK,(args.stack_mib*1024*1024,resource.getrlimit(resource.RLIMIT_STACK)[1]))
 manifest=prepare(set(args.indices) if args.indices is not None else None,args.project_provenance) if args.prepare else json.loads((ATTEMPT/'manifest.json').read_text())
 if args.run:run(manifest,args.jobs,args.allow_semantic_delta)
