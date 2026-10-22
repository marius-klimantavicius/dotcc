"""Source-only codec and wire contracts, separate from engine acceptance."""
import os
import re


def run(ctx, suite):
    projects = (
        ("protocol", ctx.root / "tests/ApiProtocol/ApiProtocol.csproj", "ApiProtocol"),
        ("codecs", ctx.root / "tests/ApiCodecs/Contracts/ApiCodecs.Contracts.csproj", "ApiCodecs.Contracts"),
    )
    modes = ("jit", "aot") if ctx.options.mode == "all" else (ctx.options.mode,)
    records = []
    for name, project, assembly in projects:
        artifacts = ctx.work / "api-contracts" / name
        for mode in modes:
            label = "api-contracts-" + name + "-" + mode
            if mode == "jit":
                ctx.managed("build", project, label + "-build", "--artifacts-path", str(artifacts), "-m:1")
                command = ["dotnet", str(artifacts / "bin" / assembly / "release" / (assembly + ".dll"))]
            else:
                output = artifacts / "publish"
                ctx.managed("publish", project, label + "-publish", "--artifacts-path", str(artifacts),
                            "-r", ctx.options.rid, "-p:PublishAot=true", "-m:1", "-o", str(output), timeout=1800)
                command = [str(output / (assembly + (".exe" if os.name == "nt" else "")))]
            stdout = ctx.run(command, label + "-run", timeout=600)
            match = re.search(r"PASS\s+(\d+)\s+", stdout) or re.search(r"(\d+)\s+assertions passed", stdout)
            if not match:
                raise RuntimeError("Missing semantic assertion count for " + label)
            records.append(dict(component=name, mode=mode, assertions=int(match[1]), status="passed"))
    ctx.receipt["pglite_api_contracts"] = records
    return records
