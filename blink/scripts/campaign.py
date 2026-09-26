"""Blink profile derivation and semantic validation, using shared delivery."""
import os
from pathlib import Path
import re
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Consumer, Recipe, Source, Suite
from campaigns.recipes import helper, json_file, link_flags
from campaigns.translation import deliver


def sources(root):
    pin = json_file(root / "config/source-manifest.json")["upstream"]
    yield Source("product", pin["url"], root / "ref" / pin["archive"], root / "ref" / pin["directory"],
                 pin["directory"], pin["sha256"], ("blink/machine.h",))


def translate(ctx):
    root = ctx.root
    ctx.script("native-oracle.sh", "--offline", timeout=1800)
    native = json_file(root / "artifacts/native/receipt.json")
    if not native["tests"] or not all(row["pass"] for row in native["tests"]):
        raise RuntimeError("Native Blink tests failed")
    ctx.receipt["native"] = native
    staged = ctx.script("probe-core.sh", "--stage-only", label="stage", timeout=300)
    profile = Path(staged.strip().splitlines()[-1]).resolve()
    if not profile.is_relative_to(root / "generated/core-profile"):
        raise RuntimeError("Unexpected staged profile path")
    if ctx.profile == "threaded":
        threaded = root / "generated/threaded-core" / ctx.run_id
        boundary = ctx.artifacts / "threaded-stage.json"
        ctx.script("stage-threaded-core.py", "--base-profile", profile, "--output", threaded,
                   "--receipt", boundary, "--mremap-validation", "--empty-epoll", "--instance-methods",
                   label="threaded-stage", timeout=300)
        derivation = json_file(boundary)
        if not derivation.get("staged") or derivation.get("profile") != str(threaded):
            raise RuntimeError("Threaded profile derivation failed")
        profile = threaded
        ctx.receipt["threaded_derivation"] = str(boundary)
    inputs = json_file(profile / "inputs.json")
    core = helper(root / "scripts/core_inputs.py")
    entries = core.profile_sources(profile, root, inputs)
    options = ["--profile", profile, "--jobs", str(min(4, ctx.options.jobs)), "--timeout", str(ctx.options.timeout)]
    if ctx.options.action == "probe":
        options += ["--emit-only"]
        if ctx.options.unit:
            options += ["--sources", *ctx.options.unit]
    output = ctx.script("assemble-core.py", *options, label="assemble", timeout=14400)
    if ctx.options.action == "probe":
        ctx.receipt["scope"] = "Blink object probing; no product published"
        return
    assembly_path = Path(output.strip().splitlines()[-1].split(": ", 1)[1])
    assembly = json_file(assembly_path)
    expected = [entry["path"] for entry in entries]
    if (not assembly["linked"] or assembly["failures"] or assembly.get("diagnostic_replay")
            or assembly["selected"] != expected or set(assembly["objects"]) != set(expected)):
        raise RuntimeError("Incomplete or diagnostic-only Blink closure")
    for name in ("managed_boundaries", "semantic_intrinsics"):
        if (profile / (name.replace("_", "-") + ".json")).exists() and not assembly.get(name):
            raise RuntimeError("Missing reviewed Blink evidence: " + name)
        ctx.receipt[name] = assembly.get(name)
    if any(name in expected for name in ("authored/GuestExecution.c", "authored/managed-driver.c")):
        raise RuntimeError("Test execution frontend leaked into product")
    objects = [Path(assembly["objects"][name]["object_path"]) for name in expected]
    ctx.receipt["assembly"] = str(assembly_path)
    ctx.receipt["objects"] = assembly["objects"]
    linked = ctx.work / "linked"
    ctx.run(["dotnet", ctx.compiler, *objects, "--emit=managedlib",
             *link_flags("BlinkCore", "Managed.Emulation"), "--deduplicate-inline",
             *(["--instance-methods"] if ctx.profile == "threaded" else []), "-o", linked], "delivery-link")
    raw = ctx.work / "raw/TranslatedBlink"
    (raw / "Sources").mkdir(parents=True)
    (raw / "Bridges").mkdir()
    sources = sorted(linked.glob("*.cs"))
    if not sources or any(re.search(r"\bCoreProbe\s*\(", path.read_text()) for path in sources):
        raise RuntimeError("Missing product sources or leaked probe entrypoint")
    for path in sources:
        shutil.copy2(path, raw / "Sources" / path.name)
    authored = json_file(profile / "host-bindings.json")["managedSources"]
    if len({Path(name).name for name in authored}) != len(authored):
        raise RuntimeError("Authored bridge basename collision")
    for name in json_file(profile / "binding-sources.json")["authored_managed"]:
        shutil.copy2(profile / name, raw / "Bridges" / Path(name).name)
    shutil.copytree(profile / "host-project", raw / "Host", ignore=shutil.ignore_patterns("bin", "obj"))

    def project(path, private):
        tree = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
        props = ET.SubElement(tree, "PropertyGroup")
        values = dict(TargetFramework="net10.0", LangVersion="14", OutputType="Library",
                      AssemblyName="TranslatedBlink", RootNamespace="Managed.Emulation",
                      AllowUnsafeBlocks="true", Nullable="disable", ImplicitUsings="enable",
                      EnableDefaultCompileItems="false", WarningsAsErrors="$(WarningsAsErrors);CS8500",
                      DefineConstants="$(DefineConstants);BLINK_FULL_CORE" + (";DOTCC_INSTANCE_FOR_HOST" if ctx.profile == "threaded" else ""))
        for key, value in values.items():
            ET.SubElement(props, key).text = value
        items = ET.SubElement(tree, "ItemGroup")
        ET.SubElement(items, "Compile", Include="Sources/**/*.cs")
        if private:
            ET.SubElement(items, "Compile", Include="Bridges/*.cs")
            ET.SubElement(items, "ProjectReference", Include="Host/Managed.Emulation.Host.csproj")
        else:
            for name in authored:
                ET.SubElement(items, "Compile", Include=os.path.relpath(root / name, path.parent), Link="Bindings/" + Path(name).name)
            ET.SubElement(items, "ProjectReference", Include=os.path.relpath(root / "src/Managed.Emulation.Host/Managed.Emulation.Host.csproj", path.parent))
        ET.indent(tree, space="  ")
        ET.ElementTree(tree).write(path, encoding="unicode")

    def finish(ctx, path, form):
        for name in ("Host", "Bridges"):
            shutil.rmtree(path.parent / name, ignore_errors=True)
        project(path, False)

    project(raw / "TranslatedBlink.csproj", True)
    deliver(ctx, raw, finish=finish)


