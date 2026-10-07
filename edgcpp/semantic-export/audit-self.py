#!/usr/bin/env python3
"""Export each real cpfe-dot translation unit and inventory semantic adapter gaps.

Uses the native compilation database and pinned portable libc/libc++ headers.
The semantic producer still supplies host system predefines: this is a diagnostic
source audit, not a portable self-host or a linked C++ program. No backend source
is replaced by a provider or a previously generated C/C# file.
"""
import argparse
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import resource
import signal
import shlex
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from backend.self.profile import portable_self_flags


def digest(path):
    result = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            result.update(block)
    return result.hexdigest()


class JsonStream:
    """Bound memory to one graph record, rather than the whole graph."""
    def __init__(self, stream):
        self.stream, self.buffer, self.offset = stream, '', 0
        self.decoder = json.JSONDecoder()

    def more(self):
        data = self.stream.read(1024 * 1024)
        self.buffer = self.buffer[self.offset:] + data
        self.offset = 0
        if not data:
            raise ValueError('Truncated graph JSON')

    def peek(self):
        while True:
            while self.offset < len(self.buffer) and self.buffer[self.offset].isspace():
                self.offset += 1
            if self.offset < len(self.buffer):
                return self.buffer[self.offset]
            self.more()

    def token(self, expected):
        if self.peek() != expected:
            raise ValueError('Expected JSON token ' + expected)
        self.offset += 1

    def value(self):
        self.peek()
        while True:
            try:
                value, self.offset = self.decoder.raw_decode(self.buffer, self.offset)
                return value
            except json.JSONDecodeError:
                self.more()


def inventory(path):
    fields_source = (ROOT / 'semantic-export/managed/SemanticGraph.cs').read_text()
    fields = set(re.findall(r'"([A-Za-z]+)"', fields_source.split('ReferenceFields =', 1)[1].split('};', 1)[0]))
    ids, references, kinds, forms, gaps = set(), set(), Counter(), Counter(), Counter()
    header, examples, edge_count, duplicate_count = {}, {}, 0, 0
    with path.open() as stream:
        parser = JsonStream(stream)
        parser.token('{')
        while parser.peek() != '}':
            key = parser.value()
            parser.token(':')
            if key not in ('nodes', 'diagnostics', 'macroInvocations'):
                header[key] = parser.value()
            else:
                parser.token('[')
                while parser.peek() != ']':
                    item = parser.value()
                    if key == 'nodes':
                        node_id, kind = item['id'], item['kind']
                        duplicate_count += node_id in ids
                        ids.add(node_id)
                        kinds[kind] += 1
                        if 'form' in item:
                            forms[kind + ':' + item['form']] += 1
                        for field, value in item.items():
                            if field in fields and not (kind == 'variable' and field == 'parameter'):
                                if isinstance(value, int):
                                    references.add(value)
                        for edge in item.get('nativeEdges', []):
                            references.add(edge['target'])
                            edge_count += 1
                    elif key == 'diagnostics':
                        message = item['message']
                        gaps[message] += 1
                        examples.setdefault(message, item['node'])
                        references.add(item['node'])
                    elif key == 'macroInvocations':
                        header['macroInvocationCount'] = header.get('macroInvocationCount', 0) + 1
                        references.add(item['definition'])
                    if parser.peek() != ']':
                        parser.token(',')
                parser.token(']')
            if parser.peek() != '}':
                parser.token(',')
        parser.token('}')
    for key in ('rootScope', 'primarySource', 'mainRoutine'):
        references.add(header.get(key, 0))
    missing = references - ids - {0}
    return {'header': header, 'nodeCount': len(ids), 'nodeKinds': dict(kinds.most_common()),
            'forms': dict(forms.most_common()), 'adapterDiagnosticCount': sum(gaps.values()),
            'adapterDiagnostics': dict(gaps.most_common()), 'diagnosticExampleNodes': examples,
            'referenceClosure': {'nativeEdgeCount': edge_count, 'distinctReferencedIds': len(references - {0}),
                                 'duplicateNodeIds': duplicate_count, 'missingCount': len(missing),
                                 'missingIdExamples': sorted(missing)[:30],
                                 'scope': 'native edges and named top-level node reference fields; not a replacement for managed package validation'}}


