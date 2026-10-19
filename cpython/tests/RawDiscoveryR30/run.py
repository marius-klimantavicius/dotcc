"""B30 early RAW discovery. Invoke only after A26's direct safe heavy release.

Uses the framework process/lock boundary, ordinary project references, the
unchanged inventory runner and existing read-only PE/PDB mappers. This does not
relax the final acceptance auditor or qualify processed/AOT products.
"""
import argparse
from collections import Counter, defaultdict
import datetime
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import sys
import traceback

ROOT = Path(__file__).resolve().parents[3]
CAMPAIGN = ROOT / "cpython"
HERE = Path(__file__).resolve().parent
ARTIFACTS = CAMPAIGN / "artifacts/owner-b-r30-20261018"
WORK = CAMPAIGN / "build/owner-b-r30-20261018"


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": digest(path)}


def read(path):
    return json.loads(Path(path).read_text())


def write(name, value):
    path = ARTIFACTS / name
    temporary = path.with_suffix(path.suffix + ".pending")
    temporary.write_text(json.dumps(value, indent=2, default=str) + "\n")
    temporary.replace(path)
    return item(path)


def module(path):
    spec = importlib.util.spec_from_file_location(Path(path).stem, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


def count_journal(report_path, inventory):
    """Count durable case events even if a later case aborts the process."""
    journal = report_path.with_suffix(".journal.jsonl")
    events = [json.loads(line) for line in journal.read_text().splitlines()] if journal.exists() else []
    started, outcomes = set(), defaultdict(list)
    for row in events:
        if row["status"] == "started":
            started.add(row["case"])
        else:
            outcomes[row["case"]].append(row)
    required = set(inventory["required_cases"])
    return {
        "journal": item(journal) if journal.exists() else None,
        "event_counts": dict(Counter(row["status"] for row in events)),
        "unique_started": len(started), "unique_completed": len(outcomes),
        "unique_passed": sum(all(r["status"] == "passed" for r in rows) for rows in outcomes.values()),
        "failed_subtest_ids": sum(any(r["status"] == "failed-subtest" for r in rows) for rows in outcomes.values()),
        "required_without_completed_outcome": len(required - outcomes.keys()),
        "never_started": len(required - started),
        "started_without_outcome": sorted(started - outcomes.keys()),
        "first_nonpass": next((row for row in events if row["status"] not in ("started", "passed")), None),
        "first_case_failure": next((row for row in events if row["status"] in
            ("failed", "error", "failed-subtest", "interrupted", "unexpected-success")), None),
        "nonpass_case_ids": [case for case, rows in outcomes.items() if any(r["status"] != "passed" for r in rows)],
        "ordinary_report": item(report_path) if report_path.exists() else None,
        "ordinary_report_counts": read(report_path).get("counts") if report_path.exists() else None,
        "scope": "Unchanged required case journal; final campaign acceptance remains open.",
    }


def runtime_sources(directory):
    """Bind actual emitted runtime sections to V17's captured source inputs."""
    closure_path = CAMPAIGN / "artifacts/owner-a-r26-20261018/capture-source-closure.after.json"
    closure = read(closure_path)
    text = "\n".join(p.read_text(encoding="utf-8-sig") for p in sorted(directory.glob("*.cs")))
    rows = []
    for path in closure["cRuntimeSelection"].values():
        path = Path(path)
        assert digest(path) == closure["sourceFiles"][str(path)]["sha256"]
        marker = "// ---- " + path.name + " ----\n#nullable enable\n"
        assert text.count(marker) == 1, path
        begin = text.index(marker) + len(marker)
        end = text.index("#nullable restore", begin)
        expected = "".join(line + "\n" for line in path.read_text().split("\n")
            if not line.lstrip().startswith(("#nullable", "namespace ")) and not line.startswith("using ")) + "\n"
        assert text[begin:end] == expected, path
        rows.append(item(path))
    return {"capture_closure": item(closure_path), "sections": rows,
            "scope": "Actual RAW runtime source bytes; no D10 or execution acceptance inferred."}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--authority", required=True, type=Path,
                        help="Direct A26 new RAW receipt plus explicit safe heavy release, retained by B30")
    parser.add_argument("--continue-discovery", action="store_true", help="Retain the standalone control blocker and proceed with actual CPython discovery")
    args = parser.parse_args()
    authority = read(args.authority)
    assert authority["safe_heavy_release"] is True
    raw = authority["raw_publication"]
    assert digest(raw["path"]) == raw["sha256"]
    publication = read(raw["path"])
    receipt_ref = publication["receipt"]
    assert digest(receipt_ref["path"]) == receipt_ref["sha256"]
    receipt = read(receipt_ref["path"])
    assert receipt["status"] == "passed" and receipt["profile"] == "upstream-tests-static"
    assert receipt["selection"]["form"] == "raw" and len(receipt["units"]) == 286
    assert set(receipt["outputs"]) == {"raw"} and receipt["hash_policy"] == "strict"
    capture_path = CAMPAIGN / "artifacts/owner-a-r26-20261018/capture-sealed.json"
    capture = read(capture_path)
    compiler = capture["tools"]["driver"]["pe"]
    compiler_library = capture["tools"]["compilerLibrary"]["pe"]
    postprocessor = capture["tools"]["postprocessor"]["pe"]
    for tool in (compiler, compiler_library, postprocessor):
        assert digest(tool["path"]) == tool["sha256"]
    assert receipt["tools"]["DotCC"]["dotcc.dll"] == compiler["sha256"]
    assert receipt["tools"]["DotCC"]["DotCC.Lib.dll"] == compiler_library["sha256"]
    project = Path(receipt["outputs"]["raw"]["project"])
    assert digest(publication["actualRawPE"]["path"]) == publication["actualRawPE"]["sha256"]
    assert digest(CAMPAIGN / "src/Host/dotcc_cpython_host.c") == publication["host"]["sha256"]
    plan = read(ARTIFACTS / "source-preparation.json")
    for source in plan["frozen_execution_inputs"]:
        assert digest(source["path"]) == source["sha256"], source["path"]

    sys.path.insert(0, str(ROOT / "Scripts"))
    from campaigns.process import execute, CommandError
    from campaigns.recipes import helper
    # Both readers are Python-only; importing them cannot build an inspector.
    pe_reader = module(CAMPAIGN / "build/owner-a-r15-20261018/pe_identity.py")
    binding = helper(CAMPAIGN / "tests/Audit/actual_profile_input.py")
    mapper = CAMPAIGN / "artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"
    commands = read(ARTIFACTS / "commands.json") if args.continue_discovery else []
    if args.continue_discovery:
        assert (ARTIFACTS / "standalone-control-blocker.json").is_file()
        assert commands[-1]["label"] == "same-c-getcwd-build" and commands[-1]["status"] == "failed"
    environment = dict(os.environ,
        DOTCC_COMPILER=compiler["path"], DOTCC_POSTPROCESSOR=postprocessor["path"],
        DOTCC_CAMPAIGN_COMMAND_LOCKS="production", DOTNET_PROCESSOR_COUNT="2",
        DOTNET_CLI_USE_MSBUILD_SERVER="0", MSBUILDDISABLENODEREUSE="1",
        UseSharedCompilation="false", DOTCC_B29_ORIGINAL_FOUR="1")

    def run(argv, label, timeout=180, check=True):
        record = {"declared_environment": {k: environment[k] for k in environment
            if k.startswith(("DOTCC_", "DOTNET_", "MSBUILD")) or k == "UseSharedCompilation"}}
        commands.append(record)
        try:
            execute(argv, label=label, cwd=Path("/tmp"), output=ARTIFACTS / (label + ".log"),
                    timeout=timeout, env=environment, record=record, check=check)
        finally:
            write("commands.json", commands)
        print(json.dumps({"label": label, "status": record["status"], "exit_code": record.get("exit_code"),
                          "seconds": record.get("seconds")}), flush=True)
        return record

    def bind(pe, actual_project, products=None):
        return {"identity": pe_reader.pe_identity(pe),
            "source_binding": binding.bind(CAMPAIGN, pe, actual_project, mapper, products)}

    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    WORK.mkdir(parents=True, exist_ok=True)
    assert args.continue_discovery or not (ARTIFACTS / "commands.json").exists(), "Existing attempt must remain retained."
    write("discovery-continuation.json" if args.continue_discovery else "actual-entry.json", {"authority": item(args.authority), "publication": raw,
        "receipt": receipt_ref, "capture": item(capture_path),
        "started_utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "scope": "Minimal actual identity/startup then RAW JIT full required-suite discovery; not D10 acceptance."})
    inventory_path = CAMPAIGN / "tests/Upstream/inventory.frozen.json"
    inventory = read(inventory_path)
    assert inventory["status"] == "frozen" and len(inventory["required_cases"]) == 4867
    build_arguments = ["-c", "Release", "--disable-build-servers", "-m:1", "-p:UseSharedCompilation=false"]
    if not args.continue_discovery:
        fixture = CAMPAIGN / "tests/Abi/getcwd_deleted_r28.c"
        native = WORK / "native-getcwd-deleted"
        run(["cc", "-std=c11", "-Wall", "-Wextra", "-Werror", fixture, "-o", native], "native-getcwd-build")
        run([native], "native-getcwd-run")
        emitted = WORK / "getcwd-emitted"
        run(["dotnet", compiler["path"], fixture, "--emit=csproj", "--runtime=c", "--literal-pool", "-o", emitted],
            "same-c-getcwd-emit")
        generated_projects = list(emitted.glob("*.csproj"))
        assert len(generated_projects) == 1
        getcwd_project = generated_projects[0]
        getcwd_output = WORK / "getcwd-app"
        getcwd_products = WORK / "getcwd-products"
        run(["dotnet", "build", getcwd_project, *build_arguments, "--artifacts-path", getcwd_products,
             "-o", getcwd_output], "same-c-getcwd-build")
        getcwd_pe = next(p for p in getcwd_output.glob("*.dll") if p.with_suffix(".runtimeconfig.json").exists())
        write("same-c-getcwd-preexecution.json", {"source": item(fixture), "compiler": compiler,
            "compiler_library": compiler_library, "actual_input": bind(getcwd_pe, getcwd_project, getcwd_products),
            "runtime_sources": runtime_sources(emitted)})
        run(["dotnet", getcwd_pe], "same-c-getcwd-run")

    discovery_project = HERE / "RawDiscoveryR30.csproj"
    products, output = WORK / "discovery-products", WORK / "discovery-app"
    # One actual ordinary consumer build serves both startup and suite execution.
    run(["dotnet", "build", discovery_project, *build_arguments, f"-p:CPythonProject={project}",
         "--artifacts-path", products, "-o", output, "-p:EmitCompilerGeneratedFiles=true"],
        "actual-raw-consumer-build", timeout=240)
    pe = output / "RawDiscoveryR30.dll"
    facade_project = CAMPAIGN / "src/Managed.CPython/Managed.CPython.csproj"
    actual_binding = {"entry": bind(pe, discovery_project, products),
        "library": bind(output / "TranslatedCPython.dll", project, products),
        "facade": bind(output / "Managed.CPython.dll", facade_project, products),
        "runtime_configuration": item(pe.with_suffix(".runtimeconfig.json")),
        "dependency_manifest": item(pe.with_suffix(".deps.json")),
        "runtime_sources": runtime_sources(project.parent),
        "references": [item(p) for p in sorted(output.glob("*.dll"))],
        "selected_project": item(project), "source_capture": item(capture_path),
        "raw_publication": raw, "suite_runner": item(output / "upstream/run_inventory.py"),
        "scope": "Actual JIT entry/library/facade PE, PDB and compiled sources; final D10/ILC/AOT gates deferred."}
    assert digest(output / "upstream/run_inventory.py") == digest(CAMPAIGN / "tests/Upstream/run_inventory.py")
    assert digest(output / "controls/frozen_startup_checks_r29.py") == digest(CAMPAIGN / "tests/Abi/frozen_startup_checks_r29.py")
    source_snapshot = {row["path"]: row["sourceSha256"]
        for name in ("entry", "library", "facade") for row in actual_binding[name]["source_binding"]["documents"]}
    write("actual-raw-consumer-preexecution.json", actual_binding)
    library = output / "cpython/stdlib"
    assert (library / "test/test_grammar.py").is_file()
    startup_path = ARTIFACTS / "owning-startup.json"
    run(["dotnet", pe, "startup", library, inventory_path, startup_path], "actual-owning-startup", timeout=120)
    startup = read(startup_path)
    prior_layout = read(CAMPAIGN / "artifacts/owner-b-r29-20261018/native-pyconfig-layout.json")
    assert all(startup["layout"][key] == prior_layout[key] for key in prior_layout if key in startup["layout"])
    assert startup["cycles"] == 2 and startup["originalFrozenFinderExecutions"] == 8
    suite_path = ARTIFACTS / "raw-jit-required-suite.json"
    try:
        run(["dotnet", pe, "suite", library, inventory_path, suite_path], "actual-full-required-raw-jit",
            timeout=900, check=False)
    finally:
        write("required-suite-journal-counts.json", count_journal(suite_path, inventory))
        assert all(digest(path) == sha for path, sha in source_snapshot.items()), "Actual compile input changed."
        assert all(digest(row["path"]) == row["sha256"] for row in actual_binding["references"]), "Actual reference changed."
    print(json.dumps(read(ARTIFACTS / "required-suite-journal-counts.json")), flush=True)


if __name__ == "__main__":
    try:
        main()
    except Exception:
        ARTIFACTS.mkdir(parents=True, exist_ok=True)
        write("first-blocker.json", {"status": "failed before complete discovery",
            "traceback": traceback.format_exc(),
            "commands": item(ARTIFACTS / "commands.json") if (ARTIFACTS / "commands.json").exists() else None,
            "scope": "Actual command or input/startup blocker; no skipped required cases become passes."})
        raise
