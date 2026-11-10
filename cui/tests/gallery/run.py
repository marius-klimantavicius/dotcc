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
import sys
import shutil
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]
REPOSITORY = ROOT.parent
sys.dont_write_bytecode = True
sys.path.insert(0, str(ROOT / "scripts"))
from stage import adapt_text_renderer, adapt_widgets

GALLERY_FILES = ("gallery_ui.c", "gallery_ui.h", "gallery_more.c", "gallery_more.h")
EMOJI_CORE_FILES = ("src/cui_text.c", "src/cui_textedit.c", "src/cui_emoji.c")


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


def parse_emoji(log, diagnostic=False):
    messages, recent, pixels, edits, inline, visibility = [], [], [], [], [], []
    message_pixels_changed = None
    summary = None
    for line in log.read_text(encoding='utf-8').splitlines():
        fields = line.split('\t')
        if fields[0] == 'EMOJI_MESSAGE':
            messages.append(dict(length=int(fields[1]), hex=fields[2], text=fields[3]))
        elif fields[0] == 'EMOJI_RECENT':
            recent.append(fields[1])
        elif fields[0] == 'EMOJI_RENDER_CHANGE':
            message_pixels_changed = bool(int(fields[1]))
        elif fields[0] == 'EMOJI_PIXELS':
            pixels.append(dict(name=fields[1], width=int(fields[2]), height=int(fields[3]), colored=int(fields[4])))
        elif fields[0] == 'EMOJI_EDIT':
            edits.append(dict(step=fields[1], length=int(fields[2]), cursor=int(fields[3]), anchor=int(fields[4]), hex=fields[5]))
        elif fields[0] == 'EMOJI_INLINE':
            inline.append(dict(name=fields[1], count=int(fields[2]), colored=int(fields[3]),
                               bounds=list(map(int, fields[4:8])), shades=int(fields[8])))
        elif fields[0] == 'EMOJI_VISIBILITY_ASSERT':
            visibility.append(dict(name=fields[1], passed=bool(int(fields[2]))))
        elif fields[0] == 'EMOJI_VISIBILITY_SUMMARY':
            summary = (int(fields[1]), fields[2])
    expected_recent = ['', '😀', '😃 😀', '😀 😃', '😀 😃']
    prefix = 'Release notes are up '
    if recent != expected_recent or not messages or messages[-1]['text'] != prefix:
        raise RuntimeError(f'{log}: incomplete emoji selections/recent persistence')
    for message in messages:
        actual = bytes.fromhex(message['hex'])
        if actual.decode('utf-8', errors='strict') != message['text'] or len(actual) != message['length']:
            raise RuntimeError(f'{log}: invalid UTF-8 message transcript')
    for text in (prefix + '😀', prefix + '😀😃', prefix + '😀😃😀'):
        if not any(row['text'] == text for row in messages):
            raise RuntimeError(f'{log}: missing exact picked message {text!r}')
    original, mixed = prefix + '😀😃😀', prefix + '😀😃X😀'
    expected_edits = [
        ('end', original, 33, 33), ('left-over-emoji', original, 29, 29),
        ('right-over-emoji', original, 33, 33), ('insert-position', original, 29, 29),
        ('insert-between', mixed, 30, 30), ('home', mixed, 0, 0),
        ('right-over-ascii', mixed, 1, 1), ('end-mixed', mixed, 34, 34),
        ('backspace-emoji', prefix + '😀😃X', 30, 30), ('left-over-ascii', prefix + '😀😃X', 29, 29),
        ('left-over-second', prefix + '😀😃X', 25, 25), ('delete-emoji', prefix + '😀X', 25, 25),
        ('right-after-delete', prefix + '😀X', 26, 26), ('backspace-ascii', prefix + '😀', 25, 25),
        ('select-whole-emoji', prefix + '😀', 21, 25), ('replace-selected-emoji', prefix + 'Z', 22, 22),
        ('restore-plain-prefix', prefix, 21, 21)]
    expected = [dict(step=step, length=len(text.encode('utf-8')), cursor=cursor, anchor=anchor,
                     hex=text.encode('utf-8').hex().upper()) for step, text, cursor, anchor in expected_edits]
    if edits != expected:
        raise RuntimeError(f'{log}: missing or incorrect OS UTF-8 boundary editing transcript')
    cells = [row for row in pixels if row['name'].endswith('-cell')]
    if len(cells) != 3 or any(row['colored'] < 50 for row in cells):
        raise RuntimeError(f'{log}: missing colored GL readback for three actual emoji picks')
    expected_inline = [('emoji-0-inline', 0), ('emoji-1-inline', 1), ('emoji-2-inline', 2),
                       ('emoji-3-inline', 3), ('emoji-edit-mixed-inline', 3), ('emoji-edit-plain-inline', 0)]
    if [(row['name'], row['count']) for row in inline] != expected_inline:
        raise RuntimeError(f'{log}: incomplete original editable-input GL captures')
    expected_visibility = ['emoji-0-inline', 'emoji-1-inline', 'picked-message-changes',
                           'emoji-2-inline', 'emoji-3-inline', 'emoji-edit-mixed-inline', 'emoji-edit-plain-inline']
    if [row['name'] for row in visibility] != expected_visibility or summary != (
            sum(not row['passed'] for row in visibility), 'diagnostic' if diagnostic else 'strict'):
        raise RuntimeError(f'{log}: incomplete inline visibility assertions or incorrect diagnostic mode')
    if message_pixels_changed is None or (not diagnostic and (not message_pixels_changed or not all(row['passed'] for row in visibility))):
        raise RuntimeError(f'{log}: picked emoji is invisible inside the original editable input')
    return dict(status='diagnostic' if diagnostic else 'passed', selections=3, messages=messages, recent=recent, pixels=pixels,
                edits=edits, inline=inline, visibility=visibility,
                messagePixelsChanged=message_pixels_changed,
                rendering='Colored glyph shape inside original edit-paint leaf, zero-color plain controls, OS UTF-8 edits, exact native inline pixels')


