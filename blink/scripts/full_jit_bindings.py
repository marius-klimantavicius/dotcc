"""Operation-specific, predecoded Full-JIT bindings for the selected Blink core.

This is a source producer, never an instruction executor. ``adapt`` runs last in
an existing adapter chain. The only general selector it emits is compile-time
BlinkFullJitSelect. Runtime operation wrappers contain no fetch or GetOp call.
The caller owns decoded-xedd lifetime, issue/completion, IP/oplen and stash.

The selection is deliberately limited to the current portable managed profile;
a C preprocessor assertion fails closed if its feature selection changes. Other
host/platform conditionals remain in their original C source context.
"""
from __future__ import annotations

import ast
import hashlib
import itertools
import json
from pathlib import Path
import re
from dataclasses import dataclass

ROOT = Path(__file__).resolve().parents[1]
INPUTS = ('scripts/full_jit_bindings.py',)
_PROFILE = {'DISABLE_JIT': True, 'HAVE_JIT': False, 'DISABLE_METAL': True,
            'DISABLE_BCD': True, 'DISABLE_BMI2': True, 'DISABLE_X87': False,
            'DISABLE_MMX': False}
ASSERT_PROFILE = '''
#if !defined(DISABLE_JIT) || !defined(DISABLE_METAL) || !defined(DISABLE_BCD) || !defined(DISABLE_BMI2) || defined(DISABLE_X87) || defined(DISABLE_MMX)
#error Full JIT bindings require the reviewed portable x87/MMX managed profile
#endif
'''
# These accessors select semantic operations. Address/register helpers continue
# to consume immutable rde, without consulting guest instruction bytes.
DOMAINS = {'Rep': range(4), 'Osz': range(2), 'Rexw': range(2),
           'ModrmReg': range(8), 'ModrmMod': range(4), 'ModrmRm': range(8),
           'Rexb': range(2), 'IsModrmRegister': range(2)}
GROUPS = {'Op0f6', 'Op0f7', 'Op0ff', 'Op0fe', 'Op1ae', 'Op1c7', 'Op101',
          'Op171', 'Op172', 'Op173', 'OpAlui', 'OpBsuwiCl', 'OpBsuwi1',
          'OpBsuwiImm', 'OpBsubiCl', 'OpBsubi1', 'OpBsubiImm', 'OpFpu',
          'OpNopEv', 'OpBit'}
FORBIDDEN = {'ExecuteInstruction', 'JitlessDispatch', 'GeneralDispatch',
             'LoadInstruction', 'GetOp', 'Actor'}


def digest(value: str | bytes) -> str:
    return hashlib.sha256(value.encode() if isinstance(value, str) else value).hexdigest()


def clean(source: str) -> str:
    """Remove comments while preserving strings, line count and token boundaries."""
    pattern = r'"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|/\*[\s\S]*?\*/|//[^\n]*'
    return re.sub(pattern, lambda m: m[0] if m[0][0] in '\"\'' else
                  re.sub(r'[^\n]', ' ', m[0]), source)


