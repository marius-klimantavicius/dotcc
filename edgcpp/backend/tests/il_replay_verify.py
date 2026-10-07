#!/usr/bin/env python3
"""Verify stock IL replay parity, option restoration and rejection boundaries."""
import argparse
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT))
from backend import il_replay


def verify_stock_promotion_views():
    """The adapter changes storage/factory boundaries, not promotion rules."""
    types = (ROOT / "upstream/src/types.c").read_text()
    expressions = (ROOT / "upstream/src/exprutil.c").read_text()
    actual = (ROOT / "backend/dot_gen_primitive_views.h").read_text()
    substitutions = {
        "type_after_integral_promotion": "source_type_after_integral_promotion",
        "type_after_bit_field_integral_promotion": "source_bitfield_promoted_type",
        "requires_bit_field_promotion": "source_requires_bitfield_promotion",
        "node_type_after_integral_promotion": "source_promoted_type",
        "integer_type": "source_integer_view", "prvalue_type": "skip_typerefs",
        "is_bit_field_extract_node": "source_is_bitfield_extract_node",
        "is_integer_or_unscoped_enum": "source_is_integer_or_unscoped_enum",
    }
    for source, name, signature in [
        (types, "type_after_integral_promotion", "a_type_ptr type_after_integral_promotion(a_type_ptr type)"),
        (expressions, "type_after_bit_field_integral_promotion", "static\na_type_ptr type_after_bit_field_integral_promotion(an_expr_node_ptr node)"),
        (expressions, "is_bit_field_extract_node", "static a_boolean is_bit_field_extract_node(an_expr_node_ptr node)"),
        (expressions, "requires_bit_field_promotion", "static a_boolean requires_bit_field_promotion(an_expr_node_ptr  node,"),
        (expressions, "node_type_after_integral_promotion", "a_type_ptr node_type_after_integral_promotion(an_expr_node_ptr node)"),
    ]:
        start = source.index(signature)
        end_marker = "}  /* " + name + " */"
        end = source.index(end_marker, start) + len(end_marker)
        fragment = source[start:end]
        for old, new in substitutions.items():
            fragment = re.sub(r"\b" + old + r"\b", new, fragment)
        assert fragment in actual, "stock promotion body drift: " + name


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--tools", required=True, type=Path)
    parser.add_argument("--compiler", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    source_dir = out / "sources"
    source_dir.mkdir()
    report = {"passed": False, "commands": [], "checks": [],
              "tools": il_replay.file_record(args.tools), "compiler": il_replay.file_record(args.compiler)}
    tools = il_replay.load_tools(args.tools)

    def save():
        il_replay.write_json(out / "results.json", report)

    def run(label, command, rejection=None, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, env=env)
        (out / (label + ".stdout")).write_text(result.stdout)
        (out / (label + ".stderr")).write_text(result.stderr)
        report["commands"].append({"label": label, "arguments": command, "exitCode": result.returncode})
        save()
        if rejection:
            if result.returncode == 0 or rejection not in result.stderr:
                raise AssertionError(f"{label}: expected rejection {rejection!r}")
        elif result.returncode:
            raise AssertionError(f"{label}: failed; see {out}")
        return result

    def cli(*arguments):
        return [sys.executable, "-B", ROOT / "dotnet.py", *arguments]

    def replay(label, checkpoint, *options, selected_tools=args.tools, rejection=None, env=None):
        return run(label, cli("--il-tools", selected_tools, "--il-replay", checkpoint,
                             "--output-dir", out / label, *options), rejection=rejection, env=env)

    try:
        verify_stock_promotion_views()
        report["checks"].append("Primitive promotion views preserve the pinned stock EDG decisions")
        fixtures = ["source_names.c", "source_control.c", "exceptions.cpp", "class_exceptions.cpp",
                    "virtual_dispatch.cpp", "variable_alias_typed.cpp", "routine_linkage.cpp",
                    "variable_asm_definition.cpp"]
        extras = ["routine_linkage_provider.cpp", "variable_asm_type.h", "variable_asm_other.cpp", "variable_asm_views.cpp"]
        for name in fixtures + extras:
            shutil.copy2(Path(__file__).parent / name, source_dir / name)
        shutil.copy2(args.compiler, out / "direct-compiler")
        original = out / "direct-compiler"
        for fixture in fixtures:
            sources = [source_dir / fixture]
            if fixture == "variable_asm_definition.cpp":
                sources += [source_dir / name for name in ("variable_asm_other.cpp", "variable_asm_views.cpp")]
            for instance in (False, True):
                label = Path(fixture).stem + ("-instance" if instance else "-static")
                common = ["--emit", "managedlib", "--unbound-externals", "partial", "--namespace", "Replay.Tests",
                          "--class-name", "Core", "--format", "none", "--method-sharding", "none",
                          *(["--instance-methods"] if instance else [])]
                direct_raw, checkpoint_raw, replay_raw = [out / (label + suffix) for suffix in (".direct.cs", ".checkpoint.cs", ".replay.cs")]
                run(label + "-direct", cli(*sources, *common, "--compiler", original,
                    "--output-dir", out / (label + "-direct"), "--raw-output", direct_raw))
                checkpoint = out / (label + "-il")
                run(label + "-checkpoint", cli(*sources, *common, "--il-tools", args.tools,
                    "--il-checkpoint", checkpoint, "--output-dir", out / (label + "-checkpoint"), "--raw-output", checkpoint_raw))
                # Deliberately conflicting inherited backend state must be
                # replaced by the checkpoint's recorded environment.
                replay(label + "-replay", checkpoint, "--split", "source", "--raw-output", replay_raw,
                       env=dict(os.environ, EDG_DOT_NAMESPACE="wrong", EDG_DOT_INSTANCE="wrong"))
                assert direct_raw.read_bytes() == checkpoint_raw.read_bytes() == replay_raw.read_bytes()
                report["checks"].append(label + ": raw C# identical; options restored; source split may vary")
                save()

        checkpoint = out / "routine_linkage-static-il"
        replay("conflicting-namespace", checkpoint, "--namespace", "Different", rejection="conflicts with checkpoint")
        replay("conflicting-language", checkpoint, "--language", "c", rejection="conflicts with checkpoint")
        replay("abbreviated-namespace", checkpoint, "--names", "Different", rejection="unrecognized arguments")
        dependency = source_dir / "routine_linkage_provider.cpp"
        before = dependency.read_bytes()
        dependency.write_bytes(before + b"\n// changed input\n")
        replay("changed-header", checkpoint, rejection="input checksum mismatch")
        dependency.write_bytes(before)
        il = checkpoint / "program.cil"
        before = il.read_bytes()
        il.write_bytes(before + b"tamper")
        replay("changed-il", checkpoint, rejection="input checksum mismatch")
        il.write_bytes(before)
        receipt_path = checkpoint / "checkpoint.json"
        before = receipt_path.read_bytes()
        tampered = json.loads(before)
        tampered["environment"]["EDG_DOT_CLASS"] = "Wrong"
        il_replay.write_json(receipt_path, tampered)
        replay("changed-receipt", checkpoint, rejection="receipt checksum mismatch")
        receipt_path.write_bytes(before)
        incompatible = json.loads(args.tools.read_text())
        incompatible["compatibility"]["definitions"].append("INCOMPATIBLE_TEST_CONFIGURATION=1")
        incompatible["compatibilityId"] = il_replay.object_digest(incompatible["compatibility"])
        il_replay.write_json(out / "incompatible-tools.json", incompatible)
        replay("changed-configuration", checkpoint, selected_tools=out / "incompatible-tools.json", rejection="source/configuration/host ABI mismatch")
        damaged_tools = json.loads(args.tools.read_text())
        damaged_tools["reader"]["sha256"] = "0" * 64
        il_replay.write_json(out / "damaged-tools.json", damaged_tools)
        replay("changed-reader", checkpoint, selected_tools=out / "damaged-tools.json", rejection="input checksum mismatch")
        report["checks"].append("Explicit semantic conflicts and header/IL/receipt/configuration/reader tampering rejected")

        primitive = source_dir / "primitive_views.c"
        shutil.copy2(Path(__file__).with_name("primitive_views.c"), primitive)
        for language in ("c", "c++"):
            label = "primitive-" + language.replace("+", "p")
            native = out / (label + "-native")
            run(label + "-native-build", ["gcc" if language == "c" else "g++", "-x", language,
                "-std=c11" if language == "c" else "-std=c++17", "-O2", primitive, "-lm", "-latomic", "-o", native])
            run(label + "-native-run", [native])
            for instance in (False, True):
                mode = label + ("-instance" if instance else "-static")
                options = ["--language", language, "--type-ir", "--format", "none", "--method-sharding", "none",
                           *(["--instance-methods"] if instance else [])]
                direct = out / (mode + "-direct.cs")
                replayed = out / (mode + "-replay.cs")
                run(mode + "-direct", cli(primitive, *options, "--compiler", original,
                    "--output-dir", out / (mode + "-direct"), "--raw-output", direct))
                run(mode + "-checkpoint", cli(primitive, *options, "--il-tools", args.tools,
                    "--il-checkpoint", out / (mode + "-il"), "--output-dir", out / mode, "--raw-output", replayed))
                assert direct.read_bytes() == replayed.read_bytes()
                types = json.loads((out / mode / "dot-gen.types.json").read_text())["types"]
                assert all(item["alignment"] > 0 for item in types
                           if item["kind"] in ("signed-integer", "unsigned-integer"))
                project = out / mode / "Generated.csproj"
                run(mode + "-jit-build", ["dotnet", "build", project, "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
                run(mode + "-jit-run", ["dotnet", project.parent / "bin/Release/net10.0/Generated.dll"])
                if args.nativeaot:
                    run(mode + "-aot-build", ["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                        "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-p:DebugType=none", "-o", out / (mode + "-aot")])
                    run(mode + "-aot-run", [out / (mode + "-aot/Generated")])
                report["checks"].append(mode + ": narrow signed/unsigned bitfields, enums, GNU/C11 atomics; parity and execution")

        api = source_dir / "macro_provider.c"
        api.write_text('#include <stdlib.h>\n#include <string.h>\n#define API_CODE 41\n'
                       'long api_value(const char *text) { return strtol(text, 0, 10) + strlen(text) + API_CODE; }\n')
        provider_checkpoint = out / "provider-il"
        run("provider-checkpoint", cli(api, "--language", "c", "--libc", "musl", "--emit", "managedlib",
            "--export-macros", "API_CODE", "--format", "none", "--method-sharding", "none",
            "--il-tools", args.tools, "--il-checkpoint", provider_checkpoint,
            "--output-dir", out / "provider-first", "--raw-output", out / "provider-first.cs"))
        provider_receipt = il_replay.load_checkpoint(provider_checkpoint, tools)
        snapshots_before = sorted((ROOT / "artifacts/musl-provider").iterdir())
        replay("provider-replay", provider_checkpoint, "--split", "function", "--raw-output", out / "provider-replay.cs")
        assert sorted((ROOT / "artifacts/musl-provider").iterdir()) == snapshots_before
        assert (out / "provider-first.cs").read_bytes() == (out / "provider-replay.cs").read_bytes()
        # Compile the original exact provider paths; calling prepare_musl again
        # would change source identities and invalidate a byte comparison.
        run("provider-direct", [original, *provider_receipt["frontendArguments"]],
            env=il_replay.restored_environment(provider_receipt["environment"]))
        assert (out / "provider-direct.stdout").read_bytes() == (out / "provider-replay.cs").read_bytes()
        first = json.loads((out / "provider-first/dot-gen.json").read_text())
        second = json.loads((out / "provider-replay/dot-gen.json").read_text())
        assert first["libcProvider"] == second["libcProvider"]
        assert first["macroExports"] == second["macroExports"]
        assert first["runtimeSources"] == second["runtimeSources"]
        report["checks"].append("Actual musl pruning/macro exports: raw parity, metadata restored, no provider regeneration")

        # An intentionally failing reader is test fault injection, not a
        # substitute backend. Successful retry uses the real compiled reader.
        failure = out / "failing-reader"
        failure.write_text("#!/bin/sh\nprintf 'injected backend failure\\n' >&2\nexit 73\n")
        failure.chmod(0o755)
        failing_tools = json.loads(args.tools.read_text())
        failing_tools["reader"] = il_replay.file_record(failure)
        failing_tools["backendId"] = il_replay.digest(failure)
        il_replay.write_json(out / "failing-tools.json", failing_tools)
        failed_checkpoint = out / "failed-emission-il"
        run("injected-failure", cli(source_dir / "source_control.c", "--il-tools", out / "failing-tools.json",
            "--il-checkpoint", failed_checkpoint, "--output-dir", out / "failed-emission",
            "--format", "none", "--method-sharding", "none"), rejection="injected backend failure")
        il_replay.load_checkpoint(failed_checkpoint, tools)
        replay("corrected-emission", failed_checkpoint)
        project = out / "corrected-emission/Generated.csproj"
        run("jit-build", ["dotnet", "build", project, "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
        run("jit-run", ["dotnet", project.parent / "bin/Release/net10.0/Generated.dll"])
        if args.nativeaot:
            run("aot-build", ["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-p:DebugType=none", "-o", out / "aot"])
            run("aot-run", [out / "aot/Generated"])
        report["checks"].append("Failed emission preserved a valid checkpoint; different compatible reader replayed it; JIT execution passed")
        if args.nativeaot:
            report["checks"].append("Replayed executable: Linux x64 NativeAOT passed")
        report["passed"] = True
    finally:
        save()


if __name__ == "__main__":
    main()
