#!/usr/bin/env python3
"""Native/common and four managed modes for the finite nonblocking drain-to-EAGAIN socket boundary."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
CLI = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
POST = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
COMMON = (b'nonblocking listener and accepted sockets passed\n'
          b'drain-to-EAGAIN edges and MSG_PEEK passed\n'
          b'opaque data maxEvents alias lifetime and descriptor reuse passed\n')
EXPECTED = COMMON + b'private requested stop and owner drain passed\n'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def tree(directory):
    return {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*') if p.is_file()}


def main():
    base = ROOT / 'artifacts/host-async-sockets'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    snapshot = attempt / 'inputs'
    snapshot.mkdir()
    temporary = attempt / 'tmp'
    temporary.mkdir()
    r = dict(passed=False, scope='finite drain-to-EAGAIN edge socket boundary; not arbitrary Linux EPOLLET or a Kestrel execution',
             source_inputs={}, tool_inputs={}, optional_inputs={}, results={}, executions={})
    environment = dict(os.environ, LC_ALL='C', TMPDIR=str(temporary),
                       MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    r['explicit_environment'] = {k: environment[k] for k in
        ('LC_ALL', 'TMPDIR', 'MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_USE_MSBUILD_SERVER')}

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(r, indent=2) + '\n')

    def copy(path, target):
        r['source_inputs'][str(path)] = sha(path)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        if sha(target) != r['source_inputs'][str(path)]:
            raise RuntimeError('Source changed during capture: ' + str(path))

    def terminate(process):
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait(timeout=5)

    def run(command, name, timeout=180):
        command = list(map(str, command))
        log = attempt / (name + '.log')
        row = dict(command=command, exit=None)
        r['results'][name] = row
        save()
        with log.open('wb') as stream:
            process = subprocess.Popen(command, cwd=REPO, env=environment, stdout=stream,
                                       stderr=subprocess.STDOUT, start_new_session=True)
            try:
                row['exit'] = process.wait(timeout=timeout)
            except BaseException:
                terminate(process)
                row['exit'] = process.returncode
                raise
            finally:
                row['log_sha256'] = sha(log)
                save()
        if row['exit']:
            raise RuntimeError(name + ' failed: ' + str(log))
        return log.read_bytes()

    def execute(command, directory, name, expected):
        before = tree(directory)
        r['executions'][name] = dict(directory=str(directory), before=before)
        output = run(command, name, 30)
        after = tree(directory)
        r['executions'][name]['after'] = after
        save()
        if before != after:
            raise RuntimeError(name + ' executable closure changed')
        if output != expected:
            raise RuntimeError(name + ' output differs: ' + repr(output))

    def check():
        for group in ('source_inputs', 'tool_inputs'):
            for path, expected in r[group].items():
                if sha(Path(path)) != expected:
                    raise RuntimeError('Frozen input changed: ' + path)
        for path, expected in r['optional_inputs'].items():
            observed = sha(Path(path)) if Path(path).is_file() else None
            if observed != expected:
                raise RuntimeError('Optional build input changed: ' + path)
        if tree(snapshot) != r['snapshot']:
            raise RuntimeError('Snapshot changed')
        for row in r['executions'].values():
            if tree(Path(row['directory'])) != row['before']:
                raise RuntimeError('Previously executed closure changed')

    def interrupted(_signal, _frame):
        raise KeyboardInterrupt('Runner terminated')
    signal.signal(signal.SIGTERM, interrupted)
    try:
        for name in ('run.py', 'probe.c', 'native-main.c', 'layout.c', 'Program.cs'):
            copy(ROOT / 'tests/HostAsyncSockets' / name, snapshot / name)
        for name in ('HostSignals.c', 'HostSignalOps.c', 'HostIoBridge.cs', 'HostEpollBridge.cs', 'HostNetworkBridge.cs', 'HostSocketQueriesBridge.cs'):
            copy(ROOT / 'src/Host' / name, snapshot / name)
        for name in ('HostSignals.h', 'HostSignalOps.h', 'host-io.h', 'host-epoll.h'):
            copy(ROOT / 'src/Host/include' / name, snapshot / 'headers' / name)
        copy(ROOT / 'config/managed-threaded/sys/epoll.h', snapshot / 'overlay/sys/epoll.h')
        for directory, target in ((ROOT / 'config/managed-host', 'profile'),
                                  (REPO / 'DotCC.Lib/include', 'generic'),
                                  (ROOT / 'src/Managed.Emulation.Host', 'Host')):
            for path in directory.rglob('*'):
                if path.is_file() and not {'bin', 'obj'}.intersection(path.relative_to(directory).parts):
                    copy(path, snapshot / target / path.relative_to(directory))
        for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props',
                     'NuGet.Config', 'NuGet.config', 'nuget.config', 'global.json'):
            path = REPO / name
            r['optional_inputs'][str(path)] = sha(path) if path.is_file() else None
        for tool in (CLI, POST):
            for path in tool.parent.iterdir():
                if path.is_file() and path.suffix in ('.dll', '.json'):
                    r['tool_inputs'][str(path)] = sha(path)
        for tool in ('dotnet', 'cc'):
            path = Path(shutil.which(tool)).resolve()
            r['tool_inputs'][str(path)] = sha(path)
        r['snapshot'] = tree(snapshot)
        save()
        run(['dotnet', '--info'], 'dotnet-info')
        run(['cc', '--version'], 'cc-info')
        native = attempt / 'native'
        native.mkdir()
        run(['cc', '-std=c17', '-D_GNU_SOURCE', '-Wall', '-Wextra', '-Werror',
             snapshot / 'probe.c', snapshot / 'native-main.c', '-o', native / 'common'], 'native-build')
        execute([native / 'common'], native, 'native-common',
                COMMON)
        layout = attempt / 'native-layout'
        layout.mkdir()
        # Only authored ABI definitions are selected here; stdio remains system libc.
        run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror', '-I', snapshot / 'headers',
             '-iquote', snapshot / 'profile', snapshot / 'layout.c', '-o', layout / 'layout'], 'layout-build')
        execute([layout / 'layout'], layout, 'native-private-layout', b'private layout 16 8 8\n')
        objects = []
        for name in ('probe', 'HostSignals', 'HostSignalOps'):
            output = attempt / (name + '.obj.cs')
            # IncludeRegistry uses the last matching -I; reviewed overlay must win.
            run(['dotnet', CLI, '-std=c17', '-DBLINK_MANAGED_EPOLL', '-I', snapshot / 'generic',
                 '-I', snapshot / 'profile', '-I', snapshot / 'headers', '-I', snapshot / 'overlay',
                 snapshot / (name + '.c'), '--emit=obj', '-o', output], 'emit-' + name)
            objects.append(output)
        raw = attempt / 'raw'
        run(['dotnet', CLI, *objects, '--runtime=c', '--emit=managedlib', '--nest-types',
             '--class-name', 'Blink', '--namespace', 'Managed.Emulation', '-o', raw], 'link')
        r['objects'] = {str(p): sha(p) for p in objects}
        r['raw_sources'] = tree(raw)
        for label in ('raw', 'optimized'):
            mode = attempt / label if label == 'raw' else attempt / 'optimized'
            if label == 'optimized':
                shutil.copytree(raw, mode)
            consumer = attempt / (label + '-consumer')
            consumer.mkdir()
            host = attempt / (label + '-Host')
            shutil.copytree(snapshot / 'Host', host)
            authored = ('Program.cs', 'HostIoBridge.cs', 'HostEpollBridge.cs', 'HostNetworkBridge.cs', 'HostSocketQueriesBridge.cs')
            for name in authored:
                shutil.copyfile(snapshot / name, consumer / name)
            xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
            properties = ET.SubElement(xml, 'PropertyGroup')
            for key, value in (('TargetFramework', 'net10.0'), ('OutputType', 'Exe'),
                               ('AllowUnsafeBlocks', 'true'), ('Nullable', 'enable'),
                               ('AssemblyName', 'HostAsyncSocketsProbe'), ('WarningsAsErrors', 'CS8500')):
                ET.SubElement(properties, key).text = value
            items = ET.SubElement(xml, 'ItemGroup')
            ET.SubElement(items, 'Compile', Include=str(mode / '*.cs'))
            host_project = host / 'Managed.Emulation.Host.csproj'
            ET.SubElement(items, 'ProjectReference', Include=str(host_project))
            ET.SubElement(items, 'TrimmerRootAssembly', Include='HostAsyncSocketsProbe')
            project = consumer / 'HostAsyncSocketsProbe.csproj'
            ET.ElementTree(xml).write(project, encoding='unicode')
            build_args = ['-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false']
            if label == 'optimized':
                run(['dotnet', 'build', host_project, *build_args], 'optimized-host-prerequisite')
                run(['dotnet', 'restore', project, '--disable-build-servers'], 'optimized-restore')
                run(['dotnet', POST, project, '--in-place'], 'postprocess')
                # Postprocessor may visit authored project inputs; restore exact originals.
                for relative in tree(snapshot / 'Host'):
                    shutil.copyfile(snapshot / 'Host' / relative, host / relative)
                for name in authored:
                    shutil.copyfile(snapshot / name, consumer / name)
            run(['dotnet', 'build', project, *build_args], label + '-build')
            binary = consumer / 'bin/Release/net10.0/HostAsyncSocketsProbe.dll'
            # Publish later adds a runtime-specific subtree beneath bin/net10.0.
            # Execute an exact isolated copy, retaining strict final tree checks.
            execution = attempt / (label + '-jit-execution')
            original_closure = tree(binary.parent)
            shutil.copytree(binary.parent, execution)
            if tree(execution) != original_closure or tree(binary.parent) != original_closure:
                raise RuntimeError('JIT closure changed during execution snapshot')
            r[label + '_jit_copy'] = dict(source=str(binary.parent), destination=str(execution),
                                         files=original_closure)
            execute(['dotnet', execution / binary.name], execution, label + '-jit', EXPECTED)
            publish = attempt / (label + '-aot')
            run(['dotnet', 'publish', project, *build_args, '-r', 'linux-x64',
                 '-p:PublishAot=true', '-o', publish], label + '-publish', 300)
            execute([publish / 'HostAsyncSocketsProbe'], publish, label + '-aot', EXPECTED)
            for relative, expected in tree(snapshot / 'Host').items():
                if sha(host / relative) != expected:
                    raise RuntimeError('Authored Host changed')
            for name in authored:
                if sha(consumer / name) != sha(snapshot / name):
                    raise RuntimeError('Authored bridge/consumer changed')
            r[label + '_generated'] = tree(mode)
        check()
        if tree(raw) != r['raw_sources']:
            raise RuntimeError('Raw source changed')
        r['passed'] = True
    except BaseException as error:
        r['passed'] = False
        r['failure'] = str(error)
        raise
    finally:
        save()
        print(attempt / 'receipt.json')


if __name__ == '__main__':
    main()
