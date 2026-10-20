"""Campaign-level fail-closed selection and optional provenance controls."""
from copy import deepcopy
import json
from types import SimpleNamespace

from campaigns.identity import HashPolicy


def check_reports(ctx, primitives, variant, report, unit, source, names):
    rows = [json.loads(line) for line in report.read_text().splitlines() if line]
    selected = [index for index, row in enumerate(rows) if row.get('event') == 'function-override']
    if len(selected) != 42:
        raise RuntimeError('Control needs the actual complete selection report')
    observed = []
    for name in ('missing', 'extra', 'duplicate', 'signature', 'target', 'header', 'unit', 'origin', 'unmatched'):
        invalid = deepcopy(rows)
        first = invalid[selected[0]]
        if name == 'missing':
            del invalid[selected[0]]
        elif name == 'extra':
            invalid.append(dict(first, name='unexpected'))
        elif name == 'duplicate':
            invalid.append(dict(first))
        elif name in ('signature', 'target', 'origin'):
            first[name] = 'incorrect'
        elif name == 'header':
            first['declarationFile'] = str(source / 'lib/common/bits.h')
        elif name == 'unit':
            first['translationUnit'] = str(unit.parent / 'different.c')
        elif name == 'unmatched':
            invalid.append(dict(event='function-override-unmatched', name='unexpected'))
        path = variant.work / ('invalid-' + name + '.jsonl')
        path.write_text(''.join(json.dumps(row) + '\n' for row in invalid))
        try:
            primitives.validate_report(variant, path, unit, source, names)
        except RuntimeError:
            observed.append(dict(control=name, status='rejected'))
        else:
            raise RuntimeError('Invalid selection was accepted: ' + name)
    return observed


def policy_checks(ctx, primitives, source, test, common):
    """An unrelated header comment is advisory/off, never an ordinary allowlist."""
    header = source / 'lib/common/mem.h'
    original = header.read_bytes()
    records = []
    try:
        header.write_bytes(original + b'\n/* primitive provenance comment control */\n')
        for mode in ('off', 'warn', 'strict'):
            work = ctx.work / 'primitives' / ('policy-' + mode)
            work.mkdir()
            variant = SimpleNamespace(root=ctx.root, work=work, profile='portable', policy=HashPolicy(mode))
            try:
                selected = primitives.unit_flags(variant, test, source, primitives.all_names(ctx.root))
            except RuntimeError:
                if mode != 'strict':
                    raise
                records.append(dict(policy=mode, unrelated_comment='rejected by explicit hash policy'))
                continue
            if mode == 'strict':
                raise RuntimeError('Explicit strict policy accepted modified historical header')
            report = ctx.artifacts / ('primitives-policy-' + mode + '-overrides.jsonl')
            ctx.run(['dotnet', ctx.compiler, *common, *selected, '--override-report', report,
                     test, '--emit=obj', '-o', work / 'primitives.cs'], 'primitives-policy-' + mode + '-emit', timeout=600)
            primitives.validate_report(variant, report, test, source, primitives.all_names(ctx.root))
            records.append(dict(policy=mode, unrelated_comment='accepted', required_selections=42,
                                warnings=variant.policy.warnings))
    finally:
        header.write_bytes(original)
    return records
