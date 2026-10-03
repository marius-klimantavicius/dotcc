"""Opt-in macro APIs, typed and expanded by EDG in the selected translation unit.

Python only selects names and constructs adapters. It never evaluates C tokens.
"""
import hashlib
import json
from pathlib import Path
import re
import subprocess

_IDENTIFIER = re.compile(r'[A-Za-z_][A-Za-z_0-9]*\Z')
_CONTEXTUAL = {'__LINE__', '__FILE__', '__BASE_FILE__', '__COUNTER__', '__INCLUDE_LEVEL__',
               '__DATE__', '__TIME__', '__TIMESTAMP__', '__func__', '__FUNCTION__', '__PRETTY_FUNCTION__'}


def parse_definitions(text):
    definitions = {}
    for line in text.replace('\\\n', '').splitlines():
        match = re.fullmatch(r'#define ([A-Za-z_][A-Za-z_0-9]*)(\([^)]*\))?[ \t]*(.*)', line)
        if not match:
            if line.strip():
                raise ValueError('unrecognized EDG --list_macros output: ' + line[:160])
            continue
        definitions[match[1]] = {'name': match[1], 'parameters': match[2], 'replacement': match[3]}
    return definitions


def _type(value):
    # Contracts are native type names, not declarators or arbitrary declarations.
    if not isinstance(value, str) or not value.strip() or any(c in value for c in ';{}\n\r#'):
        raise ValueError('macro contract types must be nonempty native type names; use typedefs for complex declarators')
    return value


def _contract(value):
    if not isinstance(value, dict) or set(value) != {'returnType', 'parameters'}:
        raise ValueError('macro signatures require returnType and parameters')
    result = {'returnType': _type(value['returnType']), 'parameters': []}
    names = set()
    if not isinstance(value['parameters'], list):
        raise ValueError('macro signature parameters must be an array')
    for parameter in value['parameters']:
        if not isinstance(parameter, dict) or set(parameter) != {'name', 'type'}:
            raise ValueError('macro parameters require name and type')
        name = parameter['name']
        if not isinstance(name, str) or not _IDENTIFIER.fullmatch(name) or name in names:
            raise ValueError('macro parameter names must be unique ASCII identifiers')
        names.add(name)
        result['parameters'].append({'name': name, 'type': _type(parameter['type'])})
    return result


