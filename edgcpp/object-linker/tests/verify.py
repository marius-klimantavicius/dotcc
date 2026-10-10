#!/usr/bin/env python3
"""Real independent EDG objects, managed linking, JIT/AOT and rejection checks."""
import argparse
import hashlib
import importlib.util
import json
import platform
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys
import lifetimes_verify
import records_verify
import references_verify
import default_adapters_verify
import template_identity_verify
import indirect_values_verify
import packed_layout_verify
import packed_typedef_identity_verify
import zero_bound_arrays_verify
import storage_verify
import project_c_verify
import provider_verify
import startup_actions_verify
import assignment_verify
import wide_overflow_verify
import record_transport_verify

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location('dotobj_compile', ROOT / 'compile.py')
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--linker', type=Path, help='Use an already built immutable linker DLL')
    parser.add_argument('--compiler', type=Path, default=ROOT.parent / 'build/bin/cpfe-dot')
    parser.add_argument('--lifetimes-only', action='store_true', help='Run the focused lexical cleanup acceptance')
    parser.add_argument('--project-c-only', action='store_true', help='Run C runtime and value-semantics acceptance')
    parser.add_argument('--storage-only', action='store_true', help='Run the linked storage/project-C acceptance')
    parser.add_argument('--startup-only', action='store_true', help='Run startup ordering, provider extraction and rejection checks')
    parser.add_argument('--assignment-only', action='store_true', help='Run C++17 RHS-first assignment acceptance')
    parser.add_argument('--records-only', action='store_true', help='Run the focused record/layout acceptance')
    parser.add_argument('--references-only', action='store_true', help='Run C++ references, receiver ABI and source-signature checks')
    parser.add_argument('--default-adapters-only', action='store_true', help='Run observed constructor defaults and real array runtime acceptance')
    parser.add_argument('--template-identity-only', action='store_true', help='Run owned public/private template specialization identity acceptance')
    parser.add_argument('--indirect-values-only', action='store_true', help='Run original class value and physical indirect ABI acceptance')
    parser.add_argument('--packed-only', action='store_true', help='Run observed packed layout and independent typedef identity acceptance')
    parser.add_argument('--record-transport-only', action='store_true', help='Run original record construction/assignment and cross-unit ABI transport checks')
    parser.add_argument('--wide-overflow-only', action='store_true', help='Run mathematical128-bit overflow and narrow intrinsic compatibility')
    parser.add_argument('--zero-bound-arrays-only', action='store_true', help='Run fixed-zero and incomplete array shape acceptance')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    compiler = out / 'cpfe-dot'
    shutil.copy2(args.compiler, compiler)
    inputs = out / 'inputs'
    inputs.mkdir()
    for name in ['fixture.h', 'provider.cpp', 'consumer.cpp', 'scopes.cpp', 'conversions.cpp', 'empty.cpp', 'loops.cpp', 'switches.cpp', 'integers.cpp', 'source_types.h', 'source_types.cpp', 'macros.h', 'macros.cpp', 'pointer_fixture.h', 'pointer_provider.cpp', 'pointers.cpp', 'array_fixture.h', 'array_provider.cpp', 'arrays.cpp', 'float_fixture.h', 'float_provider.cpp', 'floats.cpp']:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)
    shutil.copy2(ROOT / 'tests/loops.cpp', inputs / 'loops-c.c')
    shutil.copy2(ROOT / 'tests/switches.cpp', inputs / 'switches-c.c')
    shutil.copy2(ROOT / 'tests/integers.cpp', inputs / 'integers-c.c')
    shutil.copy2(ROOT / 'tests/source_types.cpp', inputs / 'source-types-c.c')
    shutil.copy2(ROOT / 'tests/macros.cpp', inputs / 'macros-c.c')
    shutil.copy2(ROOT / 'tests/pointers.cpp', inputs / 'pointers-c.c')
    shutil.copy2(ROOT / 'tests/pointer_provider.cpp', inputs / 'pointer-provider-c.c')
    shutil.copy2(ROOT / 'tests/arrays.cpp', inputs / 'arrays-c.c')
    shutil.copy2(ROOT / 'tests/array_provider.cpp', inputs / 'array-provider-c.c')
    shutil.copy2(ROOT / 'tests/floats.cpp', inputs / 'floats-c.c')
    shutil.copy2(ROOT / 'tests/float_provider.cpp', inputs / 'float-provider-c.c')
    for extension in ['.cpp', '.c']:
        shutil.copy2(ROOT / 'tests/lifetimes.cpp', inputs / ('lifetimes' + ('-c' if extension == '.c' else '') + extension))
        shutil.copy2(ROOT / 'tests/lifetime_provider.cpp', inputs / ('lifetime-provider' + ('-c' if extension == '.c' else '') + extension))
    shutil.copy2(ROOT / 'tests/lifetime_fixture.h', inputs / 'lifetime_fixture.h')
    for extension in ['.cpp', '.c']:
        shutil.copy2(ROOT / 'tests/records.cpp', inputs / ('records' + ('-c' if extension == '.c' else '') + extension))
        shutil.copy2(ROOT / 'tests/record_provider.cpp', inputs / ('record-provider' + ('-c' if extension == '.c' else '') + extension))
    shutil.copy2(ROOT / 'tests/record_fixture.h', inputs / 'record_fixture.h')
    for name in ['storage_fixture.h', 'storage_provider.c', 'storages.c', 'project_c_runtime.c', 'project_c_intrinsics.c', 'project_c_record_values.c', 'project_c_record_array_values.c', 'project_c_aggregate_initializers.c', 'project_c_function_designators.c', 'project_c_bitfield_values.c', 'goto_entries.c', 'project_c_nested_switch.c', 'project_c_synthetic_bindings.c', 'project_c_binary80.c', 'project_c_folded_records.c', 'project_c_unspecified_definitions.c', 'project_c_volatile_binary80.c', 'project_c_void_sequences.c', 'project_c_macro_record_identity.c', 'provider_root.c', 'provider_library.c', 'provider_support.c']:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)
    checks = []
    for name in ['project_c_startup_order.c', 'project_c_lifecycle_actions.c',
                 'project_cpp_startup_first.cpp', 'project_cpp_startup_second.cpp',
                 'project_cpp_startup_shared.h', 'project_cpp_record_origins.cpp',
                 'project_cpp_vptr_layout.cpp', 'project_cpp_constant_storage.cpp',
                 'cpp_assignment_order.cpp', 'project_cpp_assignment_lifting.cpp',
                 'project_c_integer128.c', 'project_c_integer128_boundaries.c',
                 'project_cpp_integer128_enum.cpp', 'project_cpp_references_members.cpp',
                 'project_cpp_block_copy.cpp', 'project_cpp_void_conditionals.cpp',
                 'project_cpp_default_ctor_adapter.cpp', 'project_cpp_base_values.cpp',
                 'template_identity_shared.h', 'template_identity_provider.cpp', 'template_identity_main.cpp',
                 'project_cpp_indirect_value_abi.cpp', 'project_cpp_packed_layout.cpp',
                 'packed_typedef_identity.h', 'packed_typedef_identity_provider.cpp', 'packed_typedef_identity_main.cpp', 'zero_bound_arrays.h',
                 'zero_bound_arrays_provider.cpp', 'zero_bound_arrays_main.cpp',
                 'cpp_dynamic_startup_scalar.cpp', 'cpp_dynamic_startup_array.cpp']:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)

    for name in record_transport_verify.FILES:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)

    def save(status):
        (out / 'results.json').write_text(json.dumps(dict(status=status, checks=checks,
            profile='scalar-control-v45', nativeAotRequested=args.nativeaot, lifetimesOnly=args.lifetimes_only, recordsOnly=args.records_only, wideOverflowOnly=args.wide_overflow_only, recordTransportOnly=args.record_transport_only,
            platform=platform.platform(), implementation='fresh-owned-semantic-graph',
            limitations=['No complete prelower source view, macro expansion history or general C++ lifetime/EH coverage.',
                         'No complete DuckDB or general C++ acceptance.']), indent=2) + '\n')

    def run(label, command, expected=0, contains=None):
        command = [str(x) for x in command]
        result = subprocess.run(command, cwd=out, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        passed = (result.returncode != 0 if expected is None else result.returncode == expected)
        if contains is not None:
            passed = passed and contains.lower() in (result.stdout + result.stderr).lower()
        checks.append(dict(name=label, command=command, exitCode=result.returncode, passed=passed))
        save('running' if passed else 'failed')
        if not passed:
            raise RuntimeError(label + ' failed; inspect retained logs at ' + str(out))
        print(label + ': passed', flush=True)
        return result

    def check(label, condition):
        checks.append(dict(name=label, passed=bool(condition)))
        save('running' if condition else 'failed')
        if not condition:
            raise RuntimeError(label)

    def compile_source(name, source, output=None, expected=0, unit_id=None, std='c++17', extra=()):
        output = output or out / (name + '.dotobj')
        run('compile-' + name, [sys.executable, '-B', ROOT / 'compile.py', source,
            '--output', output, '--work-dir', out / (name + '-compile'), '--compiler', compiler,
            '--unit-id', unit_id or name, '--std', std, *extra], expected)
        return output

    try:
        if args.linker:
            tool = args.linker.resolve()
        else:
            run('tool-build', ['dotnet', 'build', ROOT / 'managed/DotObj.Linker.csproj',
                '-c', 'Release', '--artifacts-path', out / 'tool'])
            tool = out / 'tool/bin/DotObj.Linker/release/dotobj-link.dll'
        if not tool.exists():
            raise RuntimeError('Expected linker build output missing')
        provider = compile_source('provider', inputs / 'provider.cpp')
        consumer = compile_source('consumer', inputs / 'consumer.cpp')
        scopes = compile_source('scopes', inputs / 'scopes.cpp')
        for name, obj in [('provider', provider), ('consumer', consumer), ('scopes', scopes)]:
            run('verify-' + name, ['dotnet', tool, 'verify', obj])
            invocation = json.loads((out / (name + '-compile/invocation.json')).read_text())
            check('direct-source-' + name, invocation['arguments'][-1] == str(inputs / (name + '.cpp'))
                  and '--preprocess' not in invocation['arguments'])
        run('native-build', ['c++', '-std=c++17', inputs / 'provider.cpp', inputs / 'consumer.cpp', '-o', out / 'native'])
        run('native-run', [out / 'native'])
        run('native-scopes-build', ['c++', '-std=c++17', inputs / 'scopes.cpp', '-o', out / 'native-scopes'])
        run('native-scopes-run', [out / 'native-scopes'])

        def link(label, objects, entry='main', namespace='Tests.Linked', cls='Core', expected=0, contains=None):
            directory = out / label
            run(label, ['dotnet', tool, 'link', '--out', directory, '--namespace', namespace,
                        '--class', cls, '--entry', entry, *objects], expected, contains)
            return directory

        def execute(label, directory, expected=0):
            run(label + '-build', ['dotnet', 'build', directory / 'Translated.csproj', '-c', 'Release'])
            run(label + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Translated.dll'], expected)
            if args.nativeaot:
                machine = {'x86_64': 'x64', 'aarch64': 'arm64'}.get(platform.machine())
                system = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}.get(platform.system())
                if not machine or not system:
                    raise RuntimeError('Unrecognized NativeAOT host RID')
                publish = out / (label + '-aot')
                run(label + '-publish', ['dotnet', 'publish', directory / 'Translated.csproj',
                    '-c', 'Release', '-r', system + '-' + machine, '-p:PublishAot=true', '-o', publish])
                run(label + '-aot-run', [publish / ('Translated.exe' if system == 'win' else 'Translated')], expected)

        def mutate(name, original, sql, repair=True):
            path = out / (name + '.dotobj')
            shutil.copy2(original, path)
            with sqlite3.connect(path) as db:
                for query, params in sql:
                    db.execute(query, params)
                if repair:
                    db.execute('UPDATE content_digest SET sha256=?', (MODULE.logical_hash(db),))
            return path

        if args.record_transport_only:
            record_transport_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
            return
        if args.wide_overflow_only:
            wide_overflow_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            source = inputs / 'project_c_intrinsics.c'
            obj = compile_source('narrow-intrinsics', source, std='c11')
            run('narrow-intrinsics-native-build', ['cc', '-std=c11', '-O2', source, '-o', out / 'narrow-intrinsics-native'])
            run('narrow-intrinsics-native-run', [out / 'narrow-intrinsics-native'])
            execute('narrow-intrinsics', link('link-narrow-intrinsics', [obj]))
            save('passed')
            print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
            return
        if args.zero_bound_arrays_only:
            zero_bound_arrays_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.packed_only:
            packed_layout_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            packed_typedef_identity_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.indirect_values_only:
            indirect_values_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.template_identity_only:
            template_identity_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.default_adapters_only:
            default_adapters_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.references_only:
            references_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.assignment_only:
            assignment_verify.verify(inputs, out, run, check, compile_source, link, execute)
            save('passed')
            return
        if args.startup_only:
            startup_actions_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
            save('passed')
            return
        if args.project_c_only:
            project_objects = project_c_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
            project_c_verify.malformed(project_objects, tool, run, mutate)
            save('passed')
            return
        if args.storage_only:
            storage_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
            save('passed')
            return
        if args.records_only:
            record_provider, record_consumer = records_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
            records_verify.malformed(record_provider, record_consumer, tool, run, link, mutate)
            save('passed')
            print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
            return

        if args.lifetimes_only:
            lifetime_consumer = lifetimes_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
            lifetimes_verify.malformed(lifetime_consumer, tool, run, mutate)
            save('passed')
            print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
            return

        wide_overflow_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        record_transport_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        linked = link('link', [provider, consumer])
        execute('linked', linked)
        scoped = link('link-scopes', [scopes])
        execute('scopes', scoped)
        for name in ['conversions', 'empty', 'loops', 'switches', 'integers', 'source_types', 'macros']:
            obj = compile_source(name, inputs / (name + '.cpp'))
            run('native-' + name + '-build', ['c++', '-std=c++17', inputs / (name + '.cpp'), '-o', out / ('native-' + name)])
            run('native-' + name + '-run', [out / ('native-' + name)])
            execute(name, link('link-' + name, [obj]))
        pointer_objects = []
        for language, standard, consumer_name, provider_name, extension, native in [
            ('cpp', 'c++17', 'pointers', 'pointer_provider', '.cpp', 'c++'),
            ('c', 'c11', 'pointers-c', 'pointer-provider-c', '.c', 'cc')]:
            pointer_provider = compile_source(provider_name, inputs / (provider_name + extension), std=standard)
            pointer_consumer = compile_source(consumer_name, inputs / (consumer_name + extension), std=standard)
            run('native-pointers-' + language + '-build', [native, '-std=' + standard, inputs / (provider_name + extension), inputs / (consumer_name + extension), '-o', out / ('native-pointers-' + language)])
            run('native-pointers-' + language + '-run', [out / ('native-pointers-' + language)])
            execute('pointers-' + language, link('link-pointers-' + language, [pointer_provider, pointer_consumer]))
            pointer_objects.append((pointer_provider, pointer_consumer))
            with sqlite3.connect(pointer_consumer) as db:
                check('typed-pointer-rows-' + language, db.execute("SELECT count(*) FROM types WHERE kind='pointer' AND pointee_type_id IS NOT NULL").fetchone()[0] >= 5)
                check('pointer-source-provenance-' + language, db.execute("SELECT count(*) FROM source_types WHERE kind='pointer' AND underlying_type_id IS NOT NULL").fetchone()[0] >= 5)
                check('pointer-places-' + language, db.execute("SELECT count(*) FROM nodes WHERE kind='dereference' AND value_category='place'").fetchone()[0] > 0)
        pointer_provider, pointer_consumer = pointer_objects[0]
        # Make object-local identity independence deliberate, not dependent on
        # which unrelated signatures happened to allocate IDs first.
        renumbered = out / 'pointer-provider-renumbered.dotobj'
        shutil.copy2(pointer_provider, renumbered)
        with sqlite3.connect(renumbered) as db:
            tables = [row[0] for row in db.execute("SELECT name FROM sqlite_master WHERE type='table'")]
            for table in tables:
                columns = {'id'} if table == 'types' else set()
                for row in db.execute('PRAGMA foreign_key_list(' + MODULE.quote(table) + ')'):
                    if row[2] in {'types', 'routine_types', 'record_types'}:
                        columns.add(row[3])
                if columns:
                    db.execute('UPDATE ' + MODULE.quote(table) + ' SET ' + ','.join(
                        MODULE.quote(column) + '=' + MODULE.quote(column) + '+1000000'
                        for column in sorted(columns)))
            db.execute('UPDATE content_digest SET sha256=?', (MODULE.logical_hash(db),))
        execute('pointers-renumbered', link('link-pointers-renumbered', [renumbered, pointer_consumer]))
        with sqlite3.connect(renumbered) as db:
            provider_int_pointer = db.execute("SELECT t.id FROM types t JOIN types p ON p.id=t.pointee_type_id WHERE t.kind='pointer' AND p.kind='i32'").fetchone()[0]
        with sqlite3.connect(pointer_consumer) as db:
            consumer_int_pointer = db.execute("SELECT t.id FROM types t JOIN types p ON p.id=t.pointee_type_id WHERE t.kind='pointer' AND p.kind='i32'").fetchone()[0]
        check('pointer-link-uses-structural-types', provider_int_pointer != consumer_int_pointer)
        pointer_cs = '\n'.join(p.read_text() for p in (out / 'link-pointers-cpp').glob('*.cs'))
        check('direct-pointer-source', 'unsafe' in pointer_cs and 'int*' in pointer_cs and '__edg_' not in pointer_cs)
        array_objects = []
        for language, standard, consumer_name, provider_name, extension, native in [
            ('cpp', 'c++17', 'arrays', 'array_provider', '.cpp', 'c++'),
            ('c', 'c11', 'arrays-c', 'array-provider-c', '.c', 'cc')]:
            array_provider = compile_source(provider_name, inputs / (provider_name + extension), std=standard)
            array_consumer = compile_source(consumer_name, inputs / (consumer_name + extension), std=standard)
            run('native-arrays-' + language + '-build', [native, '-std=' + standard, inputs / (provider_name + extension), inputs / (consumer_name + extension), '-o', out / ('native-arrays-' + language)])
            run('native-arrays-' + language + '-run', [out / ('native-arrays-' + language)])
            execute('arrays-' + language, link('link-arrays-' + language, [array_provider, array_consumer]))
            array_objects.append((array_provider, array_consumer))
            with sqlite3.connect(array_consumer) as db:
                check('fixed-array-type-shape-' + language, db.execute("SELECT count(*) FROM types WHERE kind='array' AND element_type_id IS NOT NULL AND element_count>0").fetchone()[0] >= 6)
                check('multidimensional-array-type-' + language, db.execute("SELECT count(*) FROM types t JOIN types e ON e.id=t.element_type_id WHERE t.kind='array' AND e.kind='array'").fetchone()[0] > 0)
                check('array-source-provenance-' + language, db.execute("SELECT count(*) FROM source_types WHERE kind='array' AND element_count>0").fetchone()[0] > 0)
                check('explicit-array-decay-' + language, db.execute("SELECT count(*) FROM nodes WHERE kind='array-decay'").fetchone()[0] > 0)
                check('lexical-array-initializers-' + language, db.execute("SELECT count(*) FROM nodes WHERE kind='array-init'").fetchone()[0] > 0)
        array_provider, array_consumer = array_objects[0]
        array_cs = '\n'.join(p.read_text() for p in (out / 'link-arrays-cpp').glob('*.cs'))
        check('array-type-keeps-source-function-name', 'public static int __DotObjArray_A3_i32()' in array_cs)
        check('constant-array-span-copy', 'ReadOnlySpan<' in array_cs and '.CopyTo(' in array_cs)
        check('array-storage-no-managed-heap', 'NativeMemory.' not in array_cs and 'GC.Allocate' not in array_cs and 'new int[' not in array_cs)
        array_collision = link('link-arrays-name-collision', [array_provider, array_consumer], cls='__DotObjArray_A3_i32')
        execute('arrays-name-collision', array_collision)
        float_objects = []
        for language, standard, consumer_name, provider_name, extension, native in [
            ('cpp', 'c++17', 'floats', 'float_provider', '.cpp', 'c++'),
            ('c', 'c11', 'floats-c', 'float-provider-c', '.c', 'cc')]:
            float_provider = compile_source(provider_name, inputs / (provider_name + extension), std=standard)
            float_consumer = compile_source(consumer_name, inputs / (consumer_name + extension), std=standard)
            run('native-floats-' + language + '-build', [native, '-std=' + standard, inputs / (provider_name + extension), inputs / (consumer_name + extension), '-o', out / ('native-floats-' + language)])
            run('native-floats-' + language + '-run', [out / ('native-floats-' + language)])
            execute('floats-' + language, link('link-floats-' + language, [float_provider, float_consumer]))
            float_objects.append((float_provider, float_consumer))
            with sqlite3.connect(float_consumer) as db:
                check('floating-source-types-' + language, {'float','double'} <= {row[0] for row in db.execute('SELECT kind FROM source_types')})
                f32_constants = {row[0] for row in db.execute("SELECT constant_text FROM nodes WHERE kind='constant' AND type_id=10")}
                f64_constants = {row[0] for row in db.execute("SELECT constant_text FROM nodes WHERE kind='constant' AND type_id=11")}
                check('exact-f32-constant-bits-' + language, {'80000000','00000001','3dcccccd','7fc12345'} <= f32_constants)
                check('exact-f64-constant-bits-' + language, {'8000000000000000','0000000000000001','3fb999999999999a','7ff8000000012345'} <= f64_constants)
        float_provider, float_consumer = float_objects[0]
        float_cs = '\n'.join(p.read_text() for p in (out / 'link-floats-cpp').glob('*.cs'))
        check('floating-array-static-data', 'ReadOnlySpan<' in float_cs and '.CopyTo(' in float_cs and 'new float[' not in float_cs and 'new double[' not in float_cs)
        lifetime_consumer = lifetimes_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
        record_provider, record_consumer = records_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
        records_verify.malformed(record_provider, record_consumer, tool, run, link, mutate)
        storage_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
        references_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        default_adapters_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        template_identity_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        indirect_values_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        packed_layout_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        packed_typedef_identity_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        zero_bound_arrays_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        project_objects = project_c_verify.verify(inputs, out, tool, run, check, compile_source, link, execute)
        project_c_verify.malformed(project_objects, tool, run, mutate)
        provider_verify.verify(inputs, out, tool, run, check, compile_source, execute)
        startup_actions_verify.verify(inputs, out, tool, run, check, compile_source, link, execute, mutate)
        for startup_case in ('scalar', 'array'):
            startup_name = 'cpp-dynamic-startup-' + startup_case
            startup_object = compile_source(startup_name, inputs / ('cpp_dynamic_startup_' + startup_case + '.cpp'))
            startup_linked = link(startup_name + '-link', [startup_object])
            execute(startup_name, startup_linked)
        cleanup_command = [sys.executable, '-B', ROOT / 'tests/cpp_cleanup_states_verify.py',
                           '--output-dir', out / 'cpp-cleanup-states', '--compiler', compiler,
                           '--linker', tool, '--packager', ROOT / 'compile.py']
        if args.nativeaot:
            cleanup_command.append('--nativeaot')
        run('cpp-cleanup-states-acceptance', cleanup_command)
        assignment_verify.verify(inputs, out, run, check, compile_source, link, execute)
        loops_object = out / 'loops.dotobj'
        loops_cs = '\n'.join(p.read_text() for p in (out / 'link-loops').glob('*.cs'))
        check('structured-loop-source', all(token in loops_cs for token in ['for (', 'do', 'while (', 'break;', 'continue;']) and 'goto ' not in loops_cs)
        with sqlite3.connect(loops_object) as db:
            check('explicit-loop-targets', db.execute("SELECT count(*) FROM nodes WHERE kind IN ('break','continue')").fetchone()[0] > 0 and db.execute("SELECT count(*) FROM nodes n LEFT JOIN edges e ON e.parent_id=n.id AND e.role='target' LEFT JOIN nodes t ON t.id=e.child_id WHERE n.kind IN ('break','continue') AND (t.id IS NULL OR t.kind NOT IN ('for','while','do-while'))").fetchone()[0] == 0)
        loops_c = compile_source('loops-c', inputs / 'loops-c.c', std='c11')
        run('native-loops-c-build', ['cc', '-std=c11', inputs / 'loops-c.c', '-o', out / 'native-loops-c'])
        run('native-loops-c-run', [out / 'native-loops-c'])
        execute('loops-c', link('link-loops-c', [loops_c]))
        switches_object = out / 'switches.dotobj'
        switches_cs = '\n'.join(p.read_text() for p in (out / 'link-switches').glob('*.cs'))
        check('structured-switch-source', all(token in switches_cs for token in ['switch (', 'case ', 'default:', 'goto case ', 'goto default;']))
        with sqlite3.connect(switches_object) as db:
            check('explicit-switch-break-targets', db.execute("SELECT count(*) FROM edges e JOIN nodes n ON n.id=e.parent_id JOIN nodes t ON t.id=e.child_id WHERE e.role='target' AND n.kind='break' AND t.kind='switch'").fetchone()[0] > 0)
            check('switch-continue-loop-targets', db.execute("SELECT count(*) FROM edges e JOIN nodes n ON n.id=e.parent_id JOIN nodes t ON t.id=e.child_id WHERE e.role='target' AND n.kind='continue' AND t.kind='for'").fetchone()[0] >= 2)
        switches_c = compile_source('switches-c', inputs / 'switches-c.c', std='c11')
        run('native-switches-c-build', ['cc', '-std=c11', inputs / 'switches-c.c', '-o', out / 'native-switches-c'])
        run('native-switches-c-run', [out / 'native-switches-c'])
        execute('switches-c', link('link-switches-c', [switches_c]))
        integers_c = compile_source('integers-c', inputs / 'integers-c.c', std='c11')
        run('native-integers-c-build', ['cc', '-std=c11', inputs / 'integers-c.c', '-o', out / 'native-integers-c'])
        run('native-integers-c-run', [out / 'native-integers-c'])
        execute('integers-c', link('link-integers-c', [integers_c]))
        source_c = compile_source('source-types-c', inputs / 'source-types-c.c', std='c11')
        run('native-source-types-c-build', ['cc', '-std=c11', inputs / 'source-types-c.c', '-o', out / 'native-source-types-c'])
        run('native-source-types-c-run', [out / 'native-source-types-c'])
        execute('source-types-c', link('link-source-types-c', [source_c]))
        for language, obj in [('c++', out / 'source_types.dotobj'), ('c', source_c)]:
            with sqlite3.connect(obj) as db:
                kinds = {row[0] for row in db.execute('SELECT kind FROM source_types')}
                aliases = {row[0] for row in db.execute("SELECT source_name FROM source_types WHERE kind='typedef'")}
                check('source-type-kinds-' + language, {'long','unsigned-long-long','bool','typedef'} <= kinds)
                check('source-typedef-chain-' + language, {'Count','ItemCount','Mask','Toggle'} <= aliases)
                check('source-const-' + language, db.execute('SELECT count(*) FROM source_types WHERE is_const=1').fetchone()[0] > 0)
                check('source-bool-before-lowering-' + language, db.execute("SELECT count(*) FROM source_types s JOIN types t ON t.id=s.execution_type_id WHERE s.kind='bool' AND t.kind='u8'").fetchone()[0] > 0)
                check('distinct-shadowed-source-types-' + language, db.execute("SELECT count(DISTINCT execution_type_id) FROM source_types WHERE kind='typedef' AND source_name='Choice'").fetchone()[0] == 2)
                check('source-binding-coverage-' + language, db.execute("SELECT count(*) FROM source_declarations WHERE kind IN ('local','parameter')").fetchone()[0] == db.execute('SELECT count(*) FROM bindings WHERE is_synthetic=0').fetchone()[0])
        check('source-map-published', (out / 'link-source_types/source-map.json').is_file())
        macros_c = compile_source('macros-c', inputs / 'macros-c.c', std='c11')
        run('native-macros-c-build', ['cc', '-std=c11', inputs / 'macros-c.c', '-o', out / 'native-macros-c'])
        run('native-macros-c-run', [out / 'native-macros-c'])
        execute('macros-c', link('link-macros-c', [macros_c]))
        for language, obj in [('c++', out / 'macros.dotobj'), ('c', macros_c)]:
            with sqlite3.connect(obj) as db:
                names = {row[0] for row in db.execute('SELECT name FROM macro_definitions')}
                check('active-macro-definitions-' + language, {'API_VALUE','API_ADD','API_STRING','API_CAT','API_VARIADIC','API_EMPTY','API_MULTI','API_UTF8','API_OPTIONAL_TEXT'} <= names and 'API_INACTIVE' not in names)
                events = db.execute("SELECT e.kind,d.definition_text FROM macro_events e LEFT JOIN macro_definitions d ON d.id=e.definition_id WHERE e.name='API_VALUE' ORDER BY e.id").fetchall()
                check('macro-redefinition-order-' + language, [x[0] for x in events] == ['define','undef','define'] and '17' in events[0][1] and '23' in events[2][1])
                check('macro-unknown-undef-' + language, db.execute("SELECT count(*) FROM macro_events WHERE name='API_NEVER_DEFINED' AND kind='undef'").fetchone()[0] == 1)
                check('macro-identical-redefinition-' + language, db.execute("SELECT count(*) FROM macro_definitions WHERE name='API_SAME'").fetchone()[0] == 1)
                check('macro-parameters-' + language, [x[0] for x in db.execute("SELECT p.name FROM macro_parameters p JOIN macro_definitions d ON d.id=p.definition_id WHERE d.name='API_ADD' ORDER BY p.ordinal")] == ['a','b'])
                check('macro-paste-stringize-' + language, '##' in db.execute("SELECT definition_text FROM macro_definitions WHERE name='API_CAT'").fetchone()[0] and '#' in db.execute("SELECT definition_text FROM macro_definitions WHERE name='API_STRING'").fetchone()[0].split(')',1)[1])
                check('macro-variadic-' + language, db.execute("SELECT variadic FROM macro_definitions WHERE name='API_VARIADIC'").fetchone()[0] == 1 and db.execute("SELECT count(*) FROM macro_parameters p JOIN macro_definitions d ON d.id=p.definition_id WHERE d.name='API_VARIADIC' AND p.name='__VA_ARGS__'").fetchone()[0] == 1)
                check('macro-source-locations-' + language, db.execute("SELECT count(*) FROM macro_definitions WHERE name LIKE 'API_%' AND (origin<>'source' OR source_file_id IS NULL OR line=0 OR column=0)").fetchone()[0] == 0)
        check('macro-map-published', (out / 'link-macros/macro-map.json').is_file())
        commandline_source = inputs / 'macro-commandline.cpp'
        commandline_source.write_text('#ifdef API_REMOVED\n#error removal failed\n#endif\nint main(void){return API_COMMAND - 41;}\n#undef API_COMMAND\n')
        command_flags = ['-DAPI_COMMAND=41', '-DAPI_REMOVED=9', '-UAPI_REMOVED']
        command_obj = compile_source('macro-commandline', commandline_source, extra=command_flags)
        run('native-macro-commandline-build', ['c++', '-std=c++17', *command_flags, commandline_source, '-o', out / 'native-macro-commandline'])
        run('native-macro-commandline-run', [out / 'native-macro-commandline'])
        execute('macro-commandline', link('link-macro-commandline', [command_obj]))
        with sqlite3.connect(command_obj) as db:
            check('macro-commandline-origin', db.execute("SELECT origin FROM macro_definitions WHERE name='API_COMMAND'").fetchone()[0] == 'command-line')
            check('macro-commandline-undef', db.execute("SELECT count(*) FROM macro_events WHERE name='API_REMOVED' AND kind='undef' AND origin='command-line'").fetchone()[0] == 1)
        integer_cs = '\n'.join(p.read_text() for p in (out / 'link-integers').glob('*.cs'))
        check('direct-integer-operators', all(token in integer_cs for token in [' ? ', ' : ', ' << ', ' >> ', ' & ', ' | ', ' ^ ', '~']))
        reverse = link('link-reversed', [consumer, provider])
        cs = lambda path: {p.name: p.read_bytes() for p in path.glob('*.cs')}
        check('stable-output-input-order', cs(linked) == cs(reverse))

        # Objects contain source snapshots. Linking must not reopen source paths.
        inputs.rename(out / 'inputs-preserved')
        relocated = link('link-without-sources', [provider, consumer], namespace='Different.Output', cls='Engine')
        execute('without-sources', relocated)
        check('source-files-remain-unavailable', not inputs.exists())

        run('unresolved-import', ['dotnet', tool, 'link', '--out', out / 'bad-unresolved', consumer], None, 'Unresolved')
        run('duplicate-unit', ['dotnet', tool, 'link', '--out', out / 'bad-unit', provider, provider], None, 'translation unit')

        lifetimes_verify.malformed(lifetime_consumer, tool, run, mutate)

        duplicate = mutate('duplicate', provider, [('UPDATE object_info SET unit_id=?', ('duplicate',))])
        run('duplicate-strong', ['dotnet', tool, 'link', '--out', out / 'bad-strong', provider, duplicate], None, 'Duplicate strong')
        mismatch = mutate('signature', consumer, [
            ('UPDATE parameters SET type_id=3 WHERE entity_id=(SELECT id FROM entities WHERE source_name=?)', ('from_a',)),
            ("INSERT INTO types SELECT max(id)+1,'routine',NULL,NULL,NULL FROM types", ()),
            ("INSERT INTO routine_types SELECT (SELECT max(id) FROM types),return_type_id,variadic,exception_policy FROM routine_types WHERE type_id=(SELECT routine_type_id FROM entities WHERE source_name='from_a')", ()),
            ("INSERT INTO routine_parameters VALUES((SELECT max(id) FROM types),0,3)", ()),
            ("UPDATE entities SET routine_type_id=(SELECT max(id) FROM types) WHERE source_name='from_a'", ()),
            ("INSERT INTO source_types (id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count,is_restrict,is_atomic) SELECT max(id)+1,'unsigned-char','',NULL,3,0,0,NULL,0,0,NULL,0,0 FROM source_types", ()),
            ("UPDATE source_parameters SET source_type_id=(SELECT max(id) FROM source_types) WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a')", ()),
            ("INSERT INTO source_types (id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count,is_restrict,is_atomic) SELECT (SELECT max(id)+1 FROM source_types),kind,source_name,underlying_type_id,(SELECT max(id) FROM types),is_const,is_volatile,source_file_id,line,column,element_count,is_restrict,is_atomic FROM source_types WHERE id=(SELECT routine_source_type_id FROM source_functions WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a'))", ()),
            ("INSERT INTO source_routine_types SELECT (SELECT max(id) FROM source_types),return_type_id,variadic,exception_policy,parameter_mode,object_type_id,object_is_const,object_is_volatile,object_ref_qualifier FROM source_routine_types WHERE type_id=(SELECT routine_source_type_id FROM source_functions WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a'))", ()),
            ("INSERT INTO source_routine_parameters SELECT (SELECT max(id) FROM source_types),ordinal,source_type_id FROM source_parameters WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a')", ()),
            ("INSERT INTO source_routine_abis SELECT (SELECT max(id) FROM source_types),return_passing,capture_proof FROM source_routine_abis WHERE source_routine_type_id=(SELECT routine_source_type_id FROM source_functions WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a'))", ()),
            ("INSERT INTO source_routine_abi_parameters SELECT (SELECT max(id) FROM source_types),execution_ordinal,parameter_role,source_ordinal FROM source_routine_abi_parameters WHERE source_routine_type_id=(SELECT routine_source_type_id FROM source_functions WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a'))", ()),
            ("UPDATE source_functions SET routine_source_type_id=(SELECT max(id) FROM source_types) WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a')", ())])
        run('signature-mismatch', ['dotnet', tool, 'link', '--out', out / 'bad-signature', provider, mismatch], None, 'Signature mismatch')
        damaged = mutate('damaged', provider, [('UPDATE entities SET source_name=? WHERE id=1', ('tampered',))], False)
        run('digest-mismatch', ['dotnet', tool, 'verify', damaged], None, 'digest')
        bad_target = mutate('target', provider, [('UPDATE object_info SET target_abi=?', ('unknown-target',))])
        run('unknown-target', ['dotnet', tool, 'verify', bad_target], None)
        bad_edge = mutate('edge', provider, [('UPDATE edges SET role=? WHERE rowid=(SELECT min(rowid) FROM edges)', ('invented',))])
        run('invalid-edge', ['dotnet', tool, 'verify', bad_edge], None)
        bad_kind = mutate('node', provider, [('UPDATE nodes SET kind=? WHERE id=(SELECT min(id) FROM nodes)', ('raw-csharp',))])
        run('invalid-node-kind', ['dotnet', tool, 'verify', bad_kind], None)
        bad_snapshot = mutate('snapshot', provider, [('UPDATE source_files SET bytes=? WHERE id=1', (b'changed',))])
        run('snapshot-mismatch', ['dotnet', tool, 'verify', bad_snapshot], None)
        bad_primary = mutate('primary-hash', provider, [('UPDATE object_info SET source_sha256=?', ('0' * 64,))])
        run('primary-provenance-mismatch', ['dotnet', tool, 'verify', bad_primary], None)
        bad_schema = mutate('schema', provider, [('ALTER TABLE nodes ADD COLUMN hidden_behavior TEXT', ())])
        run('extra-schema-payload', ['dotnet', tool, 'verify', bad_schema], None)

        wrong_target = mutate('loop-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT body_node_id FROM functions WHERE entity_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('nonloop-exit-target', ['dotnet', tool, 'verify', wrong_target], None)
        missing_target = mutate('loop-missing-target', loops_object, [(
            "DELETE FROM edges WHERE role='target' AND parent_id=(SELECT min(id) FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('missing-loop-exit-target', ['dotnet', tool, 'verify', missing_target], None)
        outer_target = mutate('loop-outer-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT min(id) FROM nodes WHERE kind='for' AND function_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue')) AND child_id IN (SELECT id FROM nodes WHERE kind='for')", ())])
        run('wrong-lexical-loop-target', ['dotnet', tool, 'verify', outer_target], None)

        # Corrupt semantic shapes while recomputing the digest: verifier must
        # reject bad switch ownership, duplicate labels and nonlocal exits.
        with sqlite3.connect(switches_object) as db:
            grouped_id = db.execute("SELECT id FROM entities WHERE source_name='grouped'").fetchone()[0]
            valued_cases = db.execute("SELECT c.id, v.id FROM nodes c JOIN edges e ON e.parent_id=c.id AND e.role='value' JOIN nodes v ON v.id=e.child_id WHERE c.kind='case' AND c.function_id=? ORDER BY c.id", (grouped_id,)).fetchall()
            first_case, first_value = valued_cases[0]
            second_case, second_value = valued_cases[1]
        bad_duplicate_case = mutate('switch-duplicate-case', switches_object, [(
            "UPDATE nodes SET constant_text=(SELECT constant_text FROM nodes WHERE id=?) WHERE id=?", (first_value, second_value))])
        run('duplicate-switch-case', ['dotnet', tool, 'verify', bad_duplicate_case], None)
        converted_duplicate = mutate('switch-converted-duplicate', switches_object, [(
            "UPDATE nodes SET type_id=7,constant_text='4294967294' WHERE id=?", (second_value,))])
        run('converted-duplicate-switch-case', ['dotnet', tool, 'verify', converted_duplicate], None)
        bad_default = mutate('switch-duplicate-default', switches_object, [
            ("DELETE FROM edges WHERE parent_id=? AND role='value'", (first_case,)),
            ("DELETE FROM nodes WHERE id=?", (first_value,))])
        run('duplicate-switch-default', ['dotnet', tool, 'verify', bad_default], None)
        bad_case_value = mutate('switch-case-expression', switches_object, [(
            "UPDATE nodes SET kind='unary',operator='neg' WHERE id=?", (first_value,))])
        run('nonconstant-switch-case', ['dotnet', tool, 'verify', bad_case_value], None)
        bad_continue_switch = mutate('switch-continue-target', switches_object, [(
            "UPDATE edges SET child_id=(SELECT min(id) FROM nodes WHERE kind='switch' AND function_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind='continue')", ())])
        run('continue-cannot-target-switch', ['dotnet', tool, 'verify', bad_continue_switch], None)
        bad_break_outer = mutate('switch-break-outer', switches_object, [(
            "UPDATE edges SET child_id=(SELECT min(id) FROM nodes WHERE kind='for' AND function_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind='break') AND child_id IN (SELECT id FROM nodes WHERE kind='switch' AND function_id=(SELECT id FROM entities WHERE source_name='nested'))", ())])
        run('break-cannot-skip-switch', ['dotnet', tool, 'verify', bad_break_outer], None)
        bad_case_scope = mutate('switch-case-outside', switches_object, [(
            "UPDATE nodes SET kind='case' WHERE id=(SELECT max(id) FROM nodes WHERE kind='return' AND function_id=(SELECT id FROM entities WHERE source_name='main'))", ())])
        run('case-outside-switch', ['dotnet', tool, 'verify', bad_case_scope], None, 'Case label is not owned by a switch')

        integers_object = out / 'integers.dotobj'
        invalid_integer = mutate('wide-constant', integers_object, [(
            "UPDATE nodes SET constant_text='18446744073709551616' WHERE id=(SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=(SELECT id FROM types WHERE kind='u64'))", ())])
        run('wide-constant-out-of-range', ['dotnet', tool, 'verify', invalid_integer], None)
        invalid_narrow = mutate('narrow-constant', integers_object, [(
            "UPDATE nodes SET constant_text='128' WHERE id=(SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=(SELECT id FROM types WHERE kind='i8'))", ())])
        run('signed-byte-constant-out-of-range', ['dotnet', tool, 'verify', invalid_narrow], None)
        invalid_truth = mutate('comparison-result', integers_object, [(
            "UPDATE nodes SET type_id=(SELECT id FROM types WHERE kind='i64') WHERE id=(SELECT min(id) FROM nodes WHERE kind='binary' AND operator='ne')", ())])
        run('comparison-result-type', ['dotnet', tool, 'verify', invalid_truth], None)
        bad_conditional = mutate('conditional-edge', integers_object, [(
            "DELETE FROM edges WHERE role='else' AND parent_id=(SELECT min(id) FROM nodes WHERE kind='conditional')", ())])
        run('conditional-missing-arm', ['dotnet', tool, 'verify', bad_conditional], None)
        bad_result = mutate('conditional-type', integers_object, [(
            "UPDATE nodes SET type_id=(SELECT id FROM types WHERE kind='void') WHERE id=(SELECT min(id) FROM nodes WHERE kind='conditional')", ())])
        run('conditional-invalid-result', ['dotnet', tool, 'verify', bad_result], None)

        source_object = out / 'source_types.dotobj'
        wrong_source_type = mutate('source-physical-type', source_object, [(
            "UPDATE source_types SET execution_type_id=(SELECT id FROM types WHERE kind='i64') WHERE kind='bool'", ())])
        run('source-physical-mismatch', ['dotnet', tool, 'verify', wrong_source_type], None)
        cyclic_source = mutate('source-alias-cycle', source_object, [(
            "UPDATE source_types SET underlying_type_id=id WHERE kind='typedef'", ())])
        run('source-type-cycle', ['dotnet', tool, 'verify', cyclic_source], None)
        missing_source = mutate('source-binding-missing', source_object, [(
            "DELETE FROM source_declarations WHERE id=(SELECT min(id) FROM source_declarations WHERE kind='local')", ())])
        run('source-binding-missing', ['dotnet', tool, 'verify', missing_source], None)
        fabricated_source = mutate('synthetic-fabricated-source', source_object, [(
            "UPDATE bindings SET is_synthetic=1 WHERE id=(SELECT binding_id FROM source_declarations WHERE kind='local' LIMIT 1)", ())])
        run('synthetic-fabricated-source', ['dotnet', tool, 'verify', fabricated_source], None)
        wrong_phase = mutate('source-capture-phase', source_object, [(
            "UPDATE source_functions SET capture_phase='after-lowering'", ())])
        run('source-capture-phase', ['dotnet', tool, 'verify', wrong_phase], None)

        with sqlite3.connect(pointer_consumer) as db:
            pointer_type = db.execute("SELECT min(id) FROM types WHERE kind='pointer'").fetchone()[0]
            dereference_id = db.execute("SELECT min(id) FROM nodes WHERE kind='dereference'").fetchone()[0]
            address_id = db.execute("SELECT min(id) FROM nodes WHERE kind='address'").fetchone()[0]
        pointer_cycle = mutate('pointer-cycle', pointer_consumer, [(
            "UPDATE types SET pointee_type_id=id WHERE id=?", (pointer_type,))])
        run('pointer-type-cycle', ['dotnet', tool, 'verify', pointer_cycle], None)
        duplicate_pointer = mutate('pointer-duplicate', pointer_consumer, [(
            "INSERT INTO types SELECT 10000,kind,pointee_type_id,element_type_id,element_count FROM types WHERE id=?", (pointer_type,))])
        run('duplicate-structural-pointer', ['dotnet', tool, 'verify', duplicate_pointer], None, 'Duplicate')
        deep_pointer = mutate('pointer-depth', pointer_consumer, [(
            "WITH RECURSIVE chain(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM chain WHERE n<129) INSERT INTO types SELECT 10000+n,'pointer',CASE WHEN n=1 THEN (SELECT min(id) FROM types WHERE kind='pointer' AND id NOT IN (SELECT pointee_type_id FROM types WHERE kind='pointer')) ELSE 9999+n END,NULL,NULL FROM chain", ())])
        run('pointer-depth-limit', ['dotnet', tool, 'verify', deep_pointer], None, 'nesting')
        pointer_unknown = mutate('pointer-unknown', pointer_consumer, [(
            "UPDATE types SET pointee_type_id=999999 WHERE id=?", (pointer_type,))])
        run('unknown-pointer-pointee', ['dotnet', tool, 'verify', pointer_unknown], None)
        wrong_load = mutate('pointer-load-type', pointer_consumer, [(
            "UPDATE nodes SET type_id=9 WHERE id=?", (dereference_id,))])
        run('dereference-type-mismatch', ['dotnet', tool, 'verify', wrong_load], None)
        wrong_address = mutate('pointer-address-type', pointer_consumer, [(
            "UPDATE nodes SET type_id=2 WHERE id=?", (address_id,))])
        run('address-type-mismatch', ['dotnet', tool, 'verify', wrong_address], None)
        invalid_address_operand = mutate('pointer-address-place', pointer_consumer, [(
            "UPDATE nodes SET kind='constant',binding_id=NULL,constant_text='0',value_category='value' WHERE id=(SELECT child_id FROM edges WHERE parent_id=? AND role='operand')", (address_id,))])
        run('address-requires-place', ['dotnet', tool, 'verify', invalid_address_operand], None)
        invalid_null = mutate('pointer-nonzero-constant', pointer_consumer, [(
            "UPDATE nodes SET constant_text='1' WHERE kind='constant' AND type_id IN (SELECT id FROM types WHERE kind='pointer')", ())])
        run('nonzero-pointer-constant', ['dotnet', tool, 'verify', invalid_null], None)
        invalid_source_pointer = mutate('pointer-source-shape', pointer_consumer, [(
            "UPDATE source_types SET underlying_type_id=id WHERE kind='pointer' AND id=(SELECT min(id) FROM source_types WHERE kind='pointer')", ())])
        run('source-pointer-cycle', ['dotnet', tool, 'verify', invalid_source_pointer], None)
        mismatch_source = out / 'pointer-mismatch.cpp'
        mismatch_source.write_text('extern "C" int pointer_read(unsigned int* value){return (int)*value;}\n')
        mismatch_pointer_object = compile_source('pointer-signature-mismatch', mismatch_source)
        link('pointer-signature-rejected', [pointer_consumer, mismatch_pointer_object], entry='main', expected=None, contains='Signature mismatch')

        with sqlite3.connect(array_consumer) as db:
            array_type = db.execute("SELECT min(id) FROM types WHERE kind='array'").fetchone()[0]
            array_init = db.execute("SELECT min(id) FROM nodes WHERE kind='array-init'").fetchone()[0]
            array_element = db.execute("SELECT min(id) FROM nodes WHERE kind='array-element'").fetchone()[0]
            array_decay = db.execute("SELECT min(id) FROM nodes WHERE kind='array-decay'").fetchone()[0]
        cyclic_array = mutate('array-type-cycle', array_consumer, [(
            "UPDATE types SET element_type_id=id WHERE id=?", (array_type,))])
        run('array-element-type-cycle', ['dotnet', tool, 'verify', cyclic_array], None)
        huge_array = mutate('array-too-large', array_consumer, [(
            "UPDATE types SET element_count=2147483647 WHERE id=?", (array_type,))])
        run('array-size-limit', ['dotnet', tool, 'verify', huge_array], None)
        invalid_index = mutate('array-element-index', array_consumer, [(
            "UPDATE nodes SET constant_text='99999999' WHERE id=?", (array_element,))])
        run('array-initializer-index-limit', ['dotnet', tool, 'verify', invalid_index], None)
        invalid_element_type = mutate('array-element-value-type', array_consumer, [(
            "UPDATE nodes SET type_id=9 WHERE id=?", (array_element,))])
        run('array-initializer-type-mismatch', ['dotnet', tool, 'verify', invalid_element_type], None)
        invalid_decay = mutate('array-decay-type', array_consumer, [(
            "UPDATE nodes SET type_id=2 WHERE id=?", (array_decay,))])
        run('array-decay-requires-element-pointer', ['dotnet', tool, 'verify', invalid_decay], None)
        missing_array_target = mutate('array-missing-target', array_consumer, [(
            "DELETE FROM edges WHERE parent_id=? AND role='target'", (array_init,))])
        run('array-initializer-missing-target', ['dotnet', tool, 'verify', missing_array_target], None)
        wrong_source_bound = mutate('array-source-bound', array_consumer, [(
            "UPDATE source_types SET element_count=element_count+1 WHERE kind='array' AND element_count>0", ())])
        run('array-source-bound-mismatch', ['dotnet', tool, 'verify', wrong_source_bound], None)
        array_mismatch_source = out / 'array-mismatch.cpp'
        array_mismatch_source.write_text('extern "C" void array_bump_row(int (*row)[4]){(*row)[1]=3;}\n')
        mismatch_array_object = compile_source('array-signature-mismatch', array_mismatch_source)
        link('array-signature-rejected', [array_consumer, mismatch_array_object], entry='main', expected=None, contains='Signature mismatch')

        with sqlite3.connect(float_consumer) as db:
            f32_node = db.execute("SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=10").fetchone()[0]
            f64_node = db.execute("SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=11").fetchone()[0]
        malformed_f32 = mutate('float-hex-width', float_consumer, [(
            "UPDATE nodes SET constant_text='0000000' WHERE id=?", (f32_node,))])
        run('float-constant-width', ['dotnet', tool, 'verify', malformed_f32], None)
        malformed_f64 = mutate('double-hex-canonical', float_consumer, [(
            "UPDATE nodes SET constant_text='3FF0000000000000' WHERE id=?", (f64_node,))])
        run('double-constant-canonical-case', ['dotnet', tool, 'verify', malformed_f64], None)
        float_bitwise = mutate('float-bitwise', float_provider, [(
            "UPDATE nodes SET operator='bit-and' WHERE kind='binary' AND operator='add' AND type_id=10", ())])
        run('float-bitwise-rejected', ['dotnet', tool, 'verify', float_bitwise], None)
        float_remainder = mutate('float-remainder', float_provider, [(
            "UPDATE nodes SET operator='rem' WHERE kind='binary' AND operator='add' AND type_id=11", ())])
        run('float-remainder-rejected', ['dotnet', tool, 'verify', float_remainder], None)
        float_shift = mutate('float-shift', float_provider, [(
            "UPDATE nodes SET operator='shl' WHERE kind='binary' AND operator='add' AND type_id=10", ())])
        run('float-shift-rejected', ['dotnet', tool, 'verify', float_shift], None)
        float_pointer_cast = mutate('float-pointer-cast', float_consumer, [(
            "UPDATE nodes SET type_id=11 WHERE kind='convert' AND type_id=(SELECT id FROM types WHERE kind='pointer' AND pointee_type_id=3) AND id=(SELECT min(id) FROM nodes WHERE kind='convert' AND type_id=(SELECT id FROM types WHERE kind='pointer' AND pointee_type_id=3))", ())])
        run('pointer-to-floating-conversion-rejected', ['dotnet', tool, 'verify', float_pointer_cast], None)
        float_source_mismatch = mutate('float-source-representation', float_consumer, [(
            "UPDATE source_types SET execution_type_id=11 WHERE kind='float'", ())])
        run('float-source-type-mismatch', ['dotnet', tool, 'verify', float_source_mismatch], None)
        float_mismatch_source = out / 'float-mismatch.cpp'
        float_mismatch_source.write_text('extern "C" double float_identity(double value){return value;}\n')
        mismatch_float_object = compile_source('float-signature-mismatch', float_mismatch_source)
        link('float-signature-rejected', [float_consumer, mismatch_float_object], entry='main', expected=None, contains='Signature mismatch')
        fenv_source = out / 'unsupported-fenv.c'
        fenv_source.write_text('#pragma STDC FENV_ACCESS ON\ndouble f(double x){return x+1.0;}\n')
        fenv_object = compile_source('unsupported-fenv', fenv_source, std='c11', expected=None)
        check('not-published-fenv', not fenv_object.exists())

        macros_object = out / 'macros.dotobj'
        wrong_macro_event = mutate('macro-event-name', macros_object, [(
            "UPDATE macro_events SET name='DIFFERENT' WHERE id=(SELECT min(id) FROM macro_events WHERE kind='define')", ())])
        run('macro-event-name-mismatch', ['dotnet', tool, 'verify', wrong_macro_event], None)
        orphan_macro = mutate('macro-orphan', macros_object, [(
            "DELETE FROM macro_events WHERE definition_id=(SELECT id FROM macro_definitions WHERE name='API_EMPTY')", ())])
        run('macro-definition-event-missing', ['dotnet', tool, 'verify', orphan_macro], None)
        wrong_macro_origin = mutate('macro-origin', macros_object, [(
            "UPDATE macro_definitions SET origin='predefined' WHERE name='API_VALUE'", ())])
        run('macro-origin-location-mismatch', ['dotnet', tool, 'verify', wrong_macro_origin], None)
        wrong_macro_form = mutate('macro-object-params', macros_object, [(
            "UPDATE macro_definitions SET form='object' WHERE name='API_ADD'", ())])
        run('macro-object-parameter-mismatch', ['dotnet', tool, 'verify', wrong_macro_form], None)
        wrong_macro_ordinal = mutate('macro-param-ordinal', macros_object, [(
            "UPDATE macro_parameters SET ordinal=10 WHERE definition_id=(SELECT id FROM macro_definitions WHERE name='API_ADD') AND ordinal=1", ())])
        run('macro-parameter-order', ['dotnet', tool, 'verify', wrong_macro_ordinal], None)

        project_c_forms = {
            'precase-execution': 'int f(int n){switch(n){n=4;case 1:return 2;default:return 3;}}\n',
            'nested-switch-case': 'int f(int n){switch(n){{case 1:return 2;}default:return 3;}}\n',
            'volatile-pointer': 'int f(volatile int* value){return *value;}\n',
            'volatile': 'int f(){volatile int value=1; return value;}\n',
            'noexcept': 'int f() noexcept {return 1;}\n',
            'push-macro': '#define VALUE 1\n#pragma push_macro("VALUE")\nint f(){return VALUE;}\n',
            'pop-macro': '#pragma pop_macro("VALUE")\nint f(){return 1;}\n',
            'global': 'int storage=1; int f(){return storage;}\n',
            'oversized-array': 'int f(){unsigned char values[65537];values[0]=1;return values[0];}\n',
            'union-pointer': 'union X { int value; }; int f(X* p){return p->value;}\n',
            'function-pointer': 'int f(int (*call)(int)){return call(3);}\n',
            'inline': 'inline int f(){return 1;} int g(){return f();}\n',
            'goto': 'int f(){int n=0; again: ++n; if(n<3)goto again; return n;}\n',
        }
        for name, code in project_c_forms.items():
            source = out / ('supported-' + name + '.cpp')
            source.write_text(code)
            obj = compile_source('supported-' + name, source)
            run('verify-supported-' + name, ['dotnet', tool, 'verify', obj])

        negatives = {
            'va-opt': '#define API_OPT(...) __VA_OPT__(17)\nint f(){return API_OPT(1); }\n',
            'variable-array': 'int f(int n){int values[n]; values[0]=2;return values[0];}\n',
            'switch-range': 'int f(int n){switch(n){case 1 ... 3:return 2;default:return 3;}}\n',
        }
        for name, code in negatives.items():
            source = out / ('unsupported-' + name + '.cpp')
            source.write_text(code)
            obj = compile_source('unsupported-' + name, source, expected=None)
            check('not-published-' + name, not obj.exists())
            if name == 'va-opt':
                check('unsupported-va-opt-provenance', '__VA_OPT__' in (out / ('unsupported-' + name + '-compile/stderr')).read_text())

        # One changed TU can be recompiled and relinked with the unchanged other object.
        shutil.copytree(out / 'inputs-preserved', inputs)
        changed = inputs / 'provider.cpp'
        changed.write_text(changed.read_text().replace('event + 1', 'event + 2'))
        changed_object = compile_source('provider-new', changed, unit_id='provider')
        incremental = link('link-incremental', [changed_object, consumer])
        run('native-new-build', ['c++', '-std=c++17', changed, inputs / 'consumer.cpp', '-o', out / 'native-new'])
        run('native-new-run', [out / 'native-new'], 3)
        execute('incremental', incremental, 3)
        save('passed')
        print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
    except Exception:
        save('failed')
        raise


if __name__ == '__main__':
    main()
