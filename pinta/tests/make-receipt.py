#!/usr/bin/env python3
"""Assemble one fixed receipt fixture; this is not a source-language compiler.

Layout/opcodes come from the pinned pinta.h and code.c. All operands fit the
single-byte ULEB128 encoding. Byte offsets in records are absolute and little-endian.
"""
import hashlib
import json
from pathlib import Path
import struct

root = Path(__file__).resolve().parent / "fixtures"
root.mkdir(exist_ok=True)
strings = ["customer", "quantity", "unitPrice", "total", "Customer: ", "\nTotal: ", "\n"]
data = bytearray(84)
offsets = []
for value in strings:
    offsets.append(len(data))
    encoded = value.encode("ascii")
    data.extend(bytes([len(encoded)]) + encoded + b"\0")
def align():
    data.extend(b"\0" * (-len(data) % 4))
align()
strings_offset = len(data)
data.extend(struct.pack("<7I", *offsets))
globals_offset = len(data)
data.extend(struct.pack("<4I", 0, 1, 2, 3))
code_offset = len(data)
# total = quantity * unitPrice; output literal/customer/literal/total/newline.
code = bytearray([0x29, 1, 0x29, 2, 0x03, 0x25, 3])
for opcode, token in [(0x23, 4), (0x29, 0), (0x23, 5), (0x29, 3), (0x23, 6)]:
    code.extend([opcode, token, 0x13, 0x1b, 0, 1, 0x2d])
code.append(0x2e)
data.extend(code)
align()
functions_offset = len(data)
data.extend(struct.pack("<5I", 0xffffffff, 0, 0, len(code), code_offset))
header = [0x50496e74, 0] + [0] * 8 + [len(strings), strings_offset,
    0, globals_offset, 4, globals_offset, 1, functions_offset, 0, len(data) - 84, 84]
struct.pack_into("<21I", data, 0, *header)
target = root / "receipt.pint"
target.write_bytes(data)
swapped = bytearray(data)
for index in [0, 1, *range(10, 21)]:
    struct.pack_into(">I", swapped, 4 * index, header[index])
for start, count in [(strings_offset, len(strings)), (globals_offset, 4), (functions_offset, 5)]:
    for index in range(count):
        value = struct.unpack_from("<I", data, start + 4 * index)[0]
        struct.pack_into(">I", swapped, start + 4 * index, value)
(root / "receipt-swapped.pint").write_bytes(swapped)
invalid_opcode = bytearray(data)
invalid_opcode[code_offset] = 0xff
(root / "invalid-opcode.pint").write_bytes(invalid_opcode)
expected = "Customer: Ada\nTotal: 37.5\n"
(root / "receipt.json").write_text(json.dumps({"format": "authored fixed Pinta bytecode",
    "sha256": hashlib.sha256(data).hexdigest(), "bytes": len(data),
    "swapped_sha256": hashlib.sha256(swapped).hexdigest(),
    "invalid_opcode_sha256": hashlib.sha256(invalid_opcode).hexdigest(),
    "globals": {"customer": "Ada", "quantity": 3, "unitPrice": "12.50"},
    "expected_total": "37.5", "expected_output": expected,
    "expected_output_utf16le_hex": expected.encode("utf-16le").hex()}, indent=2) + "\n")
print(target)
