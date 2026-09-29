#!/usr/bin/env python3
"""Materialize a native declaration dependency closure for ABI diagnostics.

Retain exact preprocessed source ranges selected by Clang's AST; reject unknown
types instead of approximating their layouts. --records enables experimental
aggregate dependency capture, requiring emitted/native layout comparison before
any ABI qualification. The default permits only primitive typedefs and enums.
"""
import argparse
import bisect
import hashlib
import json
import pathlib
import re
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[3]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('source', help='source relative to the pinned libuv tree')
parser.add_argument('--records', action='store_true')
parser.add_argument('--clang', default='clang-22')
parser.add_argument('--out', type=pathlib.Path)
parser.add_argument('--preprocessed', type=pathlib.Path,
                    help='existing genuine target preprocessor output; requires --ast')
parser.add_argument('--ast', type=pathlib.Path,
                    help='matching native Clang JSON AST of --preprocessed')
parser.add_argument('--target-id', default='linux-glibc-x64')
parser.add_argument('--target-triple', help='native target provenance, not a DotCC ABI override')
parser.add_argument('--input-receipt', type=pathlib.Path,
                    help='native toolchain/preprocessing receipt for supplied inputs')
parser.add_argument('--compiler-label', help='native compiler identity recorded with supplied inputs')
args = parser.parse_args()
if bool(args.preprocessed) != bool(args.ast):
    parser.error('--preprocessed and --ast must be supplied together')
if args.target_triple and not args.preprocessed:
    parser.error('--target-triple requires externally preprocessed target inputs; host headers cannot stand in for another target')
if not re.fullmatch(r'[A-Za-z0-9_.-]+', args.target_id):
    parser.error('--target-id must be a single safe directory component')
config = json.loads((ROOT / 'libuv/config/sources.json').read_text())
ref = ROOT / 'libuv/ref' / config['product']['directory']
source = (ref / args.source).resolve()
source.relative_to(ref.resolve())
out = args.out or ROOT / 'libuv/artifacts/native-closures' / args.target_id / pathlib.Path(args.source).with_suffix('')
out.mkdir(parents=True, exist_ok=True)
# A failed retry must never leave a previous success receipt beside stale output.
(out / 'receipt.json').write_text(json.dumps({'status': 'running', 'source': str(source), 'target_id': args.target_id}, indent=2) + '\n')
preprocessed = args.preprocessed.resolve() if args.preprocessed else out / 'native.i'
ast_path = args.ast.resolve() if args.ast else out / 'native.ast.json'
commands = []
if not args.preprocessed:
    commands = [
        [args.clang, '-E', '-std=gnu11', '-D_GNU_SOURCE', '-D_FILE_OFFSET_BITS=64',
         '-I' + str(ref / 'include'), '-I' + str(ref / 'src'), str(source), '-o', str(preprocessed)],
        [args.clang, '-Xclang', '-ast-dump=json', '-fsyntax-only', str(preprocessed)],
    ]
    subprocess.run(commands[0], check=True)
    with ast_path.open('w') as stream:
        subprocess.run(commands[1], check=True, stdout=stream)
    compiler_description = subprocess.check_output([args.clang, '--version'], text=True).splitlines()[0]
else:
    compiler_description = args.compiler_label or 'external native AST; see input receipt for compiler identity'
ast = json.loads(ast_path.read_text())
text = preprocessed.read_bytes()
by_id = {}
typedefs = {}
tags = {}
contained_declarations = {}
enum_constants = {}
roots = []
# Compute provenance from the native preprocessor's physical line markers.
# This does not depend on Clang JSON eliding repeated presumedFile values.
markers = []
position = 0
for line in text.splitlines(keepends=True):
    marker = re.match(rb'^#\s+(\d+)\s+("(?:\\.|[^"])*")', line)
    if marker:
        markers.append((position + len(line), json.loads(marker.group(2))))
    position += len(line)
