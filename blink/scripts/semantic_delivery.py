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
    if spec['version'] != 1 or spec['header'] != 'blink/endian.h':
        raise RuntimeError('Semantic specification does not identify the endian header')
    pin(header, spec['header_sha256'])
    expected = {prefix + str(bits): 'intrinsic:' + operation + '.u' + str(bits) + '.le'
                for bits in (16, 32, 64) for prefix, operation in [('Get', 'load'), ('Put', 'store')]}
    rules = spec['functionOverrides']
    if (len(rules) != 6 or {row['name']: row['target']['kind'] + ':' + row['target']['name'] for row in rules} != expected
            or any(row['linkage'] != 'internal' for row in rules)):
        raise RuntimeError('Semantic specification target set or linkage differs')
    overrides = pin(profile / 'overrides.json', inputs['staged_headers']['overrides.json'])
    all_bound = json.loads(overrides.read_text())['functionOverrides']
    boundary_names = pin_managed_boundaries(delivery, assembly, pin, inputs, all_bound)
    bound = [rule for rule in all_bound if rule['name'] in expected]
    if len(all_bound) != 6 + len(boundary_names):
        raise RuntimeError('Unreviewed semantic rule group')
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
        selected = [event for event in events if event.get('event') == 'function-override' and event.get('name') in expected]
        unmatched = [event['name'] for event in events if event.get('event') == 'function-override-unmatched' and event.get('name') in expected]
        if any(event.get('event') in ('function-override', 'function-override-unmatched') and event.get('name') not in set(expected) | boundary_names for event in events):
            raise RuntimeError('Unknown typed semantic event: ' + source)
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


