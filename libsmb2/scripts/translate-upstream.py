#!/usr/bin/env python3
"""Build unchanged pinned upstream test programs and their native controls.

The normal library translation must have succeeded first. Its immutable tool
snapshot and C object fragments are reused, with provenance checked against the
current pin and configuration. Each upstream main gets a separate executable.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import json
from pathlib import Path
import platform
import shutil
import subprocess
import tempfile
import time

from common import ROOT, SOURCE_SPEC, fetch, run, sha

PROGRAMS = {
    'prog_mkdir': 'tests/prog_mkdir.c',
    'prog_rmdir': 'tests/prog_rmdir.c',
    'prog_cat': 'tests/prog_cat.c',
    'prog_cat_cancel': 'tests/prog_cat_cancel.c',
    'smb2-cp': 'utils/smb2-cp.c',
    'aes128ccm-test': 'tests/aes128ccm-test.c',
    'ntlmssp_generate_blob': 'tests/ntlmssp_generate_blob.c',
    'prog_setsd': 'tests/prog_setsd.c',
    'prog_ssc': 'tests/prog_ssc.c',
    'prog_open_timeout': 'tests/prog_open_timeout.c',
}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--aot', action='store_true', help='Also publish raw and processed NativeAOT executables')
    parser.add_argument('--jobs', type=int, default=2, help='Concurrent program builds (default: 2)')
    parser.add_argument('--program', action='append', choices=PROGRAMS, help='Select programs; default: all supported programs')
    args = parser.parse_args()
    if not 1 <= args.jobs <= 4:
        parser.error('--jobs must be between 1 and 4')
    if platform.system() != 'Linux' or platform.machine() not in ('x86_64', 'AMD64'):
        parser.error('The current upstream native baseline targets Linux x64')
    logs = ROOT / 'artifacts/upstream-translation'
    build = ROOT / 'build/upstream'
    logs.mkdir(parents=True, exist_ok=True)
    build.mkdir(parents=True, exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix='run-', dir=build))
    receipt = dict(passed=False, source=SOURCE_SPEC, startedUnix=time.time(),
                   harnessSha256=sha(__file__), stagingDirectory=str(stage), commands=[])
    try:
        source = fetch()
        translation_file = ROOT / 'artifacts/translation-legacy/result.json'
        translation = json.loads(translation_file.read_text())
        if not translation.get('passed') or translation['source'] != SOURCE_SPEC or translation.get('profile') != 'legacy':
            raise RuntimeError('Run libsmb2/scripts/translate.sh --profile legacy successfully for the current source pin first')
        units = json.loads((ROOT / 'config/sources.json').read_text())
        expected_units = {unit: sha(source / unit) for unit in units}
        config = {str(p.relative_to(ROOT)): sha(p) for p in sorted((ROOT / 'config').rglob('*')) if p.is_file()}
        if translation['units'] != expected_units or translation['config'] != config:
            raise RuntimeError('Library translation is stale for the current sources/configuration; regenerate it first')
        library_stage = ROOT / translation['staging_directory']
        for key, subdir in [('compiler', 'compiler'), ('postprocessor', 'postprocessor')]:
            for name, digest in translation[key].items():
                if sha(library_stage / 'tools' / subdir / name) != digest:
                    raise RuntimeError('Library translation tool snapshot changed: ' + name)
        compiler = library_stage / 'tools/compiler/dotcc.dll'
        postprocessor = library_stage / 'tools/postprocessor/dotcc-postprocess.dll'
        objects = [library_stage / 'objects' / (f'{i:02d}-' + Path(unit).stem + '.cs') for i, unit in enumerate(units)]
        if not all(p.is_file() for p in objects):
            raise RuntimeError('Library object fragments are absent; regenerate the library first')
        receipt['translationReceiptSha256'] = sha(translation_file)
        receipt['libraryObjects'] = {str(p.relative_to(ROOT)): sha(p) for p in objects}
        receipt['compiler'] = translation['compiler']
        receipt['postprocessor'] = translation['postprocessor']
        defines = json.loads((ROOT / 'config/legacy-defines.json').read_text())
        # The test makefiles rely on platform headers exposing these types;
        # advertise the same available headers as the library configuration.
        flags = ['-std=c17', '-DHAVE_TIME_H="1"', '-DHAVE_STDINT_H="1"',
                 *['-D' + value for value in defines]]
        for include in [ROOT / 'config/managed', source, source / 'include', source / 'include/smb2', source / 'lib']:
            flags += ['-I', include]
        native_build = ROOT / 'build/native-oracle'
        run(['cmake', '-S', source, '-B', native_build, '-DENABLE_LIBKRB5=OFF',
             '-DENABLE_GSSAPI=OFF', '-DENABLE_LIBDCERPC=OFF', '-DENABLE_EXAMPLES=OFF',
             '-DCMAKE_BUILD_TYPE=Release'], logs / 'native-configure.log', receipt)
        run(['cmake', '--build', native_build, '--parallel', str(args.jobs)], logs / 'native-library-build.log', receipt)
        native_objects = sorted((native_build / 'lib/CMakeFiles/smb2.dir').glob('*.c.o'))
        if {p.name for p in native_objects} != {Path(unit).name + '.o' for unit in units}:
            raise RuntimeError('Native library object closure differs from the translated closure')
        receipt['nativeObjects'] = {str(p.relative_to(ROOT)): sha(p) for p in native_objects}

        def emit(name):
            relative_source = PROGRAMS[name]
            program = stage / name
            program.mkdir()
            logdir = logs / name
            native = program / 'native'
            translation_input = source / relative_source
            adaptations = []
            if name == 'ntlmssp_generate_blob':
                # This standalone source predates libsmb2.h's lease-key API
                # and includes libsmb2.h before the defining smb2.h. Supply
                # its prerequisite without changing any upstream bytes. The
                # native control uses exactly the same wrapper.
                translation_input = program / 'include-order.c'
                translation_input.write_text('#include <smb2/smb2.h>\n#include ' +
                                             json.dumps(str(source / relative_source)) + '\n')
                adaptations.append(dict(kind='prerequisite-header-wrapper', path=str(translation_input),
                                        sha256=sha(translation_input),
                                        reason='smb2_lease_key must be declared before libsmb2.h'))
            # Link CMake object files directly: private crypto helpers used by
            # standalone upstream tests need not be shared-library exports.
            run(['cc', *flags, '-D_DEFAULT_SOURCE', '-D_GNU_SOURCE', '-O2', translation_input,
                 *native_objects, '-o', native], logdir / 'native-build.log', receipt)
            main_object = program / 'main.cs'
            run(['dotnet', compiler, *flags, '--emit=obj', translation_input, '-o', main_object],
                logdir / 'translate.log', receipt)
            raw = program / 'raw/UpstreamTest'
            run(['dotnet', compiler, *objects, main_object, '--emit=csproj', '--runtime=c',
                 '--literal-pool', '--split=size', '--split-size=102400', '-o', raw], logdir / 'link.log', receipt)
            project = next(raw.glob('*.csproj'))
            processed = program / 'processed/UpstreamTest'
            shutil.copytree(raw, processed)
            processed_project = processed / project.name
            run(['dotnet', 'restore', processed_project, '--nologo'], logdir / 'restore.log', receipt)
            run(['dotnet', postprocessor, processed_project, '--in-place'], logdir / 'postprocess.log', receipt)
            commands = {'native': [str(native)]}
            outputs = {'native': sha(native)}
            for variant, directory in [('raw', raw), ('processed', processed)]:
                target = directory / project.name
                run(['dotnet', 'build', target, '-c', 'Release', '--nologo'], logdir / (variant + '-build.log'), receipt)
                assembly = directory / 'bin/Release/net10.0' / (project.stem + '.dll')
                commands[variant + '-jit'] = ['dotnet', str(assembly)]
                outputs[variant + '-jit'] = sha(assembly)
                if args.aot:
                    publish = program / (variant + '-aot')
                    run(['dotnet', 'publish', target, '-c', 'Release', '-r', 'linux-x64', '--self-contained',
                         'true', '-p:PublishAot=true', '-o', publish, '--nologo'],
                        logdir / (variant + '-aot-build.log'), receipt, timeout=1200)
                    executable = publish / project.stem
                    commands[variant + '-aot'] = [str(executable)]
                    outputs[variant + '-aot'] = sha(executable)
            print('Built upstream ' + name, flush=True)
            return name, commands, dict(source=relative_source, sha256=sha(source / relative_source),
                                        adaptations=adaptations, outputSha256=outputs)

        selected = list(dict.fromkeys(args.program or PROGRAMS))
        with ThreadPoolExecutor(max_workers=args.jobs) as workers:
            results = list(workers.map(emit, selected))
        variant_names = ['native', 'raw-jit', 'processed-jit'] + (['raw-aot', 'processed-aot'] if args.aot else [])
        baseline_blocked = {}
        for name, commands, info in results:
            if name != 'ntlmssp_generate_blob':
                continue
            # Preserve the pinned standalone vector's behavior. It creates a
            # context without the server identity now required by the NTLM
            # challenge decoder. Report a failing native baseline separately
            # from a compiler/runtime regression; do not repair its payload.
            output = subprocess.run(commands['native'], capture_output=True, timeout=30)
            stdout = logs / name / 'native-baseline.stdout'
            stderr = logs / name / 'native-baseline.stderr'
            stdout.write_bytes(output.stdout)
            stderr.write_bytes(output.stderr)
            if output.returncode:
                baseline_blocked[name] = dict(
                    reason='Pinned standalone vector omits the server identity required by ntlm_decode_challenge_message; native baseline fails',
                    nativeExitCode=output.returncode, nativeStdoutSha256=sha(stdout), nativeStderrSha256=sha(stderr),
                    logs=dict(stdout=str(stdout), stderr=str(stderr)))
        manifest = dict(sourceRoot=str(source), source=SOURCE_SPEC,
                        configurationSha256=config, librarySourceSha256=expected_units,
                        translationReceiptSha256=sha(translation_file),
                        harnessSha256=receipt['harnessSha256'],
                        baselineBlockedPrograms=baseline_blocked,
                        variants=[dict(name=v, programs={name: commands[v] for name, commands, info in results}) for v in variant_names],
                        programSources={name: info for name, commands, info in results},
                        omittedPrograms={
                            'prog_ls': 'Requires ELF dlsym(RTLD_NEXT) allocator interposition; no managed mapping qualified',
                            'smb2-dcerpc-coder-test': 'Requires the optional libdcerpc translation closure',
                            'ld_sockerr': 'LD_PRELOAD readv interposition helper; no managed mapping qualified',
                            'metastat-0202-censored': 'Requires POSIX getopt/optarg/optind header declarations and managed runtime support',
                        })
        # Publish only a fully built manifest; previous successful runs remain usable.
        manifest_path = build / 'manifest.json'
        receipt['manifest'] = str(manifest_path)
        receipt['passed'] = True
        receipt_path = stage / 'build-receipt.json'
        receipt_path.write_text(json.dumps(receipt, indent=2) + '\n')
        manifest['buildReceipt'] = str(receipt_path)
        manifest['buildReceiptSha256'] = sha(receipt_path)
        temporary = stage / 'manifest.json'
        temporary.write_text(json.dumps(manifest, indent=2) + '\n')
        temporary.replace(manifest_path)
        print(manifest_path)
    except BaseException as error:
        receipt['passed'] = False
        receipt['failure'] = str(error)
        raise
    finally:
        (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')


if __name__ == '__main__':
    main()
