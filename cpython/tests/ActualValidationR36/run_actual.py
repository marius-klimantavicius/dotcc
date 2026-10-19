"""B36 ordinary actual consumers, gated by A30's direct receipt and SAFE return.

No emission, capture, generated-source patch or publication is performed here.
The unchanged full inventory entry is used separately from focused IO proof.
"""
import argparse
import datetime
import json
import os
from pathlib import Path
import sys
import traceback

from reporting import count_journal, item

ROOT = Path(__file__).resolve().parents[3]
CPYTHON = ROOT / "cpython"
HERE = Path(__file__).resolve().parent
ART = CPYTHON / "artifacts/owner-b-r36-20261018"
WORK = CPYTHON / "build/owner-b-r36-20261018"


def read(path):
    return json.loads(Path(path).read_text())


def write(name, value):
    path = ART / name
    temporary = path.with_suffix(path.suffix + ".pending")
    temporary.write_text(json.dumps(value, indent=2, default=str) + "\n")
    temporary.replace(path)
    return item(path)


def verify(reference):
    actual = item(reference["path"])
    assert actual["sha256"] == reference["sha256"], reference["path"]
    return actual


def protected_sources():
    references = read(ART / "entry-source-pins.json")
    for reference in references:
        verify(reference)
    return references


def runtime_sections(directory):
    # The minimal pre-Python check binds the four frozen, relevant runtime
    # services, rather than delaying discovery for another full audit matrix.
    sources = {Path(r["path"]).name: r for r in protected_sources()
               if Path(r["path"]).name in ("PosixFsLib.cs", "PosixLib.cs", "PthreadLib.cs", "PthreadAttributesLib.cs")}
    text = "\n".join(p.read_text(encoding="utf-8-sig") for p in sorted(Path(directory).glob("*.cs")))
    for name, source in sources.items():
        marker = "// ---- " + name + " ----\n#nullable enable\n"
        assert text.count(marker) == 1, name
        start = text.index(marker) + len(marker)
        end = text.index("#nullable restore", start)
        expected = "".join(line + "\n" for line in Path(source["path"]).read_text().split("\n")
                           if not line.lstrip().startswith(("#nullable", "namespace ")) and not line.startswith("using ")) + "\n"
        assert text[start:end] == expected, name
    assert len(sources) == 4
    return list(sources.values())


