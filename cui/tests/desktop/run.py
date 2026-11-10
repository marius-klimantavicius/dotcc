"""Build and run the full translated desktop input qualification (JIT or AOT)."""
import argparse
import os
from pathlib import Path
import platform
import subprocess

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True,
                        help='Caller-selected disposable build/log directory')
    parser.add_argument('--cui-project', type=Path,
                        default=ROOT / 'generated/TranslatedCui/TranslatedCui.csproj')
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--xvfb', action='store_true', help='Use isolated Xvfb with Mesa on Linux')
    args = parser.parse_args()
    work = args.work_root.resolve()
    work.mkdir(parents=True, exist_ok=True)
    temporary = work / 'tmp'
    temporary.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary))
    if args.xvfb:
        env['LIBGL_ALWAYS_SOFTWARE'] = '1'
    artifacts = work / 'artifacts'
    rid = 'win-x64' if platform.system() == 'Windows' else 'linux-x64'
    command = ['dotnet', 'publish' if args.aot else 'build',
               str(ROOT / 'tests/desktop/DesktopInput.Tests.csproj'), '-c', 'Release',
               '--artifacts-path', str(artifacts), '-m:1', '--nologo',
               '-p:CuiProject=' + str(args.cui_project.resolve())]
    if args.aot:
        command += ['-r', rid, '-p:PublishAot=true']
    execute(command, work / 'build.log', env)
    if args.aot:
        command = [str(artifacts / 'publish/DesktopInput.Tests' / ('release_' + rid) /
                       ('DesktopInput.Tests.exe' if rid == 'win-x64' else 'DesktopInput.Tests'))]
    else:
        command = ['dotnet', str(artifacts / 'bin/DesktopInput.Tests/release/DesktopInput.Tests.dll')]
    if args.xvfb:
        if platform.system() != 'Linux':
            parser.error('--xvfb requires Linux')
        command = ['xvfb-run', '-a', '-s', '-screen 0 1280x800x24 +extension GLX', *command]
    execute(command, work / 'run.log', env, timeout=180)


def execute(command, log, env, timeout=None):
    print('+ ' + subprocess.list2cmdline(command), flush=True)
    with log.open('w', encoding='utf-8') as output:
        result = subprocess.run(command, env=env, stdout=output, stderr=subprocess.STDOUT, timeout=timeout)
    print(log.read_text(encoding='utf-8'), end='', flush=True)
    result.check_returncode()


if __name__ == '__main__':
    main()
