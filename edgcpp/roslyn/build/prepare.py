#!/usr/bin/env python3
"""Prepare an independent pinned EDG tree with only build/backend overlays."""
import argparse, hashlib, json, pathlib, shutil, subprocess
PIN = '89e67e07c7f6d0fc38622f9778d8e79f40deaa0f'
ROOT = pathlib.Path(__file__).resolve().parents[1]
def write_if_changed(path, text):
    data=text.encode()
    if not path.is_file() or path.read_bytes()!=data: path.write_bytes(data)
def copy_if_changed(source, destination):
    if not destination.is_file() or source.read_bytes()!=destination.read_bytes():
        shutil.copyfile(source,destination)
p = argparse.ArgumentParser()
p.add_argument('--upstream', type=pathlib.Path, default=pathlib.Path('/tmp/roslynobj-edg-upstream-20261012'))
p.add_argument('--stage', type=pathlib.Path, default=ROOT.parents[0] / 'artifacts/roslynobj/upstream')
p.add_argument('--profile', choices=('native','source','cfront','exported','sun','usl21','usl30','softfloat','host-long-double','host-double','instantiation','tableless'), default='native')
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
    if source.is_dir():
        # This single stock file is generated below from the pinned original
        # plus the independently registered backend. Preserve an identical
        # existing overlay instead of replacing it and forcing reconfigure.
        def ignore_registration(directory,names):
            return ('CMakeLists.txt',) if pathlib.Path(directory)==a.upstream/'src/cmake/cpfe' else ()
        shutil.copytree(source, destination, dirs_exist_ok=True,ignore=ignore_registration)
    else: copy_if_changed(source,destination)
# Pin the stock configuration utility independently of the embedded tree.
tool_source=ROOT.parents[0] / 'upstream/dev_tools/bin/edg-cmakedef'
tool_data=tool_source.read_bytes()
tool_blob=hashlib.sha1(b'blob '+str(len(tool_data)).encode()+b'\0'+tool_data).hexdigest()
expected_tool=subprocess.check_output(['git','-C',str(a.upstream),'rev-parse',PIN+':dev_tools/bin/edg-cmakedef'],text=True).strip()
if tool_blob != expected_tool: raise SystemExit('Stock edg-cmakedef differs from pin')
tool_destination=a.stage / 'dev_tools/bin/edg-cmakedef'
tool_destination.parent.mkdir(parents=True,exist_ok=True)
copy_if_changed(tool_source,tool_destination)
tool_destination.chmod(0o755)
config = a.stage / 'cmake/macro-conf/roslynobj'
config.mkdir(exist_ok=True)
write_if_changed(config / 'base.cmakedef','import <linux-gcc-release/base>\n')
for name in ('cpfe-cp', 'cdisp', 'cpprtlib'):
    write_if_changed(config / (name + '.cmakedef'),f'import <linux-gcc-release/{name}>\n')
profile_text=(ROOT / 'build/profiles/cpfe.cmakedef').read_text()
if a.profile != 'native':
    if a.profile in ('usl21','usl30'):
        profile_text+='\n'+(ROOT / 'build/profiles/cfront.cmakedef').read_text()
    profile_text+='\n'+(ROOT / 'build/profiles' / (a.profile + '.cmakedef')).read_text()
write_if_changed(config / 'cpfe.cmakedef',profile_text)
for f in (ROOT / 'backend').iterdir():
    if f.is_file(): copy_if_changed(f, a.stage / 'src' / f.name)
cmake = a.stage / 'src/cmake/cpfe/CMakeLists.txt'
original = subprocess.check_output(['git','-C',str(a.upstream),'show',PIN+':src/cmake/cpfe/CMakeLists.txt'], text=True)
capture_id={'native':2001,'source':2002,'cfront':2003,'exported':2004,'sun':2001,'usl21':2003,'usl30':2003,'softfloat':2001,'host-long-double':2001,'host-double':2001,'instantiation':2001,'tableless':2001}[a.profile]
write_if_changed(cmake,original + '''\n# Independent roslynobj backend; stock source semantics remain unchanged.
find_package(SQLite3 3.37 REQUIRED)
set_source_files_properties(${PROJECT_SOURCE_DIR}/roslyn_gen_be.c PROPERTIES LANGUAGE CXX)
add_executable(roslyn-cpfe ${ALL_C_FILES} ${PROJECT_SOURCE_DIR}/roslyn_gen_be.c ${TXT_FILES})
add_dependencies(roslyn-cpfe error-info)
target_include_directories(roslyn-cpfe PRIVATE ${PROJECT_SOURCE_DIR})
target_link_libraries(roslyn-cpfe SQLite::SQLite3)
target_compile_options(roslyn-cpfe PRIVATE -Wno-error=shadow)
add_softfloat_to_target(roslyn-cpfe)
'''+f'\ntarget_compile_definitions(roslyn-cpfe PRIVATE ROSLYN_CAPTURE_PROFILE_ID={capture_id} ROSLYN_BUILD_PROFILE_NAME="{a.profile}")\n')
# Verify every materialized stock input copied above, including build scripts,
# configuration files and support utilities.  No extension-based allowlist can
# silently omit a .cpp, .def, .inc or a new stock build input.
tracked = subprocess.check_output(['git','-C',str(a.upstream),'ls-tree','-r','-z',PIN,'--','CMakeLists.txt','src','cmake','util'])
blobs = {}
for record in tracked.split(b'\0'):
    if not record: continue
    description, path = record.split(b'\t',1)
    mode, kind, digest = description.split(b' ',2)
    if kind == b'blob': blobs[path.decode()] = digest.decode()
materialized = [a.upstream / 'CMakeLists.txt']
for directory in ('src','cmake','util'):
    materialized.extend(f for f in (a.upstream/directory).rglob('*') if f.is_file())
core = {}
for original_input in sorted(materialized):
    relative=original_input.relative_to(a.upstream).as_posix()
    expected_blob=blobs.get(relative)
    if expected_blob is None: raise SystemExit(f'Unpinned stock input: {original_input}')
    original_content=original_input.read_bytes()
    original_blob=hashlib.sha1(b'blob '+str(len(original_content)).encode()+b'\0'+original_content).hexdigest()
    if original_blob != expected_blob: raise SystemExit(f'Stock input differs from pin: {original_input}')
    if relative == 'src/cmake/cpfe/CMakeLists.txt': continue # Exact independent backend registration above.
    f=a.stage/relative
    content=f.read_bytes()
    blob=hashlib.sha1(b'blob '+str(len(content)).encode()+b'\0'+content).hexdigest()
    if blob!=expected_blob: raise SystemExit(f'Core source changed: {f}')
    core[relative]=hashlib.sha256(content).hexdigest()
(a.stage.parent / 'core-source-hashes.json').write_text(json.dumps({'revision':PIN,'files':core,'allowed_stock_overlays':['src/cmake/cpfe/CMakeLists.txt']},indent=2)+'\n')
print(a.stage)
