#!/usr/bin/env python3
"""Validate six retained provider-contract APIs on an actual CLI publication."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA256 = "f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185"
EXPECTED = {
    "sqlite3_initialize": ("int", []),
    "sqlite3_randomness": ("void", ["int", "void*"]),
    "sqlite3_malloc64": ("void*", ["ulong"]),
    "sqlite3_realloc64": ("void*", ["void*", "ulong"]),
    "sqlite3_msize": ("ulong", ["void*"]),
    "sqlite3_free": ("void", ["void*"])
}


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path):
    path = Path(path)
    return {"path": str(path.resolve()), "bytes": path.stat().st_size, "sha256": digest(path)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--generated-project", type=Path, required=True)
    parser.add_argument("--candidate-receipt", type=Path, required=True, help="Actual production completion.json.")
    parser.add_argument("--base-report", type=Path, help="Unchanged production23 API receipt for this publication.")
    parser.add_argument("--aliases", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    if args.verify_only:
        old = json.loads(args.report.read_text()); base_path = Path(old["baseApiReceipt"]["path"])
        if args.base_report is not None and args.base_report.resolve() != base_path.resolve():
            raise RuntimeError("Build base receipt differs from its recorded identity.")
    else:
        if args.base_report is None: parser.error("Initial validation requires --base-report.")
        base_path = args.base_report.resolve()
        if args.aliases.exists() or args.report.exists():
            raise RuntimeError("Choose fresh provider-contract aliases/report paths.")
    base = json.loads(base_path.read_text())
    if (base["status"] != "validated-cli-published-api-surface" or base["sourceMode"] != "production"
            or base["completionValidated"] is not True or base["productionPublished"] is not True
            or args.candidate_receipt.resolve() != Path(base["productionEvidence"]["completion"]["path"]).resolve()):
        raise RuntimeError("Use this actual CLI completion and its production23 API receipt.")
    validator, base_aliases = Path(base["validator"]["path"]), Path(base["aliases"]["path"])
    inventory_path = Path(base["acceptedInventory"]["path"])
    if (digest(validator) != base["validator"]["sha256"] or digest(base_aliases) != base["aliases"]["sha256"]
            or digest(inventory_path) != INVENTORY_SHA256):
        raise RuntimeError("The existing validator/aliases/public inventory changed.")
    subprocess.run([sys.executable, str(validator), "--generated-project", str(args.generated_project.resolve()),
                    "--candidate-receipt", str(args.candidate_receipt.resolve()), "--inventory", str(inventory_path),
                    "--aliases", str(base_aliases), "--report", str(base_path), "--verify-only"], check=True)
    inventory = json.loads(inventory_path.read_text())
    types = {item["locator"]: item for item in inventory["types"]}
    visited = set()

    def view(locator, active=frozenset()):
        if locator in active: raise RuntimeError("Selected API type-reference cycle.")
        descriptor = types[locator]; visited.add(locator); active = active | {locator}
        kind, description = descriptor["kind"], descriptor["description"]
        if kind == "tk_typeref":
            match = re.fullmatch(r"underlying=(unit/\d+/type/\d+);const=False;volatile=False", description)
            if not match: raise RuntimeError("Unexpected selected typedef qualification.")
            result = view(match[1], active)
            if result != "ulong" or descriptor["size"] != "8" or descriptor["alignment"] != "8":
                raise RuntimeError("Selected typedef is not the captured natural uint64.")
            return result
        if kind == "tk_pointer":
            match = re.fullmatch(r"pointee=(unit/\d+/type/\d+);reference=False;rvalue-reference=False", description)
            if not match or descriptor["size"] != "8" or descriptor["alignment"] != "8" or view(match[1], active) != "void":
                raise RuntimeError("Selected pointer is not ordinary captured void*8/8.")
            return "void*"
        if kind == "tk_void": return "void"
        if kind == "tk_integer":
            match = re.search(r";bits=(\d+);signed=(True|False);", description)
            if match and (match[1], match[2]) in (("32", "True"), ("64", "False")):
                size = str(int(match[1]) // 8)
                if descriptor["size"] != size or descriptor["alignment"] != size:
                    raise RuntimeError("Selected integer dimensions changed.")
                return "int" if match[2] == "True" else "ulong"
        raise RuntimeError("Unsupported selected provider-contract type: " + locator)

    source = (args.generated_project.resolve().parent / "Generated.cs").read_text()
    methods = {}
    for match in re.finditer(r"^    public static ([\w@:*\.]+) (@?[A-Za-z_]\w*)\(([^\n]*)\)\s*$", source, re.M):
        result, name, parameters = match.groups()
        methods.setdefault(name.lstrip("@"), []).append((result, [part.rsplit(" ", 1)[0] for part in parameters.split(", ")] if parameters else []))
    validated = []
    for name, expected in EXPECTED.items():
        selected = [item for item in inventory["functions"] if item["currentName"] == name and item["requiredBody"]]
        if len(selected) != 1 or selected[0]["bodyState"] != "definition" or selected[0]["linkageName"] != name:
            raise RuntimeError("Expected one exact required retained API definition: " + name)
        function = selected[0]; signature = function["signature"]
        if (not signature["hasPrototype"] or signature["variadic"] or signature["returnsValueAsParameter"]
                or signature["returnsValueViaCopyConstructor"] or signature["callingConvention"] not in ("cc_default", "cc_cdecl")
                or signature["languageLinkage"] != "nlk_external" or any(item["role"] != "ordinary_parameter" for item in signature["parameters"])):
            raise RuntimeError("Unaccepted selected routine roles/convention: " + name)
        actual = (view(signature["returnType"]), [view(item["type"]) for item in signature["parameters"]])
        if actual != expected or methods.get(name) != [expected]:
            raise RuntimeError("Selected API/actual generated projection differs: " + name)
        validated.append({"name": name, "selected": function, "generatedReturn": actual[0], "generatedParameters": actual[1]})
    aliases = base_aliases.read_text()
    checks = {"status": "validated-cli-published-provider-contract-api-surface", "sourceMode": "production",
              "completionValidated": True, "sourcePublished": True, "generatedExecution": False,
              "baseApiReceipt": receipt(base_path), "validator": receipt(Path(__file__)),
              "completion": receipt(args.candidate_receipt), "acceptedInventory": receipt(inventory_path),
              "apis": validated, "publicTypeDescriptions": [types[key] for key in sorted(visited)],
              "scope": "Six selected API definitions plus production23 validation; selection alone is not API or host-export invocation evidence."}
    if args.verify_only:
        if args.aliases.read_text() != aliases or any(old[key] != value for key, value in checks.items()):
            raise RuntimeError("Provider-contract aliases/receipt/source changed.")
        print("production-provider-contract-api-surface=verified")
    else:
        args.aliases.parent.mkdir(parents=True, exist_ok=True); args.report.parent.mkdir(parents=True, exist_ok=True)
        args.aliases.write_text(aliases)
        args.report.write_text(json.dumps(checks | {"aliases": receipt(args.aliases)}, indent=2) + "\n")
        print(json.dumps({"status": checks["status"], "newApis": len(validated), "baseApis": len(base["apis"])}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
