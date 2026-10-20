"""Audit exact source ownership, IL closure and executed consumer dependencies."""
from __future__ import annotations
import hashlib
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET
from campaigns.recipes import helper, json_file
from campaigns.testing import forms, modes
from campaigns.translation import check_product

ASSEMBLIES = {"TranslatedZstd", "Managed.Zstd", "ManagedConsumer", "ManagedTests"}
ELF_LIBRARIES = {"libc.so.6", "libm.so.6", "libgcc_s.so.1", "libdl.so.2", "libpthread.so.0", "librt.so.1", "libresolv.so.2", "ld-linux-x86-64.so.2"}

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def sources(project, root, profile):
    directory = project.parent.resolve()
    tree = ET.parse(project)
    if list(tree.iter("PackageReference")) or list(tree.iter("ProjectReference")):
        raise RuntimeError("Generated zstd product must have no external implementation dependency")
    owned = set(json_file(directory / ".campaign-files.json"))
    generated = {directory / name for name in owned if name.endswith(".cs")}
    if not generated:
        raise RuntimeError("Missing generated source ownership manifest")
    combined = []
    for path in generated:
        if not path.resolve().is_relative_to(directory) or not path.is_file() or path.is_symlink():
            raise RuntimeError("Invalid generated source ownership: " + str(path))
        text = path.read_text()
        if re.search(r'\bclass\s+DotCc(?:Static)?Imports\b|\bNativeImports\s*\.\s*(?:LoadLibrary|TryResolveExport)\s*\(', text):
            raise RuntimeError("Unresolved required symbols use a native fallback: " + str(path))
        combined.append(text)
    for path in directory.rglob("*.cs"):
        if not {"bin", "obj"} & set(path.relative_to(directory).parts) and path not in generated:
            raise RuntimeError("Unowned compiled product source: " + str(path))
    adapter = (root / "src/Optimization/Primitives.cs").resolve()
    adapter_links = []
    for node in tree.iter("Compile"):
        if node.get("Include") and (path := (directory / node.get("Include")).resolve()) not in generated:
            if profile != "portable" or path != adapter or node.get("Link") != "Optimization/Primitives.cs" or path.is_symlink():
                raise RuntimeError("Unexpected linked generated implementation: " + str(node.attrib))
            adapter_links.append(path)
    if len(adapter_links) != (1 if profile == "portable" else 0):
        raise RuntimeError("Profile must link exactly its original authored primitive adapter")
    primitives = helper(root / "scripts/primitives.py")
    if adapter_links:
        names = re.findall(r'\bpublic\s+static\s+\w+\s+(\w+)\s*\(', adapter.read_text())
        if sorted(names) != primitives.all_names(root):
            raise RuntimeError("Authored primitive method inventory differs")
    text = "\n".join(combined)
    if profile == "portable":
        missing = [name for name in primitives.all_names(root)
                   if not re.search(r'\bManaged\.Zstd\.Optimization\.Primitives\.' + re.escape(name) + r'\s*\(', text)]
        if missing:
            raise RuntimeError("Required generated typed primitive calls missing: " + repr(missing))
    elif "Managed.Zstd.Optimization.Primitives" in text:
        raise RuntimeError("Original profile unexpectedly uses authored primitive replacements")
    required = ("ZSTD_compress2", "ZSTD_decompressStream", "ZDICT_trainFromBuffer", "ZSTD_compressBlock_fast", "ZSTD_compressBlock_btultra2", "ZSTD_ldm_generateSequences")
    if not all(re.search(r'\b' + name + r'\s*\(', text) for name in required):
        raise RuntimeError("Required translated algorithm entrypoints missing")
    pin = json_file(root / "config/source.json")
    reference = root / "ref" / pin["directory"]
    for name in ("LICENSE", "COPYING"):
        if name not in owned or (directory / name).read_bytes() != (reference / name).read_bytes():
            raise RuntimeError("Original license text missing or changed: " + name)
    notices = []
    for relative in ("lib/common/xxhash.c", "lib/common/xxhash.h", "lib/dictBuilder/divsufsort.c"):
        original = (reference / relative).read_text()
        if not original.startswith("/*") or "*/" not in original:
            raise RuntimeError("Upstream source notice boundary differs: " + relative)
        notices.append(relative + "\n" + original[:original.index("*/") + 2])
    if "THIRD-PARTY-NOTICES" not in owned or (directory / "THIRD-PARTY-NOTICES").read_text() != "\n\n".join(notices) + "\n":
        raise RuntimeError("Third-party source notices missing or changed")
    for name in ("LICENSE", "COPYING", "THIRD-PARTY-NOTICES"):
        item = next((node for node in tree.iter("None") if node.get("Update") == name), None)
        if item is None or item.get("TargetPath") != "Zstd." + name or any(item.get(key) != "PreserveNewest" for key in ("CopyToOutputDirectory", "CopyToPublishDirectory")):
            raise RuntimeError("Notice not delivered by build/publish: " + name)
    forbidden = re.compile(r'\b(?:DllImport|LibraryImport)\s*\(|\bNativeLibrary\s*\.|\bAssembly\s*\.\s*Load|\b(?:LoadLibrary|dlopen|SetDllImportResolver)\s*\(|\bProcessStartInfo\b|\bProcess\s*\.\s*Start\s*\(|\bnew\s+Process\b|\bSystem\.IO\.Compression\b')
    authored = [*(root / "src/Managed.Zstd").glob("*.cs"), *(root / "samples/ManagedConsumer").glob("*.cs"), *adapter_links]
    for path in authored:
        if forbidden.search(path.read_text()):
            raise RuntimeError("Authored native/subprocess/alternate engine forbidden: " + str(path))
    for project_path in (root / "src/Managed.Zstd/Managed.Zstd.csproj", root / "samples/ManagedConsumer/ManagedConsumer.csproj"):
        for item in ET.parse(project_path).iter():
            if item.tag in ("PackageReference", "NativeReference", "NativeLibrary"):
                raise RuntimeError("Unexpected authored product dependency: " + str(item.attrib))
            if item.tag == "Compile" and item.get("Include"):
                linked_source = (project_path.parent / item.get("Include")).resolve()
                if linked_source not in {path.resolve() for path in authored}:
                    raise RuntimeError("Unexpected authored source link: " + str(linked_source))
    return dict(project=str(project), profile=profile, generated_source_count=len(generated), required_algorithms=list(required), generated_required_imports=[],
                primitive_adapter_links=[dict(path=str(path), sha256=digest(path)) for path in adapter_links],
                authored_sources=[dict(path=str(path), sha256=digest(path)) for path in authored])

