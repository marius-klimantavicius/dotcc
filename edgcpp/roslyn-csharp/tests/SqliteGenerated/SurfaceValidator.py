#!/usr/bin/env python3
"""Validate an unpublished genuine candidate and emit direct-call type aliases.

No compilation, reflection, source-language reconstruction or native API fallback.
The actual generated public C# declarations choose the C# spelling; accepted
public report types establish the selected scalar/pointer representation.
"""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re

INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
INVENTORY_SHA256 = "f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185"
APIS = ["sqlite3_libversion", "sqlite3_libversion_number", "sqlite3_open_v2", "sqlite3_close", "sqlite3_prepare_v2",
    "sqlite3_step", "sqlite3_finalize", "sqlite3_errmsg", "sqlite3_column_count", "sqlite3_column_name",
    "sqlite3_column_type", "sqlite3_column_int64", "sqlite3_column_double", "sqlite3_column_text", "sqlite3_column_blob",
    "sqlite3_column_bytes", "sqlite3_bind_int64", "sqlite3_bind_double", "sqlite3_bind_text", "sqlite3_bind_blob",
    "sqlite3_bind_null", "sqlite3_reset", "sqlite3_clear_bindings"]


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--generated-project", type=Path, required=True)
    parser.add_argument("--candidate-receipt", type=Path, required=True)
    parser.add_argument("--inventory", type=Path, default=Path("/mnt/storage/roslynobj-csharp-20261015/t0/sqlite-inventory-address-closure/program-report.json"))
    parser.add_argument("--provider-plan", type=Path, default=Path(__file__).resolve().parents[2] / "acceptance/sqlite/provider-plan.json")
    parser.add_argument("--aliases", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    directory = args.generated_project.resolve().parent
    candidate = json.loads(args.candidate_receipt.read_text())
    if (candidate["input"]["sha256"] != INPUT_SHA256 or candidate["errorCount"] != 0 or candidate["candidateGenerated"] is not True
        or candidate["productionPublished"] is not False or candidate["outputPublished"] is not False
        or (directory / "completion.json").exists()):
        raise RuntimeError("A zero-error, unpublished full-object diagnostic candidate is required.")
    if Path(candidate["diagnosticCandidate"]).resolve() != directory or args.generated_project.resolve() != directory / "Generated.csproj":
        raise RuntimeError("Requested project is not the exact diagnostic candidate output.")
    candidate_files = []
    for item in candidate["candidateFiles"]:
        path = (directory / item["RelativePath"]).resolve()
        if not path.is_relative_to(directory) or path.name == "completion.json":
            raise RuntimeError("Candidate file escaped its output or claimed production completion.")
        actual = receipt(path)
        if actual["sha256"] != item["sha256"] or actual["bytes"] != item["bytes"]:
            raise RuntimeError("Candidate file differs from its exact emission receipt: " + str(path))
        candidate_files.append(actual)
    if not {"Generated.cs", "Generated.csproj", "Generated.NativeImports.json", "Generated.Abi.c"} <= {Path(p["path"]).name for p in candidate_files}:
        raise RuntimeError("Candidate receipt omitted required source/project/provider artifacts.")
    if digest(args.inventory) != INVENTORY_SHA256:
        raise RuntimeError("The accepted public inventory report identity changed.")
    plan = json.loads(args.provider_plan.read_text())
    if (plan["acceptedInventoryReportSha256"] != INVENTORY_SHA256
        or candidate["providerPlan"]["sha256"] != digest(args.provider_plan)):
        raise RuntimeError("The provider plan differs from the accepted public inventory.")
    planned_native = {item["capturedImport"]["linkageName"]: item for item in plan["imports"] if item["candidateProviderGroup"] != "target-intrinsic"}
    native_report = json.loads((directory / "Generated.NativeImports.json").read_text())
    actual_imports = native_report["imports"]
    if (len(planned_native) != 88 or len(actual_imports) != 88
        or {item["linkageName"] for item in actual_imports} != set(planned_native)
        or native_report["nativeErrorProtocol"] != "LinuxX64LogicalErrno"):
        raise RuntimeError("Actual generated imports differ from the explicit 88-candidate system-provider scope/protocol.")
    for item in actual_imports:
        planned = planned_native[item["linkageName"]]
        if (item["library"] != planned["requestedProviderPath"] or item["entryPoint"] != planned["hostEntryPoint"]
            or item["callingConvention"] != "cdecl" or item["isVariadic"] != planned["selectedSignature"]["variadic"]
            or item["aggregateGateway"] or item["logicalErrnoProvider"] != (item["linkageName"] == "__errno_location")
            or item["logicalDlerrorProvider"] != (item["linkageName"] == "dlerror")):
            raise RuntimeError("Generated import mapping or selected provider protocol changed: " + item["linkageName"])
    for provider in plan["providerFiles"]:
        if digest(Path(provider["path"])) != provider["sha256"]:
            raise RuntimeError("A pinned candidate provider changed before building the generated harness.")
    inventory = json.loads(args.inventory.read_text())
    if candidate["artifact"] != inventory["inputs"][0]["artifactIdentity"]:
        raise RuntimeError("Candidate artifact identity differs from the accepted full SQLite object.")
    types = {item["locator"]: item for item in inventory["types"]}
    selected = {name: [function for function in inventory["functions"] if function["currentName"] == name and function["requiredBody"]] for name in APIS}
    if any(len(functions) != 1 for functions in selected.values()):
        raise RuntimeError("A harness API lacks exactly one selected retained definition.")
    source = (directory / "Generated.cs").read_text()
    namespace = re.search(r'^namespace ([A-Za-z_][\w.]*)\s*;', source, re.M)
    owner = re.search(r'^public static unsafe class ([A-Za-z_]\w*)\s*$', source, re.M)
    if namespace is None or owner is None: raise RuntimeError("Expected one concrete generated static application owner.")
    methods = {}
    for match in re.finditer(r'^    public static ([\w@:*\.]+) (@?[A-Za-z_]\w*)\(([^\n]*)\)\s*$', source, re.M):
        result, name, parameters = match.groups()
        methods.setdefault(name.lstrip("@"), []).append((result, [parameter.rsplit(" ", 1)[0] for parameter in parameters.split(", ")] if parameters else []))
    if any(len(methods.get(name, [])) != 1 for name in APIS):
        raise RuntimeError("An actual generated API name is missing/ambiguous; inspect concrete disambiguation before compiling.")
    db_out = methods["sqlite3_open_v2"][0][1][1]
    if not re.fullmatch(r'@?[A-Za-z_]\w*\*\*', db_out):
        raise RuntimeError("Captured complete database requires a concrete typed pointer output.")
    db_name = db_out[:-2]
    if not re.search(r'^public unsafe struct ' + re.escape(db_name) + r'\s*$', source, re.M):
        raise RuntimeError("Actual database type has no concrete emitted public record declaration.")
    visited = set()

    def view(locator: str, active: frozenset[str] = frozenset()) -> str:
        if locator in active: raise RuntimeError("A required API type reference has a cycle.")
        active = active | {locator}
        descriptor = types[locator]
        visited.add(locator)
        kind, description = descriptor["kind"], descriptor["description"]
        if kind == "tk_typeref":
            match = re.match(r'underlying=(unit/\d+/type/\d+);', description)
            if not match: raise RuntimeError("Missing exact retained underlying type.")
            return view(match[1], active)
        if kind == "tk_pointer":
            match = re.match(r'pointee=(unit/\d+/type/\d+);reference=False;rvalue-reference=False', description)
            if not match or descriptor["size"] != "8" or descriptor["alignment"] != "8":
                raise RuntimeError("An API pointer differs from the ordinary captured 8/8 representation.")
            pointee = types[match[1]]
            while pointee["kind"] == "tk_typeref":
                visited.add(pointee["locator"])
                pointee = types[re.match(r'underlying=(unit/\d+/type/\d+);', pointee["description"])[1]]
            if pointee["kind"] == "tk_routine":
                visited.add(pointee["locator"])
                routine = re.fullmatch(r'(unit/\d+/type/\d+)\((unit/\d+/type/\d+) ordinary_parameter\) cc=cc_default linkage=nlk_external result-parameter=False', pointee["description"])
                if not routine or view(routine[1], active) != "void" or view(routine[2], active) != "void*":
                    raise RuntimeError("The bind-destructor callback differs from the selected one-pointer void callback.")
                return "nint"
            if pointee["kind"] == "tk_struct" and pointee["incomplete"]:
                visited.add(pointee["locator"])
                if pointee["name"] != "sqlite3_stmt": raise RuntimeError("Unexpected incomplete API record.")
                return "void*"
            return view(match[1], active) + "*"
        if kind == "tk_struct" and descriptor["name"] == "sqlite3" and not descriptor["incomplete"]:
            if descriptor["size"] != "864" or descriptor["alignment"] != "8": raise RuntimeError("Captured complete database representation changed.")
            return db_name
        if kind == "tk_void": return "void"
        if kind == "tk_integer":
            bits = re.search(r';bits=(\d+);signed=(True|False);', description)
            scalar = {(8, "True"): "sbyte", (8, "False"): "byte", (32, "True"): "int", (64, "True"): "long"}
            if bits and (int(bits[1]), bits[2]) in scalar: return scalar[int(bits[1]), bits[2]]
        if kind == "tk_float" and ";bits=64;" in description and descriptor["size"] == "8": return "double"
        raise RuntimeError("Unaccepted captured API representation: " + locator)

    validated = []
    for name in APIS:
        function = selected[name][0]
        signature = function["signature"]
        if (not signature["hasPrototype"] or signature["variadic"] or signature["returnsValueAsParameter"]
            or signature["returnsValueViaCopyConstructor"] or signature["callingConvention"] not in {"cc_default", "cc_cdecl"}
            or any(parameter["role"] != "ordinary_parameter" for parameter in signature["parameters"])):
            raise RuntimeError("API has an unaccepted selected execution role: " + name)
        expected = (view(signature["returnType"]), [view(parameter["type"]) for parameter in signature["parameters"]])
        if methods[name][0] != expected: raise RuntimeError("Actual generated API differs from selected public representation: " + name)
        validated.append({"name": name, "selected": function, "generatedReturn": expected[0], "generatedParameters": expected[1]})
    aliases = "// Concrete aliases validated against the unpublished candidate receipt.\n" + f"global using Api = global::{namespace[1]}.{owner[1]};\n" + f"global using CapturedDatabase = global::{namespace[1]}.{db_name};\n"
    checks = {"candidateReceipt": receipt(args.candidate_receipt), "acceptedInventory": receipt(args.inventory), "providerPlan": receipt(args.provider_plan),
        "candidateFiles": candidate_files, "validator": receipt(Path(__file__))}
    if args.verify_only:
        recorded = json.loads(args.report.read_text())
        if args.aliases.read_text() != aliases or any(recorded[key] != value for key, value in checks.items()):
            raise RuntimeError("Build aliases/receipt/source set changed after candidate surface validation.")
        print("candidate-api-surface=verified")
        return 0
    if args.aliases.exists() or args.report.exists(): raise RuntimeError("Choose fresh alias/report files.")
    args.aliases.parent.mkdir(parents=True, exist_ok=True)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.aliases.write_text(aliases)
    report = {"status": "validated-unpublished-candidate-api-surface", "generatedExecution": False,
        "productionPublished": False, **checks, "aliases": receipt(args.aliases), "apis": validated,
        "candidateNativeImportCount": 88, "candidateMappingCompared": True, "wholeNativeProviderRuntimeAcceptance": False,
        "publicTypeDescriptions": [types[key] for key in sorted(visited)],
        "scope": "Generated static C# owner/type spelling validated against accepted public selected ordinary scalar/pointer facts; no execution/whole-provider acceptance"}
    args.report.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "apis": len(validated), "database": db_name, "statement": "void*"}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
