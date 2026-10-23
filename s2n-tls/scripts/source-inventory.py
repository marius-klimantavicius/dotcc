#!/usr/bin/env python3
"""Freeze/check the actual CMake selection, suite relationships and upstream cases."""
import argparse
import ast
import collections
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def selected_units(source):
    text = (source / 'CMakeLists.txt').read_text()
    groups = {}
    names = ('CRYPTO_SRC', 'ERROR_SRC', 'STUFFER_SRC', 'TLS_SRC', 'UTILS_SRC')
    for kind, name, glob in re.findall(r'file\((GLOB(?:_RECURSE)?)\s+(\w+_SRC)\s+"([^"\n]+\.c)"\)', text):
        if name not in names:
            continue
        paths = ((source / glob.split('/')[0]).rglob('*.c')
                 if kind == 'GLOB_RECURSE' else source.glob(glob))
        groups[name] = sorted(p.relative_to(source).as_posix() for p in paths)
    if tuple(groups) != names:
        raise RuntimeError('Changed CMake group selection: ' + str(tuple(groups)))
    selection = re.search(r'file\(GLOB S2N_SRC\s+(.*?)\)', text, re.S)
    if not selection or re.findall(r'\$\{(\w+)\}', selection[1]) != list(names):
        raise RuntimeError('Changed CMake library source closure')
    units = [p for values in groups.values() for p in values]
    if len(units) != len(set(units)):
        raise RuntimeError('Duplicate CMake units')
    return {'units': units, 'groups': groups, 'cmake_source': 'CMakeLists.txt',
            'upstream_c_units': len(units),
            'authored_units': json.loads((ROOT / 'config/translation.json').read_text())['authored_units']}


def suite_inventory(source):
    text = (source / 'tls/s2n_cipher_suites.c').read_text()
    table = re.search(r'static struct s2n_cipher_suite \*s2n_all_cipher_suites\[\] = \{(.*?)\n\};', text, re.S)[1]
    names = re.findall(r'&(s2n_\w+)', table)
    suites = []
    versions = ['S2N_SSLv3', 'S2N_TLS10', 'S2N_TLS11', 'S2N_TLS12', 'S2N_TLS13']
    for name in names:
        definition = re.search(r'struct s2n_cipher_suite ' + name + r' = (?:/\*[^*]+\*/ )?\{(.*?)\n\};', text, re.S)
        if not definition:
            raise RuntimeError('Missing unique suite definition: ' + name)
        body = definition[1]
        iana = re.search(r'&' + name + r',\s*/\* (0x\w+),(0x\w+) \*/', table)
        field = lambda name: re.search(r'\.' + name + r'\s*=\s*([^,\n]+)', body)[1].strip()
        minimum = field('minimum_required_tls_version')
        sslv3 = field('sslv3_record_alg') != 'NULL'
        applicable = [versions[-1]] if minimum == versions[-1] else versions[versions.index(minimum):-1]
        if ('S2N_SSLv3' in applicable) != sslv3:
            raise RuntimeError('Conflicting SSLv3 relationships: ' + name)
        suites.append({'symbol': name, 'name': field('name').strip('"'),
                       'iana_name': field('iana_name').strip('"'),
                       'iana_id': '0x%04X' % (int(iana[1], 16) * 256 + int(iana[2], 16)),
                       'minimum_version': minimum, 'versions': applicable,
                       'sslv3_record_alg': field('sslv3_record_alg').removeprefix('&') if sslv3 else None,
                       'key_exchange': field('key_exchange_alg').removeprefix('&') if '.key_exchange_alg' in body else 'TLS13',
                       'authentication': field('auth_method') if '.auth_method' in body else 'TLS13',
                       'required': True})
    return suites


