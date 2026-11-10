"""Qualify real Linux dialog/URL tools through the canonical translated CUI app."""
import argparse
import json
import os
from pathlib import Path
import platform
import queue
import re
import shutil
import signal
import subprocess
import sys
import threading
import time
import uuid

ROOT = Path(__file__).resolve().parents[2]
MARKER = 'CUI_DESKTOP_SERVICE\t'
TOOLS = ('zenity', 'kdialog', 'xdg-open', 'gio', 'xdg-mime',
         'update-desktop-database', 'xdotool', 'xclip', 'xvfb-run', 'Xvfb', 'xauth')


def write_json(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def execute(command, log, env, timeout=600):
    print('+ ' + subprocess.list2cmdline(command), flush=True)
    with log.open('w', encoding='utf-8') as output:
        process = subprocess.Popen(command, env=env, stdout=output, stderr=subprocess.STDOUT,
                                   start_new_session=True)
        try:
            code = process.wait(timeout=timeout)
        except BaseException:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            raise
    # Large canonical-product compiler logs stay in disposable storage.
    lines = log.read_text(encoding='utf-8', errors='replace').splitlines()
    print('\n'.join(lines if len(lines) < 45 else lines[-35:]), flush=True)
    if code:
        raise RuntimeError(f'Command exited {code}; full log: {log}')


def make_executable(path, body):
    path.write_text('#!' + sys.executable + '\n' + body, encoding='utf-8')
    path.chmod(0o700)


def sandbox(root, tool, real, temporary):
    for name in ('home', 'config', 'config-dirs', 'data', 'data-dirs', 'cache',
                 'runtime', 'bin', 'empty-bin', 'tool-status', 'fixture'):
        (root / name).mkdir(parents=True, exist_ok=True)
    (root / 'runtime').chmod(0o700)
    # Toolkit resources are read-only system data, separate from desktop
    # application associations. GTK aborts without its GSettings schemas.
    for name in ('glib-2.0', 'mime', 'icons'):
        resource = Path('/usr/share') / name
        if resource.exists():
            (root / 'data-dirs' / name).symlink_to(resource)
    env = dict(os.environ)
    for name in ('WAYLAND_DISPLAY', 'DBUS_STARTER_ADDRESS', 'DBUS_STARTER_BUS_TYPE',
                 'SESSION_MANAGER', 'DESKTOP_SESSION', 'KDE_FULL_SESSION',
                 'GNOME_DESKTOP_SESSION_ID', 'MATE_DESKTOP_SESSION_ID', 'DISPLAY',
                 'XDG_ACTIVATION_TOKEN', 'DESKTOP_STARTUP_ID'):
        env.pop(name, None)
    # DISPLAY and XAUTHORITY are supplied only by our enclosing xvfb-run.
    env.update(DISPLAY=os.environ['DISPLAY'], HOME=str(root / 'home'),
               XDG_CONFIG_HOME=str(root / 'config'), XDG_CONFIG_DIRS=str(root / 'config-dirs'),
               XDG_DATA_HOME=str(root / 'data'), XDG_DATA_DIRS=str(root / 'data-dirs'),
               XDG_CACHE_HOME=str(root / 'cache'), XDG_RUNTIME_DIR=str(root / 'runtime'),
               XDG_CURRENT_DESKTOP='X-Generic', DE='generic', BROWSER=shutil.which('false'),
               PATH=str(root / 'bin'), TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary),
               DBUS_SESSION_BUS_ADDRESS='unix:path=' + str(root / 'runtime/no-session-bus'),
               GDK_BACKEND='x11', GTK_USE_PORTAL='0', GIO_USE_VFS='local',
               GSETTINGS_BACKEND='memory', NO_AT_BRIDGE='1', QT_QPA_PLATFORM='xcb',
               QT_ACCESSIBILITY='0', QT_USE_PORTAL='0', LIBGL_ALWAYS_SOFTWARE='1',
               LC_ALL='C.UTF-8', LANG='C.UTF-8')
    # Remove an inherited DE override: xdg-open detects the deliberately selected
    # X-Generic/GNOME value; the GNOME case must actually take the gio branch.
    env.pop('DE', None)
    for name in ('sed', 'grep', 'egrep', 'cut', 'cat', 'head', 'tail', 'which', 'basename',
                 'dirname', 'readlink', 'tr', 'awk', 'expr', 'uname', 'file', 'xdg-mime', 'gio'):
        target = shutil.which(name)
        if not target:
            raise RuntimeError('Missing actual utility: ' + name)
        (root / 'bin' / name).symlink_to(target)
    # Transparent recording trampolines: no fabricated stdout or exit status.
    # Each one launches the installed binary with exactly the received argv.
    mapping = {name: real[name] for name in (tool, 'xdg-open')}
    recorder = f'''import json, os, pathlib, subprocess, sys, threading, time, uuid
real = {mapping!r}[pathlib.Path(sys.argv[0]).name]
base = pathlib.Path({str(root / 'tool-status')!r}) / uuid.uuid4().hex
record = dict(executable=real, argv=sys.argv[1:], started_ns=time.time_ns())
child = subprocess.Popen([real, *sys.argv[1:]], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
record['pid'] = child.pid
base.with_suffix('.started.json').write_text(json.dumps(record), encoding='utf-8')
def forward(stream, target, suffix):
    with base.with_suffix(suffix).open('wb') as log:
        for line in stream:
            log.write(line)
            log.flush()
            target.write(line)
            target.flush()
reader = threading.Thread(target=forward, args=(child.stderr, sys.stderr.buffer, '.stderr.log'))
writer = threading.Thread(target=forward, args=(child.stdout, sys.stdout.buffer, '.stdout.log'))
reader.start()
writer.start()
code = child.wait()
reader.join()
writer.join()
record.update(returncode=code, finished_ns=time.time_ns())
temp = base.with_suffix('.tmp')
temp.write_text(json.dumps(record), encoding='utf-8')
temp.replace(base.with_suffix('.done.json'))
sys.exit(code if code >= 0 else 128-code)
'''
    for name in mapping:
        make_executable(root / 'bin' / name, recorder)
    handler = root / 'bin/record-argv'
    make_executable(handler, '''import json, os, pathlib, subprocess, sys
target = pathlib.Path(os.environ['CUI_DESKTOP_ARGV_LOG'])
if os.environ.get('CUI_DESKTOP_HANDLER_CHILD') == '1':
    temp = target.with_suffix('.tmp')
    temp.write_bytes(b''.join(os.fsencode(s) + b'\\0' for s in sys.argv[1:]))
    temp.replace(target)
    sys.exit(int(os.environ['CUI_DESKTOP_HANDLER_EXIT']))
env = dict(os.environ, CUI_DESKTOP_HANDLER_CHILD='1')
child = subprocess.Popen([sys.executable, __file__, *sys.argv[1:]], env=env)
code = child.wait()
temp = target.with_suffix('.handler.tmp')
temp.write_text(json.dumps(dict(argv=sys.argv[1:], exit_code=code, pid=os.getpid(), reaped_child_pid=child.pid)), encoding='utf-8')
temp.replace(target.with_suffix('.handler.json'))
sys.exit(code)
''')
    # Older installed xdg-open URI detection accepts letters but omits digits
    # from its regex, although digits are legal after the first scheme letter.
    scheme = 'cuids' + uuid.uuid4().hex.translate(str.maketrans('0123456789', 'ghijklmnop'))
    (root / 'scheme.txt').write_text(scheme, encoding='ascii')
    applications = root / 'data/applications'
    applications.mkdir()
    (applications / 'cui-disposable.desktop').write_text(
        '[Desktop Entry]\nType=Application\nName=CUI disposable argv recorder\n'
        f'Exec={handler} desktop-fixed %u\nTerminal=false\nNoDisplay=true\n'
        f'MimeType=x-scheme-handler/{scheme};text/plain;\n', encoding='utf-8')
    (root / 'config/mimeapps.list').write_text(
        f'[Default Applications]\nx-scheme-handler/{scheme}=cui-disposable.desktop;\n'
        'text/plain=cui-disposable.desktop;\n', encoding='utf-8')
    execute([real['update-desktop-database'], str(applications)], root / 'desktop-database.log', env)
    fixture = root / 'fixture'
    for name in ("atverti λ Žąsis ' ; &.txt", "-literal λ ' ; &.txt"):
        (fixture / name).write_text('Private CUI desktop-service fixture.\n' * 4, encoding='utf-8')
    (fixture / 'aplankas λ Žąsis').mkdir()
    return env


