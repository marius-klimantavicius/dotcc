"""Freeze authoritative native backend source selection without machine paths.

Run deliberately after a source/profile upgrade. This is metadata conversion,
not a build runner. Native exact argv remain in the framework source manifest.
"""
import argparse
import json
from pathlib import Path


def freeze(native, stage):
    stage = Path(stage).resolve()
    if not native['complete_backend_link_closure']:
        raise RuntimeError('Need a complete observed backend link closure')
    contexts = {}
    seen = set()
    for row in native['backend_units']:
        if row['language'] != 'c' or row['source'] in seen:
            raise RuntimeError('Non-C or duplicate backend translation input: ' + row['source'])
        seen.add(row['source'])
        source, cwd = stage / row['source'], stage / row['cwd']
        if not source.is_file() or not source.resolve().is_relative_to(stage):
            raise RuntimeError('Missing/unsafe observed source: ' + str(source))
        defines, includes, native_options = [], [], []
        args = iter(row['command'][1:])
        for arg in args:
            if arg.startswith(('-D', '-U')):
                defines.append(arg.replace(str(stage.parent), '{native_base}'))
            elif arg == '-I' or arg.startswith('-I'):
                value = next(args) if arg == '-I' else arg[2:]
                path = (cwd / value).resolve()
                if not path.is_relative_to(stage):
                    raise RuntimeError('Native include escapes configured stage: ' + str(path))
                includes.append('{cwd}' if path == cwd else path.relative_to(stage).as_posix())
            elif arg == '-o':
                next(args)
            elif arg not in ('-c', source.name) and not arg.endswith(('.c', '.S', '.s')):
                native_options.append(arg)
        key = json.dumps([defines, includes, native_options], sort_keys=True)
        context = contexts.setdefault(key, dict(defines=defines, includes=includes,
                                                native_options=native_options, units=[]))
        context['units'].append(row['source'])
    return dict(schema=1, revision=native['revision'], profile=native['profile'],
                provenance='observed native backend link objects and server archive members',
                backend_unit_count=len(seen), complete_backend_link_closure=True,
                complete_embedded_init_product_closure=False,
                contexts=list(contexts.values()),
                separate_units=['pglite/src/pglitec/pglitec.c', 'src/Host/dotcc_pglite_boundary.c'],
                omissions=['initdb frontend/init orchestration and its required data closure remain open'],
                native_options_policy='retained as evidence; DotCC uses declared c11 dialect and audited include/define selection, not native GCC codegen flags')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('native_manifest', type=Path)
    parser.add_argument('--stage', required=True, type=Path)
    parser.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1] / 'config/backend-units.json')
    args = parser.parse_args()
    report = freeze(json.loads(args.native_manifest.read_text()), args.stage)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('Frozen backend C units:', report['backend_unit_count'])


if __name__ == '__main__':
    main()
