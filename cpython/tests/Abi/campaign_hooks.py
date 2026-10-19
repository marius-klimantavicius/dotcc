"""Real-product ABI gate, ready for the A-owned recipe's abi hook.

Consumes existing canonical artifacts and the existing exact-pin native oracle.
Compiles only authored probes/managed consumers; never emits the interpreter,
builds a native interpreter, publishes a campaign product, or normalizes skips.
"""
import json
from pathlib import Path

NATIVE_LOCK = '/tmp/dotcc-cpython-native-build.lock'


def native_controls(ctx, suite):
    if '_cpython_abi_native' in ctx.__dict__:
        return ctx.__dict__['_cpython_abi_native']
    from campaigns.identity import digest
    build = ctx.root/'build/native/build'
    source = ctx.root/'build/native/source'
    required = [build/'pyconfig.h', source/'Include/Python.h', build/'libpython3.13.a',
                build/'Modules/_hacl/libHacl_Hash_SHA2.a']
    if any(not path.is_file() for path in required):
        raise RuntimeError('Existing same-pin native ABI oracle missing; no implicit native build')
    work = ctx.work/'abi-native-controls'
    work.mkdir(parents=True, exist_ok=False)
    inputs = {'native_inputs': {str(path): digest(path) for path in required},
              'source_pin': json.loads((ctx.root/'config/source.json').read_text()),
              'probes': {name: digest(ctx.root/'tests/Abi'/name)
                         for name in ('cpython_layout.c', 'cpython_operations.c')},
              'scope': 'native oracle only; no shipped dependency or translated qualification'}
    (ctx.artifacts/'abi-native-inputs.json').write_text(json.dumps(inputs, indent=2)+'\n')
    common = ['flock', '--close', NATIVE_LOCK, 'cc', '-std=c11', '-Wall', '-Wextra', '-Werror',
              '-I', build, '-I', source/'Include']
    ctx.run([*common, ctx.root/'tests/Abi/cpython_layout.c', '-o', work/'layout'], 'abi-native-layout-build')
    text = ctx.run([work/'layout'], 'abi-native-layout-run', timeout=suite.timeout)
    layout = json.loads(text)
    ctx.run([*common, '-DCP_B_ABI_MAIN', ctx.root/'tests/Abi/cpython_operations.c',
             build/'libpython3.13.a', build/'Modules/_hacl/libHacl_Hash_SHA2.a',
             '-lm', '-ldl', '-lpthread', '-o', work/'operations'], 'abi-native-operations-build')
    operations = json.loads(ctx.run([work/'operations', source/'Lib'], 'abi-native-operations-run', timeout=suite.timeout))
    if operations != {'cycles': 2, 'refcounts': True, 'immortals': True, 'slots': True, 'varargs': True, 'vectorcall': True}:
        raise RuntimeError('Incomplete actual native ABI operation control')
    result = {'layout': layout, 'operations': operations}
    (ctx.artifacts/'abi-native-control.json').write_text(json.dumps(result, indent=2)+'\n')
    ctx.__dict__['_cpython_abi_native'] = result
    return result


def run_suite(ctx, suite):
    from campaigns.recipes import helper
    from campaigns.testing import forms, modes
    delivery = helper(ctx.root/'tests/Delivery/campaign_hooks.py')
    compare = helper(ctx.root/'tests/Abi/compare_layouts.py')
    products = {form: delivery.product(ctx, form) for form in forms(ctx)}
    pointer = json.loads((ctx.root/'artifacts/campaign/current-embedded-gil.json').read_text())
    receipt = Path(pointer['receipt'])
    provenance = json.loads(receipt.read_text())
    if provenance['status'] != 'passed' or provenance['profile'] != 'embedded-gil':
        raise RuntimeError('ABI gate requires the passed actual embedded-gil translation receipt')
    oracle = native_controls(ctx, suite)
    failed = []
    cells = ctx.receipt.setdefault('abi_cells', [])
    for form, product in products.items():
        for mode in modes(ctx):
            label = 'abi-'+form+'-'+mode
            row = {'form': form, 'mode': mode, 'profile': ctx.profile, 'product': str(product),
                   'translation_receipt': str(receipt), 'status': 'unrun'}
            cells.append(row)
            try:
                cell = delivery.build_cell(ctx, form, mode, ctx.root/'tests/Abi/Abi.csproj', 'abi')
                row['translation_identity'] = cell['translation_identity']
                row['compiled_artifacts'] = cell['compiled_artifacts']
                output = Path(cell['output'])
                command = delivery.command(output, 'Abi', mode)
                layout = ctx.artifacts/(label+'-layout.json')
                ctx.run([*command, '--layout', layout, receipt], label+'-layout-run', timeout=suite.timeout, separate=True)
                row['layout'] = compare.compare(oracle['layout'], json.loads(layout.read_text()))
                operations = ctx.artifacts/(label+'-operations.json')
                ctx.run([*command, '--operations', output/'cpython/stdlib', operations],
                        label+'-operations-run', timeout=suite.timeout, separate=True)
                observed = json.loads(operations.read_text())
                if (observed.get('status') != 'passed' or observed.get('candidate_kind') != 'translated'
                        or observed.get('cycles') != 2 or observed.get('assertions', 0) < 36
                        or any(observed.get(key) is not True for key in ('refcounts', 'immortals', 'slots', 'varargs', 'vectorcall'))):
                    raise RuntimeError('Incomplete actual translated ABI operation report')
                delivery.product(ctx, form)
                row.update(status='passed', operations=observed)
            except Exception as error:
                row.update(status='blocked-or-failed', error=str(error))
                failed.append(form+'-'+mode)
            finally:
                delivery.record(ctx, label, row)
    if failed:
        raise RuntimeError('Required actual ABI cells blocked/failed: '+', '.join(failed))
