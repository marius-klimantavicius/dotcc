#!/usr/bin/env python3
"""Export the untouched official SQLite amalgamation with a fixed feature set."""
import argparse, hashlib, json, os, pathlib, re, resource, sqlite3, subprocess, time

BUILD = pathlib.Path(__file__).resolve().parent
FEATURES = ('FTS4','FTS5','RTREE','GEOPOLY','SESSION','PREUPDATE_HOOK','RBU',
            'DBSTAT_VTAB','DBPAGE_VTAB','STMTVTAB','BYTECODE_VTAB','MATH_FUNCTIONS',
            'COLUMN_METADATA','STAT4','SNAPSHOT','CARRAY','NORMALIZE','UNLOCK_NOTIFY')
PREFIXES = ('__SSE','__AVX','__MMX','__FMA','__BMI','__LZCNT','__POPCNT','__AES',
 '__PCLMUL','__GCC_HAVE_SYNC_COMPARE_AND_SWAP','__CRC32','__FXSR','__XSAVE',
 '__x86_64','__amd64','__tune_','__INT','__UINT','__FLT','__DBL','__LDBL','__DEC',
 '__SIZEOF','__ATOMIC','__GCC_ATOMIC','__LONG','__WCHAR','__WINT','__SIG_ATOMIC',
 '__PTRDIFF','__SIZE_MAX','__SIZE_WIDTH','__SHRT','__SSSE3','__FP_FAST_FMA')
NAMES = {'__CHAR_BIT__','__SCHAR_MAX__','__OPTIMIZE__','__PIC__','__pic__',
         '__PIE__','__pie__','__STRICT_ANSI__','__MOVBE__','__LAHF_SAHF__',
         '__F16C__','__BIGGEST_ALIGNMENT__'}

