"""Read-only validation of the R23 normal native profile's actual receipt schema."""
import json
from pathlib import Path, PurePosixPath
from support import BLINK, sha

SOURCE_SHA = '0090390e0cb8d705375b0272d735130f13b3e5d30429c905fcc4bf606820bf7b'
COMMIT = '95017c711e6afc1085133d440e42b4bd78155701'
EXPECTED_RESPONSES = [dict(path='/health', status=200, body='CORECLR SERVICE OK\n'),
    dict(path='/echo', status=200, body='CORECLR:payload\n'),
    dict(path='/stop', status=200, body='STOP OK\n')]
PROFILE = dict(PublishReadyToRun=True, PublishReadyToRunComposite=False, SelfContained=False,
    PublishAot=False, RID='linux-x64', sdk='10.0.112', runtime='10.0.12', runtime_commit=COMMIT,
    compiler_package='Microsoft.NETCore.App.Crossgen2.linux-x64', compiler_version='10.0.12',
    tiering='runtime default; no tiering override')


def read_profile(ev, receipt_path, closure_override, evidence, inspector):
    path = ev.pin(receipt_path)
    native = json.loads(path.read_text())
    if (native.get('passed') is not True or native.get('completed') is not True or
            native.get('baseline') != evidence.BASELINE or native.get('blink_execution') is not False or
            native.get('native_runtime_execution') is not True or
            any(type(native.get('profile', {}).get(k)) is not type(v) or native['profile'][k] != v for k, v in PROFILE.items())):
        raise RuntimeError('Completed native-only R23 normal profile with exact runtime/compiler settings required')
    validation = native.get('native_validation', {})
    samples = [r for r in validation.get('measurements', []) if r.get('profile') == 'normal']
    if validation.get('passed') is not True or not samples:
        raise RuntimeError('Normal native HTTP validation is missing')
    for sample in samples:
        actual = [{k: r.get(k) for k in ('path', 'status', 'body')} for r in sample.get('responses', [])]
        cleanup = sample.get('cleanup', {})
        if (sample.get('passed') is not True or actual != EXPECTED_RESPONSES or cleanup.get('signals') != [] or
                cleanup.get('group_gone') is not True or cleanup.get('exit_code') != 0):
            raise RuntimeError('Normal native HTTP sample did not complete normally')
    normal = native['profiles']['normal']
    if any(native[k] != normal[k] for k in ('closure', 'closure_sha256', 'image_files')):
        raise RuntimeError('Selected R23 receipt does not bind its normal closure')
    cp = ev.pin(closure_override or native['closure'], native['closure_sha256'])
    closure = json.loads(cp.read_text())
    images = Path(closure['Images'])
    if not images.is_absolute() or not images.is_dir() or closure['Environment'] != evidence.ENVIRONMENT:
        raise RuntimeError('Absolute immutable images and unchanged baseline guest environment required')
    rows, guests = {}, set()
    for row in closure['Files']:
        name = row['File']
        relative = PurePosixPath(name)
        if (not name or relative.is_absolute() or '..' in relative.parts or relative.as_posix() != name or
                row['GuestPath'] != '/' + name or name in rows or row['GuestPath'] in guests or
                type(row['Executable']) is not bool):
            raise RuntimeError('Invalid or duplicate closure image path')
        rows[name] = row['Sha256']; guests.add(row['GuestPath'])
        ev.pin(images / name, row['Sha256'])
    if rows != native['image_files'] or evidence.manifest(images) != rows or len(rows) != normal['file_count']:
        raise RuntimeError('Actual closure manifest differs from native R23 images')
    for required in ('dotnet/dotnet', 'apps/service/CoreClrService.dll'):
        if required not in rows: raise RuntimeError('Missing service runtime image: ' + required)
    if not next(row['Executable'] for row in closure['Files'] if row['File'] == 'dotnet/dotnet'):
        raise RuntimeError('Guest dotnet is not executable')
    for framework in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'):
        version = images / 'dotnet/shared' / framework / '10.0.12/.version'
        if version.read_text().splitlines() != [COMMIT, '10.0.12']:
            raise RuntimeError('Runtime framework commit differs')
    app = images / 'apps/service/CoreClrService.dll'
    fresh = inspector.inspect(app, detail=True)
    if (fresh.get('native_payload_proven') is not True or fresh.get('ready_to_run') is not True or
            fresh['r2r']['version'] != '16.0' or fresh['r2r']['methoddef_entrypoint_count'] <= 0 or
            fresh['r2r']['runtime_function_count'] <= 0 or fresh['r2r']['native_code_bytes'] <= 0 or
            fresh['cli']['il_method_body_count'] <= 0):
        raise RuntimeError('Normal service lacks real R2R code, entrypoints, or retained IL')
    proof_ref = native['pe_proof']; proof_path = ev.pin(proof_ref['path'], proof_ref['sha256'])
    proof = json.loads(proof_path.read_text())
    if proof_ref.get('native_payload_proven') is not True or proof.get('native_payload_proven') is not True:
        raise RuntimeError('R23 PE proof is incomplete')
    proven = [r for r in proof['applications']['normal'] if r['sha256'] == rows['apps/service/CoreClrService.dll']]
    # Paths differ between the published app and the copied guest closure.
    if len(proven) != 1 or {k: v for k, v in proven[0].items() if k != 'path'} != {k: v for k, v in fresh.items() if k != 'path'}:
        raise RuntimeError('R23 PE proof does not describe the exact imported service')
    source_ref = native['source_manifest']; source_path = ev.pin(source_ref['path'], source_ref['sha256'])
    source = json.loads(source_path.read_text())
    if source.get('control_program_sha256') != SOURCE_SHA or source['files'].get('Program.cs') != SOURCE_SHA:
        raise RuntimeError('Source differs from the successful minimal IL control')
    for name, digest in source['files'].items():
        relative = PurePosixPath(name)
        if relative.is_absolute() or '..' in relative.parts:
            raise RuntimeError('Source manifest escaped retained source directory')
        ev.pin(path.parent / 'source' / name, digest)
    ev.pin(BLINK / 'tests/GuestKestrelMinimal/Program.cs', SOURCE_SHA)
    # Pin the native producer's tool/runtime/source/log identities too. These
    # remain separate native evidence; none is counted as a public guest pass.
    for name, digest in native['inputs'].items(): ev.pin(name, digest)
    for folder, files in native.get('frozen_trees', {}).items():
        if evidence.manifest(Path(folder)) != files: raise RuntimeError('Native producer tree changed: ' + folder)
        ev.tree(folder)
    for command in native['commands']:
        for name, digest in command.get('logs', {}).items(): ev.pin(name, digest)
    ev.tree(images)
    ev.value['profile'] = dict(receipt=str(path), receipt_sha256=sha(path), closure=str(cp), closure_sha256=sha(cp),
        settings=native['profile'], image_files=rows, source_manifest=source_ref, pe_proof=proof_ref,
        native_validation_passed=True, service_sha256=sha(app), native_payload_proven=True,
        runtime_method_selection_proven=False)
    return cp
