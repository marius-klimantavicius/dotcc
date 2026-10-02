#!/usr/bin/env python3
"""Count unique C++ syntax sites in the built cpfe target using libclang.

Requires clang Python bindings (tested with 21.1.7) and libclang 21.
Uses the build's defines/includes/dialect and no-exceptions/no-RTTI flags.
Host predefined macros are Clang's; platform/compiler conditional branches
can therefore differ slightly from the GCC build. Counts exclude system
headers, build-generated headers, tests, runtime libraries and inactive code.
Checked-in generated IFC source under src/ is included.
"""
import argparse
from collections import Counter, defaultdict
from concurrent.futures import ProcessPoolExecutor
import json
import os
from pathlib import Path
import re
import shlex
import subprocess
import sys

ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "artifacts/cpp-audit/python"))
from clang import cindex as cx

KINDS = {
    "CLASS_TEMPLATE": "class_template_declarations",
    "FUNCTION_TEMPLATE": "function_template_declarations",
    "TYPE_ALIAS_TEMPLATE_DECL": "alias_template_declarations",
    "CLASS_TEMPLATE_PARTIAL_SPECIALIZATION": "partial_specializations",
    "TEMPLATE_TEMPLATE_PARAMETER": "template_template_parameters",
    "PACK_EXPANSION_EXPR": "pack_expansion_expressions",
    "LAMBDA_EXPR": "lambda_expressions",
    "CXX_FOR_RANGE_STMT": "range_for_loops",
    "CONSTRUCTOR": "constructor_declarations",
    "DESTRUCTOR": "destructor_declarations",
    "CXX_METHOD": "member_function_declarations",
    "CONVERSION_FUNCTION": "conversion_function_declarations",
    "CXX_BASE_SPECIFIER": "base_specifiers",
    "CXX_NEW_EXPR": "new_expressions",
    "CXX_DELETE_EXPR": "delete_expressions",
    "CXX_THROW_EXPR": "throw_expressions",
    "CXX_TRY_STMT": "try_statements",
    "CXX_CATCH_STMT": "catch_statements",
    "CXX_DYNAMIC_CAST_EXPR": "dynamic_cast_expressions",
    "CXX_TYPEID_EXPR": "typeid_expressions",
    "CXX_STATIC_CAST_EXPR": "static_cast_expressions",
    "CXX_REINTERPRET_CAST_EXPR": "reinterpret_cast_expressions",
    "CXX_CONST_CAST_EXPR": "const_cast_expressions",
    "STATIC_ASSERT": "static_assert_declarations",
    "TYPE_ALIAS_DECL": "type_alias_declarations",
    "NAMESPACE": "namespace_blocks",
    "TEMPLATE_REF": "template_reference_sites",
}


