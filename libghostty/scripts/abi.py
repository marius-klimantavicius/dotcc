"""Generate only the typed address-access boundary, never terminal algorithms."""
import json
from pathlib import Path
import re
import shutil


def generate(root, output):
    rows = json.loads((root / "config/abi.json").read_text())["operations"]
    if len({row["id"] for row in rows}) != len(rows) or len({row["name"] for row in rows}) != len(rows):
        raise RuntimeError("Duplicate ABI operation")
    output.mkdir(parents=True, exist_ok=True)
    zig = ['const api = @import("terminal/c/main.zig");',
           'export fn dotcc_ghostty_api(operation: c_int) ?*const anyopaque {',
           '    return switch (operation) {']
    header = ['#ifndef DOTCC_GHOSTTY_ABI_H', '#define DOTCC_GHOSTTY_ABI_H',
              'void *dotcc_ghostty_api(int operation);']
    native = ['#include <ghostty/vt.h>', 'void *dotcc_ghostty_api(int operation) {',
              '    switch (operation) {']
    for row in rows:
        if not re.fullmatch(r"ghostty_[a-z_]+", row["name"]) or not re.fullmatch(r"[a-z_]+", row["zig_member"]):
            raise RuntimeError("Invalid ABI operation spelling")
        zig.append(f'        {row["id"]} => @ptrCast(&api.{row["zig_member"]}),')
        # The unevaluated type comes from the actual included public header.
        # A macro's own name is disabled while its replacement is expanded.
        header.append(f'#define {row["name"]} ((__typeof__(&{row["name"]}))dotcc_ghostty_api({row["id"]}))')
        native.append(f'    case {row["id"]}: return (void *)&{row["name"]};')
    zig += ['        else => null,', '    };', '}']
    header += ['#endif']
    native += ['    default: return 0;', '    }', '}']
    for name, lines in (("dotcc_ghostty_api.zig", zig), ("dotcc_ghostty_abi.h", header),
                        ("native_ghostty_api.c", native)):
        (output / name).write_text("\n".join(lines) + "\n", encoding="utf-8")
    return output


def stage_root(root, upstream, work):
    """Augment a private root with address access; preserve every upstream export."""
    adapter = generate(root, work / "abi")
    staged = work / "source" / "src"
    shutil.copytree(upstream / "src", staged)
    entry = staged / "lib_vt.zig"
    text = entry.read_text(encoding="utf-8")
    marker = 'const lib = @This();'
    addition = 'comptime { _ = @import("dotcc_ghostty_api.zig"); }'
    if text.count(marker) != 1 or 'dotcc_ghostty_api.zig' in text:
        raise RuntimeError("Unexpected upstream root for ABI extension")
    shutil.copyfile(adapter / "dotcc_ghostty_api.zig", staged / "dotcc_ghostty_api.zig")
    entry.write_text(text + "\n// Authored ABI address accessor; upstream exports above are preserved.\n"
                     + addition + "\n", encoding="utf-8")
    return entry, adapter
