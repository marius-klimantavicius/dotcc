"""Compatibility projection for the TLS peer harnesses."""
import json
from pathlib import Path
from campaigns.compat import provenance


def picotls_provenance(root):
    """Read canonical evidence through the field names used by peer harnesses.

    This projection never claims historical behavioral success. Empty maps mean
    unavailable evidence, which the caller handles using the shared policy.
    """
    root = Path(root)
    available = tuple(form for form, suffix in (("raw", ".Raw"), ("processed", ""))
                      if (root / "generated" / ("TranslatedPicotls" + suffix)).exists())
    current = provenance(root, "TranslatedPicotls", "default", forms=available)
    return {"format": "campaign-provenance-projection-v1",
            "raw": current.get("outputs", {}).get("raw", {}).get("hashes", {}),
            "optimized": current.get("outputs", {}).get("processed", {}).get("hashes", {}),
            "inputs": json.loads((root / "config/inputs.json").read_text()),
            "core_source_sha256": {}, "upstream_header_sha256": {}, "host_sources": [], "tool_sha256": {}}


