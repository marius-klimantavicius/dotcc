"""Source accounting and paired clean-build/initialization measurements.

`sources` is read-only apart from its report. `compare` runs managed builds and
must be scheduled by the supervisor, serially with other repository builds.
It never builds tools, regenerates products, or changes canonical output.
"""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shlex
import shutil
import statistics
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(REPO / "Scripts"))
from campaigns.delivery import ignore_build_caches, validate_emission
from campaigns.process import execute
from campaigns.recipes import helper
from campaigns.translation import relocate_project

PROFILES = ("default", "aggregate-data", "typed")
TABLE_BYTES = 7809 * 9
_FIELD_STORE = re.compile(rb"\bencoder_instructions_destination\[\d+\]\.[^\n]* = ")
_ENCODER_COPY = re.compile(
    rb"\b([BP][A-Za-z0-9_]+)\.CopyTo\(new (?:global::System\.)?Span<byte>"
    rb"\(\(byte\*\)\(encoder_instructions_destination\)(?: \+ (\d+))?, (\d+)\)\);")
_PAYLOAD = re.compile(
    rb"\b(?:internal|public) static (?:global::System\.)?ReadOnlySpan<(byte|ushort|uint|ulong)>\s+"
    rb"([BP][A-Za-z0-9_]+)\s*=>\s*\[(.*?)\];", re.S)


def read_json(path):
    return json.loads(Path(path).read_text())


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, sort_keys=True) + "\n")


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def file_record(path):
    path = Path(path)
    return dict(path=str(path), bytes=path.stat().st_size, sha256=digest(path))


def source_metrics(project):
    """Count UTF-8 source, exact encoder field-store lines and RVA payloads.

    Payload bytes count generated constant span declarations once per property;
    they are logical data bytes, not a claim about PE section padding/dedup.
    No generated C# is rewritten, and authored links are reported separately.
    """
    project = Path(project).resolve()
    validate_emission(project.parent, "TranslatedZydis")
    names = (project.parent / "Dotcc.SourceFiles.txt").read_text().splitlines()
    result = dict(project=str(project), source_bytes=0, encoder_field_store_lines=0,
                  encoder_field_store_source_bytes=0, payload_bytes=0, payload_declarations=0,
                  aggregate_initialized_bytes=0, aggregate_initializers=0,
                  encoder_payload_copy_calls=0, encoder_payload_copy_bytes=0,
                  files=[], payloads=[], encoder_copies=[])
    widths = {b"byte": 1, b"ushort": 2, b"uint": 4, b"ulong": 8}
    for name in names:
        path = project.parent / name
        if not path.resolve().is_relative_to(project.parent):
            raise RuntimeError("Unsafe generated source manifest path: " + name)
        data = path.read_bytes()
        stores = [line for line in data.splitlines(keepends=True) if _FIELD_STORE.search(line)]
        row = dict(file_record(path), name=name, encoder_field_store_lines=len(stores),
                   encoder_field_store_source_bytes=sum(map(len, stores)))
        result["files"].append(row)
        result["source_bytes"] += len(data)
        result["encoder_field_store_lines"] += len(stores)
        result["encoder_field_store_source_bytes"] += row["encoder_field_store_source_bytes"]
        for match in _ENCODER_COPY.finditer(data):
            result["encoder_copies"].append(dict(file=name, name=match[1].decode(),
                                                offset=int(match[2] or 0), bytes=int(match[3])))
        aggregate_sizes = re.findall(rb"// aggregate-data: (\d+) bytes", data)
        result["aggregate_initializers"] += len(aggregate_sizes)
        result["aggregate_initialized_bytes"] += sum(map(int, aggregate_sizes))
        for match in _PAYLOAD.finditer(data):
            # LiteralPool emits fixed-width hex collection elements. Fail on a
            # new spelling instead of silently reporting a partial byte count.
            values = re.findall(rb"0x[0-9A-Fa-f]+(?:UL|U)?", match[3])
            residue = re.sub(rb"0x[0-9A-Fa-f]+(?:UL|U)?|[\s,]", b"", match[3])
            if residue:
                raise RuntimeError("Unrecognized literal payload: " + match[2].decode())
            count = len(values) * widths[match[1]]
            result["payloads"].append(dict(file=name, name=match[2].decode(),
                                           backing=match[1].decode(), bytes=count))
            result["payload_bytes"] += count
            result["payload_declarations"] += 1
    payloads = {row["name"]: row["bytes"] for row in result["payloads"]}
    for copy in result["encoder_copies"]:
        if payloads.get(copy["name"]) != copy["bytes"]:
            raise RuntimeError("Encoder copy does not refer to its complete immutable payload")
    result["encoder_payload_copy_calls"] = len(result["encoder_copies"])
    result["encoder_payload_copy_bytes"] = sum(row["bytes"] for row in result["encoder_copies"])
    result["encoder_payload_bytes"] = sum(payloads[name] for name in
                                          {row["name"] for row in result["encoder_copies"]})
    result["authored_sources"] = [file_record((project.parent / node.get("Include")).resolve())
                                   for node in ET.parse(project).iter("Compile") if node.get("Include")]
    result["scope"] = "manifest-listed generated C# only; source bytes include headers/runtime; logical payload bytes exclude string pool"
    return result


