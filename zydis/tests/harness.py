"""Original upstream test Units and fixtures, shared by native and DotCC links."""
from __future__ import annotations

import json
from pathlib import Path
import re
import shlex

from campaigns.model import Unit


def _enum(value):
    if not isinstance(value, str) or not re.fullmatch(r"ZYDIS_[A-Z0-9_]+(?:\|ZYDIS_[A-Z0-9_]+)*", value):
        raise ValueError("Invalid upstream enum expression: " + repr(value))
    # Upstream's Python fixture schema names the zero attributes value, while
    # the public C API represents it as the integer zero (no C macro exists).
    return "0" if value == "ZYDIS_ATTRIB_NONE" else value


def _number(value, bits=64, signed=False):
    number = int(value)
    lower, upper = (-(1 << (bits - 1)), (1 << (bits - 1)) - 1) if signed else (0, (1 << bits) - 1)
    if not lower <= number <= upper:
        raise ValueError(f"Upstream number does not fit {bits} bits: {value}")
    # Hex plus an explicit unsigned cast also handles INT64_MIN without an
    # out-of-range signed integer literal.
    literal = f"0x{number & ((1 << bits) - 1):X}ULL"
    return f"(ZyanI{bits}){literal}" if signed else literal


def _request(case):
    fields = [f".{name} = {_enum(case[name])}" for name in
              ("machine_mode", "allowed_encodings", "mnemonic", "prefixes", "branch_type",
               "branch_width", "address_size_hint", "operand_size_hint")]
    operands = []
    if len(case["operands"]) > 5:
        raise ValueError("Too many upstream encoder operands")
    for op in case["operands"]:
        kind = _enum(op["operand_type"])
        values = [".type = " + kind]
        if kind == "ZYDIS_OPERAND_TYPE_REGISTER":
            values += [".reg = { .value = " + _enum(op["reg"]["value"]) +
                       ", .is4 = " + str(int(op["reg"]["is4"])) + " }"]
        elif kind == "ZYDIS_OPERAND_TYPE_MEMORY":
            mem = op["mem"]
            values += [".mem = { .base = " + _enum(mem["base"]) + ", .index = " + _enum(mem["index"]) +
                       ", .scale = " + _number(mem["scale"], 8) + ", .displacement = " +
                       _number(mem["displacement"], signed=True) + ", .size = " + _number(mem["size"], 16) + " }"]
        elif kind == "ZYDIS_OPERAND_TYPE_POINTER":
            values += [".ptr = { .segment = " + _number(op["ptr"]["segment"], 16) +
                       ", .offset = " + _number(op["ptr"]["offset"], 32) + " }"]
        elif kind == "ZYDIS_OPERAND_TYPE_IMMEDIATE":
            values += [".imm = { .u = " + _number(op["imm"]["value"]) + " }"]
        else:
            raise ValueError("Unimplemented upstream operand: " + kind)
        operands.append("{ " + ", ".join(values) + " }")
    fields += [".operand_count = " + str(len(operands))]
    if operands:
        fields += [".operands = { " + ", ".join(operands) + " }"]
    for group in ("evex", "mvex"):
        entries = ["." + name + " = " + (str(int(value)) if isinstance(value, bool) else _enum(value))
                   for name, value in case[group].items()]
        if set(case[group]) != ({"broadcast", "rounding", "sae", "zeroing_mask"} if group == "evex" else
                                {"broadcast", "conversion", "rounding", "swizzle", "sae", "eviction_hint"}):
            raise ValueError("Unexpected upstream request fields")
        fields += ["." + group + " = { " + ", ".join(entries) + " }"]
    return "{ " + ", ".join(fields) + " }"


