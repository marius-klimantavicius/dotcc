"""Campaign qualification hooks; all disposable outputs follow Context.work."""
import os
from pathlib import Path
import sys
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def source(ctx):
    if "product" not in ctx.sources:
        from campaigns.inputs import acquire
        selected = next(item for item in ctx.recipe.sources(ctx.root) if item.name == "product")
        ctx.sources["product"] = acquire(selected, ctx.policy, ctx.options.fetch)
    return ctx.sources["product"]


def tools(ctx):
    if not hasattr(ctx, "compiler"):
        ctx.tools()


def qualification(ctx, suite):
    tools(ctx)
    upstream = source(ctx)
    native = Path(os.environ.get("CUI_NATIVE_BUILD", ctx.work / "native-control"))
    if not (native / "CMakeCache.txt").is_file():
        ctx.run(["cmake", "-S", upstream, "-B", native, "-DCMAKE_BUILD_TYPE=Release",
                 "-DCUI_PLATFORM=glfw", "-DCUI_WITH_APP=ON", "-DCUI_BUILD_TESTS=ON",
                 "-DCUI_BUILD_EXAMPLES=OFF", "-DCUI_DEFAULT_FONTS=ON", "-DCUI_STB_IMAGE=ON",
                 "-DCUI_IMAGE_ALL_FORMATS=ON", "-DCUI_TEXT_SHAPING=ON"], "native-configure")
        ctx.run(["cmake", "--build", native, "--parallel", "2"], "native-build", timeout=1200)
    ctx.receipt["native_control"] = str(native)
    arguments = [sys.executable, ctx.root / "scripts/qualify.py", "--suite", suite.name,
                 "--source", upstream, "--native-build", native,
                 "--work", ctx.work / ("qualification-" + suite.name), "--compiler", ctx.compiler,
                 "--postprocessor", ctx.postprocessor, "--jobs", str(min(2, ctx.options.jobs)),
                 "--form", ctx.options.form, "--mode", ctx.options.mode, "--rid", ctx.options.rid]
    font = os.environ.get("CUI_SHAPING_FONT") or os.environ.get("CUI_ARABIC_FONT")
    if not font:
        import json
        candidate = Path(json.loads((ctx.root / "tests/qualification/font.json").read_text())["example_path"])
        if candidate.is_file():
            font = str(candidate)
    if suite.name in ("native", "upstream") and not font:
        raise RuntimeError("Set CUI_SHAPING_FONT to the trusted Arabic font fixture; see tests/qualification/font.json")
    if font:
        arguments += ["--font", font]
    if (ctx.work / "objects").is_dir() and (ctx.work / "source").is_dir():
        arguments += ["--objects-dir", ctx.work / "objects", "--staged-source", ctx.work / "source"]
    elif os.environ.get("CUI_QUALIFICATION_OBJECTS") or os.environ.get("CUI_QUALIFICATION_STAGED_SOURCE"):
        if not (os.environ.get("CUI_QUALIFICATION_OBJECTS") and os.environ.get("CUI_QUALIFICATION_STAGED_SOURCE")):
            raise RuntimeError("Supply both CUI_QUALIFICATION_OBJECTS and CUI_QUALIFICATION_STAGED_SOURCE from the same final translation")
        arguments += ["--objects-dir", os.environ["CUI_QUALIFICATION_OBJECTS"],
                      "--staged-source", os.environ["CUI_QUALIFICATION_STAGED_SOURCE"]]
    ctx.run(arguments, suite.name, timeout=suite.timeout)
    if suite.name == "native":
        binary = ctx.work / "native-desktop"
        ctx.run(["cc", "-std=c99", "-O3", "-I", upstream / "include", ctx.root / "tests/native-desktop/workflow.c",
                 native / "libcui.a", "-lm", "-lglfw", "-o", binary], "native-desktop-build")
        ctx.run([binary, ctx.work / "native-desktop.rgba"], "native-desktop-run", timeout=120, separate=True)


def consumer(ctx, suite):
    from campaigns.testing import consumer as run_consumer
    run_consumer(ctx, suite)
    native = ctx.work / "native-desktop.rgba"
    if not native.is_file():
        return
    expected = native.read_bytes()
    comparisons = []
    for form in forms(ctx):
        for mode in modes(ctx):
            actual = (ctx.work / f"consumer-{form}-{mode}.rgba").read_bytes()
            if len(expected) != len(actual):
                raise RuntimeError("Native/translated GL framebuffer sizes differ")
            maximum = max(abs(a-b) for a,b in zip(expected, actual))
            # GL conversion can round a normalized channel one byte differently.
            if maximum > 1:
                raise RuntimeError(f"Native/translated GL readback differs by {maximum}, tolerance 1")
            comparisons.append(dict(form=form, mode=mode, bytes=len(actual), maximum_channel_error=maximum, tolerance=1))
    ctx.receipt["native_gl_readback_comparisons"] = comparisons
    ctx.save()


def gallery(ctx, suite):
    for form in forms(ctx):
        check_product(ctx, form)
    arguments = [sys.executable, ctx.root / "tests/gallery/run.py", "--work-root", ctx.work / "gallery",
                 "--source", source(ctx), "--form", ctx.options.form, "--mode", ctx.options.mode]
    if sys.platform == "linux":
        arguments.append("--xvfb")
    ctx.run(arguments, "gallery", timeout=suite.timeout)
    import json
    ctx.receipt["gallery_qualification"] = json.loads((ctx.work / "gallery/report.json").read_text())
    ctx.save()


