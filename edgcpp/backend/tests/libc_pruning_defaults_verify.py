#!/usr/bin/env python3
"""Verify musl defaults, the complete-provider opt-out, and raw packaging."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path, default=ROOT/'build/bin/cpfe-dot')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    source = out/'main.c'
    source.write_text('int main(void) { return 0; }\n')
    report = {'passed':False, 'commands':[], 'checks':[]}
    def save():
        (out/'results.json').write_text(json.dumps(report, indent=2)+'\n')
    def translate(label, options, raw_input=None):
        command = [sys.executable, str(ROOT/'dotnet.py'), str(source), '--language','c',
                   '--compiler',str(args.compiler.resolve()), '--output-dir',str(out/label),
                   '--split','source', *options]
        command += ['--raw-input',str(raw_input)] if raw_input else ['--raw-output',str(out/(label+'.raw.cs'))]
        result = subprocess.run(command, capture_output=True, timeout=300)
        (out/(label+'.stdout')).write_bytes(result.stdout)
        (out/(label+'.stderr')).write_bytes(result.stderr)
        report['commands'].append({'label':label,'command':command,'exitCode':result.returncode});save()
        if result.returncode: raise RuntimeError(label+' failed; see preserved logs')
        return json.loads((out/label/'dot-gen.json').read_text())
    try:
        default = translate('default', ['--libc','musl'])
        assert default['providerPruning']['mode']=='reachable'
        assert default['libcProvider']['pruningPolicy']=='reachable'
        assert 'c32rtomb' not in (out/'default.raw.cs').read_text()
        report['checks'].append('musl defaults to reachable and omits unused c32rtomb')
        complete = translate('all', ['--libc','musl','--libc-pruning','all'])
        assert complete['providerPruning'] is None
        assert 'c32rtomb' in (out/'all.raw.cs').read_text()
        report['checks'].append('explicit all retains complete-provider c32rtomb definition')
        plain = translate('plain', [])
        assert plain['providerPruning'] is None
        report['checks'].append('ordinary translation without musl still works without extra options')
        packaged = translate('packaged', ['--libc','musl'], out/'default.raw.cs')
        assert packaged['providerPruning']==default['providerPruning']
        assert packaged['rawInput']['packagingOnly']
        report['checks'].append('raw packaging preserves original pruning metadata without applying it again')
        report['passed']=True
    finally:
        save()
    print('libc pruning defaults: '+str(len(report['checks']))+' checks passed')


if __name__=='__main__':
    main()
