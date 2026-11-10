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


def audit(ctx, suite):
    import json
    import re
    import xml.etree.ElementTree as ET
    findings = []
    for form in forms(ctx):
        product = check_product(ctx, form)
        for node in ET.parse(product).iter():
            if node.tag not in ("Compile", "ProjectReference", "Import"):
                continue
            value = node.get("Include") or node.get("Project") or ""
            if not value or "$" in value or "*" in value:
                continue
            path = (product.parent / value).resolve()
            if not path.is_relative_to(ctx.root.resolve()) or not path.is_file():
                raise RuntimeError(f"Product reference leaves campaign or is missing: {value}")
        for path in product.parent.glob("*.cs"):
            text = path.read_text(encoding="utf-8")
            if re.search(r'(?:DllImport|LibraryImport)\s*\(\s*"[^"\n]*(?:libcui|cui\.dll|glfw)', text, re.I):
                raise RuntimeError(f"Unexpected native CUI/GLFW dependency in {path.name}")
        findings.append({"form": form, "references": "campaign-local", "native_cui_glfw": False})
    pin = json.loads((ctx.root / "config/glfw-source.json").read_text())
    missing = [name for name in pin["snapshot_sha256"] if not (ctx.root / "src/Glfw" / name).is_file()]
    if missing:
        raise RuntimeError("Copied GLFW closure missing: " + ", ".join(missing))
    ctx.receipt["dependency_audit"] = findings
    ctx.save()
    print("CUI dependency audit: campaign-local references and copied GLFW closure; no native CUI/GLFW imports", flush=True)
