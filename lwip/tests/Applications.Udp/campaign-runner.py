"""Registered finite application workflows with matching native product boundaries."""
import hashlib
import json
import sys
from pathlib import Path

CASES = ("tftp-server", "tftp-get", "tftp-put", "sntp", "netbios", "mdns", "owned-tftp")


def run(ctx, suite):
    from campaigns.testing import forms, modes
    from campaigns.translation import check_product
    if ctx.options.rid != "linux-x64":
        raise ValueError("UDP application native comparison currently requires linux-x64")
    case = suite.name.removeprefix("udp-app-")
    if case not in CASES or suite.name != "udp-app-" + case:
        raise ValueError("Select an explicit reviewed UDP application case")
    native_dir = ctx.artifacts / (case + "-native")
    ctx.run([sys.executable, ctx.root / "tests/Applications.Udp/native.py",
             "--profile", ctx.profile, "--case", case, "--output", native_dir, "--product-boundaries"], case + "-native", timeout=420)
    native = native_dir / "report.json"
    results = []
    try:
        for form in forms(ctx):
            product = check_product(ctx, form)
            for mode in modes(ctx):
                label = suite.name + "-" + form + "-" + mode
                # Reuse only inside this Context/receipt, after hashing every
                # fixture, facade/host, product and configuration input again.
                source_paths = set()
                for directory in (ctx.root / "tests/Applications.Udp", ctx.root / "tests/Support",
                                  ctx.root / "src", ctx.root / "config", product.parent):
                    source_paths.update(path for path in directory.rglob("*") if path.is_file()
                        and not any(part in ("bin", "obj", "__pycache__") for part in path.parts)
                        and path.suffix in (".cs", ".csproj", ".props", ".json", ".py", ".c", ".h"))
                source_paths.update((ctx.repo / name) for name in ("Directory.Build.props", "Directory.Packages.props", "nuget.config"))
                inputs = {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in sorted(source_paths)}
                contract = {"product": str(product), "profile": ctx.profile, "form": form,
                            "mode": mode, "rid": ctx.options.rid, "inputs": inputs}
                key = hashlib.sha256(json.dumps(contract, sort_keys=True).encode()).hexdigest()
                cache = getattr(ctx, "_udp_application_builds", None)
                if cache is None:
                    cache = {};ctx._udp_application_builds = cache
                if key not in cache:
                    work = ctx.work / ("udp-fixture-" + form + "-" + mode + "-" + key[:12])
                    output = work / "app"
                    props = ["-p:LwipProject=" + str(product), "-p:LwipProfile=" + ctx.profile,
                             "-p:UseLocalLalrCc=false", "--artifacts-path", work / "artifacts", "-o", output]
                    project = ctx.root / "tests/Applications.Udp/Applications.Udp.csproj"
                    if mode == "jit":
                        ctx.managed("build", project, label + "-build", *props)
                        command = ["dotnet", output / "Applications.Udp.dll"]
                    else:
                        ctx.managed("publish", project, label + "-publish", *props, "-r", ctx.options.rid, "-p:PublishAot=true", timeout=1800)
                        command = [output / "Applications.Udp"]
                    outputs = {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in sorted(output.iterdir()) if path.is_file()}
                    cache[key] = {"contract": contract, "command": [str(item) for item in command], "outputs": outputs, "first_case": case}
                    (ctx.artifacts / ("udp-fixture-" + key + ".json")).write_text(json.dumps(cache[key], indent=2) + "\n")
                build = cache[key]
                if any(hashlib.sha256(Path(path).read_bytes()).hexdigest() != digest for path, digest in build["outputs"].items()):
                    raise RuntimeError("Reviewed UDP fixture executable changed within receipt")
                command = build["command"]
                transcript = ctx.run([*command, case, native], label + "-run", timeout=60)
                observations = [json.loads(line) for line in transcript.splitlines() if line.startswith("{")]
                if not any(item.get("case") == case and item.get("status") == "passed" for item in observations):
                    raise AssertionError("Missing completed managed UDP application case")
                results.append({"case": case, "profile": ctx.profile, "form": form, "mode": mode, "status": "passed", "fixture_build_key": key, "fixture_first_case": build["first_case"]})
    finally:
        (ctx.artifacts / (suite.name + "-report.json")).write_text(json.dumps({"results": results,
            "scope": "Finite original application and owning SNTP/TFTP cases; broader variants and other owning services remain required"}, indent=2) + "\n")