marker_offsets = [entry[0] for entry in markers]
def origin(offset):
    index = bisect.bisect_right(marker_offsets, offset) - 1
    return markers[index][1] if index >= 0 else ''
for node in ast['inner']:
    by_id[node['id']] = node
    if node['kind'] == 'TypedefDecl':
        typedefs[node['name']] = node['id']
    if node['kind'] in ('RecordDecl', 'EnumDecl') and node.get('name'):
        name = node['name']
        if name not in tags or node.get('completeDefinition') or len(node.get('inner', [])) > len(by_id[tags[name]].get('inner', [])):
            tags[name] = node['id']
    if node['kind'] == 'EnumDecl':
        for child in node.get('inner', []):
            if child['kind'] == 'EnumConstantDecl':
                enum_constants[child['id']] = node['id']
    loc = node.get('loc', {})
    location = origin(loc.get('offset', -1))
    if location == str(source):
        roots.append(node['id'])
if not roots:
    raise SystemExit('No product definitions found; source/location mapping cannot be guessed.')

def walk(value):
    if isinstance(value, dict):
        yield value
        for child in value.values():
            yield from walk(child)
    elif isinstance(value, list):
        for child in value:
            yield from walk(child)

for parent in ast['inner']:
    for child in walk(parent.get('inner', [])):
        if child.get('kind') in ('RecordDecl', 'EnumDecl', 'TypedefDecl') and 'range' in child:
            contained_declarations[child['id']] = parent['id']

primitive_words = {'void', 'char', 'signed', 'unsigned', 'short', 'int', 'long', 'float', 'double',
                   'const', 'volatile', 'restrict', '_Bool', 'enum'}
selected = set()
queue = list(roots)
while queue:
    identity = queue.pop()
    if identity in selected:
        continue
    declaration = by_id[identity]
    if declaration['kind'] not in ('FunctionDecl', 'VarDecl', 'TypedefDecl', 'EnumDecl', 'StaticAssertDecl', 'EmptyDecl') and not (args.records and declaration['kind'] == 'RecordDecl'):
        raise SystemExit('Unsupported nonprimitive declaration dependency: ' + declaration['kind'])
    selected.add(identity)
    if declaration['kind'] in ('EnumDecl', 'RecordDecl'):
        start = declaration['range']['begin']['offset']
        end = declaration['range']['end']['offset']
        for candidate in by_id.values():
            extent = candidate.get('range', {})
            if candidate['kind'] == 'TypedefDecl' and extent.get('begin', {}).get('offset', start) < start and extent.get('end', {}).get('offset', 0) > end:
                queue.append(candidate['id'])
    for node in walk(declaration):
        if 'qualType' in node:
            spelling = node['qualType']
            checked_spelling = node.get('desugaredQualType', spelling) if spelling in ('__size_t', '__ptrdiff_t') and 'typeAliasDeclId' not in node else spelling
            if args.records:
                checked_spelling = re.sub(r'\((?:unnamed|anonymous) at [^)]*\)', '', checked_spelling)
            words = set(re.findall(r'[A-Za-z_]\w*', checked_spelling))
            allowed = primitive_words | ({'struct', 'union', '__restrict', '__restrict__'} if args.records else set())
            named_types = typedefs | (tags if args.records else {})
            for name in words - allowed:
                if name in named_types:
                    queue.append(named_types[name])
            if words - allowed - named_types.keys() or 'long double' in spelling or '...' in spelling:
                raise SystemExit('Nonprimitive or unsupported ABI type requires a measured declaration closure: ' + spelling)
        if node.get('kind') in ('EnumType', 'RecordType') and 'decl' in node:
            target = node['decl']['id']
            if target in by_id:
                queue.append(target)
            elif target in contained_declarations:
                queue.append(contained_declarations[target])
            else:
                raise SystemExit('Unresolved aggregate declaration: ' + str(node['decl']))
        if 'referencedDecl' in node:
            reference = node['referencedDecl']
            if reference['id'] in by_id:
                queue.append(reference['id'])
            elif reference['id'] in enum_constants:
                queue.append(enum_constants[reference['id']])
            elif not any(local.get('id') == reference['id'] and 'range' in local
                         for local in walk(declaration)):
                raise SystemExit('Unresolved declaration dependency: ' + str(reference))