def selected(source: str) -> str:
    """Evaluate only the explicit reviewed feature conditionals.

    Unknown host conditionals are retained, not guessed from the producer host.
    Their emitted bodies remain subject to the actual compiler preprocessor.
    """
    stack = []
    active = True
    result = []
    for line in source.splitlines(keepends=True):
        match = re.match(r'\s*#\s*(if|ifdef|ifndef|elif|else|endif)\b(.*)', line)
        if not match:
            if active:
                result.append(line)
            continue
        kind, expression = match.groups()
        expression = expression.strip()
        if kind in ('if', 'ifdef', 'ifndef'):
            value = None
            if kind in ('ifdef', 'ifndef') and expression in _PROFILE:
                value = _PROFILE[expression] != (kind == 'ifndef')
            elif kind == 'if':
                probe = expression
                for name, enabled in _PROFILE.items():
                    probe = re.sub(r'\bdefined\s*\(\s*' + name + r'\s*\)', str(int(enabled)), probe)
                    probe = re.sub(r'\b' + name + r'\b', str(int(enabled)), probe)
                value = number(probe)
                value = None if value is None else bool(value)
            stack.append((active, value, bool(value)))
            if value is None:
                if active:
                    result.append(line)
            else:
                active = active and value
        elif kind in ('else', 'elif'):
            if not stack:
                raise ValueError('Unbalanced preprocessor branch')
            outer, value, taken = stack[-1]
            if value is None:
                if outer:
                    result.append(line)
                active = outer
            elif kind == 'else':
                active = outer and not taken
                stack[-1] = (outer, value, True)
            else:
                raise ValueError('Feature #elif needs explicit review: ' + expression)
        else:
            if not stack:
                raise ValueError('Unbalanced preprocessor endif')
            outer, value, _ = stack.pop()
            if value is None and outer:
                result.append(line)
            active = outer
    if stack:
        raise ValueError('Unclosed preprocessor conditional')
    return ''.join(result)


def matching(source: str, start: int, opening='(', closing=')') -> int:
    if source[start] != opening:
        raise ValueError('Expected opening delimiter')
    depth = 0
    quote = None
    escaped = False
    for index in range(start, len(source)):
        c = source[index]
        if quote:
            if escaped:
                escaped = False
            elif c == '\\':
                escaped = True
            elif c == quote:
                quote = None
        elif c in '\"\'':
            quote = c
        elif c == opening:
            depth += 1
        elif c == closing:
            depth -= 1
            if not depth:
                return index
    raise ValueError('Unbalanced C delimiter')


def number(expression: str, constants=None) -> int | None:
    """Small fail-closed integer constant evaluator; never eval C/Python code."""
    value = expression.strip()
    constants = constants or {}
    value = re.sub(r'\b0([0-7]+)\b', lambda m: '0o' + m[1], value)
    value = re.sub(r'(?<=\d)[uUlL]+\b', '', value)
    value = re.sub(r'\((?:u32|u64|i64|unsigned|int|bool)\)', '', value)
    value = value.replace('&&', ' and ').replace('||', ' or ')
    value = re.sub(r'!(?!=)', ' not ', value)
    try:
        tree = ast.parse(value.strip(), mode='eval').body
        def walk(node):
            if isinstance(node, ast.Constant) and isinstance(node.value, (int, bool)):
                return int(node.value)
            if isinstance(node, ast.Name) and node.id in constants:
                return constants[node.id]
            if isinstance(node, ast.UnaryOp):
                x = walk(node.operand)
                return {ast.Not: lambda: int(not x), ast.Invert: lambda: ~x,
                        ast.USub: lambda: -x, ast.UAdd: lambda: x}[type(node.op)]()
            if isinstance(node, ast.BinOp):
                x, y = walk(node.left), walk(node.right)
                return {ast.Add: lambda: x + y, ast.Sub: lambda: x - y,
                        ast.Mult: lambda: x * y, ast.LShift: lambda: x << y,
                        ast.RShift: lambda: x >> y, ast.BitOr: lambda: x | y,
                        ast.BitAnd: lambda: x & y, ast.BitXor: lambda: x ^ y}[type(node.op)]()
            if isinstance(node, ast.BoolOp):
                xs = [walk(x) for x in node.values]
                return int(all(xs) if isinstance(node.op, ast.And) else any(xs))
            if isinstance(node, ast.Compare) and len(node.ops) == 1:
                x, y = walk(node.left), walk(node.comparators[0])
                return int({ast.Eq: lambda: x == y, ast.NotEq: lambda: x != y,
                            ast.Lt: lambda: x < y, ast.LtE: lambda: x <= y,
                            ast.Gt: lambda: x > y, ast.GtE: lambda: x >= y}[type(node.ops[0])]())
            raise ValueError('Not constant')
        return walk(tree)
    except (SyntaxError, ValueError, KeyError, TypeError):
        return None