def prepare_macro_exports(*, compiler, standard, frontend_options, runtime_options, env,
                          sources, source, patterns, signatures_path, output_dir, language):
    """Retain the macro snapshot and replace one TU by an include-plus-adapters TU."""
    if source is None:
        if len(sources) != 1:
            raise ValueError('--macro-source is required with multiple input translation units')
        source = sources[0]
    source = source.resolve()
    if source not in sources:
        raise ValueError('--macro-source must name one of the input translation units')
    try:
        selectors = [re.compile(pattern) for pattern in patterns]
    except re.error as error:
        raise ValueError('invalid --export-macros regex: ' + str(error)) from error
    signatures = json.loads(signatures_path.read_text()) if signatures_path else {}
    if not isinstance(signatures, dict):
        raise ValueError('--macro-signatures must contain a JSON object keyed by macro name')
    command = [str(compiler), '--' + standard, '--gnu_version', '130300',
               *frontend_options, *runtime_options, '--list_macros', '--', str(source)]
    result = subprocess.run(command, env=env, capture_output=True, text=True)
    if result.returncode:
        raise ValueError('macro snapshot failed:\n' + result.stderr)
    definitions = parse_definitions(result.stdout)
    selected = [dict(item) for name, item in sorted(definitions.items())
                if any(pattern.fullmatch(name) for pattern in selectors)]
    if not selected:
        raise ValueError('--export-macros matched no active macros at the end of the selected translation unit')
    unused = set(signatures) - {item['name'] for item in selected}
    if unused:
        raise ValueError('macro signatures were not selected or are not defined: ' + ', '.join(sorted(unused)))
    identity = hashlib.sha256((str(source) + result.stdout).encode()).hexdigest()[:16]
    prefix = 'dotgen_macro_' + identity + '_'
    # Normal source maps continue to point into the original TU. A preprocessing
    # equivalence check below rejects include-sensitive source behavior.
    escaped = str(source).replace('\\', '\\\\').replace('"', '\\"')
    lines = ['/* Generated macro API adapters; retained for review. */', '#include "' + escaped + '"']
    decay = prefix + 'decay'
    if language == 'c++':
        deduced_type = prefix + 'deduced_type'
        lines += ['template<class ' + deduced_type + '> ' + deduced_type + ' ' + decay + '(' + deduced_type + ');']
    for item in selected:
        name = item['name']
        if not item['replacement'].strip():
            raise ValueError('cannot export empty/configuration macro ' + name)
        pending, seen = [name], set()
        while pending:
            token = pending.pop()
            if token in seen:
                continue
            seen.add(token)
            if token in _CONTEXTUAL:
                raise ValueError('context-dependent macro requires an explicit source API: ' + name + ' uses ' + token)
            if token in definitions:
                pending.extend(re.findall(r'\b[A-Za-z_][A-Za-z_0-9]*\b', definitions[token]['replacement']))
        bridge = prefix + name
        item['bridge'] = bridge
        signature = _contract(signatures[name]) if name in signatures else None
        if item['parameters'] is not None and signature is None:
            raise ValueError('function-like macro ' + name + ' requires a --macro-signatures type contract')
        if item['parameters'] is None and signature is not None and signature['parameters']:
            raise ValueError('object-like macro ' + name + ' cannot have signature parameters')
        item['signature'] = signature
        parameters = signature['parameters'] if signature else []
        invocation = name + ('(' + ', '.join(p['name'] for p in parameters) + ')' if item['parameters'] is not None else '')
        return_type = signature['returnType'] if signature else ('__typeof__(' + decay + '(' + name + '))' if language == 'c++' else '__typeof__((0, ' + name + '))')
        declaration = ', '.join(p['type'] + ' ' + p['name'] for p in parameters) or 'void'
        linkage = 'extern "C" ' if language == 'c++' else ''
        body = invocation + ';' if return_type.strip() == 'void' else 'return ' + invocation + ';'
        lines.append(linkage + return_type + ' ' + bridge + '(' + declaration + ') { ' + body + ' }')
    # Each input revision gets retained separately, even when packaging fails.
    adapter = '\n'.join(lines) + '\n'
    revision = hashlib.sha256(adapter.encode()).hexdigest()
    directory = output_dir.resolve() / '.macro-inputs' / revision
    directory.mkdir(parents=True, exist_ok=True)
    wrapper = directory / ('adapters.cpp' if language == 'c++' else 'adapters.c')
    wrapper.write_text(adapter)
    (directory / 'macros.txt').write_text(result.stdout)
    (directory / 'preprocessor.stderr').write_text(result.stderr)
    include_probe = directory / ('include-probe.cpp' if language == 'c++' else 'include-probe.c')
    include_probe.write_text('#include "' + escaped + '"\n')
    pp_command = [str(compiler), '--' + standard, '--gnu_version', '130300',
                  *frontend_options, *runtime_options, '--preprocess', '--']
    processed = []
    for label, path in [('original', source), ('included', include_probe)]:
        pp = subprocess.run([*pp_command, str(path)], env=env, capture_output=True, text=True)
        (directory / (label + '.i')).write_text(pp.stdout)
        (directory / (label + '.stderr')).write_text(pp.stderr)
        if pp.returncode:
            raise ValueError('macro adapter preprocessing check failed:\n' + pp.stderr)
        processed.append([line.strip() for line in pp.stdout.splitlines()
                          if line.strip() and not re.match(r'^\s*#\s*(?:line\s+)?[0-9]+(?:\s|$)', line)])
    if processed[0] != processed[1]:
        raise ValueError('macro adapter changes preprocessed source (for example __BASE_FILE__ or '
                         '__INCLUDE_LEVEL__); provide a dedicated API translation unit; evidence: ' + str(directory))
    manifest = {'schema': 1, 'source': str(source), 'selection': patterns,
                'snapshot': 'active-at-end-of-translation-unit', 'preprocessorCommand': command,
                'definitionsSha256': hashlib.sha256(result.stdout.encode()).hexdigest(),
                'wrapper': {'path': str(wrapper), 'sha256': revision}, 'macros': selected,
                'functionArgumentSemantics': 'managed arguments evaluated once before the adapter call',
                'includeWrapperVerification': 'EDG preprocessed original vs included input equal excluding line directives and blank lines'}
    if signatures_path:
        manifest['signatures'] = {'path': str(signatures_path.resolve()),
                                  'sha256': hashlib.sha256(signatures_path.read_bytes()).hexdigest()}
    (directory / 'macro-exports.json').write_text(json.dumps(manifest, indent=2) + '\n')
    return [wrapper if path == source else path for path in sources], manifest


def _parameters(text):
    parts, start, depth = [], 0, 0
    for index, char in enumerate(text):
        if char in '<([':
            depth += 1
        elif char in '>)]':
            depth -= 1
        elif char == ',' and depth == 0:
            parts.append(text[start:index].strip())
            start = index + 1
    if text.strip():
        parts.append(text[start:].strip())
    arguments = []
    for part in parts:
        name = re.search(r'(@?\w+)\Z', part)
        if not name:
            raise ValueError('unsupported generated macro parameter: ' + part)
        mode = re.match(r'(ref|out|in)\s', part)
        arguments.append((mode[1] + ' ' if mode else '') + name[1])
    return ', '.join(arguments)



