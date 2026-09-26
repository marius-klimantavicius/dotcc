#!/usr/bin/env python3
"""One larger-workload budget assessment of an immutable failed Kestrel baseline."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/KestrelGuestExecution/budget-assessment.py']

import difflib
import hashlib
import json
import os
from pathlib import Path
import runpy
import shutil
import signal
import subprocess
import sys
import tempfile
import time

BLINK = Path(__file__).resolve().parents[2]
BASE = BLINK / "artifacts/kestrel-guest-execution/attempt-md2uqfje"
BASE_SHA = _CAMPAIGN_INPUTS['BASE_SHA']
HELPER_SHA = _CAMPAIGN_INPUTS['HELPER_SHA']


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    if sha(BASE / "receipt.json") != BASE_SHA:
        raise RuntimeError("Baseline receipt identity differs")
    baseline = json.loads((BASE / "receipt.json").read_text())
    helper = BASE / "private/tests/KestrelGuestExecution/run.py"
    if sha(helper) != HELPER_SHA:
        raise RuntimeError("Frozen helper identity differs")
    functions = runpy.run_path(str(helper))
    manifest, cleanup = functions["manifest"], functions["cleanup"]
    attempt = Path(tempfile.mkdtemp(prefix="attempt-budget-", dir=BASE.parent))
    (attempt / "tmp").mkdir()
    receipt = dict(passed=False, diagnostic_completed=False, inputs={}, commands=[],
                   baseline=dict(path=str(BASE / "receipt.json"), sha256=BASE_SHA),
                   limits=dict(instructions=1_000_000_000, wall_seconds=120, outer_seconds=140),
                   scope="One normal Kestrel startup budget assessment; unchanged immutable guest/generated/Host/owner")
    print(attempt, flush=True)
    env = dict(os.environ, LC_ALL="C", TMPDIR=str(attempt / "tmp"),
               MSBUILDDISABLENODEREUSE="1", DOTNET_CLI_USE_MSBUILD_SERVER="0")
    receipt["host_environment"] = {k: env.get(k) for k in baseline["host_environment"]}

    def save():
        (attempt / "receipt.tmp").write_text(json.dumps(receipt, indent=2) + "\n")
        (attempt / "receipt.tmp").replace(attempt / "receipt.json")

    def pin(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError("Input identity differs: " + str(path))
        if receipt["inputs"].setdefault(str(path), digest) != digest:
            raise RuntimeError("Input changed: " + str(path))
        return path

    def check():
        for path, digest in receipt["inputs"].items():
            if sha(path) != digest:
                raise RuntimeError("Frozen input changed: " + path)
        for name, expected in receipt.get("prepared_trees", {}).items():
            if manifest(attempt / name) != expected:
                raise RuntimeError("Private source closure changed: " + name)

    def run(argv, label, timeout, required=True):
        check()
        row = dict(label=label, argv=list(map(str, argv)), timeout_seconds=timeout)
        receipt["commands"].append(row)
        output, errors = attempt / (label + ".stdout"), attempt / (label + ".stderr")
        process = None
        start = time.monotonic()
        save()
        try:
            with output.open("wb") as out, errors.open("wb") as err:
                process = subprocess.Popen(row["argv"], cwd=attempt, env=env, stdin=subprocess.DEVNULL,
                    stdout=out, stderr=err, start_new_session=True)
                row["exit_code"] = process.wait(timeout=timeout)
        finally:
            if process is not None:
                row["cleanup"] = cleanup(process)
            row["seconds"] = time.monotonic() - start
            row["files"] = {str(p): sha(p) for p in (output, errors) if p.exists()}
            save()
        if row["cleanup"]["signals"] or not row["cleanup"]["group_gone"]:
            raise RuntimeError("Command required forced cleanup")
        if required and row["exit_code"] != 0:
            raise RuntimeError("Command failed: " + label)
        check()

    try:
        pin(BASE / "receipt.json", BASE_SHA)
        pin(helper, HELPER_SHA)
        pin(__file__)
        shutil.copyfile(__file__, attempt / "budget-assessment.py")
        pin(attempt / "budget-assessment.py", sha(__file__))
        pin(sys.executable)
        before_trees = {}
        for name in ("private", "image", "oracle"):
            expected = baseline["source_trees"][str(BASE / name)]
            if manifest(BASE / name) != expected:
                raise RuntimeError("Immutable baseline closure differs: " + name)
            before_trees[name] = expected
            shutil.copytree(BASE / name, attempt / name, ignore=shutil.ignore_patterns("bin", "obj", "__pycache__"))
            if manifest(attempt / name) != expected:
                raise RuntimeError("Baseline copy differs")
            for relative, digest in expected.items():
                pin(BASE / name / relative, digest)
        program = attempt / "private/tests/KestrelGuestExecution/Program.cs"
        before = program.read_text()
        if before.count("100_000_000") != 1 or before.count("TimeSpan.FromSeconds(60)") != 1:
            raise RuntimeError("Unreviewed source derivation")
        after = before.replace("100_000_000", "1_000_000_000").replace("TimeSpan.FromSeconds(60)", "TimeSpan.FromSeconds(120)")
        program.write_text(after)
        receipt["derivation"] = dict(path="private/tests/KestrelGuestExecution/Program.cs",
            before_sha256=hashlib.sha256(before.encode()).hexdigest(), after_sha256=sha(program),
            diff="".join(difflib.unified_diff(before.splitlines(True), after.splitlines(True), fromfile="baseline/Program.cs", tofile="assessment/Program.cs")))
        receipt["prepared_trees"] = {name: manifest(attempt / name) for name in before_trees}
        expected_private = dict(before_trees["private"])
        expected_private["tests/KestrelGuestExecution/Program.cs"] = sha(program)
        if receipt["prepared_trees"]["private"] != expected_private:
            raise RuntimeError("Additional source changed")
        for original, digest in baseline["optional_inputs"].items():
            if digest is not None:
                source = pin(BASE / Path(original).name, digest)
                shutil.copyfile(source, attempt / source.name)
                pin(attempt / source.name, digest)
        dotnet = Path(shutil.which("dotnet")).resolve()
        pin(dotnet, baseline["inputs"][str(dotnet)])
        # Recheck precisely the SDK/runtime binaries selected by the original runner.
        for name, digest in baseline["inputs"].items():
            path = Path(name)
            if path.is_relative_to(dotnet.parent):
                pin(path, digest)
        project = attempt / "private/tests/KestrelGuestExecution/KestrelGuestExecution.csproj"
        run([dotnet, "build", project, "-c", "Release", "--disable-build-servers", "-p:UseSharedCompilation=false"], "build", 600)
        original = project.parent / "bin/Release/net10.0"
        execution = attempt / "execution"
        receipt["binaries_before"] = manifest(original, outputs=True)
        shutil.copytree(original, execution)
        if manifest(execution, outputs=True) != receipt["binaries_before"] or manifest(original, outputs=True) != receipt["binaries_before"]:
            raise RuntimeError("Execution binary copy differs")
        result_dir = attempt / "result"
        run([dotnet, execution / "KestrelGuestExecution.dll", attempt / "image", attempt / "oracle", result_dir], "optimized-jit", 140, required=False)
        receipt["binaries_after"] = manifest(execution, outputs=True)
        if receipt["binaries_after"] != receipt["binaries_before"]:
            raise RuntimeError("Execution binary closure changed")
        result_path = result_dir / "result.json"
        if result_path.exists():
            result = json.loads(result_path.read_text())
            receipt["result"], receipt["result_sha256"] = result, sha(result_path)
            receipt["diagnostic_completed"] = True
            observed = result.get("execution") or {}
            receipt["passed"] = bool(receipt["commands"][-1]["exit_code"] == 0 and
                all(result.get(k) is True for k in ("guest_passed", "ready", "joined", "is_quiescent", "io_disposed")) and
                all(result.get(k) is None for k in ("diagnostic_error", "execution_error", "notification_error")) and
                observed.get("exited") is True and observed.get("exit_status") == 0 and observed.get("stop_reason") == "None" and
                observed.get("memory_released") is True and observed.get("all_workers_joined") is True and
                0 < observed.get("instructions", 0) <= 1_000_000_000 and
                len(observed.get("threads", [])) > 0 and all(t["machine_released"] and t["signal"] == 0 and t["halt"] == 0 for t in observed["threads"]) and
                [r["name"] for r in result["cases"]] == ["health", "large", "fragmented", "missing", "stop"] and
                all(r["passed"] for r in result["cases"]))
        check()
        receipt["final_identities_stable"] = True
    except BaseException as error:
        receipt["passed"] = False
        receipt["error"] = repr(error)
        raise
    finally:
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*")
            if p.is_file() and p.name not in ("receipt.json", "receipt.tmp") and
            p.suffix in (".json", ".stdout", ".stderr", ".txt", ".response")}
        save()
        print(json.dumps({"passed": receipt["passed"], "diagnostic_completed": receipt["diagnostic_completed"],
                          "receipt": str(attempt / "receipt.json")}), flush=True)
    return 0 if receipt["passed"] else 1


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, lambda n, f: (_ for _ in ()).throw(InterruptedError(str(n))))
    raise SystemExit(main())
