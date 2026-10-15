#!/usr/bin/env python3
"""Validate selected failure APIs and the captured SysV variadic callback pack."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA256 = "f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185"
APIS = ["sqlite3_memory_used", "sqlite3_hard_heap_limit64", "sqlite3_get_autocommit", "sqlite3_test_control",
        "sqlite3_errcode", "sqlite3_extended_errcode", "sqlite3_extended_result_codes"]


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--generated-project", type=Path, required=True)
    parser.add_argument("--candidate-receipt", type=Path, required=True, help="Actual production completion.json.")
    parser.add_argument("--base-report", type=Path, help="Existing unchanged production23 API receipt.")
    parser.add_argument("--aliases", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    if args.verify_only:
        old = json.loads(args.report.read_text())
        base_path = Path(old["baseApiReceipt"]["path"])
        if args.base_report is not None and args.base_report.resolve() != base_path.resolve():
            raise RuntimeError("Build base receipt differs from its recorded identity.")
    else:
        if args.base_report is None:
            parser.error("Initial validation requires --base-report.")
        base_path = args.base_report.resolve()
        if args.aliases.exists() or args.report.exists():
            raise RuntimeError("Choose fresh failure aliases/receipt paths.")
    base = json.loads(base_path.read_text())
    if (base["status"] != "validated-cli-published-api-surface" or base["sourceMode"] != "production"
            or base["completionValidated"] is not True or base["productionPublished"] is not True
            or args.candidate_receipt.resolve() != Path(base["productionEvidence"]["completion"]["path"]).resolve()):
        raise RuntimeError("Use the actual production CLI completion and existing validated23 surface.")
    validator = Path(base["validator"]["path"])
    base_aliases = Path(base["aliases"]["path"])
    inventory_path = Path(base["acceptedInventory"]["path"])
    if (digest(validator) != base["validator"]["sha256"] or digest(base_aliases) != base["aliases"]["sha256"]
            or digest(inventory_path) != INVENTORY_SHA256):
        raise RuntimeError("The validated production validator/aliases/public inventory changed.")
    # Replay the existing production source/provider/completion checks without
    # changing that validator, report or aliases. No reader/native SQL load.
    subprocess.run([sys.executable, str(validator), "--generated-project", str(args.generated_project.resolve()),
                    "--candidate-receipt", str(args.candidate_receipt.resolve()), "--inventory", str(inventory_path),
                    "--aliases", str(base_aliases), "--report", str(base_path), "--verify-only"], check=True)
    inventory = json.loads(inventory_path.read_text())
    types = {item["locator"]: item for item in inventory["types"]}
    database = next(item for item in base["apis"] if item["name"] == "sqlite3_open_v2")["generatedParameters"][1][:-2]
    visited = set()

    def view(locator: str, active: frozenset[str] = frozenset()) -> str:
        if locator in active:
            raise RuntimeError("Selected API type-reference cycle.")
        descriptor = types[locator]
        visited.add(locator)
        active = active | {locator}
        kind, description = descriptor["kind"], descriptor["description"]
        if kind == "tk_typeref":
            match = re.match(r"underlying=(unit/\d+/type/\d+);", description)
            if not match: raise RuntimeError("Missing recorded underlying type.")
            return view(match[1], active)
        if kind == "tk_pointer":
            match = re.fullmatch(r"pointee=(unit/\d+/type/\d+);reference=False;rvalue-reference=False", description)
            if not match or descriptor["size"] != "8" or descriptor["alignment"] != "8":
                raise RuntimeError("Selected API pointer is not ordinary captured8/8.")
            pointee = types[match[1]]
            if pointee["kind"] == "tk_routine":
                visited.add(pointee["locator"])
                routine = re.fullmatch(r"(unit/\d+/type/\d+)\((unit/\d+/type/\d+) ordinary_parameter\) cc=cc_default linkage=nlk_external result-parameter=False", pointee["description"])
                if not routine or view(routine[1], active) != "int" or view(routine[2], active) != "int":
                    raise RuntimeError("Fault callback is not the captured int(int) Cdecl-compatible routine.")
                return "nint"
            return view(match[1], active) + "*"
        if kind == "tk_struct" and descriptor["incomplete"] and descriptor["name"] == "sqlite3_stmt":
            return "void"
        if kind == "tk_struct" and descriptor["name"] == "sqlite3" and not descriptor["incomplete"]:
            if descriptor["size"] != "864" or descriptor["alignment"] != "8":
                raise RuntimeError("The accepted complete database layout changed.")
            return database
        if kind == "tk_void": return "void"
        if kind == "tk_integer":
            bits = re.search(r";bits=(\d+);signed=(True|False);", description)
            if bits and ((bits[2] == "True" and int(bits[1]) in (8, 32, 64)) or (bits[2] == "False" and int(bits[1]) == 32)):
                size = int(bits[1]) // 8
                if descriptor["size"] != str(size) or descriptor["alignment"] != str(size):
                    raise RuntimeError("Selected scalar size/alignment changed.")
                return "uint" if bits[2] == "False" else {8: "sbyte", 32: "int", 64: "long"}[int(bits[1])]
        raise RuntimeError("Unaccepted failure scalar/pointer type: " + locator)

    directory = args.generated_project.resolve().parent
    source = (directory / "Generated.cs").read_text()
    methods = {}
    for match in re.finditer(r"^    public static ([\w@:*\.]+) (@?[A-Za-z_]\w*)\(([^\n]*)\)\s*$", source, re.M):
        result, name, parameters = match.groups()
        methods.setdefault(name.lstrip("@"), []).append((result, [part.rsplit(" ", 1)[0] for part in parameters.split(", ")] if parameters else []))
    expected = {
        "sqlite3_memory_used": ("long", []), "sqlite3_hard_heap_limit64": ("long", ["long"]),
        "sqlite3_get_autocommit": ("int", [database + "*"]), "sqlite3_test_control": ("int", ["int"]),
        "sqlite3_errcode": ("int", [database + "*"]), "sqlite3_extended_errcode": ("int", [database + "*"]),
        "sqlite3_extended_result_codes": ("int", [database + "*", "int"])
    }
    validated = []
    for name in APIS:
        selected = [item for item in inventory["functions"] if item["currentName"] == name and item["requiredBody"]]
        if len(selected) != 1 or selected[0]["bodyState"] != "definition" or selected[0]["linkageName"] != name:
            raise RuntimeError("Failure API lacks one exact retained definition: " + name)
        function, signature = selected[0], selected[0]["signature"]
        if (not signature["hasPrototype"] or signature["variadic"] != (name == "sqlite3_test_control") or signature["returnsValueAsParameter"]
                or signature["returnsValueViaCopyConstructor"] or signature["callingConvention"] not in ("cc_default", "cc_cdecl")
                or any(item["role"] != "ordinary_parameter" for item in signature["parameters"])):
            raise RuntimeError("Unsupported selected role/convention: " + name)
        retained = (view(signature["returnType"]), [view(item["type"]) for item in signature["parameters"]])
        projected = (retained[0], retained[1] + ["global::RoslynObj.CSharp.Runtime.SysVVaList*"]) if name == "sqlite3_test_control" else retained
        if retained != expected[name] or methods.get(name) != [projected]:
            raise RuntimeError("Actual generated API differs from selected bounded representation: " + name)
        validated.append({"name": name, "selected": function, "generatedReturn": projected[0], "generatedParameters": projected[1]})
    tags = [item for item in inventory["types"] if item["name"] == "__va_list_tag" and item["kind"] == "tk_struct"]
    if len(tags) != 1 or tags[0]["size"] != "24" or tags[0]["alignment"] != "8":
        raise RuntimeError("Captured builtin variadic tag is not24/8.")
    tag = tags[0]
    coordinates = [(item["name"], item["offset"], view(item["type"])) for item in tag["members"]]
    if coordinates != [("gp_offset", "0", "uint"), ("fp_offset", "4", "uint"),
                       ("overflow_arg_area", "8", "void*"), ("reg_save_area", "16", "void*")]:
        raise RuntimeError("Captured variadic tag field types/coordinates differ.")
    visited.add(tag["locator"])
    builtins = [item for item in inventory["types"] if item["name"] == "__builtin_va_list" and item["kind"] == "tk_typeref"]
    if len(builtins) != 1 or builtins[0]["size"] != "24" or builtins[0]["alignment"] != "8":
        raise RuntimeError("Missing exact builtin va_list wrapper.")
    underlying = re.fullmatch(r"underlying=(unit/\d+/type/\d+);const=False;volatile=False", builtins[0]["description"])
    if not underlying: raise RuntimeError("Unexpected builtin va_list qualification.")
    array = types[underlying[1]]
    if (array["kind"] != "tk_array" or array["size"] != "24" or array["alignment"] != "8"
        or array["description"] != "element=" + tag["locator"] + ";bound-kind=constant;bound=1;vla=False"):
        raise RuntimeError("Builtin va_list is not the recorded one-element tag array.")
    visited.update((builtins[0]["locator"], array["locator"]))
    callback_types = [item for item in inventory["types"] if item["name"] == "sqlite3FaultFuncType"]
    if len(callback_types) != 1 or callback_types[0]["size"] != "8" or callback_types[0]["alignment"] != "8" or view(callback_types[0]["locator"]) != "nint":
        raise RuntimeError("The retained fault callback is not an8/8 int(int) function pointer.")
    runtime = Path(base["productionEvidence"]["runtimeProjectPath"]).parent / "SysVVariadic.cs"
    runtime_text = runtime.read_text()
    if not re.search(r"StructLayout\(LayoutKind.Explicit, Size = 24, Pack = 8\)", runtime_text):
        raise RuntimeError("Runtime variadic storage dimensions changed.")
    for offset, field_type, field in ((0, "uint", "GpOffset"), (4, "uint", "FpOffset"),
                                     (8, "byte*", "OverflowArgumentArea"), (16, "byte*", "RegisterSaveArea")):
        if f"[FieldOffset({offset})] public {field_type} {field};" not in runtime_text:
            raise RuntimeError("Runtime variadic field contract changed.")
    if "public const int GpRegisterBytes = 48;" not in runtime_text or "public const int RegisterSaveBytes = 176;" not in runtime_text:
        raise RuntimeError("Runtime variadic register constants changed.")
    aliases = base_aliases.read_text()
    checks = {"status": "validated-cli-published-failure-api-surface", "sourceMode": "production",
              "completionValidated": True, "sourcePublished": True, "generatedExecution": False,
              "baseApiReceipt": receipt(base_path), "validator": receipt(Path(__file__)),
              "completion": receipt(args.candidate_receipt), "acceptedInventory": receipt(inventory_path),
              "apis": validated, "runtimeVariadicAdapter": receipt(runtime),
              "packContract": {"size": 24, "alignment": 8, "fixedGpBytes": 8, "fixedFpOffset": 48, "callbackPointerSlot": 8, "registerSaveBytes": 176},
              "publicTypeDescriptions": [types[key] for key in sorted(visited)],
              "scope": "Seven selected failure APIs plus production23 validation; explicit one-pointer variadic pack, builtin tag and int(int) callback; no arbitrary variadic/OS-failure claim."}
    if args.verify_only:
        if args.aliases.read_text() != aliases or any(old[key] != value for key, value in checks.items()):
            raise RuntimeError("Failure build aliases/receipt/source changed.")
        print("production-failure-api-surface=verified")
    else:
        args.aliases.parent.mkdir(parents=True, exist_ok=True)
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.aliases.write_text(aliases)
        args.report.write_text(json.dumps(checks | {"aliases": receipt(args.aliases)}, indent=2) + "\n")
        print(json.dumps({"status": checks["status"], "newApis": len(validated), "baseApis": len(base["apis"])}))
    return 0


if __name__ == "__main__":
    sys.exit(main())
