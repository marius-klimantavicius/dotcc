"""Package upstream notices from acquired, pinned reference inputs."""
import json
from pathlib import Path
import shutil


def stage(root, destination):
    pins = json.loads((root / "config/inputs.json").read_text())
    notices = json.loads((root / "config/notices.json").read_text())
    destination.mkdir(parents=True, exist_ok=True)
    records = []
    lines = ["Third-party sources and declarations", "", "The product translates s2n-tls C source and uses a managed cryptographic provider.",
             "OpenSSL supplies declaration headers; no native libcrypto backend is delivered.", ""]
    for key, paths in notices["sources"].items():
        pin = pins[key]
        for name in paths:
            source = root / "ref" / pin["directory"] / name
            if not source.is_file():
                raise RuntimeError("Missing acquired license/notice: " + str(source))
            target = destination / "notices" / key / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
            records.append({"source": key, "revision": pin["revision"], "file": target.relative_to(destination).as_posix()})
        lines += [key + " (" + pin["revision"] + ")", pin["url"], "License: " + pin["license"], ""]
    (destination / "THIRD-PARTY-NOTICES.txt").write_text("\n".join(lines))
    (destination / "notices/manifest.json").write_text(json.dumps(records, indent=2) + "\n")
