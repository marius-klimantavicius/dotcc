#!/usr/bin/env python3
"""Prepare an independent pinned EDG tree with only build/backend overlays."""
import argparse, hashlib, json, pathlib, shutil, subprocess
PIN = '89e67e07c7f6d0fc38622f9778d8e79f40deaa0f'
ROOT = pathlib.Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('--upstream', type=pathlib.Path, default=pathlib.Path('/tmp/roslynobj-edg-upstream-20261012'))
p.add_argument('--stage', type=pathlib.Path, default=ROOT.parents[0] / 'artifacts/roslynobj/upstream')
p.add_argument('--profile', choices=('native','source','cfront','exported'), default='native')
a = p.parse_args()
actual = subprocess.check_output(['git', '-C', str(a.upstream), 'rev-parse', 'HEAD'], text=True).strip()
if actual != PIN:
    raise SystemExit(f'Expected {PIN}, found {actual}')
a.stage.mkdir(parents=True, exist_ok=True)
# Copy materialized build paths from the pristine pinned checkout.  Avoid
# git archive's eager fetching of the entire partially cloned corpus.
for rel in ('CMakeLists.txt', 'src', 'cmake', 'util'):
    source = a.upstream / rel
    destination = a.stage / rel
    if source.is_dir(): shutil.copytree(source, destination, dirs_exist_ok=True)
    else: shutil.copyfile(source, destination)
# Pin the stock configuration utility independently of the embedded tree.
tool_source=ROOT.parents[0] / 'upstream/dev_tools/bin/edg-cmakedef'
tool_data=tool_source.read_bytes()
tool_blob=hashlib.sha1(b'blob '+str(len(tool_data)).encode()+b'\0'+tool_data).hexdigest()
expected_tool=subprocess.check_output(['git','-C',str(a.upstream),'rev-parse',PIN+':dev_tools/bin/edg-cmakedef'],text=True).strip()
if tool_blob != expected_tool: raise SystemExit('Stock edg-cmakedef differs from pin')
tool_destination=a.stage / 'dev_tools/bin/edg-cmakedef'
tool_destination.parent.mkdir(parents=True,exist_ok=True)
shutil.copyfile(tool_source,tool_destination)
tool_destination.chmod(0o755)
config = a.stage / 'cmake/macro-conf/roslynobj'
config.mkdir(exist_ok=True)
(config / 'base.cmakedef').write_text('import <linux-gcc-release/base>\n')
for name in ('cpfe-cp', 'cdisp', 'cpprtlib'):
    (config / (name + '.cmakedef')).write_text(f'import <linux-gcc-release/{name}>\n')
shutil.copyfile(ROOT / 'build/profiles/cpfe.cmakedef', config / 'cpfe.cmakedef')
if a.profile != 'native':
    with (config / 'cpfe.cmakedef').open('a') as profile:
        profile.write('\n' + (ROOT / 'build/profiles' / (a.profile + '.cmakedef')).read_text())
for f in (ROOT / 'backend').iterdir():
    if f.is_file(): shutil.copyfile(f, a.stage / 'src' / f.name)
cmake = a.stage / 'src/cmake/cpfe/CMakeLists.txt'
original = subprocess.check_output(['git','-C',str(a.upstream),'show',PIN+':src/cmake/cpfe/CMakeLists.txt'], text=True)
capture_id={'native':2001,'source':2002,'cfront':2003,'exported':2004}[a.profile]
cmake.write_text(original + '''\n# Independent roslynobj backend; stock source semantics remain unchanged.
find_package(SQLite3 3.37 REQUIRED)
set_source_files_properties(${PROJECT_SOURCE_DIR}/roslyn_gen_be.c PROPERTIES LANGUAGE CXX)
add_executable(roslyn-cpfe ${ALL_C_FILES} ${PROJECT_SOURCE_DIR}/roslyn_gen_be.c ${TXT_FILES})
add_dependencies(roslyn-cpfe error-info)
target_include_directories(roslyn-cpfe PRIVATE ${PROJECT_SOURCE_DIR})
target_link_libraries(roslyn-cpfe PRIVATE SQLite::SQLite3)
target_compile_options(roslyn-cpfe PRIVATE -Wno-error=shadow)
add_softfloat_to_target(roslyn-cpfe)
'''+f'\ntarget_compile_definitions(roslyn-cpfe PRIVATE ROSLYN_CAPTURE_PROFILE_ID={capture_id})\n')
core = {}
for original_input in sorted((a.upstream / 'src').rglob('*')):
    if original_input.is_file():
        relative=original_input.relative_to(a.upstream/'src')
        if relative.as_posix()=='cmake/cpfe/CMakeLists.txt':continue # Explicit isolated backend registration above.
        f=a.stage/'src'/relative
        expected_blob = subprocess.check_output(['git','-C',str(a.upstream),'rev-parse',PIN+':src/'+relative.as_posix()],text=True).strip()
        content=f.read_bytes()
        blob=hashlib.sha1(b'blob '+str(len(content)).encode()+b'\0'+content).hexdigest()
        if blob!=expected_blob: raise SystemExit(f'Core source changed: {f}')
        core[relative.as_posix()]=hashlib.sha256(content).hexdigest()
(a.stage.parent / 'core-source-hashes.json').write_text(json.dumps({'revision':PIN,'files':core},indent=2)+'\n')
print(a.stage)
