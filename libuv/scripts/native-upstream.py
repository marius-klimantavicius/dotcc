#!/usr/bin/env python3
"""Preserve native TAP outcomes including skips/failures; never bless failing oracles."""
import json
import os
import re
import shutil
from campaigns.compat import references


def parse_results(listing, output, exit_code):
    names = [line.split()[0] for line in listing.splitlines()
             if re.fullmatch(r"[a-zA-Z_][a-zA-Z_0-9]*(?:\s+\(helpers:.*\))?", line.strip())]
    cases = []
    for line in output.splitlines():
        match = re.match(r"^(not ok|ok)\s+(\d+)\s*-?\s*(.*)", line)
        if not match:
            continue
        detail = match[3]
        cases.append({"number": int(match[2]), "name": detail.split("#")[0].strip(),
                      "status": "skipped" if "# SKIP" in detail.upper() else "passed" if match[1] == "ok" else "failed",
                      "tap": line})
    sequence_ok = [case["number"] for case in cases] == list(range(1, len(names) + 1))
    names_ok = len(set(names)) == len(names) and sorted(case["name"] for case in cases) == sorted(names)
    return {"scope": "native upstream only; managed cases unrun", "exit_code": exit_code,
            "listed": len(names), "sequence_complete": sequence_ok, "names_complete": names_ok,
            "cases": cases, "counts": {status: sum(case["status"] == status for case in cases)
                                        for status in ("passed", "failed", "skipped")}}


def run(ctx):
    source = references(ctx.root, ctx.options.fetch == "never")["product"]
    build = ctx.root / "build/native-upstream"
    ctx.run(["cmake", "-S", source, "-B", build, "-DLIBUV_BUILD_TESTS=ON",
             "-DLIBUV_BUILD_BENCH=OFF", "-DLIBUV_BUILD_SHARED=ON",
             "-DCMAKE_BUILD_TYPE=Release"], "native-upstream-configure")
    ctx.run(["cmake", "--build", build, "--config", "Release", "--target", "uv_run_tests_a",
             "-j", str(ctx.options.jobs)], "native-upstream-build", timeout=900)
    suffix = ".exe" if os.name == "nt" else ""
    candidates = [build / ("uv_run_tests_a" + suffix), build / "Release" / ("uv_run_tests_a" + suffix)]
    binary = next(path for path in candidates if path.is_file())
    # Tests mutate fixtures/working-directory files. Keep acquired inputs intact.
    fixtures = ctx.work / "native-upstream-inputs"
    shutil.copytree(source, fixtures)
    listing = ctx.run([binary, "--list"], "native-upstream-list", cwd=fixtures)
    output = ctx.run([binary], "native-upstream-run", cwd=fixtures, timeout=900, check=False)
    command = next(row for row in reversed(ctx.receipt["commands"]) if row["label"] == "native-upstream-run")
    report = parse_results(listing, output, command["exit_code"])
    target = ctx.artifacts / "native-upstream-results.json"
    target.write_text(json.dumps(report, indent=2) + "\n")
    ctx.receipt["native_upstream"] = {"report": str(target), **report["counts"]}
    ctx.save()
    if (report["exit_code"] != 0 or not report["listed"] or not report["sequence_complete"]
            or not report["names_complete"] or report["counts"]["failed"]):
        raise RuntimeError(f"Native upstream oracle incomplete/failing: {report['counts']}; exit {report['exit_code']}; {len(report['cases'])}/{report['listed']} cases. See {target}")