def xdo(real, env, *arguments, check=True):
    result = subprocess.run([real['xdotool'], *arguments], env=env, stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE, encoding='utf-8', timeout=5)
    if check and result.returncode:
        raise RuntimeError(f'xdotool {arguments}: {result.stderr}')
    return result


def windows(real, env, title):
    # KDE may append a tool caption after the caller-supplied title.
    result = xdo(real, env, 'search', '--onlyvisible', '--name', '^' + re.escape(title) + '($| .*)', check=False)
    return result.stdout.split() if result.returncode == 0 else []


def paste(real, env, text):
    clipboard = subprocess.Popen([real['xclip'], '-selection', 'clipboard', '-in', '-quiet'],
                                 env=env, stdin=subprocess.PIPE, stdout=subprocess.DEVNULL,
                                 stderr=subprocess.DEVNULL)
    try:
        clipboard.stdin.write(text.encode('utf-8'))
        clipboard.stdin.close()
        time.sleep(0.1)
        xdo(real, env, 'key', '--clearmodifiers', 'ctrl+v')
        time.sleep(0.25)
    finally:
        clipboard.terminate()
        clipboard.wait(timeout=5)


def drive_dialog(real, env, tool, case, title, path, log):
    deadline = time.monotonic() + 15
    while time.monotonic() < deadline:
        found = windows(real, env, title)
        if found:
            break
        for receipt in (Path(env['HOME']).parent / 'tool-status').glob('*.done.json'):
            record = json.loads(receipt.read_text())
            if any(title in arg for arg in record['argv']):
                raise RuntimeError(f'Actual tool exited before showing a window: {record}; stderr: {receipt.with_suffix("").with_suffix(".stderr.log")}')
        time.sleep(0.05)
    else:
        raise TimeoutError('Real dialog window did not appear: ' + title)
    if len(found) != 1:
        raise RuntimeError(f'Expected one private dialog, found {found}')
    window = found[0]
    pid = int(xdo(real, env, 'getwindowpid', window).stdout)
    starts = [json.loads(p.read_text()) for p in (Path(env['HOME']).parent / 'tool-status').glob('*.started.json')]
    if not any(r['pid'] == pid and r['executable'] == real[tool] for r in starts):
        raise RuntimeError('Window does not belong to the actual selected desktop tool')
    log.write(f'{case}: actual={real[tool]}, window={window}, pid={pid}\n')
    log.flush()
    xdo(real, env, 'windowfocus', '--sync', window)
    time.sleep(0.25)
    if case == 'open-cancel':
        xdo(real, env, 'key', '--clearmodifiers', 'Escape')
    elif case == 'save-confirm':
        # The actual tool selects the seeded parent directory. GTK4 zenity may
        # leave Name blank for a new file: enter the basename in its Name field.
        xdo(real, env, 'key', '--clearmodifiers', 'ctrl+a')
        paste(real, env, Path(path).name)
        xdo(real, env, 'key', '--clearmodifiers', 'Return')
    else:
        # Paste exact UTF-8 via the clipboard of our private X server. XTEST
        # drives Ctrl+V; fast temporary xdotool Unicode keymap changes can race
        # toolkit event processing and lose codepoints.
        # No direct toolkit calls, fabricated stdout, or app callbacks.
        xdo(real, env, 'key', '--clearmodifiers', 'ctrl+l')
        time.sleep(0.15)
        xdo(real, env, 'key', '--clearmodifiers', 'ctrl+a')
        paste(real, env, path)
        xdo(real, env, 'key', '--clearmodifiers', 'Return')
        time.sleep(0.4)
        # Folder choosers navigate first, then confirm the selected directory.
        if windows(real, env, title):
            xdo(real, env, 'key', '--clearmodifiers', 'alt+s' if case == 'save-confirm' else 'alt+o')
    deadline = time.monotonic() + 10
    while windows(real, env, title):
        if time.monotonic() >= deadline:
            capture = shutil.which('import', path=os.defpath)
            if capture:
                subprocess.run([capture, '-window', window, str(Path(log.name).with_suffix('.png'))],
                               env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=5)
            raise TimeoutError('Real dialog did not finish: ' + title)
        time.sleep(0.05)