def unchanged_upstream_sources(library):
    previous = read(CPYTHON / "artifacts/owner-b-r34-20261018/full-required-raw-jit.json")
    previous_root = CPYTHON / "build/owner-b-r34-20261018/actual-app/cpython/stdlib"
    old_paths = {Path(d["upstream_path"]) for module in previous["discovery"]
                 for d in module["case_details"]}
    rows, virtual = [], []
    for old in sorted(old_paths):
        if not old.is_relative_to(previous_root) or not old.is_file():
            virtual.append({"reportedPath": str(old),
                            "basis": "Synthetic doctest/dynamic metadata path; no physical source file claimed"})
            continue
        actual = Path(library) / old.relative_to(previous_root)
        assert item(actual)["sha256"] == item(old)["sha256"], str(actual)
        rows.append(item(actual))
    return {"physicalFiles": rows, "syntheticLocations": virtual}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--authority", type=Path, required=True)
    args = parser.parse_args()
    authority = read(args.authority)
    assert authority["safe_heavy_release"] is True
    assert authority["direct_a30_handoff"] is True
    for name in ("publication", "release", "capture"):
        verify(authority[name])
    release = read(authority["release"]["path"])
    assert release["activeOwned"] == [] and release["queuedOwned"] == []
    assert release["terminalPidsAbsent"] and all(not Path("/proc", str(pid)).exists()
                                               for pid in release["terminalPidsAbsent"])
    assert len(release["locks"]) == 3 and all(row["independentAcquireAndRelease"]
                                             and row["freeAtTransfer"] for row in release["locks"])
    verify(authority["same_c_emitted_proof"])
    same_c = read(authority["same_c_emitted_proof"]["path"])
    assert same_c["status"] == "passed" and same_c["checksPerMode"] == 136
    assert same_c["nativeOracle"]["checks"] == 136 and same_c["nativeOracle"]["failures"] == 0
    for reference in (same_c["source"], same_c["jitTranscript"], same_c["aotTranscript"], same_c["nativeOracle"]["log"]):
        verify(reference)
    assert same_c["jitTranscript"]["sha256"] == same_c["aotTranscript"]["sha256"] == same_c["nativeOracle"]["log"]["sha256"]
    assert same_c["capture"]["sha256"] == authority["capture"]["sha256"]
    publication = read(authority["publication"]["path"])
    receipt_ref = publication["receipt"]
    verify(receipt_ref)
    receipt = read(receipt_ref["path"])
    assert receipt["status"] == "passed" and receipt["profile"] == "upstream-tests-static"
    assert receipt["selection"]["form"] == "raw" and len(receipt["units"]) == 286
    assert receipt["selection"]["jobs"] == 8 and receipt["hash_policy"] == "strict"
    assert set(receipt["outputs"]) == {"raw"}
    project = Path(receipt["outputs"]["raw"]["project"])
    verify(publication["actualRawPE"])
    verify(publication["project"])
    previous_publication = read(CPYTHON / "artifacts/owner-a-r29-20261018/v20/raw-publication-handoff.json")
    assert publication["actualRawPE"]["sha256"] != previous_publication["actualRawPE"]["sha256"]
    capture = read(authority["capture"]["path"])
    compiler = capture["tools"]["driver"]["pe"]
    compiler_library = capture["tools"]["compilerLibrary"]["pe"]
    postprocessor = capture["tools"]["postprocessor"]["pe"]
    for tool in (compiler, compiler_library, postprocessor):
        verify(tool)
    assert receipt["tools"]["DotCC"]["dotcc.dll"] == compiler["sha256"]
    assert receipt["tools"]["DotCC"]["DotCC.Lib.dll"] == compiler_library["sha256"]
    protected_sources()
    assert not (ART / "commands.json").exists(), "Retain existing attempt; do not rerun over it"
    WORK.mkdir(parents=True, exist_ok=True)

    sys.path.insert(0, str(ROOT / "Scripts"))
    from campaigns.process import execute
    from campaigns.recipes import helper
    identity = helper(CPYTHON / "build/owner-a-r15-20261018/pe_identity.py")
    binder = helper(CPYTHON / "tests/Audit/actual_profile_input.py")
    mapper = CPYTHON / "artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"
    environment = dict(os.environ, DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_USE_MSBUILD_SERVER="0",
                       MSBUILDDISABLENODEREUSE="1", UseSharedCompilation="false",
                       DOTCC_B29_ORIGINAL_FOUR="1")
    # Keep the same ordinary lock/process protocol as B34. The authority is
    # checked before any command attempts to acquire a shared execution lock.
    commands = []

    def run(argv, label, timeout=120, check=True, overrides=None):
        env = dict(environment, **(overrides or {}))
        record = {"declared_environment": {k: v for k, v in env.items()
                  if k.startswith(("DOTCC_", "DOTNET_", "MSBUILD")) or k == "UseSharedCompilation"}}
        commands.append(record)
        argv = ["flock", "-n", "--close", "/tmp/dotcc-cpython-shared-build.lock", *argv]
        try:
            execute(argv, label=label, cwd=Path("/tmp"), output=ART / (label + ".log"),
                    timeout=timeout, env=env, record=record, check=check)
        finally:
            write("commands.json", commands)
        print(json.dumps({"label": label, "status": record["status"], "exit_code": record.get("exit_code"),
                          "seconds": record.get("seconds")}), flush=True)
        return record

    def bind(pe, project, products):
        return {"identity": identity.pe_identity(pe),
                "sourceBinding": binder.bind(CPYTHON, pe, project, mapper, products)}

    write("actual-entry.json", {"authority": item(args.authority), "receipt": receipt_ref,
          "utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
          "scope": "Minimal actual input binding, separate exact12 IO proof, immediate full4867 RAW JIT"})
    products, output = WORK / "actual-products", WORK / "actual-app"
    build_args = ["-c", "Release", "--disable-build-servers", "-m:1", "-p:UseSharedCompilation=false",
                  "-nodeReuse:false", "-p:NuGetAudit=false", "-p:RestoreIgnoreFailedSources=true"]
    discovery = CPYTHON / "tests/RawDiscoveryR30/RawDiscoveryR30.csproj"
    run(["dotnet", "build", discovery, *build_args, f"-p:CPythonProject={project}",
         "--artifacts-path", products, "-o", output], "actual-entry-build", timeout=240)
    entry = output / "RawDiscoveryR30.dll"
    proof = {"publication": authority["publication"], "capture": authority["capture"],
             "entry": bind(entry, discovery, products),
             "facade": bind(output / "Managed.CPython.dll", CPYTHON / "src/Managed.CPython/Managed.CPython.csproj", products),
             "library": bind(output / "TranslatedCPython.dll", project, products),
             "runtimeSections": runtime_sections(project.parent),
             "runtimeConfiguration": item(entry.with_suffix(".runtimeconfig.json")),
             "dependencyManifest": item(entry.with_suffix(".deps.json")),
             "references": [item(p) for p in sorted(output.glob("*.dll"))],
             "runner": item(output / "upstream/run_inventory.py"),
             "unchangedUpstreamSources": unchanged_upstream_sources(output / "cpython/stdlib"),
             "inventory": item(CPYTHON / "tests/Upstream/inventory.frozen.json"),
             "tools": [compiler, compiler_library, postprocessor],
             "reusedSameCNativeJitAotProof": authority["same_c_emitted_proof"],
             "scope": "Own actual consumer PE/PDB/Compile/PathMap bytes; different PE from canonical remains separate"}
    assert proof["runner"]["sha256"] == item(CPYTHON / "tests/Upstream/run_inventory.py")["sha256"]
    write("actual-input-proof.json", proof)
    library = output / "cpython/stdlib"
    inventory = CPYTHON / "tests/Upstream/inventory.frozen.json"

    # This reference-only diagnostic entry consumes the exact bound actual
    # assemblies; it does not rebuild or replace the interpreter/facade.
    replay_project = CPYTHON / "tests/RawReplayR30/RawReplayR30.csproj"
    replay_products, replay_output = WORK / "replay-products", WORK / "replay-app"
    run(["dotnet", "build", replay_project, *build_args, f"-p:ActualInputDirectory={output}",
         "--artifacts-path", replay_products, "-o", replay_output], "focused-entry-build")
    for name in ("Managed.CPython.dll", "TranslatedCPython.dll"):
        assert item(replay_output / name)["sha256"] == item(output / name)["sha256"]
    write("focused-input-proof.json", {"entry": bind(replay_output / "RawReplayR30.dll", replay_project, replay_products),
          "actualProductProof": item(ART / "actual-input-proof.json"),
          "focusedScript": item(HERE / "focused_io.py"), "sourceHandoff": item(CPYTHON / "artifacts/owner-b-r34-20261018/io-failure-handoff-b35.json")})
    focused = run(["dotnet", replay_output / "RawReplayR30.dll", library, output / "upstream", HERE / "focused_io.py"],
                 "focused-original12-io", check=False,
                 overrides={"DOTCC_B36_FOCUSED_REPORT": str(ART / "focused-original12-io.json"),
                            "DOTCC_B36_IO_HANDOFF": str(CPYTHON / "artifacts/owner-b-r34-20261018/io-failure-handoff-b35.json")})
    # A focused failure remains evidence; always execute the whole frozen suite
    # unless interpreter/input integrity itself failed.
    report = ART / "full-required-raw-jit.json"
    try:
        full = run(["dotnet", entry, "suite", library, inventory, report], "full-required-raw-jit", timeout=900, check=False)
    finally:
        journal = report.with_suffix(".journal.jsonl")
        if journal.exists():
            write("full-required-journal-counts.json", count_journal(journal, read(inventory)["required_cases"]))
        protected_sources()
        for reference in proof["references"]:
            verify(reference)
        for reference in proof["unchangedUpstreamSources"]["physicalFiles"]:
            verify(reference)
        for name in ("entry", "facade", "library"):
            for document in proof[name]["sourceBinding"]["documents"]:
                assert item(document["path"])["sha256"] == document["sourceSha256"]
    counts = read(ART / "full-required-journal-counts.json")
    ordinary = read(report) if report.exists() else None
    if ordinary is not None:
        assert ordinary["selected_case_ids"] == read(inventory)["required_cases"]
        for status, count in counts["outcomeEvents"].items():
            assert ordinary["counts"][status] == count, (status, count)
    complete = (counts["uniqueStarted"] == counts["uniqueCompleted"] == counts["required"] == 4867
                and not counts["unexpectedIds"] and not counts["outcomesWithoutStart"])
    write("actual-run-result.json", {"focusedCommand": focused, "fullCommand": full,
          "allRequiredIdsCompleted": complete,
          "counts": item(ART / "full-required-journal-counts.json"), "inputProof": item(ART / "actual-input-proof.json"),
          "scope": "Actual RAW JIT full run observed; required markers and final campaign gates remain OPEN"})
    print(json.dumps({"allRequiredIdsCompleted": complete, "uniqueOutcomes": counts["uniqueOutcomes"],
                      "outcomeEvents": counts["outcomeEvents"], "withoutCompletedOutcome": counts["withoutCompletedOutcome"]}), flush=True)


if __name__ == "__main__":
    try:
        main()
    except BaseException:
        ART.mkdir(parents=True, exist_ok=True)
        write("actual-blocker.json", {"status": "actual command/input/harness blocker", "traceback": traceback.format_exc()})
        raise
