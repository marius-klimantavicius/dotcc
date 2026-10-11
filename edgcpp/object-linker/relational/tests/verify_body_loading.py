#!/usr/bin/env python3
"""Exercise many function bodies without retaining them all during generation."""
import argparse
from pathlib import Path
import re
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work/'source'
    source.mkdir()
    sources = []
    for unit in range(4):
        path = source/f'unit{unit}.cpp'
        path.write_text('\n'.join(f'''int calculate_{i}(int input) {{
    int result = input;
    for (int index = 0; index < 20; ++index) {{
        if (index % 3 == 0) continue;
        result += index + {i};
    }}
    return result;
}}''' for i in range(unit*10, (unit+1)*10)) + '\n')
        sources.append(path)
    main_source = source/'main.cpp'
    expected = sum(1 + sum(index+i for index in range(20) if index%3) for i in range(40))
    main_source.write_text('\n'.join(f'int calculate_{i}(int);' for i in range(40)) +
        '\nint main() {\n    int result = 0;\n' +
        '\n'.join(f'    result += calculate_{i}(1);' for i in range(40)) +
        f'\n    return result == {expected} ? 0 : 1;\n}}\n')
    sources.append(main_source)
    run(['g++', '-std=c++20', *sources, '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    objects = []
    total_nodes = 0
    for path in sources:
        obj = work/f'{path.stem}.dotobj'
        run([sys.executable, RELATIONAL/'compile.py', path, '--output', obj,
             '--work-dir', work/f'{path.stem}-work', '--compiler', args.compiler.resolve()], work/f'{path.stem}-compile.log')
        with sqlite3.connect(obj.as_uri()+'?mode=ro', uri=True) as db:
            assert not list(db.execute('PRAGMA foreign_key_check'))
            total_nodes += db.execute('SELECT count(*) FROM nodes WHERE function_id IS NOT NULL').fetchone()[0]
        objects.append(obj)
    source.rename(work/'original-source')
    generated = work/'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated,
         '--namespace', 'Review.BodyLoading', '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    metrics = re.search(r'Body loading: peak rows (\d+); peak nodes (\d+); functions loaded (\d+); active bodies (\d+)',
                        (work/'link.log').read_text())
    assert metrics, 'Body-loading metrics missing'
    peak_rows, peak_nodes, loaded, active = map(int, metrics.groups())
    assert 0 < peak_nodes < total_nodes//3, (peak_nodes, total_nodes)
    assert loaded >= 41 and active == 0, (loaded, active)
    (work/'body-loading-summary.txt').write_text(f'Total function nodes: {total_nodes}\n'
        f'Peak body nodes: {peak_nodes}\nPeak body rows: {peak_rows}\nFunctions loaded: {loaded}\n'
        'This measures body retention only. Type/symbol catalogs are still materialized.\n')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print(f'PASS: {total_nodes} total function nodes, {peak_nodes} peak retained body nodes; native/source-hidden C#' +
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
