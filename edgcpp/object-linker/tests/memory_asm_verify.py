#!/usr/bin/env python3
"""Independent-TU empty GNU memory assembly: owned operands and JIT/AOT parity."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, default=ROOT / 'compile.py')
    parser.add_argument('--allow-export-rejection', action='store_true')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs, tooling = out / 'inputs', out / 'tooling'
    inputs.mkdir()
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('memory_asm_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    fixtures, checks, commands, pending = [], [], [], []
    for name in ('memory_asm.h', 'memory_asm_provider.cpp', 'memory_asm_main.cpp'):
        shutil.copy2(ROOT / 'tests' / name, inputs / name)
        fixtures.append(dict(name=name, sha256=sha(inputs / name)))

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=0, cwd=out):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=cwd, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, cwd=str(cwd), exitCode=result.returncode))
        if expected is not None:
            check(name, result.returncode == expected)
        return result

    def compile_source(name, source, expected=0):
        obj = out / (name + '.dotobj')
        result = run(name + '-export', [sys.executable, '-B', tooling / 'compile.py', source,
            '--output', obj, '--work-dir', out / (name + '-work'),
            '--compiler', args.compiler.resolve(), '--std', 'c++17'], expected=expected)
        return obj, result

    failure = None
    try:
        sources = [inputs / ('memory_asm_' + name + '.cpp') for name in ('provider', 'main')]
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=gnu++17', '-O' + optimization,
                '-fno-exceptions', '-fno-rtti', *sources, '-o', binary])
            result = run('native-O' + optimization, [binary])
            check('native-silent-success-O' + optimization, not result.stdout and not result.stderr)
            oracle = result
        stock = []
        for source in sources:
            label = source.stem
            work = out / (label + '-stock')
            work.mkdir()
            run(label + '-stock-lowering', [args.stock_compiler.resolve(), '--c++17',
                '--gnu_version', '130300', '--clear_flag', 'use_predefined_macro_file',
                '--instantiate', 'used', '--no_exceptions', '--no_rtti', source], cwd=work)
            c_file = source.with_suffix('.int.c')
            if not c_file.exists():
                c_file = work / c_file.name
            check(label + '-stock-source-retained', c_file.exists())
            stock.append(c_file)
            fixtures.append(dict(name=str(c_file), sha256=sha(c_file),
                                 purpose='Stock C lowering reference; native C++ supplies the execution oracle'))
        # Stock lowering exposes the saved VLA dimension, but its input operand
        # has decayed to a pointer expression. Retain that evidence unchanged;
        # do not patch the C output to manufacture another execution oracle.
        objects = []
        for label, source in zip(('provider', 'main'), sources):
            obj, result = compile_source(label, source, None if args.allow_export_rejection else 0)
            if result.returncode:
                pending.append(dict(unit=label, diagnostic=(out / (label + '-work/stderr')).read_text()))
            else:
                run(label + '-verify', ['dotnet', args.linker.resolve(), 'verify', obj])
                objects.append(obj)
        if not pending:
            provider = objects[0]
            with sqlite3.connect(provider) as db:
                db.row_factory = sqlite3.Row
                barriers = list(db.execute('SELECT * FROM memory_barriers'))
                operands = list(db.execute('SELECT * FROM memory_barrier_operands'))
                dimensions = list(db.execute('SELECT * FROM memory_barrier_dimensions ORDER BY node_id,operand_ordinal,ordinal'))
                check('owned-barrier-count', len(barriers) == 12)
                check('owned-volatility-and-proof', sum(r['has_volatile_keyword'] for r in barriers) == 11 and
                      all(r['capture_proof'] == 'observed-empty-gnu-asm' for r in barriers))
                check('owned-directions', sorted(r['direction'] for r in operands) ==
                      sorted(['input'] * 5 + ['output'] * 2 + ['inout'] * 5))
                check('named-operands-retained', {'destination', 'update', 'source'} <= {r['source_name'] for r in operands})
                matrix = next(r for r in operands if r['source_shape'] == 'fixed-array')
                check('fixed-matrix-owned-extent', matrix['element_size'] == 2 and matrix['fixed_element_count'] == 6 and
                      [r['fixed_count'] for r in dimensions if r['node_id'] == matrix['node_id']] == [2, 3])
                variable = [r for r in operands if r['source_shape'] == 'vla-array']
                check('variable-extents-owned-once', len(variable) == 3 and
                      sorted(r['element_size'] for r in variable) == [1, 1, 2] and
                      all(r['extent_binding_id'] is not None and r['extent_kind'] == 'vla-total' for r in variable))
                check('operand-source-locations', all(r['source_file_id'] is not None and r['line'] > 0 for r in operands))
                for operand in operands:
                    edges = db.execute('SELECT role FROM edges WHERE parent_id=? AND ordinal=? AND role IN (?,?)',
                        (operand['node_id'], operand['ordinal'], 'address', 'extent')).fetchall()
                    check('operand-edges-' + str(operand['node_id']) + '-' + str(operand['ordinal']),
                          sorted(r['role'] for r in edges) == ['address', 'extent'])
                fixed_id, fixed_ordinal = matrix['node_id'], matrix['ordinal']
                extent_id = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='extent' AND ordinal=?",
                                       (fixed_id, fixed_ordinal)).fetchone()[0]
            inputs.rename(out / 'retained-source')
            generated = out / 'generated'
            run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', *objects])
            text = '\n'.join(path.read_text() for path in generated.glob('Translated.*.cs'))
            check('BCL-memory-barrier-emitted', text.count('global::System.Threading.Thread.MemoryBarrier()') == 12)
            run('build', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '--nologo', '-v:q'])
            result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            check('jit-native-parity', (result.stdout, result.stderr) == (oracle.stdout, oracle.stderr))
            if args.nativeaot:
                run('publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', args.rid,
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q'])
                result = run('nativeaot', [out / 'aot/Translated'])
                check('nativeaot-native-parity', (result.stdout, result.stderr) == (oracle.stdout, oracle.stderr))
            mutations = [
                ('wrong-element-size', 'UPDATE memory_barrier_operands SET element_size=4 WHERE node_id=? AND ordinal=?', (fixed_id, fixed_ordinal)),
                ('wrong-fixed-count', 'UPDATE memory_barrier_operands SET fixed_element_count=7 WHERE node_id=? AND ordinal=?', (fixed_id, fixed_ordinal)),
                ('wrong-fixed-dimension', 'UPDATE memory_barrier_dimensions SET fixed_count=9 WHERE node_id=? AND operand_ordinal=? AND ordinal=0', (fixed_id, fixed_ordinal)),
                ('wrong-extent-value', "UPDATE nodes SET constant_text='0' WHERE id=?", (extent_id,)),
                ('missing-extent-edge', "DELETE FROM edges WHERE parent_id=? AND role='extent' AND ordinal=?", (fixed_id, fixed_ordinal)),
            ]
            for label, sql, parameters in mutations:
                malformed = out / (label + '.dotobj')
                shutil.copy2(provider, malformed)
                with sqlite3.connect(malformed) as db:
                    db.execute(sql, parameters)
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                result = run('reject-' + label, ['dotnet', args.linker.resolve(), 'verify', malformed], expected=1)
                check('semantic-rejection-' + label, b'digest' not in result.stderr.lower())
            negative = {
                'nonempty': 'void probe() { __asm__ __volatile__("nop" ::: "memory"); }',
                'register-input': 'void probe(int value) { __asm__ __volatile__("" : : "r"(value)); }',
                'register-output': 'void probe(int *value) { __asm__ __volatile__("" : "=r"(*value)); }',
                # EDG deliberately discards "cc" with a warning in extasm.c;
                # use an actual retained clobber to test the export boundary.
                'unsupported-clobber': 'void probe() { __asm__ __volatile__("" ::: "rax"); }',
                'goto': 'void probe() { __asm__ goto("" ::: "memory" : done); done:; }',
                'rank-two-vla': 'void probe(char *p,int n,int m) { __asm__ __volatile__("" : : "m"(*(char (*)[n][m])p)); }',
            }
            for label, source in negative.items():
                path = out / ('negative-' + label + '.cpp')
                path.write_text(source + '\n')
                _, result = compile_source('negative-' + label, path, expected=1)
                diagnostic = (out / ('negative-' + label + '-work/stderr')).read_text()
                check('explicit-gate-' + label, 'unsupported dotobj' in diagnostic)
    except Exception as error:
        failure = str(error)
    status = 'failed' if failure else 'native-oracles-passed-export-pending' if pending else 'passed'
    result = dict(status=status, error=failure, checks=checks, commands=commands, fixtures=fixtures, pending=pending,
                  compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
                  linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)),
                  managedExecutionVerified=status == 'passed', nativeAotVerified=args.nativeaot and status == 'passed',
                  scope='Exact empty GNU memory-only asm; independent source TUs, owned addresses/extents, side effects and source-hidden linking. No arbitrary asm or invalid C/C++ lvalue claim.')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=status, checks=len(checks), pending=len(pending), error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
