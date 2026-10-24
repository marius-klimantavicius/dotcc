#!/usr/bin/env python3
"""Bounded ordinary original-body controls; never a full original-suite runner.

Only selection.json's exact paths are executable. Global s2n_init is deliberately
absent. Original test sources/assertions are included unchanged, and BEGIN_TEST
uses the original NO_INIT startup followed by the real memory initializer.
Neither providers nor protocol engines are linked into the executable closure.
"""
import argparse
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import resource
import shlex
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(REPO / 'Scripts'))
from managed_test_storage import require_storage_reserve, validate_output_root

POLICY = json.loads((HERE / 'selection.json').read_text())
RESERVE = 2 * 1024**3
MODES = ('native-isolated', 'raw-jit', 'raw-aot', 'processed-jit', 'processed-aot')
CORE = ['029-s2n_errno', '030-s2n_stuffer', '032-s2n_stuffer_hex',
        '033-s2n_stuffer_network_order', '035-s2n_stuffer_text', '162-s2n_blob',
        '163-s2n_ensure', '168-s2n_mem', '171-s2n_safety', '025-s2n_sequence',
        '170-s2n_rfc5952']


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n')


def function(source, name):
    """Copy one original simple top-level function, without rewriting its body."""
    match = re.search(r'(?m)^int ' + re.escape(name) + r'\([^;{}]*\)\n\{.*?^\}', source, re.S | re.M)
    if not match:
        raise RuntimeError('Missing unique original function: ' + name)
    return match.group(0)


def limits():
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    resource.setrlimit(resource.RLIMIT_CPU, (1200, 1200))
    resource.setrlimit(resource.RLIMIT_FSIZE, (2 * 1024**3, 2 * 1024**3))


