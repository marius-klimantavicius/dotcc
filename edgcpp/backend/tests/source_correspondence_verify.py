"""Check readable source correspondence alongside C/C++ execution equivalence."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys


def function_body(source, name):
    # This fixture has no strings/interpolation in the inspected methods.
    # Permit descriptive C++ mangling, but reject a public wrapper as a
    # substitute for inspection of the actual implementation body.
    pattern = re.compile(r'^\s*(?:public|private|internal)\s+[^\n{};=]*?\b@?'
                         + re.escape(name) + r'(?:__[\w]+)?\s*\([^\n{};]*\)\s*\{', re.M)
    choices = []
    for match in pattern.finditer(source):
        begin = match.end()
        depth = 1
        end = begin
        while depth and end < len(source):
            depth += (source[end] == '{') - (source[end] == '}')
            end += 1
        if depth:
            raise AssertionError('Unbalanced generated fixture method ' + name)
        choices.append(source[begin:end - 1])
    if not choices:
        raise AssertionError('Missing source-named implementation: ' + name)
    return max(choices, key=len)


def audit_source(directory):
    source = '\n'.join(path.read_text() for path in sorted(directory.glob('Translated*.cs')))
    failures = []
    bodies = {}
    for name in ['sum', 'update_flags', 'contribution', 'control_flow', 'keyword_names', 'shadowing']:
        try:
            bodies[name] = function_body(source, name)
        except AssertionError as error:
            failures.append(str(error))
    for name in ['sum', 'update_flags', 'contribution', 'control_flow']:
        body = bodies.get(name, '')
        if re.search(r'__edg_(?:assign|bits_mutate)\d+', body):
            failures.append(name + ': numbered assignment/mutation helper in ordinary source code')
        if re.search(r'\bgoto\b|__edg_(?:if|loop|for|switch)\d+', body):
            failures.append(name + ': flattened ordinary structured control flow')
        if re.search(r'\?\s*1\s*:\s*0', body):
            failures.append(name + ': unnecessary integer truth roundtrip')
        if re.search(r'0x[0-9a-fA-F]+UL', body):
            failures.append(name + ': source decimal constants replaced by casted hexadecimal constants')
    required = {
        'sum': [r'\bfor\s*\(', r'\btotal\s*\+=', r'\breturn\s+total\b',
                r'\bitems\b', r'\bindex\b', r'\.\s*@?value\b'],
        'update_flags': [r'->\s*@?ready\s*=', r'->\s*@?mode\s*\+=', r'->\s*@?delta\s*='],
        'contribution': [r'\bswitch\s*\(', r'\bcase\s+', r'\bdefault\s*:', r'\bModeAdd\b'],
        'control_flow': [r'\bfor\s*\(', r'\bwhile\s*\(', r'\bdo\b', r'\bif\s*\(',
                         r'\belse\b', r'\bbreak\s*;', r'\bcontinue\s*;', r'\badjustment\b'],
    }
    for name, patterns in required.items():
        for pattern in patterns:
            if not re.search(pattern, bodies.get(name, '')):
                failures.append(name + ': missing recognizable source construct ' + pattern)
    for pattern in [r'\bstruct\s+@?Item\b', r'\bstruct\s+@?Flags\b',
                    r'\benum\s+@?Mode\b', r'\bModeSkip\b', r'\bModeDouble\b',
                    r'\bpublic\s+\w+\s+@?value\b', r'\bpublic\s+\w+\s+@?weight\b',
                    r'\bpublic\s+\w+\s+@?ready\s*\{',
                    r'\bpublic\s+\w+\s+@?mode\s*\{',
                    r'\bpublic\s+\w+\s+@?delta\s*\{',
                    r'@event\b', r'@ref\b', r'@params\b', r'@out\b', r'@lock\b']:
        if not re.search(pattern, source):
            failures.append('missing source declaration/keyword escape ' + pattern)
    report = {'failures': failures, 'reviewedFunctionBodies': bodies}
    (directory / 'source-correspondence-review.json').write_text(json.dumps(report, indent=2) + '\n')
    if failures:
        raise AssertionError('; '.join(failures))
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--native-only', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    source = Path(__file__).with_name('source_correspondence.c')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'checks': [],
              'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest()}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, **kwargs):
        command = list(map(str, command))
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        stdout = stderr = b''
        try:
            result = subprocess.run(command, capture_output=True, timeout=600, **kwargs)
            stdout, stderr = result.stdout, result.stderr
            entry['exitCode'] = result.returncode
        except subprocess.TimeoutExpired as error:
            stdout, stderr = error.stdout or b'', error.stderr or b''
            entry['timedOut'] = True
            raise
        finally:
            (out / (label + '.stdout')).write_bytes(stdout)
            (out / (label + '.stderr')).write_bytes(stderr)
            save()
        if result.returncode:
            raise RuntimeError(label + ' failed; see retained logs')

    try:
        for language, compiler, standard in [('c', 'gcc', 'c11'), ('c++', 'g++', 'c++17')]:
            label = language.replace('+', 'x')
            run(label + '-native-build', [compiler, '-std=' + standard, '-O2', source, '-o', out / label])
            run(label + '-native', [out / label])
            original = out / (label + '-original.c')
            run(label + '-edg-lower', [root / 'build/bin/cpfe', '--' + standard, '--gnu_version', '130300',
                                      *(['--instantiate', 'used'] if language == 'c++' else []),
                                      '--gen_c_file_name', original, '-o', out / (label + '.il'), source],
                env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc')))
            run(label + '-original-build', ['gcc', '-std=gnu11', '-O2', original,
                                            root / 'build/lib/libC.a', '-o', out / (label + '-original')])
            run(label + '-original', [out / (label + '-original')])
            report['checks'].append(label + ': source and original EDG C semantics')
            if args.native_only:
                continue
            for instance, split in [(False, 'none'), (True, 'size')]:
                mode = label + ('-instance' if instance else '-static')
                directory = out / mode
                run(mode + '-translate', [sys.executable, root / 'dotnet.py', source, '--language', language,
                                          '--class-name', 'Readable', '--split', split, '--split-size', '2500',
                                          '--output-dir', directory,
                                          *(['--instance-methods'] if instance else [])])
                audit_source(directory)
                report['checks'].append(mode + ': named declarations and structured expression/control-flow review')
                run(mode + '-build', ['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release',
                                      '--nologo', '-p:TreatWarningsAsErrors=true'])
                run(mode + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
                report['checks'].append(mode + ': JIT')
                if args.nativeaot:
                    run(mode + '-publish', ['dotnet', 'publish', directory / 'Generated.csproj', '-c', 'Release',
                                            '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true',
                                            '-o', directory / 'aot'])
                    run(mode + '-nativeaot', [directory / 'aot/Generated'])
                    report['checks'].append(mode + ': NativeAOT')
        report['status'] = 'passed'
    except Exception as error:
        report['status'] = 'failed'
        report['failure'] = str(error)
        raise
    finally:
        save()
    print(json.dumps(report['checks'], indent=2))


if __name__ == '__main__':
    main()
