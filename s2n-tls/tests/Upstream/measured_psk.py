"""Require actual native peer proof before adapting its obsolete skip predicate."""
import hashlib
import inspect
import json
from pathlib import Path


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def validate(peers):
    selected = peers.get('measured_psk_peer')
    if not selected:
        return None
    policy = json.loads(Path(__file__).with_name('python-adaptations.json').read_text())['measured_external_psk']
    if selected['label'] != policy['peer_label']:
        raise RuntimeError('Measured PSK peer label differs from its bounded adaptation')
    proof_path = Path(selected['proof']).resolve()
    proof = json.loads(proof_path.read_text())
    build_path = Path(proof['derived_peer_build']).resolve()
    build = json.loads(build_path.read_text())
    if (proof['status'], proof['passed'], proof['failed']) != ('passed', 12, 0):
        raise RuntimeError('Measured PSK peer lacks its complete passing native proof')
    if build['status'] != 'passed' or build['label'] != selected['label']:
        raise RuntimeError('Measured PSK peer build did not pass with the declared label')
    axes = {(role, suite, variant) for role in ('client', 'server')
            for suite in ('TLS_AES_128_GCM_SHA256', 'TLS_AES_256_GCM_SHA384')
            for variant in ('positive', 'wrong-secret', 'wrong-identity')}
    cases = proof['cases']
    if len(cases) != len(axes) or {(r['derived_role'], r['cipher'], r['variant']) for r in cases} != axes:
        raise RuntimeError('Measured PSK proof omits a required role/digest/negative control')
    inputs = {str(proof_path): digest(proof_path), str(build_path): digest(build_path)}
    for inventory in (proof['inputs'], build['inputs'], build['outputs']):
        for path, expected in inventory.items():
            if path in inputs and inputs[path] != expected:
                raise RuntimeError('Measured PSK proof inventories disagree: ' + path)
            inputs[path] = expected
    for row in cases:
        if row['status'] != 'passed' or len(row['endpoints']) != 2:
            raise RuntimeError('Measured PSK proof has a failed or missing actual endpoint')
        positive = row['variant'] == 'positive'
        fallback = row['variant'] == 'wrong-identity' and row['derived_role'] == 'client'
        if positive and not (row.get('psk_identity_verified') and row.get('payload_verified')):
            raise RuntimeError('Measured PSK proof lacks negotiated identity and actual payload')
        if fallback and not (row.get('full_handshake_fallback') and row.get('payload_verified')):
            raise RuntimeError('Measured PSK proof lacks unknown-identity certificate fallback')
        if not (positive or fallback) and not row.get('rejected'):
            raise RuntimeError('Measured PSK negative control did not reject')
        for endpoint in row['endpoints']:
            if not endpoint['drained'] or ((positive or fallback) and endpoint['exit_code'] != 0):
                raise RuntimeError('Measured PSK proof endpoint did not terminate correctly')
            for log in endpoint['logs'].values():
                inputs[log['path']] = log['sha256']
    derived = Path(build['derived']).resolve()
    binary = str(derived / 'apps/openssl')
    if peers['commands'].get('OpenSSL:openssl') != [binary] or binary not in build['outputs']:
        raise RuntimeError('Original OpenSSL PSK commands do not select the measured binary exactly')
    if peers['commands'].get('openssl') == [binary]:
        raise RuntimeError('Measured peer must not replace ordinary OpenSSL/version commands')
    for name in ('libssl.a', 'libcrypto.a'):
        expected = build['outputs'][str(derived / name)]
        originals = [value for path, value in build['inputs'].items() if Path(path).name == name]
        if originals != [expected]:
            raise RuntimeError('Measured PSK app changed its pinned TLS libraries')
    for path, expected in inputs.items():
        if digest(path) != expected:
            raise RuntimeError('Measured PSK peer evidence changed: ' + path)
    return dict(selected, build_receipt=str(build_path), binary=binary, inputs_sha256=inputs)


def install(items, report, selected):
    if not selected:
        return
    policy = json.loads(Path(__file__).with_name('python-adaptations.json').read_text())['measured_external_psk']
    modules = {item.module for item in items}
    if len(modules) != 1 or Path(next(iter(modules)).__file__).name != policy['family']:
        raise RuntimeError('Measured PSK skip adaptation is limited to the original external-PSK family')
    module = next(iter(modules))
    original = getattr(module, policy['helper'])
    source = inspect.getsource(original)
    if hashlib.sha256(source.encode()).hexdigest() != policy['helper_sha256']:
        raise RuntimeError('Original PSK skip helper changed; refusing the measured peer adaptation')
    observation = dict(policy, original_file=module.__file__, original_file_sha256=digest(module.__file__),
                       peer_label=selected['label'], proof=selected['proof'], adapted_calls=[])
    report['measured_psk_adaptation'] = observation

    def measured_skip(provider, psk_hash_alg):
        if provider == module.OpenSSL and psk_hash_alg == 'SHA384':
            import os
            observation['adapted_calls'].append(os.environ.get('PYTEST_CURRENT_TEST'))
            return
        return original(provider, psk_hash_alg)

    setattr(module, policy['helper'], measured_skip)