@dataclass(frozen=True)
class Function:
    name: str
    prefix: str
    params: str
    body: str


def functions(source: str) -> dict[str, Function]:
    source = clean(selected(source))
    result = {}
    pattern = re.compile(r'^([\w \t*]+?)\b(\w+)\s*\((P(?:\s*,[^{};]*)?)\)\s*\{', re.M)
    for match in pattern.finditer(source):
        prefix, name, params = match.groups()
        end = matching(source, match.end() - 1, '{', '}')
        if name in result:
            raise ValueError('Ambiguous selected P definition: ' + name)
        result[name] = Function(name, prefix.strip(), params, source[match.end():end])
    return result


def dispatch(source: str) -> tuple[list[str], dict[int, str]]:
    source = clean(selected(source))
    match = re.search(r'static const nexgen32e_f kNexgen32e\[\]\s*=\s*\{([^}]+)\};', source)
    if not match:
        raise ValueError('Full-JIT dense dispatch declaration differs')
    dense = [x.strip() for x in match[1].split(',') if x.strip()]
    if not dense or any(not re.fullmatch(r'Op\w+', name) for name in dense):
        raise ValueError('Full-JIT dense dispatch has an unrecognized arm')
    start = source.index('nexgen32e_f GetOp(long op) {')
    end = matching(source, source.index('{', start), '{', '}')
    body = source[start:end + 1]
    expected = '''nexgen32e_f GetOp(long op) { if (op < ARRAYLEN(kNexgen32e)) {
      return kNexgen32e[op]; } else { switch (op) { SPARSE default: return OpUd; } } }'''
    sparse = {}
    def arm(match):
        op, name = int(match[1], 0), match[2]
        if op < len(dense) or op in sparse:
            raise ValueError('Duplicate/overlapping sparse dispatch')
        sparse[op] = name
        return ''
    residual = re.sub(r'XLAT\(\s*(0x[0-9a-fA-F]+)\s*,\s*(Op\w+)\s*\);', arm, body)
    if re.sub(r'\s+', '', residual) != re.sub(r'\s+', '', expected.replace('SPARSE', '')):
        raise ValueError('Full-JIT GetOp control flow differs')
    return dense, sparse


def upstream_machine() -> str:
    manifest = json.loads((ROOT / 'config/source-manifest.json').read_text())
    return (ROOT / 'ref' / manifest['upstream']['directory'] / 'blink/machine.c').read_text()


def primary_opcodes() -> dict[str, list[int]]:
    dense, sparse = dispatch(upstream_machine())
    result = {}
    for op, name in itertools.chain(enumerate(dense), sparse.items()):
        result.setdefault(name, []).append(op)
    return result


def expand_cases(body: str) -> str:
    pattern = re.compile(r'\bCASE\(')
    while match := pattern.search(body):
        end = matching(body, match.end() - 1)
        args = body[match.end():end]
        depth = 0
        split = None
        for i, c in enumerate(args):
            depth += c in '(['
            depth -= c in ')]'
            if c == ',' and not depth:
                split = i
                break
        if split is None:
            raise ValueError('Unrecognized CASE macro')
        body = body[:match.start()] + 'case ' + args[:split] + ': ' + args[split + 1:] + '; break' + body[end + 1:]
    return body


def statement_end(body: str, start: int) -> int:
    start = skip_space(body, start)
    if body[start] == '{':
        return matching(body, start, '{', '}') + 1
    if re.match(r'(if|switch|for|while)\b', body[start:]):
        opening = body.index('(', start)
        end = statement_end(body, matching(body, opening) + 1)
        if body[start:start + 2] == 'if':
            tail = skip_space(body, end)
            if re.match(r'else\b', body[tail:]):
                end = statement_end(body, tail + 4)
        return end
    index = start
    while index < len(body):
        if body[index] in '(\"\'':
            if body[index] == '(':
                index = matching(body, index)
            else:
                q = body[index]
                index += 1
                while index < len(body) and body[index] != q:
                    index += 2 if body[index] == '\\' else 1
        elif body[index] == ';':
            return index + 1
        index += 1
    raise ValueError('Unterminated C statement')