def run_tool(command, root, tool, env, real):
    records = []
    events = queue.Queue()
    with (root / 'run.log').open('w', encoding='utf-8') as log, (root / 'automation.log').open('w', encoding='utf-8') as automation:
        process = subprocess.Popen([*command, str(root), tool], env=env, cwd=root,
                                   stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                   encoding='utf-8', errors='strict', start_new_session=True)
        def read():
            try:
                for line in process.stdout:
                    events.put(line)
            finally:
                events.put(None)
        reader = threading.Thread(target=read, daemon=True)
        reader.start()
        deadline = time.monotonic() + 180
        passed = False
        try:
            while True:
                if time.monotonic() > deadline:
                    raise TimeoutError('Desktop-services runner timeout')
                try:
                    line = events.get(timeout=0.1)
                except queue.Empty:
                    continue
                if line is None:
                    break
                log.write(line)
                log.flush()
                print(line, end='', flush=True)
                if not line.startswith(MARKER):
                    continue
                parts = line.rstrip('\n').split('\t')[1:]
                event, *fields = parts
                records.append(dict(event=event, fields=fields))
                if event == 'DIALOG':
                    drive_dialog(real, env, tool, *fields, automation)
                elif event == 'PASS':
                    passed = True
            code = process.wait(timeout=10)
            if code or not passed:
                raise RuntimeError(f'{tool} runner exit={code}, PASS={passed}; see {root / "run.log"}')
        finally:
            # Kill only this runner's process group, including a hung real tool.
            # Completed wrappers have already waited/reaped their actual child.
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait()
            reader.join(timeout=5)
    actual = [json.loads(p.read_text()) for p in (root / 'tool-status').glob('*.done.json')]
    dialogs = sorted((r for r in actual if r['executable'] == real[tool]), key=lambda r: r['started_ns'])
    launchers = sorted((r for r in actual if r['executable'] == real['xdg-open']), key=lambda r: r['started_ns'])
    if [r['returncode'] for r in dialogs] != [0, 1, 0, 0, 0]:
        raise AssertionError(f'Actual {tool} statuses: {dialogs}')
    # Generic: handler success, synchronous handler failure, missing association,
    # leading-dash local file. GNOME/gio: launch accepted despite handler exit23.
    if [r['returncode'] for r in launchers] != [0, 4, 3, 0, 0]:
        raise AssertionError(f'Actual xdg-open statuses: {launchers}')
    handlers = {p.stem.removesuffix('.handler'): json.loads(p.read_text()) for p in root.glob('*.handler.json')}
    if {name: r['exit_code'] for name, r in handlers.items()} != {
            'generic-success': 0, 'generic-handler-fails': 23, 'leading-dash': 0, 'gio-handler-fails': 23}:
        raise AssertionError(f'Unexpected private handler records: {handlers}')
    if len(list((root / 'tool-status').glob('*.started.json'))) != len(actual):
        raise AssertionError('An actual desktop tool did not complete')
    for receipt in actual:
        try:
            os.kill(receipt['pid'], 0)
        except ProcessLookupError:
            continue
        raise AssertionError('Actual desktop-tool process still exists: ' + str(receipt['pid']))
    if not all(p.is_file() for p in (root / 'fixture').iterdir() if p.name.endswith('.txt')):
        raise AssertionError('Fixture file damaged')
    summary = dict(status='passed', dialog_tool=tool, assertions=int(records[-1]['fields'][1]),
                   events=records, tools=actual, handlers=handlers,
                   dialog_statuses=[r['returncode'] for r in dialogs],
                   launcher_statuses=[r['returncode'] for r in launchers],
                   cleanup='App released/reinitialized; actual tool children exited/reaped; Xvfb removed by outer coordinator')
    summary['versions'] = {}
    for name in (tool, 'xdg-open', 'gio'):
        result = subprocess.run([real[name], '--version'], env=env, stdout=subprocess.PIPE,
                                stderr=subprocess.STDOUT, encoding='utf-8', timeout=10)
        summary['versions'][name] = dict(path=real[name], returncode=result.returncode, output=result.stdout.strip())
    write_json(root / 'evidence.json', summary)
    return summary