def distribution(samples):
    return dict(samples=samples, minimum=min(samples), median=statistics.median(samples),
                maximum=max(samples), mean=statistics.mean(samples),
                stdev=statistics.stdev(samples) if len(samples) > 1 else None)


def run(report, command, cwd, label, timeout=3600):
    record = {}
    report["commands"].append(record)
    try:
        execute(command, label=label, cwd=cwd, output=Path(report["output"]) / "logs" / (label + ".log"),
                timeout=timeout, separate=True, record=record)
    finally:
        write_json(Path(report["output"]) / "report.json", report)
    return record


def _tool_identity(receipt):
    snapshots = receipt.get("tool_snapshots", {})
    if not all(snapshots.get(name, {}).get("files") for name in ("DotCC", "DotCC.PostProcess")):
        raise RuntimeError("Fresh receipts must identify compiler and postprocessor snapshots")
    return {name: {key: value for key, value in snapshot.items() if key in ("executable", "files")}
            for name, snapshot in snapshots.items()}


def _recorded_emission(receipt):
    """Read emission policy from the receipt, including historical defaults."""
    profile = receipt.get("profile")
    emission = receipt.get("zydis_selection", {}).get("emission")
    if not isinstance(emission, dict) or type(emission.get("aggregate_data")) is not bool:
        raise RuntimeError("Receipt lacks explicit aggregate-data emission selection")
    optimized = emission["aggregate_data"]
    expected = dict(profile=profile, aggregate_data=optimized, aggregate_data_threshold=1024,
                    object_flags=["--aggregate-data"] if optimized else [])
    if (emission != expected or profile not in PROFILES
            or (profile == "typed" and optimized) or (profile == "aggregate-data" and not optimized)):
        raise RuntimeError("Inconsistent recorded aggregate-data emission selection")
    return emission


def _object_commands(receipt, emission):
    optimized = emission["aggregate_data"]
    commands = sorted((row for row in receipt["commands"] if re.fullmatch(r"\d{3}-.+", row["label"])),
                      key=lambda row: row["label"])
    if len(commands) != 18:
        raise RuntimeError("Measurement requires all 18 current product object emissions")
    flags = []
    for row in commands:
        command = row.get("requested_command", row["command"])
        if row["status"] != "passed" or command.count("--aggregate-data") != int(optimized):
            raise RuntimeError("Wrong/missing object-stage aggregate-data flag: " + row["label"])
        # Discard per-run tool/report/output paths and the one intended option.
        values = command[2:]
        normalized = []
        skip = False
        for value in values:
            if skip:
                skip = False
            elif value in ("--override-report", "-o"):
                skip = True
            elif value != "--aggregate-data":
                normalized.append(value)
        flags.append(normalized)
    return flags


def _link_command(receipt):
    rows = [row for row in receipt["commands"] if row["label"] == "link"]
    if len(rows) != 1 or rows[0]["status"] != "passed":
        raise RuntimeError("Missing successful product link command")
    row = rows[0]
    values = iter(row.get("requested_command", row["command"])[2:])
    result = []
    for value in values:
        if value == "-o":
            next(values)
        elif value.endswith(".cs"):
            result.append("object/" + Path(value).name)
        else:
            result.append(value)
    return result