class Run:
    def __init__(self, args):
        self.args = args
        self.work = validate_output_root(args.output, protected_roots=(REPO,))
        if self.work == Path('/mnt/storage') or not self.work.is_relative_to(Path('/mnt/storage')):
            raise RuntimeError('Use a real private external directory under /mnt/storage')
        require_storage_reserve((REPO, self.work))
        self.work.mkdir(parents=True, exist_ok=False)
        (self.work / 'logs').mkdir()
        (self.work / 'tmp').mkdir()
        self.env = dict(os.environ, TMPDIR=str(self.work / 'tmp'),
                        DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_PROCESSOR_COUNT='2',
                        MSBUILDDISABLENODEREUSE='1')
        self.campaign = args.campaign.resolve()
        self.source = self.campaign / 'source'
        self.compiler = self.campaign / 'tools/DotCC/dotcc.dll'
        self.postprocessor = self.campaign / 'tools/DotCC.PostProcess/dotcc-postprocess.dll'
        self.native_report = ROOT / 'artifacts/native-profile-upstream/all-required-214654-ktls-guards/report.json'
        self.ledger = ROOT / 'build/provider/required257-223542/progress.json'
        self.report = {'schema_version': 1, 'status': 'running',
                       'scope': POLICY['scope'], 'canonical_credit': False,
                       'commands': [], 'results': [], 'inputs_sha256': {},
                       'context_inputs_sha256': {str(ROOT / 'docs' / n): sha(ROOT / 'docs' / n)
                           for n in ('PLAN.md', 'validation.md', 'blockers.md', 'orchestration.json')},
                       'limits': {'serial': True, 'reserve_bytes': RESERVE,
                                  'cpu_seconds_per_command': 1200, 'max_file_bytes': 2 * 1024**3,
                                  'runtime_timeout_seconds': 90, 'dotnet_processor_count': 2}}
        shutil.copyfile(HERE / 'qualify.py', self.work / 'harness.py')
        shutil.copyfile(HERE / 'selection.json', self.work / 'selection.json')
        self.protect([self.ledger, self.native_report, ROOT / 'config/upstream-tests.json',
                      self.compiler, self.postprocessor, self.work / 'harness.py', self.work / 'selection.json'])

    def protect(self, paths):
        self.report['inputs_sha256'].update({str(p): sha(p) for p in paths})

    def save(self):
        write(self.work / 'receipt.json', self.report)

    def command(self, argv, label, timeout=300):
        for location in (REPO, self.work):
            free = shutil.disk_usage(location).free
            self.report.setdefault('reserve_checks', []).append({'path': str(location), 'free_bytes': free})
            if free < RESERVE:
                raise RuntimeError('2 GiB reserve unavailable at ' + str(location))
        log = self.work / 'logs' / (f'{len(self.report["commands"]):03d}-' + label + '.log')
        argv = list(map(str, argv))
        row = {'argv': argv, 'cwd': str(self.work), 'timeout_seconds': timeout,
               'log': str(log), 'started_unix': time.time()}
        self.report['commands'].append(row)
        self.save()
        print(label, flush=True)
        with log.open('w') as stream:
            try:
                process = subprocess.run(argv, cwd=self.work, env=self.env, stdout=stream,
                                         stderr=subprocess.STDOUT, timeout=timeout, preexec_fn=limits)
                row['exit_code'] = process.returncode
            except subprocess.TimeoutExpired:
                row['exit_code'] = 'timeout'
        row.update(seconds=time.time() - row['started_unix'], log_sha256=sha(log))
        self.save()
        return row

    def required(self, argv, label, timeout=300):
        result = self.command(argv, label, timeout)
        if result['exit_code'] != 0:
            raise RuntimeError('Command failed; preserved ' + result['log'])
        return result

    def audit(self):
        inventory = json.loads((ROOT / 'config/upstream-tests.json').read_text())
        ledger = json.loads(self.ledger.read_text())
        native = {r['case']: r for r in json.loads(self.native_report.read_text())['upstream']}
        rows = []
        for case in inventory['cases']:
            path = case['path']
            if not (case['status'].startswith('required') and path.endswith('.c')):
                continue
            original = ROOT / 'ref/s2n-tls' / path
            text = original.read_text()
            name = original.stem.removeprefix('s2n_').removesuffix('_test')
            prior = ledger['cases'][path]['status']
            if path in POLICY['runnable']:
                status, reason = 'runnable-isolated-body', POLICY['runnable'][path]
            elif prior == 'passed':
                status, reason = 'already-qualified', 'Preserved final-epoch all4 case pass; no fresh run required in this lane.'
            elif name == 'connection_preferences':
                status, reason = 'already-qualified-separate', 'Separate actual132 all4 receipt exists; canonical failed history is retained. No repeat.'
            else:
                status = 'held-host-gap' if name == 'io' else 'held'
                reason = POLICY['held_ordinary_candidates'].get(name,
                    'Original whole case tests protocol/cipher/provider/credential behavior; outside the ordinary lane. No body extraction or main execution.')
            rows.append({'case': path, 'classification': status, 'reason': reason,
                         'historical_status': prior, 'original_sha256': sha(original),
                         'assertion_sites': [{'line': i, 'text': line.strip()} for i, line in enumerate(text.splitlines(), 1)
                                             if re.search(r'\bEXPECT_[A-Z_]+\s*\(', line)],
                         'direct_s2n_calls': sorted(set(re.findall(r'\b(s2n_\w+)\s*\(', text))),
                         'native_assertions': native[path]['assertions'],
                         'native_original_sha256': native[path]['original_sha256'],
                         'native_log': native[path]['log']})
        if len(rows) != 257 or set(POLICY['runnable']) - {r['case'] for r in rows}:
            raise RuntimeError('Full257 inventory or selection mismatch')
        write(self.work / 'inventory.json', {'counts': dict(Counter(r['classification'] for r in rows)),
                                            'historical_counts': ledger['counts'], 'cases': rows})
        self.report.update(inventory=str(self.work / 'inventory.json'),
                           inventory_sha256=sha(self.work / 'inventory.json'), historical_counts=ledger['counts'],
                           classification_counts=dict(Counter(r['classification'] for r in rows)))
        self.native = native
        self.save()

    def execute(self):
        requested = self.args.case
        if not requested or len(set(requested)) != len(requested) or set(requested) - POLICY['runnable'].keys():
            raise RuntimeError('Supply explicit unique --case paths from selection.json')
        self.report['selected_cases'] = requested
        self.report['bounded_batches'] = [[p for p in batch if p in requested] for batch in POLICY['batches']]
        receipt = json.loads((ROOT / 'artifacts/campaign' / self.campaign.name / 'receipt.json').read_text())
        original = next(r['command'] for r in receipt['commands'] if '--emit=obj' in r['command'])
        opts = original[2:original.index('--emit=obj') - 1]
        for flag in ('--override-report', '--overrides-file'):
            if flag in opts:
                index = opts.index(flag)
                del opts[index:index + 2]
        # Original objects embed the entire provider binding catalogue. Re-emit only
        # unchanged ordinary TUs without that unused catalogue; no provider is linked.
        opts = ['-I', str(self.source / 'tests'), *opts]
        self.report['compiler_options'] = opts
        self.report['ordinary_emission_reason'] = 'Original product objects preserve unused provider overrides; fresh unchanged ordinary TUs omit that catalogue to make provider runtime dependencies impossible.'
        include = self.work / 'noncrypto-startup.h'
        include.write_text('#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n'
                           '#include "s2n_test.h"\n#include "utils/s2n_mem.h"\n'
                           '#undef BEGIN_TEST\n#define BEGIN_TEST() do { BEGIN_TEST_NO_INIT(); '
                           'EXPECT_SUCCESS_WITHOUT_COUNT(s2n_mem_init()); } while (0)\n')
        init = (self.source / 'utils/s2n_init.c').read_text()
        tls13 = (self.source / 'tls/s2n_tls13.c').read_text()
        bootstrap = self.work / 'bootstrap.c'
        bootstrap.write_text('#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n'
                             '#include "utils/s2n_mem.h"\n#include "utils/s2n_safety.h"\n'
                             '#include "tls/s2n_tls13.h"\n'
                             'static bool atexit_cleanup = false;\n'
                             'uint8_t s2n_highest_protocol_version = S2N_TLS13;\n'
                             'bool s2n_use_default_tls13_config_flag = false;\n' +
                             '\n'.join([function(init, 's2n_enable_atexit'), function(init, 's2n_cleanup'),
                                        function(tls13, 's2n_disable_tls13_in_test')]) + '\n')
        self.report['startup'] = {'description': 'Original BEGIN_TEST_NO_INIT plus actual s2n_mem_init. No global s2n_init or atexit registration.',
                                  'copied_verbatim_functions': ['s2n_enable_atexit', 's2n_cleanup', 's2n_disable_tls13_in_test'],
                                  'full_original_execution': False}
        self.protect([self.source / 'utils/s2n_init.c', self.source / 'tls/s2n_tls13.c',
                      self.source / 'tests/s2n_test.h'])
        original_objects = [self.campaign / 'objects' / (name + '.cs') for name in CORE]
        self.protect(original_objects)
        self.report['preserved_original_product_objects'] = {str(p): sha(p) for p in original_objects}
        objects = []
        native_flags = [x for x in opts if x.startswith('-D')]
        native_flags += ['-I', self.source, '-I', self.source / 'tests', '-I', self.source / 'api',
                         '-I', ROOT / 'src/Boundary', '-I', self.campaign / 'include',
                         '-I', ROOT / 'ref/openssl-legacy/include']
        native_core = []
        for obj in original_objects:
            name = obj.stem.split('-', 1)[1]
            matches = list(self.source.glob('*/' + name + '.c'))
            if len(matches) != 1:
                raise RuntimeError('Native core source ambiguity: ' + name)
            source = matches[0]
            self.protect([source])
            managed = self.work / (name + '-ordinary.cs')
            self.required(['dotnet', self.compiler, *opts, source, '--emit=obj', '-o', managed], 'emit-core-' + name)
            objects.append(managed)
            target = self.work / (name + '.o')
            self.required(['cc', '-std=c99', '-O2', '-ffunction-sections', '-fdata-sections',
                           *native_flags, '-c', source, '-o', target], 'native-core-' + name)
            native_core.append(target)
        extra = [bootstrap]
        if any('mem_testlib' in p for p in requested):
            extra.append(self.source / 'tests/testlib/s2n_mem_testlib.c')
        for source in extra:
            self.protect([source])
            target = self.work / (source.stem + '-support.cs')
            self.required(['dotnet', self.compiler, *opts, source, '--emit=obj', '-o', target], 'emit-' + source.stem)
            objects.append(target)
            target_o = self.work / (source.stem + '-support.o')
            self.required(['cc', '-std=c99', '-O2', '-ffunction-sections', '-fdata-sections',
                           *native_flags, '-c', source, '-o', target_o], 'native-' + source.stem)
            native_core.append(target_o)
        dispatch = []
        for case in requested:
            source = ROOT / 'ref/s2n-tls' / case
            self.protect([source])
            if sha(source) != self.native[case]['original_sha256']:
                raise RuntimeError('Original test differs from native original source: ' + case)
            entry = 'ordinary_' + source.stem
            wrapper = self.work / (source.stem + '.c')
            wrapper.write_text('#define test_count ' + entry + '_count\n'
                               '#define s2n_use_color_in_output ' + entry + '_color\n'
                               '#include "noncrypto-startup.h"\n#define main ' + entry + '\n'
                               '#include "' + str(source) + '"\n')
            target = wrapper.with_suffix('.cs')
            dep = wrapper.with_suffix('.d')
            self.required(['dotnet', self.compiler, *opts, wrapper, '--emit=obj', '-MD', '-MF', dep, '-o', target], 'emit-' + source.stem)
            deps = shlex.split(dep.read_text().replace('\\\n', ' ').split(':', 1)[1])
            self.protect([Path(p) for p in deps])
            objects.append(target)
            binary = self.work / (source.stem + '-native')
            # The native control uses exactly the same wrapper and memory-only startup.
            self.required(['cc', '-std=c99', '-O2', '-ffunction-sections', '-fdata-sections', *native_flags,
                           '-D' + entry + '=main', wrapper, *native_core, '-Wl,--gc-sections', '-o', binary], 'native-link-' + source.stem)
            native_run = self.command([binary], 'native-run-' + source.stem, 90)
            self.result(case, 'native-isolated', native_run, binary)
            signature = re.search(r'int main\(([^)]*)\)', source.read_text()).group(1)
            call = '0, null' if 'argc' in signature else ''
            dispatch.append('"' + case + '" => Raw.' + entry + '(' + call + '),')
        raw = self.work / 'raw'
        self.required(['dotnet', self.compiler, *objects, '--emit=managedlib', '--nest-types', '--runtime=c',
                       '--class-name', 'S2nTls', '--namespace', 'Managed.Security.S2n', '--split=size',
                       '--split-size=262144', '--literal-pool', '--deduplicate-inline', '--prune-unused-inline',
                       '-o', raw], 'link-ordinary', 600)
        project = raw / (raw.name + '.csproj')
        tree = ET.parse(project)
        # Use the preserved runtime DLL, without rebuilding shared projects/caches.
        for group in list(tree.getroot()):
            for node in list(group):
                if node.tag == 'ProjectReference':
                    raise RuntimeError('Unexpected project reference from isolated linker: ' + str(node.attrib))
        group = ET.SubElement(tree.getroot(), 'PropertyGroup')
        for key, value in {'OutputType': 'Exe', 'AssemblyName': 'OrdinaryQualification',
                           'StartupObject': 'OrdinaryMain', 'TargetFramework': 'net10.0',
                           'LangVersion': '14', 'ImplicitUsings': 'enable'}.items():
            ET.SubElement(group, key).text = value
        tree.write(project, encoding='unicode')
        (raw / 'OrdinaryMain.cs').write_text('using Raw = Managed.Security.S2n.S2nTls;\n'
            'internal static unsafe class OrdinaryMain { static int Main(string[] args) {\n'
            'if (args.Length != 1) return 64; return args[0] switch {\n' + '\n'.join(dispatch) +
            '\n_ => 64 }; } }\n')
        self.report['generated_raw_sha256'] = {str(p): sha(p) for p in raw.glob('*.cs')}
        self.required(['dotnet', 'build', project, '-c', 'Release', '-m:1', '-p:UseSharedCompilation=false'], 'build-raw', 1200)
        processed = self.work / 'processed'
        shutil.copytree(raw, processed, ignore=shutil.ignore_patterns('bin', 'obj'))
        self.required(['dotnet', 'build', processed / project.name, '-c', 'Release', '-m:1', '-p:UseSharedCompilation=false'], 'semantic-context', 1200)
        self.required(['dotnet', self.postprocessor, processed / project.name, '--in-place'], 'postprocess', 900)
        self.report['generated_processed_sha256'] = {str(p): sha(p) for p in processed.glob('*.cs')}
        for form, directory in [('raw', raw), ('processed', processed)]:
            self.required(['dotnet', 'build', directory / project.name, '-c', 'Release', '-m:1', '-p:UseSharedCompilation=false'], 'build-' + form, 1200)
            dll = directory / 'bin/Release/net10.0/OrdinaryQualification.dll'
            deps = dll.with_suffix('.deps.json').read_text()
            if re.search(r'BouncyCastle|CryptoProvider|Managed\.S2nTls', deps):
                raise RuntimeError('Forbidden provider dependency in isolated runtime closure')
            self.required(['dotnet', 'publish', directory / project.name, '-c', 'Release', '-r', 'linux-x64',
                           '-p:PublishAot=true', '-p:UseSharedCompilation=false', '-m:1', '-o', directory / 'publish'], 'publish-' + form, 1200)
            binary = directory / 'publish/OrdinaryQualification'
            for mode, prefix, output in [('jit', ['dotnet', dll], dll), ('aot', [binary], binary)]:
                for batch in self.report['bounded_batches']:
                    for case in batch:
                        run = self.command([*prefix, case], form + '-' + mode + '-' + Path(case).stem, 90)
                        self.result(case, form + '-' + mode, run, output)
        self.report['runtime_outputs_sha256'] = {str(p): sha(p) for directory in (raw, processed)
            for sub in ('bin', 'publish') for p in (directory / sub).rglob('*') if p.is_file()}

    def result(self, case, mode, command, binary):
        content = Path(command['log']).read_text()
        counts = re.findall(r'PASSED\s+(\d+) tests', content)
        expected = self.native[case]['assertions']
        count = int(counts[0]) if len(counts) == 1 else None
        passed = command['exit_code'] == 0 and count == expected and 'SKIPPED' not in content and 'FAILED' not in content
        self.report['results'].append({'case': case, 'mode': mode, 'status': 'passed' if passed else 'failed',
            'actual_assertions': count, 'native_original_assertions': expected, 'log': command['log'],
            'log_sha256': command['log_sha256'], 'output': str(binary), 'output_sha256': sha(binary)})
        self.save()

    def finish(self):
        changed = [p for p, h in self.report['inputs_sha256'].items() if sha(p) != h]
        self.report['input_drift'] = changed
        if changed:
            raise RuntimeError('Protected inputs changed')
        self.report['result_counts'] = dict(Counter(r['status'] for r in self.report['results']))
        self.report['actual_assertion_counts_by_mode'] = dict(Counter({mode: sum(r['actual_assertions'] or 0 for r in self.report['results'] if r['mode'] == mode)
            for mode in sorted({r['mode'] for r in self.report['results']})}))
        self.report['context_input_drift'] = [p for p, h in self.report.get('context_inputs_sha256', {}).items()
                                               if not Path(p).is_file() or sha(p) != h]
        if self.args.audit_only:
            self.report['execution'] = 'unrun-audit-only'
            self.report['status'] = 'inventory-audited' if not self.report['results'] else 'failed'
        else:
            expected = Counter((case, mode) for case in self.args.case for mode in MODES)
            observed = Counter((row['case'], row['mode']) for row in self.report['results'])
            complete = bool(expected) and expected == observed and self.report.get('selected_cases') == self.args.case
            self.report['complete_selected_matrix'] = complete
            self.report['status'] = 'passed' if complete and all(r['status'] == 'passed' for r in self.report['results']) else 'failed'
        self.save()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--campaign', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--case', action='append', default=[])
    parser.add_argument('--audit-only', action='store_true')
    args = parser.parse_args()
    run = Run(args)
    try:
        run.audit()
        if not args.audit_only:
            run.execute()
        run.finish()
        return 0 if run.report['status'] in ('passed', 'inventory-audited') else 1
    except Exception as error:
        run.report.update(status='failed', error=str(error))
        run.save()
        raise


if __name__ == '__main__':
    raise SystemExit(main())