ranges = []
for identity in selected:
    declaration = by_id[identity]
    extent = declaration.get('range', {})
    begin, end = extent.get('begin', {}), extent.get('end', {})
    if 'offset' not in begin or 'offset' not in end or 'tokLen' not in end:
        raise SystemExit('Unavailable exact preprocessed range: ' + str(declaration.get('name')))
    start, finish = begin['offset'], end['offset'] + end['tokLen']
    if any(other != identity and by_id[other].get('range', {}).get('begin', {}).get('offset', start) < start and by_id[other].get('range', {}).get('end', {}).get('offset', 0) >= finish for other in selected):
        continue  # enclosing typedef carries the exact enum declaration too
    body = text[start:finish]
    # Clang declaration ranges omit the terminating semicolon for prototypes and
    # variables. Preserve that original punctuation, never synthesize its body.
    if not body.rstrip().endswith(b';') and (declaration['kind'] != 'FunctionDecl' or not body.rstrip().endswith(b'}')):
        cursor = finish
        while cursor < len(text) and text[cursor:cursor + 1].isspace():
            cursor += 1
        if text[cursor:cursor + 1] != b';':
            raise SystemExit('Missing declaration terminator in native source range.')
        finish = cursor + 1
        body = text[start:finish]
    ranges.append((start, finish, declaration.get('name', '(' + declaration['kind'] + ')'), body))
ranges.sort()
for previous, current in zip(ranges, ranges[1:]):
    if previous[1] > current[0]:
        raise SystemExit('Overlapping source declarations cannot be materialized independently.')
closure = out / 'closure.c'
def standard_line_markers(body):
    # Native cpp linemarkers are metadata, not C tokens. Canonicalize only
    # their syntax so DotCC reads the same declaration/body token stream.
    return re.sub(rb'(?m)^#\s+(\d+)\s+("(?:\\.|[^"])*")[^\n]*',
                  lambda match: b'#line ' + match[1] + b' ' + match[2], body)
closure.write_bytes(b'\n\n'.join(standard_line_markers(row[3]) for row in ranges) + b'\n')
(out / 'receipt.json').write_text(json.dumps({
    'status': 'passed', 'line_marker_adaptation': 'GCC linemarkers canonicalized to standard #line; C tokens retained', 'diagnostic_only': True, 'abi_scope': 'unverified aggregate closure' if args.records else 'native compiler primitive-only dependency closure',
    'source': str(source), 'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
    'source_revision': config['product']['revision'],
    'preprocessed_sha256': hashlib.sha256(text).hexdigest(),
    'ast_sha256': hashlib.sha256(ast_path.read_bytes()).hexdigest(),
    'closure_sha256': hashlib.sha256(closure.read_bytes()).hexdigest(),
    'compiler': compiler_description,
    'target_id': args.target_id, 'target_triple': args.target_triple,
    'external_preprocessed': str(preprocessed) if args.preprocessed else None,
    'external_ast': str(ast_path) if args.ast else None,
    'input_receipt': str(args.input_receipt.resolve()) if args.input_receipt else None,
    'input_receipt_sha256': hashlib.sha256(args.input_receipt.read_bytes()).hexdigest() if args.input_receipt else None,
    'commands': commands, 'product_declarations': [{'name': by_id[key].get('name'), 'kind': by_id[key]['kind']} for key in roots],
    'selected_declarations': [{'name': row[2], 'start': row[0], 'end': row[1], 'origin': origin(row[0])} for row in ranges],
    'omitted_top_level_declarations': len(ast['inner']) - len(ranges),
}, indent=2) + '\n')
print(closure)
