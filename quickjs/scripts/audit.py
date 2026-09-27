#!/usr/bin/env python3
"""Audit exact current-run source ownership, managed metadata and native dependencies."""
from pathlib import Path
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
APP_ASSEMBLIES = {'ManagedConsumer', 'Managed.QuickJs', 'TranslatedQuickJs', 'QuickJs.Behavior'}
NATIVE_NEEDED = {'libc.so.6', 'libm.so.6', 'libgcc_s.so.1', 'libdl.so.2', 'libpthread.so.0',
                 'librt.so.1', 'libresolv.so.2', 'ld-linux-x86-64.so.2'}


def sources(project, root):
    from campaigns.recipes import helper
    directory = project.parent.resolve()
    tree = ET.parse(project)
    if list(tree.iter('PackageReference')) or list(tree.iter('ProjectReference')):
        raise RuntimeError(f'Unexpected product dependency in {project}')
    owned = set(json.loads((directory / '.campaign-files.json').read_text()))
    generated = {directory / name for name in owned if name.endswith('.cs')}
    if not generated:
        raise RuntimeError(f'No generated sources in {directory}')
    rc_helpers = []
    inline_exports = json.loads((root / 'config/profile.json').read_text())['inline_exports']
    inline_definitions = {name: 0 for name in inline_exports}
    for path in generated:
        if not path.resolve().is_relative_to(directory) or not path.is_file():
            raise RuntimeError(f'Invalid generated source ownership: {path}')
        text = path.read_text()
        if re.search(r'\bDotCC_JS_\w+\b', text):
            raise RuntimeError(f'Unexpected forwarding wrapper instead of exported upstream inline: {path}')
        # The embedded shared runtime may retain its generic helper definition;
        # translated QuickJS must never call it after branch-hint overrides.
        hint_text = re.sub(r"public static long __builtin_expect\(long value, long expected\) => value;", "", text)
        if re.search(r"\b__builtin_expect\s*\(", hint_text):
            raise RuntimeError(f"Branch-hint call survived the value-only override: {path}")
        for name in inline_exports:
            inline_definitions[name] += len(re.findall(
                r'\bpublic\s+static\s+unsafe\s+\w+\s+' + re.escape(name) + r'\s*\(', text))
        rc_helpers.extend(re.findall(r'\bstatic\s+unsafe\s+JSRefCountHeader\s*\*\s+(__js_rc(?:__unit_[A-F0-9]+)?)\s*\(', text))
        # Compiler-generated per-symbol bindings are forbidden. The generic,
        # unused NativeImports helper in the shared runtime is permitted.
        if re.search(r'\bclass\s+DotCc(?:Static)?Imports\b|\bNativeImports\s*\.\s*(?:LoadLibrary|TryResolveExport)\s*\(', text):
            symbols = re.findall(r'NativeImports\.TryResolveExport\([^,]+,\s*"([^"]+)"', text)
            raise RuntimeError(f'Generated required-symbol native fallback in {path}: {symbols}')
    if rc_helpers != ['__js_rc']:
        raise RuntimeError(f'Expected one deduplicated __js_rc definition in {directory}: {rc_helpers}')
    if any(count != 1 for count in inline_definitions.values()):
        raise RuntimeError(f'Expected one original-name upstream inline export per API: {inline_definitions}')
    for path in directory.rglob('*.cs'):
        if not {'bin', 'obj'}.intersection(path.relative_to(directory).parts) and path not in generated:
            raise RuntimeError(f'Unowned compiled source in generated product: {path}')
    host = {path.resolve() for path in (root / 'src/Host').glob('*.cs')}
    explicit = set()
    for node in tree.iter('Compile'):
        include = node.get('Include')
        if include:
            path = (directory / include).resolve()
            if path not in host and path not in generated:
                raise RuntimeError(f'Unexpected linked implementation: {path}')
            explicit.add(path)
    if not host <= explicit:
        raise RuntimeError(f'Host implementations are not linked from original paths: {sorted(host - explicit)}')
    pin = json.loads((root / 'config/source.json').read_text())
    reference = root / 'ref' / pin['directory']
    notice_record = helper(root / 'scripts/notices.py').validate(reference)
    for name in ('LICENSE', 'VERSION', 'THIRD-PARTY-NOTICES'):
        expected = (root if name == 'THIRD-PARTY-NOTICES' else reference) / name
        if name not in owned or not (directory / name).is_file():
            raise RuntimeError(f'Missing upstream notice/identity: {directory / name}')
        if (directory / name).read_bytes() != expected.read_bytes():
            raise RuntimeError(f'Upstream notice/identity was not preserved: {directory / name}')
        item = next((node for node in tree.iter('None') if node.get('Update') == name), None)
        if item is None or item.get('TargetPath') != 'QuickJs.' + name or any(
                item.get(key) != 'PreserveNewest' for key in ('CopyToOutputDirectory', 'CopyToPublishDirectory')):
            raise RuntimeError(f'Upstream notice is not delivered by build/publish: {name}')
    # Runtime loader facilities belong to the shared runtime, never project host replacements.
    dynamic = re.compile(r'\b(?:DllImport|LibraryImport)\s*\(|\bNativeLibrary\s*\.|\bAssembly\s*\.\s*Load|\b(?:LoadLibrary|dlopen|SetDllImportResolver)\s*\(')
    authored = [*(root / 'src/Host').glob('*.cs'), *(root / 'src/Managed.QuickJs').glob('*.cs'), *(root / 'samples/ManagedConsumer').glob('*.cs')]
    for path in authored:
        if dynamic.search(path.read_text()):
            raise RuntimeError(f'Project-authored native/dynamic loading is prohibited: {path}')
    return dict(project=str(project), generated_source_count=len(generated), linked_hosts=sorted(map(str, host)),
                compiler_native_binding_symbols=[], upstream_notices=notice_record, inline_exports=inline_definitions,
                inline_deduplication={'__js_rc_definitions': len(rc_helpers), 'name': rc_helpers[0]})