def inventory(source):
    source = Path(source)
    cases = source / "tests/cases"
    inputs = sorted(cases.glob("*.in"))
    if not inputs:
        raise RuntimeError("Missing upstream decoder regression inventory")
    if {p.stem for p in inputs} != {p.stem for p in cases.glob("*.out")}:
        raise RuntimeError("Upstream decoder .in/.out inventory differs")
    rows = []
    for path in inputs:
        args = shlex.split(path.read_text())
        if not args or not all(arg and "\0" not in arg for arg in args):
            raise ValueError("Invalid upstream info arguments: " + str(path))
        rows.append(dict(id="info/" + path.stem, kind="info", input=str(path),
                         expected=str(path.with_suffix(".out")), arguments=args))
    for filename, kind in (("enc_test_cases.json", "encoder"), ("re_enc_test_cases.json", "reencoder")):
        path = source / "tests" / filename
        cases = json.loads(path.read_text())
        if not isinstance(cases, list) or not cases:
            raise RuntimeError("Missing upstream " + kind + " regression inventory")
        rows += [dict(id=f"{kind}/{i:03d}", kind=kind, input=str(path), index=i,
                      description=case.get("description", case.get("mnemonic", ""))) for i, case in enumerate(cases)]
    rows.append(dict(id="absolute/original", kind="absolute", input=str(source / "tools/ZydisTestEncoderAbsolute.c")))
    return rows


def fixtures(source, output):
    """Emit actual upstream C types and enum identifiers, never ABI mirror structs."""
    source, output = Path(source), Path(output)
    output.mkdir(parents=True, exist_ok=True)
    rows = inventory(source)
    lines = ["/* Generated from local upstream fixtures; do not hand edit. */"]
    info = [row for row in rows if row["kind"] == "info"]
    lines += [f"#define ZQ_INFO_MAX_ARGC {max(len(row['arguments']) + 1 for row in info)}",
              f"#define ZQ_INFO_MAX_ARGUMENT {max(len(arg.encode('utf-8')) for row in info for arg in ['ZydisInfo', *row['arguments']]) + 1}"]
    for i, row in enumerate(info):
        argv = ["ZydisInfo", *row["arguments"]]
        lines.append(f"static const char* const zq_argv_{i}[] = {{ " + ", ".join(json.dumps(arg) for arg in argv) + " };")
    lines.append("static const ZqInfoCase zq_info_cases[] = {")
    for i, row in enumerate(info):
        lines.append("{ " + json.dumps(row["id"]) + f", {len(row['arguments']) + 1}, zq_argv_{i}" + " },")
    lines.append("};")
    enc = json.loads((source / "tests/enc_test_cases.json").read_text())
    lines.append("static const ZydisEncoderRequest zq_encoder_cases[] = {")
    lines.extend(_request(case) + "," for case in enc)
    lines.append("};")
    reenc = json.loads((source / "tests/re_enc_test_cases.json").read_text())
    lines.append("static const ZqReencodeCase zq_reencoder_cases[] = {")
    for case in reenc:
        data = bytes.fromhex(case["payload"])
        if not 0 < len(data) <= 32:
            raise ValueError("Invalid upstream re-encoder payload length")
        lines.append("{ " + _enum(case["machine_mode"]) + ", " + _enum(case["stack_width"]) +
                     ", " + str(len(data)) + ", { " + ", ".join(f"0x{v:02X}" for v in data) + " } },")
    lines.append("};")
    header = output / "zq_fixtures.h"
    content = "\n".join(lines) + "\n"
    if not header.is_file() or header.read_text() != content:
        header.write_text(content)
    (output / "inventory.json").write_text(json.dumps(rows, indent=2) + "\n")
    return output


def test_units(ctx, source):
    include = fixtures(source, ctx.work / "zydis-qualification-fixtures")
    own = ctx.root / "tests"
    tools = Path(source) / "tools"
    return [
        Unit(own / "qualification.c", ("-I", str(include), "-I", str(tools))),
        Unit(tools / "ZydisInfo.c", ("-Dmain=ZqInfoMain",)),
        Unit(tools / "ZydisToolsShared.c"),
        # Original CLI color detection, test-only; NO_COLOR=1 makes output
        # deterministic. It is not part of the delivered library closure.
        Unit(ctx.sources["zycore"] / "src/API/Terminal.c"),
        Unit(tools / "ZydisFuzzShared.c", ("-Dmain=ZqUnusedFuzzMain", "-DZydisFuzzTarget=ZqFuzzEncode")),
        Unit(tools / "ZydisFuzzEncoder.c", ("-DZydisFuzzTarget=ZqFuzzEncode",)),
        Unit(tools / "ZydisFuzzReEncoding.c", ("-DZydisFuzzTarget=ZqFuzzReencode",)),
        Unit(tools / "ZydisTestEncoderAbsolute.c", ("-Dmain=ZqAbsoluteMain",)),
    ]
