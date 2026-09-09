#!/usr/bin/env python3
"""Generate independent unsafe storage checks from the native layout transcript.

This is test code, not a repair of translated output. C# wrappers can contain
translated flexible-array headers; equivalent nested headers are not standard C.
"""
import argparse
from pathlib import Path
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("native", type=Path)
parser.add_argument("output", type=Path)
args = parser.parse_args()
types = {}
arrays = {}
for line in args.native.read_text().splitlines():
    fields = line.split()
    if fields[0] not in ("header", "required", "array"):
        continue
    # The native spelling may include a struct/union tag keyword.
    if fields[1] in ("struct", "union"):
        fields[1:3] = [fields[2]]
    kind, name = fields[:2]
    start = 2 if kind == "header" else 3
    size, alignment = map(int, fields[start:start + 2])
    # Mem is SQLite's typedef of struct sqlite3_value; emitted names use tags.
    emitted = {"Mem": "sqlite3_value"}.get(name, name)
    if not re.fullmatch(r"[A-Za-z_]\w*", emitted):
        raise SystemExit(f"Invalid aggregate name: {name}")
    layout = (size, alignment)
    if emitted in types and types[emitted] != layout:
        raise SystemExit(f"Conflicting native layout: {name}")
    types[emitted] = layout
    if kind == "array":
        member = fields[2]
        if not re.fullmatch(r"[A-Za-z_]\w*", member):
            raise SystemExit(f"Invalid array member: {member}")
        array_size, element_size, count, first, last, first_ok, last_ok = map(int, fields[5:])
        if (array_size != element_size * count or last - first != (count - 1) * element_size
                or first + array_size > size or first_ok != 1 or last_ok != 1):
            raise SystemExit(f"Native pointer-array storage check failed: {name}.{member}")
        arrays[(emitted, member)] = array_size
if not types or not arrays:
    raise SystemExit("Native transcript lacks aggregate or pointer-array checks")

source = ["// Generated test sidecar from the native layout oracle; engine output is untouched.",
          "internal static unsafe class LayoutStorageChecks", "{"]
for name in sorted(types):
    source.append(f"    private struct Prefix_{name} {{ public byte Prefix; public {name} Value; }}")
source += ["    [System.Runtime.CompilerServices.ModuleInitializer]",
           "    internal static void Check()", "    {"]
for name, (size, alignment) in sorted(types.items()):
    source += [f"        Prefix_{name} probe_{name} = default;",
               f"        long alignment_{name} = (byte*)&probe_{name}.Value - (byte*)&probe_{name};",
               f"        if (sizeof({name}) != {size} || alignment_{name} != {alignment})",
               f'            throw new System.InvalidOperationException($"Native layout mismatch: {name}, size={{sizeof({name})}}, alignment={{alignment_{name}}}");']
for (name, member), size in sorted(arrays.items()):
    wrapper = f"__IA_{name}_{member}"
    source += [f"        if (sizeof({wrapper}) != {size})",
               f'            throw new System.InvalidOperationException($"Native inline-array size mismatch: {name}.{member}, size={{sizeof({wrapper})}}");']
source += ["    }", "}", ""]
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text("\n".join(source))
print(f"Generated {len(types)} actual aggregate size/alignment and {len(arrays)} inline-array size checks")
