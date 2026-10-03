#!/usr/bin/env python3
"""Build the semantic exporter in an isolated copy; never mutate canonical EDG."""
import argparse, hashlib, json, pathlib, shutil, subprocess
p=argparse.ArgumentParser(); p.add_argument('--work',type=pathlib.Path,required=True);p.add_argument('-j',type=int,default=4);a=p.parse_args()
def write(path,data):
    if not path.exists() or path.read_text()!=data:path.write_text(data)
def copy(source,target):
    if not target.exists() or source.read_bytes()!=target.read_bytes():shutil.copyfile(source,target)
root=pathlib.Path(__file__).resolve().parents[1]; work=a.work.resolve();src=work/'upstream';build=work/'build'
work.mkdir(parents=True,exist_ok=True)
if not src.exists():
    shutil.copytree(root/'upstream',src,symlinks=True,ignore=shutil.ignore_patterns('.git','__pycache__','tests'))
# This build retains the supported C++ regeneration configuration, replacing only
# its emission callback. Parsing and semantic analysis are untouched.
copy(root/'semantic-export/native/semantic_export_be.c',src/'src/semantic_export_be.c')
reference=(root/'upstream/src/cp_gen_be.c').read_text()
assert reference.count('void back_end(void)')==1
write(src/'src/cp_gen_be.c',reference.replace('void back_end(void)','void semantic_unused_regeneration_back_end(void)'))
subprocess.run(['python3',str(root/'semantic-export/native/generate_names.py'),str(src/'src/il_def.h'),str(src/'src/semantic_enum_names.h')],check=True)
config=src/'cmake/macro-conf/semantic';config.mkdir(exist_ok=True)
write(config/'base.cmakedef','import <linux-gcc/base>\nIL_SHOULD_BE_WRITTEN_TO_FILE=0\nCOMPILE_MULTIPLE_TRANSLATION_UNITS=0\nDOING_SOURCE_ANALYSIS=0\nDO_IL_LOWERING=0\nGENERATE_SOURCE_SEQUENCE_LISTS=1\nEXTRA_SOURCE_POSITIONS_IN_IL=1\nALL_TEMPLATE_INFO_IN_IL=1\nRECORD_TEMPLATE_STRINGS=1\nMAINTAIN_CLASS_MEMBER_LIST=1\nRECORD_MACROS_IN_IL=1\nFULLY_RESOLVED_MACRO_POSITIONS=1\nMACRO_INVOCATION_TREE_IN_IL=1\nRECORD_MACRO_ARGS=1\nDEFAULT_REMOVE_UNNEEDED_ENTITIES=0\nIL_WALK_NEEDED=1\n')
for target in ['cpfe','cpfe-cp','cdisp','cpfe-dot','cpfe-dot-oracle']:
    write(config/(target+'.cmakedef'),'import <default/'+target+'>\n' if (src/'cmake/macro-conf/default'/(target+'.cmakedef')).exists() else 'BACK_END_IS_C_GEN_BE=0\nBACK_END_IS_CP_GEN_BE=1\n')
# Only configure standard targets; local dot backend overlays are independent.
f=src/'src/CMakeLists.txt';text=f.read_text();text=text.replace('add_subdirectory(cmake/cpfe-dot)','').replace('add_subdirectory(cmake/cpfe-dot-oracle)','');write(f,text)
target=src/'src/cmake/cpfe-cp/CMakeLists.txt'
text=target.read_text()
if 'target_compile_options(cpfe-cp PRIVATE -Wno-error=shadow)' not in text:
    target.write_text(text+'\ntarget_compile_options(cpfe-cp PRIVATE -Wno-error=shadow)\n')
text=target.read_text()
if 'target_sources(cpfe-cp PRIVATE' not in text:
    target.write_text(text+'\nset_source_files_properties(${PROJECT_SOURCE_DIR}/semantic_export_be.c PROPERTIES LANGUAGE CXX)\ntarget_sources(cpfe-cp PRIVATE ${PROJECT_SOURCE_DIR}/semantic_export_be.c)\ntarget_include_directories(cpfe-cp PRIVATE ${PROJECT_SOURCE_DIR})\n')
commands=[['cmake','-S',str(src),'-B',str(build),'-G','Ninja','-DCMAKE_BUILD_TYPE=Debug','-DCMAKE_CXX_FLAGS_DEBUG=-O0 -g0','-DEDG_MACRO_CONF=semantic','-DEDG_BUILD_LIB=OFF','-DEDG_CPFE_USE_ASAN=OFF','-DEDG_ENABLE_CONVERSION_WARNINGS=OFF','-DCMAKE_EXPORT_COMPILE_COMMANDS=ON'],['cmake','--build',str(build),'--target','cpfe-cp','-j',str(a.j)]]
for i,c in enumerate(commands):
    (work/f'build-command-{i}.json').write_text(json.dumps(c,indent=2))
    with (work/f'build-{i}.log').open('w') as log:
        r=subprocess.run(c,stdout=log,stderr=subprocess.STDOUT)
    if r.returncode: print((work/f'build-{i}.log').read_text()[-15000:]);raise SystemExit(r.returncode)
shutil.copyfile(build/'bin/cpfe-cp',build/'bin/cpfe-semantic');(build/'bin/cpfe-semantic').chmod(0o755)
binary=build/'bin/cpfe-semantic'
sha=lambda b:hashlib.sha256(b).hexdigest()
inputs=[{'path':str(p.relative_to(src)),'sha256':sha(p.read_bytes())} for p in sorted((src/'src').glob('*')) if p.is_file() and p.suffix in ['.c','.h']]
commands=json.loads((build/'compile_commands.json').read_text())
command=next(x for x in commands if x['file'].endswith('/semantic_export_be.c'))
manifest={'schema':'edg-semantic-producer/1','binarySha256':sha(binary.read_bytes()),'compileCommand':command,'inputs':inputs,'configuration':(config/'base.cmakedef').read_text(),'sourceAdapter':'semantic_export_be.c','referenceCallbackRenamed':True}
(binary.parent/'cpfe-semantic.producer.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(binary)
