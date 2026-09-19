#!/usr/bin/env python3
"""Inventory final source dependencies and publish whole-assembly-rooted NativeAOT.

Builds use private copies, leaving the generated projects and their obj/bin alone.
This qualifies compilation/dependencies, not SMB protocol correctness.
"""
import argparse
import json
from pathlib import Path
import re
import shutil
import tempfile
import xml.etree.ElementTree as ET
from common import ROOT, run, sha

ALLOWED_IMPORTS = {
    'libc': {'link', 'chown', 'mkfifo', 'getrusage', 'getppid', 'kill'},
    'kernel32.dll': {'CreateHardLinkW', 'GetCurrentProcess', 'GetProcessTimes',
                     'CreateToolhelp32Snapshot', 'Process32FirstW', 'Process32NextW',
                     'CloseHandle', 'GetCurrentProcessId', 'OpenProcess', 'TerminateProcess'},
    'psapi.dll': {'GetProcessMemoryInfo'},
}
DYNAMIC_CODE = re.compile(r'\b(?:Reflection\.Emit|DynamicMethod|AssemblyBuilder|TypeBuilder|'
                          r'Assembly\.Load(?:From|File)?\s*\(|Expression\s*\.\s*Compile\s*\(|'
                          r'Marshal\.GetDelegateForFunctionPointer|Marshal\.GetFunctionPointerForDelegate)')
IMPORT = re.compile(r'\[(?:[\w:.]+\.)?(DllImport|LibraryImport)(?:Attribute)?\(\s*"([^"\n]+)"([^]]*)\)\]'
                    r'\s*((?:public|private|internal|protected|static|extern|partial|unsafe|\w+[?*]?|\s)+)'
                    r'\s+(\w+)\s*\(', re.MULTILINE)