def worker(args, real, command, temporary):
    if os.environ.get('XAUTHORITY') != str(args.work_root / 'xauthority'):
        raise RuntimeError('Worker requires the coordinator-owned Xvfb authorization file')
    siblings = Path(f'/proc/{os.getppid()}/task/{os.getppid()}/children').read_text().split()
    matches = []
    for pid in siblings:
        try:
            arguments = Path(f'/proc/{pid}/cmdline').read_bytes().decode().split('\0')
        except FileNotFoundError:
            continue
        if arguments and Path(arguments[0]).name == 'Xvfb' and os.environ['DISPLAY'] in arguments:
            matches.append(dict(pid=int(pid), display=os.environ['DISPLAY'], argv=arguments[:-1]))
    if len(matches) != 1:
        raise RuntimeError('Worker display is not the coordinator-owned Xvfb child')
    write_json(args.work_root / 'xvfb-process.json', matches[0])
    summaries = []
    for tool in args.dialog_tools.split(','):
        run_root = args.work_root / tool
        # Refuse overwrite of prior private fixtures/statuses: evidence is per run.
        run_root.mkdir()
        env = sandbox(run_root, tool, real, temporary)
        summaries.append(run_tool(command, run_root, tool, env, real))
    write_json(args.work_root / 'runtime-evidence.json', dict(status='passed', runs=summaries,
               windows='unrun: external interactive Windows gate remains required'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True, help='Fresh disposable build/log/fixture directory')
    parser.add_argument('--cui-project', type=Path, default=ROOT / 'generated/TranslatedCui/TranslatedCui.csproj')
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--dialog-tools', default='zenity,kdialog', help='zenity,kdialog (default), or zenity')
    parser.add_argument('--worker', action='store_true', help=argparse.SUPPRESS)
    args = parser.parse_args()
    args.work_root = args.work_root.resolve()
    args.cui_project = args.cui_project.resolve()
    args.work_root.mkdir(parents=True, exist_ok=True)
    if platform.system() != 'Linux':
        write_json(args.work_root / 'evidence.json', dict(status='unrun',
                   reason='Real desktop-tool coordinator requires Linux/X11; Windows external gate remains open'))
        print('UNRUN: Linux/X11 tool coordinator; actual Windows interactive services remain an external gate.')
        return 77
    if any(c.isspace() for c in str(args.work_root)):
        parser.error('xdg-open generic desktop-file lookup requires a work root without whitespace')
    if args.dialog_tools not in ('zenity,kdialog', 'zenity', 'kdialog'):
        parser.error('--dialog-tools must be zenity,kdialog, zenity, or kdialog')
    real = {name: shutil.which(name) for name in (*TOOLS, 'dotnet')}
    required = set(TOOLS) - {'zenity', 'kdialog'} | set(args.dialog_tools.split(',')) | {'dotnet'}
    missing = sorted(name for name in required if not real[name])
    if missing:
        parser.error('Missing installed prerequisites: ' + ', '.join(missing))
    temporary = args.work_root / 'tmp'
    temporary.mkdir(exist_ok=True)
    artifacts = args.work_root / 'artifacts'
    product = 'DesktopServices.Tests'
    command = ([str(artifacts / f'publish/{product}/release_linux-x64' / product)] if args.aot else
               [real['dotnet'], str(artifacts / f'bin/{product}/release/{product}.dll')])
    if args.worker:
        worker(args, real, command, temporary)
        return 0
    env = dict(os.environ, TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary), PYTHONDONTWRITEBYTECODE='1')
    for name in ('DISPLAY', 'WAYLAND_DISPLAY', 'XAUTHORITY'):
        env.pop(name, None)
    build = [real['dotnet'], 'publish' if args.aot else 'build', str(Path(__file__).with_name(product + '.csproj')),
             '-c', 'Release', '--artifacts-path', str(artifacts), '-m:1', '--nologo',
             '-p:CuiProject=' + str(args.cui_project)]
    if args.aot:
        build += ['-r', 'linux-x64', '-p:PublishAot=true']
    started = time.monotonic()
    execute(build, args.work_root / 'build.log', env, timeout=1200)
    worker_command = [real['xvfb-run'], '-a', '-e', str(args.work_root / 'xvfb.log'),
                      '-f', str(args.work_root / 'xauthority'), '-s',
                      '-screen 0 1280x800x24 -nolisten tcp +extension GLX',
                      sys.executable, str(Path(__file__).resolve()), '--worker',
                      '--work-root', str(args.work_root), '--cui-project', str(args.cui_project),
                      '--dialog-tools', args.dialog_tools]
    if args.aot:
        worker_command.append('--aot')
    try:
        execute(worker_command, args.work_root / 'coordinator.log', env, timeout=420)
    finally:
        receipt = args.work_root / 'xvfb-process.json'
        if receipt.exists():
            pid = json.loads(receipt.read_text())['pid']
            deadline = time.monotonic() + 5
            while True:
                try:
                    running = Path(f'/proc/{pid}/cmdline').read_bytes().split(b'\0')[0].endswith(b'Xvfb')
                except FileNotFoundError:
                    running = False
                if not running:
                    break
                if time.monotonic() >= deadline:
                    raise RuntimeError('Coordinator-owned Xvfb did not terminate')
                time.sleep(0.05)
    evidence = json.loads((args.work_root / 'runtime-evidence.json').read_text(encoding='utf-8'))
    evidence.update(project=str(args.cui_project), runtime='NativeAOT' if args.aot else 'JIT',
                    platform=platform.platform(), elapsed_seconds=round(time.monotonic() - started, 2),
                    build_command=build, run_command=worker_command,
                    xvfb=dict(**json.loads((args.work_root / 'xvfb-process.json').read_text()), terminated=True),
                    status_semantics='Public result follows xdg-open exit; gio may accept a launch whose handler later fails')
    write_json(args.work_root / 'evidence.json', evidence)
    print('PASS real desktop services; evidence: ' + str(args.work_root / 'evidence.json'))
    return 0


if __name__ == '__main__':
    sys.exit(main())
