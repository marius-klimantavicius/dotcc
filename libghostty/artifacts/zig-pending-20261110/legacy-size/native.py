"""Compare actual legacy original/patched Size and Writer behavior in four native lanes."""
import hashlib
import json
import os
import subprocess
from pathlib import Path

E = Path(__file__).resolve().parent
W = Path('/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110/legacy-size')
Z = Path('/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/tools/zig-x86_64-linux-0.17.0')
subprocess.run(['python3', str(E / 'prepare.py')], check=True)
records = []
env = dict(os.environ, TMPDIR=str(W / 'tmp'))
for profile in ['c', 'zig']:
    for mode in ['Debug', 'ReleaseFast']:
        binary = W / ('native-' + profile + '-' + mode)
        command = [str(Z / 'zig'), 'build-exe', '-O', mode, '--cache-dir', str(W / 'cache'),
                   '--global-cache-dir', str(W / 'global'), '--dep', 'terminal_options', '--dep', 'proof_options',
                   '-Mroot=' + str(W / 'source/proof.zig'),
                   '-Mterminal_options=' + str(W / (profile + '-options.zig')),
                   '-Mproof_options=' + str(W / 'native-options.zig'), '-femit-bin=' + str(binary)]
        for stage, args, expected in [('build', command, 0), ('run', [str(binary)], 42)]:
            result = subprocess.run(args, env=env, text=True, capture_output=True)
            records.append(dict(profile=profile, mode=mode, stage=stage, command=args,
                                exit=result.returncode, stdout=result.stdout, stderr=result.stderr))
            (E / 'native.json').write_text(json.dumps(records, indent=2) + '\n')
            assert result.returncode == expected, records[-1]
            if stage == 'run':
                assert 'comparisons=2925 hash=3517276540470399028' in result.stderr
                print(profile, mode, result.stderr.strip(), flush=True)
print('C/Zig x Debug/ReleaseFast native actual Writer/layout comparison PASS (11,700 encodings).')