def skip_space(source: str, index: int) -> int:
    while index < len(source) and source[index].isspace():
        index += 1
    return index


def simplify(body: str, constants: dict[str, int]) -> str:
    """Remove constant dispatch; retain dynamic guest-value control flow.

    This is deliberately conservative: unrecognized expressions stay in source.
    Only complete constant if/switch statements are removed. Switch pruning
    handles fall-through labels, and declines bodies with nonterminal breaks.
    """
    body = expand_cases(body)
    # x87 uses a local opcode and memory-form variable for its dispatch key.
    env = dict(constants, true=1, false=0)
    straightline = re.split(r'\b(?:if|for|while|switch|do)\b', body, maxsplit=1)[0]
    for match in re.finditer(r'(?<![.>])\b(\w+)\s*=\s*([^;{}]+);', straightline):
        name, expression = match.groups()
        if (len(re.findall(r'(?<![.>])\b' + re.escape(name) + r'\s*=(?!=)', body)) == 1
                and not re.search(r'\b' + re.escape(name) + r'\s*(?:\+\+|--|[-+*/&|^%]=)', body)
                and not re.search(r'(?:\+\+|--)\s*\b' + re.escape(name) + r'\b', body)):
            value = constant_expression(expression, env)
            if value is not None:
                env[name] = value
    out = []
    position = 0
    keyword = re.compile(r'\b(if|switch)\s*\(')
    while match := keyword.search(body, position):
        opening = body.index('(', match.start())
        close = matching(body, opening)
        value = constant_expression(body[opening + 1:close], env)
        start = skip_space(body, close + 1)
        if value is None or start >= len(body):
            out.append(body[position:close + 1])
            position = close + 1
            continue
        end = statement_end(body, start)
        if match[1] == 'if':
            other = skip_space(body, end)
            else_end = end
            false_body = ''
            if re.match(r'else\b', body[other:]):
                else_start = skip_space(body, other + 4)
                else_end = statement_end(body, else_start)
                false_body = body[else_start:else_end]
            chosen = body[start:end] if value else false_body
            out.append(body[position:match.start()] + simplify(chosen, env))
            position = else_end
        else:
            if body[start] != '{':
                raise ValueError('Switch without compound body')
            inner = body[start + 1:end - 1]
            labels = []
            depth = 0
            # Case labels only at this switch's outer nesting level.
            tokens = re.compile(r'[{}]|\bcase\s+([^:]+):|\bdefault\s*:')
            for token in tokens.finditer(inner):
                if token[0] == '{':
                    depth += 1
                elif token[0] == '}':
                    depth -= 1
                elif depth == 0:
                    key = None if token[0].startswith('default') else constant_expression(token[1], env)
                    if key is None and not token[0].startswith('default'):
                        labels = []
                        break
                    labels.append((key, token.start(), token.end()))
            index = next((i for i, row in enumerate(labels) if row[0] == value), None)
            if index is None:
                index = next((i for i, row in enumerate(labels) if row[0] is None), None)
            chosen = ''
            if labels and index is not None:
                # Honor consecutive labels/fall-through up to the first break.
                for i in range(index, len(labels)):
                    part = inner[labels[i][2]:labels[i + 1][1] if i + 1 < len(labels) else len(inner)]
                    chosen += part
                    if re.search(r'\bbreak\s*;\s*$', part):
                        chosen = re.sub(r'\bbreak\s*;\s*$', '', chosen)
                        break
                if re.search(r'\bbreak\b', chosen):
                    labels = []  # Not a safely detachable switch arm.
            if not labels:
                out.append(body[position:end])
            else:
                out.append(body[position:match.start()] + '{' + simplify(chosen, env) + '}')
            position = end
    out.append(body[position:])
    return ''.join(out)


