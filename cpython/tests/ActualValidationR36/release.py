"""One terminal/free-lock/no-queue check at B36's completed run boundary."""
import datetime
import fcntl
import json
from pathlib import Path
from reporting import item

ROOT = Path(__file__).resolve().parents[3]
ART = ROOT / "cpython/artifacts/owner-b-r36-20261018"
commands = json.loads((ART / "commands.json").read_text())
assert commands and all(row["status"] in ("passed", "failed", "timed-out", "cancelled") for row in commands)
pids = {row["pid"] for row in commands}
assert all(not Path("/proc", str(pid)).exists() for pid in pids), "Owned command PID still exists"
locks = []
for path in (Path("/tmp/dotcc-cpython-shared-build.lock"), Path("/tmp/dotcc-cpython-native-build.lock"),
             ROOT / "cpython/build/campaign/run.lock"):
    # Existing coordination paths only; no lock file is created or removed.
    with path.open("r+") as stream:
        fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
        fcntl.flock(stream, fcntl.LOCK_UN)
    locks.append({"path": str(path), "independentAcquireAndRelease": True, "freeAtTransfer": True})
result = {"status": "DIRECT GENUINE SAFE RELEASE to A30/supervisor; B36 source/reporting-only after full actual run",
          "utc": datetime.datetime.now(datetime.timezone.utc).isoformat(), "terminalPidsAbsent": sorted(pids),
          "activeOwned": [], "queuedOwned": [], "locks": locks, "commandCount": len(commands),
          "ownedCommandSeconds": sum(row.get("seconds", 0) for row in commands),
          "commands": item(ART / "commands.json"), "counts": item(ART / "full-required-journal-counts.json"),
          "inputProof": item(ART / "actual-input-proof.json"),
          "scope": "No extra build/emit/native/AOT/campaign work queued; remaining required markers and final campaign gates OPEN"}
path = ART / "serial-slot-release-to-a30.json"
assert not path.exists(), "Preserve the original release receipt"
path.write_text(json.dumps(result, indent=2) + "\n")
print(json.dumps({"release": item(path), "terminalPids": len(pids), "counts": result["counts"]}))
