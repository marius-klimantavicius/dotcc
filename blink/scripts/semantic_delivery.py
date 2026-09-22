"""Pin the actual typed endian selection evidence consumed by P5 runners."""
import json
from pathlib import Path


def pin_semantic_delivery(delivery, assembly, pin):
    """Add every checked file to the caller's ordinary pre/post hash closure."""
    root = Path(__file__).resolve().parents[1]
    pin(__file__)
    profile = Path(delivery['profile'])
    summary = assembly.get('semantic_intrinsics')
    if not summary or delivery.get('semantic_intrinsics') != summary:
        raise RuntimeError('Public delivery and assembly semantic summaries differ or are absent')
    inputs_path = pin(profile / 'inputs.json', delivery['profile_inputs_sha256'])
    inputs = json.loads(inputs_path.read_text())
    if (delivery.get('selected_profile') != 'threaded' or
            assembly['identity']['profile_inputs_sha256'] != delivery['profile_inputs_sha256'] or
            assembly['identity']['compiler_sha256'] != delivery['compiler'] or
            inputs['compiler'] != delivery['compiler']):
        raise RuntimeError('Semantic producer profile/compiler identities differ')
    specification = pin(profile / 'semantic-intrinsics.json', summary['specification_sha256'])
    if inputs['staged_headers'].get('semantic-intrinsics.json') != summary['specification_sha256']:
        raise RuntimeError('Semantic specification differs from staged profile identity')
    spec = json.loads(specification.read_text())
    header = root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/endian.h'
    header_sha = '630c0a03219a13ef0382cb77f30ebe9b9a65c41bdf76621311e3720e91384e8c'
    if spec['version'] != 1 or spec['header'] != 'blink/endian.h' or spec['header_sha256'] != header_sha:
        raise RuntimeError('Semantic specification does not identify the pinned upstream endian header')
    pin(header, header_sha)
    expected = {prefix + str(bits): 'intrinsic:' + operation + '.u' + str(bits) + '.le'
                for bits in (16, 32, 64) for prefix, operation in [('Get', 'load'), ('Put', 'store')]}
    rules = spec['functionOverrides']
    if (len(rules) != 6 or {row['name']: row['target']['kind'] + ':' + row['target']['name'] for row in rules} != expected
            or any(row['linkage'] != 'internal' for row in rules)):
        raise RuntimeError('Semantic specification target set or linkage differs')
    overrides = pin(profile / 'overrides.json', inputs['staged_headers']['overrides.json'])
    bound = json.loads(overrides.read_text())['functionOverrides']
    if len(bound) != 6 or {row['name'] for row in bound} != set(expected):
        raise RuntimeError('Semantic physical selectors differ')
    for rule in bound:
        original = next(row for row in rules if row['name'] == rule['name'])
        if rule != dict(original, declarationFile=str(header)):
            raise RuntimeError('Semantic rule differs from its pinned physical declaration selector')
    objects, coverage = assembly['objects'], summary['coverage']
    if len(objects) != 108 or set(coverage) != set(objects):
        raise RuntimeError('Semantic coverage does not contain exactly the 108 producer objects')
    required = set(spec['required_units'])
    if not {'blink/machine.c', 'blink/syscall.c', 'blink/loader.c', 'blink/ssefloat.c'} <= required <= set(objects):
        raise RuntimeError('Required core semantic units differ')
    selected_count = 0
    types = {16: 'unsigned short', 32: 'unsigned int', 64: 'unsigned long'}
    for source, row in objects.items():
        observed = coverage[source]
        if (row.get('semantic_intrinsics') != observed or
                observed['specification_sha256'] != summary['specification_sha256']):
            raise RuntimeError('Per-object semantic evidence differs: ' + source)
        report = pin(observed['report'], observed['report_sha256'])
        events = [json.loads(line) for line in report.read_text().splitlines() if line.strip()]
        selected = [event for event in events if event.get('event') == 'function-override']
        unmatched = [event['name'] for event in events if event.get('event') == 'function-override-unmatched']
        if selected != observed['selected'] or observed['absent'] is not (not selected):
            raise RuntimeError('Original typed report differs from semantic summary: ' + source)
        canonical = pin(row['canonical_source'], row['source_sha256'])
        if not selected:
            if source in required or len(unmatched) != 6 or set(unmatched) != set(expected):
                raise RuntimeError('Missing explicit absent-unit semantic evidence: ' + source)
            continue
        selected_count += 1
        if unmatched or len(selected) != 6 or {event['name']: event['target'] for event in selected} != expected:
            raise RuntimeError('Expected all six typed endian selections: ' + source)
        for event in selected:
            bits = int(event['name'][3:])
            signature = ('void(unsigned char*,' + types[bits] + ')' if event['name'].startswith('Put')
                         else types[bits] + '([Const]unsigned char*)')
            if (event['matches'] != '1' or event['signature'] != signature or
                    event['declarationFile'] != str(header) or event['translationUnit'] != str(canonical)):
                raise RuntimeError('Typed endian declaration provenance differs: ' + source)
    if summary['selected_units'] != selected_count or summary['absent_units'] != 108 - selected_count:
        raise RuntimeError('Semantic selected/absent unit counts differ')
    return summary
