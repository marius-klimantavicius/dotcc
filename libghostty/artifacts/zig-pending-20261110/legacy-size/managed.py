"""Run using the coordinator's already-built compiler; creates no compiler checkout/build."""
import argparse
import hashlib
import json
import os
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--profile', choices=['c', 'zig'], action='append')
parser.add_argument('--strict-native-zig-layout', action='store_true')
args = parser.parse_args()
E = Path(__file__).resolve().parent
W = Path('/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110/legacy-size')
env = dict(os.environ, TMPDIR=str(W / 'tmp'))
compiler = args.compiler.resolve()
receipt = dict(compiler=str(compiler), compiler_sha256=hashlib.sha256(compiler.read_bytes()).hexdigest(), stages=[])

def execute(profile, lane, stage, command, expected=0):
    result = subprocess.run(command, env=env, text=True, capture_output=True)
    receipt['stages'].append(dict(profile=profile, lane=lane, stage=stage, command=command,
                                 exit=result.returncode, stdout=result.stdout, stderr=result.stderr))
    (E / ('zig-native-layout-failure.json' if args.strict_native_zig_layout else 'managed.json')).write_text(json.dumps(receipt, indent=2) + '\n')
    assert result.returncode == expected, receipt['stages'][-1]
    print(profile, lane, stage, result.returncode, flush=True)

for profile in args.profile or ['c', 'zig']:
    strict_zig = profile == 'zig' and args.strict_native_zig_layout
    scope = 'zig-native-layout' if strict_zig else profile
    proof = W / 'source' / ('managed-zig-behavior.zig' if profile == 'zig' and not strict_zig else 'managed-proof.zig')
    graph = W / ('zig-behavior-graph.json' if profile == 'zig' and not strict_zig else profile + '-graph.json')
    source = W / (scope + '-source')
    execute(profile, 'source', 'emit', ['dotnet', str(compiler), str(proof),
            '--zig-modules-file', str(graph), '--emit=managedlib', '-o', str(source)])
    obj = W / (scope + '.csobj')
    execute(profile, 'object', 'emit', ['dotnet', str(compiler), str(proof),
            '--zig-modules-file', str(graph), '--emit=obj', '-o', str(obj)])
    linked = W / (scope + '-object')
    execute(profile, 'object', 'link', ['dotnet', str(compiler), str(obj), '--emit=managedlib', '-o', str(linked)])
    for lane, project in [('source', source), ('object', linked)]:
        # Add a host around the unchanged translated answer, bypassing std/start.
        (project / 'ProofHost.cs').write_bytes((E / 'ProofHost.cs').read_bytes())
        project_file = project / (project.name + '.csproj')
        project_file.write_text(project_file.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>'))
        build = W / ('build-' + scope + '-' + lane)
        execute(profile, lane, 'build', ['dotnet', 'build', str(project / (project.name + '.csproj')),
                '-c', 'Release', '--nologo', '-v:q', '-p:PublishAot=false',
                '-p:UseArtifactsOutput=true', '-p:ArtifactsPath=' + str(build)])
        execute(profile, lane, 'run', ['dotnet', str(build / 'bin' / project.name / 'release' / (project.name + '.dll'))], 15 if strict_zig else 42)
print('Strict non-C native layout limitation reproduced in source/object (exit15).' if args.strict_native_zig_layout else 'C native layout plus C/Zig original/patched Writer behavior PASS in source/object (11,700 encodings).')
