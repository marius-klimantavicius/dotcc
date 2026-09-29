#!/usr/bin/env python3
"""Use acquired real target headers; never synthesize foreign ABI declarations."""
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import re
from campaigns.inputs import acquire
from campaigns.recipes import json_file


def run(ctx):
    sources = {row.name: row for row in ctx.recipe.sources(ctx.root)}
    if "zig" not in sources:
        raise RuntimeError("No pinned Zig host binary metadata for this host; supply a supported toolchain before probing.")
    source = acquire(sources["product"], ctx.policy, ctx.options.fetch)
    toolchain = acquire(sources["zig"], ctx.policy, ctx.options.fetch)
    compiler = toolchain / ("zig.exe" if __import__("os").name == "nt" else "zig")
    metadata = json_file(ctx.root / "config/toolchains.json")
    platforms = {row["id"]: row for row in json_file(ctx.root / "config/platforms.json")["backends"]}
    output = ctx.root / "build/foreign-headers" / ctx.run_id
    output.mkdir(parents=True)
    empty = output / "abi-profile.c"
    empty.write_text('#include "uv.h"\n')
    environment = {"ZIG_GLOBAL_CACHE_DIR": str(ctx.root / "build/zig-global-cache"),
                   "ZIG_LOCAL_CACHE_DIR": str(ctx.work / "zig-local-cache")}
    ctx.run([compiler, "version"], "foreign-toolchain-version", env=environment)
    report = {"scope": "real target preprocessing and native compile-time diagnostics; no execution or DotCC translation claimed",
              "source_revision": json_file(ctx.root / "config/sources.json")["product"]["revision"],
              "toolchain_version": metadata["version"], "toolchain": str(toolchain), "targets": []}
    for target in metadata["targets"]:
        directory = output / target["id"]
        directory.mkdir()
        common = [compiler, "cc", "-target", target["triple"], "-std=gnu11",
                  *("-D" + item for item in target["defines"]),
                  "-I" + str(source / "include"), "-I" + str(source / "src")]
        macros = ctx.run([*common, "-dM", "-E", empty], target["id"] + "-macros", env=environment, cwd=directory)
        # Include empty object-like macros (e.g. _UCRT); absence is different.
        macro_values = dict(re.findall(r"^#define (\w+)(?:[ \t]+([^\n]*))?$", macros, re.M))
        selected = {name: macro_values.get(name) for name in
                    ("__SIZEOF_POINTER__", "__SIZEOF_LONG__", "__SIZEOF_WCHAR_T__", "__BYTE_ORDER__",
                     "__ORDER_LITTLE_ENDIAN__", "_WIN32", "_WIN64", "__APPLE__", "__aarch64__", "__x86_64__",
                     "__MINGW32__", "__MSVCRT_VERSION__", "_UCRT", "__ENVIRONMENT_MAC_OS_X_VERSION_MIN_REQUIRED__")}

        def inspect(name):
            label = target["id"] + "-" + name.replace("/", "_").removesuffix(".c")
            preprocessed = directory / (name.replace("/", "_") + ".i")
            ctx.run([*common, "-E", source / name, "-o", preprocessed], label + "-preprocess",
                    check=False, env=environment, cwd=directory)
            # Record per-command exit, not merely existence of partial .i output.
            command = next(row for row in reversed(ctx.receipt["commands"]) if row["label"] == label + "-preprocess")
            row = {"source": name, "preprocess_exit": command["exit_code"], "preprocessed": str(preprocessed)}
            if command["exit_code"] == 0:
                content = preprocessed.read_bytes()
                row["sha256"] = hashlib.sha256(content).hexdigest()
                row["headers"] = sorted(set(re.findall(r'^#\s+\d+\s+"([^"\n]+)"', content.decode(errors="replace"), re.M)))
                ctx.run([compiler, "clang", "-target", target["clang_target"], "-std=gnu11",
                         "-fsyntax-only", preprocessed], label + "-syntax", check=False, env=environment, cwd=directory)
                syntax = next(command for command in reversed(ctx.receipt["commands"]) if command["label"] == label + "-syntax")
                row["syntax_exit"] = syntax["exit_code"]
            else:
                row["syntax_exit"] = None
            return row

        with ThreadPoolExecutor(max_workers=ctx.options.jobs) as executor:
            units = list(executor.map(inspect, platforms[target["backend"]]["source_units"]))
        diagnostics = {}
        for kind, options in (("layouts", ["-Xclang", "-fdump-record-layouts-complete"]),
                              ("ast", ["-Xclang", "-ast-dump=json"])):
            ctx.run([compiler, "clang", "-target", target["clang_target"], "-std=gnu11", *options,
                     "-fsyntax-only", directory / "src_version.c.i"],
                    target["id"] + "-" + kind, check=False, env=environment, separate=True, cwd=directory)
            command = next(row for row in reversed(ctx.receipt["commands"]) if row["label"] == target["id"] + "-" + kind)
            diagnostics[kind] = command["exit_code"]
        layouts = {}
        layout_text = (ctx.artifacts / (target["id"] + "-layouts.log")).read_text()
        for chunk in re.split(r"\*\*\* Dumping AST Record Layout\n", layout_text):
            name = re.search(r"^\s*0 \| (?:struct|union) (.+)$", chunk, re.M)
            size = re.search(r"\[sizeof=(\d+), align=(\d+)\]", chunk)
            if name and size and (name[1].startswith("uv_") or name[1] in ("_OVERLAPPED", "_WSABUF", "stat", "kevent")):
                layouts[name[1]] = {"size": int(size[1]), "alignment": int(size[2])}
        result = {**target, "macros": selected, "units": units,
                  "diagnostics_exit": diagnostics, "selected_native_layouts": layouts,
                  "preprocessed": sum(row["preprocess_exit"] == 0 for row in units),
                  "syntax_passed": sum(row["syntax_exit"] == 0 for row in units),
                  "ast_log": str(ctx.artifacts / (target["id"] + "-ast.log")),
                  "layout_log": str(ctx.artifacts / (target["id"] + "-layouts.log"))}
        report["targets"].append(result)
        (ctx.artifacts / "foreign-headers.json").write_text(json.dumps(report, indent=2) + "\n")
    ctx.receipt["foreign_headers"] = {"report": str(ctx.artifacts / "foreign-headers.json"),
                                       "targets": [{"id": row["id"], "total": len(row["units"]),
                                                    "preprocessed": row["preprocessed"], "syntax_passed": row["syntax_passed"]}
                                                   for row in report["targets"]], "runtime_qualification": False}
    ctx.save()
    if any(row["preprocessed"] != len(row["units"]) or row["syntax_passed"] != len(row["units"])
           or any(code != 0 for code in row["diagnostics_exit"].values())
           or "uv_loop_s" not in row["selected_native_layouts"] for row in report["targets"]):
        raise RuntimeError("Some foreign source/header probes failed; see foreign-headers.json and per-unit logs. Missing SDK declarations remain blockers.")
