#!/usr/bin/env python3
"""Run one frozen static-musl HTTP witness with explicit NativeAOT GC settings."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/DotNetService/native-gc-profile.py']

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import runpy
import shutil
import signal
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[3]
BUILD_RECEIPT_SHA = _CAMPAIGN_INPUTS['BUILD_RECEIPT_SHA']
BINARY_SHA = _CAMPAIGN_INPUTS['BINARY_SHA']
ORACLE_SHA = _CAMPAIGN_INPUTS['ORACLE_SHA']
ENVIRONMENT = {"LANG": "C", "DOTNET_GCHeapHardLimit": "1000000",
               "DOTNET_GCRegionRange": "2000000", "DOTNET_GCRegionSize": "100000"}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def interrupted(number, frame):
    raise InterruptedError(f"received signal {number}")


def group_exists(pid):
    try:
        os.killpg(pid, 0)
        return True
    except ProcessLookupError:
        return False


def cleanup(process, receipt):
    # Drain the entire owned strace/guest group even if its leader already exited.
    signal.alarm(0)
    row = {"process_group": process.pid, "signals": []}
    receipt.setdefault("cleanup", []).append(row)
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        process.poll()
        if not group_exists(process.pid):
            break
        try:
            os.killpg(process.pid, sig)
            row["signals"].append(sig.name)
        except ProcessLookupError:
            break
        deadline = time.monotonic() + seconds
        while time.monotonic() < deadline:
            process.poll()
            if not group_exists(process.pid):
                break
            time.sleep(0.05)
    row["leader_exit_code"] = process.poll()
    row["group_gone"] = not group_exists(process.pid)
    if not row["group_gone"] or row["leader_exit_code"] is None:
        raise RuntimeError("owned native process group did not finish cleanup")


def trace_observations(path):
    lines = path.read_text().splitlines()
    mappings = []
    for line in lines:
        match = re.match(r"^(\d+)\s+mmap\(([^,]+), (\d+), ([^,]+), (.*)\)\s+=\s+(.*)$", line)
        if match:
            mappings.append({"tid": int(match[1]), "address_argument": match[2],
                             "length": int(match[3]), "protection": match[4],
                             "remaining_arguments": match[5], "result": match[6],
                             "success": bool(re.match(r"0x[0-9a-f]+$", match[6])), "line": line})
    return {"line_count": len(lines), "mmap_completed_rows": mappings,
            "successful_prot_none_lengths": [m["length"] for m in mappings
                if m["success"] and m["protection"] == "PROT_NONE"],
            "observed_tids": sorted({int(m[1]) for line in lines
                                     if (m := re.match(r"^(\d+)\s", line))}),
            "thread_rows": [line for line in lines if re.search(
                r"\b(?:clone|clone3|set_tid_address|set_robust_list|futex)\(|<\.\.\. (?:clone|clone3|futex) resumed>", line)],
            "unfinished_mmap_rows": [line for line in lines if "mmap(" in line and "<unfinished ...>" in line],
            "interpretation": "Observed mmap requests, not attribution to GC or total live/committed memory; full trace retained."}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--musl-receipt", type=Path, default=ROOT / "blink/artifacts/dotnet-guest-musl/attempt-8za50rji/receipt.json")
    args = parser.parse_args()
    base = ROOT / "blink/artifacts/dotnet-guest-gc-profile"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    receipt = {"passed": False, "attempt": str(attempt), "environment": ENVIRONMENT,
               "scope": "one native Linux frozen-musl HTTP witness; no build, Blink execution or memory-quota qualification",
               "limits": {"overall_execution_seconds": 60, "ready_seconds": 20,
                          "per_http_response_seconds": 15, "response_bytes": 8192,
                          "cleanup_term_seconds": 3, "cleanup_kill_seconds": 5},
               "tools": {}, "inputs": {}, "runner_sha256": sha(__file__)}
    started = time.monotonic()
    print(attempt, flush=True)

    def pin(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError(f"input identity mismatch: {path}")
        receipt["inputs"][str(path)] = digest
        return path

    try:
        source_receipt = pin(args.musl_receipt, BUILD_RECEIPT_SHA)
        original = json.loads(source_receipt.read_text())
        if not all(original.get(key) is True for key in ("passed", "final_identities_stable", "static_elf_verified")):
            raise RuntimeError("musl producer is not qualified")
        producer = source_receipt.parent
        receipt["producer"] = {"receipt": str(source_receipt), "sha256": BUILD_RECEIPT_SHA,
                               "runtime": original["runtime"], "image": original["image"]}
        binary = pin(original["binary"]["path"], BINARY_SHA)
        receipt["binary"] = {"path": str(binary), "sha256": BINARY_SHA, "size": binary.stat().st_size}
        pin(__file__)
        shutil.copy2(__file__, attempt / "runner.py")
        shutil.copy2(source_receipt, attempt / "producer-receipt.json")
        for relative, digest in original["frozen_inputs"].items():
            pin(producer / relative, digest)
        for relative in ("health.request", "health.response", "stop.request", "stop.response"):
            pin(producer / relative, original["artifacts"][relative])
        oracle_path = pin(producer / "runner.py", ORACLE_SHA)
        shutil.copy2(oracle_path, attempt / "oracle.py")
        pin(attempt / "oracle.py", ORACLE_SHA)
        pin(attempt / "runner.py", receipt["runner_sha256"])
        pin(attempt / "producer-receipt.json", BUILD_RECEIPT_SHA)
        strace = pin(original["tools"]["strace"]["resolved"], original["tools"]["strace"]["sha256"])
        python = pin(sys.executable)
        receipt["tools"] = {"strace": {"path": str(strace), "sha256": sha(strace)},
                            "python": {"path": str(python), "sha256": sha(python), "version": sys.version}}
        receipt["oracle"] = {"source": str(oracle_path), "sha256": ORACLE_SHA,
                             "function": "native_oracle", "cleanup_override": "whole owned process group; no request/response changes"}
        oracle = runpy.run_path(str(attempt / "oracle.py"), run_name="frozen_musl_oracle")["native_oracle"]
        oracle.__globals__["stop"] = lambda process: cleanup(process, receipt)
        signal.alarm(60)
        try:
            oracle(attempt, binary, receipt, dict(ENVIRONMENT))
        finally:
            signal.alarm(0)
        # Require the original exact HTTP bytes, independently of the oracle's comparisons.
        for relative in ("health.request", "health.response", "stop.request", "stop.response"):
            if sha(attempt / relative) != original["artifacts"][relative]:
                raise RuntimeError(f"HTTP oracle bytes differ: {relative}")
        if [row["path"] for row in receipt["native_cases"]] != ["/health", "/stop"]:
            raise RuntimeError("native HTTP case inventory differs")
        receipt["observations"] = trace_observations(attempt / "native.strace")
        for path, digest in receipt["inputs"].items():
            if sha(path) != digest:
                raise RuntimeError(f"input changed during native witness: {path}")
        receipt["final_identities_stable"] = True
        receipt["passed"] = True
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
    finally:
        signal.alarm(0)
        receipt["elapsed_seconds"] = time.monotonic() - started
        receipt["artifacts"] = {p.name: sha(p) for p in attempt.iterdir() if p.is_file()
                                and p.name not in ("receipt.json", "receipt.tmp")}
        temporary = attempt / "receipt.tmp"
        temporary.write_text(json.dumps(receipt, indent=2) + "\n")
        temporary.replace(attempt / "receipt.json")
    print(json.dumps({"passed": receipt["passed"], "receipt": str(attempt / "receipt.json")}), flush=True)
    return 0 if receipt["passed"] else 1


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    signal.signal(signal.SIGALRM, interrupted)
    sys.exit(main())