def validate_inline_pixels(emoji, directory, diagnostic=False):
    """Independently check actual saved RGBA; a changed crop or text alone cannot pass."""
    for row in emoji['inline']:
        crop = next(item for item in emoji['pixels'] if item['name'] == row['name'])
        width, height = crop['width'], crop['height']
        pixels = (directory / (row['name'] + '.rgba')).read_bytes()
        if len(pixels) != width * height * 4:
            raise RuntimeError(f'{row["name"]}: incorrect original-input framebuffer length')
        points, shades = [], set()
        for at in range(0, len(pixels), 4):
            r, g, b = pixels[at:at + 3]
            if max(r, g, b) - min(r, g, b) > 32:
                points.append((at // 4 % width, at // 4 // width))
                shades.add((r >> 4, g >> 4, b >> 4))
        bounds = ([min(x for x, y in points), min(y for x, y in points),
                   max(x for x, y in points), max(y for x, y in points)] if points else [width, height, -1, -1])
        if row['colored'] != len(points) or crop['colored'] != len(points) or row['bounds'] != bounds or row['shades'] != len(shades):
            raise RuntimeError(f'{row["name"]}: saved pixels do not match observed colored shape')
        count = row['count']
        visible = (len(points) >= 30 * count and bounds[2] - bounds[0] + 1 >= 6 * count and
                   bounds[3] - bounds[1] + 1 >= 6 and len(shades) >= 3) if count else not points
        observed = next(item['passed'] for item in emoji['visibility'] if item['name'] == row['name'])
        if visible != observed or (not diagnostic and not visible):
            raise RuntimeError(f'{row["name"]}: missing colored emoji shape inside original editable input')
        row['sha256'] = hashlib.sha256(pixels).hexdigest()


def compare_emoji(native, managed, native_dir, managed_dir):
    rows = []
    for expected, actual in zip(native['pixels'], managed['pixels'], strict=True):
        if any(expected[key] != actual[key] for key in ('name', 'width', 'height')):
            raise RuntimeError('Native/managed emoji GL crop names or dimensions differ')
        if not expected['name'].endswith(('-cell', '-inline')):
            continue
        filename = expected['name'] + '.rgba'
        left, right = (native_dir / filename).read_bytes(), (managed_dir / filename).read_bytes()
        if len(left) != expected['width'] * expected['height'] * 4 or len(right) != len(left):
            raise RuntimeError(f'{filename}: incorrect emoji framebuffer length')
        maximum = max(abs(a - b) for a, b in zip(left, right, strict=True))
        tolerance = 0 if expected['name'].endswith('-inline') else 1
        if maximum > tolerance:
            raise RuntimeError(f'{filename}: native/managed emoji GL pixels exceed tolerance {tolerance}, max {maximum}')
        rows.append(dict(name=expected['name'], bytes=len(left), maximumError=maximum,
                         nativeSha256=hashlib.sha256(left).hexdigest(), managedSha256=hashlib.sha256(right).hexdigest()))
    return rows


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
    parser.add_argument('--diagnostic-allow-invisible-emoji', action='store_true',
                        help='Retain all before-fix workflows/crops despite visibility failures; never qualifies as a passing run')
    parser.add_argument('--native-oracle', choices=('adapted', 'original'), default='adapted',
                        help='Original is a diagnostic expected to fail the emoji popup-close regression')
    args = parser.parse_args()
    if platform.system() != 'Linux':
        parser.error('This native/OS runner requires Linux/X11. Windows gallery execution remains unrun.')
    work = args.work_root.resolve()
    if work.is_relative_to(REPOSITORY):
        parser.error('--work-root must be outside the repository')
    source = args.source.resolve()
    for name in GALLERY_FILES:
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
    # An inherited diagnostic environment must never weaken normal acceptance.
    env.pop('CUI_GALLERY_DIAGNOSTIC_INVISIBLE', None)
    if args.diagnostic_allow_invisible_emoji:
        env['CUI_GALLERY_DIAGNOSTIC_INVISIBLE'] = '1'
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
                  diagnosticAllowInvisibleEmoji=args.diagnostic_allow_invisible_emoji,
                  source=str(source), sourceHashes={name: hashlib.sha256((source / 'examples' / name).read_bytes()).hexdigest()
                                                   for name in GALLERY_FILES},
                  emojiCoreSourceHashes={name: sha256(source / name) for name in EMOJI_CORE_FILES},
                  comparison='Main-content nodes/text exact; all RGBA channels within 1; no dynamic-content exclusion')
    try:
        native = work / 'native'
        native.mkdir(exist_ok=True)
        native_source = native / 'source'
        if native_source.exists():
            shutil.rmtree(native_source)
        shutil.copytree(source, native_source, ignore=shutil.ignore_patterns('.git', 'build', '__pycache__'))
        adaptations = (adapt_widgets(native_source) + adapt_text_renderer(native_source)) if args.native_oracle == 'adapted' else []
        report['nativeOracle'] = dict(kind=args.native_oracle, source=str(native_source), adaptations=adaptations,
                                     sourceHashes={name: sha256(native_source / 'examples' / name) for name in GALLERY_FILES},
                                     emojiCoreSourceHashes={name: sha256(native_source / name) for name in EMOJI_CORE_FILES})
        native_capture = native / 'captures'
        native_capture.mkdir(exist_ok=True)
        execute(['cmake', '-S', ROOT / 'tests/gallery', '-B', native / 'build', '-DCMAKE_BUILD_TYPE=Release',
                 '-DCUI_SOURCE_ROOT=' + str(native_source)], native / 'configure.log', env)
        execute(['cmake', '--build', native / 'build', '--target', 'gallery-control', '--parallel', '2'],
                native / 'build.log', env)
        report['nativeBinarySha256'] = sha256(native / 'build/gallery-control')
        with display(work, env, args.xvfb) as (runtime_env, display_record):
            report['display'] = display_record
            execute(['xdotool', 'mousemove', '1439', '999'], native / 'pointer.log', runtime_env, 15)
            execute([native / 'build/gallery-control', native_capture], native / 'run.log', runtime_env, 180, fixture)
            native_pages = parse_pages(native / 'run.log')
            report['nativePages'] = native_pages
            report['nativeInput'] = str(native / 'input.log')
            report['nativeInputStatus'] = 'running'
            execute([native / 'build/gallery-control', '--input'], native / 'input.log',
                    dict(runtime_env, CUI_GALLERY_INPUT_CAPTURE=str(native_capture)), 600, fixture)
            report['nativeEmoji'] = parse_emoji(native / 'input.log', args.diagnostic_allow_invisible_emoji)
            validate_inline_pixels(report['nativeEmoji'], native_capture, args.diagnostic_allow_invisible_emoji)
            report['nativeInputStatus'] = 'diagnostic' if args.diagnostic_allow_invisible_emoji else 'passed'
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
                        emoji = parse_emoji(variant / 'run.log', args.diagnostic_allow_invisible_emoji)
                        validate_inline_pixels(emoji, captures, args.diagnostic_allow_invisible_emoji)
                        for field in ('messages', 'recent', 'edits'):
                            if emoji[field] != report['nativeEmoji'][field]:
                                raise RuntimeError(f'{form}-{mode}: emoji {field} differs from native')
                        emoji['comparisons'] = compare_emoji(report['nativeEmoji'], emoji, native_capture, captures)
                        result = dict(form=form, mode=mode, status='running', behaviorStatus='passed',
                                      report=str(captures / 'report.json'), emoji=emoji,
                                      binarySha256=sha256(Path(command[-1])),
                                      assemblyHashes={path.name: sha256(path) for path in
                                                      (artifacts / 'bin/Gallery.Tests' / ('release_linux-x64' if mode == 'aot' else 'release')).glob('*.dll')})
                        report['variants'].append(result)
                        try:
                            result['comparisons'] = compare(native_pages, actual, native_capture, captures)
                        except Exception as error:
                            result.update(status='failed', comparisonStatus='failed', error=str(error))
                            raise
                        result.update(status='diagnostic' if args.diagnostic_allow_invisible_emoji else 'passed', comparisonStatus='passed')
                        print(f'{result["status"].upper()} {form}-{mode}: all 27 native gallery comparisons and OS workflows', flush=True)
        report['status'] = 'diagnostic' if args.diagnostic_allow_invisible_emoji else 'passed'
    except BaseException as error:
        report['status'] = 'failed'
        report['error'] = str(error)
        if report.get('nativeInputStatus') == 'running':
            report['nativeInputStatus'] = 'failed'
        raise
    finally:
        (work / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    if args.diagnostic_allow_invisible_emoji:
        raise SystemExit('Diagnostic capture only: inline visibility acceptance was disabled; report is not a qualification pass')


if __name__ == '__main__':
    main()