LEXICAL = re.compile(r'//[^\n]*|/\*.*?\*/|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.DOTALL)


def without_comments(text):
    return LEXICAL.sub(lambda match: re.sub(r'[^\n]', ' ', match.group())
                       if match.group().startswith(('//', '/*')) else match.group(), text)


def source_files(project):
    return sorted(path for path in project.parent.rglob('*') if path.is_file()
                  and path.suffix in ('.cs', '.csproj')
                  and not {'bin', 'obj'}.intersection(path.relative_to(project.parent).parts))


def inventory(project):
    tree = ET.parse(project)
    assembly = tree.findtext('.//AssemblyName') or project.stem
    if assembly != 'TranslatedLibsmb2':
        raise RuntimeError('Rooted audit expects exact assembly name TranslatedLibsmb2: ' + assembly)
    result = dict(project=str(project), assembly=assembly, files={}, imports=[],
                  native_loader_helpers=[], dynamic_code=[], unexpected_dependencies=[],
                  translated_native_loader_calls=[], unparsed_import_attributes=[])
    for reference in tree.findall('.//PackageReference') + tree.findall('.//ProjectReference') + tree.findall('.//NativeLibrary'):
        result['unexpected_dependencies'].append(dict(kind=reference.tag, attributes=reference.attrib))
    for path in source_files(project):
        relative = str(path.relative_to(project.parent))
        result['files'][relative] = sha(path)
        if path.suffix != '.cs':
            continue
        original = path.read_text()
        code = without_comments(original)
        modules = []
        for match in re.finditer(r'^\s*// ---- ([\w.]+\.cs) ----', original, re.MULTILINE):
            modules.append((match.start(), match.group(1)))

        def location(offset):
            module = None
            for start, name in modules:
                if start > offset:
                    break
                module = name
            return dict(file=relative, line=original.count('\n', 0, offset) + 1, runtime_module=module)

        parsed_imports = list(IMPORT.finditer(code))
        all_imports = list(re.finditer(r'\[\s*(?:[\w:.]+\.)?(?:DllImport|LibraryImport)(?:Attribute)?\s*\(', code))
        parsed_offsets = {match.start() for match in parsed_imports}
        result['unparsed_import_attributes'].extend(location(match.start()) for match in all_imports
                                                     if match.start() not in parsed_offsets)
        for match in parsed_imports:
            kind, library, options, declaration, method = match.groups()
            entry = re.search(r'EntryPoint\s*=\s*"([^"]+)"', options)
            symbol = entry.group(1) if entry else method
            allowed = library in ALLOWED_IMPORTS and symbol in ALLOWED_IMPORTS[library]
            result['imports'].append(dict(**location(match.start()), kind=kind, library=library,
                                           symbol=symbol, method=method, generic_host_allowlisted=allowed))
        for match in DYNAMIC_CODE.finditer(code):
            result['dynamic_code'].append(dict(**location(match.start()), expression=match.group()))
        for match in re.finditer(r'\bNativeLibrary\.\w+\s*\(', code):
            loc = location(match.start())
            result['native_loader_helpers'].append(dict(**loc, expression=match.group()))
            if loc['runtime_module'] not in ('DlfcnLib.cs', 'NativeImports.cs'):
                result['translated_native_loader_calls'].append(dict(**loc, expression=match.group()))
        for match in re.finditer(r'\b(?:NativeImports\.(?:LoadLibrary|TryResolveExport)|DotCcImports\.\w+|dlopen|dlsym)\s*\(', code):
            loc = location(match.start())
            if loc['runtime_module'] not in ('DlfcnLib.cs', 'NativeImports.cs'):
                result['translated_native_loader_calls'].append(dict(**loc, expression=match.group()))
    result['static_passed'] = (all(item['generic_host_allowlisted'] for item in result['imports'])
                               and not result['dynamic_code'] and not result['unexpected_dependencies']
                               and not result['translated_native_loader_calls'] and not result['unparsed_import_attributes'])
    return result


def inspect_aot_artifacts(harness, audit):
    maps = list(harness.rglob('ProductAudit.map.xml'))
    responses = list(harness.rglob('ProductAudit.ilc.rsp'))
    if len(maps) != 1 or len(responses) != 1:
        raise RuntimeError('Expected one NativeAOT map and compiler response file')
    arguments = responses[0].read_text().splitlines()
    if '--root:TranslatedLibsmb2' not in arguments:
        raise RuntimeError('Native compiler did not receive the complete generated assembly root')
    method_names = [node.attrib['Name'] for node in ET.parse(maps[0]).iter('MethodCode')
                    if node.attrib.get('Name', '').startswith('TranslatedLibsmb2_')]
    if not any('__smb2_connect_share' in name for name in method_names):
        raise RuntimeError('Native map lacks uninvoked rooted SMB methods')
    audit['aot_map'] = str(maps[0].relative_to(ROOT))
    audit['aot_map_sha256'] = sha(maps[0])
    audit['aot_response'] = str(responses[0].relative_to(ROOT))
    audit['aot_response_sha256'] = sha(responses[0])
    audit['aot_root_argument'] = '--root:TranslatedLibsmb2'
    audit['compiled_generated_method_count'] = len(method_names)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--static-only', action='store_true', help='Inventory sources without any .NET build')
    parser.add_argument('--variant', choices=['raw', 'processed', 'both'], default='both')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    logs = ROOT / 'artifacts/product-audit'
    logs.mkdir(parents=True, exist_ok=True)
    build = ROOT / 'build/product-audit'
    build.mkdir(parents=True, exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix='run-', dir=build))
    receipt = dict(passed=False, static_only=args.static_only, rid=args.rid,
                   scope='source dependency inventory and complete generated assembly NativeAOT compilation',
                   limitations=['Not SMB behavioral qualification.',
                                'Source scanning is an inventory, not a complete call-graph proof.',
                                'Generic host P/Invokes and dormant native loader helpers remain in the runtime.'],
                   staging_directory=str(stage.relative_to(ROOT)), variants={})
    try:
        for variant, directory in [('raw', 'TranslatedLibsmb2.Raw'), ('processed', 'TranslatedLibsmb2')]:
            if args.variant not in ('both', variant):
                continue
            project = ROOT / 'generated' / directory / 'TranslatedLibsmb2.csproj'
            if not project.is_file():
                raise RuntimeError('Missing final product: ' + str(project))
            audit = inventory(project)
            receipt['variants'][variant] = audit
            if not audit['static_passed']:
                raise RuntimeError('Unexpected dependency or dynamic-code finding in ' + variant)
            print(f'{variant}: {len(audit["imports"])} generic host imports, '
                  f'{len(audit["native_loader_helpers"])} dormant native loader call sites; static inventory PASS', flush=True)
            if args.static_only:
                continue
            destination = stage / variant
            library = destination / 'library'
            harness = destination / 'harness'
            library.mkdir(parents=True)
            harness.mkdir()
            for relative in audit['files']:
                target = library / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(project.parent / relative, target)
            if audit['files'] != {relative: sha(library / relative) for relative in audit['files']}:
                raise RuntimeError('Generated sources changed while snapshotting')
            for path in (ROOT / 'tests/ProductAudit').iterdir():
                if path.suffix in ('.cs', '.csproj'):
                    shutil.copyfile(path, harness / path.name)
            isolated_project = harness / 'ProductAudit.csproj'
            option = '-p:Libsmb2GeneratedProject=' + str(library / 'TranslatedLibsmb2.csproj')
            evaluated = run(['dotnet', 'msbuild', isolated_project, option, '-getItem:TrimmerRootAssembly'],
                            logs / (variant + '-roots.log'), receipt)
            roots = json.loads(evaluated)['Items']['TrimmerRootAssembly']
            if not any(item['Identity'] == 'TranslatedLibsmb2' for item in roots):
                raise RuntimeError('Complete generated assembly is not an evaluated AOT root')
            audit['evaluated_roots'] = roots
            publish = destination / 'publish'
            run(['dotnet', 'publish', isolated_project, '-c', 'Release', '-r', args.rid,
                 '-p:PublishAot=true', '-p:IlcGenerateMapFile=true', option, '-o', publish, '--nologo'],
                logs / (variant + '-aot-build.log'), receipt, timeout=1800)
            binary = publish / 'ProductAudit'
            output = run([binary], logs / (variant + '-aot-run.log'), receipt, timeout=30).strip()
            if not re.fullmatch(r'ProductAudit: smb2_context=\d+', output):
                raise RuntimeError('Unexpected audit harness output: ' + output)
            audit['aot_output'] = output
            audit['aot_binary_sha256'] = sha(binary)
            inspect_aot_artifacts(harness, audit)
            audit['aot_analysis_warnings'] = re.findall(r'warning IL\d+[^\n]*',
                (logs / (variant + '-aot-build.log')).read_text())
            if audit['aot_analysis_warnings']:
                raise RuntimeError('NativeAOT/trim analysis warnings require review')
            if args.rid.startswith('linux-'):
                dependencies = run(['readelf', '-d', binary], logs / (variant + '-elf-dynamic.log'), receipt)
                audit['elf_needed'] = re.findall(r'\(NEEDED\).*?\[([^]]+)\]', dependencies)
                forbidden = re.compile(r'smb|crypto|ssl|krb|gss|ws2_32|secur32', re.IGNORECASE)
                if any(forbidden.search(name) for name in audit['elf_needed']):
                    raise RuntimeError('Native SMB/crypto dependency needs origin review (BCL implementation dependencies may be legitimate)')
            if audit['files'] != inventory(project)['files']:
                raise RuntimeError('Generated sources changed during audit')
            print(variant + ': whole-assembly-rooted NativeAOT publish and execution PASS', flush=True)
        if not args.static_only and len({item['aot_output'] for item in receipt['variants'].values()}) != 1:
            raise RuntimeError('Raw and processed audit ABI anchors differ')
        receipt['passed'] = True
    except BaseException as error:
        receipt['failure'] = str(error)
        raise
    finally:
        (logs / ('static-result.json' if args.static_only else 'result.json')).write_text(json.dumps(receipt, indent=2) + '\n')


if __name__ == '__main__':
    main()
