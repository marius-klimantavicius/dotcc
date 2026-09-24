"""Preserve pinned dependency licenses and per-source notice comments."""
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

# Consume C literals too, so comment-looking text inside strings is not parsed
# as a comment. Consecutive // lines remain a single complete notice block.
TOKENS = re.compile(r'"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|/\*[\s\S]*?\*/|//[^\n]*(?:\n[ \t]*//[^\n]*)*')
NOTICE = re.compile(r'copyright|SPDX|licen[cs]e|redistribution|permission is hereby', re.I)


def write_notices(reference, configuration, project):
    reference, configuration, project = map(Path, (reference, configuration, project))
    manifest = json.loads((configuration / "sources.json").read_text())
    licenses = json.loads((configuration / "licenses.json").read_text())
    if licenses.get("commit") != manifest.get("commit"):
        raise RuntimeError("License and source inventories have different pins")
    records = {}
    for record in manifest["sources"] + manifest.get("headers", []) + licenses["files"]:
        name = record["path"]
        if Path(name).is_absolute() or ".." in Path(name).parts:
            raise RuntimeError("Unsafe notice source path: " + name)
        path = reference / name
        # Generated command definitions contain no original notices.
        if name in manifest.get("generated", []):
            continue
        contents = path.read_bytes()
        digest = hashlib.sha256(contents).hexdigest()
        if record.get("sha256") and record["sha256"] != digest:
            raise RuntimeError("Notice source hash mismatch: " + name)
        records[name] = (contents.decode("utf-8"), digest)
    standalone = {r["path"] for r in licenses["files"] if Path(r["path"]).suffix not in (".c", ".h")}
    sections = ["Upstream notices for translated Valkey\nPinned commit: " + str(manifest.get("commit"))]
    included = {}
    for name, (contents, digest) in sorted(records.items()):
        blocks = [contents] if name in standalone else [m.group() for m in TOKENS.finditer(contents)
                    if m.group().startswith(("/*", "//")) and NOTICE.search(m.group())]
        if not blocks:
            continue
        sections.append("Source: " + name + "\nSHA-256: " + digest + "\n\n" + "\n\n".join(blocks))
        included[name] = digest
    output = project.parent / "UPSTREAM-NOTICES.txt"
    output.write_text(("\n\n" + "=" * 78 + "\n\n").join(sections) + "\n", encoding="utf-8")
    tree = ET.parse(project)
    for group in list(tree.getroot()):
        if group.get("Label") == "UpstreamNotices":
            tree.getroot().remove(group)
    group = ET.SubElement(tree.getroot(), "ItemGroup", Label="UpstreamNotices")
    ET.SubElement(group, "None", Update=output.name, CopyToOutputDirectory="PreserveNewest", CopyToPublishDirectory="PreserveNewest")
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    return {"files": included, "sha256": hashlib.sha256(output.read_bytes()).hexdigest()}