def audit(entry):
    if not cx.Config.loaded:
        cx.Config.set_library_file(os.environ.get("EDG_AUDIT_LIBCLANG", "/lib/x86_64-linux-gnu/libclang-21.so.21"))
    flags = [v for v in shlex.split(entry["command"])
             if v.startswith(("-D", "-I", "-std=")) or v in ("-fno-exceptions", "-fno-rtti")]
    # quadmath.h belongs to GCC, and is not in Clang's resource headers.
    compiler = shlex.split(entry["command"])[0]
    gcc_include = subprocess.check_output([compiler, "-print-file-name=include"], text=True).strip()
    flags += ["-idirafter", gcc_include]
    tu = cx.Index.create().parse(entry["file"], args=["-x", "c++", *flags])
    diagnostics = [{"severity": d.severity, "text": str(d)} for d in tu.diagnostics]
    sites, files, text_cache, functions = {}, set(), {}, {}
    counts = Counter()
    prefix = str(ROOT / "upstream/src") + "/"

    def walk(cursor):
        location = cursor.location
        filename = str(location.file) if location.file else ""
        if filename and not filename.startswith(prefix):
            return
        if filename:
            files.add(filename)
            kind = cursor.kind.name
            counts[kind] += 1

            def add(category):
                key = (category, filename, location.offset)
                if key not in sites:
                    sites[key] = {"category": category, "file": str(Path(filename).relative_to(ROOT)),
                                  "line": location.line, "column": location.column,
                                  "offset": location.offset, "name": cursor.spelling}

            if kind in KINDS:
                add(KINDS[kind])
            if kind in ("VAR_DECL", "PARM_DECL", "FIELD_DECL", "TYPE_ALIAS_DECL", "TYPEDEF_DECL"):
                type_kind = cursor.type.kind.name
                if type_kind == "LVALUEREFERENCE": add("lvalue_reference_declarations")
                if type_kind == "RVALUEREFERENCE": add("rvalue_reference_declarations")
                if type_kind == "AUTO": add("auto_typed_declarations")
            if kind in ("CONSTRUCTOR", "DESTRUCTOR", "CXX_METHOD", "FUNCTION_DECL", "FUNCTION_TEMPLATE", "VAR_DECL"):
                if filename not in text_cache: text_cache[filename] = Path(filename).read_bytes()
                text = text_cache[filename]
                if str(cursor.extent.start.file) == filename:
                    before_name = text[cursor.extent.start.offset:location.offset].decode(errors="replace")
                    # Strip comments before inspecting declaration prefixes.
                    before_name = re.sub(r'/\*.*?\*/|//[^\n]*', '', before_name, flags=re.S)
                    if re.search(r'\bconstexpr\b', before_name): add("constexpr_declarations")
                if kind in ("CONSTRUCTOR", "DESTRUCTOR", "CXX_METHOD") and cursor.is_virtual_method():
                    add("virtual_methods")
                if kind == "CONSTRUCTOR":
                    if cursor.is_copy_constructor(): add("copy_constructors")
                    if cursor.is_move_constructor(): add("move_constructors")
                if cursor.spelling.startswith("operator"):
                    add("operator_declarations")
                if kind == "FUNCTION_DECL":
                    usr = cursor.get_usr()
                    functions[usr] = {"name": cursor.spelling, "parent": cursor.semantic_parent.get_usr(),
                                      "file": str(Path(filename).relative_to(ROOT)), "line": location.line}
            if kind == "CXX_BASE_SPECIFIER" and cursor.is_virtual_base(): add("virtual_bases")
            if kind == "ENUM_DECL":
                if cursor.is_scoped_enum(): add("scoped_enums")
                if filename not in text_cache: text_cache[filename] = Path(filename).read_bytes()
                text = text_cache[filename]
                tail = text[location.offset:location.offset + 150].decode(errors="replace")
                if re.match(r'\w+\s*:', tail): add("explicit_enum_underlying_types")
        for child in cursor.get_children():
            walk(child)

    walk(tu.cursor)
    return {"source": entry["file"], "diagnostics": diagnostics,
            "files": sorted(files), "sites": list(sites.values()), "functions": functions}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--jobs", type=int, default=4)
    parser.add_argument("--limit", type=int)
    args = parser.parse_args()
    entries = [e for e in json.loads((ROOT / "build/compile_commands.json").read_text())
               if "/cpfe.dir/" in e["command"]]
    if args.limit: entries = entries[:args.limit]
    out = ROOT / "artifacts/cpp-audit"
    out.mkdir(parents=True, exist_ok=True)
    sites, files, functions, diagnostics = {}, set(), {}, []
    with ProcessPoolExecutor(max_workers=args.jobs) as pool:
        for n, result in enumerate(pool.map(audit, entries), 1):
            print(f"{n}/{len(entries)} {Path(result['source']).name}: {len(result['diagnostics'])} diagnostics", flush=True)
            diagnostics.append({"source": result["source"], "diagnostics": result["diagnostics"]})
            for site in result["sites"]:
                sites[(site["category"], site["file"], site["offset"])] = site
            files.update(result["files"])
            functions.update(result["functions"])
    categories = defaultdict(list)
    for site in sites.values(): categories[site["category"]].append(site)
    groups = defaultdict(list)
    for usr, f in functions.items(): groups[(f["parent"], f["name"])].append(f)
    overloaded = [v for v in groups.values() if len(v) > 1]
    summary = {"scope": "cpfe target only; unique EDG source locations, declarations and definitions counted separately",
               "translation_units": len(entries), "source_files_including_headers": len(files),
               "physical_source_lines_including_comments": sum(len(Path(f).read_bytes().splitlines()) for f in files),
               "error_diagnostics": sum(d["severity"] >= 3 for r in diagnostics for d in r["diagnostics"]),
               "counts": {k: len(categories[k]) for k in sorted(set(KINDS.values()) | set(categories) | {"virtual_methods", "virtual_bases"})},
               "overloaded_free_function_names": len(overloaded)}
    (out / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
    (out / "sites.json").write_text(json.dumps(dict(sorted(categories.items())), indent=2) + "\n")
    (out / "diagnostics.json").write_text(json.dumps(diagnostics, indent=2) + "\n")
    (out / "overloads.json").write_text(json.dumps(overloaded, indent=2) + "\n")
    print(json.dumps(summary, indent=2))
    if summary["error_diagnostics"]: sys.exit(1)


if __name__ == "__main__": main()
