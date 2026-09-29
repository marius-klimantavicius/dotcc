"""Owning LOWPAN/ZEP/BLE lifetimes with valid independent wire inputs."""
import json
import sys
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite):
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('LOWPAN service qualification currently requires linux-x64')
    ctx.run([sys.executable, ctx.root / 'tests/Zep/native.py', '--profile', ctx.profile],
            'lowpan-services-native', timeout=1200)
    native = json.loads((ctx.root / 'artifacts/zep-native' / ctx.profile / 'report.json').read_text())
    for key, expected in {'zep_inputs': 4, 'valid_radio_fcs': True, 'ble_input': True,
                          'shared_timer': True, 'fragment_survives_other_close': True,
                          'pending_fragment_cleanup': True, 'queued_loopback_fifo_stop': True,
                          'partial_init_failure': True, 'restart': True,
                          'baseline_restored': True, 'negative_exit': 92}.items():
        if native.get(key) != expected:
            raise AssertionError('Native LOWPAN service control mismatch: ' + key)
    (ctx.artifacts / 'lowpan-services-native-report.json').write_text(json.dumps(native, indent=2) + '\n')
    ctx.run([sys.executable, ctx.root / 'tests/Zep/native.py', '--profile', ctx.profile, '--output-case'],
            'lowpan-output-native', timeout=1200)
    output_native = json.loads((ctx.root / 'artifacts/zep-output-native' / ctx.profile / 'report.json').read_text())
    for key in ('valid_fcs', 'compression', 'fragmentation_reassembly', 'ipv6_mtu_boundary', 'valid_duplicate', 'valid_reorder', 'expiry', 'baseline_restored'):
        if output_native.get(key) is not True:
            raise AssertionError('Native LOWPAN output mismatch: ' + key)
    (ctx.artifacts / 'lowpan-output-native-report.json').write_text(json.dumps(output_native, indent=2) + '\n')
    results = []
    for form in forms(ctx):
        product = check_product(ctx, form)
        for mode in modes(ctx):
            label = 'lowpan-services-' + form + '-' + mode
            work = ctx.work / label
            output = work / 'app'
            arguments = ['-p:LwipProject=' + str(product), '-p:UseLocalLalrCc=false',
                         '--artifacts-path', work / 'artifacts', '-o', output]
            project = ctx.root / 'tests/Zep/ZepServices.csproj'
            if mode == 'jit':
                ctx.managed('build', project, label + '-build', *arguments)
                command = ['dotnet', output / 'ZepServices.dll']
            else:
                ctx.managed('publish', project, label + '-build', *arguments,
                            '-r', ctx.options.rid, '-p:PublishAot=true', timeout=1800)
                command = [output / 'ZepServices']
            transcript = ctx.run(command, label + '-run', timeout=180)
            if 'PASS lowpan-services:' not in transcript:
                raise AssertionError('LOWPAN service assertions did not complete')
            results.append({'profile': ctx.profile, 'form': form, 'mode': mode,
                            'status': 'passed', 'scope': 'valid ZEP/BLE/radio input, copied radio/BLE output, compression/MTU/reassembly, unchanged duplicate/reorder/expiry, shared timer and owning lifetimes'})
    (ctx.artifacts / 'lowpan-services-report.json').write_text(json.dumps(results, indent=2) + '\n')
