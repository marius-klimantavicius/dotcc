"""Exercise the semantic address analyzer with independent authored C# controls.

These are deliberately valid/invalid analyzer inputs, never repairs to generated
CPython. Runtime evaluation and GC controls are separate native/emitted tests.
"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[3]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--analyzer', required=True, type=Path)
parser.add_argument('--work', required=True, type=Path)
parser.add_argument('--artifacts', required=True, type=Path)
args = parser.parse_args()
args.work.mkdir()
args.artifacts.mkdir()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
commands = []
shared = ['flock', '--close', '/tmp/dotcc-cpython-shared-build.lock']

def run(label, command, expected):
    command = [str(part) for part in command]
    result = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    log = args.artifacts/(label+'.log')
    log.write_bytes(result.stdout)
    commands.append({'label': label, 'argv': command, 'cwd': str(ROOT), 'exit': result.returncode,
                     'log': str(log), 'log_sha256': sha(log)})
    (args.artifacts/'commands.json').write_text(json.dumps(commands, indent=2)+'\n')
    assert result.returncode == expected, (label, result.returncode, result.stdout[-4000:])

helper_source = ROOT/'DotCC.Lib/GlobalAddressCache.cs'
# The exact current compiler-owned mechanism is part of the control input.
helper = helper_source.read_text().split('"""', 2)[1]
helper = '\n'.join(line[8:] if line.startswith('        ') else line for line in helper.splitlines())
base = '''using System;
using System.Runtime.CompilerServices;
public struct CGlobals { public int Counter; }
public sealed class ManagedOwner { public int Counter; }
public static unsafe class Control {
    public static CGlobals Globals;
    public static ManagedOwner Current = new();
    public static int Other;
    static void Call() { }
BODY
}
HELPER
'''
direct = '(int*)Unsafe.AsPointer(ref Globals.Counter)'
cache = '''__DotccGlobalAddressCache __dotcc_address0 = default;
static ref int __dotcc_address0_Get() => ref Globals.Counter;
'''
use = '__dotcc_address0.Get<int>(&__dotcc_address0_Get)'
cases = [
    ('repeated-direct', f'static int Read() {{ return *{direct} + *{direct}; }}', 1, 0, 1, 0),
    ('loop-direct', f'static int Read() {{ int n=0; for(int i=0;i<2;i++) n+=*{direct}; return n; }}', 1, 0, 1, 0),
    ('separate-regions', f'static int A() => *{direct}; static int B() => *{direct};', 0, 0, 0, 0),
    ('lazy-call-branch-label', f'''static int Read(bool take) {{ {cache}
int n=0; if(take) n=*{use}; Call(); if(!take) goto after; n+=*{use};
after: return n+*{use}; }}''', 0, 0, 0, 1),
    ('duplicate-caches', f'''static int Read() {{ {cache}
__DotccGlobalAddressCache __dotcc_address1 = default;
static ref int __dotcc_address1_Get() => ref Globals.Counter;
return *{use} + *__dotcc_address1.Get<int>(&__dotcc_address1_Get); }}''', 1, 0, 1, 2),
    ('unbound-cache-owner', f'''static int Read() {{ {cache.replace('Globals.Counter', 'Other')}
return *{use}; }}''', 1, 1, 0, 0),
    ('reacquiring-helper', f'static int Read() {{ {cache} return *{use} + *{use}; }}', 1, 1, 0, 1),
    ('changed-managed-owner', '''static int Read() {
int n=*(int*)Unsafe.AsPointer(ref Current.Counter);
Current=new ManagedOwner(); return n+*(int*)Unsafe.AsPointer(ref Current.Counter);
}''', 0, 0, 0, 0),
]
results = []
for name, body, exit_code, unknown, unresolved, caches in cases:
    case = args.work/name
    case.mkdir()
    implementation = helper
    if name == 'reacquiring-helper':
        implementation = helper.replace('if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value))', '')
    source = case/'Control.cs'
    source.write_text(base.replace('BODY', body).replace('HELPER', implementation))
    project = case/'Control.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<EnableDefaultCompileItems>false</EnableDefaultCompileItems><ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup><ItemGroup><Compile Include="Control.cs" /></ItemGroup></Project>''')
    receipt = case/'receipt.json'
    receipt.write_text(json.dumps({'run_id': 'authored-address-analyzer-'+name})+'\n')
    inventory = case/'inventory.json'
    inventory.write_text(json.dumps({'translationReceipt': str(receipt), 'receiptSha256': sha(receipt),
        'source': str(case), 'files': [{'path': str(source), 'sha256': sha(source)}]}, indent=2)+'\n')
    run(name+'-build', shared+['dotnet', 'build', project, '-c', 'Release', '--disable-build-servers', '-m:1',
        '--artifacts-path', case/'products', '-o', case/'pe'], 0)
    report = args.artifacts/(name+'.json')
    run(name+'-analyze', shared+['dotnet', args.analyzer, inventory, case/'pe/Control.dll', report], exit_code)
    actual = json.loads(report.read_text())
    assert actual['semantic_errors'] == [], (name, actual['semantic_errors'])
    assert len(actual['unknown_cache_bindings']) == unknown, (name, actual['unknown_cache_bindings'])
    assert len(actual['unresolved_pure_sites']) == unresolved, (name, actual['unresolved_pure_sites'])
    assert len(actual['cache_bindings_and_uses']) == caches, (name, actual['cache_bindings_and_uses'])
    if name == 'changed-managed-owner':
        dynamic = [row for row in actual['remaining_address_conversions'] if 'Current.Counter' in row['expression']]
        assert len(dynamic) == 2 and all(row['bound_symbol']['owner_expression']=='Current' for row in dynamic)
        assert all(any(ref['facts']['symbol']=='Current' for ref in row['bound_references']) for row in dynamic)
    results.append({'name': name, 'status': 'passed expected analyzer outcome', 'expectedExit': exit_code,
        'unknown': unknown, 'unresolved': unresolved, 'caches': caches,
        'report': {'path': str(report), 'sha256': sha(report)},
        'source': {'path': str(source), 'sha256': sha(source)},
        'project': {'path': str(project), 'sha256': sha(project)},
        'assembly': {'path': str(case/'pe/Control.dll'), 'sha256': sha(case/'pe/Control.dll')}})
report = {'status': 'passed', 'scope': 'Eight authored semantic-analyzer positive/negative controls; no CPython or runtime acceptance',
    'analyzer': {'path': str(args.analyzer), 'sha256': sha(args.analyzer)},
    'compilerMechanismSource': {'path': str(helper_source), 'sha256': sha(helper_source)},
    'driver': {'path': str(Path(__file__)), 'sha256': sha(Path(__file__))}, 'cases': results}
(args.artifacts/'comparison.json').write_text(json.dumps(report, indent=2)+'\n')
print(json.dumps({'status': 'passed', 'controls': len(results)}))
