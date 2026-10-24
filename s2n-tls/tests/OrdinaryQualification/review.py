#!/usr/bin/env python3
"""Read-only evidence/closure review for one completed ordinary-body attempt."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import subprocess


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def code_only(text):
    return re.sub(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*.*?\*/',
                  ' ', text, flags=re.S)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('attempt', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    work = args.attempt.resolve()
    receipt_path = work / 'receipt.json'
    report = json.loads(receipt_path.read_text())
    failures, checked = [], {}

    def check(path, expected):
        path = Path(path)
        observed = sha(path) if path.is_file() else None
        if observed != expected:
            failures.append({'path': str(path), 'expected': expected, 'actual': observed})
        checked[str(path)] = observed

    for path, expected in report['inputs_sha256'].items():
        check(path, expected)
    for key in ('generated_raw_sha256', 'generated_processed_sha256', 'runtime_outputs_sha256'):
        for path, expected in report.get(key, {}).items():
            check(path, expected)
    check(report['inventory'], report['inventory_sha256'])
    for command in report['commands']:
        check(command['log'], command['log_sha256'])
        if command['exit_code'] != 0:
            failures.append({'command_failed': command})
    # Additional measured provenance for the exact intermediate objects, wrapper
    # sources, generated projects and private harness snapshots. These are a
    # post-execution observation, distinct from the pre-execution input hashes.
    intermediate_observed = {str(p): sha(p) for p in work.iterdir() if p.is_file()
                             and p.suffix in ('.c', '.h', '.cs', '.o', '.d', '.py')}
    project_observed = {str(p): sha(p) for form in ('raw', 'processed')
                        for p in (work / form).glob('*.csproj')}
    result_keys = Counter((row['case'], row['mode']) for row in report['results'])
    for case in report['selected_cases']:
        for mode in ('native-isolated', 'raw-jit', 'raw-aot', 'processed-jit', 'processed-aot'):
            if result_keys[(case, mode)] != 1:
                failures.append({'missing_or_duplicate': [case, mode]})
    for result in report['results']:
        check(result['log'], result['log_sha256'])
        check(result['output'], result['output_sha256'])
        log = Path(result['log']).read_text()
        counts = re.findall(r'PASSED\s+(\d+) tests', log)
        if (len(counts) != 1 or int(counts[0]) != result['native_original_assertions']
                or result['actual_assertions'] != result['native_original_assertions']
                or result['status'] != 'passed' or 'FAILED' in log or 'SKIPPED' in log):
            failures.append({'assertion_mismatch': result})
    # No catalogue wrappers, provider initialization, randomness, or protocol
    # constructors are present in either actual emitted tree, even as dead code.
    forbidden = r'\b(?:Provider_\w+|s2n_init|s2n_rand_init|s2n_hash_\w+|s2n_get_(?:public|private)_random_data|s2n_config_new\w*|s2n_connection_new|s2n_negotiate)\s*\('
    closure = {}
    for form in ('raw', 'processed'):
        texts = [p.read_text() for p in (work / form).glob('*.cs')]
        clean = code_only('\n'.join(texts))
        hits = sorted(set(re.findall(forbidden, clean)))
        provider_names = sorted(set(re.findall(r'global::Managed\.S2nTls|BouncyCastle|CryptoProvider', clean)))
        # The complete safety TU has a PKCS1 helper, but the ordinary controls
        # never reference it. Exactly one occurrence is its method definition.
        pkcs1_occurrences = len(re.findall(r'\bs2n_constant_time_pkcs1_unpad_or_dont\s*\(', clean))
        closure[form] = {'forbidden_call_hits': hits, 'provider_names': provider_names,
                         'pkcs1_helper_occurrences': pkcs1_occurrences,
                         'pkcs1_helper_has_no_callers': pkcs1_occurrences == 1}
        if hits or provider_names or pkcs1_occurrences != 1:
            failures.append({'closure': closure[form]})
    native_dependencies = {}
    for binary in sorted(work.glob('*-native')):
        result = subprocess.run(['readelf', '-d', str(binary)], text=True, capture_output=True, check=True)
        needed = re.findall(r'\(NEEDED\).*?\[(.*?)\]', result.stdout)
        native_dependencies[str(binary)] = needed
        if any(re.search(r'crypto|ssl|s2n', lib, re.I) for lib in needed):
            failures.append({'native_provider_dependency': [str(binary), needed]})
    inventory = json.loads(Path(report['inventory']).read_text())
    for row in inventory['cases']:
        if row['classification'] == 'runnable-isolated-body' and row['original_sha256'] != row['native_original_sha256']:
            failures.append({'native_original_source_mismatch': row['case']})
    review = {'status': 'passed' if report['status'] == 'passed' and not failures else 'failed',
              'receipt': str(receipt_path), 'receipt_sha256': sha(receipt_path),
              'review_source': str(Path(__file__).resolve()), 'review_source_sha256': sha(__file__),
              'files_checked': len(checked), 'checked_sha256': checked,
              'intermediate_observed_after_execution_sha256': intermediate_observed,
              'project_observed_after_execution_sha256': project_observed,
              'result_counts': dict(Counter(r['status'] for r in report['results'])),
              'generated_closure': closure, 'native_needed_libraries': native_dependencies,
              'full_original_execution': False, 'canonical_credit': False, 'failures': failures}
    args.output.write_text(json.dumps(review, indent=2) + '\n')
    print(json.dumps({k: review[k] for k in ('status', 'files_checked', 'result_counts', 'generated_closure', 'failures')}, indent=2))
    return 0 if review['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