def write(path, data):
    path.write_text(json.dumps(data, indent=2) + '\n')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', required=True, type=pathlib.Path, help='Fresh official 3.54.0 package root with source/producer provenance JSON')
    parser.add_argument('--attempt-root', type=pathlib.Path, help='New immutable output directory (default: ROOT/export)')
    args = parser.parse_args()
    ROOT = args.root.resolve()
    provenance = json.loads((ROOT/'source-provenance.json').read_text())
    producer = json.loads((ROOT/'producer-provenance.json').read_text())
    assert provenance['version'] == '3.54.0'
    source = ROOT/'source/sqlite-amalgamation-3540000/sqlite3.c'
    assert sha(source) == provenance['source_file_sha256'][str(source.relative_to(ROOT))]
    assert sha(pathlib.Path(producer['compiler'])) == producer['compiler_sha256']
    attempt = (args.attempt_root or ROOT/'export').resolve();attempt.mkdir(exist_ok=False)
    features = ['-DSQLITE_THREADSAFE=1', *['-DSQLITE_ENABLE_'+name+'=1' for name in FEATURES]]
    host = ['/usr/bin/gcc','-std=gnu17',*features]
    macro_cmd = [*host,'-dM','-E','-x','c','-']
    macros_result = subprocess.run(macro_cmd,input='',capture_output=True,text=True,check=True)
    (attempt/'host-predefines.txt').write_text(macros_result.stdout)
    macros = dict(re.findall(r'^#define (\w+) (.*)$',macros_result.stdout,re.M))
    extra = ['-D'+name+'='+value for name,value in macros.items() if name in NAMES or name.startswith(PREFIXES)]
    assert macros['__STDC_VERSION__']=='201710L' and '__STRICT_ANSI__' not in macros
    assert macros['__x86_64__']=='1' and macros['__SIZEOF_POINTER__']=='8'
    include_cmd = [*host,'-E','-v','-x','c','-']
    include_result = subprocess.run(include_cmd,input='',capture_output=True,text=True,check=True)
    (attempt/'host-include-search.txt').write_text(include_result.stderr)
    inside=False;includes=[]
    for line in include_result.stderr.splitlines():
        if '#include <...> search starts here:' in line:inside=True;continue
        if 'End of search list.' in line:inside=False
        if inside and line.startswith(' '):includes += ['--sys_include',line.strip()]
    configured_macro_cmd = [*host,'-dM','-E',str(source)]
    configured = subprocess.run(configured_macro_cmd,capture_output=True,text=True,check=True)
    (attempt/'configured-source-macros.txt').write_text(configured.stdout)
    cm = dict(re.findall(r'^#define (\w+) (.*)$',configured.stdout,re.M))
    assert all(cm['SQLITE_ENABLE_'+name]=='1' for name in FEATURES)
    assert cm['SQLITE_THREADSAFE']=='1' and not any(name in cm for name in ('SQLITE_OMIT_JSON','SQLITE_OMIT_WAL','SQLITE_OMIT_VIRTUALTABLE'))
    gnu_version = 10000*int(macros['__GNUC__'])+100*int(macros['__GNUC_MINOR__'])+int(macros['__GNUC_PATCHLEVEL__'])
    artifact = attempt/'sqlite3.roslynobj'
    argv = ['python3',str(BUILD/'roslyn-cpfe.py'),'-o',str(artifact),'--timing',
            '--gcc','--c17','--no_strict_gnu','--gnu_version',str(gnu_version),
            *features,*extra,*includes,str(source)]
    recipe = {'source':str(source),'source_sha256':sha(source),'official_source_identity':{name:re.search(r'^#define '+name+r'\s+(.+)$',source.read_text(),re.M).group(1) for name in ('SQLITE_VERSION','SQLITE_VERSION_NUMBER','SQLITE_SOURCE_ID')},'feature_flags':features,
              'host_reference_compile_argv':[*host,'-c',str(source),'-o','sqlite3.o'],
              'host_reference_compile_executed':False,'host_macro_command':macro_cmd,
              'host_include_command':include_cmd,'configured_source_macro_command':configured_macro_cmd,
              'actual_edg_argv':argv,'forwarded_actual_host_predefines':extra,
              'language':'GNU C17','compiler':producer['compiler'],'compiler_sha256':producer['compiler_sha256'],
              'process_stack_limit_bytes':67108864,'json_jsonb_wal':'Enabled by default; no OMIT flags',
              'driver_sha256':sha(pathlib.Path(__file__))}
    write(attempt/'recipe.json',recipe)
    resource.setrlimit(resource.RLIMIT_STACK,(67108864,resource.getrlimit(resource.RLIMIT_STACK)[1]))
    start=time.monotonic()
    with (attempt/'compiler.log').open('w') as log:
        result = subprocess.run(['/usr/bin/time','-v','-o',str(attempt/'time.txt'),*argv],
             cwd=ROOT,env={**os.environ,'ROSLYNOBJ_COMPILER':producer['compiler']},stdout=log,stderr=subprocess.STDOUT)
    log=(attempt/'compiler.log').read_text(errors='replace');timing={}
    for name,cpu,elapsed in re.findall(r'(Front end|Back end|Total compilation) time\s+([\d.]+) \(CPU\)\s+([\d.]+) \(elapsed\)',log):
        timing[name]={'cpu_seconds':float(cpu),'elapsed_seconds_rounded_stock':float(elapsed)}
    metrics={'exit_code':result.returncode,'combined_wall_seconds':time.monotonic()-start,
             'stock_timing':timing,'source_sha256':sha(source),'recipe':str(attempt/'recipe.json'),
             'log':str(attempt/'compiler.log'),'time_log':str(attempt/'time.txt'),
             'process_stack_limit_bytes':67108864,'status':'failed'}
    match=re.search(r'Maximum resident set size \(kbytes\): (\d+)',(attempt/'time.txt').read_text())
    metrics['max_rss_kib']=int(match.group(1)) if match else None
    if result.returncode==0 and artifact.is_file():
        db=sqlite3.connect(artifact.resolve().as_uri()+'?mode=ro',uri=True)
        header=db.execute('select complete,format_major,format_minor from object_header').fetchone()
        cap=db.execute("select v.value from build_capability b join text_atom n on n.id=b.name_id join text_atom v on v.id=b.value_id where n.value='normalized_semantic_schema_v1'").fetchone()
        assert header==(1,2,0) and cap==(producer['contract'],)
        metrics.update(status='published',artifact=str(artifact),bytes=artifact.stat().st_size,
            sha256=sha(artifact),header=header,contract=cap[0],page_size=db.execute('pragma page_size').fetchone()[0])
        db.close()
    write(attempt/'result.json',metrics)
    print(json.dumps(metrics,indent=2))
    return result.returncode

if __name__=='__main__':
    raise SystemExit(main())
