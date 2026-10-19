"""Bounded exact-pin marker controls; no native build or full-seed pass claim.

This runs only after the actual full run, with the already-pinned native oracle.
Missing import closures remain explicit per-case blockers. Decorators and all
test/class fixtures remain unchanged.
"""
import argparse
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import traceback
import unittest


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def run(args):
    manifest = json.loads(args.markers.read_text())
    assert sys.version_info[:3] == (3, 13, 15) and __debug__
    spec = importlib.util.spec_from_file_location("r36_unchanged_inventory", args.runner)
    runner = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(runner)
    results = []
    report = {"scope": "native oracle exact required marker controls; no translated/full-native qualification",
              "python": sys.version, "executable": {"path": sys.executable, "sha256": digest(sys.executable)},
              "flags": {"utf8_mode": sys.flags.utf8_mode, "dont_write_bytecode": sys.dont_write_bytecode,
                        "isolated": sys.flags.isolated, "optimize": sys.flags.optimize},
              "markerManifest": str(args.markers), "cases": results, "status": "running"}

    def checkpoint():
        report["countsByAvailability"] = dict(Counter(r["availability"] for r in results))
        args.report.write_text(json.dumps(report, indent=2) + "\n")

    checkpoint()
    for marker in manifest["cases"]:
        case = marker["case"]
        row = {"case": case, "availability": "blocked", "status": "blocked",
               "requiredMarkerRemainsNonpass": True}
        try:
            loader = unittest.TestLoader()
            suite = loader.loadTestsFromName(case)
            if loader.errors:
                raise RuntimeError("Exact native case loader failed:\n" + "\n".join(loader.errors))
            loaded = list(runner.flatten(suite))
            assert len(loaded) == 1 and loaded[0].id() == case, [test.id() for test in loaded]
            detail = runner.case_detail(loaded[0])
            original = marker["source"]
            method = getattr(type(loaded[0]), loaded[0]._testMethodName)
            source_path = Path(runner.inspect.getsourcefile(runner.source_callable(method))).resolve()
            assert digest(source_path) == original["file"]["sha256"], "Native original test source differs"
            assert detail["source"] == original["originalMethodSource"], "Native original method differs"
            assert detail["expected_failure"] == original["expectedFailure"], "Native XFAIL marker differs"
            row["nativeSource"] = {"path": str(source_path), "sha256": digest(source_path)}
            with args.report.with_suffix(".unittest.log").open("a") as stream:
                result = unittest.TextTestRunner(verbosity=2, resultclass=runner.CaseResult, stream=stream).run(suite)
            row.update(availability="executed exact unchanged case", outcomes=result.cases, executed=result.testsRun,
                       status="passed" if result.testsRun == 1 and len(result.cases) == 1
                       and result.cases[0]["status"] == "passed" else "required-nonpass")
        except BaseException:
            row["closureOrControlFailure"] = traceback.format_exc()
        results.append(row)
        checkpoint()
    assert len(results) == len(manifest["cases"])
    report["status"] = "controls recorded; required native/translated marker adjudication OPEN"
    checkpoint()
    print(json.dumps({"scope": report["scope"], "cases": len(results), "countsByAvailability": report["countsByAvailability"]}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--markers", required=True, type=Path)
    parser.add_argument("--runner", required=True, type=Path)
    parser.add_argument("--report", required=True, type=Path)
    run(parser.parse_args())
