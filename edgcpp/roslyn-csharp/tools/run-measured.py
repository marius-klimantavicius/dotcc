#!/usr/bin/env python3
"""Run one acceptance command with frozen wall/RSS/log budgets and a receipt."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import resource
import signal
import subprocess
import sys
import time


def process_tree_rss(root_pid: int) -> int:
    """Sum live Linux RSS for the command's descendants (shared pages count twice)."""
    pending = [root_pid]
    visited: set[int] = set()
    total = 0
    while pending:
        pid = pending.pop()
        if pid in visited:
            continue
        visited.add(pid)
        try:
            text = Path(f"/proc/{pid}/status").read_text()
            for line in text.splitlines():
                if line.startswith("VmRSS:"):
                    total += int(line.split()[1]) * 1024
                    break
            children = Path(f"/proc/{pid}/task/{pid}/children").read_text()
            pending.extend(map(int, children.split()))
        except (FileNotFoundError, ProcessLookupError, PermissionError):
            pass
    return total


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--timeout-seconds", type=float, required=True)
    parser.add_argument("--memory-bytes", type=int, required=True)
    parser.add_argument("--log-bytes", type=int, default=64 * 1024 * 1024)
    parser.add_argument("command", nargs=argparse.REMAINDER)
    args = parser.parse_args()
    command = args.command[1:] if args.command[:1] == ["--"] else args.command
    if not command or min(args.timeout_seconds, args.memory_bytes, args.log_bytes) <= 0:
        parser.error("a command and positive budgets are required")
    if not Path("/proc/self/status").is_file():
        parser.error("this initial acceptance runner requires Linux /proc")
    args.output_dir.mkdir(parents=True, exist_ok=False)
    stdout_path = args.output_dir / "stdout.txt"
    stderr_path = args.output_dir / "stderr.txt"
    started = time.monotonic()
    peak = 0
    violation = None
    interrupted = False
    with stdout_path.open("wb") as stdout, stderr_path.open("wb") as stderr:
        process = subprocess.Popen(command, stdout=stdout, stderr=stderr, start_new_session=True)
        try:
            while process.poll() is None:
                peak = max(peak, process_tree_rss(process.pid))
                elapsed = time.monotonic() - started
                if elapsed > args.timeout_seconds:
                    violation = "wall-time"
                elif peak > args.memory_bytes:
                    violation = "process-tree-rss"
                elif stdout_path.stat().st_size + stderr_path.stat().st_size > args.log_bytes:
                    violation = "log-size"
                if violation:
                    os.killpg(process.pid, signal.SIGTERM)
                    break
                time.sleep(0.1)
        except KeyboardInterrupt:
            interrupted = True
            os.killpg(process.pid, signal.SIGTERM)
        try:
            exit_code = process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGKILL)
            exit_code = process.wait()
    elapsed = time.monotonic() - started
    waited_peak = resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss * 1024
    if not violation and max(peak, waited_peak) > args.memory_bytes:
        violation = "process-rss"
    if not violation and elapsed > args.timeout_seconds:
        violation = "wall-time"
    if not violation and stdout_path.stat().st_size + stderr_path.stat().st_size > args.log_bytes:
        violation = "log-size"
    logs = []
    for path in (stdout_path, stderr_path):
        with path.open("rb") as stream:
            digest = hashlib.file_digest(stream, "sha256").hexdigest()
        logs.append({"path": path.name, "bytes": path.stat().st_size, "sha256": digest})
    status = "cancelled" if interrupted else "budget-exceeded" if violation else "passed" if exit_code == 0 else "failed"
    report = {
        "status": status,
        "command": command,
        "workingDirectory": str(Path.cwd()),
        "exitCode": exit_code,
        "elapsedSeconds": elapsed,
        "sampledPeakProcessTreeRssBytes": peak,
        "waitedPeakProcessRssBytes": waited_peak,
        "measurement": "Linux /proc live process-tree RSS sampled every 100ms; shared pages may count more than once; short peaks may be missed",
        "budgets": {"timeoutSeconds": args.timeout_seconds, "memoryBytes": args.memory_bytes, "logBytes": args.log_bytes},
        "budgetViolation": violation,
        "logs": logs,
    }
    (args.output_dir / "run.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({k: report[k] for k in ("status", "exitCode", "elapsedSeconds", "sampledPeakProcessTreeRssBytes", "budgetViolation")}))
    return 130 if interrupted else 124 if violation else exit_code if 0 <= exit_code <= 255 else 1


if __name__ == "__main__":
    sys.exit(main())
