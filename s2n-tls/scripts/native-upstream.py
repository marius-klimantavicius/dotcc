"""Execute every required original C case against the pinned native control.

The broader native configuration is recorded verbatim. Conditional branch/count
differences remain findings; this runner never turns skips into product passes.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--case", action="append", help="Exact required C case stem; default all")
    parser.add_argument("--no-build", action="store_true")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    output = (args.output or ROOT / "artifacts/native-upstream" / run_id / "report.json").resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    config = json.loads((ROOT / "config/upstream-tests.json").read_text())
    cases = [case for case in config["cases"] if case["status"].startswith("required") and case["path"].endswith(".c")]
    if args.case:
        if set(args.case) - {Path(case["path"]).stem for case in cases}:
            raise RuntimeError("Unknown required original C case")
        cases = [case for case in cases if Path(case["path"]).stem in args.case]
    build, source = ROOT / "build/native/s2n-adapted", ROOT / "build/native/s2n-source"
    report = {"status": "running", "scope": "pinned native C controls, original assertions; translated product gate separate",
              "commands": [], "upstream": [], "required_c_cases": len(cases)}

    def save():
        output.write_text(json.dumps(report, indent=2) + "\n")

    def digest(path):
        return hashlib.sha256(path.read_bytes()).hexdigest()

    def command(argv, label, timeout=1800, cwd=ROOT):
        log = output.parent / (label + ".log")
        command = list(map(str, argv))
        row = {"argv": command, "cwd": str(cwd), "log": str(log)}
        report["commands"].append(row)
        save()
        with log.open("w") as stream:
            try:
                process = subprocess.run(command, cwd=cwd, stdout=stream, stderr=subprocess.STDOUT,
                                         env={**os.environ, "S2N_DONT_MLOCK": "1"}, timeout=timeout)
                row["exit_code"] = process.returncode
            except subprocess.TimeoutExpired:
                row["exit_code"] = "timeout"
        save()
        return row, log

    try:
        library = build / "lib/libs2n.a"
        if not library.is_file():
            raise RuntimeError("Prepare pinned native controls first; no system TLS backend fallback")
        report["native_library_sha256"] = digest(library)
        report["native_flags"] = (build / "CMakeFiles/s2n.dir/flags.make").read_text()
        if not args.no_build:
            result, _ = command(["cmake", "--build", build, "-j2", "--target", *[Path(case["path"]).stem for case in cases]], "build")
            if result["exit_code"] != 0:
                # Continue with available built cases; missing executables stay
                # failures. The failed build remains a gate failure.
                report["build_failed"] = True
        for case in cases:
            name = Path(case["path"]).stem
            row = {"name": name, "case": case["path"], "status": "failed", "assertions": 0}
            report["upstream"].append(row)
            try:
                original, staged = ROOT / "ref/s2n-tls" / case["path"], source / case["path"]
                if original.read_bytes() != staged.read_bytes():
                    raise RuntimeError("Native case differs from original assertions/source")
                binary = build / "bin" / name
                row.update(original_sha256=digest(original), binary_sha256=digest(binary))
                result, log = command([binary], name, timeout=180, cwd=source / "tests/unit")
                transcript = log.read_text(errors="replace")
                counts = re.findall(r"PASSED\s+(\d+) tests", transcript)
                if result["exit_code"] != 0 or len(counts) != 1 or int(counts[0]) <= 0:
                    raise RuntimeError("Original native assertions failed or omitted a positive count")
                if re.search(r"\bSKIP(?:PED)?\b", transcript, re.I):
                    raise RuntimeError("Native capability/test skip is not an acceptance pass")
                row.update(status="passed", assertions=int(counts[0]), log=str(log))
            except Exception as error:
                row["error"] = str(error)
            save()
        report["counts"] = {status: sum(row["status"] == status for row in report["upstream"]) for status in ("passed", "failed")}
        if not report.get("build_failed") and not report["counts"]["failed"] and report["counts"]["passed"] == len(cases):
            report["status"] = "passed"
        else:
            report["status"] = "failed"
    except Exception as error:
        report.update(status="failed", error=str(error))
    finally:
        save()
    print("REPORT " + str(output))
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
