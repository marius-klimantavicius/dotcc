#!/usr/bin/env python3
"""Check literal normalized field names in managed-owned native adapters."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
sql = (root / 'schema/roslynobj.sql').read_text()
schema = {
    name: set(re.findall(r'^  (\w+) (?:INTEGER|TEXT)', body, re.M))
    for name, body in re.findall(
        r'CREATE TABLE (\w+) \(\n(.*?)\n\) (?:WITHOUT ROWID, )?STRICT;', sql, re.S)
}
issues = []
for filename in ('roslyn_source_sequences.h', 'roslyn_templates.h',
                 'roslyn_expressions_extra.h', 'roslyn_attributes.h', 'roslyn_routines.h', 'roslyn_execution.h'):
    source = (root / 'backend' / filename).read_text()
    calls = 0
    for match in re.finditer(r'DB\.put\("(\w+)"\s*,', source):
        table = match[1]
        end = match.end()
        depth = 1
        quoted = escaped = False
        while end < len(source) and depth:
            char = source[end]
            if quoted:
                if escaped:
                    escaped = False
                elif char == '\\':
                    escaped = True
                elif char == '"':
                    quoted = False
            elif char == '"':
                quoted = True
            elif char == '(':
                depth += 1
            elif char == ')':
                depth -= 1
            end += 1
        fields = set(re.findall(r'\{\s*"(\w+)"\s*,', source[match.end():end]))
        missing = fields - schema.get(table, set())
        if table not in schema or missing:
            issues.append((filename, table, sorted(missing)))
        calls += 1
    # Each put_fields vector belongs to one adapter function. Check both its
    # initializer and literal push_back destinations without treating nested
    # ordered-relation writes as fields of the enclosing payload.
    for function in re.split(r'(?=^static (?:Rid|void) )', source, flags=re.M):
        for payload in re.finditer(r'DB\.put_fields\("(\w+)"\s*,\s*(\w+)\s*\)',function):
            table, variable = payload.groups()
            initializer = re.search(r'\b'+variable+r'\s*=\s*\{(.*?)\};',function,re.S)
            fields = set(re.findall(r'\{\s*"(\w+)"\s*,',initializer[1])) if initializer else set()
            fields.update(re.findall(r'\b'+variable+r'\.push_back\(\{\s*"(\w+)"',function))
            missing = fields - schema.get(table,set())
            if table not in schema or missing:
                issues.append((filename,table,sorted(missing)))
            calls += 1
    print(f'{filename}: {calls} literal writes checked')
if issues:
    raise SystemExit(f'Unknown schema fields: {issues}')
print('Native literal field names match the shared schema.')