CONTROL_BEHAVIOR = {
    's2n_config_disable_x509_verification': 'Explicit bypass of certificate chain/identity validation; test validating and bypassed outcomes.',
    's2n_config_disable_x509_time_verification': 'Permit expired/not-yet-valid certificates while retaining other validation.',
    's2n_config_disable_x509_intent_verification': 'Permit mismatched KeyUsage/ExtendedKeyUsage while retaining other validation.',
    's2n_config_allow_ip_in_cn': 'Explicit legacy hostname workaround: permit IP address Common Name fallback only with no SAN extension.',
    's2n_config_set_check_stapled_ocsp_response': 'Toggle validation of received stapled OCSP response.',
    's2n_config_set_verify_host_callback': 'Custom hostname acceptance callback; callback rejection must fail.',
    's2n_connection_set_verify_host_callback': 'Per-connection hostname acceptance override.',
    's2n_config_set_max_cert_chain_depth': 'Change certificate chain-depth limit; over-limit validation rejects.',
    's2n_config_set_cipher_preferences': 'Select upstream named security policy without changing named-policy semantics.',
    's2n_connection_set_cipher_preferences': 'Per-connection named security policy override.',
    's2n_connection_set_blinding': 'Select built-in delay or self-service delay; self-service caller must honor s2n_connection_get_delay.',
    's2n_config_set_max_blinding_delay': 'Configure maximum error blinding delay; lower delay is an explicit caller choice.',
    's2n_config_enable_cert_req_dss_legacy_compat': 'Include legacy DSS certificate type in server CertificateRequest; no DSA cipher-suite promise.',
    's2n_config_set_client_auth_type': 'Choose no/optional/required client-certificate authentication.',
    's2n_connection_set_client_auth_type': 'Per-connection client-certificate authentication override.',
    's2n_config_set_alert_behavior': 'Select behavior for received warning alerts; preserve fatal alert rejection.',
    's2n_config_set_verify_after_sign': 'Control optional self-verification of newly generated signatures.',
    's2n_config_set_key_log_cb': 'Explicit optional callback exports session secrets; preserve opt-in callback and lifetime semantics.',
    's2n_config_set_npn': 'Explicit legacy NPN enablement; preserve upstream protocol applicability and ALPN interaction.',
    's2n_config_set_serialization_version': 'Explicit connection-state serialization enablement; default is off.',
    's2n_connection_serialize': 'Exports plaintext key material without integrity protection; retain upstream buffered-I/O and RC4 rejection constraints.',
    's2n_connection_deserialize': 'Imports caller-protected connection state without intrinsic integrity verification; config must be reapplied.',
    's2n_config_set_cert_validation_cb': 'Additional application certificate validation callback, including asynchronous accept/reject; does not bypass preceding validation.',
    's2n_crypto_disable_init': 'Caller owns crypto library initialization when automatic initialization is disabled.',
    's2n_disable_atexit': 'Caller explicitly owns final library cleanup; do not imply independent global domains.',
    's2n_cleanup_final': 'Actual global library cleanup after all connections/configs/credentials drain; s2n_cleanup is a retained no-op.',
    's2n_rand_set_callbacks': 'Retained no-op. Cannot substitute for actual libcrypto/RAND or OS entropy ownership.',
    's2n_config_set_renegotiate_request_cb': 'Client-side renegotiation request callback; preserve secure renegotiation rules.',
    's2n_config_ktls_enable_unsafe_tls13': 'Outside selected product: kTLS/offload explicitly excluded by PLAN.',
}

NATIVE_TESTS = ['s2n_sslv3_test', 's2n_rc4_test', 's2n_3des_test', 's2n_cbc_test',
                's2n_hash_all_algs_test', 's2n_hmac_test', 's2n_ssl_prf_test',
                's2n_tls_prf_test', 's2n_tls13_prf_test', 's2n_cipher_suites_test',
                's2n_connection_blinding_test', 's2n_x509_intent_verification_test',
                's2n_x509_validator_time_verification_test', 's2n_client_renegotiation_info_extension_test',
                's2n_client_secure_renegotiation_test', 's2n_server_cert_request_test']


def controls(source):
    output = []
    headers = [source / 'api/s2n.h', *sorted((source / 'api/unstable').glob('*.h'))]
    declarations = {}
    for path in headers:
        for match in re.finditer(r'S2N_API[^;]+;', path.read_text(), re.S):
            name = re.search(r'\b(s2n_\w+)\s*\(', match[0])
            if name:
                declarations[name[1]] = path.relative_to(source).as_posix()
    for name, behavior in CONTROL_BEHAVIOR.items():
        if name not in declarations:
            raise RuntimeError('Missing inventoried control ' + name)
        output.append({'api': name, 'header': declarations[name], 'behavior': behavior,
                       'status': 'excluded' if 'ktls' in name else 'required-unrun'})
    return output