def primitive_selection(ctx):
    """Recheck physical source/signature selection, independently of build success."""
    origin = ctx.receipt
    if "primitive_overrides" not in origin:
        pointer = json_file(ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json"))
        origin = json_file(Path(pointer["receipt"]))
    record = origin.get("primitive_overrides", {})
    policy = json_file(ctx.root / "config/translation.json")
    if record.get("profile") != ctx.profile or len(record.get("units", [])) != len(policy["units"]) + 1:
        raise RuntimeError("Complete selected-profile primitive override evidence missing")
    primitives = helper(ctx.root / "scripts/primitives.py")
    expected = primitives.inventory(ctx.root)["expected_units"]
    source = Path(origin["zstd_selection"]["staged_source"])
    wanted = {str((source / name).resolve()): names for name, names in expected.items() if name in policy["units"]}
    wanted[str((ctx.root / policy["bridge"]).resolve())] = expected[policy["bridge"]]
    observed = {str(Path(row["unit"]).resolve()) for row in record["units"]}
    if observed != set(wanted):
        raise RuntimeError("Primitive source-unit coverage differs")
    verified = []
    for row in record["units"]:
        unit = Path(row["unit"])
        actual = primitives.validate_report(ctx, Path(row["report"]), unit, source, wanted[str(unit.resolve())])
        if actual != row:
            raise RuntimeError("Retained primitive provenance changed: " + str(unit))
        verified.append(dict(unit=str(unit), report=str(row["report"]), report_sha256=digest(Path(row["report"])), selected=len(actual["selected"])))
    adapter = (ctx.root / "src/Optimization/Primitives.cs").resolve()
    if ctx.profile == "portable":
        if Path(record.get("adapter", "")).resolve() != adapter or origin.get("production_inputs", {}).get(str(adapter)) != digest(adapter):
            raise RuntimeError("Selected original authored primitive source binding differs")
    elif record.get("adapter") is not None:
        raise RuntimeError("Original profile unexpectedly retains primitive adapter")
    return dict(profile=ctx.profile, units=verified, selections=sum(row["selected"] for row in verified))

def notices(directory, product):
    records = []
    for name in ("LICENSE", "COPYING", "THIRD-PARTY-NOTICES"):
        path = directory / ("Zstd." + name)
        if not path.is_file() or path.read_bytes() != (product.parent / name).read_bytes():
            raise RuntimeError("Application delivery notice missing/changed: " + str(path))
        records.append(dict(path=str(path), sha256=digest(path)))
    return records

def dependencies(path):
    document = json_file(path)
    libraries = document.get("libraries", {})
    if not libraries:
        raise RuntimeError("Missing application dependency inventory: " + str(path))
    for identity, entry in libraries.items():
        name = identity.split("/")[0]
        framework = entry.get("type") == "runtimepack" and name == "runtimepack.Microsoft.NETCore.App.Runtime.linux-x64"
        if not framework and (entry.get("type") != "project" or name not in ASSEMBLIES):
            raise RuntimeError("Unapproved application dependency: " + identity)
    for target in document.get("targets", {}).values():
        for identity, entry in target.items():
            if not identity.startswith("runtimepack.Microsoft.NETCore.App.Runtime.") and (entry.get("native") or any(value.get("assetType") == "native" for value in entry.get("runtimeTargets", {}).values())):
                raise RuntimeError("Unexpected native package asset: " + identity)
    return dict(path=str(path), sha256=digest(path), libraries=sorted(libraries))

def aot_il(ctx, project, build, label):
    command = build["command"]
    properties = [arg for arg in command if arg.startswith("-p:")]
    properties += ["-p:Configuration=Release", "-p:RuntimeIdentifier=" + command[command.index("-r") + 1]]
    if "--artifacts-path" in command:
        properties += ["-p:ArtifactsPath=" + command[command.index("--artifacts-path") + 1]]
    target = Path(ctx.run(["dotnet", "msbuild", project, "-nologo", "-getProperty:TargetPath", *properties], label).strip())
    if not target.is_absolute() or not target.is_file():
        raise RuntimeError("Exact NativeAOT input IL missing: " + str(target))
    return target

def run(ctx, suite):
    report = dict(status="running", forms=[], artifacts=[])
    report_path = ctx.artifacts / "audit-report.json"
    try:
        report["primitive_selection"] = primitive_selection(ctx)
        inspector = ctx.root / "tests/Audit/Zstd.Audit.csproj"
        output = ctx.work / "audit-inspector"
        ctx.managed("build", inspector, "audit-inspector-build", "-m:1", "--artifacts-path", ctx.work / "audit-intermediates", "-o", output)
        consumer_project = ctx.root / "samples/ManagedConsumer/ManagedConsumer.csproj"
        api_project = ctx.root / "src/Managed.Zstd/Managed.Zstd.csproj"
        sample = ET.parse(consumer_project)
        if list(sample.iter("TrimmerRootAssembly")) or not any("src/Managed.Zstd/Managed.Zstd.csproj" in node.get("Include", "") for node in sample.iter("ProjectReference")):
            raise RuntimeError("Ordinary consumer must reference owning API and exercise normal trimming")
        if not any(node.get("Include") == "$(ZstdProject)" for node in ET.parse(api_project).iter("ProjectReference")):
            raise RuntimeError("Owning API does not reference selected generated product")
        for form in forms(ctx):
            product = check_product(ctx, form)
            report["forms"].append(dict(form=form, **sources(product, ctx.root, ctx.profile)))
            for mode in modes(ctx):
                consumer_label = "consumer-" + form + "-" + mode
                consumer_runs = [entry for entry in ctx.receipt["commands"] if entry.get("label") == consumer_label + "-run" and entry.get("status") == "passed"]
                if len(consumer_runs) != 1:
                    raise RuntimeError("Audit requires exact current-invocation consumer execution; select consumer and abi before audit")
                command = consumer_runs[0]["command"]
                consumer = Path(command[1] if mode == "jit" else command[0])
                harnesses = [entry for entry in ctx.receipt.get("zstd_managed_harnesses", []) if entry["form"] == form and entry["mode"] == mode and entry["whole_library_rooted"]]
                if len(harnesses) != 1:
                    raise RuntimeError("Audit requires a whole-library-rooted current-invocation managed harness")
                harness = harnesses[0]
                harness_path = Path(harness["executable"])
                if digest(harness_path) != harness["executable_sha256"]:
                    raise RuntimeError("Executed rooted harness changed")
                row = dict(form=form, mode=mode, consumer=str(consumer), consumer_sha256=digest(consumer), rooted_harness=str(harness_path),
                           consumer_notices=notices(consumer.parent, product), harness_notices=notices(harness_path.parent, product))
                report["artifacts"].append(row)
                if mode == "aot":
                    path = Path(harness["aot_map"])
                    if digest(path) != harness["aot_map_sha256"]:
                        raise RuntimeError("Actual rooted AOT method map changed")
                    methods = [node.get("Name", "") for node in ET.parse(path).iter("MethodCode") if int(node.get("Length", "0")) > 0]
                    required = ("ZSTD_compress2", "ZSTD_decompressStream", "ZDICT_trainFromBuffer", "ZSTD_compressBlock_btultra2", "ZSTD_ldm_generateSequences")
                    missing = [name for name in required if not any("TranslatedZstd_Managed_Zstd_Native_Zstd__" + name in method for method in methods)]
                    if missing:
                        raise RuntimeError("Rooted AOT translated machine code missing: " + repr(missing))
                    row["aot_machine_code"] = dict(map=str(path), required=list(required), emitted_method_count=len(methods))
                for role, entry, build_label, project in (
                    ("consumer", consumer, consumer_label + "-publish", consumer_project),
                    ("harness", harness_path, "managed-tests-" + form + "-aot-publish", ctx.root / "tests/ManagedTests/ManagedTests.csproj"),
                ):
                    if mode == "aot":
                        if sys.platform == "linux":
                            dynamic = ctx.run(["readelf", "-d", entry], "audit-" + form + "-" + mode + "-" + role + "-elf")
                            needed = re.findall(r'\(NEEDED\).*\[([^]]+)\]', dynamic)
                            if not needed or set(needed) - ELF_LIBRARIES:
                                raise RuntimeError("Unapproved native application dependency: " + repr(needed))
                            row[role + "_native_dependencies"] = needed
                        builds = [record for record in ctx.receipt["commands"] if record.get("label") == build_label and record.get("status") == "passed"]
                        if len(builds) != 1:
                            raise RuntimeError("Exact NativeAOT publishing command absent: " + build_label)
                        il = aot_il(ctx, project, builds[0], "audit-" + form + "-" + role + "-il-target")
                    else:
                        il = entry
                    row[role + "_dependencies"] = dependencies(il.with_suffix(".deps.json"))
                    metadata = ctx.run(["dotnet", output / "Zstd.Audit.dll", role, il, "facade", il.parent / "Managed.Zstd.dll", "product", il.parent / "TranslatedZstd.dll"],
                                       "audit-" + form + "-" + mode + "-" + role + "-metadata")
                    row[role + "_metadata"] = json.loads(metadata)
        report["status"] = "passed"
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        report_path.write_text(json.dumps(report, indent=2) + "\n")
        ctx.receipt["zstd_delivery_audit"] = str(report_path)
        ctx.save()