def constant_expression(expression: str, constants: dict[str, int]) -> int | None:
    # The sole selected function-like dispatch-key macro outside rde.h.
    while match := re.search(r'\bDISP\(([^()]*)\)', expression):
        xs = [constant_expression(x, constants) for x in match[1].split(',')]
        if len(xs) != 3 or any(x is None for x in xs):
            return None
        expression = expression[:match.start()] + str(((xs[0] & 7) << 4) | (xs[1] << 3) | xs[2]) + expression[match.end():]
    return number(expression, constants)


class Producer:
    def __init__(self, path: str, source: str):
        self.path, self.original = path, source
        self.source = selected(source)
        self.functions = functions(source)
        self.opcodes = primary_opcodes()
        self.aliases = dict(re.findall(r'^#define\s+(Op\w+)\s+(Op\w+)\s*$', self.source, re.M))
        for alias, target in self.aliases.items():
            if alias in self.opcodes:
                self.opcodes.setdefault(target, []).extend(self.opcodes[alias])
        self.roots = sorted(set(self.functions) & set(self.opcodes))
        self.generated = {}
        self.prototypes = {}
        self.origins = {}
        self.variants = {}
        self.constants = {'FPUREG': 0, 'MEMORY': 1, 'ALU_CMP': 7, 'ALU_SUB': 5,
                          'ALU_AND': 4}
        for key, value in re.findall(r'^#define\s+(\w+)\s+(\d+)\s*$', self.source, re.M):
            self.constants[key] = int(value)
        manifest = json.loads((ROOT / 'config/source-manifest.json').read_text())
        string_header = ROOT / 'ref' / manifest['upstream']['directory'] / 'blink/string.h'
        for key, value in re.findall(r'^#define\s+(STRING_\w+)\s+(\d+)\s*$', string_header.read_text(), re.M):
            self.constants[key] = int(value)

    def closure(self, root: str) -> set[str]:
        result = set()
        pending = [root]
        while pending:
            name = pending.pop()
            if name in result:
                continue
            result.add(name)
            body = self.functions[name].body
            calls = set(re.findall(r'\b(\w+)\s*\(', body))
            if calls & FORBIDDEN:
                raise ValueError('Semantic helper reaches interpreter: ' + name)
            pending.extend((calls & self.functions.keys()) - result)
        return result

    def fields(self, root: str) -> tuple[str, ...]:
        closure = self.closure(root)
        bodies = '\n'.join(self.functions[name].body for name in sorted(closure))
        fields = [name for name in ('Rep', 'Osz') if re.search(r'\b' + name + r'\(rde\)', bodies)]
        if root in GROUPS:
            fields += [name for name in ('ModrmReg', 'ModrmMod', 'ModrmRm', 'IsModrmRegister')
                       if re.search(r'\b' + name + r'\(rde\)', bodies)]
            # The register index is an operand for ALU/bit/shift families.
            if root not in ('OpFpu', 'Op101', 'OpNopEv'):
                fields = [name for name in fields if name not in ('ModrmMod', 'ModrmRm')]
            # IsModrmRegister is derived from ModrmMod; do not enumerate
            # impossible combinations of the same decoded field.
            if 'ModrmMod' in fields:
                fields = [name for name in fields if name != 'IsModrmRegister']
        if root == 'Op1c7':
            fields.append('Rexw')
        if root == 'OpNop':
            fields.append('Rexb')
        if re.search(r'\b(?:Mopcode|Opcode)\(rde\)', bodies):
            fields.append('Mopcode')
        return tuple(fields)

    def clone(self, name: str, frozen: dict[str, int], ancestors=(), bound_operation=None) -> str:
        if name in ancestors:
            raise ValueError('Recursive semantic clone needs review: ' + name)
        fn = self.functions[name]
        body = fn.body
        params = fn.params
        if bound_operation is not None:
            if name not in ('OpCvt', 'StringOp') or not re.fullmatch(r'P,\s*(?:unsigned long|int)\s+op', params):
                raise ValueError('Unrecognized shared semantic operation parameter: ' + name)
            body = re.sub(r'\bop\b', str(bound_operation), body)
            params = 'P'
        for field, value in frozen.items():
            body = re.sub(r'\b' + field + r'\(rde\)', str(value), body)
        if 'Mopcode' in frozen:
            body = re.sub(r'\bOpcode\(rde\)', str(frozen['Mopcode'] & 255), body)
        if 'ModrmMod' in frozen:
            body = re.sub(r'\bIsModrmRegister\(rde\)', str(int(frozen['ModrmMod'] == 3)), body)
        # Selected ModRM dispatch arrays become direct semantic calls.
        arrays = re.findall(r'static const nexgen32e_f\s+(kOp\w+)\[\]\s*=\s*\{([^}]+)\}', clean(self.source))
        for array, initializer in arrays:
            targets = re.findall(r'\bOp\w+\b', initializer)
            if len(targets) != 8:
                raise ValueError('Subgroup dispatch array size differs: ' + array)
            body = re.sub(r'\b' + array + r'\[(\d+)\]\(A\)', lambda m: targets[int(m[1])] + '(A)', body)
        # A first constant branch can expose a straight-line assignment used by
        # a later subgroup switch (e.g. BT/BTS/BTR/BTC's local `op`).
        body = simplify(simplify(body, self.constants), self.constants)
        for callee in sorted(set(re.findall(r'\b(\w+)\s*\(', body)) & self.functions.keys()):
            if callee in ('OpCvt', 'StringOp'):
                pattern = re.compile(r'\b' + callee + r'\(A,\s*(\w+)\s*\)')
                def specialize(match):
                    operation = self.constants.get(match[1])
                    if operation is None:
                        raise ValueError('Unresolved shared semantic operation: ' + match[0])
                    target = self.clone(callee, frozen, ancestors + (name,), bound_operation=operation)
                    return target + '(A)'
                body = pattern.sub(specialize, body)
                if re.search(r'\b' + callee + r'\s*\(', body):
                    raise ValueError('Unfrozen shared semantic operation: ' + callee)
                continue
            target = self.clone(callee, frozen, ancestors + (name,))
            body = re.sub(r'\b' + callee + r'(?=\s*\()', target, body)
        if re.search(r'\b(?:Opcode|Mopcode|GetOp|LoadInstruction|JitlessDispatch|ExecuteInstruction)\s*\(', body):
            raise ValueError('Unfrozen opcode/fetch dispatch: ' + name)
        key = (fn.prefix, params, re.sub(r'\s+', ' ', body).strip())
        identity = digest(json.dumps(key))[:20]
        target = 'BlinkFullJitOp_' + Path(self.path).stem + '_' + identity
        # All clones stay within their original translation unit, so internal
        # declarations, original static tables and helper visibility are valid.
        prefix = re.sub(r'\b(static|relegated|optimizesize|dontinline|forceinline)\b', '', fn.prefix).strip()
        declaration = 'static ' + prefix + ' ' + target + '(' + params + ')'
        self.prototypes[target] = declaration + ';\n'
        self.generated[target] = declaration + ' {' + body + '\n}\n'
        self.origins.setdefault(target, set()).add(name)
        return target

    def emit(self) -> tuple[str, dict]:
        selectors = []
        rows = []
        for root in self.roots:
            fields = self.fields(root)
            variants = []
            domains = [sorted(set(self.opcodes[root])) if field == 'Mopcode' else DOMAINS[field] for field in fields]
            for values in itertools.product(*domains):
                frozen = dict(zip(fields, values))
                wrapper = self.clone(root, frozen)
                variants.append((values, wrapper))
            name = 'BlinkFullJitChoose_' + root
            selector = 'nexgen32e_f ' + name + '(u64 rde, i64 disp, u64 uimm0) {\n'
            # A flat switch has a one-to-one finite key, with no runtime helper
            # selector: this function is called solely while compiling.
            key = '0'
            multipliers = []
            multiplier = 1
            for field in fields:
                multipliers.append(multiplier)
                key += ' + ' + field + '(rde) * ' + str(multiplier)
                multiplier *= 2048 if field == 'Mopcode' else len(DOMAINS[field])
            if fields:
                selector += '  switch (' + key + ') {\n'
                for values, wrapper in variants:
                    selector += '    case ' + str(sum(x * y for x, y in zip(values, multipliers))) + ': return ' + wrapper + ';\n'
                selector += '    default: return 0;\n  }\n'
            else:
                selector += '  return ' + variants[0][1] + ';\n'
            selectors.append(selector + '}\n')
            rows.append(dict(handler=root, selector=name, fields=list(fields),
                             variants=[dict(values=dict(zip(fields, values)), wrapper=wrapper) for values, wrapper in variants]))
        text = ASSERT_PROFILE + '\n/* Full-JIT operation-specific semantic clones. */\n'
        text += ''.join(self.prototypes[name] for name in sorted(self.prototypes))
        text += ''.join(self.generated[name] for name in sorted(self.generated))
        text += ''.join(selectors)
        self.variants = {row['handler']: row for row in rows}
        inventory = dict(path=self.path, source_sha256=digest(self.original),
                         selected_sha256=digest(self.source), roots=rows,
                         wrappers=len(self.generated), generated_sha256=digest(text),
                         wrapper_inventory={name: dict(
                             source_functions=sorted(self.origins[name]),
                             body_sha256=digest(body),
                             calls=sorted(set(re.findall(r'\b(\w+)\s*\(', body)) - {name}),
                             retained_decoded_fields=sorted(set(re.findall(r'\b(\w+)\(rde\)', body))))
                             for name, body in sorted(self.generated.items())},
                         functions={name: dict(signature=fn.prefix + ' ' + name + '(' + fn.params + ')',
                                               body_sha256=digest(fn.body))
                                    for name, fn in sorted(self.functions.items())})
        return text, inventory


