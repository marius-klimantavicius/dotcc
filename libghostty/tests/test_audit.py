"""Pure Python negative controls for the bounded scalar delivery audit.

Run directly: PYTHONDONTWRITEBYTECODE=1 python3 libghostty/tests/test_audit.py
No compiler, campaign CLI, project lock, managed build or product mutation.
"""
from copy import deepcopy
import importlib.util
import json
from pathlib import Path
import tempfile
from types import ModuleType, SimpleNamespace
import unittest
from unittest.mock import patch


SPEC = importlib.util.spec_from_file_location("ghostty_delivery_audit", Path(__file__).with_name("audit.py"))
audit = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(audit)
EVIDENCE = Path(__file__).resolve().parents[1] / "build/audit-strengthening-r10"


def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    return path


def write_json(path, value):
    return write(path, json.dumps(value))


NOTICE_ITEM = ('<None Update="THIRD-PARTY-NOTICES" TargetPath="GhosttyVt.THIRD-PARTY-NOTICES" '
               'CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />')
PROJECT = ('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Library</OutputType>'
           '<TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup>' + NOTICE_ITEM + '</ItemGroup></Project>')
EXPORT = 'public static unsafe byte* ghostty_alloc(int* a, ulong n) { return null; }'


class AuditControls(unittest.TestCase):
    def setUp(self):
        EVIDENCE.mkdir(parents=True, exist_ok=True)
        self.temp = tempfile.TemporaryDirectory(prefix="control-", dir=EVIDENCE)
        self.addCleanup(self.temp.cleanup)
        self.base = Path(self.temp.name)
        self.project = write(self.base / "delivery/Library.csproj", PROJECT)
        self.source = 'namespace Managed; public static partial class GhosttyVt {\n' + EXPORT + '\n}'
        self.own({"Library.cs": self.source})

    def own(self, sources):
        for name, text in sources.items():
            write(self.project.parent / name, text)
        write_json(self.project.parent / ".campaign-files.json", [*sources, self.project.name, "Dotcc.SourceFiles.txt"])
        write(self.project.parent / "Dotcc.SourceFiles.txt", "\n".join(sources) + "\n")

    def records(self, source=None):
        return audit.methods(source or self.source)

    def test_public_body_is_unique_and_not_a_declaration(self):
        self.assertEqual(set(audit._exports(self.records(), {"ghostty_alloc"})), {"ghostty_alloc"})
        for body in ('public static extern byte* ghostty_alloc(int* a, ulong n);',
                     '// ' + EXPORT, '/* ' + EXPORT + ' */',
                     'private static byte* ghostty_alloc(int* a, ulong n) { return null; }',
                     'public static string s = "' + EXPORT + '";'):
            with self.subTest(body=body), self.assertRaisesRegex(RuntimeError, "exactly one actual public"):
                audit._exports(self.records('public class GhosttyVt {\n' + body + '\n}'), {"ghostty_alloc"})
        with self.assertRaisesRegex(RuntimeError, "exactly one actual public"):
            audit._exports(self.records(self.source.replace(EXPORT, EXPORT + '\n' + EXPORT)), {"ghostty_alloc"})

    def test_wrong_owner_and_nested_export_do_not_count(self):
        for source in (self.source.replace("class GhosttyVt", "class Other"),
                       'public class GhosttyVt { public class Helper {\n' + EXPORT + '\n}}'):
            with self.subTest(source=source), self.assertRaisesRegex(RuntimeError, "exactly one actual public"):
                audit._exports(self.records(source), {"ghostty_alloc"})

    def test_expression_export_and_braces_inside_literals(self):
        source = 'public class GhosttyVt {\npublic static int ghostty_alloc() => "{".Length;\n}'
        self.assertEqual(len(audit._exports(self.records(source), {"ghostty_alloc"})), 1)

    def test_diagnostic_denylist_includes_abi_count_run_io_host(self):
        names = ('dotcc_abi_main', 'dotcc_oracle_main', 'dotcc_boundary_main', 'dotcc_upstream_run',
                 'dotcc_upstream_count', 'dotcc_upstream_io_live', 'dotcc_upstream_host_finish')
        for name in names:
            with self.subTest(name=name):
                self.assertIsNotNone(audit.DIAGNOSTICS.search(audit._mask('public static int ' + name + '() { return 0; }')))
        self.assertIsNone(audit.DIAGNOSTICS.search(audit._mask('// dotcc_abi_main\nstring x="dotcc_abi_main";')))

    def test_recursive_sdk_sources_and_build_caches(self):
        self.assertEqual(set(audit.owned_sources(self.project)), {"Library.cs"})
        write(self.project.parent / "obj/Debug/net10.0/sdk.cs", 'unowned generated build input')
        write(self.project.parent / "bin/Release/copy.cs", 'cache copy')
        self.assertEqual(set(audit.owned_sources(self.project)), {"Library.cs"})
        write(self.project.parent / "nested/unowned.cs", 'public class Provider {}')
        with self.assertRaisesRegex(RuntimeError, "unowned.*nested/unowned.cs"):
            audit.owned_sources(self.project)

    def test_nested_cache_names_and_dot_filenames_are_sdk_sources(self):
        # SDK excludes BaseOutputPath/BaseIntermediateOutputPath at project root;
        # a nested folder named bin/obj is still reached by **/*.cs.
        for name in ('nested/obj/unowned.cs', 'nested/bin/unowned.cs', '.unowned.cs'):
            with self.subTest(name=name):
                path = write(self.project.parent / name, 'class Unowned {}')
                with self.assertRaisesRegex(RuntimeError, 'source set differs'):
                    audit.owned_sources(self.project)
                path.unlink()

    def test_owned_nested_source_is_scanned(self):
        self.own({"Library.cs": self.source, "nested/Helper.cs": "class Helper {}"})
        self.assertEqual(set(audit.owned_sources(self.project)), {"Library.cs", "nested/Helper.cs"})

    def test_source_list_and_ownership_must_agree(self):
        write(self.project.parent / "Dotcc.SourceFiles.txt", "Library.cs\nLibrary.cs\n")
        with self.assertRaisesRegex(RuntimeError, "Emitter source list"):
            audit.owned_sources(self.project)
        write_json(self.project.parent / ".campaign-files.json", ["../outside.cs"])
        write(self.project.parent.parent / "outside.cs", "class Outside {}")
        with self.assertRaisesRegex(RuntimeError, "Unsafe/missing owned source"):
            audit.owned_sources(self.project)

    def test_native_binary_outside_manifest_is_rejected(self):
        for name in ("nested/provider.so", "libterminal.so.4", "native/provider.obj", "provider.dll"):
            with self.subTest(name=name):
                path = write(self.project.parent / name, "binary")
                with self.assertRaisesRegex(RuntimeError, "Native/binary artifacts"):
                    audit.owned_sources(self.project)
                path.unlink()

    def test_dependencies_and_imported_targets_fail_closed(self):
        self.assertEqual(audit.project_inputs(self.project)["project_references"], [])
        for item in ('<ProjectReference Include="../Terminal.csproj" />',
                     '<PackageReference Include="Terminal" Version="1.0" />',
                     '<Reference Include="Other"><HintPath>other.dll</HintPath></Reference>',
                     '<NativeReference Include="terminal.so" />',
                     '<Compile Include="../external.cs" />', '<Compile Remove="Library.cs" />'):
            with self.subTest(item=item):
                write(self.project, PROJECT.replace(NOTICE_ITEM, NOTICE_ITEM + item))
                with self.assertRaisesRegex(RuntimeError, "Unexpected dependency/Compile"):
                    audit.project_inputs(self.project)
        write(self.project, PROJECT.replace('</Project>', '<Import Project="injected.targets" /></Project>'))
        with self.assertRaisesRegex(RuntimeError, "Unexpected build/dependency input"):
            audit.project_inputs(self.project)

    def test_inherited_dependency_and_restore_dependency_are_rejected(self):
        write(self.base / "Directory.Build.targets", '<Project><ItemGroup><Reference Include="terminal" /></ItemGroup></Project>')
        with self.assertRaisesRegex(RuntimeError, "Unexpected dependency/Compile"):
            audit.project_inputs(self.project)
        (self.base / "Directory.Build.targets").unlink()
        write_json(self.project.parent / "obj/project.assets.json", {"libraries": {"Other/1.0": {"type": "project"}}})
        with self.assertRaisesRegex(RuntimeError, "Unexpected resolved"):
            audit.project_inputs(self.project)

    def test_notice_full_bodies_not_labels_and_copy_both_outputs(self):
        pins = {"license_inputs": [{"name": "ghostty", "path": "LICENSE", "distribute": True},
                                  {"name": "runtime", "toolchain_path": "LICENSE", "distribute": True}]}
        product = self.base / "upstream"
        zig = self.base / "zig"
        write(product / "LICENSE", "Copyright upstream\nPermission to use the upstream text.\n")
        write(zig / "LICENSE", "Runtime license\nPermission to use the runtime.\n")
        source_roots = {"product": product, "zig": zig}
        notice = self.project.parent / "THIRD-PARTY-NOTICES"
        write(notice, "ghostty\n\nruntime\n")
        with self.assertRaisesRegex(RuntimeError, "full notice bodies"):
            audit.notices(self.project, pins, source_roots, self.base)
        write(notice, 'ghostty\n' + (product / 'LICENSE').read_text() + '\n\nruntime\n' + (zig / 'LICENSE').read_text() + '\n')
        self.assertEqual(len(audit.notices(self.project, pins, source_roots, self.base)), 2)
        for property_name in ("CopyToOutputDirectory", "CopyToPublishDirectory"):
            write(self.project, PROJECT.replace(property_name + '="PreserveNewest"', property_name + '="Never"'))
            with self.subTest(property_name=property_name), self.assertRaisesRegex(RuntimeError, "Unexpected dependency/Compile"):
                audit.project_inputs(self.project)

    def test_repository_runtime_license_requires_actual_full_body(self):
        repo = Path(__file__).resolve().parents[2]
        original_body = (repo / 'LICENSE').read_text(encoding='utf-8')
        pins = {'license_inputs': [{'name': 'DotCC shared runtime', 'license': 'MIT',
                                  'repository_path': 'LICENSE', 'distribute': True}]}
        notice = self.project.parent / 'THIRD-PARTY-NOTICES'
        write(notice, 'DotCC shared runtime\nMIT\nCopyright Sebastian Godelet\n')
        with self.assertRaisesRegex(RuntimeError, 'full notice bodies'):
            audit.notices(self.project, pins, {}, repo)
        write(notice, 'DotCC shared runtime\n' + original_body + '\n')
        self.assertEqual(audit.notices(self.project, pins, {}, repo)[0]['source'], str(repo / 'LICENSE'))
        # Retain the actual body except its permission grant: labels still survive.
        write(notice, 'DotCC shared runtime\n' + original_body.split('Permission', 1)[0] + '\n')
        with self.assertRaisesRegex(RuntimeError, 'full notice bodies'):
            audit.notices(self.project, pins, {}, repo)

    def boundary(self, body=None):
        body = body or ('public static class Libc {\n[DllImport("libc", EntryPoint="close")]\n'
                        'private static extern int HostClose(int fd);\n'
                        'public static int close(int fd) { return HostClose(fd); }\n'
                        'public static IntPtr dlopen(string name) { return NativeLibrary.Load(name); }\n}')
        runtime_records = audit.methods(body, "authored.cs")
        runtime_imports = audit.native_imports(body, "authored.cs")
        return body, runtime_records, runtime_imports

    def test_host_import_inventory_allows_shared_services(self):
        body, records, imports = self.boundary()
        result = audit.host_boundary({"out.cs": body}, audit.methods(body, "out.cs"), records, imports, ["libc"])
        self.assertEqual(result["native_library_counts"], {"libc": 1})
        self.assertEqual(result["shared_runtime_dynamic_sites"][0]["method"], "dlopen")
        for changed in (body.replace('"libc"', '"terminal"'), body.replace('EntryPoint="close"', 'EntryPoint="terminal_new"')):
            with self.subTest(changed=changed), self.assertRaisesRegex(RuntimeError, "Unexpected native|imports differ"):
                audit.host_boundary({"out.cs": changed}, audit.methods(changed, "out.cs"), records, imports, ["libc"])
        with self.assertRaisesRegex(RuntimeError, "explicit allowed_host_libraries"):
            audit.host_boundary({"out.cs": body}, audit.methods(body, "out.cs"), records, imports, None)

    def test_variable_dynamic_loader_change_and_new_call_are_rejected(self):
        body, records, imports = self.boundary()
        for changed in (body.replace('NativeLibrary.Load(name)', 'NativeLibrary.Load(otherName)'),
                        body + '\nclass Engine {\npublic static void Start() { NativeLibrary.Load(variable); }\n}',
                        body + '\nclass DotCcImports {}'):
            with self.subTest(changed=changed), self.assertRaisesRegex(RuntimeError, "dynamic load closure|native import table"):
                audit.host_boundary({"out.cs": changed}, audit.methods(changed, "out.cs"), records, imports, ["libc"])

    def test_c_extern_attribution_uses_bodies_and_current_metadata(self):
        runtime = 'public static class Libc {\npublic static int memcpy(int x) { return x; }\n}'
        delivered = runtime + '\nclass GhosttyVt {\npublic static int api() { return memcpy(2) + dotcc_ghostty_api(); }\npublic static int dotcc_ghostty_api() { return 1; }\n}'
        objects = {"engine.cs": '//!!dotcc-obj import:memcpy delegate*<int,int>\n//!!dotcc-obj function:dotcc_ghostty_api\n',
                   "bridge.cs": '//!!dotcc-obj import:dotcc_ghostty_api delegate*<int>\n'}
        rows = audit.attribute_imports(objects, audit.methods(delivered), audit.methods(runtime, "runtime.cs"))
        self.assertEqual([r["symbol"] for r in rows], ["memcpy", "dotcc_ghostty_api"])
        self.assertTrue(all(r["selected_lexically"] for r in rows))
        self.assertEqual(rows[1]["translated_definitions"], ["engine.cs"])
        objects["engine.cs"] += '//!!dotcc-obj import:external_terminal_new delegate*<int>\n'
        # An absent, pruned prototype is inventoried without claiming resolution.
        absent = audit.attribute_imports(objects, audit.methods(delivered), audit.methods(runtime))
        self.assertEqual(absent[1]["resolution"], "unselected object candidate")
        delivered = delivered.replace('return memcpy(2)', 'return external_terminal_new() + memcpy(2)')
        with self.assertRaisesRegex(RuntimeError, "Unattributed selected C extern"):
            audit.attribute_imports(objects, audit.methods(delivered), audit.methods(runtime))

    def test_selected_loader_and_missing_runtime_body_rejected(self):
        runtime = 'public static class Libc {\npublic static int dlopen(int x) { return x; }\n}'
        delivered = runtime + '\nclass GhosttyVt {\npublic static int api() { return dlopen(2); }\n}'
        with self.assertRaisesRegex(RuntimeError, "selected a dynamic loader"):
            audit.attribute_imports({"engine": '//!!dotcc-obj import:dlopen delegate*<int,int>\n'},
                                    audit.methods(delivered), audit.methods(runtime))
        runtime = runtime.replace("dlopen", "memcpy")
        delivered = delivered.replace("dlopen", "memcpy").replace('return x;', 'return x+1;')
        with self.assertRaisesRegex(RuntimeError, "runtime body differs"):
            audit.attribute_imports({"engine": '//!!dotcc-obj import:memcpy delegate*<int,int>\n'},
                                    audit.methods(delivered), audit.methods(runtime))

    def closure_fixture(self):
        root = self.base / "campaign"
        native = root / "build/campaign/current/native"
        product, zig = root / "upstream", root / "zig"
        pins = {"dependencies": [{"name": "uucode", "zig_hash": "current-unicode", "role": "product Unicode"}],
                "upstream_version": "current", "library_version": "current", "build_system_integrations": ["freetype"]}
        policy = {"zig_root": "src/lib_vt.zig", "bridge": "src/boundary.c", "dialect": "c17", "c_target": "x86_64-freestanding-none",
                  "features": ["formatter"], "excluded_features": ["snapshot"], "native_build_arguments": ["-Dsimd=false"]}
        engine = write(native / "engine.c", 'zig_extern void ghostty_alloc(void);\n')
        bridge = write(root / policy["bridge"], 'void bridge(void) {}')
        write(product / policy["zig_root"], 'const lib = @This();\n')
        write(native / "source/src/lib_vt.zig", 'const lib = @This();\n\n// Authored ABI address accessor; upstream exports above are preserved.\ncomptime { _ = @import("dotcc_ghostty_api.zig"); }\n')
        write_json(root / "config/abi.json", {"operations": [{"id": 1, "name": "ghostty_alloc", "zig_member": "alloc"}]})
        for path in (native / "source/src/dotcc_ghostty_api.zig", native / "abi/dotcc_ghostty_api.zig"):
            write(path, 'const api = @import("terminal/c/main.zig");\n'
                  'export fn dotcc_ghostty_api(operation: c_int) ?*const anyopaque {\n'
                  '    return switch (operation) {\n        1 => @ptrCast(&api.alloc),\n'
                  '        else => null,\n    };\n}\n')
        paths = {"root": native / "source/src/lib_vt.zig", "build_config": product / "src/build/uucode_config.zig"}
        for name, filename in (("uucode", "root.zig"), ("types.zig", "types.zig"), ("config.zig", "config.zig"), ("storage.zig", "storage.zig")):
            paths[name] = write(product / "zig-pkg/current-unicode/src" / filename, '// current Unicode input\n')
        write(paths["build_config"], '// current config\n')
        for name, filename in (("build_options", "options.zig"), ("terminal_options", "options.zig"), ("unicode_tables", "props.zig"), ("symbols_tables", "symbols.zig"), ("tables", "tables.zig")):
            paths[name] = write(native / "zig-cache/o" / name / filename, '// current generated input\n')
        write(paths["terminal_options"], 'pub const c_abi: bool = true;\npub const formatter: bool = true;\npub const snapshot: bool = false;\npub const simd: bool = false;\npub const artifact: Artifact = .lib;\n')
        argv = [str(zig / "zig"), 'build-lib']
        for name, deps in audit.MODULE_DEPS.items():
            for dep in sorted(deps):
                argv += ['--dep', dep]
            if name == 'root':
                argv += ['-target', policy['c_target']]
            argv.append('-M' + name + '=' + str(paths[name]))
        argv += ['-fcompiler-rt', '-fubsan-rt', '-ofmt=c', '-femit-bin=' + str(engine)]
        includes = list(map(str, [zig / "lib", product / "include", native / "abi"]))
        closure = {"source": str(engine), "target": policy['c_target'], "command": argv, "includes": includes,
                   "generated_modules": [a for a in argv if a.startswith('-M')]}
        closure_path = write_json(native / "engine-closure.json", closure)
        write_json(native / "emit-c-command.json", argv)
        original_argv = [a for a in argv if a != '-ofmt=c' and not a.startswith('-femit-bin=')]
        i = original_argv.index('-target')
        del original_argv[i:i + 2]
        original_argv = ['-Mroot=' + str(product / policy['zig_root']) if a.startswith('-Mroot=') else a for a in original_argv]
        write_json(native / "native-engine-command.json", original_argv)
        build = [str(zig / "zig"), 'build', *policy['native_build_arguments'], '-Dversion-string=current',
                 '-Dlib-version-string=current', '--system', str(product / 'zig-pkg'), '-fno-sys=freetype']
        write_json(native / "native-command.json", build)
        commands = [{"label": "native-upstream-build", "command": build, "cwd": str(product), "status": "passed"},
                    {"label": "native-emit-whole-engine-c", "command": argv, "status": "passed"}]
        objects = []
        for index, source in enumerate((engine, bridge)):
            label = f'{index:03d}-' + source.stem
            obj = write(native.parent / "objects" / (label + '.cs'), '//!!dotcc-obj function:ghostty_alloc\n//!!dotcc-obj function:dotcc_ghostty_api\n' if index == 0 else '// bridge')
            objects.append(str(obj))
            flags = ['-std=c17', *[part for include in includes for part in ('-I', include)]]
            flags += ['--overrides-file', str(root / 'config/zig-c-overrides.json')] if index == 0 else ['-DDOTCC_GHOSTTY_INDIRECT_ABI=1']
            flags += ['--override-report', str(root / 'artifacts' / (label + '-overrides.jsonl'))]
            commands.append({"label": label, "command": ['dotnet', 'compiler.dll', *flags, str(source), '--emit=obj', '-o', str(obj)], "status": "passed"})
        commands.append({"label": "link", "command": ['dotnet', 'compiler.dll', *objects, '--runtime=c'], "status": "passed"})
        receipt = {"status": "passed", "profile": "scalar", "action": "translate", "hash_policy": "warn",
                   "tasks": [{"name": "translation", "status": "passed"}],
                   "staging": "build/campaign/current", "sources": {"product": str(product), "zig": str(zig)},
                   "selection": {"fetch": "never"}, "commands": commands, "units": [str(engine), str(bridge)],
                   "libghostty_selection": {"generated_c": str(engine), "engine_command": argv}}
        return root, receipt, pins, policy, closure, closure_path, paths

    def test_current_structural_source_closure_accepts_no_historical_hashes(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        result, objects, roots = audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        self.assertEqual(set(result['modules']), set(audit.MODULE_DEPS))
        self.assertEqual(len(objects), 2)
        self.assertEqual(result['providers'][0]['name'], 'uucode')

    def test_source_closure_accepts_verify_after_translation_and_rejects_incomplete_or_failed_runs(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        for status in ('running', 'passed'):
            with self.subTest(verify=status):
                current = deepcopy(receipt)
                current.update(action='verify', status=status)
                audit.source_closure(root, current, pins, policy, ['ghostty_alloc'])
        for action, status in (('verify', 'failed'), ('verify', 'cancelled'),
                               ('translate', 'running'), ('probe', 'passed')):
            with self.subTest(action=action, status=status):
                current = deepcopy(receipt)
                current.update(action=action, status=status)
                with self.assertRaisesRegex(RuntimeError, 'completed successful scalar translation task'):
                    audit.source_closure(root, current, pins, policy, ['ghostty_alloc'])
        for tasks in ([], [{'name': 'translation', 'status': 'running'}],
                      [{'name': 'translation', 'status': 'failed'}],
                      [{'name': 'translation', 'status': 'passed'}] * 2):
            with self.subTest(tasks=tasks):
                current = deepcopy(receipt)
                current.update(action='verify', status='running', tasks=tasks)
                with self.assertRaisesRegex(RuntimeError, 'completed successful scalar translation task'):
                    audit.source_closure(root, current, pins, policy, ['ghostty_alloc'])

    def test_normal_hook_retains_ctx_receipt_and_records_closure(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        required = ['ghostty_alloc', *['ghostty_export_' + str(i) for i in range(92)]]
        write(Path(closure['source']), ''.join('zig_extern void ' + name + '(void);\n' for name in required))
        engine_object = closure_path.parent.parent / 'objects/000-engine.cs'
        write(engine_object, ''.join('//!!dotcc-obj function:' + name + '\n' for name in [*required, 'dotcc_ghostty_api']))
        self.source = 'namespace Managed; public static partial class GhosttyVt {\n' + ''.join(
            'public static void ' + name + '() {}\n' for name in required) + '}\n'
        self.own({'Library.cs': self.source})
        pins.update(allowed_host_libraries=['libc'], license_inputs=[{'name': 'upstream', 'path': 'LICENSE', 'distribute': True}])
        product = Path(receipt['sources']['product'])
        write(product / 'LICENSE', 'Upstream license body\n')
        write(self.project.parent / 'THIRD-PARTY-NOTICES', 'upstream\nUpstream license body\n\n')
        write_json(root / 'config/source.json', pins)
        write_json(root / 'config/translation.json', policy)
        write_json(root / 'config/api-inventory.json', {'required_exports': required})
        write(root.parent / 'DotCC.Libc/Libc.cs', 'namespace DotCC.Libc; public static class Libc {}')
        receipt['outputs'] = {'raw': {'project': str(self.project), 'hashes': {}}}
        receipt_path = write_json(root / 'artifacts/campaign/current/receipt.json', receipt)
        write_json(root / 'artifacts/campaign/current-scalar.json', {'receipt': str(receipt_path), 'outputs': receipt['outputs']})
        artifacts = root / 'artifacts/test-audit'
        artifacts.mkdir(parents=True)
        ctx = SimpleNamespace(root=root, repo=root.parent, profile='scalar', artifacts=artifacts, receipt={})
        testing, translation = ModuleType('campaigns.testing'), ModuleType('campaigns.translation')
        testing.forms = lambda unused: ['raw']
        translation.check_product = lambda unused, form: self.project
        with patch.dict('sys.modules', {'campaigns.testing': testing, 'campaigns.translation': translation}):
            audit.run(ctx, None)
        report = json.loads(Path(ctx.receipt['delivery_audit']).read_text())
        self.assertEqual(report['results'][0]['actual_upstream_exports'], 93)
        self.assertEqual(report['source_closure']['modules']['root']['role'], 'staged real root')
        self.assertEqual(report['translation_receipt'], str(receipt_path))
        self.assertTrue(report['limits'])
        self.assertNotIn('native_terminal_imports', report['results'][0])
        write(self.project.parent / 'Library.cs', self.source + '\nclass Diagnosis {\npublic static int dotcc_abi_main() { return 0; }\n}')
        ctx.receipt.clear()
        with patch.dict('sys.modules', {'campaigns.testing': testing, 'campaigns.translation': translation}):
            with self.assertRaisesRegex(RuntimeError, 'Diagnostic entrypoint escaped'):
                audit.run(ctx, None)
        self.assertNotIn('delivery_audit', ctx.receipt)

    def test_unknown_module_include_feature_and_root_are_rejected(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        for mutate in (lambda c: c['includes'].append('/external/provider'),
                       lambda c: c['generated_modules'].append('-Mterminal=/external/provider.zig')):
            changed = deepcopy(closure)
            mutate(changed)
            write_json(closure_path, changed)
            with self.subTest(changed=changed), self.assertRaisesRegex(RuntimeError, "include/provider root|Module inventory"):
                audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        write_json(closure_path, closure)
        write(paths['terminal_options'], paths['terminal_options'].read_text().replace('snapshot: bool = false', 'snapshot: bool = true'))
        with self.assertRaisesRegex(RuntimeError, "terminal feature values"):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        write(paths['terminal_options'], paths['terminal_options'].read_text().replace('snapshot: bool = true', 'snapshot: bool = false'))
        write(paths['root'], '// substitute terminal implementation')
        with self.assertRaisesRegex(RuntimeError, "Staged root is not the real upstream root"):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])

    def sync_engine_command(self, receipt, closure, closure_path):
        argv = closure['command']
        closure['generated_modules'] = [a for a in argv if a.startswith('-M')]
        receipt['libghostty_selection']['engine_command'] = argv
        next(c for c in receipt['commands'] if c['label'] == 'native-emit-whole-engine-c')['command'] = argv
        write_json(closure_path, closure)
        write_json(closure_path.parent / 'emit-c-command.json', argv)
        original = [a for a in argv if a != '-ofmt=c' and not a.startswith('-femit-bin=')]
        i = original.index('-target')
        del original[i:i + 2]
        product = Path(receipt['sources']['product'])
        original = ['-Mroot=' + str(product / 'src/lib_vt.zig') if a.startswith('-Mroot=') else a for a in original]
        write_json(closure_path.parent / 'native-engine-command.json', original)

    def test_consistently_recorded_extra_module_provider_is_rejected(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        provider = write(root / 'external/terminal.zig', 'pub fn terminal() void {}')
        closure['command'].insert(-4, '-Mterminal_provider=' + str(provider))
        self.sync_engine_command(receipt, closure, closure_path)
        with self.assertRaisesRegex(RuntimeError, 'engine module/provider set'):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])

    def test_extra_object_include_and_emit_native_provider_are_rejected(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        object_command = next(c for c in receipt['commands'] if c['label'] == '000-engine')['command']
        object_command[2:2] = ['-I', '/external/provider']
        with self.assertRaisesRegex(RuntimeError, 'object input/flag/include/provider'):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        del object_command[2:4]
        closure['command'].insert(closure['command'].index('-ofmt=c'), '-lterminal')
        self.sync_engine_command(receipt, closure, closure_path)
        with self.assertRaisesRegex(RuntimeError, 'engine flag/native provider'):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])

    def test_off_mode_private_cache_accepts_without_provenance_hashes(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        receipt['hash_policy'] = 'off'
        i = closure['command'].index('-ofmt=c')
        closure['command'][i:i] = ['--global-cache-dir', str(closure_path.parent / 'zig-global-cache')]
        self.sync_engine_command(receipt, closure, closure_path)
        result, unused_objects, unused_roots = audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        self.assertEqual(result['receipt_profile'], 'scalar')

    def test_candidate_referenced_in_initializer_requires_real_provider(self):
        source = 'class GhosttyVt {\nprivate static int value = external_terminal_new();\n}'
        objects = {'engine': '//!!dotcc-obj import:external_terminal_new delegate*<int>\n'}
        with self.assertRaisesRegex(RuntimeError, 'Unattributed selected C extern'):
            audit.attribute_imports(objects, audit.methods(source), [], {'out.cs': source})

    def test_extra_translation_unit_and_off_cache_are_rejected(self):
        root, receipt, pins, policy, closure, closure_path, paths = self.closure_fixture()
        receipt['units'].append('/external/engine.c')
        with self.assertRaisesRegex(RuntimeError, "translation source closure"):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])
        receipt['units'].pop()
        receipt['hash_policy'] = 'off'
        with self.assertRaisesRegex(RuntimeError, "global-cache-dir"):
            audit.source_closure(root, receipt, pins, policy, ['ghostty_alloc'])


if __name__ == "__main__":
    unittest.main(verbosity=2)
