"""Prepare original-C/Python qualification or consume explicit current evidence.

Only source acquisition in the shared framework and declared test prerequisite
helper may fetch. A retained evidence bundle never starts replacement controllers.
"""
import hashlib
import json
import os
from pathlib import Path
import sys

from campaigns.recipes import helper


def load(path):
    return json.loads(Path(path).read_text())


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def campaign(ctx):
    if ctx.options.action == 'verify':
        selected = ctx.work
    else:
        pointer = load(ctx.root / 'artifacts/campaign' / ('current-' + ctx.profile + '.json'))
        selected = ctx.root / 'build/campaign' / Path(pointer['receipt']).parent.name
    helper(ctx.root / 'scripts/translation_evidence.py').read_closed(ctx.root, selected)
    return selected.resolve()


def sealed_inputs(ctx, policy, data):
    """Check the retained selection before any acceptance command can use it.

    This seal establishes immutable inputs and consumer code, not successful
    assertions. The specialist C/Python validators still check those receipts.
    """
    if data.get('status') != 'passed':
        raise RuntimeError('Retained upstream input bundle is not sealed passed')
    if Path(data['campaign_run']).resolve() != campaign(ctx):
        raise RuntimeError('Retained upstream evidence belongs to another translation; no implicit reuse')
    selected = {}

    def select(path, status):
        path = str(Path(path).resolve())
        if path in selected and selected[path] != status:
            raise RuntimeError('Retained input is assigned conflicting evidence roles')
        selected[path] = status

    for name in ('c_report', 'native_report'):
        select(data[name], 'passed')
    if data.get('native_equivalence'):
        select(data['native_equivalence'], 'passed')
    if data.get('python_executables'):
        select(data['python_executables'], 'built')
    for paths in data.get('python_reports', {}).values():
        for path in paths:
            select(path, 'passed')
    for path in data.get('native_python_reports', []):
        select(path, 'passed')
    recorded = data.get('input_sha256', {})
    if set(recorded) != set(selected):
        raise RuntimeError('Retained input seal does not cover exactly the selected receipts')
    for path, status in selected.items():
        if digest(path) != recorded[path] or load(path).get('status') != status:
            raise RuntimeError('Retained receipt changed or is incomplete: ' + path)

    required = {str((ctx.root / name).resolve()) for name in policy['acceptance_consumers']}
    sources = data.get('consumer_source_sha256', {})
    snapshots = data.get('consumer_source_snapshots', {})
    if set(sources) != required or set(snapshots) != required:
        raise RuntimeError('Retained input seal lacks the exact acceptance consumer sources')
    for path in required:
        if Path(snapshots[path]).resolve() == Path(path):
            raise RuntimeError('Acceptance source snapshot must be a preserved copy: ' + path)
        if digest(path) != sources[path] or digest(snapshots[path]) != sources[path]:
            raise RuntimeError('Acceptance consumer or preserved source changed: ' + path)
    bundle = data.get('input_bundle')
    if bundle and digest(bundle['path']) != bundle['sha256']:
        raise RuntimeError('Retained input bundle changed after selection')


def existing(ctx, policy):
    name = os.environ.get(policy['reuse_evidence_environment'])
    if not name:
        return None
    path = Path(name).resolve()
    original = path.read_bytes()
    data = json.loads(original)
    if data.get('schema_version') != 1:
        raise RuntimeError('Unsupported upstream qualification input bundle')
    data['input_bundle'] = {'path': str(path), 'sha256': hashlib.sha256(original).hexdigest()}
    sealed_inputs(ctx, policy, data)
    return data


def prepare_c(ctx, policy):
    selected = campaign(ctx)
    work = ctx.work / 'upstream'
    output = ctx.artifacts / 'upstream-preparation'
    work.mkdir(exist_ok=True)
    output.mkdir(exist_ok=True)
    native = output / 'native-profile.json'
    ctx.run([sys.executable, ctx.root / 'scripts/native-profile-upstream.py', '--campaign', selected,
             '--output', native], 'upstream-native-profile', timeout=14400)
    emitted = work / 'emissions.json'
    ctx.run([sys.executable, ctx.root / 'tests/Upstream/build.py', '--campaign-run', selected,
             '--emit-only', '--form', 'raw', '--mode', 'jit', '--output', emitted],
            'upstream-required-c-emission', timeout=14400)
    c_output = work / 'required-c'
    ctx.run([sys.executable, ctx.root / 'tests/Provider/run-required-c.py', '--campaign-run', selected,
             '--emission-manifest', emitted, '--native-report', native, '--provider-policy',
             ctx.root / policy['test_provider_policy'], '--batch-size', policy['c_batch_size'],
             '--jobs', policy['c_build_jobs'], '--output', c_output], 'upstream-required-c-all4', timeout=262800)
    return {'schema_version': 1, 'campaign_run': str(selected), 'native_report': str(native),
            'c_report': str(c_output / 'progress.json')}


