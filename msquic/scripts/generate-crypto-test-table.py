#!/usr/bin/env python3
"""Generate test-only typed fail-fast callbacks from the emitted public host ABI."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
generated = ROOT / 'generated/TranslatedMsQuic'
sources = (generated / 'Dotcc.SourceFiles.txt').read_text().splitlines()
tables = []
for name in sources:
    source = (generated / name).read_text()
    match = re.search(r'(?m)^(?P<indent>[ \t]*)public unsafe struct MSQUIC_HOST_TABLE\s*\{(?P<body>.*?)\n(?P=indent)\}', source, re.S)
    if match:
        tables.append(match.group('body'))
if len(tables) != 1:
    raise RuntimeError('Expected exactly one host table declaration')
callbacks = re.findall(r'public delegate\*<(.+?)> (\w+);', tables[0])
if len(callbacks) != 99:
    raise RuntimeError('Host operation inventory changed; review test generator')


def split_types(text):
    parts, start, depth = [], 0, 0
    for i, character in enumerate(text):
        if character == '<': depth += 1
        elif character == '>': depth -= 1
        elif character == ',' and depth == 0:
            parts.append(text[start:i].strip()); start = i + 1
    parts.append(text[start:].strip())
    return parts


lines = ['// Generated test-only fail-fast table; never compiled into the product host.',
         'using static Managed.Transport.MsQuic;\nusing static Managed.Transport.MsQuic.Libc;',
         'namespace Managed.Transport.Hosting;',
         'public sealed unsafe partial class MsQuicHost', '{',
         '    private static void RegisterUnexercised(ref MSQUIC_HOST_TABLE table)', '    {']
for signature, name in callbacks:
    lines.append(f'        table.{name} = &Unexercised_{name};')
lines.append('    }')
lines.append('    internal static void CombineIv(byte* iv, byte* number, byte* output) => '
             'QuicCryptoCombineIvAndPacketNumber(iv, number, output);')
for signature, name in callbacks:
    types = split_types(signature)
    args = ', '.join(f'{kind} arg{i}' for i, kind in enumerate(types[:-1]))
    lines.extend([f'    private static {types[-1]} Unexercised_{name}({args})', '    {',
                  f'        FatalInvariant("Unexercised packet-test operation: {name}");'])
    if types[-1] != 'void': lines.append('        return default;')
    lines.append('    }')
lines.append('}')
output = ROOT / 'build/packet-crypto-common/Unexercised.cs'
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text('\n'.join(lines) + '\n')
print('Generated 99 typed test-only failure callbacks')
