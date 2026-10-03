#!/usr/bin/env python3
"""Compile C or C++ translation units to an experimental C# project."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parent
RUNTIME_DEPENDENCIES = {
    "LibcTimezone": ("LibcTimezoneData",), "LibcTimezoneData": (),
    "NonlocalJump": (),
    "ExtendedFloat": ("Float128", "LibcFloat"),
    "Float128": ("Float128Conversions",), "Float128Conversions": ("Float80",),
    "Float80": (), "LibcContext": (), "AtomicOps": (), "VaList": (),
    "LibcFiles": ("LibcAppend", "LibcPaths", "LibcFilesWide"), "LibcFilesWide": (), "LibcAppend": (), "LibcPaths": (),
    "LibcFilesFormat": ("LibcFiles", "VaList"),
    "LibcFilesScan": ("LibcFiles", "VaList"), "LibcEnvironment": (),
    "LibcText": ("LibcErrorMessages", "LibcLocale"), "LibcErrorMessages": (), "LibcLocale": (),
    "LibcClock": (), "Vla": (), "LibcxxAtomicWait": (), "LibcFloat": ("Float80",), "LibcMath": ("Float80",), "LibcScanMath": ("Float80",),
    "LibcxxThreads": (), "LibcCalendar": (), "LibcProcess": (),
}

def content_bytes(value):
    return value.encode("utf-8") if isinstance(value, str) else value


def runtime_assets(files, helpers):
    """Copy only selected, hashed deployment assets into the generated project."""
    if "LibcTimezoneData" not in helpers:
        return {}, ""
    directory = ROOT / "backend/runtime/tzdb"
    bundle = (directory / "tzdb.bin").read_bytes()
    source = (directory / "source.json").read_bytes()
    metadata_bytes = (directory / "bundle.json").read_bytes()
    metadata = json.loads(metadata_bytes)
    if hashlib.sha256(bundle).hexdigest() != metadata["bundleSha256"]:
        raise ValueError("timezone bundle hash mismatch")
    if hashlib.sha256(source).hexdigest() != metadata["sourceManifestSha256"]:
        raise ValueError("timezone source manifest hash mismatch")
    if json.loads(source)["version"] != metadata["version"]:
        raise ValueError("timezone source and bundle versions differ")
    if len(bundle) != metadata["bundleBytes"]:
        raise ValueError("timezone bundle length mismatch")
    assets = {
        "Runtime.Tzdb.bin": bundle,
        "Runtime.Tzdb.source.json": source,
        "Runtime.Tzdb.bundle.json": metadata_bytes,
        "Runtime.Tzdb.LICENSE.txt": (directory / "LICENSE").read_bytes(),
    }
    files.update(assets)
    manifest = {name: {"sha256": hashlib.sha256(value).hexdigest(), "bytes": len(value)}
                for name, value in assets.items()}
    manifest["Runtime.Tzdb.bin"].update(version=metadata["version"], logicalName="Edg.Runtime.Tzdb.bin")
    return manifest, '    <EmbeddedResource Include="Runtime.Tzdb.bin" LogicalName="Edg.Runtime.Tzdb.bin" />'


def identifier(value):
    if not re.fullmatch(r"[A-Za-z_][A-Za-z_0-9]*", value):
        raise argparse.ArgumentTypeError("expected an ASCII identifier")
    return value


def namespace(value):
    if not value:
        return value
    for part in value.split("."):
        identifier(part)
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path, nargs="+")
    parser.add_argument("--language", choices=("c", "c++"), default="c++")
    parser.add_argument("--std", choices=("c89", "c99", "c11", "c17", "c23", "c++11", "c++14", "c++17", "c++20", "c++23", "c++26"),
                        help="select EDG language mode (default C11 or C++17)")
    parser.add_argument("--target", choices=("legacy-linux-x64", "portable-lp64-binary80-v1", "portable-lp64-binary80-v2"),
                        default="portable-lp64-binary80-v2", help="select the translated program's target ABI and predefined macros")
    parser.add_argument("-I", "--include", action="append", type=Path, default=[])
    parser.add_argument("-D", "--define", action="append", default=[])
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--namespace", default="Edg.Generated", type=namespace)
    parser.add_argument("--class-name", default="TranslatedProgram", type=identifier)
    parser.add_argument("--instance-methods", action="store_true",
                        help="give each generated class instance its own globals and TLS")
    parser.add_argument("--emit", choices=("exe", "managedlib"), default="exe")
    parser.add_argument("--type-ir", action="store_true",
                        help="verify and save the experimental backend-owned physical type graph (not a linkable program IR)")
    parser.add_argument("--runtime", choices=("none", "edg-exceptions"), default="none",
                        help="opt in to the pinned EDG exception classes and allocation bootstrap")
    parser.add_argument("--libc", choices=("none", "dotcc", "musl"), default="none",
                        help="select audited BCL imports, optionally adding pinned translated musl formatting/numeric sources")
    parser.add_argument("--nest-types", action=argparse.BooleanOptionalAction, default=True)
    parser.add_argument("--split", choices=("none", "function", "size"), default="none")
    parser.add_argument("--split-size", type=int, default=262144,
                        help="target UTF-8 bytes per member file in size mode; functions stay whole")
    args = parser.parse_args()
    standard=args.std or ("c11" if args.language=="c" else "c++17")
    if ("++" in standard) != (args.language=="c++"):
        parser.error("--std must match --language")
    if args.split_size <= 0:
        parser.error("--split-size must be positive")
    if args.class_name in ("Run", "Dispose", "Shutdown", "ReleaseThread") or args.class_name.startswith("__edg_"):
        parser.error("class name conflicts with a reserved backend member")
    compiler = ROOT / "build/bin/cpfe-dot"
    if not compiler.is_file():
        parser.error("build the backend first: EDG_BUILD_TARGET=cpfe-dot edgcpp/build.sh")
    # Verbatim identifiers also accept C# keywords, including namespace components.
    ns = ".".join("@" + part for part in args.namespace.split(".")) if args.namespace else ""
    cls = "@" + args.class_name
    env = dict(os.environ, EDG_BASE=str(ROOT / "upstream/bases/docker/dev-env/gcc"),
               EDG_DOT_NAMESPACE=ns, EDG_DOT_CLASS=cls,
               EDG_DOT_INSTANCE="1" if args.instance_methods else "0",
               EDG_DOT_EMIT=args.emit, EDG_DOT_NEST="1" if args.nest_types else "0",
               EDG_DOT_RUNTIME=args.runtime, EDG_DOT_LIBC="dotcc" if args.libc=="musl" else args.libc,
               EDG_DOT_TARGET=args.target, EDG_DOT_TYPE_IR="1" if args.type_ir else "0")
    # Never discover host system headers. The optional bootstrap exposes only
    # the pinned EDG exception headers for the current LP64 target. Keep EH
    # enabled so unsupported EH remains visible in the IL.
    runtime_options = []
    if args.runtime == "edg-exceptions" and args.language == "c":
        parser.error("edg-exceptions requires C++ input")
    sources = [p.resolve() for p in args.source]
    runtime_sources = []
    libc_provider = None
    if args.libc == "musl":
        from backend.musl_provider import prepare_musl_provider
        try:
            runtime_sources,libc_provider = prepare_musl_provider(ROOT)
        except (OSError,ValueError,KeyError) as error:
            parser.error(str(error))
    if args.runtime == "edg-exceptions":
        runtime_options = ["-I", str(ROOT / "backend/runtime/include")]
        if len(sources)+len(runtime_sources) == 1:
            runtime_options += ["--preinclude", str(ROOT / "backend/runtime/exceptions.cpp")]
        else:
            # One bootstrap definition per merged program; preincluding its
            # definitions into every input violates the C++ ODR.
            runtime_sources.append(ROOT / "backend/runtime/exceptions.cpp")
    if args.libc in ("dotcc", "musl"):
        runtime_options = ["-I", str(ROOT / "backend/runtime/libc/include"), *runtime_options]
    frontend_options = [flag for path in args.include for flag in ("-I", str(path.resolve()))]
    frontend_options += ["-D"+definition for definition in args.define]
    if args.language == "c++":
        # This driver emits a complete program, so referenced non-inline
        # templates need definitions rather than a later native link pass.
        frontend_options += ["--instantiate", "used"]
    target_manifest = {"id": args.target}
    if args.target != "legacy-linux-x64":
        frontend_options += ["--clear_flag", "use_predefined_macro_file"]
        target_manifest["inputs"] = [{"file": p.name, "sha256": hashlib.sha256(p.read_bytes()).hexdigest()}
                                     for p in (ROOT / "backend/target" / (args.target+".json"),
                                               ROOT / "backend/target" / (args.target+".def"))]
    result = subprocess.run([str(compiler), "--"+standard, "--gnu_version", "130300", *frontend_options, *runtime_options, "--", *map(str,sources+runtime_sources)],
                            env=env, capture_output=True, text=True)
    if result.stderr:
        print(result.stderr, end="", file=sys.stderr)
    if result.returncode:
        return result.returncode
    if not result.stdout.startswith("// Generated by dot_gen_be.c;"):
        parser.error("backend did not produce C# output")
    qualified = f"{ns}.{cls}" if ns else cls
    main_arguments = "// dot-gen:main-arguments utf8-owned-v1\n" in result.stdout
    run_arguments = "global::System.Environment.GetCommandLineArgs()" if main_arguments else ""
    invocation = f"new {qualified}().Run({run_arguments})" if args.instance_methods else f"{qualified}.Run({run_arguments})"
    exports = []
    def export_record(match):
        exports.append({"symbol": bytes.fromhex(match[1]).decode("utf-8"), "method": match[2]})
        return ""
    source = re.sub(r"^// dot-gen:export ([0-9a-f]*) (\w+)\n", export_record, result.stdout, flags=re.M)
    source = source.replace("// dot-gen:main-arguments utf8-owned-v1\n", "")
    type_ir_records = re.findall(r"^// dot-gen:type-ir (.*)\n", source, flags=re.M)
    if args.type_ir and len(type_ir_records) != 1:
        parser.error("backend did not emit one owned physical type graph")
    type_ir = json.loads(type_ir_records[0]) if type_ir_records else None
    source = re.sub(r"^// dot-gen:type-ir .*\n", "", source, flags=re.M)
    termination = "// dot-gen:termination owner-v1\n" in source
    source = source.replace("// dot-gen:termination owner-v1\n", "")
    imports = sorted(set(re.findall(r"^// dot-gen:import (\w+)\n", source, flags=re.M)))
    source = re.sub(r"^// dot-gen:import \w+\n", "", source, flags=re.M)
    # These are explicit backend protocol delimiters, not a C# source parser.
    members = re.compile(r"^// dot-gen:member-begin\n(.*?)^// dot-gen:member-end\n", re.M | re.S)
    chunks = members.findall(source)
    translated = members.sub(lambda m: m[1] if args.split == "none" else "", source)
    files = {
        "Translated.cs": translated,
    }
    if type_ir is not None:
        files["dot-gen.types.json"] = json.dumps(type_ir, indent=2) + "\n"
    if args.emit == "exe":
        if termination:
            receiver = "owner" if args.instance_methods else qualified
            owner = f"using var owner=new {qualified}(); " if args.instance_methods else ""
            cleanup = f" finally {{ {qualified}.Shutdown(); }}" if not args.instance_methods and "void Shutdown()" in result.stdout else ""
            files["EntryPoint.cs"] = f"internal static class EntryPoint {{ public static int Main() {{ {owner}try {{ return {receiver}.Run({run_arguments}); }} catch(global::System.Exception error) {{ {receiver}.__EdgUnhandledProgram(error); throw; }}{cleanup} }} }}\n"
        elif "void Shutdown()" in result.stdout:
            if args.instance_methods:
                files["EntryPoint.cs"] = f"internal static class EntryPoint {{ public static int Main() {{ using var owner=new {qualified}(); return owner.Run({run_arguments}); }} }}\n"
            else:
                files["EntryPoint.cs"] = f"internal static class EntryPoint {{ public static int Main() {{ try {{ return {invocation}; }} finally {{ {qualified}.Shutdown(); }} }} }}\n"
        else:
            files["EntryPoint.cs"] = f"internal static class EntryPoint {{ public static int Main() => {invocation}; }}\n"
    if args.split != "none":
        wrapper = (f"namespace {ns};\n" if ns else "") + f"public {'sealed' if args.instance_methods else 'static'} unsafe partial class {cls} {{\n"
        groups = []
        for chunk in chunks:
            if (not groups or args.split == "function" or
                    len((wrapper + groups[-1] + chunk + "}\n").encode("utf-8")) > args.split_size):
                groups.append(chunk)
            else:
                groups[-1] += chunk
        for index, group in enumerate(groups):
            files[f"Translated.part{index:05d}.cs"] = wrapper + group + "}\n"
    runtime_helpers={name for name in RUNTIME_DEPENDENCIES if f"global::Edg.Runtime.{name}" in source}
    if "__edg_with_file_lock" in imports:
        runtime_helpers.add("LibcFilesFormat")
    if "__edg_with_file_read_lock" in imports:
        runtime_helpers.add("LibcFilesScan")
    pending=list(runtime_helpers)
    while pending:
        for dependency in RUNTIME_DEPENDENCIES[pending.pop()]:
            if dependency not in runtime_helpers:
                runtime_helpers.add(dependency);pending.append(dependency)
    for helper in sorted(runtime_helpers):
        files[f"Runtime.{helper}.cs"] = (ROOT / f"backend/runtime/{helper}.cs").read_text()
    asset_manifest, resource_items = runtime_assets(files, runtime_helpers)
    compile_items = "\n".join(f'    <Compile Include="{filename}" />' for filename in files if filename.endswith(".cs"))
    files["Generated.csproj"] = '''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>OUTPUT_TYPE</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <IsAotCompatible>true</IsAotCompatible>
    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <NoWarn>$(NoWarn);CS0162;CS0164;CS0219;CS0414;CS1718</NoWarn>
  </PropertyGroup>
  <ItemGroup>
COMPILE_ITEMS
RESOURCE_ITEMS
  </ItemGroup>
</Project>
'''.replace("COMPILE_ITEMS", compile_items).replace("RESOURCE_ITEMS", resource_items).replace("OUTPUT_TYPE", "Exe" if args.emit == "exe" else "Library")
    out = args.output_dir.resolve()
    manifest = out / "dot-gen.json"
    owned = set()
    if manifest.exists():
        owned = set(json.loads(manifest.read_text())["files"])
        allowed={"Translated.cs", "EntryPoint.cs", "Generated.csproj", "dot-gen.types.json", "Runtime.Tzdb.bin", "Runtime.Tzdb.source.json", "Runtime.Tzdb.bundle.json", "Runtime.Tzdb.LICENSE.txt"} | {f"Runtime.{name}.cs" for name in RUNTIME_DEPENDENCIES}
        if any(name not in allowed and
               not re.fullmatch(r"Translated\.part[0-9]{5,}\.cs", name) for name in owned):
            parser.error("invalid generated-file ownership manifest")
    for filename in files:
        if (out / filename).exists() and filename not in owned:
            parser.error(f"refusing to overwrite unowned file: {out / filename}")
    out.mkdir(parents=True, exist_ok=True)
    files["dot-gen.json"] = json.dumps({
        "schema": 1, "abi": "experimental-runtime-v3", "cleanupModel": "edg-region-tables", "source": str(sources[0]) if len(sources)==1 else None,
        "sources": [{"path": str(p), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sources],
        "runtimeSources": [{"path": str(p), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in runtime_sources],
        "runtimeAssets": asset_manifest,
        "runtimeHelpers": {name: hashlib.sha256(files[f"Runtime.{name}.cs"].encode()).hexdigest() for name in sorted(runtime_helpers)},
        "namespace": args.namespace, "class": args.class_name,
        "instanceMethods": args.instance_methods, "files": sorted(files),
        "split": args.split, "splitSize": args.split_size,
        "mainArguments": "utf8-owned-v1" if main_arguments else None,
        "language": args.language, "standard": standard, "includeDirectories": [str(p.resolve()) for p in args.include],
        "defines": args.define, "lifecycle": "explicit-shutdown-release-thread" if "void Shutdown()" in source else "process",
        "longDouble": "software-binary80" if "global::Edg.Runtime.Float80" in source else None,
        "extendedFloating": {"binary128": "software-ieee-v1" if "global::Edg.Runtime.Float128" in source else None,
                             "binary16": "system-half-v1" if "global::System.Half" in source else None},
        "nonlocalJump": {"version": 1, "bufferSize": 16, "bufferAlignment": 8, "continuation": "activation-site-dispatch"} if "NonlocalJump" in runtime_helpers else None,
        "varargs": "portable-typed-pack-v1" if "global::Edg.Runtime.VaList" in source else None,
        "emit": args.emit, "runtime": args.runtime, "nestTypes": args.nest_types, "exports": exports,
        "filePositionABI": {"version": 2, "size": 16, "alignment": 8, "fields": {"offset": "i64@0", "state": "u32@8", "value": "u32@12"}} if "LibcFiles" in runtime_helpers else None,
        "libc": args.libc, "libcProvider": libc_provider, "imports": imports,
        "target": target_manifest,
        "termination": "owner-v1" if termination else None,
        "physicalTypeIR": {"file":"dot-gen.types.json", "sha256":hashlib.sha256(files["dot-gen.types.json"].encode()).hexdigest()} if type_ir is not None else None,
    }, indent=2) + "\n"
    # Keep superseded generation inputs during the full implementation campaign.
    # Historical files are outside the explicit project Compile item list.
    changed = {name for name in owned | {"dot-gen.json"}
               if (out / name).is_file() and
               (name not in files or (out / name).read_bytes() != content_bytes(files[name]))}
    if changed:
        history = out / ".dot-gen-history" / str(time.time_ns())
        history.mkdir(parents=True)
        for name in sorted(changed):
            shutil.copy2(out / name, history / name)
    for filename, content in files.items():
        with tempfile.NamedTemporaryFile(mode="wb", dir=out, delete=False) as tmp:
            tmp.write(content_bytes(content))
        os.replace(tmp.name, out / filename)
    for filename in owned - files.keys():
        # The previous revision has been retained in .dot-gen-history above.
        (out / filename).unlink(missing_ok=True)
    print(out / "Generated.csproj")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
