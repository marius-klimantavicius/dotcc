"""Bounded scalar source-delivery audit, independent of historical pin hashes.

This recognizes the project's emitted C# and simple SDK library project shape.
It is not a general C#/MSBuild dependency resolver or a runtime reachability/ABI
proof. Unknown build inputs fail closed; shared OS-service imports are inventoried.
"""
from collections import Counter, defaultdict
import json
import os
from pathlib import Path
import re
import xml.etree.ElementTree as ET


DIAGNOSTICS = re.compile(r"\bdotcc_(?:oracle_main|boundary_main|abi_main|upstream_(?:run|count|io\w*|host\w*))\b")
BINARY_SUFFIXES = {".so", ".a", ".dylib", ".lib", ".exe", ".dll", ".o", ".obj"}
# Lexical masking preserves offsets. Only the ordinary/verbatim strings emitted
# by this route are supported; this deliberately does not evaluate preprocessor branches.
LEXEMES = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
METHOD = re.compile(
    r"^[ \t]*(?P<mods>(?:(?:public|private|internal|protected|static|unsafe|extern|partial|abstract|virtual|override|sealed|new|async|readonly)\s+)+)"
    r"[\w.@:<>,?\[\]*]+\s+(?P<name>@?\w+)(?:<[^\n{};()]+>)?\s*\(", re.M)
DECLARATION = re.compile(METHOD.pattern.removeprefix("^"))
TYPE = re.compile(r"\b(?:class|struct|interface)\s+(\w+)[^;{}]*\{")
IMPORT = re.compile(r"\b(?:DllImport|LibraryImport)(?:Attribute)?\s*\(")
DYNAMIC = re.compile(r"\b(?:NativeLibrary\s*\.\s*\w+|NativeImports\s*\.\s*\w+|(?:Assembly|AssemblyLoadContext)\s*\.\s*(?:Load\w*)|(?:dlopen|dlsym|LoadLibrary|LoadLibraryA|LoadLibraryW|GetProcAddress))\s*\(")
LOADER_NAMES = {"dlopen", "dlsym", "LoadLibrary", "LoadLibraryA", "LoadLibraryW", "GetProcAddress"}
MODULE_DEPS = {
    "root": {"build_options", "terminal_options", "unicode_tables", "symbols_tables", "uucode"},
    "build_options": set(), "terminal_options": set(), "unicode_tables": set(), "symbols_tables": set(),
    "uucode": {"types.zig", "config.zig", "tables"}, "types.zig": set(),
    "config.zig": {"types.zig", "storage.zig"},
    "tables": {"config.zig", "storage.zig", "build_config"},
    "storage.zig": {"config.zig"}, "build_config": {"config.zig", "storage.zig"},
}
LIMITS = [
    "Static audit of current scalar source/project inputs; no runtime, physical ABI or rooted-AOT claim.",
    "Simple Microsoft.NET.Sdk default Compile globs only; explicit imports/items outside this shape are rejected.",
    "SDK/framework implicit dependencies and environment/CLI MSBuild overrides are not resolved here.",
    "Known loader APIs and current runtime bodies are checked; arbitrary reflection and C# dynamic data flow are not analyzed.",
    "Object imports include unused inline/header candidates; selected means a lexical reference outside runtime types, not runtime reachability.",
    "Module argv and selected options/accessor are checked; conditional Zig imports are not independently resolved.",
]


def _read(path):
    return Path(path).read_text(encoding="utf-8")


def _json(path):
    return json.loads(_read(path))


def _require(condition, message):
    if not condition:
        raise RuntimeError(message)


def _mask(text, strings=True):
    return LEXEMES.sub(lambda m: re.sub(r"[^\n]", " ", m[0])
                       if strings or m[0].startswith(("//", "/*")) else m[0], text)


