"""Audit delivered scalar artifacts and their declared source/host boundaries."""
import json
import re
import xml.etree.ElementTree as ET

from campaigns.testing import forms
from campaigns.translation import check_product


def run(ctx, suite):
    inventory = json.loads((ctx.root / "config/api-inventory.json").read_text(encoding="utf-8"))
    required = set(inventory["required_exports"])
    results = []
    for form in forms(ctx):
        project = check_product(ctx, form)
        manifest = project.parent / ".campaign-files.json"
        if not manifest.is_file():
            raise RuntimeError("Missing generated-file ownership manifest: " + str(manifest))
        files = json.loads(manifest.read_text(encoding="utf-8"))
        source = "\n".join((project.parent / name).read_text(encoding="utf-8")
                           for name in files if name.endswith(".cs"))
        if any(name in source for name in ("dotcc_oracle_main", "dotcc_boundary_main", "dotcc_upstream_run")):
            raise RuntimeError("Test entrypoints escaped into the delivered scalar library")
        methods = set(re.findall(r"\bpublic\s+(?:static\s+|unsafe\s+)*[^;{}\n]*?\b(ghostty_[a-z_]+)\s*\(", source))
        missing = required - methods
        if missing:
            raise RuntimeError("Delivered scalar library lost required upstream exports: " + ", ".join(sorted(missing)))
        if re.search(r"\b(?:DllImport|LibraryImport)\s*\(\s*\"[^\"]*ghostty", source, re.I):
            raise RuntimeError("A native terminal library import is present")
        if re.search(r"NativeLibrary\.(?:Load|TryLoad)\s*\(\s*\"[^\"]*ghostty", source, re.I):
            raise RuntimeError("A dynamic native terminal backend is present")
        tree = ET.parse(project)
        packages = [node.get("Include", "") for node in tree.iter("PackageReference")]
        if packages:
            raise RuntimeError("Unexpected delivered package dependency: " + ", ".join(packages))
        notices = project.parent / "THIRD-PARTY-NOTICES"
        if not notices.is_file():
            raise RuntimeError("Missing delivered upstream notices")
        notice_text = notices.read_text(encoding="utf-8")
        pins = json.loads((ctx.root / "config/source.json").read_text(encoding="utf-8"))
        for item in pins["license_inputs"]:
            if item.get("distribute") and item["name"] not in notice_text:
                raise RuntimeError("Missing license attribution: " + item["name"])
        binaries = [name for name in files if name.endswith((".so", ".a", ".dylib", ".lib", ".exe", ".dll"))]
        if binaries:
            raise RuntimeError("Native/binary artifacts in generated-source delivery: " + ", ".join(binaries))
        results.append(dict(form=form, project=str(project), required_exports=len(required),
                            actual_upstream_exports=len(methods), native_terminal_imports=0,
                            test_entrypoints=0, package_dependencies=packages,
                            notices=str(notices)))
    path = ctx.artifacts / "delivery-audit.json"
    path.write_text(json.dumps(dict(results=results), indent=2) + "\n", encoding="utf-8")
    ctx.receipt["delivery_audit"] = str(path)
