"""Candidate/baseline actual target header/layout and real object controls.

Run sequentially after the native group. No full product is generated.
"""
import json
import os
from pathlib import Path
import shutil
import sys

from source_review import CPY, WORK, ref, read
sys.path.insert(0, str(CPY.parent / 'Scripts'))
from campaigns.process import execute
from campaigns.recipes import helper


def main():
    assert (WORK / 'native-feature/result.json').is_file(), 'Native feature controls must pass first'
    config = helper(CPY / 'scripts/pyconfig.py')
    candidate = read(WORK / 'candidate-policy.json')
    baseline = read(WORK / 'before/config/pyconfig-policy.json')
    review = read(WORK / 'candidate-source-review.json')
    base = WORK / ('header-controls' + ('-' + sys.argv[1] if len(sys.argv) > 1 else ''))
    base.mkdir()
    records = []
    environment = dict(os.environ, DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0',
                       MSBUILDDISABLENODEREUSE='1', UseSharedCompilation='false', DOTCC_CAMPAIGN_COMMAND_LOCKS='')

    def run(argv, label, native=False):
        record = {}
        records.append(record)
        lock = '/tmp/dotcc-cpython-native-build.lock' if native else '/tmp/dotcc-cpython-shared-build.lock'
        try:
            execute(['flock', '-n', '--close', lock, *argv], cwd=CPY.parent, env=environment,
                    label=label, output=base / (label + '.log'), timeout=90, record=record)
        finally:
            (base / 'commands.json').write_text(json.dumps(records, indent=2) + '\n')
        print(json.dumps(dict(label=label, exit_code=record['exit_code'], seconds=record['seconds'])), flush=True)
        return (base / (label + '.log')).read_text()

    compiler = CPY / 'build/owner-a-r31-20261018/v22/tools/compiler/dotcc.dll'
    source = Path(__file__).parent / 'header_contract.c'
    bridge = CPY / 'NativeOracle/ForkHooks/native_layout_syntax.h'
    results = {}
    for profile in ('embedded-gil', 'upstream-tests-static'):
        upstream = Path(review['profiles'][profile]['sourceRoot'])
        observed = {}
        for label, document in (('baseline', baseline), ('candidate', candidate)):
            directory = base / profile / label
            directory.mkdir(parents=True)
            (directory / 'pyconfig.h').write_text(config.render(config.resolve(document, profile)))
            includes = [directory, upstream / 'Include', upstream / 'Include/internal', upstream]
            options = [arg for path in includes for arg in ('-I', path)]
            expectation = '-DR42_EXPECT_UTF8=' + str(int(label == 'candidate'))
            executable = directory / 'native-layout'
            native = run(['cc', '-std=c11', '-nostdinc', '-fshort-wchar', '-U__GNUC__', '-U__GNUC_MINOR__',
                '-U__GNUC_PATCHLEVEL__', '-D__dotcc__=1', '-DR42_NATIVE_HEADER_CONTROL=1', expectation,
                '-include', bridge, *options, '-I', CPY.parent / 'DotCC.Lib/include', source, '-o', executable],
                profile+'-'+label+'-native-build', native=True)
            output = run([executable], profile+'-'+label+'-native-layout', native=True)
            values = list(map(int, output.split()))
            assert len(values) == 8 and values[4] == 2
            obj = directory / 'header.cs'
            run(['dotnet', compiler, '--emit=obj', '-std=c11', expectation, *options, source, '-o', obj],
                profile+'-'+label+'-object')
            text = obj.read_text()
            # Real compiler evaluates these sizeof/offsetof functions. Retain
            # the complete object and use its narrow function bodies only.
            import re
            bodies = {}
            for name in ('interpreter_size', 'runtime_size', 'main_offset', 'warnings_offset'):
                match = re.search(r'\br42_' + name + r'\([^)]*\)\s*\{([^}]*)\}', text)
                assert match, name
                bodies[name] = match.group(1).strip()
            observed[label] = dict(nativeValues=values, emittedFunctions=bodies, object=ref(obj), header=ref(directory / 'pyconfig.h'))
        assert observed['baseline']['nativeValues'] == observed['candidate']['nativeValues']
        assert observed['baseline']['emittedFunctions'] == observed['candidate']['emittedFunctions']
        results[profile] = observed
    (base / 'result.json').write_text(json.dumps(dict(status='Target-header/native layout and real object before/after controls PASS',
        scope='No actual regenerated CPython layout/execution claim', compiler=ref(compiler), source=ref(source),
        nativeSyntaxBridge=ref(bridge), profiles=results, commands=ref(base / 'commands.json')), indent=2) + '\n')
    print(json.dumps(ref(base / 'result.json')))


if __name__ == '__main__':
    main()