def paired_inputs(baseline_path, optimized_path, forms):
    recipes = helper(ROOT / "scripts/campaign.py")
    qualification = helper(ROOT / "scripts/qualification.py")
    selected = recipes.native_configuration(ROOT)
    receipts, products, identities = {}, {}, {}
    for optimized, path in zip((False, True), (baseline_path, optimized_path)):
        receipt = read_json(path)
        profile = receipt.get("profile")
        if (receipt.get("project") != "zydis" or profile not in PROFILES
                or receipt.get("status") != "passed" or not receipt.get("outputs")):
            raise RuntimeError("Expected successful fresh Zydis translation/verify receipt")
        emission = _recorded_emission(receipt)
        if emission["aggregate_data"] != optimized:
            role = "Optimized" if optimized else "Baseline"
            flags = "--aggregate-data" if optimized else "no --aggregate-data; use typed for current campaigns"
            kind = "optimized" if optimized else "unoptimized"
            raise RuntimeError(f"{role} receipt must record {kind} emission ({flags})")
        if profile in receipts:
            raise RuntimeError("Paired receipts must have distinct profile names")
        receipts[profile] = receipt
        identities[profile] = dict(tools=_tool_identity(receipt), objects=_object_commands(receipt, emission),
                                   link=_link_command(receipt),
                                   sdk=receipt["dotnet_sdk"], selection=receipt["zydis_selection"].copy())
        identities[profile]["selection"].pop("emission")
        current = qualification._inputs(type("Context", (), {"root": ROOT, "profile": profile})(), selected)
        # The current campaign may assign a different policy to "default".
        # Source bytes must still be current, but emission follows this receipt.
        current["campaign/emission"] = json.dumps(emission, sort_keys=True)
        for form in forms:
            entry = receipt["outputs"][form]
            product = Path(entry["project"]).resolve()
            if read_json(product.parent / "zydis-emission-profile.json") != emission:
                raise RuntimeError("Current product profile stamp differs from receipt")
            companion = Path(entry["companions"]["TranslatedZydisQualification"]["project"])
            companion_inputs = read_json(companion.parent / "zydis-test-inputs.json")
            expected_inputs = current.copy()
            if profile == "default" and not optimized and "campaign/emission" not in companion_inputs:
                # Historical unoptimized default companions omitted this key.
                expected_inputs.pop("campaign/emission")
            if companion_inputs != expected_inputs:
                raise RuntimeError("Companion inputs/profile differ from current source bytes; regenerate " + profile)
            # Hashes may be disabled in the campaign. Record actual current bytes
            # here, but never infer output identity from an empty hash manifest.
            for name, expected in entry.get("hashes", {}).items():
                if digest(product.parent / name) != expected:
                    raise RuntimeError("Current measured output differs from supplied receipt: " + name)
            products[profile, form] = product
    baseline_profile, optimized_profile = receipts
    if identities[baseline_profile] != identities[optimized_profile]:
        raise RuntimeError("Paired receipts differ in tools, SDK, source selection or object flags beyond --aggregate-data")
    return receipts, products, identities[baseline_profile]


def native_table(report):
    selected = helper(ROOT / "scripts/campaign.py").native_configuration(ROOT)
    output = Path(report["output"]) / "native"
    output.mkdir()
    compiler = shlex.split(os.environ.get("CC", "cc"))
    if not compiler:
        raise RuntimeError("Empty CC")
    run(report, [*compiler, "--version"], output, "native-version")
    source = ROOT / "tests/AggregateDataProbe/native_encoder_dump.c"
    table_source = selected["sources"]["zydis"] / "src/EncoderData.c"
    binary = output / ("encoder-dump.exe" if os.name == "nt" else "encoder-dump")
    run(report, [*compiler, *selected["flags"], "-O2", source, table_source, "-o", binary], output, "native-build")
    target = output / "encoder-instructions.bin"
    run(report, [binary, target], output, "native-dump")
    if target.stat().st_size != TABLE_BYTES:
        raise RuntimeError("Unexpected native encoder table byte count")
    report["native_table"] = dict(file_record(target), source=file_record(source),
                                  table_source=file_record(table_source), flags=selected["flags"])
    return target.read_bytes()


def stage_probe(product, destination):
    destination.mkdir(parents=True)
    for name in ("AggregateDataProbe.csproj", "Program.cs"):
        shutil.copy2(ROOT / "tests/AggregateDataProbe" / name, destination / name)
    staged = destination / "product"
    shutil.copytree(product.parent, staged, ignore=ignore_build_caches)
    relocate_project(staged / product.name, product.parent)


