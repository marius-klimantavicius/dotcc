"""Replay original required importlib cases without changing upstream callables.

This is a focused actual-product diagnostic, not subset qualification. The full
required inventory still runs separately through unchanged run_embedded.
"""
from collections import Counter
import json
import os
from pathlib import Path
import sys
import unittest

import run_inventory

selection = json.loads(Path(os.environ["DOTCC_B32_SELECTION"]).read_text())
inventory = json.loads(Path(selection["inventory"]).read_text())
assert sys.version_info[:3] == tuple(map(int, inventory["python_version"].split(".")))
selected = selection[os.environ["DOTCC_B32_FOCUS"]]
assert selected and set(selected) <= set(inventory["required_cases"])
live = run_inventory.DiscoveredCases()
discovery = run_inventory.discover(["test.test_importlib"], live,
    inventory.get("platform_only_module_skips"))
report_path = Path(os.environ["DOTCC_B32_REPORT"])
report = {"scope": "focused actual-product diagnostic; full qualification remains open",
          "python": sys.version, "selected": selected, "status": "blocked",
          "discovery_status": discovery[0]["status"], "cases": []}
if discovery[0]["status"] != "discovered":
    report["discovery_failure"] = discovery[0]
else:
    assert set(selected) <= live.keys(), sorted(set(selected) - live.keys())
    suite = unittest.TestSuite(test for test in live.ordered if test.id() in set(selected))
    expected = Counter(test.id() for test in run_inventory.flatten(suite))
    assert expected == Counter({case: len(live[case]) for case in selected})
    with report_path.with_suffix(".journal.jsonl").open("w") as journal:
        def result_factory(*args, **kwargs):
            result = run_inventory.CaseResult(*args, **kwargs)
            result.journal = journal
            return result
        result = unittest.TextTestRunner(verbosity=2, resultclass=result_factory).run(suite)
    passed = (result.wasSuccessful() and result.testsRun == sum(expected.values())
              and Counter(row["case"] for row in result.cases) == expected
              and all(row["status"] == "passed" for row in result.cases))
    report.update(status="passed" if passed else "blocked-or-failed", cases=result.cases,
                  testsRun=result.testsRun, expected_occurrences=sum(expected.values()),
                  unique_selected=len(selected),
                  event_counts=dict(Counter(row["status"] for row in result.cases)))
report_path.write_text(json.dumps(report, indent=2) + "\n")
print(json.dumps({key: value for key, value in report.items() if key != "cases"}), flush=True)
assert report["status"] == "passed", report