def _adapter_signature(member, managed_name):
    # The emitter keeps the entire exported boundary (including catch clauses)
    # on one line. Stop at the matching parameter-list close, never at a later
    # catch (...). This scanner handles nested function-pointer/tuple syntax.
    prefix = re.search(r'^public (?:static )?([^\n{]+?) ' + re.escape(managed_name) + r'\(', member, re.M)
    if not prefix:
        return None
    start, depth = prefix.end(), 1
    for index in range(start, len(member)):
        if member[index] == '(':
            depth += 1
        elif member[index] == ')':
            depth -= 1
            if depth == 0:
                if not member[index + 1:].lstrip().startswith('{'):
                    return None
                return prefix[1], member[start:index]
    return None


def emit_macro_exports(source, manifest, exports, source_symbols, class_name, instance):
    """Promote compiler-folded integer literals, otherwise delegate to typed EDG APIs."""
    if manifest is None:
        return source, []
    members = {}
    member_pattern = re.compile(r'^// dot-gen:member-begin\n(.*?)^// dot-gen:member-end\n', re.M | re.S)
    for match in member_pattern.finditer(source):
        origin = re.search(r'^// dot-gen:origin (@?\w+)\n', match[1], re.M)
        if origin:
            members[origin[1]] = match[1]
    public = {item['symbol']: item['method'] for item in exports}
    bodies = {item['sourceName']: item['managedName'] for item in source_symbols if item['kind'] == 'function'}
    used = {item['managedName'].lstrip('@') for item in source_symbols}
    used.update(item['method'].lstrip('@') for item in exports)
    used.update((class_name, 'Run', 'Dispose', 'Shutdown', 'ReleaseThread'))
    replacements, additions, records = {}, [], []
    for item in manifest['macros']:
        name, bridge = item['name'], item['bridge']
        if name in used or name.startswith('__edg_'):
            raise ValueError('macro export conflicts with a generated member: ' + name)
        managed = public.get(bridge)
        if not managed or managed not in members:
            raise ValueError('EDG did not emit a public macro adapter for ' + name)
        signature = _adapter_signature(members[managed], managed)
        if not signature:
            raise ValueError('unsupported EDG adapter signature for ' + name)
        return_type, parameters = signature
        original = members[managed]
        replacements[managed] = original.replace('public ', 'private ', 1)
        body = members.get(bodies.get(bridge, ''), '')
        folded = re.search(r'\{\s*return (-?\d+(?:U|L|UL)?|true|false);\s*\}\s*\Z', body)
        primitive = return_type in ('sbyte', 'byte', 'short', 'ushort', 'int', 'uint', 'long', 'ulong', 'bool')
        declaration = 'public ' + ('' if instance else 'static ')
        invocation = managed + '(' + _parameters(parameters) + ')'
        if item['parameters'] is None and primitive and folded:
            declaration = 'public const ' + return_type + ' @' + name + ' = ' + folded[1] + ';'
            item['managedKind'] = 'const'
            item['constantValue'] = folded[1]
        elif item['parameters'] is None:
            if return_type == 'void':
                raise ValueError('object-like macro cannot export a void property: ' + name)
            declaration += return_type + ' @' + name + ' => ' + invocation + ';'
            item['managedKind'] = 'property'
        else:
            declaration += return_type + ' @' + name + '(' + parameters + ') => ' + invocation + ';'
            item['managedKind'] = 'method'
        item.update(managedName='@' + name, managedType=return_type)
        additions.append('// dot-gen:member-begin\n// dot-gen:origin @' + name + '\n' + declaration + '\n// dot-gen:member-end\n')
        records.append({'kind': 'macro', 'sourceFile': manifest['source'], 'sourceLine': 0, 'sourceColumn': 0,
                        'sourceName': name, 'managedName': '@' + name, 'scope': '', 'macroSnapshot': manifest['snapshot']})
    def substitute(match):
        origin = re.search(r'^// dot-gen:origin (@?\w+)\n', match[1], re.M)
        content = replacements.get(origin[1], match[1]) if origin else match[1]
        return '// dot-gen:member-begin\n' + content + '// dot-gen:member-end\n'
    source = member_pattern.sub(substitute, source)
    header = re.search(r'^public (?:static|sealed) unsafe partial class @?' + re.escape(class_name) + r'(?: : [^\n{]+)? \{\n', source, re.M)
    if not header:
        raise ValueError('cannot locate generated owner for macro APIs')
    source = source[:header.end()] + ''.join(additions) + source[header.end():]
    bridge_names = {item['bridge'] for item in manifest['macros']}
    exports[:] = [item for item in exports if item['symbol'] not in bridge_names]
    return source, records
