#!/usr/bin/env python3
"""Bind emitted libc partials to the existing, shared EDG BCL runtime sources.

Binding is signature checked. Unknown imports remain explicit failures, never
native-library fallbacks or fabricated success bodies. Runtime implementations
and allocation algorithms are reused from the direct backend's canonical source.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sys
from abi import Boundaries
from atomics import expression as atomic_expression

EDG = Path(__file__).resolve().parents[2]
BACKEND = EDG / 'backend'
RUNTIME = BACKEND / 'runtime'
FILE_SESSIONS = {
    '__edg_with_file_lock': ('WithFileLock', 'LibcFilesFormat', 'sbyte*'),
    '__edg_with_file_read_lock': ('WithFileReadLock', 'LibcFilesScan', 'sbyte*'),
    '__edg_with_file_wide_lock': ('WithFileWideLock', 'LibcFilesWideFormat', 'int*'),
    '__edg_with_file_wide_read_lock': ('WithFileWideReadLock', 'LibcFilesWideScan', 'int*'),
}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def split_parameters(text):
    values, begin, depth = [], 0, 0
    for index, char in enumerate(text):
        if char in '<([':
            depth += 1
        elif char in '>)]':
            depth -= 1
        elif char == ',' and depth == 0:
            values.append(text[begin:index].strip())
            begin = index + 1
        if depth < 0:
            raise ValueError('Unbalanced parameter type')
    if depth:
        raise ValueError('Unbalanced parameter type')
    if text[begin:].strip():
        values.append(text[begin:].strip())
    return values


def mappings():
    result, inputs = {}, []
    for path in sorted(RUNTIME.glob('libc*-bindings.def')):
        inputs.append(dict(path=str(path), sha256=sha(path)))
        for line in path.read_text().splitlines():
            found = re.fullmatch(r'EDG_LIBC\((.*)\)', line.strip())
            if not found:
                continue
            symbol, returns, parameters, target = json.loads('[' + found.group(1) + ']')
            key = (symbol, returns, tuple(split_parameters(parameters)))
            if key in result and result[key] != target:
                raise ValueError('Conflicting canonical runtime binding: ' + symbol)
            result[key] = target
    path = BACKEND / 'dot_gen_posix.h'
    inputs.append(dict(path=str(path), sha256=sha(path)))
    table = path.read_text().split('const posix_binding posix_bindings[]={', 1)[1].split('\n};', 1)[0]
    for line in table.splitlines():
        found = re.fullmatch(r'\s*\{(.*),(true|false)\},', line)
        if found:
            symbol, returns, parameters, target = json.loads('[' + found.group(1) + ']')
            result[(symbol, returns, tuple(split_parameters(parameters)))] = ('__edg_files.' if symbol in ('fdopen', 'fileno') else '__edg_posix.') + target
    # Same checked callback signatures as libc_format_lock_routine in the
    # direct backend. The implementation remains in shared LibcFiles partials.
    path = BACKEND / 'dot_gen_imports.h'
    inputs.append(dict(path=str(path), sha256=sha(path)))
    for symbol, (target, _, character) in FILE_SESSIONS.items():
        parameters = ('FILE*', 'delegate* managed<void*, ' + character + ', global::Edg.Runtime.VaList*, int>',
                      'void*', character, 'global::Edg.Runtime.VaList*')
        result[(symbol, 'int', parameters)] = '__edg_files.' + target
    return result, inputs


def helper_sources(names):
    # Reuse the direct backend's maintained runtime dependency graph.
    sys.path.insert(0, str(EDG))
    try:
        spec = importlib.util.spec_from_file_location('dotobj_direct_pipeline', EDG / 'dotnet.py')
        pipeline = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(pipeline)
    finally:
        sys.path.pop(0)
    selected, pending = set(), list(names)
    while pending:
        name = pending.pop()
        if name in selected:
            continue
        selected.add(name)
        pending += list(pipeline.RUNTIME_DEPENDENCIES.get(name, ()))
    return {name: RUNTIME / (name + '.cs') for name in sorted(selected)}


def libc_helpers(needed):
    """Select exact existing C# helper bodies, including their transitive calls."""
    imports = (BACKEND / 'dot_gen_imports.h').read_text()
    start = imports.index('void ensure_libc() {')
    start = imports.index('R"CS(', start) + len('R"CS(')
    end = imports.index(')CS";', start)
    code = imports[start:end].replace('@MOD@', 'static ')
    pieces = re.findall(r'// dot-gen:member-begin\n(.*?)// dot-gen:member-end', code, re.S)
    by_name = {}
    for piece in pieces:
        match = re.search(r'private static [^\n]+?\s+(__edg_\w+)\(', piece)
        if not match:
            raise ValueError('Canonical libc helper marker changed')
        by_name[match.group(1)] = piece
    selected, pending = set(), list(needed)
    while pending:
        name = pending.pop()
        if name in selected:
            continue
        if name not in by_name:
            raise ValueError('Missing canonical allocation helper: ' + name)
        selected.add(name)
        pending += [call for call in re.findall(r'\b(__edg_\w+)\(', by_name[name]) if call in by_name]
    result = 'internal static readonly global::Edg.Runtime.LibcContext __edg_libc = new();\n'
    if selected:
        lifecycle = (BACKEND / 'dot_gen_lifecycle.h').read_text()
        start = lifecycle.index('private sealed class __edg_heap_owner {')
        end = lifecycle.index('// dot-gen:member-end', start)
        result += lifecycle[start:end]
        result += 'private static readonly __edg_heap_owner __edg_heap = new();\n'
        result += '\n'.join(by_name[name].replace('private static ', 'internal static ', 1) for name in sorted(selected))
    return result