def main():
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', required=True, type=Path)
    parser.add_argument('--libcxx-acquisition', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--source', action='append', help='select source basenames (default all 77 target TUs)')
    parser.add_argument('--timeout', type=int, default=600, help='per export timeout in seconds')
    parser.add_argument('--dot-system-include', type=Path,
                        help='explicit host architecture headers for libc++ features.h (diagnostic host profile)')
    parser.add_argument('--dot-header-profile', choices=('pinned-libcxx', 'host'), default='pinned-libcxx',
                        help='host uses the native compiler header search for the actual dot backend')
    parser.add_argument('--host-source', action='append', default=[],
                        help='additional source basename requiring the diagnostic host header profile')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    database = ROOT / 'build/compile_commands.json'
    entries = [entry for entry in json.loads(database.read_text()) if '/cpfe-dot.dir/' in entry['command']]
    if not entries:
        parser.error('No cpfe-dot entries in compilation database')
    if args.source:
        entries = [entry for entry in entries if Path(entry['file']).name in args.source]
        if set(args.source) != {Path(entry['file']).name for entry in entries}:
            parser.error('Requested source missing from cpfe-dot compilation database')
    # Test the actual dot backend first, then the primary program, then others.
    entries.sort(key=lambda entry: ({'dot_gen_be.c': 0, 'cfe.c': 1}.get(Path(entry['file']).name, 2), Path(entry['file']).name))
    acquisition = args.libcxx_acquisition.resolve()
    acquisition_manifest = acquisition / 'acquisition.json'
    manifest = json.loads(acquisition_manifest.read_text())
    pin = json.loads((ROOT / 'backend/runtime/libcxx/source.json').read_text())
    if manifest['source']['sha256'] != pin['sha256']:
        parser.error('libc++ source pin mismatch')
    header_hashes = {}
    for name, expected in manifest['fileHashes'].items():
        if name.startswith(('config/', pin['archiveRoot'] + '/include/')):
            actual = digest(acquisition / name)
            if actual != expected:
                parser.error('libc++ header/configuration hash mismatch: ' + name)
            header_hashes[name] = actual
    if not header_hashes:
        parser.error('No pinned libc++ header hashes')
    includes = [ROOT / 'backend/runtime/posix/include', ROOT / 'backend/self/include',
                acquisition / 'config', acquisition / pin['archiveRoot'] / 'include',
                ROOT / 'backend/runtime/libc/include', ROOT / 'backend/runtime/include']
    overrides = [*portable_self_flags(), '-DUSE_MMAP_FOR_MEMORY_REGIONS=0',
                 '-D__DATE__="Jan  1 1970"', '-D__TIME__="00:00:00"']
    target_macros = ROOT / 'backend/target/portable-lp64-binary80-v2.def'
    dot_overrides = ['-D' + name + '=' + value for name, value in
                     re.findall(r'DOT_TARGET_MACRO\((\w+), "(.*)"\)', target_macros.read_text())]
    host_includes = []
    if args.dot_header_profile == 'host' or args.host_source:
        native_compiler = shlex.split(entries[0]['command'])[0]
        search = subprocess.run([native_compiler, '-E', '-x', 'c++', '-', '-v'], input='',
                                text=True, capture_output=True, check=True).stderr
        host_includes = [Path(line.strip()) for line in search.split('#include <...> search starts here:')[1]
                         .split('End of search list.')[0].splitlines() if line.strip()]
        dot_overrides += ['-D__UINT64_MAX__=18446744073709551615UL', '-D__UINT32_MAX__=4294967295U']
    report = {'schema': 'edg-semantic-self-audit/1', 'program': 'cpfe-dot', 'sourceCount': len(entries),
              'scope': 'independent source translation units; no semantic link or code generation',
              'profile': 'portable provider headers and self-host facts with native semantic-producer system predefines',
              'limitations': ['Producer has no dot_sys_predef portable target hook; host Linux/x86 predefines remain.',
                              'Native exceptions and RTTI flags preserved; no runtime provider source injected.',
                              'Successful export does not establish complete semantic serialization or emission readiness.'],
              'compiler': str(args.compiler.resolve()), 'compilerSha256': digest(args.compiler.resolve()),
              'compileDatabase': str(database), 'compileDatabaseSha256': digest(database),
              'generatedConfigSha256': digest(ROOT / 'build/src/includes/cmake_defines.h'),
              'libcxxAcquisition': str(acquisition_manifest), 'libcxxAcquisitionSha256': digest(acquisition_manifest),
              'libcxxHeaderHashes': header_hashes, 'profileOverrides': overrides,
              'includeDirectories': list(map(str, includes)),
              'dotTargetMacroTableSha256': digest(target_macros),
              'dotSystemInclude': str(args.dot_system_include) if args.dot_system_include else None,
              'dotHeaderProfile': args.dot_header_profile, 'dotHostIncludes': list(map(str, host_includes)),
              'additionalHostSources': args.host_source,
              'canonicalDotBackendSha256': digest(ROOT / 'backend/dot_gen_be.c'),
              'dotBackendCopyMatchesCanonical': digest(ROOT / 'backend/dot_gen_be.c') == digest(ROOT / 'upstream/src/dot_gen_be.c'),
              'selfProfileSha256': digest(ROOT / 'backend/self/profile.py'),
              'auditToolSha256': digest(Path(__file__).resolve()), 'units': []}
    for entry in entries:
        source = Path(entry['file'])
        native = shlex.split(entry['command'])
        flags = [flag for flag in native if flag.startswith(('-D', '-I'))]
        flags += overrides + ['--c++', '--c++17', '--gnu_version', '130300']
        flags += ['--no_exceptions'] if '-fno-exceptions' in native else []
        flags += ['--no_rtti'] if '-fno-rtti' in native else []
        # Most EDG units need only the lightweight C/new headers. libc++ C
        # wrappers pull host features.h through __config under this producer.
        unit_includes = includes if source.name == 'dot_gen_be.c' else [includes[0], includes[1], includes[4], includes[5]]
        uses_host_headers = source.name in args.host_source or (source.name == 'dot_gen_be.c' and args.dot_header_profile == 'host')
        if uses_host_headers:
            unit_includes = host_includes
        flags += ['-I' + str(path) for path in unit_includes]
        if source.name == 'dot_gen_be.c' or uses_host_headers:
            flags += dot_overrides
            if args.dot_system_include:
                flags += ['-I' + str(args.dot_system_include.resolve())]
        package = out / source.stem
        command = [sys.executable, str(ROOT / 'semantic-export/export.py'), '--compiler', str(args.compiler.resolve()),
                   '--output', str(package), str(source), '--', *flags]
        result = {'source': str(source), 'sourceSha256': digest(source), 'nativeCompileCommand': entry,
                  'command': command, 'package': str(package)}
        start = time.monotonic()
        try:
            with (out / (source.stem + '.driver.log')).open('w') as log:
                process = subprocess.Popen(command, cwd=ROOT, stdout=log, stderr=subprocess.STDOUT, start_new_session=True)
                try:
                    process.wait(timeout=args.timeout)
                except subprocess.TimeoutExpired:
                    os.killpg(process.pid, signal.SIGKILL)
                    process.wait()
                    raise
            result['exitCode'] = process.returncode
        except subprocess.TimeoutExpired:
            result.update(exitCode=None, timedOut=True)
        result['seconds'] = round(time.monotonic() - start, 3)
        if result['exitCode'] == 0:
            try:
                result['inventory'] = inventory(package / 'graph.json')
                result['graphBytes'] = (package / 'graph.json').stat().st_size
                manifest = json.loads((package / 'package.json').read_text())
                primary_id = result['inventory']['header']['primarySource']
                primary = next((item for item in manifest['sources'] if item['node'] == primary_id), None)
                if primary is None:
                    raise ValueError('Primary source is missing from the package source manifest')
                snapshot_hash = digest(package / primary['path'])
                result['primarySourceProvenance'] = {
                    'beforeSha256': result['sourceSha256'], 'snapshotSha256': snapshot_hash,
                    'manifestSha256': primary['sha256'],
                    'originalPathMatches': Path(primary['originalPath']).resolve() == source.resolve(),
                    'beforeMatchesSnapshot': result['sourceSha256'] == snapshot_hash,
                }
                if (snapshot_hash != primary['sha256'] or snapshot_hash != result['sourceSha256'] or
                        not result['primarySourceProvenance']['originalPathMatches']):
                    raise ValueError('Primary-source bytes changed between pre-export hash and package snapshot')
            except (ValueError, KeyError) as error:
                result['inventoryError'] = str(error)
        report['units'].append(result)
        report['successfulExports'] = sum(unit['exitCode'] == 0 for unit in report['units'])
        report['failedExports'] = len(report['units']) - report['successfulExports']
        report['failedInventories'] = sum('inventoryError' in unit or
                                         unit.get('inventory', {}).get('referenceClosure', {}).get('missingCount', 0) != 0 or
                                         unit.get('inventory', {}).get('referenceClosure', {}).get('duplicateNodeIds', 0) != 0
                                         for unit in report['units'])
        temporary = out / 'results.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(out / 'results.json')
        print(f"{len(report['units'])}/{len(entries)} {source.name}: exit {result['exitCode']} "
              f"{result.get('inventory', {}).get('nodeCount', 0)} nodes "
              f"{result.get('inventory', {}).get('adapterDiagnosticCount', 0)} gaps", flush=True)
    return 0 if report['successfulExports'] == len(entries) and not report['failedInventories'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