def abi(ctx, suite):
    tools(ctx)
    ctx.run([sys.executable, "-B", "-m", "unittest", "discover", "-s", ctx.root / "tests/recipe", "-v"], "recipe-contracts")
    ctx.run([sys.executable, ctx.root / "tests/host-glfw/verify-abi.py", "--source-root", source(ctx)], "gl-abi")
    ctx.run([sys.executable, ctx.root / "scripts/generate-gl.py", "--check"], "gl-generated-check")
    ctx.run([sys.executable, ctx.root / "tests/platform/run.py", "--source", source(ctx),
             "--work", ctx.work / "platform", "--dotcc", ctx.compiler], "platform-abi", timeout=suite.timeout)
    project = ctx.root / "tests/host-glfw/HostGlfw.Tests.csproj"
    for form in forms(ctx):
        product = check_product(ctx, form)
        for mode in modes(ctx):
            label = f"host-{form}-{mode}"
            output = ctx.work / label
            prop = "-p:CuiProject=" + str(product)
            if mode == "jit":
                ctx.managed("build", project, label + "-build", prop, "-m:1", "-o", output)
                command = ["dotnet", output / "HostGlfw.Tests.dll"]
            else:
                ctx.managed("publish", project, label + "-publish", prop, "-m:1", "-r", ctx.options.rid,
                            "-p:PublishAot=true", "-o", output, timeout=1800)
                command = [output / ("HostGlfw.Tests.exe" if sys.platform == "win32" else "HostGlfw.Tests")]
            ctx.run(command, label + "-run", timeout=suite.timeout, separate=True)
            desktop = [sys.executable, ctx.root / "tests/desktop/run.py", "--work-root", ctx.work / ("desktop-" + form + "-" + mode),
                       "--cui-project", product]
            if mode == "aot":
                desktop.append("--aot")
            ctx.run(desktop, "desktop-" + form + "-" + mode, timeout=suite.timeout, separate=True)
            if sys.platform == "linux":
                services = [sys.executable, ctx.root / "tests/desktop-services/run.py", "--work-root",
                            ctx.work / ("desktop-services-" + form + "-" + mode), "--cui-project", product]
                if mode == "aot":
                    services.append("--aot")
                ctx.run(services, "desktop-services-" + form + "-" + mode, timeout=suite.timeout, separate=True)


def audit(ctx, suite):
    import json
    import re
    import xml.etree.ElementTree as ET
    findings = []
    for form in forms(ctx):
        product = check_product(ctx, form)
        suffix = ".Raw" if form == "raw" else ""
        companion = ctx.root / f"generated/TranslatedCuiGallery{suffix}/TranslatedCuiGallery.csproj"
        sample = ctx.root / "samples/ManagedConsumer/ManagedConsumer.csproj"
        evaluated = {}
        for name, project in (("core", product), ("gallery", companion), ("sample", sample)):
            if not project.is_file():
                raise RuntimeError(f"Missing {name} project: {project}")
            for node in ET.parse(project).iter():
                if node.tag not in ("Compile", "ProjectReference", "Import"):
                    continue
                value = node.get("Include") or node.get("Project") or ""
                if not value or "$" in value or "*" in value:
                    continue
                path = (project.parent / value).resolve()
                if not path.is_relative_to(ctx.root.resolve()) or not path.is_file():
                    raise RuntimeError(f"Product reference leaves campaign or is missing: {value}")
            label = f"audit-{form}-{name}-references"
            arguments = ["dotnet", "msbuild", project, "-getItem:ProjectReference", "--nologo"]
            if name == "sample":
                arguments.append("-p:CuiProject=" + str(product))
            ctx.run(arguments, label)
            data = json.loads((ctx.artifacts / (label + ".log")).read_text())
            references = [Path(row["FullPath"]).resolve() for row in data["Items"]["ProjectReference"]]
            for path in references:
                if not path.is_relative_to(ctx.root.resolve()) or not path.is_file():
                    raise RuntimeError(f"Evaluated {name} project reference leaves campaign or is missing: {path}")
            evaluated[name] = references
            if name in ("core", "gallery"):
                for path in project.parent.glob("*.cs"):
                    text = path.read_text(encoding="utf-8")
                    if re.search(r'(?:DllImport|LibraryImport)\s*\(\s*"[^"\n]*(?:libcui|cui\.dll|glfw)', text, re.I):
                        raise RuntimeError(f"Unexpected native CUI/GLFW dependency in {path.name}")
        if evaluated["gallery"] != [product.resolve()]:
            raise RuntimeError(f"Gallery does not reference the matching {form} core")
        if set(evaluated["sample"]) != {product.resolve(), companion.resolve()}:
            raise RuntimeError(f"Sample does not reference the matching {form} core and gallery")
        findings.append({"form": form, "references": "campaign-local and evaluated", "native_cui_glfw": False,
                         "gallery_core": "matching form", "sample": "ordinary core/gallery project references"})
    pin = json.loads((ctx.root / "config/glfw-source.json").read_text())
    missing = [name for name in pin["snapshot_sha256"] if not (ctx.root / "src/Glfw" / name).is_file()]
    if missing:
        raise RuntimeError("Copied GLFW closure missing: " + ", ".join(missing))
    ctx.receipt["dependency_audit"] = findings
    ctx.save()
    print("CUI dependency audit: campaign-local references and copied GLFW closure; no native CUI/GLFW imports", flush=True)
