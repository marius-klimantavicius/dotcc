#!/usr/bin/env python3
"""Array/VTT source unwinding against native C++ and EDG's stock C backend."""
import argparse
import hashlib
import importlib.util
import json
import resource
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

HOST = '''using Tests.ArrayCleanup;
internal static unsafe class Host
{
    internal static int Frees;
    public static int Main(string[] arguments)
    {
        if (arguments.Length != 0) { try { Core.array_cleanup_terminate(int.Parse(arguments[0])); return 99; } catch (global::System.Exception) { return 98; } }
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        for (int scenario = 0; scenario != 5; ++scenario)
            foreach (int fail in new[] {0, 1, 2, 3, 4, 6, 7, 9, 10, 11, 12})
                global::System.Console.WriteLine($"a {scenario} {fail} {Core.array_cleanup_case(scenario, fail)}");
        for (int scenario = 0; scenario != 3; ++scenario)
            foreach (int fail in new[] {0, 1, 2, 5, 8, 10, 12, 13, 14})
                global::System.Console.WriteLine($"v {scenario} {fail} {Core.vtt_cleanup_case(scenario, fail)}");
        Core.array_delete_adapter_probe(global::System.Runtime.InteropServices.NativeMemory.Alloc(13));
        if (Frees != 1) return 1;
        global::System.Console.WriteLine($"d {Frees}");
        return 0;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void dotobj_test_free(void* pointer)
        {
            ++global::Host.Frees;
            global::System.Runtime.InteropServices.NativeMemory.Free(pointer);
        }
    }
}
'''

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'stock-compiler', 'stock-runtime', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    source = inputs / 'cpp_array_vtt_cleanup.cpp'; native = inputs / 'cpp_array_vtt_cleanup_native.cpp'
    delete = inputs / 'cpp_array_vtt_delete.cpp'
    for file in (source, native, delete): shutil.copy2(Path(__file__).with_name(file.name), file)
    checks, commands = [], []
    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)
    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=900,
            preexec_fn=lambda: resource.setrlimit(resource.RLIMIT_CORE, (0, 0)))
        (out / (name + '.stdout')).write_bytes(result.stdout); (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)
        return result
    error = None
    try:
        outputs=[]
        for optimization in ('0','2'):
            binary=out / ('native-O'+optimization)
            run('native-build-O'+optimization, ['c++', '-std=c++17', '-O'+optimization, source, native, delete, '-o', binary])
            outputs.append(run('native-run-O'+optimization,[binary]).stdout)
        check('native-optimization-equivalence', outputs[0] == outputs[1])
        stock=out / 'stock.c'
        run('stock-translate', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300', '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', source])
        stock = out / 'cpp_array_vtt_cleanup.int.c'
        for optimization in ('0','2'):
            obj=out / ('stock-O'+optimization+'.o'); binary=out / ('stock-O'+optimization)
            run('stock-compile-O'+optimization,['cc','-std=c11','-O'+optimization,'-c',stock,'-o',obj])
            run('stock-link-O'+optimization,['c++','-std=c++17',native,delete,obj,args.stock_runtime.resolve(),'-o',binary])
            check('stock-equivalence-O'+optimization, run('stock-run-O'+optimization,[binary]).stdout == outputs[0])
        tooling=out / 'tooling'; tooling.mkdir()
        for filename in ('compile.py','schema.sql'): shutil.copy2(args.packager.resolve().parent / filename,tooling / filename)
        obj=out / 'cleanup.dotobj'
        run('export',[sys.executable,'-B',tooling / 'compile.py',source,'--output',obj,'--work-dir',out / 'work','--compiler',args.compiler.resolve(),'--std','c++17','--exceptions'])
        run('verify',['dotnet',args.linker.resolve(),'verify',obj])
        delete_obj=out/'delete.dotobj'
        run('export-delete',[sys.executable,'-B',tooling/'compile.py',delete,'--output',delete_obj,'--work-dir',out/'delete-work','--compiler',args.compiler.resolve(),'--std','c++17'])
        with sqlite3.connect(obj) as db:
            check('owned-array-runtime-prefix',db.execute("SELECT count(*) FROM array_runtime_operations WHERE operation_kind='construct'").fetchone()[0]>0)
            check('owned-array-cleanup',db.execute('SELECT count(*) FROM cleanup_arrays').fetchone()[0]>=4)
            check('owned-vtt-cleanup',db.execute('SELECT count(*) FROM cleanup_vtt_arguments').fetchone()[0]>0)
            check('owned-virtual-base-layout',db.execute('SELECT count(*) FROM virtual_base_subobjects').fetchone()[0]>0)
        inputs.rename(out / 'retained-source')
        generated=out / 'generated'
        run('source-hidden-closed-link',['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Tests.ArrayCleanup','--class','Core','--allow-externs',obj,delete_obj,*[argument for provider in args.provider for argument in ('--provider',provider.resolve())]])
        report=json.loads((generated/'link-report.json').read_text())
        check('only-real-host-free-adapter',[row['ExternalKey'] for row in report['runtimeRequirements']]==['dotobj_test_free'])
        check('no-unbound-storage',not report['runtimeStorageRequirements'])
        check('shared-array-provider',(generated/'Runtime.CppArrays.cs').is_file())
        check('shared-exception-provider',(generated/'Runtime.CppExceptions.cs').is_file())
        (generated/'Host.cs').write_text(HOST)
        project=generated/'Translated.csproj'; project.write_text(project.read_text().replace('<OutputType>Library</OutputType>','<OutputType>Exe</OutputType><StartupObject>Host</StartupObject>'))
        run('build',['dotnet','build',project,'-c','Release','--nologo','-v:q'])
        check('jit-equivalence',run('jit',['dotnet',generated/'bin/Release/net10.0/Translated.dll']).stdout==outputs[0])
        for scenario in range(3):
            failure=run('jit-terminate-'+str(scenario),['dotnet',generated/'bin/Release/net10.0/Translated.dll',str(scenario)],False)
            check('jit-terminate-diagnostic-'+str(scenario),b'during unwinding' in failure.stderr)
        if args.nativeaot:
            publish=out/'publish'
            run('publish',['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',publish,'--nologo','-v:q'])
            check('nativeaot-equivalence',run('nativeaot',[publish/'Translated']).stdout==outputs[0])
            for scenario in range(3):
                failure=run('nativeaot-terminate-'+str(scenario),[publish/'Translated',str(scenario)],False)
                check('nativeaot-terminate-diagnostic-'+str(scenario),b'during unwinding' in failure.stderr)
        spec=importlib.util.spec_from_file_location('dotobj_packager',tooling/'compile.py'); packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
        def reject(name,sql):
            bad=out/(name+'.dotobj');shutil.copy2(obj,bad)
            with sqlite3.connect(bad) as db:
                db.execute(sql); db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),));db.commit()
            run(name,['dotnet',args.linker.resolve(),'verify',bad],False)
        reject('reject-array-subobject-callback',"INSERT INTO cleanup_arrays SELECT a.id,t.execution_type_id,1,r.size,'observed-array-region' FROM cleanup_actions a JOIN abi_routine_origins o ON o.entity_id=a.callback_entity_id JOIN source_destruction_obligations d ON d.id=a.source_obligation_id JOIN source_types t ON t.id=d.source_type_id JOIN record_types r ON r.type_id=t.execution_type_id WHERE o.entry_kind='subobject' AND a.id NOT IN (SELECT action_id FROM cleanup_arrays) LIMIT 1")
        reject('reject-array-runtime-callback',"UPDATE array_runtime_operations SET callback_entity_id=(SELECT entity_id FROM source_functions WHERE function_kind='constructor' LIMIT 1)")
        reject('reject-array-stride','UPDATE cleanup_arrays SET element_size=element_size+1 WHERE action_id=(SELECT min(action_id) FROM cleanup_arrays)')
        reject('reject-array-count','UPDATE cleanup_arrays SET element_count=element_count+1 WHERE action_id=(SELECT min(action_id) FROM cleanup_arrays)')
        reject('reject-missing-vtt-abi-slot',"UPDATE abi_routine_origins SET entry_kind='subobject' WHERE entity_id=(SELECT a.entity_id FROM abi_routine_origins a JOIN parameters p ON p.entity_id=a.primary_entity_id AND p.parameter_role='vtt' WHERE a.entry_kind='complete' LIMIT 1)")
        reject('reject-vtt-wrong-owner','UPDATE cleanup_vtt_arguments SET binding_id=(SELECT b.id FROM bindings b WHERE b.function_id<>(SELECT function_entity_id FROM cleanup_actions WHERE id=cleanup_vtt_arguments.action_id) LIMIT 1)')
        reject('reject-virtual-offset','UPDATE virtual_base_subobjects SET byte_offset=byte_offset+1')
    except Exception as exception: error=str(exception)
    receipt=dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands,cases=83,
        limitations=['Fixed complete arrays; dynamic VLA/cookie allocation cleanup and exception_ptr are separate work.',
                      'RTTI storage comes from supplied translated LLVM/EDG source providers; test owns global delete replacement and matching BCL free adapter.'],
        providers=[dict(path=str(path.resolve()),sha256=hashlib.sha256(path.read_bytes()).hexdigest()) for path in args.provider])
    for name in ('compiler','linker','packager'):
        path=getattr(args,name).resolve();receipt[name]=dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out/'results.json').write_text(json.dumps(receipt,indent=2)+'\n')
    print(json.dumps(dict(status=receipt['status'],checks=len(checks),error=error)))
    return 1 if error else 0
if __name__=='__main__':sys.exit(main())