def sample(report, product, destination, mode, rid, init_samples, label, expected):
    stage_probe(product, destination)
    project = "AggregateDataProbe.csproj"
    # Exactly the same argv, properties and relative layout in both profiles.
    # Restore is deliberately outside the timed build/publish interval. Every
    # build sample has its own initially absent output/intermediate directories.
    common = [project, "-p:Configuration=Release", "-m:1", "--artifacts-path", "build",
              "-p:UseSharedCompilation=false", "-p:Optimize=true", "-p:DebugType=portable",
              "-p:DebugSymbols=true", "-p:RootTranslatedLibrary=true"]
    runtime = ["-r", rid, "-p:PublishAot=true"] if mode == "aot" else []
    run(report, ["dotnet", "restore", *common, *runtime], destination, label + "-restore")
    built = run(report, ["dotnet", "publish" if mode == "aot" else "build", *common, *runtime,
                         "--no-restore", "-o", "bin"], destination, label + "-build")
    command = ([str(destination / "bin/Zydis.AggregateDataProbe")] if mode == "aot" else
               ["dotnet", str(destination / "bin/Zydis.AggregateDataProbe.dll")])
    result = dict(build_seconds=built["seconds"], binaries=[], init_seconds=[], process_seconds=[], tables=[])
    for path in sorted((destination / "bin").iterdir()):
        if path.is_file():
            result["binaries"].append(file_record(path))
    binary = destination / "bin" / ("Zydis.AggregateDataProbe" if mode == "aot" else "Zydis.AggregateDataProbe.dll")
    result["probe_binary"] = file_record(binary)
    assembly = destination / "bin/TranslatedZydis.dll"
    if assembly.is_file():
        result["product_assembly"] = file_record(assembly)
    for index in range(init_samples):
        target = destination / f"encoder-instructions-{index:02d}.bin"
        record = run(report, [*command, target], destination, label + f"-init-{index:02d}")
        lines = Path(record["stdout"]).read_text().splitlines()
        values = dict(line.split("=", 1) for line in lines)
        if (len(values) != 5 or int(values["encoder_bytes"]) != TABLE_BYTES
                or int(values["encoder_rows"]) != 7809 or int(values["encoder_row_bytes"]) != 9
                or target.read_bytes() != expected or values["encoder_sha256"] != digest(target)):
            raise RuntimeError("Actual emitted encoder table differs byte-for-byte from fresh native control: " + label)
        result["init_seconds"].append(float(values["init_seconds"]))
        result["process_seconds"].append(record["seconds"])
        result["tables"].append(dict(file_record(target), comparison="all 70281 bytes equal native"))
    return result


