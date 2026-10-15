#!/usr/bin/env python3
"""Validate genuine diagnostic/CLI source and emit direct-call type aliases.

No compilation, reflection, source-language reconstruction or native API fallback.
The actual generated public C# declarations choose the C# spelling; accepted
public report types establish the selected scalar/pointer representation.
"""
from __future__ import annotations
import argparse
import copy
import hashlib
import json
import os
from pathlib import Path
import re
import xml.etree.ElementTree as ET

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


def production_evidence(project: Path, completion_path: Path, program_path: Path, manifest_path: Path,
    inventory: dict, plan: dict) -> dict:
    directory = project.resolve().parent
    if project.resolve() != directory / "Generated.csproj" or completion_path.resolve() != directory / "completion.json" or program_path.resolve() != directory / "program-report.json":
        raise RuntimeError("Production evidence must name the actual CLI project/completion/program report together.")
    completion, program = json.loads(completion_path.read_text()), json.loads(program_path.read_text())
    request = copy.deepcopy(json.loads(manifest_path.read_text()))
    def resolve(value: str) -> str:
        return os.path.abspath(manifest_path.resolve().parent / value)
    request["output"]["directory"] = resolve(request["output"]["directory"])
    request["runtime"]["runtimeProjectPath"] = resolve(request["runtime"]["runtimeProjectPath"])
    for item in request["inputs"]: item["path"] = resolve(item["path"])
    for binding in request["runtime"]["bindings"]: binding["libraryPath"] = resolve(binding["libraryPath"])
    if (completion["complete"] is not True or completion["manifestVersion"] != request["manifestVersion"]
        or completion["translatorVersion"] != request["translatorVersion"] or program["translatorVersion"] != request["translatorVersion"]
        or completion["runtimeVersion"] != request["runtime"]["version"] or request["translatorVersion"] != inventory["translatorVersion"]
        or program["reportVersion"] != 1 or request["runtime"]["version"] != request["translatorVersion"] or program["inventoryComplete"] is not True
        or program["translationComplete"] is not True or any(item["severity"] == "error" for item in program["diagnostics"])):
        raise RuntimeError("CLI completion/report/version does not establish successful complete source translation.")
    if request["manifestVersion"] != 1: raise RuntimeError("Unsupported production manifest version.")
    for key in ("target", "link", "budgets"):
        if any(program[key].get(name) != value for name, value in request[key].items()):
            raise RuntimeError("Completed report differs from the explicit request: " + key)
    if program["target"] != inventory["target"]:
        raise RuntimeError("Production source target differs from the accepted full SQLite target.")
    for key in ("roots", "output", "runtime"):
        if any(program[key].get(name) != value for name, value in request[key].items()):
            raise RuntimeError("Completed report differs from the explicit request: " + key)
    if (program["roots"]["mode"] != "library" or program["roots"]["retainAllDefinitions"] is not True or program["roots"]["exports"]
        or Path(program["output"]["directory"]).resolve() != directory or request["runtime"]["nativeErrorProtocol"] != "linuxX64LogicalErrno"):
        raise RuntimeError("Production source must retain all bodies as the explicitly selected library/provider profile.")
    accepted_inputs, completed_inputs = inventory["inputs"], program["inputs"]
    if len(accepted_inputs) != 1 or len(completed_inputs) != 1 or len(request["inputs"]) != 1:
        raise RuntimeError("Production SQLite requires the one unchanged full object.")
    actual, wanted, declared = completed_inputs[0], accepted_inputs[0], request["inputs"][0]
    identity = lambda item: (item["ordinal"], item["sha256"], item["artifactIdentity"])
    if (actual["sha256"] != INPUT_SHA256 or identity(actual) != identity(wanted)
        or [identity(item) for item in completion["inputs"]] != [identity(actual)]
        or any(actual[name] != declared[name] for name in ("path", "sha256", "artifactIdentity"))):
        raise RuntimeError("CLI completion/request/report do not identify the accepted immutable input.")
    required = {(item["kind"], item["linkageName"]): item for item in inventory["imports"] if item["required"]}
    imports = [item for item in program["imports"] if item["required"]]
    actual_imports = {(item["kind"], item["linkageName"]): item for item in imports}
    if len(required) != 103 or len(imports) != 103 or set(actual_imports) != set(required):
        raise RuntimeError("Completed report does not retain exactly all 103 public requirements.")
    bindings = request["runtime"]["bindings"]
    binding_map = {(item["kind"], item["linkageName"]): item for item in bindings}
    if len(bindings) != 88 or len(binding_map) != 88:
        raise RuntimeError("Production request requires exactly the explicit 88 native bindings.")
    providers = {item["path"]: item for item in plan["providerFiles"]}
    aliases = {"__USER_LABEL_PREFIX__" + name: name for name in ("open64", "stat64", "fstat64", "ftruncate64", "fcntl64", "pread64", "pwrite64", "mmap64", "lstat64")}
    if (plan["explicitAliases"] != aliases or len(plan["imports"]) != 103
        or {(item["capturedImport"]["kind"], item["capturedImport"]["linkageName"]) for item in plan["imports"]} != set(required)):
        raise RuntimeError("The explicitly accepted alias/requirement scope changed.")
    functions = {item["locator"]: item for item in program["functions"]}
    intrinsics = 0
    for selected in plan["imports"]:
        captured = selected["capturedImport"]
        key = captured["kind"], captured["linkageName"]
        current, accepted = actual_imports[key], required[key]
        for field in ("locator", "name", "signature", "callingConvention", "directCalls", "addressReferences", "addressExposed"):
            if current[field] != accepted[field] or current[field] != captured[field]:
                raise RuntimeError("Completed public requirement changed: " + str(key))
        if functions[current["locator"]]["signature"] != selected["selectedSignature"]:
            raise RuntimeError("Completed selected function signature differs from its captured provider plan.")
        if selected["candidateProviderGroup"] == "target-intrinsic":
            if (key in binding_map or current["provider"] != "RoslynObj.CSharp.Runtime.TargetIntrinsics" or current["bindingStatus"] != "recorded-intrinsic-lowering-selected"
                or functions[current["locator"]]["builtin"] != selected["recordedBuiltin"]):
                raise RuntimeError("A retained compiler intrinsic lacks its selected runtime lowering or acquired a native fallback.")
            intrinsics += 1
            continue
        binding, contracts = binding_map[key], selected["contracts"]
        if selected["hostEntryPoint"] != aliases.get(key[1], key[1]):
            raise RuntimeError("A production binding introduced an unreviewed native alias.")
        expected = {"provider": selected["candidateProviderGroup"], "symbol": selected["hostEntryPoint"],
            "libraryPath": selected["requestedProviderPath"], "librarySha256": providers[selected["requestedProviderPath"]]["sha256"],
            "signature": captured["signature"], "callingConvention": captured["callingConvention"],
            "ownership": contracts["ownership"], "errorContract": contracts["errors"],
            "threadContract": contracts["threads"], "callbackContract": contracts["callbacks"]}
        if any(binding.get(name) != value for name, value in expected.items()):
            raise RuntimeError("Production native binding/provider/contract differs from the explicit accepted plan: " + str(key))
        report_fields = {"provider": "provider", "providerSymbol": "symbol", "bindingSignature": "signature", "bindingCallingConvention": "callingConvention",
            "ownership": "ownership", "errorContract": "errorContract", "threadContract": "threadContract", "callbackContract": "callbackContract"}
        if current["bindingStatus"] != "declared-unverified" or any(current[name] != binding[value] for name, value in report_fields.items()):
            raise RuntimeError("Completed report does not preserve the explicit native binding contract: " + str(key))
    if intrinsics != 15 or set(binding_map) != {key for key, item in actual_imports.items() if item["provider"] != "RoslynObj.CSharp.Runtime.TargetIntrinsics"}:
        raise RuntimeError("Completed native/intrinsic requirement partition changed.")
    if set(request["runtime"]["providers"]) != {binding["provider"] for binding in bindings}:
        raise RuntimeError("Production provider declarations differ from the exact native binding groups.")
    retained = lambda value: {item["locator"] for item in value["functions"] if item["requiredBody"]}
    if retained(program) != retained(inventory): raise RuntimeError("Completed report reduced the retained application definitions.")
    files, seen = [], set()
    for item in completion["files"]:
        path = (directory / item["relativePath"]).resolve()
        if not path.is_relative_to(directory) or path == completion_path.resolve() or path in seen:
            raise RuntimeError("A completion file escaped/duplicated its output inventory.")
        actual_file = receipt(path)
        if actual_file["sha256"] != item["sha256"] or actual_file["bytes"] != item["sizeBytes"]:
            raise RuntimeError("Published CLI file differs from its completion hash/size: " + str(path))
        files.append(actual_file)
        seen.add(path)
    if not {directory / name for name in ("Generated.cs", "Generated.csproj", "Generated.NativeImports.json", "Generated.Abi.c", "program-report.json")} <= seen:
        raise RuntimeError("CLI completion omitted a required generated source/provider/program artifact.")
    references = ET.parse(project).getroot().findall(".//ProjectReference")
    if len(references) != 1 or (directory / references[0].attrib["Include"]).resolve() != Path(request["runtime"]["runtimeProjectPath"]):
        raise RuntimeError("Actual generated project does not reference the exact requested target runtime.")
    return {"completion": receipt(completion_path), "programReport": receipt(program_path), "requestManifest": receipt(manifest_path),
        "files": files, "input": actual, "retainedDefinitions": len(retained(program)), "runtimeProjectPath": request["runtime"]["runtimeProjectPath"],
        "namespaceName": request["output"]["namespaceName"], "className": request["output"]["className"],
        "requiredImports": 103, "nativeBindings": 88, "runtimeIntrinsics": 15, "sourcePublished": True, "completionValidated": True,
        "scope": "CLI source publication and exact request/public-requirement reconciliation; build/runtime/provider/T4 coverage is separate"}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--generated-project", type=Path, required=True)
    parser.add_argument("--mode", choices=("diagnostic", "production"))
    parser.add_argument("--candidate-receipt", type=Path)
    parser.add_argument("--completion", type=Path)
    parser.add_argument("--program-report", type=Path)
    parser.add_argument("--request-manifest", type=Path)
    parser.add_argument("--inventory", type=Path, default=Path("/mnt/storage/roslynobj-csharp-20261015/t0/sqlite-inventory-address-closure/program-report.json"))
    parser.add_argument("--provider-plan", type=Path, default=Path(__file__).resolve().parents[2] / "acceptance/sqlite/provider-plan.json")
    parser.add_argument("--aliases", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    directory = args.generated_project.resolve().parent
    # The existing project build target passes its historical CandidateReceipt
    # property. In production it names the real completion file; replay the
    # explicit production evidence previously recorded by initial validation.
    if args.verify_only and args.mode is None:
        previous = json.loads(args.report.read_text())
        if previous.get("sourceMode") == "production":
            evidence = previous["productionEvidence"]
            args.mode = "production"
            args.completion = Path(evidence["completion"]["path"])
            args.program_report = Path(evidence["programReport"]["path"])
            args.request_manifest = Path(evidence["requestManifest"]["path"])
            if args.candidate_receipt is None or args.candidate_receipt.resolve() != args.completion.resolve():
                raise RuntimeError("Production build must pass the actual validated completion receipt.")
    production = args.mode == "production"
    if production and (not all((args.completion, args.program_report, args.request_manifest)) or args.candidate_receipt is not None and not args.verify_only):
        parser.error("Production mode requires explicit --completion, --program-report and --request-manifest, without a diagnostic receipt.")
    if not production and (args.candidate_receipt is None or any((args.completion, args.program_report, args.request_manifest))):
        parser.error("Diagnostic mode requires --candidate-receipt; production evidence requires explicit production mode.")
    if digest(args.inventory) != INVENTORY_SHA256:
        raise RuntimeError("The accepted public inventory report identity changed.")
    inventory = json.loads(args.inventory.read_text())
    plan = json.loads(args.provider_plan.read_text())
    if plan["acceptedInventoryReportSha256"] != INVENTORY_SHA256:
        raise RuntimeError("The provider plan differs from the accepted public inventory.")
    published = production_evidence(args.generated_project, args.completion, args.program_report, args.request_manifest, inventory, plan) if production else None
    candidate_files = []
    if production:
        candidate_files = published["files"]
    else:
        candidate = json.loads(args.candidate_receipt.read_text())
        if (candidate["input"]["sha256"] != INPUT_SHA256 or candidate["errorCount"] != 0 or candidate["candidateGenerated"] is not True
            or candidate["productionPublished"] is not False or candidate["outputPublished"] is not False
            or (directory / "completion.json").exists()):
            raise RuntimeError("A zero-error, unpublished full-object diagnostic candidate is required.")
        if Path(candidate["diagnosticCandidate"]).resolve() != directory or args.generated_project.resolve() != directory / "Generated.csproj":
            raise RuntimeError("Requested project is not the exact diagnostic candidate output.")
        for item in candidate["candidateFiles"]:
            path = (directory / item["RelativePath"]).resolve()
            if not path.is_relative_to(directory) or path.name == "completion.json":
                raise RuntimeError("Candidate file escaped its output or claimed production completion.")
            actual = receipt(path)
            if actual["sha256"] != item["sha256"] or actual["bytes"] != item["bytes"]:
                raise RuntimeError("Candidate file differs from its exact emission receipt: " + str(path))
            candidate_files.append(actual)
        if candidate["providerPlan"]["sha256"] != digest(args.provider_plan) or candidate["artifact"] != inventory["inputs"][0]["artifactIdentity"]:
            raise RuntimeError("Candidate artifact/provider plan differs from accepted public evidence.")
    if not {"Generated.cs", "Generated.csproj", "Generated.NativeImports.json", "Generated.Abi.c"} <= {Path(p["path"]).name for p in candidate_files}:
        raise RuntimeError("Candidate receipt omitted required source/project/provider artifacts.")
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
    types = {item["locator"]: item for item in inventory["types"]}
    selected = {name: [function for function in inventory["functions"] if function["currentName"] == name and function["requiredBody"]] for name in APIS}
    if any(len(functions) != 1 for functions in selected.values()):
        raise RuntimeError("A harness API lacks exactly one selected retained definition.")
    source = (directory / "Generated.cs").read_text()
    namespace = re.search(r'^namespace ([A-Za-z_][\w.]*)\s*;', source, re.M)
    owner = re.search(r'^public static unsafe class ([A-Za-z_]\w*)\s*$', source, re.M)
    if namespace is None or owner is None: raise RuntimeError("Expected one concrete generated static application owner.")
    if production and (namespace[1] != published["namespaceName"] or owner[1] != published["className"]):
        raise RuntimeError("Actual generated C# owner differs from the published request/report.")
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
    alias_basis = "CLI completion receipt" if production else "unpublished candidate receipt"
    aliases = f"// Concrete aliases validated against the {alias_basis}.\n" + f"global using Api = global::{namespace[1]}.{owner[1]};\n" + f"global using CapturedDatabase = global::{namespace[1]}.{db_name};\n"
    checks = {"acceptedInventory": receipt(args.inventory), "providerPlan": receipt(args.provider_plan), "validator": receipt(Path(__file__))}
    if production: checks.update({"sourceMode": "production", "sourcePublished": True, "completionValidated": True,
        "productionEvidence": published, "publishedFiles": candidate_files})
    else: checks.update({"candidateReceipt": receipt(args.candidate_receipt), "candidateFiles": candidate_files})
    if args.verify_only:
        recorded = json.loads(args.report.read_text())
        if args.aliases.read_text() != aliases or any(recorded[key] != value for key, value in checks.items()):
            raise RuntimeError("Build aliases/receipt/source set changed after candidate surface validation.")
        print("production-api-surface=verified" if production else "candidate-api-surface=verified")
        return 0
    if args.aliases.exists() or args.report.exists(): raise RuntimeError("Choose fresh alias/report files.")
    args.aliases.parent.mkdir(parents=True, exist_ok=True)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.aliases.write_text(aliases)
    report = {"status": "validated-cli-published-api-surface" if production else "validated-unpublished-candidate-api-surface", "generatedExecution": False,
        "productionPublished": production, **checks, "aliases": receipt(args.aliases), "apis": validated,
        ("nativeImportCount" if production else "candidateNativeImportCount"): 88,
        ("nativeMappingCompared" if production else "candidateMappingCompared"): True, "wholeNativeProviderRuntimeAcceptance": False,
        "publicTypeDescriptions": [types[key] for key in sorted(visited)],
        "scope": "Generated static C# owner/type spelling validated against accepted public selected ordinary scalar/pointer facts; no execution/whole-provider acceptance"}
    args.report.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "apis": len(validated), "database": db_name, "statement": "void*"}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