def service_sources(groups, allocation):
    groups = set(groups)
    if 'command' in groups:
        groups.add('files')
    if 'files' in groups:
        groups.update(('text', 'posix'))
    factories = {
        'text': ('LibcText', '__edg_libc.SetErrno'),
        'math': ('LibcMath', '__edg_libc.SetErrno'),
        'clock': ('LibcClock', '__edg_libc.SetErrno'),
        'environment': ('LibcEnvironment', ''),
        'posix': ('PosixContext', '__edg_libc.SetErrno'),
        'files': ('LibcFiles', '__edg_libc.SetErrno, getEncoding: () => __edg_text.MbCurMax(), fileDescriptors: __edg_posix'),
        'command': ('LibcCommand', '__edg_libc.SetErrno, __edg_files.WorkingDirectory'),
    }
    code = '// <auto-generated />\n#nullable enable\nnamespace Edg.Runtime;\ninternal static unsafe class DotObjServices\n{\n'
    code += libc_helpers(allocation)
    helpers = {'LibcContext'}
    for group in sorted(groups - {'libc'}):
        name, arguments = factories[group]
        helpers.add(name)
        code += 'private static readonly global::System.Lazy<global::Edg.Runtime.' + name + '> __edg_' + group + '_value = new(() => new(' + arguments + '));\n'
        code += 'internal static global::Edg.Runtime.' + name + ' __edg_' + group + ' => __edg_' + group + '_value.Value;\n'
    return code + '}\n', helpers


def provided_methods(project, requirements, paths):
    """Accept separately authored providers only for this exact linked contract."""
    keys = ('ExternalKey', 'Signature', 'container', 'generatedName')
    expected = {tuple(row[key] for key in keys) for row in requirements}
    provided, receipts = {}, []
    for path in paths:
        path = Path(path).resolve()
        receipt = json.loads(path.read_text())
        if receipt.get('schema') != 1 or receipt.get('status') != 'provided' or receipt.get('linkReportSha256') != sha(project / 'link-report.json'):
            raise ValueError('Provider receipt belongs to a different linked contract: ' + str(path))
        if not receipt.get('files'):
            raise ValueError('Provider receipt has no implementation files: ' + str(path))
        for name, digest in receipt['files'].items():
            if Path(name).name != name or not name.endswith('.cs') or sha(project / name) != digest:
                raise ValueError('Provider implementation changed or has an invalid path: ' + name)
        for method in receipt['methods']:
            key = tuple(method[column] for column in keys)
            if key not in expected or key in provided:
                raise ValueError('Provider requirement is absent or provided twice: ' + method['ExternalKey'])
            provided[key] = str(path)
        receipts.append(dict(path=str(path), sha256=sha(path)))
    return provided, receipts


