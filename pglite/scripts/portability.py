"""Select upstream portable algorithms in a private configured-header copy."""
import re
import hashlib
from pathlib import Path

from campaigns.recipes import helper, json_file, link_flags


def prepare_overlay(root, include):
    policy = json_file(Path(root) / "config/portable-codegen.json")
    header = Path(include) / "pg_config.h"
    original = header.read_text()
    staged = original
    for macro, (before, after) in policy["configuration"].items():
        pattern = (rf"^/\* #undef {re.escape(macro)} \*/$" if before is None else
                   rf"^#define {re.escape(macro)} {re.escape(before)}$")
        replacement = f"/* #undef {macro} */" if after is None else f"#define {macro} {after}"
        staged, count = re.subn(pattern, replacement, staged, flags=re.MULTILINE)
        if count != 1:
            raise RuntimeError(f"Portable codegen configuration expects one {macro} declaration, got {count}")
    updates = {header: staged}
    for adaptation in policy.get("header_adaptations", []):
        path = Path(include) / adaptation["path"]
        text = path.read_text()
        if text.count(adaptation["before"]) != 1:
            raise RuntimeError(f"Portable header adaptation needs one context in {adaptation['path']}")
        updates[path] = text.replace(adaptation["before"], adaptation["after"], 1)
    for parser in policy.get("parser_semantic_types", []):
        path = Path(include) / parser["public_header"]
        text, count = re.subn(r"\bYYSTYPE\b", parser["tag"], path.read_text())
        if count != parser["public_occurrences"]:
            raise RuntimeError("Parser interface adaptation needs exact YYSTYPE occurrences in " + str(path))
        updates[path] = text
    for adaptation in policy.get("header_identifiers", []):
        path = Path(include) / adaptation["path"]
        text, count = re.subn(r"\b" + re.escape(adaptation["before"]) + r"\b",
                              adaptation["after"], updates.get(path, path.read_text()))
        if count != adaptation["occurrences"]:
            raise RuntimeError("Header identifier adaptation needs exact occurrences in " + str(path))
        updates[path] = text
    # Validate all anchors before writing; acquired/configured inputs are never edited.
    for path, text in updates.items():
        path.write_text(text)
    return {"policy": "config/portable-codegen.json", "header": str(header),
            "configuration": policy["configuration"],
            "portable_sources": policy["portable_sources"],
            "header_adaptations": policy.get("header_adaptations", []),
            "parser_semantic_types": policy.get("parser_semantic_types", []),
            "header_identifiers": policy.get("header_identifiers", []),
            "unit_defines": policy.get("unit_defines", {}),
            "excluded_native_codegen_units": policy["excluded_native_codegen_units"]}


def prepare_parser_units(root, source, destination):
    """Separate exported semantic types while retaining original parser bodies.

    A command define alone suppresses Bison's YYSTYPE declaration guard. Copy
    each real parser/scanner pair beside its adapted generated header so quoted
    includes resolve that header; its guard and type use the same distinct tag.
    """
    source, destination = Path(source).resolve(), Path(destination).resolve()
    if destination == source or source in destination.parents or destination in source.parents:
        raise RuntimeError("Parser staging must be separate from the source tree")
    if destination.exists():
        raise RuntimeError("Parser staging destination already exists")
    policy = json_file(Path(root) / "config/portable-codegen.json")
    files, units = {}, {}
    for parser in policy.get("parser_semantic_types", []):
        name = parser["generated_header"]
        text, count = re.subn(r"\bYYSTYPE\b", parser["tag"], (source / name).read_text())
        if count != parser["generated_occurrences"]:
            raise RuntimeError("Parser generated-header adaptation needs exact YYSTYPE occurrences in " + name)
        files[name] = text
        for name in parser["units"]:
            files[name] = (source / name).read_text()
            units[name] = {"path": str(destination / name), "defines": ["YYSTYPE=" + parser["tag"]]}
    # Validate every source and guard before creating any private output.
    destination.mkdir(parents=True)
    for name, text in files.items():
        path = destination / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text)
    return {"units": units, "directory": str(destination),
            "files_sha256": {name: hashlib.sha256((destination / name).read_bytes()).hexdigest() for name in files}}


def run(ctx, suite=None):
    info = helper(ctx.root / "scripts/prepare.py").prepare(ctx)
    source = Path(info["source"])
    work = ctx.work / "portable-codegen"
    work.mkdir()
    port = helper(ctx.root / "scripts/atomics.py").prepare_overlay(source, work / "include")
    ctx.receipt["pglite_portable_codegen"] = prepare_overlay(ctx.root, port["include"])
    include = ["-I", port["include"]]
    fixture = ctx.root / "tests/Translation/portable-codegen.c"
    inputs = [fixture, source / "src/port/pg_bitutils.c", source / "src/port/pg_crc32c_sb8.c"]
    native = work / "native"
    ctx.run(["cc", "-std=c11", "-O2", *include, "-DPORTABLE_CODEGEN_NATIVE",
             *inputs, "-o", native], "portable-native-build")
    ctx.run([native], "portable-native-run")
    objects = []
    for index, path in enumerate(inputs):
        obj = work / f"{index}.cs"
        ctx.run(["dotnet", ctx.compiler, "-std=c11", *include, path,
                 "--emit=obj", "-o", obj], f"portable-emission-{index}")
        objects.append(obj)
    output = work / "managed"
    ctx.run(["dotnet", ctx.compiler, *objects, "--emit=managedlib",
             *link_flags("PortableCodegen", "Managed.PGlite.Probes"), "-o", output], "portable-link")
    project = ctx.root / "tests/PortableCodegen/PortableCodegen.csproj"
    ctx.run(["dotnet", "run", "--project", project, "-c", "Release",
             f"-p:PortableCodegenProject={output / 'managed.csproj'}",
             "--artifacts-path", work / "dotnet", "-m:1"], "portable-managed-run")