def delivered_notices(directory, product):
    records = []
    for name in ('LICENSE', 'VERSION', 'THIRD-PARTY-NOTICES'):
        path = directory / ('QuickJs.' + name)
        if not path.is_file() or path.read_bytes() != (product.parent / name).read_bytes():
            raise RuntimeError(f'Built/published application did not preserve upstream notices: {path}')
        records.append(dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    return records


def dependency_manifest(path):
    if not path.is_file():
        raise RuntimeError(f'Required ordinary consumer dependency manifest missing: {path}')
    document = json.loads(path.read_text())
    libraries = document.get('libraries', {})
    if not libraries:
        raise RuntimeError(f'Empty dependency manifest: {path}')
    for identity, library in libraries.items():
        name = identity.split('/')[0]
        framework = library.get('type') == 'runtimepack' and name == 'runtimepack.Microsoft.NETCore.App.Runtime.linux-x64'
        if not framework and (library.get('type') != 'project' or name not in APP_ASSEMBLIES):
            raise RuntimeError(f'Unapproved dependency {identity} ({library.get("type")}): {path}')
    for target in document.get('targets', {}).values():
        for identity, entry in target.items():
            framework = identity.split('/')[0] == 'runtimepack.Microsoft.NETCore.App.Runtime.linux-x64'
            if not framework and (entry.get('native') or any(v.get('assetType') == 'native' for v in entry.get('runtimeTargets', {}).values())):
                raise RuntimeError(f'Unexpected native package dependency {identity}: {path}')
    return dict(path=str(path), libraries=sorted(libraries), sha256=hashlib.sha256(path.read_bytes()).hexdigest())


def published_il(ctx, project, publish_command, label):
    # Query the SDK's evaluated target using the exact publisher properties;
    # don't infer SDK directory naming or glob possibly stale build products.
    properties = [arg for arg in publish_command if arg.startswith('-p:')]
    properties += ['-p:Configuration=Release']
    if '-r' in publish_command:
        properties += ['-p:RuntimeIdentifier=' + publish_command[publish_command.index('-r') + 1]]
    if '--artifacts-path' in publish_command:
        properties += ['-p:ArtifactsPath=' + publish_command[publish_command.index('--artifacts-path') + 1]]
    target = ctx.run(['dotnet', 'msbuild', project, '-nologo', '-getProperty:TargetPath', *properties], label).strip()
    path = Path(target)
    if not path.is_absolute() or not path.is_file():
        raise RuntimeError(f'Exact NativeAOT input assembly absent for {project}: {target}')
    return path


def rooted_aot_map(rooted):
    path = Path(rooted.get('aot_map', ''))
    if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest() != rooted.get('aot_map_sha256'):
        raise RuntimeError(f'Actual rooted NativeAOT method map missing or changed: {path}')
    prefix = 'TranslatedQuickJs_Managed_Interpreters_QuickJs__'
    methods = {node.get('Name'): int(node.get('Length', '0'))
               for node in ET.parse(path).iter('MethodCode')
               if node.get('Name', '').startswith(prefix) and int(node.get('Length', '0')) > 0}
    representatives = ['JS_Eval', 'JS_CallInternal', 'js_closure2', 'lre_exec',
                       'unicode_from_utf8', 'dbuf_put', 'mp_add', 'JS_FreeValue', 'JS_DupValue',
                       'JS_FreeValueRT', 'JS_DupValueRT', 'JS_NewCFunctionMagic',
                       'js_atomics_op', 'js_atomics_store', 'js_atomics_isLockFree',
                       'js_atomics_pause', 'js_atomics_wait', 'js_atomics_notify']
    missing = [name for name in representatives if not any(
        method == prefix + name or method.startswith(prefix + name + '__unit_') for method in methods)]
    if missing:
        raise RuntimeError(f'Rooted NativeAOT map lacks translated machine code: {missing}')
    return dict(path=str(path), sha256=rooted['aot_map_sha256'],
                translated_method_count=len(methods), translated_code_bytes=sum(methods.values()),
                required_methods=representatives)


def run(ctx, suite):
    from campaigns.testing import forms, modes
    from campaigns.translation import check_product
    inspector_project = ctx.root / 'tests/Audit/QuickJs.Audit.csproj'
    inspector_output = ctx.work / 'audit-inspector'
    ctx.managed('build', inspector_project, 'audit-inspector-build', '-o', inspector_output)
    inspector = ['dotnet', inspector_output / 'QuickJs.Audit.dll']
    report = {'scope': 'exact current-run source, managed metadata and ELF dependencies', 'forms': [], 'artifacts': [], 'status': 'running'}
    output = ctx.artifacts / 'dependency-audit.json'
    try:
        consumer_project = ET.parse(ctx.root / 'samples/ManagedConsumer/ManagedConsumer.csproj')
        if list(consumer_project.iter('TrimmerRootAssembly')):
            raise RuntimeError('Separate consumer must exercise ordinary trimming')
        facade_project = ET.parse(ctx.root / 'src/Managed.QuickJs/Managed.QuickJs.csproj')
        if not any(node.get('Include') == '$(QuickJsProject)' for node in facade_project.iter('ProjectReference')):
            raise RuntimeError('Owning API must reference the selected generated product')
        harness_project = ET.parse(ctx.root / 'tests/Behavior/QuickJs.Behavior.csproj')
        if not any(node.get('Include') == 'TranslatedQuickJs' for node in harness_project.iter('TrimmerRootAssembly')):
            raise RuntimeError('ABI harness must support full-library AOT rooting')
        for form in forms(ctx):
            product = check_product(ctx, form)
            report['forms'].append(dict(form=form, **sources(product, ctx.root)))
            for mode in modes(ctx):
                label = f'consumer-{form}-{mode}-run'
                completed = [command for command in ctx.receipt['commands'] if command.get('label') == label and command.get('status') == 'passed']
                if len(completed) != 1:
                    raise RuntimeError(f'Required exact current-run consumer artifact absent: {label}; run consumer and abi before audit in the same invocation')
                command = completed[0]['command']
                consumer = Path(command[1] if mode == 'jit' else command[0])
                harnesses = [row for row in ctx.receipt.get('managed_harnesses', []) if row['form'] == form and row['mode'] == mode and row['whole_library_rooted']]
                if len(harnesses) != 1:
                    raise RuntimeError(f'Required rooted ABI harness absent for {form}/{mode}')
                rooted = harnesses[0]
                rooted_path = Path(rooted['executable'])
                if not rooted_path.is_file() or hashlib.sha256(rooted_path.read_bytes()).hexdigest() != rooted['executable_sha256']:
                    raise RuntimeError(f'Qualified ABI harness changed or disappeared: {rooted_path}')
                if not consumer.is_file():
                    raise RuntimeError(f'Executed consumer artifact missing: {consumer}')
                row = dict(form=form, mode=mode, consumer=str(consumer), rooted_harness=str(rooted_path), consumer_sha256=hashlib.sha256(consumer.read_bytes()).hexdigest())
                row['consumer_notices'] = delivered_notices(consumer.parent, product)
                row['harness_notices'] = delivered_notices(rooted_path.parent, product)
                report['artifacts'].append(row)
                if mode == 'jit':
                    row['consumer_dependencies'] = dependency_manifest(consumer.with_suffix('.deps.json'))
                    row['harness_dependencies'] = dependency_manifest(rooted_path.with_suffix('.deps.json'))
                    for kind, entry in [('consumer', consumer), ('harness', rooted_path)]:
                        arguments = [kind, entry, 'facade', entry.parent / 'Managed.QuickJs.dll', 'product', entry.parent / 'TranslatedQuickJs.dll']
                        metadata = ctx.run([*inspector, *arguments], f'audit-{form}-{mode}-{kind}-metadata')
                        row[kind + '_metadata'] = json.loads(metadata)
                else:
                    row['rooted_aot_code'] = rooted_aot_map(rooted)
                    for kind, entry in [('consumer', consumer), ('harness', rooted_path)]:
                        dynamic = ctx.run(['readelf', '-d', entry], f'audit-{form}-{mode}-{kind}-elf')
                        needed = re.findall(r'\(NEEDED\).*\[([^]]+)\]', dynamic)
                        if not needed:
                            raise RuntimeError(f'No dynamic dependency table found in NativeAOT output: {entry}')
                        unexpected = set(needed) - NATIVE_NEEDED
                        if unexpected:
                            raise RuntimeError(f'Unexpected NativeAOT dependencies {sorted(unexpected)}: {entry}')
                        row[kind + '_native_dependencies'] = needed
                    # AOT output need not retain IL; inspect the exact IL that fed publishing.
                    for kind, entry, build_label in [('consumer', consumer, f'consumer-{form}-aot-publish'), ('harness', rooted_path, f'harness-{form}-aot-rooted-publish')]:
                        builds = [c for c in ctx.receipt['commands'] if c.get('label') == build_label and c.get('status') == 'passed']
                        if len(builds) != 1:
                            raise RuntimeError(f'NativeAOT publishing receipt absent: {build_label}')
                        cmd = builds[0]['command']
                        project = ctx.root / ('samples/ManagedConsumer/ManagedConsumer.csproj' if kind == 'consumer' else 'tests/Behavior/QuickJs.Behavior.csproj')
                        path = published_il(ctx, project, cmd, f'audit-{form}-{mode}-{kind}-target')
                        # The application output contains the resolved project-reference
                        # copies passed to NativeAOT. Referenced projects may clear the
                        # application's RuntimeIdentifier, so querying them with its
                        # global properties would identify a nonexistent output.
                        arguments = [kind, path, 'facade', path.parent / 'Managed.QuickJs.dll',
                                     'product', path.parent / 'TranslatedQuickJs.dll']
                        row[kind + '_dependencies'] = dependency_manifest(path.with_suffix('.deps.json'))
                        metadata = ctx.run([*inspector, *arguments], f'audit-{form}-{mode}-{kind}-metadata')
                        row[kind + '_metadata'] = json.loads(metadata)
        report['status'] = 'pass'
        print(f'PASS exact artifact audit: {output}', flush=True)
    except BaseException as error:
        report['status'] = 'fail'
        report['error'] = str(error)
        raise
    finally:
        output.write_text(json.dumps(report, indent=2) + '\n')
        ctx.receipt['dependency_audit'] = str(output)
        ctx.save()


if __name__ == '__main__':
    os.execv(sys.executable, [sys.executable, str(ROOT.parent / 'Scripts/campaign.py'), 'test', 'quickjs', *sys.argv[1:], '--suite', 'audit'])
