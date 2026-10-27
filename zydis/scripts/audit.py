"""Audit delivered product closure without building tools or managed projects.

The recipe calls run(ctx, suite) after qualification and consumer suites. Evidence
comes from that context's command receipt and reports, never a glob of old runs.
Both forms and both distinct AOT executables are required, even for --mode jit.
Missing evidence fails and still writes zydis-audit.json plus a receipt pointer.

For a private emission, use --preliminary-project PATH --artifacts DIRECTORY
[--emission-receipt PATH]. This performs the same static checks, writes an
explicitly unqualified report, and returns failure because delivery/AOT evidence
is absent. It neither publishes artifacts nor invokes the campaign runner.
"""
from __future__ import annotations

import argparse
from bisect import bisect_right
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import sys
from types import SimpleNamespace
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.delivery import source_files, validate_emission
from campaigns.identity import write_json
from campaigns.layout import Layout
from campaigns.recipes import helper
from campaigns.translation import check_product


def _json(path):
    return json.loads(Path(path).read_text())


def _hash(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def _require(condition, message):
    if not condition:
        raise RuntimeError(message)


# Preserve offsets while hiding comments and strings from symbol searches.
_TOKENS = re.compile(r'//[^\n]*|/\*.*?\*/|""".*?"""|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)


def _code(text):
    return _TOKENS.sub(lambda match: re.sub(r"[^\n]", " ", match[0]), text)


def _close(code, opening):
    depth = 0
    for match in re.finditer(r"[{}]", code[opening:]):
        depth += 1 if match[0] == "{" else -1
        if not depth:
            return opening + match.start()
    raise RuntimeError("Unbalanced emitted method body")


_METHOD = re.compile(
    r"(?m)^[ \t]*(public|internal|private)\s+"
    r"(?:(?:static|unsafe|partial|new|virtual|override|async)\s+)*"
    r"[\w.<>:*?\[\]]+\s+(@?\w+)\s*\([^;{}]*?\)\s*(\{|=>)")
_IMPORT = re.compile(
    r'\[(?:[\w.]+\.)?(DllImport|LibraryImport)(?:Attribute)?\s*\(\s*"([^"]+)"([^\]]*)\)\]'
    r'\s*[^;{}]*?\b(\w+)\s*\([^;{}]*?\)\s*;')
_MODULE = re.compile(r"(?m)^// ---- ([\w.]+\.cs) ----$")


def _headers(source):
    exports, types, files = set(), set(), []
    for path in sorted((source / "include/Zydis").rglob("*.h")):
        if "Internal" in path.relative_to(source).parts:
            continue
        text = re.sub(r"/\*.*?\*/|//[^\n]*", "", path.read_text(), flags=re.S)
        exports.update(re.findall(r"\bZYDIS_EXPORT\s+[^;{}#]*?\b(Zydis\w+)\s*\(", text))
        types.update(re.findall(r"\btypedef\s+(?:struct|enum|union)\s+(Zydis\w+)\b", text))
        files.append(dict(path=str(path), sha256=_hash(path)))
    _require(exports, "No upstream public exports found")
    return exports, types, files


def _inventory(ctx, policy):
    selected = helper(ctx.root / "scripts/campaign.py").native_configuration(ctx.root, ctx.sources or None)
    units = selected["units"]
    _require(len(units) == policy["sourceUnitCount"], "Full product must select all 18 library units")
    _require(all(path.parent == selected["sources"]["zydis"] / "src" for path in units),
             "A non-library or Zycore backend unit entered the product")
    required = {"-DZYDIS_STATIC_BUILD", "-DZYCORE_STATIC_BUILD", "-D__x86_64__",
                "-UZYDIS_MINIMAL_MODE", "-UZYDIS_DISABLE_DECODER", "-UZYDIS_DISABLE_ENCODER",
                "-UZYDIS_DISABLE_FORMATTER", "-UZYDIS_DISABLE_AVX512", "-UZYDIS_DISABLE_KNC", "-UZYDIS_DISABLE_SEGMENT"}
    _require(required <= set(selected["flags"]), "Missing full-feature/target profile flags")
    _require(not any(re.match(r"-D(?:ZYDIS_MINIMAL_MODE|ZYDIS_DISABLE_)", flag) for flag in selected["flags"]),
             "A disabled feature entered the full product profile")
    exported, types, headers = _headers(selected["sources"]["zydis"])
    _require(exported == set(policy["publicFunctions"]),
             "Current upstream public export inventory differs from audit policy: " +
             str(sorted(exported.symmetric_difference(policy["publicFunctions"]))))
    header = (selected["sources"]["zydis"] / "include/Zydis/Zydis.h").read_text()
    macro = re.search(r"(?m)^#define\s+ZYDIS_VERSION\s+(0x[0-9A-Fa-f]+)", header)
    _require(macro and int(macro[1], 16) == int(policy["publicVersion"]["value"], 16),
             "Upstream public version must be 4.1.0.0 despite release tag v4.1.1")
    return selected, exported, types, dict(
        units=[dict(path=str(path), sha256=_hash(path)) for path in units],
        native_flags=selected["flags"], headers=headers, public_functions=sorted(exported),
        public_aggregate_types=sorted(types), public_version=policy["publicVersion"],
        zycore_compiled_units=[], scope="current source policy and content; hashes identify this audit, not historical provenance")


def _check_emission_profile(ctx, project):
    recipe = helper(ctx.root / "scripts/campaign.py")
    profile = getattr(ctx, "profile", recipe.recipe.default_profile)
    emission = recipe.emission_identity(profile)
    identity = project.parent / "zydis-emission-profile.json"
    # Historical default products predate the explicit profile stamp. The
    # optional profile always requires it, independent of provenance policy.
    if identity.is_file() or profile != recipe.recipe.default_profile:
        _require(identity.is_file() and _json(identity) == emission,
                 "Product emission profile differs from selected campaign profile")


def _project(ctx, project, selected, policy):
    validate_emission(project.parent, policy["product"])
    _check_emission_profile(ctx, project)
    tree = ET.parse(project)
    _require(tree.findtext(".//OutputType") == "Library", "Product OutputType must be Library")
    _require(tree.findtext(".//AssemblyName") == policy["product"], "Unexpected product assembly name")
    for tag in ("ProjectReference", "PackageReference", "Reference", "NativeLibrary", "Import", "Target", "UsingTask", "StartupObject"):
        _require(not list(tree.iter(tag)), "Unexpected product dependency/build injection: " + tag)
    names = (project.parent / "Dotcc.SourceFiles.txt").read_text().splitlines()
    generated = {(project.parent / name).resolve() for name in names}
    observed = {path.resolve() for path in source_files(project.parent) if path.suffix == ".cs"}
    _require(observed == generated, "Unowned/missing C# product files: " + str(sorted(map(str, observed ^ generated))))
    config = _json(ctx.root / "config/translation.json")
    authored = {path.resolve() for glob in config["authoredCompileGlobs"] for path in ctx.root.glob(glob) if path.is_file()}
    _require(authored, "Missing authored API files")
    linked = []
    for node in tree.iter("Compile"):
        value = node.get("Include", "")
        _require(value and not any(part in value for part in ("$", "*", ";")) and
                 not any(node.get(key) for key in ("Condition", "Exclude", "Remove", "Update")),
                 "Unsupported/conditional product Compile entry: " + str(node.attrib))
        path = (project.parent / value).resolve()
        _require(path in authored, "Unexpected linked product source: " + str(path))
        link = node.get("Link") or node.findtext("Link")
        _require(link == "Authored/" + path.relative_to(ctx.root.resolve() / "src").as_posix(),
                 "Authored API link does not retain its original path: " + str(node.attrib))
        linked.append(path)
    _require(set(linked) == authored and len(linked) == len(authored), "Authored API links missing/duplicated")
    for node in tree.iter():
        if node.tag in ("EnableDefaultCompileItems", "EnableDefaultItems"):
            _require((node.text or "").lower() != "false", "Generated sources disabled in product")
    for notice in config["notices"]:
        path = project.parent / notice["target"]
        original = selected["sources"][notice["source"]] / notice["path"]
        _require(path.is_file() and path.read_bytes() == original.read_bytes(), "Missing/modified upstream notice: " + str(path))
        nodes = [n for n in tree.iter("None") if n.get("Update") == notice["target"]]
        _require(len(nodes) == 1 and all(nodes[0].get(key) == "PreserveNewest" for key in
                                       ("CopyToOutputDirectory", "CopyToPublishDirectory")),
                 "Notice is not retained in build and publish: " + str(path))
    ownership = project.parent / ".campaign-files.json"
    if ownership.exists():
        owned = _json(ownership)
        _require(set(names) | {project.name} <= set(owned), "Campaign ownership manifest omits product files")
    files = [*source_files(project.parent), *sorted(authored)]
    return sorted(generated), sorted(authored), dict(
        project=str(project), project_sha256=_hash(project),
        generated_manifest=names, authored_links=[str(path) for path in sorted(authored)],
        files=[dict(path=str(path), sha256=_hash(path)) for path in files],
        newest_input_mtime=max(path.stat().st_mtime for path in files))


def _symbols(generated, authored, exported, types, policy):
    methods, public_types, imports, runtime_calls, modules = Counter(), set(), [], [], set()
    api_bodies, version_values = {}, []
    for path in [*generated, *authored]:
        text = path.read_text()
        code = _code(text)
        markers = list(_MODULE.finditer(text)) if path in generated else []
        offsets = [m.start() for m in markers]
        # A module marker must follow the actual emitter's runtime boundary.
        boundary = text.find("// ---- Embedded DotCC.Libc runtime")
        def module_at(position):
            index = bisect_right(offsets, position) - 1
            return markers[index][1] if index >= 0 and boundary >= 0 and position > boundary else None
        for match in _IMPORT.finditer(text):
            # Ignore examples in XML/comments; the attribute remains visible.
            if not re.search(r"\b(?:DllImport|LibraryImport)(?:Attribute)?\b", code[match.start():match.start() + 100]):
                continue
            module = module_at(match.start())
            entry = re.search(r'EntryPoint\s*=\s*"([^"]+)"', match[3])
            entry = entry[1] if entry else match[4]
            allowed = policy["runtimeImports"].get(module, {}).get(match[2], [])
            _require(match[1] == "DllImport" and entry in allowed,
                     f"Unreviewed native import in {path}: {module} {match[2]}!{entry}")
            imports.append(dict(file=str(path), module=module, library=match[2], entry_point=entry,
                                method=match[4], classification="shared runtime platform service; reachability not asserted"))
        # All externs must have a reviewed platform import, including local externs.
        externs = re.findall(r"\bextern\s+[^;{}]*?\b(\w+)\s*\([^;{}]*?\)\s*;", code)
        _require(Counter(externs) == Counter(row["method"] for row in imports if row["file"] == str(path)),
                 "Unresolved/unclassified extern declaration in " + str(path))
        for match in re.finditer(r"\b(?:DllImport|LibraryImport)(?:Attribute)?\s*\(", code):
            _require(any(row["file"] == str(path) and row["method"] in text[match.start():text.find(";", match.start())]
                         for row in imports), "Unparsed native import attribute: " + str(path))
        for match in re.finditer(r"\bNativeLibrary\s*\.", code):
            module = module_at(match.start())
            _require(module in policy["runtimeDynamicModules"], "Product uses native dynamic loading: " + str(path))
            runtime_calls.append(dict(file=str(path), module=module, operation=code[match.end():].split("(", 1)[0].strip()))
        # Only scan translated/authored code for runtime helper usage and stubs.
        product_code = code[:boundary] if boundary >= 0 and path in generated else code
        for pattern in (r"\b(?:" + "|".join(map(re.escape, policy["prohibitedProductIdentifiers"])) + r")\w*\b",
                        r"\b(?:NativeImports\s*\.|dlopen\s*\(|dlsym\s*\(|LoadLibrary\s*\(|GetProcAddress\s*\()",
                        r"\b(?:NotImplementedException|MissingMethodException|EntryPointNotFoundException)\b",
                        r"\b(?:main|Main)\s*\("):
            hit = re.search(pattern, product_code)
            _require(hit is None, f"Prohibited product test/backend/stub/entrypoint in {path}: {hit[0] if hit else ''}")
        for pattern in policy["prohibitedProductStrings"]:
            # Test transcript sentinels are literal data, so inspect unmasked text.
            product_text = text[:boundary] if boundary >= 0 and path in generated else text
            _require(re.search(pattern, product_text) is None, "Test workload/transcript in product: " + str(path))
        public_types.update(re.findall(r"\bpublic\s+(?:unsafe\s+)?(?:partial\s+)?(?:struct|enum|class)\s+(Zydis\w+)\b", product_code))
        for match in _METHOD.finditer(product_code):
            name = match[2].lstrip("@")
            if name not in exported:
                continue
            _require(match[1] == "public", "Upstream API is no longer public: " + name)
            methods[name] += 1
            opening = match.end() - 1
            if match[3] == "{":
                body = product_code[opening + 1:_close(product_code, opening)].strip()
            else:
                body = "return " + product_code[match.end():product_code.index(";", match.end())].strip() + ";"
            api_bodies[name] = dict(file=str(path), body_sha256=hashlib.sha256(body.encode()).hexdigest())
            if name == "ZydisGetVersion":
                value = re.fullmatch(r"return\s+(0x[0-9A-Fa-f]+|[0-9]+)(?:[uUlL]+)?\s*;", body)
                _require(value is not None, "Unrecognized version function; review required")
                version_values.append(int(value[1], 0))
            if name not in policy["constantBodyExceptions"]:
                expression = re.fullmatch(r"return\s+(.+);", body, re.S)
                constant = False
                if expression:
                    tokens = re.sub(r"\b(?:0x[0-9a-fA-F]+|\d+)[uUlL]*\b", "", expression[1])
                    identifiers = re.findall(r"\b\w+\b", tokens)
                    constant = all(n in {"unchecked", "checked", "default", "null", "true", "false", "uint", "int", "ulong", "long", "byte", "sbyte", "short", "ushort", "nint", "nuint"}
                                   or n.startswith(("ZYAN_STATUS_", "ZYDIS_STATUS_")) for n in identifiers)
                _require(body and not constant and not body.startswith("throw "), "Empty/constant/unimplemented upstream API body: " + name)
        version_values.extend(int(n) for n in re.findall(r"\bconst\s+ulong\s+ZYDIS_VERSION\s*=\s*unchecked\(\(ulong\)\((\d+)UL\)\)", product_code))
        modules.update(m[1] for m in markers)
    _require(set(methods) == exported and all(n == 1 for n in methods.values()),
             "Missing/duplicate upstream public functions: " + str({name: methods[name] for name in sorted(exported) if methods[name] != 1}))
    _require(types <= public_types, "Missing public upstream aggregate types: " + str(sorted(types - public_types)))
    _require(len(version_values) == 2 and set(version_values) == {int(policy["publicVersion"]["value"], 16)},
             "Public ZYDIS_VERSION/function mismatch: " + str(version_values))
    return dict(public_functions=api_bodies, public_aggregate_types=sorted(types),
                reviewed_runtime_imports=imports, shared_runtime_modules=sorted(modules),
                generic_runtime_dynamic_operations=runtime_calls,
                limitations="Source closure and narrow constant-body checks; execution coverage comes only from separate qualification evidence")


def _dependencies(path, allowed_projects):
    data = _json(path)
    libraries = data.get("libraries", {})
    _require(libraries, "Empty dependency manifest: " + str(path))
    for name, row in libraries.items():
        if row.get("type") == "project":
            _require(name.rsplit("/", 1)[0] in allowed_projects, "Unexpected managed project dependency: " + name)
        else:
            _require(row.get("type") == "runtimepack" and name.startswith("Microsoft.NETCore.App.Runtime."),
                     "Unreviewed managed/package dependency: " + name)
    native = [name for target in data.get("targets", {}).values() for row in target.values() for name in row.get("native", {})]
    _require(not any(re.search(r"zydis|zycore", name, re.I) for name in native), "Native Zydis asset in dependency manifest")
    return dict(path=str(path), sha256=_hash(path), libraries=libraries, native_assets=native)


def _command(ctx, label):
    rows = [row for row in ctx.receipt.get("commands", []) if row.get("label") == label]
    _require(len(rows) == 1, "Missing/ambiguous current-run command evidence: " + label)
    row = rows[0]
    _require(row.get("status") == "passed" and row.get("exit_code") == 0, "Failed/incomplete command: " + label)
    return row


def _property(command, name):
    prefix = "-p:" + name + "="
    values = [str(arg)[len(prefix):] for arg in command if str(arg).startswith(prefix)]
    _require(len(values) <= 1, "Ambiguous MSBuild property: " + name)
    return values[0] if values else None


def _binary(ctx, path, label, policy):
    _require(path.is_file(), "Missing published AOT executable: " + str(path))
    with path.open("rb") as stream:
        _require(stream.read(4) == b"\x7fELF", "Current qualification audit requires a linux ELF AOT executable")
    data = ctx.run(["readelf", "-dW", path], label + "-needed")
    needed = re.findall(r"\(NEEDED\).*?\[([^\]]+)\]", data)
    for name in needed:
        _require(any(re.fullmatch(pattern, name) for pattern in policy["elfNeededPatterns"]), "Unreviewed AOT native dependency: " + name)
    symbols = ctx.run(["readelf", "--dyn-syms", "--wide", path], label + "-symbols")
    undefined = []
    for line in symbols.splitlines():
        parts = line.split()
        if len(parts) >= 8 and parts[6] == "UND":
            name = parts[7]
            _require(not re.search(r"zydis|zycore|^Zyan", name, re.I), "Unresolved native product symbol: " + name)
            _require("@GLIBC" in name or "@GCC_" in name or "@GLIBCXX_" in name or name.startswith(("_ITM_", "__gmon_start__", "__cxa_")),
                     "Unreviewed unresolved AOT import: " + name)
            undefined.append(name)
    return dict(path=str(path), sha256=_hash(path), needed=needed, undefined_platform_symbols=undefined)


def _rejection_controls(ctx, native_report, inventory):
    expected = [row for row in inventory if row["kind"] == "info" and not Path(row["expected"]).read_bytes()]
    controls = native_report.get("original_rejection_controls", [])
    _require(expected and [row["id"] for row in controls] == [row["id"] for row in expected],
             "Missing/changed original CLI rejection control inventory")
    diagnostics, evidence = [], []
    for row in controls:
        label = "zydis-original-" + row["id"].replace("/", "-")
        commands = [c for c in ctx.receipt["commands"] if c.get("label") == label]
        _require(len(commands) == 1, "Missing/ambiguous original CLI rejection command: " + label)
        command = commands[0]
        status = row.get("exit_code")
        _require(isinstance(status, int) and 0 < status <= 255 and command.get("exit_code") == status and
                 command.get("expected_exit_code") == status and command.get("upstream_rejection_control") is True,
                 "Original CLI rejection status/control is missing or invalid: " + label)
        stdout = Path(command["stdout"])
        stderr = stdout.with_suffix(".stderr")
        error = stderr.read_bytes()
        _require(not stdout.read_bytes() and error and error == row["stderr"].encode("utf-8"),
                 "Original rejecting CLI snapshot/diagnostics differ: " + label)
        diagnostics.append(error)
        evidence.append(dict(id=row["id"], exit_code=status, stderr=str(stderr), stderr_sha256=_hash(stderr)))
    return b"".join(diagnostics), evidence


def _qualification_stderr(group, managed, native, rejection_diagnostics):
    stderr, native_stderr = managed.with_suffix(".stderr"), native.with_suffix(".stderr")
    _require(stderr.is_file() and native_stderr.is_file(), "Rooted AOT/native stderr is missing")
    errors, original_errors = stderr.read_bytes(), native_stderr.read_bytes()
    _require(errors == original_errors, "Rooted AOT/native stderr differs")
    expected = rejection_diagnostics if group == 2 else b""
    _require(original_errors == expected, "Unexpected rooted AOT/native diagnostics for group " + str(group))


def _evidence(ctx, form, product, policy, kind):
    _require(sys.platform == "linux" and ctx.options.rid == "linux-x64", "AOT audit currently requires linux-x64")
    result = {}
    labels = {"rooted_companion": f"qualification-{form}-aot-rooted", "ordinary_consumer": f"consumer-{form}-aot"}
    label = labels[kind]
    published = _command(ctx, label + "-publish")
    command = list(map(str, published["command"]))
    _require(command[:2] == ["dotnet", "publish"] and _property(command, "PublishAot") == "true",
             "Evidence is not an AOT publish: " + label)
    _require("-r" in command and command[command.index("-r") + 1] == ctx.options.rid, "AOT RID mismatch: " + label)
    _require("-o" in command, "Missing publish output: " + label)
    output = Path(command[command.index("-o") + 1]).resolve()
    root = kind == "rooted_companion"
    settings = policy["qualification"] if root else policy["consumer"]
    project = (ctx.root / settings["project"]).resolve()
    _require(str(project) in command, "Wrong AOT consumer project: " + label)
    translated = (Layout(ctx.root, policy["companion"], ctx.recipe.default_profile).project(ctx.profile, form).resolve()
                  if root else Path(product["project"]).resolve())
    _require(_property(command, "TranslatedProject") == str(translated), "AOT publish references wrong product: " + label)
    _require(translated.is_file(), "Missing AOT input project: " + str(translated))
    tree = ET.parse(project)
    refs = list(tree.iter("ProjectReference"))
    _require(len(refs) == 1 and refs[0].get("Include") == "$(TranslatedProject)", "Consumer must reference its distinct translated project")
    if root:
        validate_emission(translated.parent, policy["companion"])
        qualification = helper(ctx.root / "scripts/qualification.py")
        selected = helper(ctx.root / "scripts/campaign.py").native_configuration(ctx.root, ctx.sources or None)
        current_inputs = qualification._inputs(ctx, selected)
        manifest = translated.parent / "zydis-test-inputs.json"
        _require(manifest.is_file() and _json(manifest) == current_inputs,
                 "Qualification companion source/config/fixture identity differs from current inputs")
        _require(_property(command, settings["rootProperty"]) == "true", "Qualification AOT library was not rooted")
        roots = list(tree.iter("TrimmerRootAssembly"))
        _require(any(n.get("Include") == settings["rootAssembly"] for n in roots), "Rooted qualification assembly missing")
        groups = [group for group in tree.iter("ItemGroup") if any(n.get("Include") == settings["rootAssembly"] for n in group.findall("TrimmerRootAssembly"))]
        condition = "'$(" + settings["rootProperty"] + ")' == 'true'"
        _require(any(group.get("Condition") == condition or any(n.get("Condition") == condition for n in group.findall("TrimmerRootAssembly"))
                     for group in groups), "Qualification root condition differs from the enabled publish property")
        report_path = Path(ctx.receipt.get("zydis_qualification_reports", {}).get("rooted-aot", ctx.artifacts / settings["report"]))
        report = _json(report_path)
        _require(report.get("status") == "passed", "Rooted AOT qualification report is not passed")
        native_report = _json(report["native_report"])
        _require(native_report.get("status") == "passed" and native_report.get("inputs") == current_inputs,
                 "Rooted AOT native reference is incomplete/stale for current inputs")
        rejection_diagnostics, rejection_evidence = _rejection_controls(
            ctx, native_report, helper(ctx.root / "tests/harness.py").inventory(selected["sources"]["zydis"]))
        cells = [row for row in report.get("cells", []) if row.get("form") == form and row.get("mode") == "aot"]
        _require(Counter(row.get("group") for row in cells) == Counter(settings["groups"]), "Incomplete rooted AOT original-C group inventory")
        for row in cells:
            _require(row.get("status") == "passed" and row.get("failed") == row.get("skipped") == 0 and row.get("passed", 0) > 0,
                     "Failed/skipped rooted AOT qualification group")
            native, managed = Path(row["native"]), Path(row["managed"])
            _require(native.read_bytes() == managed.read_bytes() and row.get("comparison") == "exact transcript bytes", "Rooted AOT/native transcript mismatch")
            ran = _command(ctx, managed.stem)
            _require(str(output / "Zydis.Qualification") == str(ran["command"][0]), "Qualification transcript ran a different binary")
            _require(Path(ran["stdout"]).resolve() == managed.resolve(), "Rooted report is not bound to current command output")
            _qualification_stderr(row["group"], managed, native, rejection_diagnostics)
        control_rows = [row for row in ctx.receipt["commands"] if row.get("label") == label + "-failure-control"]
        _require(len(control_rows) == 1 and control_rows[0].get("exit_code") == 1 and control_rows[0].get("deliberate_failure_control") is True,
                 "Missing rooted AOT assertion exit/failure control")
        transcript = Path(control_rows[0]["stdout"]).read_text()
        _require("CHECK FAILED" in transcript and "deliberate failure control" in transcript, "Rooted AOT failure control is incomplete")
        result["rooted_report"] = dict(path=str(report_path), sha256=_hash(report_path))
        result["original_rejection_controls"] = rejection_evidence
    else:
        trimmed = _property(command, "PublishTrimmed") or tree.findtext(".//PublishTrimmed")
        _require(trimmed == "true", "Ordinary AOT API consumer must explicitly enable PublishTrimmed=true")
        _require(_property(command, "RootQualificationLibrary") != "true" and not list(tree.iter("TrimmerRootAssembly")),
                 "Ordinary API consumer must not root a qualification/product library")
        _require(not list(tree.iter("TrimmerRootDescriptor")), "Ordinary consumer has an external rooting descriptor")
        ran = _command(ctx, label + "-run")
        assembly = tree.findtext(".//AssemblyName") or project.stem
        _require(Path(ran["command"][0]).resolve() == output / assembly, "Ordinary consumer ran a different output")
        _require(policy["consumer"]["successMarker"] in Path(ran["stdout"]).read_text(), "Ordinary consumer workflow did not complete")
        stderr = Path(ran["stdout"]).with_suffix(".stderr")
        _require(stderr.is_file() and not stderr.read_bytes(), "Ordinary API consumer stderr is missing/nonempty")
    inputs = [Path(row["path"]) for row in product["files"]]
    inputs += [project, *[p for p in project.parent.rglob("*.cs") if not {"bin", "obj"}.intersection(p.relative_to(project.parent).parts)]]
    if root:
        inputs += [p for p in source_files(translated.parent) if p.suffix in (".cs", ".csproj", ".json")]
    _require(published.get("started") is not None and all(p.stat().st_mtime <= published["started"] for p in inputs),
             "Audited source/project changed after AOT publish: " + label)
    assembly = tree.findtext(".//AssemblyName") or project.stem
    result[kind] = dict(publish_label=label + "-publish", source_binding="explicit project paths and no newer source mtimes", binary=_binary(ctx, output / assembly, "audit-" + label, policy))
    deps = output / (assembly + ".deps.json")
    result[kind]["dependency_manifest"] = (_dependencies(deps, {assembly, policy["companion"] if root else policy["product"]})
                                                   if deps.is_file() else {"absent": "AOT output; project references, native symbols and NEEDED dependencies audited"})
    result[kind]["status"] = "passed"
    if "rooted_report" in result:
        result[kind]["report"] = result["rooted_report"]
        result[kind]["original_rejection_controls"] = result["original_rejection_controls"]
    return result[kind]


def _audit(ctx, projects, *, preliminary=False, emission_receipt=None):
    destination = ctx.artifacts / "zydis-audit.json"
    report = dict(schema_version=1, status="running", qualified=False, delivered=False,
                  scope="preliminary private emitted source closure; not delivered/qualified" if preliminary else "final raw+processed product closure and distinct AOT evidence",
                  policy=str(ctx.root / "config/audit-policy.json"), products={}, failures=[])
    def check(name, action):
        try:
            return action()
        except Exception as error:
            report["failures"].append(dict(check=name, error=f"{type(error).__name__}: {error}"))
            return None
    try:
        policy = _json(ctx.root / "config/audit-policy.json")
        report["policy_sha256"] = _hash(ctx.root / "config/audit-policy.json")
        inventory = check("source-inventory", lambda: _inventory(ctx, policy))
        if inventory:
            selected, exported, types, report["source_inventory"] = inventory
            receipt = _json(emission_receipt) if emission_receipt else ctx.receipt
            if receipt.get("units") is not None:
                expected = [str(p) for p in selected["units"]]
                check("emission-unit-inventory", lambda: _require(receipt.get("units") == expected, "Recorded emitted units differ from full current selection"))
                report["emission_units"] = receipt["units"]
            report["emission_receipt"] = str(emission_receipt) if emission_receipt else "current context; optional historical receipts are not required in warn/off modes"
            for form, project in projects.items():
                project = Path(project).resolve()
                row = report["products"][form] = {"project": str(project), "status": "failed"}
                closure = check(form + ":project", lambda: _project(ctx, project, selected, policy))
                if closure:
                    generated, authored, details = closure
                    row.update(details)
                    symbols = check(form + ":symbols", lambda: _symbols(generated, authored, exported, types, policy))
                    if symbols is not None:
                        row.update(symbols=symbols, status="passed", static_status="passed")
                    deps = project.parent / "bin/Release/net10.0" / (policy["product"] + ".deps.json")
                    if deps.is_file():
                        row["managed_dependencies"] = check(form + ":dependencies", lambda: _dependencies(deps, {policy["product"]}))
                    if not preliminary:
                        row["qualification"] = {}
                        for kind in ("rooted_companion", "ordinary_consumer"):
                            evidence = check(form + ":" + kind, lambda: _evidence(ctx, form, details, policy, kind))
                            row["qualification"][kind] = evidence or {"status": "failed", "reason": "Evidence unavailable/invalid; see failures"}
                            if evidence is None:
                                row["status"] = "failed"
        check("delivery-forms", lambda: _require(not preliminary and set(projects) == set(policy["requiredForms"]),
                                                 "Both delivered raw and processed forms are required; private emission is preliminary only"))
        if preliminary:
            report["failures"].append(dict(check="qualification-evidence", error="No delivered products, rooted AOT companion or ordinary trimmed consumer execution was supplied; qualification is unavailable and fails."))
        report["status"] = "failed" if report["failures"] else "passed"
        report["qualified"] = report["delivered"] = report["status"] == "passed" and not preliminary
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        write_json(destination, report)
        ctx.receipt["zydis_audit"] = str(destination)
        ctx.save()
    return report


def run(ctx, suite):
    projects, errors = {}, []
    for form in ("raw", "processed"):
        try:
            projects[form] = check_product(ctx, form)
        except Exception as error:
            errors.append(dict(check=form + ":delivery", error=str(error)))
    report = _audit(ctx, projects)
    if errors:
        report["failures"].extend(errors)
        report.update(status="failed", qualified=False, delivered=False)
        write_json(ctx.artifacts / "zydis-audit.json", report)
    if report["status"] != "passed":
        raise RuntimeError("Zydis delivery audit failed: " + str(ctx.artifacts / "zydis-audit.json"))
    return report


def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("--preliminary-project", required=True, type=Path)
    parser.add_argument("--emission-receipt", type=Path)
    parser.add_argument("--artifacts", required=True, type=Path)
    parser.add_argument("--profile", choices=("default", "aggregate-data"), default="default")
    args = parser.parse_args()
    args.artifacts.mkdir(parents=True, exist_ok=True)
    ctx = SimpleNamespace(root=ROOT, sources={}, artifacts=args.artifacts.resolve(), profile=args.profile,
                          receipt={"status": "preliminary", "scope": "private source audit only; no builds or delivery"})
    ctx.save = lambda: write_json(ctx.artifacts / "receipt.json", ctx.receipt)
    report = _audit(ctx, {"raw": args.preliminary_project}, preliminary=True, emission_receipt=args.emission_receipt)
    print(json.dumps(dict(report=str(ctx.artifacts / "zydis-audit.json"), status=report["status"],
                          static_products={name: row["status"] for name, row in report["products"].items()},
                          qualified=report["qualified"], failures=report["failures"]), indent=2))
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
