#!/usr/bin/env python3
"""Assemble one fixed CALL_INTERNAL/store-global callback-root fixture."""
import hashlib
import json
from pathlib import Path
import struct

root = Path(__file__).resolve().parent / "fixtures"
root.mkdir(exist_ok=True)
data = bytearray(84)
string_offset = len(data)
data.extend(b"\x06answer\0")
strings_offset = len(data)
data.extend(struct.pack("<I", string_offset))
globals_offset = len(data)
data.extend(struct.pack("<I", 0))
code_offset = len(data)
# CALL_INTERNAL token=1 arguments=0; STORE_GLOBAL token=0; EXIT.
code = bytes([0x1b, 1, 0, 0x25, 0, 0x2e])
data.extend(code)
data.extend(b"\0" * (-len(data) % 4))
functions_offset = len(data)
data.extend(struct.pack("<5I", 0xffffffff, 0, 0, len(code), code_offset))
header = [0x50496e74, 0] + [0] * 8 + [1, strings_offset,
    0, globals_offset, 1, globals_offset, 1, functions_offset, 0, len(data) - 84, 84]
struct.pack_into("<21I", data, 0, *header)
target = root / "callback-return.pint"
target.write_bytes(data)
(root / "callback-return.json").write_text(json.dumps({
    "format": "authored fixed Pinta bytecode",
    "sha256": hashlib.sha256(data).hexdigest(),
    "bytes": len(data),
    "code_hex": code.hex(),
    "callback_token": 1,
    "callback_argument_count": 0,
    "result_global": "answer",
    "expected_result": "Ą😀",
    "expected_utf16_units": [0x0104, 0xd83d, 0xde00],
    "expected_utf16le_hex": "04013dd800de",
    "contract": "Callback assigns its caller-rooted return slot, forces Pinta compaction, and returns; guest stores that exact result in answer."
}, ensure_ascii=False, indent=2) + "\n")
print(target)
