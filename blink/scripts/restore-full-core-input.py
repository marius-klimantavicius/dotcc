#!/usr/bin/env python3
"""Install the separately retained, exact FullCore producer input. No translation."""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import tempfile


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path, help="retained native14-processed.zip")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1] / "src/Managed.Emulation.FullCore"
    spec = json.loads((project / "Producer/manifest.json").read_text())["archive"]
    target = project / spec["path"]
    source = args.archive.resolve(strict=True)
    if source.stat().st_size != spec["bytes"]:
        parser.error("producer archive length differs from the qualified manifest")
    with source.open("rb") as incoming:
        digest = hashlib.file_digest(incoming, "sha256").hexdigest()
    if digest != spec["sha256"]:
        parser.error("producer archive hash differs from the qualified manifest")
    if source != target.resolve():
        temporary = None
        try:
            with tempfile.NamedTemporaryFile(dir=target.parent, delete=False) as staged:
                temporary = Path(staged.name)
                with source.open("rb") as incoming:
                    shutil.copyfileobj(incoming, staged)
            temporary.replace(target)
        finally:
            if temporary is not None:
                temporary.unlink(missing_ok=True)
    print(f"Verified {target}: SHA256 {spec['sha256']}")


if __name__ == "__main__":
    main()