def _tokens(text):
    # Preserve string contents (including whitespace) when comparing loader bodies.
    return re.findall(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\w+|[^\s]', _mask(text, False))


def _close(code, start, left="{", right="}"):
    depth = 0
    for match in re.compile(re.escape(left) + "|" + re.escape(right)).finditer(code, start):
        depth += 1 if match[0] == left else -1
        if depth == 0:
            return match.start()
    raise RuntimeError("Unbalanced emitted source delimiter")


def methods(text, path="<source>"):
    """Recognize emitted ordinary method declarations, requiring an actual body."""
    code = _mask(text)
    scopes = [(m.start(), _close(code, m.end() - 1), m[1]) for m in TYPE.finditer(code)]
    active, cursor, result = [], 0, []
    for match in METHOD.finditer(code):
        while cursor < len(scopes) and scopes[cursor][0] < match.start():
            scope = scopes[cursor]
            while active and active[-1][1] < scope[0]:
                active.pop()
            active.append(scope)
            cursor += 1
        while active and active[-1][1] < match.start():
            active.pop()
        end_params = _close(code, match.end() - 1, "(", ")")
        tail = end_params + 1
        while tail < len(code) and code[tail].isspace():
            tail += 1
        if code[tail:tail + 1] == "{":
            end = _close(code, tail) + 1
        elif code[tail:tail + 2] == "=>":
            end = code.find(";", tail) + 1
            _require(end > tail, "Unterminated expression body")
        else:
            continue  # extern/abstract/partial declarations are not public bodies.
        result.append(dict(name=match["name"].lstrip("@"), mods=match["mods"].split(),
                           owner=".".join(s[2] for s in active), start=match.start(), end=end,
                           signature=text[match.start():end_params + 1],
                           body=text[tail:end], code=code[tail:end], path=str(path)))
    return result


def _exports(records, required):
    found = defaultdict(list)
    for record in records:
        if (record["name"] in required and record["owner"] == "GhosttyVt"
                and {"public", "static"} <= set(record["mods"])
                and not {"extern", "abstract"} & set(record["mods"])):
            found[record["name"]].append(record)
    bad = {name: len(found[name]) for name in sorted(required) if len(found[name]) != 1}
    _require(not bad, "Required exports need exactly one actual public GhosttyVt body: " + str(bad))
    return {name: dict(file=found[name][0]["path"], signature=found[name][0]["signature"].strip())
            for name in sorted(required)}


def _files(directory):
    """SDK default source glob: root bin/obj caches and dot directories excluded."""
    for parent, dirs, files in os.walk(directory, followlinks=False):
        dirs[:] = sorted(d for d in dirs if not d.startswith(".")
                         and not (Path(parent) == directory and d in ("bin", "obj")))
        for name in dirs:
            _require(not (Path(parent) / name).is_symlink(), "Symlink in delivered source tree")
        for name in sorted(files):
            path = Path(parent) / name
            _require(not path.is_symlink(), "Symlink in delivered source tree: " + str(path))
            yield path


def owned_sources(project):
    directory = project.parent
    names = _json(directory / ".campaign-files.json")
    _require(isinstance(names, list) and len(names) == len(set(names)), "Invalid generated-file ownership manifest")
    for name in names:
        path = directory / name
        _require(not Path(name).is_absolute() and ".." not in Path(name).parts
                 and path.resolve().is_relative_to(directory.resolve()) and path.is_file(),
                 "Unsafe/missing owned source: " + str(name))
    actual_files = list(_files(directory))
    binaries = [str(p.relative_to(directory)) for p in actual_files
                if p.suffix.lower() in BINARY_SUFFIXES or re.search(r"\.so\.\d", p.name)]
    _require(not binaries, "Native/binary artifacts in source delivery: " + str(binaries))
    actual = {p.relative_to(directory).as_posix() for p in actual_files if p.suffix == ".cs"}
    owned = {name for name in names if name.endswith(".cs")}
    _require(actual == owned, "Actual recursive C# source set differs from ownership manifest: "
             + str(dict(unowned=sorted(actual - owned), missing=sorted(owned - actual))))
    emitted = _read(directory / "Dotcc.SourceFiles.txt").splitlines()
    _require(len(emitted) == len(set(emitted)) and set(emitted) == actual,
             "Emitter source list differs from actual C# source set")
    return {name: _read(directory / name) for name in sorted(actual)}


def project_inputs(project):
    """Accept the current small project dialect; do not pretend to evaluate MSBuild."""
    roots = [(project, ET.parse(project).getroot())]
    _require(roots[0][1].attrib == {"Sdk": "Microsoft.NET.Sdk"}, "Unexpected project SDK/attributes")
    # Inspect all candidates, even ones masked by a nearer Directory.Build file.
    for parent in project.parents:
        for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props"):
            path = parent / name
            if path.is_file():
                roots.append((path, ET.parse(path).getroot()))
        _require(not (parent / "MSBuild.rsp").exists() and not (parent / "Directory.Build.rsp").exists(),
                 "Unexpected automatic MSBuild response file")
    safe_properties = {"OutputType", "TargetFramework", "RootNamespace", "AssemblyName", "AllowUnsafeBlocks",
                       "Nullable", "LangVersion", "ImplicitUsings", "TreatWarningsAsErrors", "Deterministic",
                       "ContinuousIntegrationBuild", "DebugType", "UseLocalLalrCc", "ManagePackageVersionsCentrally"}
    inventory = []
    for path, root in roots:
        _require(root.tag == "Project" and (path == project or not root.attrib), "Unsupported project XML: " + str(path))
        for group in root:
            _require(group.tag in ("PropertyGroup", "ItemGroup"), "Unexpected build/dependency input in " + str(path) + ": " + group.tag)
            _require(set(group.attrib) <= {"Condition"} and (path != project or not group.attrib),
                     "Unsupported conditional build input in " + str(path))
            for item in group:
                inventory.append(dict(file=str(path), kind=item.tag, attributes=dict(item.attrib), value=item.text))
                if group.tag == "PropertyGroup":
                    _require(item.tag in safe_properties and not list(item), "Unexpected project property: " + item.tag)
                    _require(set(item.attrib) <= {"Condition"} and not re.search(r"[$@%]\(", item.text or ""),
                             "Unsupported evaluated project property: " + item.tag)
                    if item.tag == "OutputType":
                        _require(item.text == "Library", "Delivered project must be a library")
                elif path.name == "Directory.Packages.props" and item.tag == "PackageVersion":
                    _require(set(item.attrib) == {"Include", "Version"} and not list(item),
                             "Unexpected central package input")
                else:
                    _require(path == project and item.tag == "None" and not list(item)
                             and item.attrib == {"Update": "THIRD-PARTY-NOTICES",
                                 "TargetPath": "GhosttyVt.THIRD-PARTY-NOTICES",
                                 "CopyToOutputDirectory": "PreserveNewest", "CopyToPublishDirectory": "PreserveNewest"},
                             "Unexpected dependency/Compile/copy item: " + item.tag + " in " + str(path))
        if path == project:
            _require(root.findtext("./PropertyGroup/OutputType") == "Library", "Missing Library output type")
            notices = root.findall("./ItemGroup/None")
            _require(len(notices) == 1 and all(not g.attrib for g in root.findall("ItemGroup")),
                     "Missing/unconditional build and publish notice copy configuration")
    # Existing restore metadata is corroborating evidence, never an alternate resolver.
    assets = project.parent / "obj/project.assets.json"
    if assets.is_file():
        data = _json(assets)
        _require(not data.get("libraries") and not any(data.get("targets", {}).values()),
                 "Unexpected resolved project/package/assembly dependency")
    return dict(inputs=inventory, project_references=[], package_references=[], assembly_references=[],
                native_references=[], existing_assets=str(assets) if assets.is_file() else None)


def notices(project, pins, sources, repo):
    chunks, inventory = [], []
    for item in pins["license_inputs"]:
        if not item.get("distribute"):
            continue
        if "repository_path" in item:
            path = Path(repo) / item["repository_path"]
        else:
            key = "zig" if "toolchain_path" in item else item.get("source", "product")
            path = Path(sources[key]) / item.get("toolchain_path", item.get("path", ""))
        body = _read(path)
        _require(bool(body.strip()), "Empty declared license body: " + item["name"])
        chunks.append(item["name"] + "\n" + body)
        inventory.append(dict(name=item["name"], source=str(path), characters=len(body)))
    _require(bool(chunks), "No distributable license bodies")
    _require(_read(project.parent / "THIRD-PARTY-NOTICES") == "\n\n".join(chunks) + "\n",
             "Delivered full notice bodies differ from declared source texts")
    return inventory


def native_imports(text, path):
    code = _mask(text)
    results = []
    for match in IMPORT.finditer(code):
        end = _close(code, match.end() - 1, "(", ")")
        args = text[match.end():end]
        library = re.match(r'\s*"([^"\\]+)"\s*(?:,|$)', args)
        _require(library is not None, "Nonliteral/unsupported native import library in " + str(path))
        start = end + 1
        while start < len(code) and (code[start].isspace() or code[start] == "]"):
            start += 1
        declaration = DECLARATION.match(code, start)
        _require(declaration is not None and "extern" in declaration["mods"].split(),
                 "Unsupported native import declaration in " + str(path))
        entry = re.search(r'\bEntryPoint\s*=\s*"([^"\\]+)"', args)
        name = declaration["name"].lstrip("@")
        end_declaration = _close(code, declaration.end() - 1, "(", ")")
        results.append(dict(library=library[1], entrypoint=entry[1] if entry else name, method=name,
                            attribute=" ".join(_tokens(text[match.start():end + 1])),
                            signature=" ".join(_tokens(text[start:end_declaration + 1])), file=str(path)))
    return results


def runtime_inventory(repo):
    # This is the C-profile resource selection in Compiler.Resources.LoadRuntimeBlock(false).
    paths = sorted(p for p in (repo / "DotCC.Libc").glob("*.cs")
                   if not p.name.startswith("Zig") and p.name != "Slice.cs")
    _require(bool(paths), "Missing current authored C runtime")
    sources = {str(p): _read(p) for p in paths}
    records = [record for path, text in sources.items() for record in methods(text, path)]
    imports = [record for path, text in sources.items() for record in native_imports(text, path)]
    return records, imports


def host_boundary(sources, records, runtime_records, runtime_imports, allowed):
    _require(isinstance(allowed, list) and allowed and len(allowed) == len(set(allowed))
             and all(isinstance(x, str) and x for x in allowed), "source.json needs explicit allowed_host_libraries")
    imports = [record for path, text in sources.items() for record in native_imports(text, path)]
    for item in imports + runtime_imports:
        _require(item["library"] in allowed and not re.search(r"ghostty|harfbuzz|freetype|oniguruma|simdutf", item["entrypoint"], re.I),
                 "Unexpected native host library/provider import: " + str(item))
    key = lambda row: (row["library"], row["entrypoint"], row["method"], row["attribute"], row["signature"])
    _require(Counter(map(key, imports)) == Counter(map(key, runtime_imports)),
             "Delivered native imports differ from current authored runtime inventory")
    by_name = defaultdict(list)
    for item in runtime_records:
        by_name[item["name"]].append(item)
    dynamic_sites = []
    for path, text in sources.items():
        code = _mask(text)
        file_records = [r for r in records if r["path"] == str(path)]
        for match in DYNAMIC.finditer(code):
            enclosing = next((r for r in file_records if r["start"] <= match.start() < r["end"]), None)
            _require(enclosing is not None, "Dynamic loader outside recognized runtime method: " + str(path))
            candidates = by_name[enclosing["name"]]
            _require(any(enclosing["owner"].split(".")[-1] == c["owner"].split(".")[-1]
                         and _tokens(enclosing["signature"] + enclosing["body"]) == _tokens(c["signature"] + c["body"])
                         for c in candidates), "Changed/unattributed dynamic load closure: " + enclosing["name"])
            dynamic_sites.append(dict(file=str(path), owner=enclosing["owner"], method=enclosing["name"], expression=match[0]))
        _require(not re.search(r"\bDotCc(?:Static)?Imports\b", code), "Unexpected selected native import table")
    return dict(allowed_host_libraries=allowed, native_imports=imports,
                native_library_counts=dict(Counter(r["library"] for r in imports)),
                shared_runtime_dynamic_sites=dynamic_sites)


def _metadata(text, kind):
    return re.findall(r"^//!!dotcc-obj " + re.escape(kind) + r":([^\s]+)", text, re.M)


def attribute_imports(objects, records, runtime_records, source_texts=None):
    """Attribute candidates using object records and delivered method references."""
    definitions = defaultdict(list)
    runtime = defaultdict(list)
    for record in runtime_records:
        if "public" in record["mods"]:
            runtime[record["name"]].append(record)
    runtime_owners = {r["owner"].split(".")[-1] for r in runtime_records}
    translated = [r for r in records if r["owner"].split(".")[-1] not in runtime_owners]
    actual = defaultdict(list)
    for record in records:
        actual[record["name"]].append(record)
    referenced = set()
    for record in translated:
        referenced.update(re.findall(r"\b[A-Za-z_]\w*\b", record["code"]))
    if source_texts is not None:
        # Include initializers, address tables and helpers as well as method bodies.
        # Exclude only the current runtime's type blocks from this lexical selection.
        for text in source_texts.values():
            code = _mask(text)
            spans = [(m.end() - 1, _close(code, m.end() - 1)) for m in TYPE.finditer(code)
                     if m[1] in runtime_owners]
            start = 0
            for left, right in spans:
                if left >= start:
                    referenced.update(re.findall(r"\b[A-Za-z_]\w*\b", code[start:left]))
                    start = right + 1
            referenced.update(re.findall(r"\b[A-Za-z_]\w*\b", code[start:]))
    for path, text in objects.items():
        for name in _metadata(text, "function"):
            definitions[name].append(str(path))
    result = []
    for path, text in objects.items():
        for name, signature in re.findall(r"^//!!dotcc-obj import:([^\s]+) ([^\n]+)", text, re.M):
            selected = name in referenced
            providers = definitions[name]
            service = runtime[name]
            if selected:
                _require(name not in LOADER_NAMES, "Translated engine selected a dynamic loader: " + name)
                _require(bool(actual[name]) and bool(providers or service), "Unattributed selected C extern: " + name)
                if providers:
                    _require(any(r["owner"] == "GhosttyVt" and "static" in r["mods"] for r in actual[name]),
                             "Selected translated definition has no delivered owner body: " + name)
                if service and not providers:
                    _require(any(r["owner"].split(".")[-1] == "Libc" and
                                 any(_tokens(r["signature"] + r["body"]) == _tokens(s["signature"] + s["body"]) for s in service)
                                 for r in actual[name]), "Selected runtime body differs from current input: " + name)
            # Support-header compiler/math prototypes can remain in object import
            # metadata after their only inline users were pruned. Do not invent a
            # delivered runtime definition for an absent candidate.
            result.append(dict(object=str(path), symbol=name, object_signature=signature, selected_lexically=selected,
                               resolution=("translated definition" if providers else "current runtime body") if selected else "unselected object candidate",
                               translated_definitions=providers,
                               runtime_definitions=sorted({s["path"] for s in service}),
                               delivered_definitions=sorted({s["path"] for s in actual[name]})))
    return result


def _command(receipt, label):
    rows = [r for r in receipt["commands"] if r["label"] == label]
    _require(len(rows) == 1 and rows[0]["status"] == "passed", "Missing successful current command: " + label)
    return rows[0]


def _option(argv, name):
    _require(argv.count(name) == 1 and argv.index(name) + 1 < len(argv), "Missing/duplicate command option: " + name)
    return argv[argv.index(name) + 1]


def source_closure(root, receipt, pins, policy, required):
    # verify publishes its validated translation before running its suites; its
    # outer receipt is still running when this final audit is invoked.
    translation = [row for row in receipt.get("tasks", []) if row.get("name") == "translation"]
    completed = len(translation) == 1 and translation[0].get("status") == "passed"
    action, status = receipt.get("action"), receipt.get("status")
    admissible = (status == "passed" and action in ("translate", "verify")
                  or status == "running" and action == "verify")
    _require(completed and admissible and receipt.get("profile") == "scalar",
             "Delivery closure requires a completed successful scalar translation task")
    sources = {k: Path(v) for k, v in receipt["sources"].items()}
    product, zig = sources["product"], sources["zig"]
    selection = receipt["libghostty_selection"]
    engine = Path(selection["generated_c"])
    native = (root / receipt["staging"] / "native").resolve()
    _require(engine.resolve() == native / "engine.c", "Engine outside recorded private staging")
    closure = _json(native / "engine-closure.json")
    argv = closure["command"]
    emit = _command(receipt, "native-emit-whole-engine-c")
    _require(argv == emit["command"] == selection["engine_command"] == _json(native / "emit-c-command.json")
             and closure["source"] == str(engine) and closure["target"] == policy["c_target"]
             and _option(argv, "-target") == policy["c_target"] and "-ofmt=c" in argv
             and "-fcompiler-rt" in argv and "-fubsan-rt" in argv
             and argv[0] == str(zig / ("zig.exe" if os.name == "nt" else "zig"))
             and "-femit-bin=" + str(engine) in argv, "Current engine emission command/target differs from closure")
    _require(closure["generated_modules"] == [a for a in argv if a.startswith("-M")], "Module inventory differs from actual argv")
    _require(closure["includes"] == list(map(str, [zig / "lib", product / "include", native / "abi"])),
             "Unexpected engine include/provider root")
    build = _command(receipt, "native-upstream-build")
    _require(build["command"] == _json(native / "native-command.json") and build["cwd"] == str(product),
             "Upstream build command differs from current recorded inputs")
    _require(all(a in build["command"] for a in policy["native_build_arguments"]), "Changed scalar feature build arguments")
    expected_defines = set(policy["native_build_arguments"]) | {
        "-Dversion-string=" + pins["upstream_version"], "-Dlib-version-string=" + pins["library_version"]}
    _require({a for a in build["command"] if a.startswith("-D")} == expected_defines, "Unexpected upstream feature/provider override")
    if receipt["selection"]["fetch"] == "never":
        _require(_option(build["command"], "--system") == str(product / "zig-pkg")
                 and {a for a in build["command"] if a.startswith("-fno-sys=")}
                 == {"-fno-sys=" + name for name in pins["build_system_integrations"]}, "Offline provider policy changed")
    if receipt["hash_policy"] == "off":
        _require(Path(_option(argv, "--global-cache-dir")) == native / "zig-global-cache", "Off-mode cache is not private")
    # Check the precise upstream-to-C transformation, preserving actual module flags.
    original_command = _json(native / "native-engine-command.json")
    expected_command = ["-Mroot=" + str(native / "source" / policy["zig_root"])
                        if a.startswith("-Mroot=") else a for a in original_command
                        if not a.startswith("-femit-bin=") and a != "-fPIC"]
    root_index = next(i for i, a in enumerate(expected_command) if a.startswith("-Mroot="))
    expected_command[root_index:root_index] = ["-target", policy["c_target"]]
    expected_command += ["-ofmt=c", "-femit-bin=" + str(engine)]
    _require(argv == expected_command, "C emission differs from actual upstream module/flag closure")
    module_rows, deps, flags = {}, [], []
    arguments = iter(argv[2:])
    valued = {"-target", "--cache-dir", "--global-cache-dir", "--build-root", "--name", "--zig-lib-dir"}
    plain = {"-Ofast", "-Odebug", "-static", "-fcompiler-rt", "-fubsan-rt", "-ofmt=c"}
    for arg in arguments:
        if arg == "--dep":
            dep = next(arguments, None)
            _require(dep is not None, "Missing module dependency argument")
            deps.append(dep)
        elif arg.startswith("-M"):
            name, value = arg[2:].split("=", 1)
            _require(name not in module_rows, "Duplicate engine module: " + name)
            module_rows[name] = dict(path=str((product / value).resolve()), dependencies=deps, flags=flags)
            deps, flags = [], []
        elif arg in valued:
            value = next(arguments, None)
            _require(value is not None, "Missing engine flag argument")
            flags.extend([arg, value])
        else:
            _require(arg in plain or arg == "-femit-bin=" + str(engine),
                     "Unexpected engine flag/native provider: " + arg)
            flags.append(arg)
    _require(not deps, "Dangling engine module dependencies")
    _require(set(module_rows) == set(MODULE_DEPS), "Unexpected terminal engine module/provider set")
    for name, row in module_rows.items():
        _require(set(row["dependencies"]) == MODULE_DEPS[name] and len(row["dependencies"]) == len(set(row["dependencies"])),
                 "Changed module dependency edges: " + name)
        _require(Path(row["path"]).is_file(), "Missing current module input: " + row["path"])
    expected_paths = {"root": native / "source" / policy["zig_root"],
                      "build_config": product / "src/build/uucode_config.zig"}
    unicode_dep = [d for d in pins["dependencies"] if d["name"] == "uucode"]
    _require(len(unicode_dep) == 1 and "product" in unicode_dep[0]["role"], "Missing selected Unicode provider metadata")
    unicode_root = product / "zig-pkg" / unicode_dep[0]["zig_hash"] / "src"
    for name, filename in (("uucode", "root.zig"), ("types.zig", "types.zig"), ("config.zig", "config.zig"), ("storage.zig", "storage.zig")):
        expected_paths[name] = unicode_root / filename
    for name, path in expected_paths.items():
        _require(Path(module_rows[name]["path"]) == path.resolve(), "Unexpected module source/provider: " + name)
        module_rows[name]["role"] = "staged real root" if name == "root" else "current upstream Unicode input"
    for name, filename in (("build_options", "options.zig"), ("terminal_options", "options.zig"),
                           ("unicode_tables", "props.zig"), ("symbols_tables", "symbols.zig"), ("tables", "tables.zig")):
        path = Path(module_rows[name]["path"])
        _require(path.is_relative_to(native / "zig-cache/o") and path.name == filename, "Unexpected generated module provider: " + name)
        module_rows[name]["role"] = "current upstream generated input"
    options = _read(module_rows["terminal_options"]["path"])
    booleans = dict(re.findall(r"pub const (\w+): bool = (true|false);", options))
    enabled = {k for k, v in booleans.items() if v == "true"}
    _require(enabled == {"c_abi", *policy["features"]} and booleans.get("simd") == "false"
             and all(booleans.get({"tmux": "tmux_control_mode"}.get(k, k)) == "false" for k in policy["excluded_features"]),
             "Current terminal feature values differ from scalar policy")
    _require('artifact:' in options and '= .lib;' in options, "Not a terminal library artifact")
    original = _read(product / policy["zig_root"])
    addition = '\n// Authored ABI address accessor; upstream exports above are preserved.\ncomptime { _ = @import("dotcc_ghostty_api.zig"); }\n'
    _require(_read(expected_paths["root"]) == original + addition, "Staged root is not the real upstream root plus ABI accessor")
    operations = _json(root / "config/abi.json")["operations"]
    _require(len({r["id"] for r in operations}) == len(operations)
             and len({r["name"] for r in operations}) == len(operations)
             and all(r["name"] in required for r in operations), "Invalid ABI operation inventory")
    expected_accessor = ['const api = @import("terminal/c/main.zig");',
                         'export fn dotcc_ghostty_api(operation: c_int) ?*const anyopaque {',
                         '    return switch (operation) {']
    expected_accessor += [f'        {r["id"]} => @ptrCast(&api.{r["zig_member"]}),' for r in operations]
    expected_accessor += ['        else => null,', '    };', '}']
    accessor = _read(native / "source/src/dotcc_ghostty_api.zig")
    _require(accessor == _read(native / "abi/dotcc_ghostty_api.zig") == "\n".join(expected_accessor) + "\n",
             "ABI accessor body differs from current operation metadata")
    engine_text = _read(engine)
    _require(not any(s in engine_text for s in ("zig_mangled", "zig_e_zig_windows_teb", "zig_e_zig_windows_peb")),
             "Suppressed support alias returned to engine closure")
    exports = re.findall(r"^zig_extern\s+[^\n]*?\b(ghostty_\w+)\s*\(", engine_text, re.M)
    _require(Counter(exports) == Counter(required), "Actual engine public export set differs from required inventory")
    units = [str(engine), str(root / policy["bridge"])]
    _require(receipt["units"] == units, "Unexpected scalar translation source closure")
    objects = {}
    for index, unit in enumerate(units):
        label = f"{index:03d}-" + Path(unit).stem
        command = _command(receipt, label)["command"]
        path = Path(_option(command, "-o"))
        _require(unit in command and "--emit=obj" in command and path.parent == native.parent / "objects",
                 "Object emission is not attributed to current scalar input")
        expected_flags = ["-std=" + policy["dialect"]]
        if policy.get("aggregate_data", False):
            expected_flags.append("--aggregate-data")
        expected_flags += [part for include in closure["includes"] for part in ("-I", include)]
        expected_flags += (["--overrides-file", str(root / "config/zig-c-overrides.json")] if index == 0
                           else ["-DDOTCC_GHOSTTY_INDIRECT_ABI=1"])
        expected_flags += ["--override-report", _option(command, "--override-report"), unit, "--emit=obj", "-o", str(path)]
        _require(command[0] == "dotnet" and command[2:] == expected_flags,
                 "Unexpected object input/flag/include/provider closure: " + label)
        objects[str(path)] = _read(path)
    link = _command(receipt, "link")["command"]
    _require([a for a in link if a.endswith(".cs")] == list(objects)
             and "--runtime=c" in link and not any(a.startswith(("-l", "-L", "--library", "--native")) for a in link),
             "Unexpected managed link source/native provider closure")
    functions = _metadata(next(iter(objects.values())), "function")
    inlines = set(_metadata(next(iter(objects.values())), "inline"))
    _require(Counter(n for n in functions if n not in inlines) == Counter([*required, "dotcc_ghostty_api"]),
             "Engine object actual definitions differ from upstream exports/accessor")
    return dict(source=str(engine), receipt_profile="scalar", modules=module_rows, includes=closure["includes"],
                emission_command=argv, trailing_flags=flags, upstream_build_command=build["command"], terminal_boolean_options=booleans,
                units=units, source_metadata_roles=dict(selected_inputs=pins.get("selected_inputs", {}),
                    generated_inputs=pins.get("generated_inputs", []), excluded_system_integrations=pins["build_system_integrations"]),
                object_files=list(objects), providers=[dict(name="uucode", role=unicode_dep[0]["role"], source=str(unicode_root))]), objects, sources


def run(ctx, suite):
    # Keep the normal framework hook/receipt contract; no alternate campaign runner.
    from campaigns.testing import forms
    from campaigns.translation import check_product

    _require(ctx.profile == "scalar", "Delivery audit supports scalar products only")
    pins = _json(ctx.root / "config/source.json")
    policy = _json(ctx.root / "config/translation.json")
    required = _json(ctx.root / "config/api-inventory.json")["required_exports"]
    _require(len(required) == 93 and len(set(required)) == 93, "Required export inventory must contain 93 unique names")
    pointer_path = ctx.root / "artifacts/campaign/current-scalar.json"
    pointer = _json(pointer_path)
    receipt_path = Path(pointer["receipt"])
    receipt = _json(receipt_path)
    closure, objects, source_roots = source_closure(ctx.root, receipt, pins, policy, required)
    runtime_records, runtime_imports = runtime_inventory(ctx.repo)
    results = []
    for form in forms(ctx):
        project = check_product(ctx, form)
        _require(pointer["outputs"][form]["project"] == str(project)
                 and receipt["outputs"][form]["project"] == str(project)
                 and not pointer["outputs"][form].get("companions"), "Current pointer has a different delivered project/dependency")
        sources = owned_sources(project)
        records = [r for name, text in sources.items() for r in methods(text, name)]
        for name, text in sources.items():
            code = _mask(text)
            _require(re.findall(r"\bnamespace\s+([\w.]+)\s*;", code) == ["Managed"]
                     and not re.search(r"\bnamespace\s+[\w.]+\s*\{", code), "Unexpected delivered namespace shape: " + name)
            _require(not re.search(r"^\s*#\s*(?:if|elif|else|endif|define|undef)\b", code, re.M),
                     "Conditional compiled source is outside the bounded scalar audit: " + name)
            _require(not re.search(r"\busing\s+(?:static\s+|\w+\s*=)[^;]*(?:NativeLibrary|NativeImports|AssemblyLoadContext|Assembly)\b", code),
                     "Unexpected dynamic-loader alias in delivered source: " + name)
            _require(not DIAGNOSTICS.search(code), "Diagnostic entrypoint escaped into scalar source: " + name)
        exports = _exports(records, required)
        dependencies = project_inputs(project)
        licenses = notices(project, pins, source_roots, ctx.repo)
        host = host_boundary(sources, records, runtime_records, runtime_imports, pins.get("allowed_host_libraries"))
        attribution = attribute_imports(objects, records, runtime_records, sources)
        results.append(dict(form=form, project=str(project), source_files=sorted(sources), required_exports=len(required),
                            actual_upstream_exports=len(exports), exports=exports, test_entrypoints=0,
                            dependencies=dependencies, notices=licenses, host_boundary=host,
                            c_extern_attribution=attribution))
    _require(_json(pointer_path) == pointer, "Scalar product pointer changed during audit")
    path = ctx.artifacts / "delivery-audit.json"
    path.write_text(json.dumps(dict(results=results, translation_receipt=str(receipt_path),
                                   source_closure=closure, limits=LIMITS), indent=2) + "\n", encoding="utf-8")
    ctx.receipt["delivery_audit"] = str(path)
