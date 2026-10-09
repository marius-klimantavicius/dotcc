"""Generate independent integer-math oracles for every wide overflow type triple."""
import argparse
import itertools
import json
from pathlib import Path
import sqlite3
import subprocess

TYPES = [(('i' if signed else 'u') + str(bits), bits, signed)
         for bits in (8, 16, 32, 64, 128) for signed in (True, False)]
SPELLINGS = ['signed char', 'unsigned char', 'short', 'unsigned short', 'int',
             'unsigned int', 'long long', 'unsigned long long', '__int128', 'unsigned __int128']


def limits(type_):
    _, bits, signed = type_
    return (-(1 << (bits - 1)), (1 << (bits - 1)) - 1) if signed else (0, (1 << bits) - 1)


def literal(value, type_):
    name, bits, _ = type_
    pattern = value % (1 << bits)
    if bits == 128:
        return '(%s)(((u128)0x%xULL << 64) | (u128)0x%xULL)' % (name, pattern >> 64, pattern & ((1 << 64) - 1))
    return '(%s)0x%xULL' % (name, pattern)


def expected(a, b, operation, result_type):
    value = {'add': lambda: a + b, 'sub': lambda: a - b, 'mul': lambda: a * b}[operation]()
    low, high = limits(result_type)
    pattern = value % (1 << result_type[1])
    if result_type[2] and pattern >= (1 << (result_type[1] - 1)):
        pattern -= 1 << result_type[1]
    return int(value < low or value > high), pattern


