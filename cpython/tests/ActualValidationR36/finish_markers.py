"""At the completed full-run boundary, record bounded native availability.

Reuses the existing exact-pin executable. No native build, source/decorator
change, full native qualification or translated marker reclassification.
"""
from collections import Counter
import json
import os
from pathlib import Path
import sys

from marker_inventory import inventory
from reporting import item

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
ART = ROOT / "cpython/artifacts/owner-b-r36-20261018"
completed = json.loads((ART / "actual-run-result.json").read_text())
assert completed["allRequiredIdsCompleted"] is True, "Native controls follow the full actual run"
markers = inventory(ART / "full-required-raw-jit.json")
manifest = ART / "required-marker-source-inventory.json"
manifest.write_text(json.dumps(markers, indent=2) + "\n")
preparation = json.loads((ART / "source-preparation.json").read_text())
native = preparation["existingNativeOracle"]
assert item(native["path"])["sha256"] == native["sha256"]
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.process import execute

commands = json.loads((ART / "commands.json").read_text())
assert commands[-1]["label"] == "full-required-raw-jit"
record = {}
commands.append(record)
report = ART / "native-marker-availability.json"
command = ["flock", "-n", "--close", "/tmp/dotcc-cpython-shared-build.lock",
           native["path"], "-I", "-B", "-X", "utf8=1", HERE / "native_markers.py",
           "--markers", manifest, "--runner", ROOT / "cpython/tests/Upstream/run_inventory.py",
           "--report", report]
try:
    execute(command, label="native-marker-availability", cwd=Path("/tmp"),
            output=ART / "native-marker-availability.log", timeout=90, env=dict(os.environ), record=record, check=False)
finally:
    (ART / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
if report.exists():
    native_report = json.loads(report.read_text())
    by_case = {r["case"]: r for r in native_report["cases"]}
    for marker in markers["cases"]:
        marker["nativeControl"] = by_case.get(marker["case"], {"status": "UNRUN after bounded native control interruption"})
    markers["nativeControlReport"] = item(report)
    markers["nativeAvailabilityCounts"] = dict(Counter(r["nativeControl"].get("availability", "UNRUN") for r in markers["cases"]))
    manifest.write_text(json.dumps(markers, indent=2) + "\n")
print(json.dumps({"markerCount": markers["count"], "nativeCommandStatus": record["status"],
                  "seconds": record.get("seconds"), "nativeAvailabilityCounts": markers.get("nativeAvailabilityCounts"),
                  "scope": "Required markers remain nonpasses; native full-seed qualification OPEN"}))
