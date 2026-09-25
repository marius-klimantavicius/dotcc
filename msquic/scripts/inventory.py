#!/usr/bin/env python3
"""Record the immutable upstream source/API inventory; do not infer support."""
import hashlib
import json
from pathlib import Path
import re
from source_inputs import source_units

ROOT = Path(__file__).resolve().parents[1]
pin = json.loads((ROOT / 'config/source.json').read_text())
source = ROOT / 'ref' / pin['directory']
units = source_units(source)
inventory = {'revision': pin['commit'], 'role': 'compiler survey candidates',
             'product_closure_frozen': False,
             'data_model': 'LP64, little-endian, Linux x64 first; host ABI not yet validated',
             'units': [{'path': name,
                        'sha256': hashlib.sha256((source / name).read_bytes()).hexdigest(),
                        'lines': len((source / name).read_text().splitlines()),
                        'selection': 'candidate; host/feature closure review pending'} for name in units]}
(ROOT / 'config/source-inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
header = (source / 'src/inc/msquic.h').read_text()
parameters = re.findall(r'^#define\s+(QUIC_PARAM_\w+)\s+([^\n]+)', header, re.M)
api_body = header.split('typedef struct QUIC_API_TABLE {', 1)[1].split('} QUIC_API_TABLE;', 1)[0]
api = re.findall(r'^\s*(\w+)\s+(\w+);', api_body, re.M)
surface = {'revision': pin['commit'],
           'support_policy': 'No managed API is available yet. Every entry needs an explicit implementation or rejection before P8.',
           'api_table': [{'name': name, 'type': typ, 'status': 'required later; not implemented'} for typ, name in api],
           'parameters': [{'name': name, 'declaration': declaration.strip(),
                           'status': 'not exposed; individual profile review pending'} for name, declaration in parameters]}
(ROOT / 'config/public-api-inventory.json').write_text(json.dumps(surface, indent=2) + '\n')
print(f'{len(units)} source candidates; {len(api)} API entries; {len(parameters)} parameter identifiers')