def adapt(path: str, content: bytes) -> bytes:
    if not path.startswith('blink/') or not path.endswith('.c'):
        return content
    if b'/* Full-JIT operation-specific semantic clones. */' in content:
        raise ValueError('Full-JIT adapter applied twice')
    producer = Producer(path, content.decode())
    if not producer.roots:
        return content
    suffix, _ = producer.emit()
    source = content.decode()
    if path == 'blink/machine.c':
        dense, sparse = dispatch(source)
        if (dense, sparse) != dispatch(upstream_machine()):
            raise ValueError('Selected dispatch changed; update producer contract')
        for before, after in (
            ('nexgen32e_f GetOp(long op) {', 'nexgen32e_f GetOp(long op) {\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  if (g_machine) blink_host_guest_interpreter_dispatch(g_machine);\n#endif'),
            ('void JitlessDispatch(P) {', 'void JitlessDispatch(P) {\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  blink_host_guest_interpreter_dispatch(m);\n#endif'),
            ('void ExecuteInstruction(struct Machine *m) {', 'void ExecuteInstruction(struct Machine *m) {\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  blink_host_guest_interpreter_dispatch(m);\n#endif')):
            if source.count(before) != 1:
                raise ValueError('Full-JIT guard anchor differs: ' + before)
            source = source.replace(before, after, 1)
        names = sorted(set(dense) | set(sparse.values()) | {'OpUd'})
        declarations = []
        for name in names:
            target = producer.aliases.get(name, name)
            declarations.append('nexgen32e_f BlinkFullJitChoose_' + target + '(u64, i64, u64);\n')
            if target != name:
                declarations.append('#define BlinkFullJitChoose_' + name + ' BlinkFullJitChoose_' + target + '\n')
        selector = 'nexgen32e_f BlinkFullJitSelect(u64 rde, i64 disp, u64 uimm0) {\n  switch (Mopcode(rde)) {\n'
        for op, name in itertools.chain(enumerate(dense), sorted(sparse.items())):
            selector += '    case ' + hex(op) + ': return BlinkFullJitChoose_' + name + '(rde, disp, uimm0);\n'
        selector += '    default: return BlinkFullJitChoose_OpUd(rde, disp, uimm0);\n  }\n}\n'
        suffix = ''.join(declarations) + suffix + selector
    return (source + '\n' + suffix).encode()