def bind(project, allow_unresolved=False, provider_receipts=()):
    link = json.loads((project / 'link-report.json').read_text())
    provided, receipts = provided_methods(project, link['runtimeRequirements'], provider_receipts)
    boundaries = Boundaries(project, link)
    definitions, binding_inputs = mappings()
    methods, provider_bound = [], []
    for requirement in link['runtimeRequirements']:
        key = tuple(requirement[column] for column in ('ExternalKey', 'Signature', 'container', 'generatedName'))
        if key in provided:
            provider_bound.append(dict(name=requirement['ExternalKey'], providerReceipt=provided[key]))
            continue
        arguments = [(p['type'], p['name']) for p in requirement['parameters']]
        methods.append(dict(result=requirement['returnType'], name=requirement['generatedName'],
                            symbol=requirement['ExternalKey'], container=requirement['container'], arguments=arguments,
                            variadic=requirement['variadic'], variadicParameter=requirement['variadicParameter']))
    bound, unresolved, bodies, helpers, allocation, groups = provider_bound, [], {}, set(), set(), set()
    services = 'global::Edg.Runtime.DotObjServices.'
    for method in methods:
        atomic = atomic_expression(method)
        if atomic is not None:
            helpers.add('AtomicOps')
            bound.append(dict(name=method['symbol'], result=method['result'], parameters=[kind for kind, _ in method['arguments']],
                              target='global::Edg.Runtime.AtomicOps'))
            parameters = ', '.join(kind + ' ' + name for kind, name in method['arguments'])
            statement = ('' if method['result'] == 'void' else 'return ') + atomic + ';'
            bodies.setdefault(method['container'], []).append('public static partial ' + method['result'] + ' ' + method['name'] + '(' + parameters + ')\n    {\n        ' + statement + '\n    }\n')
            continue
        signature = (method['symbol'], boundaries.classify(method['result'])[0], tuple(boundaries.classify(t)[0] for t, _ in method['arguments']))
        target = definitions.get(signature)
        supported = target is not None and (target.startswith(('global::System.', 'global::Edg.Runtime.',
                    '__edg_libc.', '__edg_text.', '__edg_math.', '__edg_clock.', '__edg_environment.', '__edg_posix.', '__edg_files.', '__edg_command.')) or
                    target in ('__edg_malloc', '__edg_calloc', '__edg_realloc', '__edg_free', '__edg_aligned_alloc', '__edg_posix_memalign'))
        supported = supported and method['variadic'] == (signature[0] in ('open', 'fcntl', 'ioctl'))
        if not supported:
            unresolved.append(dict(name=signature[0], result=signature[1], parameters=signature[2], canonicalTarget=target))
            continue
        bound.append(dict(name=signature[0], result=signature[1], parameters=signature[2], target=target))
        if signature[0] in FILE_SESSIONS:
            helpers.add(FILE_SESSIONS[signature[0]][1])
        helpers.update(re.findall(r'global::Edg\.Runtime\.(\w+)\.', target))
        if target.startswith('__edg_'):
            if '.' in target:
                groups.add(target.split('.', 1)[0].removeprefix('__edg_'))
            else:
                allocation.add(target)
            target = services + target
        call_arguments = [('(' + boundaries.classify(kind)[1] + ')' if boundaries.classify(kind)[1] else '') + name for kind, name in method['arguments']]
        declaration_arguments = [kind + ' ' + name for kind, name in method['arguments']]
        if method['variadic']:
            extra = method['variadicParameter']
            declaration_arguments.append('params ' + extra['type'] + ' ' + extra['name'])
            call_arguments.append(extra['name'])
        if signature[0] in ('mmap', 'munmap'):
            call_arguments[1] = '(nuint)' + call_arguments[1]
        if signature[0] == 'realpath':
            call_arguments.append('size => (nint)' + services + '__edg_malloc(size)')
            allocation.add('__edg_malloc')
        before = ''
        if signature[0] == 'ioctl':
            groups.add('files')
            before = '_ = ' + services + '__edg_files;\n        '
        call = target + '(' + ', '.join(call_arguments) + ')'
        if boundaries.classify(method['result'])[1]:
            call = '(' + method['result'] + ')' + call
        statement = ('' if method['result'] == 'void' else 'return ') + call + ';'
        declaration = 'public static partial ' + method['result'] + ' ' + method['name'] + '(' + ', '.join(declaration_arguments) + ')'
        bodies.setdefault(method['container'], []).append(declaration + '\n    {\n        ' + before + statement + '\n    }\n')
    for storage in link.get('runtimeStorageRequirements', []):
        symbol = storage['ExternalKey']
        token, _ = boundaries.classify(storage['csharpType'])
        expression = None
        if symbol in ('stdin', 'stdout', 'stderr') and token == 'FILE*' and storage['size'] == 8 and storage['alignment'] <= 8 and not storage['isThreadLocal']:
            groups.add('files')
            expression = services + '__edg_files.' + {'stdin': 'StdinLocation', 'stdout': 'StdoutLocation', 'stderr': 'StderrLocation'}[symbol]
        elif symbol == 'errno' and token == 'int' and storage['size'] == 4 and storage['alignment'] <= 4:
            groups.add('libc')
            expression = services + '__edg_libc.ErrnoLocation()'
        if expression is None:
            unresolved.append(dict(name=symbol, storage=True, type=storage['csharpType']))
        else:
            bound.append(dict(name=symbol, storage=True, target=expression))
            bodies.setdefault(storage['container'], []).append('public static partial void* ' + storage['accessorName'] + '() => ' + expression + ';\n')
    files = {}
    for index, (container, methods) in enumerate(sorted(bodies.items())):
        namespace, name = container.removeprefix('global::').rsplit('.', 1)
        files['Runtime.Bindings.' + str(index) + '.generated.cs'] = '// <auto-generated />\n#nullable enable\nnamespace ' + namespace + ';\npublic static unsafe partial class ' + name + '\n{\n' + '\n'.join(methods) + '}\n'
    if groups or allocation:
        files['Runtime.Services.generated.cs'], service_helpers = service_sources(groups, allocation)
        helpers.update(service_helpers)
    sources = []
    reused = []
    for name, path in helper_sources(helpers).items():
        data = path.read_text()
        sources.append(dict(path=str(path), sha256=sha(path)))
        existing = project / ('Runtime.' + name + '.cs')
        if existing.exists():
            if existing.read_text() not in (data, '// <auto-generated />\n' + data, '// <auto-generated />\n#nullable enable\n' + data):
                raise ValueError('Existing shared runtime differs from canonical source: ' + str(existing))
            reused.append(dict(path=str(existing), sha256=sha(existing)))
            continue
        files['Runtime.Provider.' + name + '.cs'] = '// <auto-generated />\n' + data
    if groups or allocation:
        for path in (BACKEND / 'dot_gen_imports.h', BACKEND / 'dot_gen_lifecycle.h'):
            sources.append(dict(path=str(path), sha256=sha(path)))
    if 'AtomicOps' in helpers:
        path = BACKEND / 'dot_gen_atomics.h'
        binding_inputs.append(dict(path=str(path), sha256=sha(path)))
    report = dict(schema=1, status='bound' if not unresolved else 'unresolved', linkReport=dict(path=str(project / 'link-report.json'), sha256=sha(project / 'link-report.json')),
                  sourceMap=dict(path=str(project / link['sourceMap']), sha256=sha(project / link['sourceMap'])),
                  bindingInputs=binding_inputs, implementationInputs=sources, sourceDependencyGraph=dict(path=str(EDG / 'dotnet.py'), sha256=sha(EDG / 'dotnet.py')),
                  bound=bound, unresolved=unresolved, reusedFiles=reused, providerReceipts=receipts,
                  adapterInputs=[dict(path=str(path), sha256=sha(path)) for path in (Path(__file__), Path(__file__).with_name('abi.py'), Path(__file__).with_name('atomics.py'))],
                  runtimeMode='static translation; per-assembly libc state', executionVerified=False)
    report_path = project / 'runtime-bindings.json'
    if report_path.exists():
        raise ValueError('Runtime binding outputs already exist; retain this attempt and use a fresh linked project')
    if unresolved and not allow_unresolved:
        report_path.write_text(json.dumps(report, indent=2) + '\n')
        raise ValueError('Unresolved runtime imports: ' + ', '.join(x['name'] for x in unresolved))
    for name in files:
        if (project / name).exists():
            raise ValueError('Refusing to replace an existing runtime provider: ' + name)
    for name, data in files.items():
        (project / name).write_text(data)
    report['files'] = {name: sha(project / name) for name in files}
    report_path.write_text(json.dumps(report, indent=2) + '\n')
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--project', type=Path, required=True)
    parser.add_argument('--allow-unresolved', action='store_true', help='stage known bindings for review; unresolved partials still fail C# compilation')
    parser.add_argument('--provider-receipt', action='append', type=Path, default=[], help='validated receipt for project-specific partial implementations in this linked project')
    args = parser.parse_args()
    report = bind(args.project.resolve(), args.allow_unresolved, args.provider_receipt)
    print(json.dumps(dict(status=report['status'], bound=len(report['bound']), unresolved=len(report['unresolved']))))


if __name__ == '__main__':
    main()
