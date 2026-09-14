#!/usr/bin/env python3
"""Deterministic lexical inventory; deliberately not a reachability claim."""
import hashlib, json, pathlib, re
root = pathlib.Path(__file__).resolve().parents[1]
source = root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
rows = []
for path in sorted((source / 'blink').glob('*.[ch]')):
    text = path.read_text()
    rows.append({'path': str(path.relative_to(source)), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                 'headers': re.findall(r'^#include\s+[<"]([^>"\n]+)', text, re.M),
                 'thread_local_declarations': re.findall(r'^_Thread_local[^;]+;', text, re.M),
                 'extern_declarations': re.findall(r'^extern[^;]+;', text, re.M)})
output = root / 'config/source-inventory.json'
output.write_text(json.dumps({'kind': 'lexical-source-inventory-not-link-closure', 'revision': source.name.removeprefix('blink-'), 'files': rows}, indent=2) + '\n')
print(output)
