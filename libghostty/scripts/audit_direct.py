#!/usr/bin/env python3
"""Audit the selected direct Zig source products using retained delivery checks.

This checks static ownership, real public entry bodies, notices, selected runtime
and native-import providers. Execution and physical ABI qualification are separate.
"""
import argparse
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import re


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest().upper()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--products", type=Path, required=True)
    parser.add_argument("--engine-object", type=Path, required=True)
    parser.add_argument("--boundary-object", type=Path, required=True)
    parser.add_argument("--raw-audit", type=Path, required=True)
    parser.add_argument("--processed-audit", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    repo = root.parent
    spec = importlib.util.spec_from_file_location("direct_delivery_checks", root / "tests/audit.py")
    audit = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(audit)
    required = json.loads((root / "config/direct.json").read_text())
    names = required["managed_roots"]
    audit._require(len(names) == len(set(names)) == 96, "Direct vt-1.0.0 scope needs 96 unique selected roots")
    products = json.loads(args.products.read_text())
    graph_path = Path(products["graph"])
    graph = json.loads(graph_path.read_text())
    for key in ("schema_version", "root_module", "output_mode", "optimize_mode", "stdlib_types",
                "function_pointer_abi", "managed_roots"):
        audit._require(graph[key] == required[key], "Direct graph selection differs: " + key)
    audit._require([module["name"] for module in graph["modules"]] == required["expected_modules"],
                   "Unexpected direct module inventory/order")
    for module in graph["modules"]:
        audit._require({dependency["name"] for dependency in module["dependencies"]}
                       == audit.MODULE_DEPS[module["name"]], "Unexpected direct dependency graph")
        audit._require(Path(module["root"]).is_file(), "Missing actual module source")
    pins = json.loads((root / "config/source.json").read_text())
    source_roots = {item["name"]: root / "ref" / item["directory"]
                    for item in pins.get("license_sources", [])}
    source_roots.update(product=root / "ref" / pins["directory"],
                        zig=Path(graph["zig_lib_dir"]).parent)
    audit._require(Path(graph["modules"][0]["root"]).resolve()
                   == (source_roots["product"] / "src/lib_vt.zig").resolve(), "Unexpected Zig engine root")
    objects = {str(path.resolve()): path.read_text() for path in (args.engine_object, args.boundary_object)}
    runtime_records, runtime_imports = audit.runtime_inventory(repo, include_zig=True)
    results = []
    for form in ("raw", "processed"):
        project = Path(products[form]).resolve()
        sources = audit.owned_sources(project)
        records = [record for name, text in sources.items() for record in audit.methods(text, name)]
        for name, text in sources.items():
            code = audit._mask(text)
            audit._require(re.findall(r"\bnamespace\s+([\w.]+)\s*;", code) == ["Managed"]
                           and not re.search(r"\bnamespace\s+[\w.]+\s*\{", code),
                           "Unexpected direct source namespace: " + name)
            audit._require(not re.search(r"^\s*#\s*(?:if|elif|else|endif|define|undef)\b", code, re.M),
                           "Conditional compiled source is outside the direct audit")
            audit._require(not re.search(r"\busing\s+(?:static\s+|\w+\s*=)[^;]*"
                                         r"(?:NativeLibrary|NativeImports|AssemblyLoadContext|Assembly)\b", code),
                           "Unexpected dynamic-loader alias in direct source")
            audit._require(not audit.DIAGNOSTICS.search(code)
                           and "__FactoryAllocation" not in code and "__RunFactoryAllocation" not in code,
                           "Diagnostic entrypoint escaped into direct delivery")
        exports = audit._exports(records, set(names))
        pe_path = getattr(args, form + "_audit")
        pe = json.loads(pe_path.read_text())
        audit._require(Path(pe["source_directory"]).resolve() == project.parent
                       and pe["source_files"] == len(sources)
                       and pe["source_bytes"] == sum((project.parent / name).stat().st_size for name in sources),
                       "PE audit source ownership mismatch")
        audit._require(sha(pe["assembly"]) == pe["assembly_sha256"], "PE audit assembly changed")
        selected = [method for method in pe["public_api"] if method["name"] in names]
        audit._require(Counter(method["name"] for method in selected) == Counter(names)
                       and all(method["owner"] == "Managed.GhosttyVt" and method["has_body"]
                               and method["il_bytes"] > 0 for method in selected),
                       "Selected PE entry methods need unique actual owner bodies")
        audit._require(all(name == "mscorlib" or name == "netstandard" or name.startswith("System")
                           or name == "Microsoft.CSharp" for name in pe["assembly_references"]),
                       "Unexpected direct PE assembly dependency")
        results.append(dict(form=form, project=str(project), source_files=sorted(sources),
                            selected_roots=len(names), source_exports=exports, pe_entries=selected,
                            assembly=pe["assembly"], assembly_sha256=pe["assembly_sha256"],
                            assembly_references=pe["assembly_references"],
                            dependencies=audit.project_inputs(project),
                            notices=audit.notices(project, pins, source_roots, repo),
                            host_boundary=audit.host_boundary(sources, records, runtime_records,
                                                             runtime_imports, pins["allowed_host_libraries"]),
                            import_attribution=audit.attribute_imports(objects, records, runtime_records, sources)))
    runtime_files = sorted((repo / "DotCC.Libc").glob("*.cs"))
    report = dict(status="pass", products=str(args.products.resolve()), graph=str(graph_path),
                  graph_sha256=sha(graph_path), selected_module_count=len(graph["modules"]),
                  modules=[dict(name=module["name"], source=module["root"], sha256=sha(module["root"]))
                           for module in graph["modules"]],
                  object_inputs=[dict(path=path, sha256=sha(path)) for path in objects],
                  runtime_profile="Auto with Zig source objects: all authored DotCC.Libc resources",
                  runtime_files=[dict(path=str(path), sha256=sha(path)) for path in runtime_files],
                  results=results,
                  limits=["Static selected-product audit; entry-body presence is not semantic or ABI certification.",
                          "Import selection is lexical; runtime/physical ABI and rooted NativeAOT are separate checks."])
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + "\n")
    print(f"direct delivery audit PASS: raw/processed{len(names)} source+PE bodies, notices, ownership and host imports")


if __name__ == "__main__":
    main()