def compare(args):
    if sys.platform != "linux" or platform.machine().lower() not in ("x86_64", "amd64") or args.rid != "linux-x64":
        raise RuntimeError("Current native/emitted table measurement requires execution on linux-x64")
    forms = ("raw", "processed") if args.form == "all" else (args.form,)
    modes = ("jit", "aot") if args.mode == "all" else (args.mode,)
    receipts, products, identity = paired_inputs(args.baseline_receipt, args.optimized_receipt, forms)
    profiles = tuple(receipts)
    baseline_profile, optimized_profile = profiles
    output = args.output.resolve()
    # Never delete or reuse an earlier measurement's intermediate caches.
    output.mkdir(parents=True, exist_ok=False)
    report = dict(schema_version=2, status="running", output=str(output), commands=[], cells=[],
                  utc=datetime.now(timezone.utc).isoformat(),
                  receipts={profile: file_record(path) for profile, path in
                            zip(profiles, (args.baseline_receipt, args.optimized_receipt))},
                  comparison=dict(baseline=baseline_profile, optimized=optimized_profile,
                                  emissions={profile: receipts[profile]["zydis_selection"]["emission"]
                                             for profile in profiles},
                                  difference="optimized minus baseline"),
                  qualification={profile: {key: receipts[profile].get(key) for key in
                                            ("zydis_audit", "zydis_qualification_reports", "zydis_managed_boundary_report")}
                                 for profile in profiles},
                  equivalent_emission=identity, build_samples=args.build_samples, init_samples=args.init_samples,
                  host=dict(platform=platform.platform(), machine=platform.machine(), cpu_count=os.cpu_count()),
                  environment={name: os.environ.get(name) for name in
                               ("DOTNET_TieredCompilation", "DOTNET_TieredPGO", "DOTNET_ReadyToRun",
                                "DOTNET_GCServer", "DOTNET_GCHeapHardLimit", "COMPlus_TieredCompilation",
                                "COMPlus_TieredPGO", "COMPlus_ReadyToRun", "MSBuildSDKsPath", "DOTNET_ROOT")},
                  controls={name: file_record(REPO / name) for name in
                            ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json")
                            if (REPO / name).is_file()},
                  probe={name: file_record(ROOT / "tests/AggregateDataProbe" / name)
                         for name in ("AggregateDataProbe.csproj", "Program.cs", "native_encoder_dump.c")},
                  measurement_helper=file_record(__file__),
                  limitations=["Timing samples include OS cache, scheduling and JIT noise; no performance acceptance threshold.",
                               "Build/publish wall time excludes restore and staging; each sample uses fresh intermediates.",
                               "Initialization times the complete explicit product class constructor in a fresh process; JIT work inside is included.",
                               "Process wall time also includes startup, hashing and table-file I/O; it is reported separately.",
                               "AOT probe roots the whole translated library in both profiles; ordinary trimmed consumer qualification remains separate.",
                               "This report does not replace the complete campaign qualification/audit gates."])
    baseline = REPO / "artifacts/aggregate-byte-data/baseline.json"
    if baseline.is_file():
        report["historical_context_only"] = dict(file_record(baseline), data=read_json(baseline),
                                                  comparison="not a paired performance baseline")
    try:
        run(report, ["dotnet", "--info"], output, "dotnet-info")
        expected = native_table(report)
        report["sources"] = {profile: {form: source_metrics(products[profile, form]) for form in forms}
                             for profile in profiles}
        for form in forms:
            optimized = report["sources"][optimized_profile][form]
            cursor = 0
            for copy in optimized["encoder_copies"]:
                if copy["offset"] != cursor or not 0 < copy["bytes"] <= 4096:
                    raise RuntimeError("Encoder table does not use complete bounded byte copies")
                cursor += copy["bytes"]
            if cursor != TABLE_BYTES or optimized["encoder_field_store_lines"] != 0:
                raise RuntimeError("Actual encoder table did not select the byte-data path")
        report["source_differences"] = {
            form: {name: report["sources"][optimized_profile][form][name] - report["sources"][baseline_profile][form][name]
                   for name in ("source_bytes", "encoder_field_store_lines", "encoder_field_store_source_bytes",
                                "payload_bytes", "aggregate_initialized_bytes", "encoder_payload_copy_bytes")}
            for form in forms}
        for form in forms:
            for mode in modes:
                cells = {profile: dict(profile=profile, form=form, mode=mode, samples=[]) for profile in profiles}
                report["cells"].extend(cells.values())
                for index in range(args.build_samples):
                    # Alternate who runs first to make simple temporal bias visible.
                    for profile in profiles if index % 2 == 0 else profiles[::-1]:
                        label = f"{form}-{mode}-{profile}-{index:02d}"
                        destination = output / "work" / form / mode / profile / f"sample-{index:02d}"
                        cells[profile]["samples"].append(sample(report, products[profile, form], destination,
                                                              mode, args.rid, args.init_samples, label, expected))
                for cell in cells.values():
                    cell["build_seconds"] = distribution([sample["build_seconds"] for sample in cell["samples"]])
                    for name in ("init_seconds", "process_seconds"):
                        cell[name] = distribution([value for sample in cell["samples"] for value in sample[name]])
        report["median_differences"] = []
        for form in forms:
            for mode in modes:
                cells = {cell["profile"]: cell for cell in report["cells"] if cell["form"] == form and cell["mode"] == mode}
                report["median_differences"].append(dict(
                    form=form, mode=mode, optimized_minus_baseline={
                        name: cells[optimized_profile][name]["median"] - cells[baseline_profile][name]["median"]
                        for name in ("build_seconds", "init_seconds", "process_seconds")}))
        report["status"] = "passed"
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        write_json(output / "report.json", report)
    print(output / "report.json")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="action", required=True)
    sources = commands.add_parser("sources", help="source accounting only; never builds")
    sources.add_argument("--project", type=Path, required=True)
    sources.add_argument("--output", type=Path, required=True)
    paired = commands.add_parser("compare", help="supervisor-only paired managed builds and fresh-process table/init probes")
    paired.add_argument("--baseline-receipt", "--default-receipt", type=Path, required=True,
                        help="unoptimized typed receipt, or historical unoptimized default receipt")
    paired.add_argument("--optimized-receipt", "--aggregate-receipt", type=Path, required=True,
                        help="optimized default or aggregate-data receipt")
    paired.add_argument("--output", type=Path, required=True, help="new directory; existing directories are rejected")
    paired.add_argument("--form", choices=("raw", "processed", "all"), default="all")
    paired.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    paired.add_argument("--rid", default="linux-x64")
    paired.add_argument("--build-samples", type=int, default=3)
    paired.add_argument("--init-samples", type=int, default=5)
    args = parser.parse_args()
    if args.action == "sources":
        write_json(args.output, source_metrics(args.project))
    else:
        if args.build_samples <= 0 or args.init_samples <= 0:
            parser.error("sample counts must be positive")
        compare(args)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
