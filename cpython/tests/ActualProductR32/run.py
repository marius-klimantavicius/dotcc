"""B32 actual RAW discovery, only after A28's DIRECT new receipt and release."""
import argparse
import datetime
import fcntl
import importlib.util
import json
import os
from pathlib import Path
import stat
import sys
import traceback

ROOT = Path(__file__).resolve().parents[3]
CAMPAIGN = ROOT / "cpython"
HERE = Path(__file__).resolve().parent
ARTIFACTS = CAMPAIGN / "artifacts/owner-b-r32-20261018"
WORK = CAMPAIGN / "build/owner-b-r32-20261018"


def module(path):
    spec = importlib.util.spec_from_file_location(Path(path).stem, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


# Reuse unchanged Python-only identity/journal helpers; importing starts no work.
prior = module(CAMPAIGN / "tests/RawDiscoveryR30/run.py")
digest, item, read = prior.digest, prior.item, prior.read


def write(name, value):
    path = ARTIFACTS / name
    temporary = path.with_suffix(path.suffix + ".pending")
    temporary.write_text(json.dumps(value, indent=2, default=str) + "\n")
    temporary.replace(path)
    return item(path)


def verified(reference):
    assert digest(reference["path"]) == reference["sha256"], reference["path"]
    return read(reference["path"])


def stat_section(directory, source):
    """Bind just the changed runtime section before early discovery."""
    path = Path(source["path"])
    assert digest(path) == source["sha256"]
    marker = "// ---- " + path.name + " ----\n#nullable enable\n"
    matches = []
    for emitted in sorted(directory.glob("*.cs")):
        text = emitted.read_text(encoding="utf-8-sig")
        if marker in text:
            assert text.count(marker) == 1
            begin = text.index(marker) + len(marker)
            end = text.index("#nullable restore", begin)
            expected = "".join(line + "\n" for line in path.read_text().split("\n")
                if not line.lstrip().startswith(("#nullable", "namespace "))
                and not line.startswith("using ")) + "\n"
            assert text[begin:end] == expected, emitted
            matches.append(item(emitted))
    assert len(matches) == 1, matches
    return {"source": source, "actual_generated_source": matches[0],
            "scope": "B31 stat bytes in actual generated source; matched PDB binds compiled bytes separately"}


def pyc_observation(log):
    values = [json.loads(line) for line in Path(log).read_text().splitlines()
              if line.startswith('{"scope": "native/RAW file-permission reduction;')]
    assert len(values) == 1, log
    for row in values[0]["rows"]:
        for key in ("created", "renamed", "source", "compiled"):
            if key not in row or key == "created":
                # This original pathname was renamed by the unchanged fixture.
                continue
            reported = row[key]
            physical = os.stat(reported["path"])
            assert reported["mode"] == oct(physical.st_mode), reported
            assert reported["permissions"] == oct(stat.S_IMODE(physical.st_mode)), reported
            assert (reported["uid"], reported["gid"], reported["size"]) == (
                physical.st_uid, physical.st_gid, physical.st_size), reported
    return values[0]


def compare_pyc(native, actual):
    rows = []
    for expected, observed in zip(native["rows"], actual["rows"], strict=True):
        assert expected["operation"] == observed["operation"]
        keys = ("created", "renamed") if "created" in expected else ("source", "compiled")
        for key in keys:
            for field in ("mode", "permissions", "uid", "gid", "size"):
                assert expected[key][field] == observed[key][field], (key, field, expected, observed)
        if "created" in expected:
            assert expected["requested_mode"] == observed["requested_mode"]
            assert expected["flags"] == observed["flags"]
            assert expected["read"] == observed["read"] == "exact bytes"
        else:
            assert expected["loaded_value"] == observed["loaded_value"] == 42
        rows.append({"operation": observed["operation"],
                     **{key: {field: observed[key][field]
                         for field in ("mode", "permissions", "uid", "gid", "size")} for key in keys},
                     "result": observed.get("read", observed.get("loaded_value"))})
    assert len(rows) == 4
    return {"status": "passed", "rows": rows,
            "scope": "Exact unchanged R30 fixture native/actual metadata and real pyc loading parity"}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--authority", type=Path, required=True)
    args = parser.parse_args()
    authority = read(args.authority)
    assert authority["safe_heavy_release"] is True and authority["direct_handoff_message"]
    verified(authority["release"])
    publication = verified(authority["raw_publication"])
    capture = verified(authority["capture"])
    receipt = verified(publication["receipt"])
    assert receipt["status"] == "passed" and receipt["profile"] == "upstream-tests-static"
    assert receipt["selection"]["form"] == "raw" and len(receipt["units"]) == 286
    assert set(receipt["outputs"]) == {"raw"}
    preparation = read(ARTIFACTS / "source-preparation.json")
    assert publication["actualRawPE"]["sha256"] != preparation["prior_raw_pe"]["sha256"]
    assert digest(publication["actualRawPE"]["path"]) == publication["actualRawPE"]["sha256"]
    for source in preparation["frozen_execution_inputs"]:
        assert digest(source["path"]) == source["sha256"], source["path"]
    runtime = preparation["b31_stat_source"]
    assert digest(runtime["path"]) == runtime["sha256"]
    tools = {name: row["pe"] for name, row in capture["tools"].items()}
    for tool in tools.values():
        assert digest(tool["path"]) == tool["sha256"]
    assert receipt["tools"]["DotCC"]["dotcc.dll"] == tools["driver"]["sha256"]
    assert receipt["tools"]["DotCC"]["DotCC.Lib.dll"] == tools["compilerLibrary"]["sha256"]
    assert tools["compilerLibrary"]["sha256"] != preparation["prior_compiler_library"]["sha256"]
    project = Path(receipt["outputs"]["raw"]["project"])
    inventory_path = CAMPAIGN / "tests/Upstream/inventory.frozen.json"
    inventory = read(inventory_path)
    assert inventory["status"] == "frozen" and len(inventory["required_cases"]) == 4867
    assert not (ARTIFACTS / "commands.json").exists(), "Preserve the existing attempt; do not replay it."
    WORK.mkdir(parents=True, exist_ok=True)
    free = []
    for path in ("/tmp/dotcc-cpython-shared-build.lock", "/tmp/dotcc-cpython-native-build.lock",
                 str(CAMPAIGN / "build/campaign/run.lock")):
        with open(path, "a") as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            fcntl.flock(stream, fcntl.LOCK_UN)
        free.append(path)
    write("actual-entry.json", {"authority": item(args.authority), "publication": authority["raw_publication"],
        "capture": authority["capture"], "receipt": publication["receipt"], "free_checks": free,
        "started_utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "scope": "Early actual RAW JIT discovery; all final acceptance gates remain open"})
    sys.path.insert(0, str(ROOT / "Scripts"))
    from campaigns.process import execute
    binding = module(CAMPAIGN / "tests/Audit/actual_profile_input.py")
    pe_reader = module(CAMPAIGN / "build/owner-a-r15-20261018/pe_identity.py")
    mapper = CAMPAIGN / "artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"
    commands = []
    environment = dict(os.environ, DOTCC_CAMPAIGN_COMMAND_LOCKS="production", DOTNET_PROCESSOR_COUNT="2",
        DOTNET_CLI_USE_MSBUILD_SERVER="0", MSBUILDDISABLENODEREUSE="1", UseSharedCompilation="false",
        DOTCC_B29_ORIGINAL_FOUR="1", DOTCC_COMPILER=tools["driver"]["path"],
        DOTCC_POSTPROCESSOR=tools["postprocessor"]["path"])

    def run(argv, label, timeout=180, check=True, overrides=None):
        env = dict(environment, **(overrides or {}))
        record = {"declared_environment": {key: value for key, value in env.items()
            if key.startswith(("DOTCC_", "DOTNET_", "COMPlus_", "MSBUILD")) or key == "UseSharedCompilation"}}
        commands.append(record)
        try:
            execute(argv, label=label, cwd=Path("/tmp"), output=ARTIFACTS / (label + ".log"),
                    timeout=timeout, env=env, record=record, check=check)
        finally:
            write("commands.json", commands)
        print(json.dumps({"label": label, "status": record["status"],
                          "exit_code": record.get("exit_code"), "seconds": record.get("seconds")}), flush=True)
        return record

    def bind(pe, actual_project, products):
        return {"identity": pe_reader.pe_identity(pe),
            "source_binding": binding.bind(CAMPAIGN, pe, actual_project, mapper, products)}

    build_arguments = ["-c", "Release", "--disable-build-servers", "-m:1", "-p:UseSharedCompilation=false"]
    discovery_project = CAMPAIGN / "tests/RawDiscoveryR30/RawDiscoveryR30.csproj"
    products, output = WORK / "discovery-products", WORK / "discovery-app"
    run(["dotnet", "build", discovery_project, *build_arguments, f"-p:CPythonProject={project}",
         "--artifacts-path", products, "-o", output, "-p:EmitCompilerGeneratedFiles=true"],
        "actual-raw-consumer-build", timeout=240)
    pe = output / "RawDiscoveryR30.dll"
    actual = {"entry": bind(pe, discovery_project, products),
        "library": bind(output / "TranslatedCPython.dll", project, products),
        "facade": bind(output / "Managed.CPython.dll", CAMPAIGN / "src/Managed.CPython/Managed.CPython.csproj", products),
        "runtime_configuration": item(pe.with_suffix(".runtimeconfig.json")),
        "dependency_manifest": item(pe.with_suffix(".deps.json")),
        "references": [item(path) for path in sorted(output.glob("*.dll"))],
        "stat_source": stat_section(project.parent, runtime), "tools": tools,
        "source_capture": authority["capture"], "publication": authority["raw_publication"],
        "suite_runner": item(output / "upstream/run_inventory.py"),
        "scope": "Own actual entry/facade/library PE/PDB/Compile/runtime binding; final D10/ILC/AOT deferred"}
    assert digest(output / "upstream/run_inventory.py") == digest(CAMPAIGN / "tests/Upstream/run_inventory.py")
    assert digest(output / "controls/frozen_startup_checks_r29.py") == digest(CAMPAIGN / "tests/Abi/frozen_startup_checks_r29.py")
    source_snapshot = {row["path"]: row["sourceSha256"] for name in ("entry", "library", "facade")
                       for row in actual[name]["source_binding"]["documents"]}
    stat_document = actual["stat_source"]["actual_generated_source"]
    assert any(row["path"] == stat_document["path"] and row["sourceSha256"] == stat_document["sha256"]
               for row in actual["library"]["source_binding"]["documents"])
    write("actual-raw-consumer-preexecution.json", actual)
    library = output / "cpython/stdlib"
    for source in preparation["upstream_module_inputs"]:
        assert digest(library / source["relative"]) == source["sha256"], source["relative"]
    run(["dotnet", pe, "startup", library, inventory_path, ARTIFACTS / "owning-startup.json"],
        "actual-owning-startup", timeout=120)
    startup = read(ARTIFACTS / "owning-startup.json")
    assert startup["cycles"] == 2 and startup["callbacks"] == 2 and startup["originalFrozenFinderExecutions"] == 8
    native = preparation["native_python"]
    assert digest(native["path"]) == native["sha256"]
    fixture = CAMPAIGN / "tests/RawReplayR30/pyc_permission_r30.py"
    run([native["path"], "-I", "-B", fixture], "native-pyc-permission", timeout=120,
        overrides={"DOTCC_B30_PYC_ROOT": str(WORK / "native-pyc-files")})
    replay_project = CAMPAIGN / "tests/RawReplayR30/RawReplayR30.csproj"
    replay_products, replay_output = WORK / "replay-products", WORK / "replay-app"
    run(["dotnet", "build", replay_project, *build_arguments, f"-p:ActualInputDirectory={output}",
         "--artifacts-path", replay_products, "-o", replay_output], "actual-replay-build", timeout=120)
    replay_pe = replay_output / "RawReplayR30.dll"
    replay = bind(replay_pe, replay_project, replay_products)
    for name in ("TranslatedCPython.dll", "Managed.CPython.dll"):
        assert digest(replay_output / name) == digest(output / name)
    write("actual-replay-preexecution.json", {"entry": replay,
        "references": [item(path) for path in sorted(replay_output.glob("*.dll"))],
        "runtime_configuration": item(replay_pe.with_suffix(".runtimeconfig.json")),
        "dependency_manifest": item(replay_pe.with_suffix(".deps.json")), "actual_product": item(pe)})
    replay_args = ["dotnet", replay_pe, library, output / "upstream"]
    run([*replay_args, fixture], "actual-pyc-permission", timeout=120,
        overrides={"DOTCC_B30_PYC_ROOT": str(WORK / "actual-pyc-files")})
    write("pyc-permission-parity.json", compare_pyc(pyc_observation(ARTIFACTS / "native-pyc-permission.log"),
        pyc_observation(ARTIFACTS / "actual-pyc-permission.log")))
    focused = HERE / "focused_importlib.py"
    for selection, label in (("first", "original-first-importlib"), ("prior_errors", "prior38-importlib-errors")):
        run([*replay_args, focused], label, timeout=120, check=False, overrides={
            "DOTCC_B32_SELECTION": str(ARTIFACTS / "focused-selection.json"),
            "DOTCC_B32_FOCUS": selection, "DOTCC_B32_REPORT": str(ARTIFACTS / (label + ".json"))})
    # No exhaustive matrix/negative/metadata detour before actual full discovery.
    suite_path = ARTIFACTS / "raw-jit-required-suite.json"
    try:
        run(["dotnet", pe, "suite", library, inventory_path, suite_path], "actual-full-required-raw-jit",
            timeout=900, check=False)
    finally:
        counts = prior.count_journal(suite_path, inventory)
        journal = suite_path.with_suffix(".journal.jsonl")
        events = [json.loads(line) for line in journal.read_text().splitlines()] if journal.exists() else []
        required = set(inventory["required_cases"])
        counts["unexpected_case_ids"] = sorted({row["case"] for row in events} - required)
        counts["unique_by_status"] = {status: len({row["case"] for row in events
            if row["status"] == status and row["case"] in required})
            for status in sorted({row["status"] for row in events})}
        counts["actual_execution_command"] = commands[-1]
        write("required-suite-journal-counts.json", counts)
        assert all(digest(path) == sha for path, sha in source_snapshot.items()), "Actual Compile input changed"
        assert all(digest(row["path"]) == row["sha256"] for row in actual["references"]), "Actual reference changed"
        assert all(digest(row["path"]) == row["sha256"] for row in preparation["frozen_execution_inputs"])
    print(json.dumps({key: value for key, value in counts.items()
        if key not in ("nonpass_case_ids", "actual_execution_command")}), flush=True)
    write("actual-suite-outcome.json", {"status": commands[-1]["status"],
        "exit_code": commands[-1]["exit_code"], "journal_counts": item(ARTIFACTS / "required-suite-journal-counts.json"),
        "first_case_failure": counts["first_case_failure"],
        "started_without_outcome": counts["started_without_outcome"],
        "scope": "Actual required RAW JIT outcome only; every other final acceptance gate remains open"})
    return 0 if commands[-1]["status"] == "passed" else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception:
        ARTIFACTS.mkdir(parents=True, exist_ok=True)
        write("first-blocker.json", {"status": "failed before complete discovery", "traceback": traceback.format_exc(),
            "commands": item(ARTIFACTS / "commands.json") if (ARTIFACTS / "commands.json").exists() else None,
            "scope": "Actual command/input blocker; no unrun required cases become passes"})
        raise
