#!/usr/bin/env python3
"""Compare native product offsets to metadata and generate real storage checks.

The generated code is a separate consumer of public product types. It neither
edits the engine nor uses its offset constants as the expected oracle values.
"""
import argparse
import base64
from pathlib import Path
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("native", type=Path)
parser.add_argument("engine", type=Path)
parser.add_argument("output", type=Path)
args = parser.parse_args()
source = args.engine.read_text()


def decode(value):
    return base64.b64decode(value, validate=True).decode()


def emitted(name):
    name = {"Mem": "sqlite3_value"}.get(name, name)
    if not re.fullmatch(r"[A-Za-z_]\w*", name):
        raise SystemExit("Unsupported aggregate name: " + name)
    return name


requests = {}
descriptions = {}
for block in re.findall(r"/\* dotcc-layout-v1\n(.*?)end-dotcc-layout \*/", source, re.S):
    aggregate = None
    for line in block.splitlines():
        parts = line.split("\t")
        if parts[0] == "aggregate":
            aggregate = decode(parts[1])
        elif parts[0] == "field" and aggregate is not None:
            key = aggregate, decode(parts[1])
            description = decode(parts[2])
            if key in descriptions and descriptions[key] != description:
                raise SystemExit(f"Conflicting metadata field type: {key}")
            descriptions[key] = description
        elif parts[0] == "request":
            _, helper, owner, member, offset = parts
            key = decode(owner), decode(member)
            request = helper, int(offset)
            if key in requests and requests[key] != request:
                raise SystemExit(f"Conflicting metadata request: {key}")
            requests[key] = request


def struct_body(name):
    match = re.search(r"\bpublic\s+(?:unsafe\s+)?(?:partial\s+)?struct\s+" + re.escape(name) + r"\s*\{", source)
    if not match:
        raise SystemExit("Missing public product aggregate: " + name)
    cursor, depth = match.end(), 1
    while depth and cursor < len(source):
        depth += (source[cursor] == "{") - (source[cursor] == "}")
        cursor += 1
    if depth:
        raise SystemExit("Unterminated product aggregate: " + name)
    return source[match.end():cursor - 1]


offsets, arrays, required = {}, {}, set()
for line in args.native.read_text().splitlines():
    parts = line.split()
    if not parts:
        continue
    if parts[0] not in ("header", "field", "required", "array"):
        raise SystemExit("Unexpected native transcript line: " + line)
    if parts[1] in ("struct", "union"):
        parts[1:3] = [parts[2]]
    kind, owner = parts[:2]
    owner = emitted(owner)
    if kind == "header":
        continue
    member = parts[2]
    if not re.fullmatch(r"[A-Za-z_]\w*(?:\.[A-Za-z_]\w*|\[\d+\])*", member):
        raise SystemExit("Unsupported native member path: " + member)
    key = owner, member
    if kind == "array":
        size, alignment, span, element, count, first, last, first_ok, last_ok = map(int, parts[3:])
        if element != 8 or count < 2 or first_ok != 1 or last_ok != 1:
            raise SystemExit("Unexpected native pointer-array representation: " + line)
        arrays[key] = count, first, last
        continue
    if kind == "required":
        size, alignment, offset, address = map(int, parts[3:])
        required.add(key)
        if key not in requests:
            raise SystemExit(f"Missing product offsetof metadata: {key}")
        helper, value = requests[key]
        body = re.search(r"internal static class " + re.escape(helper) + r"\s*\{(.*?)\}", source, re.S)
        if not body:
            raise SystemExit("Missing emitted constant helper: " + helper)
        constants = []
        for label in ("Size", "Alignment", "Value"):
            constant = re.search(r"\b" + label + r"\s*=\s*(\d+)", body[1])
            if not constant:
                raise SystemExit("Missing layout constant " + helper + "." + label)
            constants.append(int(constant[1]))
        if constants != [size, alignment, offset] or value != offset:
            raise SystemExit(f"Product metadata mismatch {owner}.{member}: native {(size, alignment, offset)}, emitted {constants}, request {value}")
    else:
        offset, address = map(int, parts[3:])
    if offset != address:
        raise SystemExit("Native offsetof/address mismatch: " + line)
    if key in offsets and offsets[key] != offset:
        raise SystemExit(f"Conflicting native offsets: {key}")
    offsets[key] = offset

if not required or not arrays:
    raise SystemExit("Native product transcript lacks required offsets or arrays")
if required != set(requests):
    raise SystemExit(f"Product offsetof request coverage mismatch: missing={set(requests) - required}, extra={required - set(requests)}")

lines = ["// Independent test consumer generated from native product-profile measurements.",
         "internal static unsafe class ProductOffsetChecks", "{",
         "    internal static void Check()", "    {"]
for index, ((owner, member), expected) in enumerate(sorted(offsets.items())):
    variable = f"value{index}"
    expression = f"{variable}.{member}"
    description = descriptions.get((owner, member), "")
    # Flexible arrays are pointer getters into the header's trailing storage;
    # fixed buffers decay to their first element. Ordinary fields use &field.
    if description.startswith("a:0:"):
        address = f"(byte*)({expression})"
    elif re.fullmatch(r"[A-Za-z_]\w*", member) and re.search(
            r"\bpublic\s+fixed\s+\S+\s+" + re.escape(member) + r"\[", struct_body(owner)):
        address = f"(byte*)({expression})"
    else:
        address = f"(byte*)&({expression})"
    lines += [f"        {owner} {variable} = default;",
              f"        long offset{index} = {address} - (byte*)&{variable};",
              f"        if (offset{index} != {expected})",
              f'            throw new System.InvalidOperationException($"Product offset {owner}.{member}: expected {expected}, got {{offset{index}}}");']
for index, ((owner, member), (count, first, last)) in enumerate(sorted(arrays.items())):
    variable = f"array{index}"
    lines += [f"        {owner} {variable} = default;",
              f"        byte firstMarker{index} = 17, lastMarker{index} = 42;",
              f"        nint* first{index} = (nint*)&{variable}.{member}[0];",
              f"        nint* last{index} = (nint*)&{variable}.{member}[{count - 1}];",
              f"        if ((byte*)first{index} - (byte*)&{variable} != {first} || (byte*)last{index} - (byte*)&{variable} != {last})",
              f'            throw new System.InvalidOperationException("Product pointer-array offsets: {owner}.{member}");',
              f"        *first{index} = (nint)(&firstMarker{index}); *last{index} = (nint)(&lastMarker{index});",
              f"        if (*first{index} != (nint)(&firstMarker{index}) || *last{index} != (nint)(&lastMarker{index}))",
              f'            throw new System.InvalidOperationException("Product pointer-array storage: {owner}.{member}");']
lines += [f'        System.Console.WriteLine("PASS {len(required)} product offsetof contracts, {len(offsets)} actual field offsets and {len(arrays)} pointer-array first/last writes");',
          "    }", "}", ""]
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text("\n".join(lines))
print(f"PASS {len(required)} product offsetof metadata contracts match the exact native profile")
print(f"Generated {len(offsets)} actual field-offset and {len(arrays)} pointer-array storage checks")