recipe = Recipe("blink", "TranslatedBlink", ("threaded", "single-thread"),
                ("src/Managed.Emulation.Host/Managed.Emulation.Host.csproj",
                 "src/Managed.Emulation.ThreadedExecution/Managed.Emulation.ThreadedExecution.csproj",
                 "src/Managed.Emulation/Managed.Emulation.csproj",
                 "src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj",
                 "samples/ManagedConsumer/ManagedConsumer.csproj"), sources, translate=translate,
                suites=(Suite("consumer", ("consumer",), matrix=True, profiles=("threaded",)),
                        Suite("source-inputs", ("script", "test-source-inputs.py")),
                        Suite("host-files", ("script", "test-host-files.sh")),
                        Suite("host-sockets", ("script", "test-host-sockets.sh")),
                        Suite("instance-io", ("script", "test-instance-io.sh")),
                        *(Suite(name, ("script", "test-" + name + ".py"), 14400, platforms=("linux",),
                                scope="opt-in specialist regression; prepares its own test artifacts")
                          for name in ("sqlite-regression", "picotls-regression", "msquic-regression",
                                       "language-regressions", "wat-regression", "zig-regression")),
                        *(Suite(name, ("python", "{root}/tests/" + directory + "/run.py"), 7200,
                                platforms=("linux",), profiles=("threaded",))
                          for name, directory in (("machine-api", "MachineApi"),
                                                  ("consumer-delivery", "ManagedConsumerDelivery"),
                                                  ("guest-threads", "GuestThreads")))),
                legacy_owned=("Sources/*.cs",), consumer=Consumer(property="BlinkProject", arguments=("--help",)),
                default_suites=("consumer",), verify_suites=("source-inputs", "host-files", "host-sockets", "instance-io", "consumer"))
