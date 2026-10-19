"""Exact-pin native reduction of an observed required ID, retaining decorators.

Run after the actual RAW suite journals a genuine failure. This is a separate
oracle control, never a translated pass or a full native-profile claim.
"""
import json
from pathlib import Path
import sys
import unittest

if len(sys.argv) != 5:
    raise SystemExit("Expected frozen inventory, discovery seed, actual failed case ID, report.")
inventory_path, seed, case, report_path = sys.argv[1:]
inventory = json.loads(Path(inventory_path).read_text())
assert sys.version_info[:3] == tuple(map(int, inventory["python_version"].split(".")))
assert case in inventory["required_cases"] and seed in inventory["discovery_modules"]
sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "Upstream"))
import run_inventory

live = run_inventory.DiscoveredCases()
discovery = run_inventory.discover([seed], live, inventory.get("platform_only_module_skips"))
report = {"scope": "native oracle reduction of actual failed required case; no qualification",
          "case": case, "seed": seed, "python": sys.version, "discovery": discovery,
          "inventory": str(Path(inventory_path).resolve()), "status": "blocked"}
exit_code = 1
if discovery[0]["status"] == "discovered" and case in live:
    suite = unittest.TestSuite(live[case])
    result = unittest.TextTestRunner(verbosity=2, resultclass=run_inventory.CaseResult).run(suite)
    report.update(cases=result.cases, executed=result.testsRun,
                  expected_occurrences=len(live[case]),
                  skipped=len(result.skipped), expected_failures=len(result.expectedFailures))
    passed = (result.wasSuccessful() and result.testsRun == len(live[case])
              and bool(result.cases) and all(row["status"] == "passed" for row in result.cases))
    report["status"] = "passed" if passed else "blocked-or-failed"
    exit_code = 0 if passed else 1
Path(report_path).write_text(json.dumps(report, indent=2) + "\n")
raise SystemExit(exit_code)
