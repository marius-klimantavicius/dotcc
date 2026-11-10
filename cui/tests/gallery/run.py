"""Qualify the original translated 27-page gallery against native GLFW on Linux."""
import argparse
from contextlib import contextmanager
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import platform
import signal
import shutil
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]
REPOSITORY = ROOT.parent


def sha256(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def execute(command, log, env, timeout=1200, cwd=None):
    print('+ ' + subprocess.list2cmdline(list(map(str, command))), flush=True)
    with log.open('w', encoding='utf-8') as output:
        process = subprocess.Popen(list(map(str, command)), stdout=output,
                                   stderr=subprocess.STDOUT, env=env, cwd=cwd, start_new_session=True)
        try:
            status = process.wait(timeout=timeout)
        except BaseException:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            raise
    print(log.read_text(encoding='utf-8'), end='', flush=True)
    if status:
        raise RuntimeError(f'Command exited {status}; retained log: {log}')


@contextmanager
def display(work, env, xvfb):
    if not xvfb:
        if not env.get('DISPLAY'):
            raise RuntimeError('DISPLAY is missing; supply --xvfb for an owned headless display')
        yield env, {'kind': 'existing', 'display': env['DISPLAY']}
        return
    number = work / 'xvfb-display'
    record = {'kind': 'owned Xvfb', 'cleanup': 'pending'}
    with number.open('w+') as descriptor, (work / 'xvfb.log').open('w') as log:
        process = subprocess.Popen(['Xvfb', '-displayfd', str(descriptor.fileno()),
                                    '-screen', '0', '1440x1000x24', '+extension', 'GLX', '-nolisten', 'tcp'],
                                   pass_fds=(descriptor.fileno(),), stdout=log, stderr=log,
                                   env=env, start_new_session=True)
        record['pid'] = process.pid
        try:
            deadline = time.monotonic() + 15
            while not number.read_text().strip():
                if process.poll() is not None or time.monotonic() > deadline:
                    raise RuntimeError(f'Xvfb startup failed; see {work / "xvfb.log"}')
                time.sleep(.05)
            record['display'] = ':' + number.read_text().strip()
            yield dict(env, DISPLAY=record['display'], LIBGL_ALWAYS_SOFTWARE='1'), record
        finally:
            if process.poll() is None:
                os.killpg(process.pid, signal.SIGTERM)
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                os.killpg(process.pid, signal.SIGKILL)
                process.wait(timeout=5)
            record['cleanup'] = 'terminated and reaped'
            record['exitCode'] = process.returncode


def parse_pages(log):
    result = []
    for line in log.read_text(encoding='utf-8').splitlines():
        if not line.startswith('PAGE\t'):
            continue
        _, page, name, nodes, text, text_hash, different = line.split('\t')
        result.append(dict(page=int(page), name=name, nodes=int(nodes), mainText=int(text),
                           textHash=int(text_hash), different=int(different)))
    if len(result) != 27 or [row['page'] for row in result] != list(range(27)):
        raise RuntimeError(f'{log}: expected all 27 ordered upstream pages')
    return result


def compare(native, managed, native_dir, managed_dir):
    rows = []
    for expected, actual in zip(native, managed, strict=True):
        page = expected['page']
        # The sidebar alone cannot satisfy these checks: text and pixels are
        # sampled only in the gallery's main content (x >= 260).
        for key in ('page', 'name', 'nodes', 'mainText', 'textHash'):
            if actual[key] != expected[key]:
                raise RuntimeError(f'Page {page} {key}: managed {actual[key]!r}, native {expected[key]!r}')
        filename = f'page-{page:02}.rgba'
        left = (native_dir / filename).read_bytes()
        right = (managed_dir / filename).read_bytes()
        if len(left) != 1240 * 820 * 4 or len(right) != len(left):
            raise RuntimeError(f'Page {page}: incorrect framebuffer length')
        # Full window includes a moving sidebar selection spring. Compare main
        # content only; sidebar selection is independently asserted through OS input.
        a = b''.join(left[(y * 1240 + 260) * 4:(y + 1) * 1240 * 4] for y in range(820))
        b = b''.join(right[(y * 1240 + 260) * 4:(y + 1) * 1240 * 4] for y in range(820))
        different, above, maximum = 0, 0, 0
        if a != b:
            for first, second in zip(a, b, strict=True):
                delta = abs(first - second)
                different += delta != 0
                above += delta > 1
                maximum = max(maximum, delta)
        row = dict(page=page, name=actual['name'], mainContentBytes=len(a),
                   differentChannels=different, channelsAboveTolerance=above, maximumError=maximum,
                   nativeSha256=hashlib.sha256(a).hexdigest(), managedSha256=hashlib.sha256(b).hexdigest())
        rows.append(row)
        if above:
            raise RuntimeError(f'Page {page} ({actual["name"]}): {above} main-content channels exceed tolerance 1, max {maximum}')
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True)
    parser.add_argument('--source', type=Path, required=True, help='Pinned original upstream source')
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--xvfb', action='store_true')
    parser.add_argument('--native-only', action='store_true')
    args = parser.parse_args()
    if platform.system() != 'Linux':
        parser.error('This native/OS runner requires Linux/X11. Windows gallery execution remains unrun.')
    work = args.work_root.resolve()
    if work.is_relative_to(REPOSITORY):
        parser.error('--work-root must be outside the repository')
    source = args.source.resolve()
    for name in ('gallery_ui.c', 'gallery_ui.h', 'gallery_more.c', 'gallery_more.h'):
        if not (source / 'examples' / name).is_file():
            parser.error(f'Missing upstream gallery source: {name}')
    work.mkdir(parents=True, exist_ok=True)
    if (work / 'report.json').exists():
        previous = work / 'history' / datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S.%fZ')
        previous.mkdir(parents=True)
        for path in [work / 'report.json', *work.glob('**/*.log')]:
            if 'history' in path.relative_to(work).parts:
                continue
            destination = previous / path.relative_to(work)
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, destination)
    temporary = work / 'tmp'
    temporary.mkdir(exist_ok=True)
    env = dict(os.environ, CUI_WORK_ROOT=str(work), TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary))
    # The harness chooses explicit theme/accent in both native and managed code.
    env.pop('CUI_THEME', None)
    env.pop('CUI_ACCENT', None)
    # The original Files page browses '.'. Give both binaries the same immutable
    # directory instead of a repository changing while other agents work.
    fixture = work / 'files-fixture'
    fixture.mkdir(exist_ok=True)
    (fixture / 'documents').mkdir(exist_ok=True)
    for name, value in (('notes.txt', 'Gallery qualification\n'), ('sample.csv', 'a,b\n1,2\n')):
        path = fixture / name
        if not path.exists():
            path.write_text(value)
    report = dict(status='running', windows='unrun', variants=[],
                  source=str(source), sourceHashes={name: hashlib.sha256((source / 'examples' / name).read_bytes()).hexdigest()
                                                   for name in ('gallery_ui.c', 'gallery_ui.h', 'gallery_more.c', 'gallery_more.h')},
                  comparison='Main-content nodes/text exact; all RGBA channels within 1; no dynamic-content exclusion')
    try:
        native = work / 'native'
        native.mkdir(exist_ok=True)
        native_capture = native / 'captures'
        native_capture.mkdir(exist_ok=True)
        execute(['cmake', '-S', ROOT / 'tests/gallery', '-B', native / 'build', '-DCMAKE_BUILD_TYPE=Release',
                 '-DCUI_SOURCE_ROOT=' + str(source)], native / 'configure.log', env)
        execute(['cmake', '--build', native / 'build', '--target', 'gallery-control', '--parallel', '2'],
                native / 'build.log', env)
        report['nativeBinarySha256'] = sha256(native / 'build/gallery-control')
        with display(work, env, args.xvfb) as (runtime_env, display_record):
            report['display'] = display_record
            execute(['xdotool', 'mousemove', '1439', '999'], native / 'pointer.log', runtime_env, 15)
            execute([native / 'build/gallery-control', native_capture], native / 'run.log', runtime_env, 180, fixture)
            native_pages = parse_pages(native / 'run.log')
            report['nativePages'] = native_pages
            execute([native / 'build/gallery-control', '--input'], native / 'input.log', runtime_env, 600, fixture)
            report['nativeInput'] = str(native / 'input.log')
            if not args.native_only:
                forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
                modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
                for form in forms:
                    suffix = '.Raw' if form == 'raw' else ''
                    core = ROOT / f'generated/TranslatedCui{suffix}/TranslatedCui.csproj'
                    gallery = ROOT / f'generated/TranslatedCuiGallery{suffix}/TranslatedCuiGallery.csproj'
                    if not core.is_file() or not gallery.is_file():
                        raise RuntimeError(f'Missing canonical {form} core/gallery; translate before running qualification')
                    for mode in modes:
                        variant = work / f'{form}-{mode}'
                        variant.mkdir(exist_ok=True)
                        captures = variant / 'captures'
                        artifacts = variant / 'artifacts'
                        command = ['dotnet', 'publish' if mode == 'aot' else 'build',
                                   ROOT / 'tests/gallery/Gallery.Tests.csproj', '-c', 'Release',
                                   '--artifacts-path', artifacts, '-m:1', '--nologo',
                                   '-p:CuiProject=' + str(core), '-p:CuiCoreProject=' + str(core),
                                   '-p:GalleryProject=' + str(gallery)]
                        if mode == 'aot':
                            command += ['-r', 'linux-x64', '-p:PublishAot=true']
                        execute(command, variant / 'build.log', env, 3600)
                        command = ([artifacts / 'publish/Gallery.Tests/release_linux-x64/Gallery.Tests'] if mode == 'aot'
                                   else ['dotnet', artifacts / 'bin/Gallery.Tests/release/Gallery.Tests.dll'])
                        execute(['xdotool', 'mousemove', '1439', '999'], variant / 'pointer.log', runtime_env, 15)
                        execute([*command, captures], variant / 'run.log', runtime_env, 600, fixture)
                        actual = parse_pages(variant / 'run.log')
                        result = dict(form=form, mode=mode, status='running', behaviorStatus='passed',
                                      report=str(captures / 'report.json'),
                                      binarySha256=sha256(Path(command[-1])),
                                      assemblyHashes={path.name: sha256(path) for path in
                                                      (artifacts / 'bin/Gallery.Tests' / ('release_linux-x64' if mode == 'aot' else 'release')).glob('*.dll')})
                        report['variants'].append(result)
                        try:
                            result['comparisons'] = compare(native_pages, actual, native_capture, captures)
                        except Exception as error:
                            result.update(status='failed', comparisonStatus='failed', error=str(error))
                            raise
                        result.update(status='passed', comparisonStatus='passed')
                        print(f'PASS {form}-{mode}: all 27 native gallery comparisons and OS workflows', flush=True)
        report['status'] = 'passed'
    except BaseException as error:
        report['status'] = 'failed'
        report['error'] = str(error)
        raise
    finally:
        (work / 'report.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__':
    main()
