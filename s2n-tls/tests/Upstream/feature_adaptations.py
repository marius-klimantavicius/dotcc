"""Exact accepted outside-PLAN conditional guards; original assertion bytes stay."""
import hashlib
import json
from pathlib import Path

DECLARATIONS = Path(__file__).with_name('test-adaptations.json')


def apply(case, original):
    declarations = json.loads(DECLARATIONS.read_text())
    selected = next((r for r in declarations['cases'] if r['case'] == case), None)
    if selected is None: return original, []
    digest = hashlib.sha256(original).hexdigest()
    if digest != selected['original_sha256']:
        raise RuntimeError('Accepted feature guard does not match exact frozen upstream source: ' + case)
    lines = original.splitlines(keepends=True)
    receipt = {'feature': declarations['feature'], 'case': case, 'original_sha256': digest,
               'defined_macro': declarations['macro'], 'regions': []}
    for region in selected['regions']:
        start, end = region['start_line'], region['end_line']
        complete = b''.join(lines[start - 1:end])
        if hashlib.sha256(complete).hexdigest() != region['original_region_sha256']:
            raise RuntimeError('Accepted excluded region changed: ' + case + ':' + str(start))
        if lines[start - 1].strip().decode() != region['original_condition']:
            raise RuntimeError('Accepted conditional directive changed')
        body = b''.join(lines[start:end])
        receipt['regions'].append({'start_line': start, 'end_line': end,
            'original_region_sha256': region['original_region_sha256'], 'preserved_body_sha256': hashlib.sha256(body).hexdigest(),
            'assertion_sites_explicitly_excluded': region['assertion_sites'], 'original_condition': region['original_condition'],
            'replacement_condition': region['proposed_condition']})
        lines[start - 1] = region['proposed_condition'].encode() + b'\n'
    # No line other than a named opening conditional can differ.
    transformed = b''.join(lines)
    reverse = transformed.splitlines(keepends=True)
    for region in selected['regions']: reverse[region['start_line'] - 1] = original.splitlines(keepends=True)[region['start_line'] - 1]
    if b''.join(reverse) != original: raise RuntimeError('Feature adaptation changed bytes outside the declared conditional directives')
    receipt['adapted_sha256'] = hashlib.sha256(transformed).hexdigest()
    return transformed, [receipt]