def test_inventory(source):
    cases = []
    for path in sorted((source / 'tests').rglob('*')):
        if path.suffix not in ('.c', '.py', '.rs', '.sh') or not path.is_file():
            continue
        relative = path.relative_to(source).as_posix()
        content = path.read_text(errors='replace')
        category, reason, status = 'harness', 'Shared fixture/support; exercised by dependent cases.', 'support'
        if relative.startswith('tests/unit/') and path.suffix == '.c':
            category, reason, status = 'unit', 'Applicable upstream assertions; conditional feature branches reviewed separately.', 'required-unrun'
            exclusions = {'ktls': 'kTLS/offload', 'mlkem': 'post-quantum', 'mldsa': 'post-quantum',
                          'early_data': '0-RTT', 'quic': 'QUIC transport', 'fips': 'FIPS qualification',
                          'seccomp': 'native platform sandbox', 'mem_usage': 'native process memory accounting'}
            for token, label in exclusions.items():
                if token in path.stem:
                    status, reason = 'excluded', label + ' is outside selected product.'
                    break
            if path.name == 's2n_tls_record_stuffer_test.c':
                # The pin places its complete BEGIN_TEST/END_TEST workload under
                # literal #if 0. Native execution exits0 with no assertions.
                if not re.search(r'int main\([^)]*\)\s*\{\s*#if 0.*?END_TEST\(\);\s*#endif\s*return 0;\s*\}', content, re.S):
                    raise RuntimeError('Upstream disabled stuffer test changed; review applicability')
                status, reason = 'excluded', 'Upstream disables the complete workload under literal #if0; native exits0 without assertions. No active test is counted as passing.'
        elif relative.startswith('tests/integrationv2/test_') and path.suffix == '.py':
            category, reason, status = 'integration', 'Peer/harness adaptation required; preserve individual pytest assertions and parameter combinations.', 'required-unrun'
            if any(token in path.stem for token in ('pq', 'early_data', 'ktls', 'quic', 'fips')):
                status, reason = 'excluded', 'Dedicated optional feature outside selected product.'
        elif '/cbmc/' in relative or '/saw/' in relative or '/sidetrail/' in relative or '/formal/' in relative:
            category, reason, status = 'formal', 'Formal verification harness is outside functional translation qualification.', 'excluded'
        elif '/fuzz/' in relative:
            category, reason, status = 'fuzz', 'Extended fuzzing is not selected; ordinary malformed-input unit assertions remain required.', 'excluded'
        elif '/integration/' in relative and path.name.startswith('s2n'):
            category, reason, status = 'integration', 'Legacy integration harness requires applicability review; not counted as passing.', 'review-unrun'
        tests = []
        if path.suffix == '.py':
            try:
                tests = [n.name for n in ast.walk(ast.parse(content)) if isinstance(n, (ast.FunctionDef, ast.AsyncFunctionDef)) and n.name.startswith('test_')]
            except SyntaxError:
                pass
        guards = [{'line': i, 'text': line.strip()} for i, line in enumerate(content.splitlines(), 1)
                  if re.search(r'\b(?:continue|SKIP|pytest\.skip|pytest\.mark\.skip|END_TEST|s2n_is_in_fips_mode|is_available|is_supported)\b', line)]
        cases.append({'path': relative, 'category': category, 'status': status, 'reason': reason,
                      'assertion_sites': len(re.findall(r'\b(?:EXPECT_\w+|assert)\s*\(', content)),
                      'test_functions': tests, 'conditional_or_skip_sites': guards,
                      'execution': {'passed': 0, 'failed': 0, 'skipped': 0, 'unrun': True}})
    return {'counting': 'File/case-family inventory and static assertion sites, not runtime assertion counts. Parameterized cases require runtime collection and execution receipts.',
            'summary': dict(collections.Counter(c['status'] for c in cases)), 'native_control_tests': NATIVE_TESTS, 'cases': cases}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, default=ROOT / 'ref/s2n-tls')
    parser.add_argument('--write', action='store_true')
    args = parser.parse_args()
    manifest = selected_units(args.source)
    suites = suite_inventory(args.source)
    if len(manifest['units']) != 174 or len(suites) != 36 or sum(bool(s['sslv3_record_alg']) for s in suites) != 14:
        raise RuntimeError('Pinned source count/suite/SSLv3 inventory mismatch')
    compatibility = {'default_profile': 'compat', 'required_versions': ['S2N_SSLv3', 'S2N_TLS10', 'S2N_TLS11', 'S2N_TLS12', 'S2N_TLS13'],
                     'suite_count': len(suites), 'sslv3_suite_count': 14, 'suites': suites,
                     'suite_version_pairs': sum(len(s['versions']) for s in suites),
                     'internal_null_suite': 'TLS_NULL_WITH_NULL_NULL is initialization state, never negotiable.',
                     'controls': controls(args.source),
                     'legacy_contracts': ['SSLv2-format ClientHello compatibility only; full SSLv2 excluded.',
                         'Static RSA, finite-field DHE, ECDHE and ECDSA; no replacement TLS core.',
                         'SSLv3 MAC/PRF, combined MD5/SHA1, RC4 state across records, 3DES/CBC chaining.',
                         'Unsafe controls are selectable; compat does not implicitly bypass certificate validation.'],
                     'skip_policy': 'Every required unavailable capability or conditional skip remains failed/unrun, never a pass.'}
    compatibility['environment_controls'] = [
        {'name': 'S2N_DONT_MLOCK', 'source': 'utils/s2n_mem.c', 'behavior': 'Disables memory locking; native harness prerequisite, not a legacy algorithm switch.'},
        {'name': 'S2N_INTEG_TEST', 'source': 'utils/s2n_init.c', 'behavior': 'Integration-test mode; must not be required for default compat product.'},
        {'name': 'S2N_PRINT_STACKTRACE', 'source': 'utils/s2n_init.c', 'behavior': 'Diagnostic stacktrace enablement.'}]
    records = {'source-manifest.json': manifest, 'compatibility.json': compatibility, 'upstream-tests.json': test_inventory(args.source)}
    for name, record in records.items():
        target = ROOT / 'config' / name
        if args.write:
            target.write_text(json.dumps(record, indent=2) + '\n')
        elif json.loads(target.read_text()) != record:
            raise RuntimeError('Inventory drift: ' + str(target))
    print(json.dumps({'units': len(manifest['units']), 'suites': len(suites), 'sslv3': 14,
                      'suite_version_pairs': compatibility['suite_version_pairs'], 'upstream': records['upstream-tests.json']['summary']}))


if __name__ == '__main__':
    main()