def pin_managed_boundaries(delivery, assembly, pin, inputs, all_bound):
    """Verify whole-function owner handoffs independently of endian intrinsics."""
    profile = Path(delivery['profile'])
    specification = profile / 'managed-boundaries.json'
    summary = assembly.get('managed_boundaries')
    if not specification.exists():
        if summary or delivery.get('managed_boundaries'):
            raise RuntimeError('Managed boundary summary has no specification')
        return set()  # Historical endian-only product.
    if not summary or summary != delivery.get('managed_boundaries'):
        raise RuntimeError('Managed boundary delivery and assembly summaries differ')
    pin(specification, summary['specification_sha256'])
    if inputs['staged_headers'].get('managed-boundaries.json') != summary['specification_sha256']:
        raise RuntimeError('Managed boundary specification differs from profile identity')
    spec = json.loads(specification.read_text())
    root = Path(__file__).resolve().parents[1]
    upstream = root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    targets = {
        'SignalActor': ('blink_host_guest_signal_actor', 'blink/machine.h', 'void(named:Machine*)', False),
        'KillOtherThreads': ('blink_host_guest_stop_other_threads', 'blink/machine.h', 'void(named:System*)', False),
        'SysExitGroup': ('blink_host_guest_group_exit', 'blink/syscall.h', 'void(named:Machine*,int)', True),
        'SysExit': ('blink_host_guest_exit', 'blink/syscall.h', 'void(named:Machine*,int)', True),
    }
    # Historical deliveries retain their four original owner handoffs. New
    # deliveries select both members of the reviewed page-table override pair.
    page_table = {'TrackHostPage', 'FindHostPage'} <= {rule['name'] for rule in spec['functionOverrides']}
    if page_table:
        targets['TrackHostPage'] = ('blink_host_track_page', None, 'unsigned long(unsigned char*)', False)
        targets['FindHostPage'] = ('blink_host_find_page', 'blink/machine.h', 'unsigned char*(unsigned long)', False)
    instance = delivery.get('product_surface', {}).get('instance_abi') == 'instance-v1'
    if instance:
        marker = pin(profile / 'instance-abi.json', inputs['staged_headers']['instance-abi.json'])
        if json.loads(marker.read_text()) != {'abi': 'instance-v1'}:
            raise RuntimeError('Instance ABI marker differs')
        targets['blink_host_guest_pthread_atfork'] = ('blink_host_guest_pthread_atfork', None, 'int(void(),void(),void())', False)
        for name, signature in {
            'blink_host_guest_thread_start': 'int(named:Machine*)',
            'blink_host_guest_signal_checkpoint': 'void(named:Machine*)',
            'blink_host_guest_signal_wake': 'int(named:Machine*)',
            'blink_host_guest_signal_enqueue_info': 'void(named:Machine*,int,int,unsigned int)',
            'blink_host_guest_signal_deliver_tkill': 'void(named:Machine*,int,int,unsigned int)',
            'blink_host_guest_signal_apply_info': 'void(named:Machine*,int,named:siginfo_linux*)',
        }.items():
            targets[name] = (name, None, signature, False)
        targets['TerminateSignal'] = ('TerminateSignal', 'blink/signal.h', 'void(named:Machine*,int,int)', False)
        authored = spec.get('authored_headers', {})
        if set(authored) != {'host-guest-threads.h'}:
            raise RuntimeError('Authored callback header set differs')
        pin(root / 'src/Host/include/host-guest-threads.h', authored['host-guest-threads.h'])
    rules = {rule['name']: rule for rule in spec['functionOverrides']}
    if spec['version'] != 1 or len(spec['functionOverrides']) != len(targets) or set(rules) != set(targets):
        raise RuntimeError('Managed boundary target set differs')
    if set(spec['headers']) != ({'blink/machine.h', 'blink/syscall.h', 'blink/signal.h'} if instance else {'blink/machine.h', 'blink/syscall.h'}) or set(spec['implementations']) != {'blink/syscall.c', 'blink/memorymalloc.c'}:
        raise RuntimeError('Managed boundary pinned source set differs')
    for name, digest in {**spec['headers'], **spec['implementations']}.items():
        pin(upstream / name, digest)
    bound = {rule['name']: rule for rule in all_bound if rule['name'] in rules}
    if set(bound) != set(rules):
        raise RuntimeError('Managed boundary physical selectors missing')
    for name, rule in rules.items():
        method, header, _, terminal = targets[name]
        target = {'kind': 'managedMethod', 'method': 'global::Managed.Emulation.BlinkCore.' + method}
        if terminal:
            target['doesNotReturn'] = True
        if instance:
            target['passInstance'] = True
        linkage = 'internal' if name in ('TrackHostPage', 'FindHostPage') else 'external'
        if (rule['target'] != target or rule.get('declarationFile') != header or rule['linkage'] != linkage
                or rule.get('requireMatch', False) or rule.get('translationUnit') is not None
                or bound[name] != (dict(rule, declarationFile=str(upstream / header)) if header else rule)):
            raise RuntimeError('Managed boundary rule contract differs: ' + name)
    objects, coverage = assembly['objects'], summary['coverage']
    if len(objects) != 108 or set(coverage) != set(objects):
        raise RuntimeError('Managed boundary producer coverage differs')
    required = spec['required_units']
    expected_required = {'blink/syscall.c': ['SignalActor', 'SysExitGroup', 'SysExit'], 'blink/memorymalloc.c': ['KillOtherThreads']}
    if page_table:
        expected_required['blink/memorymalloc.c'] += ['TrackHostPage', 'FindHostPage']
    if instance:
        expected_required['blink/signal.c'] = ['TerminateSignal']
    if required != expected_required:
        raise RuntimeError('Managed boundary required producers differ')
    selected_count = 0
    for source, row in objects.items():
        observed = coverage[source]
        if row.get('managed_boundaries') != observed or observed['specification_sha256'] != summary['specification_sha256']:
            raise RuntimeError('Per-object managed boundary summary differs: ' + source)
        report = pin(observed['report'], observed['report_sha256'])
        events = [json.loads(line) for line in report.read_text().splitlines() if line.strip()]
        selected = [event for event in events if event.get('event') == 'function-override' and event.get('name') in targets]
        unmatched = [event['name'] for event in events if event.get('event') == 'function-override-unmatched' and event.get('name') in targets]
        names = [event['name'] for event in selected]
        if (observed['selected'] != selected or observed['unmatched'] != unmatched or observed['absent'] is not (not selected)
                or len(names + unmatched) != len(targets) or set(names + unmatched) != set(targets)
                or not set(required.get(source, [])).issubset(names)):
            raise RuntimeError('Managed boundary typed selection differs: ' + source)
        canonical = pin(row['canonical_source'], row['source_sha256'])
        selected_count += bool(selected)
        for event in selected:
            method, header, signature, terminal = targets[event['name']]
            source_defined = event['name'] == 'TrackHostPage'
            if source_defined:
                if source != 'blink/memorymalloc.c' or Path(event['declarationFile']) != canonical:
                    raise RuntimeError('Page-table source selector differs: ' + source)
            elif header is None:
                command = row['command']
                actual_header = Path(command[command.index('-I') + 1]) / 'authored/host-guest-threads.h'
                if Path(event['declarationFile']) != actual_header:
                    raise RuntimeError('Authored callback physical selector differs: ' + source)
                pin(actual_header, spec['authored_headers']['host-guest-threads.h'])
            if (event['matches'] != '1' or event['signature'] != signature
                    or event['target'] != 'managedMethod:global::Managed.Emulation.BlinkCore.' + method
                    or event.get('doesNotReturn', 'false') != str(terminal).lower()
                    or (header is not None and event['declarationFile'] != str(upstream / header))
                    or (not source_defined and header is None and Path(event['declarationFile']).name != 'host-guest-threads.h')
                    or event.get('passInstance', 'false') != str(instance).lower()
                    or event['translationUnit'] != str(canonical)):
                raise RuntimeError('Managed boundary typed provenance differs: ' + source)
    if summary['selected_units'] != selected_count or summary['absent_units'] != 108 - selected_count:
        raise RuntimeError('Managed boundary selected/absent counts differ')
    return set(targets)
