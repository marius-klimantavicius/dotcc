"""Guarded embedded-profile staging; acquired references are never modified."""
import json
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[1]


def stage(reference, destination, *, native_dispatch=False):
    reference, destination = Path(reference), Path(destination)
    pin = json.loads((ROOT / "config/source.json").read_text())
    actual = (reference / "VERSION").read_text().strip()
    if actual != pin["version"]:
        raise RuntimeError(f"CONFIG_VERSION metadata {pin['version']!r} differs from VERSION {actual!r}")
    shutil.copytree(reference, destination)
    records = []
    for adaptation in json.loads((ROOT / "config/adaptations.json").read_text())["adaptations"]:
        # TESTS ONLY: native-switch-dispatch is selected by NativeOracle.
        # Actual DotCC translation keeps the source definition and overrides it
        # through config/dotcc-overrides.json during preprocessing.
        if adaptation.get("native_only") and not native_dispatch:
            continue
        path = destination / adaptation["path"]
        text = path.read_text()
        if text.count(adaptation["original"]) != 1:
            raise RuntimeError(f"Adaptation {adaptation['id']} needs exactly one local anchor in {path}")
        path.write_text(text.replace(adaptation["original"], adaptation["replacement"], 1))
        records.append({"id": adaptation["id"], "path": adaptation["path"], "reason": adaptation["reason"]})
    return records
