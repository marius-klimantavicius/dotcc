#!/usr/bin/env python3
"""Inventory pinned upstream tests without claiming that they were executed."""
import json
from common import ROOT, SOURCE_SPEC, fetch, sha

source = fetch()
tests = source / 'tests'
files = {str(path.relative_to(source)): sha(path)
         for path in sorted(tests.rglob('*')) if path.is_file()}
cases = []
for path in sorted(tests.glob('test_*.sh')):
    name = path.name
    requirements = ['isolated authenticated SMB share']
    if 'valgrind' in name:
        requirements.append('native Valgrind instrumentation')
    if 'socket_error' in name or 'malloc_error' in name:
        requirements.append('native LD_PRELOAD fault injection')
    if 'open_timeout' in name:
        requirements.append('special scrambla libsmb2_issue_484 server; ordinary Samba is insufficient')
    if 'dcerpc' in name:
        requirements.append('optional full libdcerpc profile, deferred by this campaign')
    if 'overdrawn' in name:
        requirements.append('10 independent native processes with 2000 stat requests each')
    cases.append(dict(path=str(path.relative_to(source)), sha256=sha(path), requirements=requirements,
                      campaign_execution='not executed by this inventory',
                      note='Custom campaign differentials do not count as execution of this upstream script.'))
receipt = dict(source=SOURCE_SPEC, inventory_complete=True, upstream_execution_claim=False,
               test_files=files, shell_cases=cases,
               standalone_c_sources=[str(path.relative_to(source)) for path in sorted(tests.glob('*.c'))],
               makefile_programs=['prog_ls', 'prog_mkdir', 'prog_rmdir', 'prog_cat', 'prog_cat_cancel',
                                  'prog_open_timeout', 'prog_setsd', 'prog_ssc', 'metastat-0202-censored'],
               optional_programs=['smb2-dcerpc-coder-test'],
               native_injection_helper='ld_sockerr',
               additional_vector_sources=['aes128ccm-test.c', 'ntlmssp_generate_blob.c'])
destination = ROOT / 'artifacts/upstream-inventory.json'
destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(receipt, indent=2) + '\n')
print(f'{len(files)} upstream test files; {len(cases)} shell cases; no execution claim: {destination}')
