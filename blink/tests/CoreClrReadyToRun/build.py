#!/usr/bin/env python3
"""Publish and prove an isolated normal-R2R service, with matched native IL controls.

Never builds Blink, runs a translated guest, changes the original fixture, or
installs packages globally. All generated files live in readytorun-* artifacts.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import shutil
import statistics
import sys
from zipfile import ZipFile
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
BLINK = HERE.parents[1]
REPO = BLINK.parent
sys.path.insert(0, str(BLINK / 'tests/CoreClrRuntime'))
from evidence import Evidence, DOTNET, SDK, VERSION, manifest, sha
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command
from inspect_pe import inspect
from native import measure
from test_inspector import verify_pair

COMMIT = '95017c711e6afc1085133d440e42b4bd78155701'
SOURCE_SHA = '0090390e0cb8d705375b0272d735130f13b3e5d30429c905fcc4bf606820bf7b'
BASE = BLINK / 'artifacts/coreclr-runtime/service-minimal-control/attempt-sa7ix7mn/closure.json'
PACKAGE_IDS = ['microsoft.netcore.app.crossgen2.linux-x64',
    'microsoft.netcore.app.runtime.linux-x64', 'microsoft.aspnetcore.app.runtime.linux-x64']


def write_json(ev, name, value):
    path = ev.directory / name
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n')
    ev.pin(path)
    return dict(path=str(path), sha256=sha(path))


def packages(ev, cache, feed, allow_download):
    destination = ev.directory / 'feed'
    destination.mkdir()
    records = []
    for package in PACKAGE_IDS:
        name = f'{package}.{VERSION}.nupkg'
        candidates = [cache / package / VERSION / name, feed / name]
        source = next((p for p in candidates if p.is_file()), None)
        target = destination / name
        url = f'https://api.nuget.org/v3-flatcontainer/{package}/{VERSION}/{name}'
        if source:
            ev.capture(source, target)
            origin = str(source.resolve())
        elif allow_download:
            curl = ev.pin(shutil.which('curl'))
            ev.run('download-' + package, [curl, '--connect-timeout', '15', '--max-time', '90',
                '--retry', '0', '--fail', '--location', '--silent', '--show-error',
                '--dump-header', ev.directory / (package + '.headers'), '--output', target, url], timeout=95)
            origin = url
            ev.pin(target)
        else:
            raise RuntimeError('Pinned package absent; supply --allow-official-download: ' + name)
        with ZipFile(target) as archive:
            nuspec = next(n for n in archive.namelist() if n.endswith('.nuspec'))
            root = ET.fromstring(archive.read(nuspec))
            ns = {'n': root.tag.split('}')[0][1:]}
            metadata = root.find('n:metadata', ns)
            if metadata.find('n:id', ns).text.lower() != package or metadata.find('n:version', ns).text != VERSION:
                raise RuntimeError('Package identity differs: ' + name)
            repository = metadata.find('n:repository', ns)
            if repository is None or repository.attrib.get('commit') != COMMIT:
                raise RuntimeError('Package source commit differs: ' + name)
        data = target.read_bytes()
        sha512 = base64.b64encode(hashlib.sha512(data).digest()).decode()
        cached_sha = cache / package / VERSION / (name + '.sha512')
        if source == candidates[0] and cached_sha.is_file() and cached_sha.read_text().strip() != sha512:
            raise RuntimeError('Cached nupkg SHA512 differs: ' + name)
        records.append(dict(id=package, version=VERSION, source=origin, official_url=url,
            path=str(target), sha256=sha(target), sha512_base64=sha512,
            size=len(data), repository=repository.attrib))
    ev.value['packages'] = records
    config = ev.directory / 'NuGet.Config'
    config.write_text('<configuration><packageSources><clear/><add key="retained" value="' +
        str(destination) + '"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>\n')
    ev.pin(config)
    return config


def build_closure(ev, base, published, profile):
    original = json.loads(base.read_text())
    prior = Path(original['Images'])
    images = ev.directory / profile / 'images'
    before, rows = {}, []
    for row in original['Files']:
        path = Path(row['File'])
        if path.is_absolute() or '..' in path.parts or row['GuestPath'] != '/' + path.as_posix():
            raise RuntimeError('Unexpected baseline guest path')
        source = prior / path
        if sha(source) != row['Sha256']:
            raise RuntimeError('Baseline file differs: ' + str(source))
        before[path.as_posix()] = row['Sha256']
        target = images / path
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
        rows.append(dict(row))
    for source in sorted(published.rglob('*')):
        if source.is_file():
            path = Path('apps/service') / source.relative_to(published)
            target = images / path
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, target)
            rows = [r for r in rows if r['File'] != path.as_posix()]
            rows.append(dict(GuestPath='/' + path.as_posix(), File=path.as_posix(),
                Sha256=sha(target), Executable=False))
    after = manifest(images)
    if set(before) - set(after):
        raise RuntimeError('Profile dropped original closure files')
    changes = [name for name in before if before[name] != after[name]]
    if any(not name.startswith('apps/service/') for name in changes):
        raise RuntimeError('Non-service closure file changed')
    total = sum((images / name).stat().st_size for name in after)
    if total > 512 * 1024 * 1024:
        raise RuntimeError('Guest closure exceeds 512 MiB storage budget')
    cp = images.parent / 'closure.json'
    cp.write_text(json.dumps(dict(Images=str(images), Files=sorted(rows, key=lambda r: r['File']),
        Environment=original['Environment']), indent=2) + '\n')
    ev.pin(cp)
    ev.tree(images)
    return dict(closure=str(cp), closure_sha256=sha(cp), image_files=after,
        file_count=len(after), storage_bytes=total, storage_limit_bytes=512 * 1024 * 1024,
        changed_baseline_files=changes, additional_files=sorted(set(after) - set(before)),
        retained_app_support_files=sorted(name for name in before if name.startswith('apps/service/')
            and not (published / Path(name).relative_to('apps/service')).is_file()))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-closure', type=Path, default=BASE)
    parser.add_argument('--package-cache', type=Path, default=Path.home() / '.nuget/packages')
    parser.add_argument('--package-feed', type=Path,
        default=BLINK / 'artifacts/coreclr-runtime/readytorun-r23-discovery')
    parser.add_argument('--allow-official-download', action='store_true')
    parser.add_argument('--samples', type=int, default=5)
    args = parser.parse_args()
    if not 1 <= args.samples <= 10:
        parser.error('--samples must be from 1 through 10')
    ev = Evidence('readytorun-native')
    ev.value.update(scope='Native-only ReadyToRun preparation; no translated guest execution',
        blink_execution=False, native_runtime_execution=True, guest_validation=False,
        prior_filter_stop='Prior coordinator automated content-filter stop retained; no safeguards disabled or bypassed',
        profile=dict(PublishReadyToRun=True, PublishReadyToRunComposite=False, SelfContained=False,
            PublishAot=False, RID='linux-x64', sdk=SDK, runtime=VERSION,
            runtime_commit=COMMIT, compiler_package='Microsoft.NETCore.App.Crossgen2.linux-x64',
            compiler_version=VERSION, tiering='runtime default; no tiering override'),
        native_validation=dict(passed=False, measurements=[]))
    try:
        for key in list(ev.environment):
            if key.startswith(('DOTNET_', 'COMPlus_', 'CORECLR_', 'LD_')):
                ev.environment.pop(key)
        ev.environment.update(DOTNET_PROCESSOR_COUNT='2', DOTNET_ROOT=str(DOTNET.parent),
            DOTNET_MULTILEVEL_LOOKUP='0', DOTNET_ROLL_FORWARD='Disable',
            DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_CLI_USE_MSBUILD_SERVER='0',
            DOTNET_CLI_HOME=str(ev.directory / 'cli-home'),
            NUGET_PACKAGES=str(ev.directory / 'packages'),
            NUGET_HTTP_CACHE_PATH=str(ev.directory / 'http-cache'), MSBUILDDISABLENODEREUSE='1')
        source = ev.directory / 'source'
        for path in HERE.rglob('*'):
            if path.is_file() and not {'bin', 'obj', '__pycache__'}.intersection(path.relative_to(HERE).parts):
                ev.capture(path, source / path.relative_to(HERE))
        if sha(source / 'Program.cs') != SOURCE_SHA:
            raise RuntimeError('Service source differs from successful minimal IL control')
        control = BLINK / 'artifacts/coreclr-runtime/service-minimal-control/attempt-sa7ix7mn'
        ev.capture(control / 'source/Program.cs', ev.directory / 'control/Program.cs', SOURCE_SHA)
        ev.capture(control / 'receipt.json', ev.directory / 'control/receipt.json')
        if not json.loads((ev.directory / 'control/receipt.json').read_text()).get('guest_functionally_passed'):
            raise RuntimeError('Selected minimal IL control did not pass')
        ev.capture(args.base_closure, ev.directory / 'control/closure.json')
        ev.value['source_manifest'] = write_json(ev, 'source-manifest.json',
            dict(files=manifest(source), control_program_sha256=SOURCE_SHA,
                source_profile='same WebHostBuilder middleware as successful minimal IL control',
                original_slim_profile='unchanged; distinct CoreClrRuntime/Service'))
        for path in (BLINK / 'tests/CoreClrRuntime/evidence.py', REPO / 'Scripts/production_support.py'):
            ev.capture(path, ev.directory / 'harness-support' / path.name)
        dotnet = ev.pin(DOTNET)
        info, _ = ev.run('sdk-info', [dotnet, '--info'], cwd=source)
        sdk, _ = ev.run('sdk-version', [dotnet, '--version'], cwd=source)
        if sdk.strip() != SDK.encode() or COMMIT.encode()[:10] not in info:
            raise RuntimeError('Installed SDK/host pin differs')
        sdk_root = DOTNET.parent / 'sdk' / SDK
        tool_inputs = {'sdk': dict(path=str(sdk_root), files=manifest(sdk_root)), 'targeting_packs': {}}
        for framework in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'):
            path = DOTNET.parent / 'packs' / (framework + '.Ref') / VERSION
            tool_inputs['targeting_packs'][framework] = dict(path=str(path), files=manifest(path))
        if any(not p['files'] for p in tool_inputs['targeting_packs'].values()):
            raise RuntimeError('Pinned targeting pack absent')
        ev.value['build_inputs'] = write_json(ev, 'build-inputs.json', tool_inputs)
        config = packages(ev, args.package_cache, args.package_feed, args.allow_official_download)
        profiles = {}
        for profile, publish_profile in (('il', 'IL'), ('normal', 'Normal')):
            project_source = ev.directory / profile / 'source'
            shutil.copytree(source, project_source)
            project = project_source / 'Service.csproj'
            common = [f'-p:PublishProfile={publish_profile}', '-p:RuntimeIdentifier=linux-x64',
                '-p:RuntimeFrameworkVersion=10.0.12', '-p:TargetLatestRuntimePatch=false',
                '-p:SelfContained=false', '-p:RollForward=Disable',
                '-p:PathMap=' + str(project_source) + '=/_/CoreClrReadyToRun']
            ev.run(profile + '-restore', locked_command([dotnet, 'restore', project,
                '--configfile', config, '--disable-parallel', '--disable-build-servers',
                '-m:1', *common], env=ev.environment), timeout=120, cwd=project_source)
            published = project_source.parent / 'published'
            ev.run(profile + '-publish', locked_command([dotnet, 'publish', project, '-c', 'Release',
                '--no-restore', '--no-self-contained', '-o', published, '-v:normal',
                *common], env=ev.environment), timeout=180, cwd=project_source)
            assets = project_source / 'obj/project.assets.json'
            ev.pin(assets)
            runtime = json.loads((published / 'CoreClrService.runtimeconfig.json').read_text())['runtimeOptions']
            frameworks = runtime.get('frameworks', [runtime.get('framework')])
            if runtime.get('rollForward') != 'Disable' or {f['version'] for f in frameworks} != {VERSION}:
                raise RuntimeError('Published runtimeconfig does not pin both frameworks')
            ev.tree(published)
            profiles[profile] = build_closure(ev, ev.directory / 'control/closure.json', published, profile)
            profiles[profile]['assets'] = dict(path=str(assets), sha256=sha(assets))
            profiles[profile]['published'] = str(published)
        ev.value['profiles'] = profiles
        ev.value.update({k: profiles['normal'][k] for k in ('closure', 'closure_sha256', 'image_files')})
        package_root = ev.directory / 'packages'
        ev.tree(package_root)
        crossgen = package_root / PACKAGE_IDS[0] / VERSION / 'tools/crossgen2'
        ev.pin(crossgen)
        ev.run('crossgen2-version', [crossgen, '--version'], timeout=15)
        resolved = list((ev.directory / 'normal/source/obj').rglob('readytorun-tools.txt'))
        if len(resolved) != 1 or ('compiler=' + str(crossgen)) not in resolved[0].read_text().splitlines():
            raise RuntimeError('SDK resolved another crossgen2 compiler')
        ev.pin(resolved[0])
        ev.value['compiler'] = dict(path=str(crossgen), sha256=sha(crossgen),
            resolved_inputs=str(resolved[0]), resolved_inputs_sha256=sha(resolved[0]),
            package_closure=manifest(package_root))
        normal_images = Path(profiles['normal']['closure']).parent / 'images'
        for framework in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'):
            identity = normal_images / 'dotnet/shared' / framework / VERSION / '.version'
            if identity.read_text().splitlines() != [COMMIT, VERSION]:
                raise RuntimeError('Retained runtime commit differs')
        inventory = {}
        for framework in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'):
            folder = normal_images / 'dotnet/shared' / framework / VERSION
            rows = [inspect(p) for p in sorted(folder.glob('*.dll'))]
            inventory[framework] = dict(assemblies=rows, summary=dict(managed_dlls=len(rows),
                r2r=sum(r['ready_to_run'] for r in rows),
                il_only_with_bodies=[Path(r['path']).name for r in rows if not r['ready_to_run'] and r['cli']['il_method_body_count']],
                il_only_without_bodies=[Path(r['path']).name for r in rows if not r['ready_to_run'] and not r['cli']['il_method_body_count']]))
        applications = {profile: [inspect(p, detail=True) for p in sorted(Path(value['published']).rglob('*.dll'))]
            for profile, value in profiles.items()}
        pair = {profile: Path(value['published']) / 'CoreClrService.dll' for profile, value in profiles.items()}
        regression = verify_pair(pair['il'], pair['normal'])
        ev.value['inspector_regression'] = write_json(ev, 'inspector-regression.json', regression)
        proof = dict(frameworks=inventory, applications=applications, regression=regression,
            native_payload_proven=True, scope='static payload presence; runtime method selection not inferred')
        ev.value['pe_proof'] = dict(**write_json(ev, 'pe-proof.json', proof), native_payload_proven=True)
        ev.run('inspector-positive-regression', [sys.executable, source / 'test_inspector.py',
            '--il', pair['il'], '--r2r', pair['normal']], timeout=30)
        measurements = ev.value['native_validation']['measurements']
        for sample in range(args.samples):
            order = ('il', 'normal') if sample % 2 == 0 else ('normal', 'il')
            for profile in order:
                result = measure(ev, profiles[profile]['closure'], f'native-{profile}-{sample + 1}')
                measurements.append(dict(profile=profile, sample=sample + 1, **result))
                ev.save()
        summaries = {profile: {key: dict(median=statistics.median(r[key] for r in measurements if r['profile'] == profile),
            minimum=min(r[key] for r in measurements if r['profile'] == profile),
            maximum=max(r[key] for r in measurements if r['profile'] == profile))
            for key in ('ready_seconds', 'first_http_seconds', 'first_http_request_seconds')}
            for profile in profiles}
        ev.value['native_validation'].update(passed=True, samples_per_profile=args.samples,
            matched_environment=True, tiering_override=False, summaries=summaries,
            limitations='Warm filesystem, fresh processes, no cache flush or CPU affinity; native timing is not Blink timing')
        ev.value['selection'] = dict(selected='normal', composite_published=False,
            reason='Framework dependencies already have native R2R payloads; normal adds app native methods without changing runtime files',
            composite_decision='Deferred to normal public guest startup/instruction measurements owned by coordinator')
        # Framework/runtime bytes and loader identity remain exactly those of the original closure.
        interpreter = '/lib64/ld-linux-x86-64.so.2'
        retained_loader = normal_images / interpreter.lstrip('/')
        if sha(interpreter) != sha(retained_loader):
            raise RuntimeError('Native kernel loader differs from retained guest loader')
        ev.value['native_loader'] = dict(path=interpreter, sha256=sha(interpreter),
            invocation='normal dotnet executable; kernel PT_INTERP; no explicit ld-linux launch')
        for name in ('LICENSE.txt', 'ThirdPartyNotices.txt'):
            ev.capture(DOTNET.parent / name, ev.directory / 'licenses' / name)
        for name in ('sdk',):
            if manifest(tool_inputs[name]['path']) != tool_inputs[name]['files']:
                raise RuntimeError('SDK closure changed during build')
        for item in tool_inputs['targeting_packs'].values():
            if manifest(item['path']) != item['files']:
                raise RuntimeError('Targeting pack changed during build')
        ev.check()
        ev.value['passed'] = True
    except BaseException as error:
        ev.value['error'] = repr(error)
    return ev.finish()


if __name__ == '__main__':
    raise SystemExit(main())