def prepare_python(ctx, policy, data):
    work = ctx.work / 'upstream'
    output = ctx.artifacts / 'upstream-preparation'
    work.mkdir(exist_ok=True)
    output.mkdir(exist_ok=True)
    ctx.run([sys.executable, ctx.root / 'scripts/test-prerequisites.py', '--fetch', ctx.options.fetch],
            'upstream-peer-prerequisites', timeout=14400)
    selected = Path(data['campaign_run'])
    endpoints = work / 'python-endpoints.json'
    ctx.run([sys.executable, ctx.root / 'tests/Upstream/build.py', '--campaign-run', selected,
             '--endpoint', 's2nc', '--endpoint', 's2nd', '--form', ctx.options.form,
             '--mode', ctx.options.mode, '--output', endpoints], 'upstream-original-cli', timeout=14400)
    ctx.run(['cmake', '--build', ctx.root / 'build/native/s2n-adapted', '-j2', '--target', 's2nc', 's2nd'],
            'upstream-native-cli', timeout=3600)
    native_endpoints = work / 'native-python-endpoints.json'
    rows = []
    for name in ('s2nc', 's2nd'):
        binary = ctx.root / 'build/native/s2n-adapted/bin' / name
        rows.append({'case': 'bin/' + name + '.c', 'form': 'raw', 'mode': 'jit', 'status': 'built',
                     'command': [str(binary)], 'execution_sha256': {str(binary): digest(binary)}})
    native_endpoints.write_text(json.dumps({'status': 'built', 'scope': 'Same-pin original native CLI; translated apps remain independent',
                                            'executables': rows}, indent=2) + '\n')
    text = ctx.run([sys.executable, ctx.root / 'scripts/psk-peer.py', '--fetch', ctx.options.fetch],
                   'upstream-explicit-psk-peer', timeout=3600)
    build_path = Path(text.strip().splitlines()[-1])
    built = load(build_path)
    if built['status'] != 'passed':
        raise RuntimeError('Explicit PSK peer build failed')
    proof = output / 'native-psk-proof'
    ctx.run([sys.executable, ctx.root / 'scripts/psk-peer-check.py', '--peer-receipt', build_path,
             '--output', proof], 'upstream-native-psk-controls', timeout=600)
    ordinary = ctx.root / policy['peer_commands']
    peers = load(ordinary)
    peers['commands']['OpenSSL:openssl'] = [str(Path(built['derived']) / 'apps/openssl')]
    peers['measured_psk_peer'] = {'label': built['label'], 'proof': str(proof / 'report.json')}
    psk_peers = work / 'psk-peers.json'
    psk_peers.write_text(json.dumps(peers, indent=2) + '\n')
    native_matrix, translated_matrix = output / 'native-python-matrix.json', output / 'translated-python-matrix.json'
    for kind, manifest, form, mode, destination, ports in (
        ('native', native_endpoints, 'raw', 'jit', native_matrix, policy['native_ports']),
        ('translated', endpoints, ctx.options.form, ctx.options.mode, translated_matrix, policy['translated_ports'])):
        command = [sys.executable, ctx.root / 'tests/Upstream/python-matrix.py', '--executables', manifest,
                   '--peer-commands', ordinary, '--psk-peer-commands', psk_peers, '--python', ctx.root / policy['python'],
                   '--form', form, '--mode', mode, '--endpoint-kind', kind, '--workers',
                   policy['native_python_workers'] if kind == 'native' else policy['python_workers'],
                   '--port-low', ports[0], '--port-high', ports[1], '--output', destination]
        if kind == 'native':
            command += ['--provider-version', policy['native_provider_version']]
        ctx.run(command, 'upstream-original-python-' + kind, timeout=262800)
    # A matrix identifies exact accepted family rows, including adopted rows
    # from a multi-family parent. Forwarding every leaf path can duplicate that
    # parent or accidentally include a failed sibling family.
    native_record, translated_record = load(native_matrix), load(translated_matrix)
    if native_record['status'] != 'passed' or translated_record['status'] != 'passed':
        raise RuntimeError('Complete original Python matrices have not passed')
    data['native_python_reports'] = [str(native_matrix)]
    data['python_reports'] = {form + '/' + mode: [str(translated_matrix)]
                              for form, mode in translated_record['selection']['variants']}
    data.update(python_executables=str(endpoints), python_matrices=[str(native_matrix), str(translated_matrix)])


def inputs(ctx, *, python=False):
    policy = load(ctx.root / 'config/qualification.json')
    if not hasattr(ctx, '_s2n_upstream_inputs'):
        retained = existing(ctx, policy)
        ctx._s2n_upstream_inputs = retained if retained is not None else prepare_c(ctx, policy)
        ctx._s2n_upstream_retained = retained is not None
    data = ctx._s2n_upstream_inputs
    if ctx._s2n_upstream_retained:
        sealed_inputs(ctx, policy, data)
    if python and not ctx._s2n_upstream_retained and 'python_reports' not in data:
        prepare_python(ctx, policy, data)
    ctx.receipt['upstream_qualification_inputs'] = data
    ctx.save()
    return data


def run(ctx, *, python):
    data = inputs(ctx, python=python)
    forms = ('raw', 'processed') if ctx.options.form == 'all' else (ctx.options.form,)
    modes = ('jit', 'aot') if ctx.options.mode == 'all' else (ctx.options.mode,)
    for form in forms:
        for mode in modes:
            destination = ctx.artifacts / ('original-upstream-' + form + '-' + mode + ('' if python else '-c') + '.json')
            command = [sys.executable, ctx.root / 'tests/Upstream/run.py', '--form', form, '--mode', mode,
                       '--c-report', data['c_report'], '--campaign-run', data['campaign_run'],
                       '--native-report', data['native_report'], '--output', destination]
            if data.get('native_equivalence'):
                command += ['--native-equivalence', data['native_equivalence']]
            if python:
                reports = data.get('python_reports', {}).get(form + '/' + mode, [])
                natives = data.get('native_python_reports', [])
                if not reports or not natives:
                    raise RuntimeError('Required existing original Python family receipts are missing; no implicit controllers will be started')
                for path in reports:
                    command += ['--python-report', path]
                for path in natives:
                    command += ['--native-python-report', path]
            else:
                command += ['--c-only']
            ctx.run(command, destination.stem, timeout=14400)
            if ctx._s2n_upstream_retained:
                sealed_inputs(ctx, load(ctx.root / 'config/qualification.json'), data)