def inventory(sources: dict[str, bytes]) -> dict:
    """Enumerate selected arms and every frozen selector tuple, fail closed.

    Sources must be the actual adapter-chain outputs *before* this adapter.
    The result is a producer manifest, not a guest ISA conformance claim.
    """
    if 'blink/machine.c' not in sources:
        raise ValueError('Inventory requires the actual selected machine.c')
    machine = Producer('blink/machine.c', sources['blink/machine.c'].decode())
    dense, sparse = dispatch(machine.original)
    units = []
    handlers = {}
    closure = {}
    for path, content in sorted(sources.items()):
        producer = Producer(path, content.decode())
        closure[path] = dict(source_sha256=digest(content),
                             selected_sha256=digest(producer.source),
                             typed_handlers={name: dict(signature=fn.prefix + ' ' + name + '(' + fn.params + ')',
                                                        body_sha256=digest(fn.body))
                                             for name, fn in sorted(producer.functions.items())})
        if not producer.roots:
            continue
        _, row = producer.emit()
        units.append(row)
        for root in row['roots']:
            if root['handler'] in handlers:
                raise ValueError('Duplicate selected semantic handler: ' + root['handler'])
            handlers[root['handler']] = path
    arms = []
    for op, name in itertools.chain(enumerate(dense), sorted(sparse.items()), [(None, 'OpUd')]):
        target = machine.aliases.get(name, name)
        if target not in handlers:
            raise ValueError('Missing selected handler implementation: ' + target)
        arms.append(dict(opcode=op, opcode_hex=None if op is None else hex(op),
                         source_handler=name, selected_handler=target, source=handlers[target],
                         selector='BlinkFullJitChoose_' + target,
                         disposition='guest-invalid' if target == 'OpUd' else 'semantic-helper'))
    return dict(schema=1, kind='full-jit-selected-dispatch-inventory',
                features=_PROFILE, dense_arms=len(dense), sparse_arms=len(sparse),
                default='OpUd', dispatch=arms, units=units,
                source_closure=closure,
                decoded_dimensions=dict(
                    machine_mode=dict(omode=[2], name='XED_MODE_LONG', reason='DISABLE_METAL'),
                    effective_address_mode=dict(values=[1, 2], names=['XED_MODE_LEGACY', 'XED_MODE_LONG'],
                                                excluded=dict(value=0, reason='Eamode asserts unreachable under DISABLE_METAL')),
                    modrm_mod=[0, 1, 2, 3], modrm_reg=list(range(8)), modrm_rm=list(range(8)),
                    rep=list(range(4)), osz=[0, 1], rexw=[0, 1], lock=[0, 1],
                    reg_log2=[0, 1, 2, 3], word_log2=[0, 1, 2, 3],
                    interpretation='Frozen selector tuples cover a conservative superset; remaining fields are immutable operands/form/width/address/atomic semantics. Decoder constraints still govern valid tuples.'),
                predecoded_contract=dict(runtime_fetch=False, runtime_general_opcode_dispatch=False,
                                         xedd='caller-owned decoded snapshot',
                                         pre_post='caller owns IP/oplen/stash and issue/completion',
                                         allowed_modes='all modes selected by existing handlers; mode/address/width semantics retained'))