def generate(directory):
    directory = Path(directory)
    directory.mkdir(parents=True, exist_ok=True)
    lines = ['// Generated from wide_overflow_verify.py; expectations use Python integer arithmetic.']
    lines += ['typedef %s %s;' % (spelling, type_[0]) for spelling, type_ in zip(SPELLINGS, TYPES)]
    lines += ['extern "C" {', 'int wide_overflow_failure;']
    functions, cases = [], []
    for a_type, b_type, r_type in itertools.product(TYPES, repeat=3):
        if max(a_type[1], b_type[1], r_type[1]) != 128:
            continue
        amin, amax = limits(a_type)
        bmin, bmax = limits(b_type)
        samples = list(dict.fromkeys([(amin, bmin), (amax, bmax), (amin, bmax), (amax, bmin),
                                     (0, bmax), (1, bmax), (amax, 1), (amax // 2 + 1, 2),
                                     (-1 if a_type[2] else amax, 2)]))
        for operation in ('add', 'sub', 'mul'):
            key = '_'.join([operation, a_type[0], b_type[0], r_type[0]])
            name = 'wide_' + key
            lines += ['int %s(%s left, %s right, %s *result) {' % (name, a_type[0], b_type[0], r_type[0]),
                      '    return __builtin_%s_overflow(left, right, result);' % operation, '}']
            group = 'check_' + key
            functions.append(group)
            lines += ['static int %s() {' % group, '    %s result;' % r_type[0]]
            for a, b in samples:
                overflow, result = expected(a, b, operation, r_type)
                ordinal = len(cases) + 1
                cases.append(dict(id=ordinal, function=name, left=str(a), right=str(b),
                                  overflow=bool(overflow), stored=str(result)))
                lines += ['    wide_overflow_failure = %d;' % ordinal,
                          '    if (%s(%s, %s, &result) != %d || result != %s) return 1;' %
                          (name, literal(a, a_type), literal(b, b_type), overflow, literal(result, r_type))]
            lines += ['    return 0;', '}']
    ordinary_wrappers = len(functions)
    # Real volatile stores at every supported narrow destination width/sign.
    for r_type in TYPES[:-2]:
        for operation in ('add', 'sub', 'mul'):
            name = 'wide_volatile_' + operation + '_' + r_type[0]
            a, b = (1 << 128) - 1, -1
            overflow, result = expected(a, b, operation, r_type)
            ordinal = len(cases) + 1
            cases.append(dict(id=ordinal, function=name, left=str(a), right=str(b), overflow=bool(overflow), stored=str(result)))
            lines += ['int %s(u128 left, i128 right, volatile %s *result) {' % (name, r_type[0]),
                      '    return __builtin_%s_overflow(left, right, result);' % operation, '}',
                      'static int check_%s() {' % name, '    volatile %s result = 0;' % r_type[0],
                      '    wide_overflow_failure = %d;' % ordinal,
                      '    return %s(%s, %s, &result) != %d || result != %s;' %
                      (name, literal(a, TYPES[-1]), literal(b, TYPES[-2]), overflow, literal(result, r_type)), '}']
            functions.append('check_' + name)
    side_effect_id = len(cases) + 1
    cases.append(dict(id=side_effect_id, function='check_side_effects', description='all three argument expressions once, signed128/unsigned128 multiply to volatile i16, aliased u128 destination'))
    lines += ['static int left_calls, right_calls, destination_calls;',
              'static volatile i16 side_result;',
              'static i128 side_left() { ++left_calls; return -3; }',
              'static u128 side_right() { ++right_calls; return 7; }',
              'static volatile i16 *side_destination() { ++destination_calls; return &side_result; }',
              'static int check_side_effects() {', '    wide_overflow_failure = %d;' % side_effect_id,
              '    left_calls = right_calls = destination_calls = 0;',
              '    int overflow = __builtin_mul_overflow(side_left(), side_right(), side_destination());',
              '    if (overflow || side_result != -21 || left_calls != 1 || right_calls != 1 || destination_calls != 1) return 1;',
              '    u128 alias = ~(u128)0;',
              '    overflow = __builtin_mul_overflow(alias, (u128)2, &alias);',
              '    return !overflow || alias != (~(u128)0) - 1;', '}']
    functions.append('check_side_effects')
    lines += ['int wide_overflow_test() {']
    lines += ['    if (%s()) return wide_overflow_failure;' % function for function in functions]
    lines += ['    wide_overflow_failure = 0;', '    return 0;', '}',
              'int main() { return wide_overflow_test() != 0; }', '}']
    source = directory / 'wide_overflow.cpp'
    source.write_text('\n'.join(lines) + '\n')
    receipt = dict(typeTriples=ordinary_wrappers // 3, ordinaryWrappers=ordinary_wrappers,
                   volatileWrappers=24, testCases=len(cases), cases=cases,
                   failureDiagnostic='wide_overflow_test() returns first failing case ID; exported wide_overflow_failure retains it')
    (directory / 'expected.json').write_text(json.dumps(receipt, indent=2) + '\n')
    return source, receipt


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    source, oracle = generate(out / 'wide-overflow-source')
    for optimization in ('0', '2'):
        binary = out / ('wide-overflow-native-O' + optimization)
        run('wide-overflow-native-build-O' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary])
        run('wide-overflow-native-run-O' + optimization, [binary])
    obj = compile_source('wide-overflow', source, extra=['--no-exceptions', '--no-rtti'])
    run('wide-overflow-verify', ['dotnet', tool, 'verify', obj])
    with sqlite3.connect(obj) as db:
        count = db.execute("SELECT count(*) FROM nodes WHERE kind='intrinsic' AND operator IN ('add-overflow','sub-overflow','mul-overflow')").fetchone()[0]
        check('wide-overflow-all-type-triples-owned', count >= oracle['ordinaryWrappers'])
        count = db.execute("SELECT count(*) FROM nodes WHERE kind='intrinsic' AND operator IN ('add-overflow-volatile','sub-overflow-volatile','mul-overflow-volatile')").fetchone()[0]
        check('wide-overflow-volatile-narrow-stores-owned', count >= 24)
    source.parent.rename(out / 'retained-wide-overflow-source')
    generated = link('link-wide-overflow-source-hidden', [obj])
    execute('wide-overflow', generated)
    return obj


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--generate-only', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    source, oracle = generate(out / 'source')
    commands = []
    if not args.generate_only:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            for label, command in [('build', ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary]), ('run', [binary])]:
                result = subprocess.run(list(map(str, command)), cwd=out, capture_output=True, timeout=300)
                (out / (label + '-O' + optimization + '.stdout')).write_bytes(result.stdout)
                (out / (label + '-O' + optimization + '.stderr')).write_bytes(result.stderr)
                commands.append(dict(command=list(map(str, command)), exitCode=result.returncode))
                if result.returncode:
                    break
            if commands[-1]['exitCode']:
                break
    receipt = {key: value for key, value in oracle.items() if key != 'cases'}
    receipt.update(status='generated' if args.generate_only else 'passed' if all(x['exitCode'] == 0 for x in commands) else 'failed', commands=commands)
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: receipt[key] for key in ('status', 'typeTriples', 'testCases')}))
    return 1 if receipt['status'] == 'failed' else 0


if __name__ == '__main__':
    raise SystemExit(main())
