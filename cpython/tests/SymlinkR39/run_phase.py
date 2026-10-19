"""Focused native/current-Libc controls using the existing command boundary."""
import json
import os
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.process import execute

HERE = Path(__file__).resolve().parent
WORK = ROOT / "cpython/build/owner-b-r39-20261018"
ART = ROOT / "cpython/artifacts/owner-b-r39-20261018"
phase = sys.argv[1]
receipt = ART / (phase + "-commands.json")
if receipt.exists():
    raise SystemExit("Preserve original evidence; use a fresh phase name.")
environment = dict(os.environ, DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_USE_MSBUILD_SERVER="0",
                   MSBUILDDISABLENODEREUSE="1", UseSharedCompilation="false",
                   DOTCC_CAMPAIGN_COMMAND_LOCKS="production")
records = []


def run(argv, label, *, cwd=Path("/tmp"), check=True, timeout=120):
    record = {"declared_environment": {k: environment[k] for k in
              ("DOTNET_PROCESSOR_COUNT", "DOTNET_CLI_USE_MSBUILD_SERVER", "MSBUILDDISABLENODEREUSE",
               "UseSharedCompilation", "DOTCC_CAMPAIGN_COMMAND_LOCKS")}}
    records.append(record)
    try:
        execute(list(map(str, argv)), label=label, cwd=cwd, output=ART / (label + ".log"),
                timeout=timeout, env=environment, record=record, check=check)
    finally:
        receipt.write_text(json.dumps(records, indent=2) + "\n")
    print(json.dumps({k: record.get(k) for k in ("label", "status", "exit_code", "seconds")}), flush=True)


common = ["-c", "Release", "--disable-build-servers", "-m:1",
          "-p:BuildInParallel=false", "-p:UseSharedCompilation=false"]
if phase in ("native", "native-abi", "native-final"):
    files = WORK / (phase + "-files")
    files.mkdir()
    run(["cc", "-std=c17", "-D_POSIX_C_SOURCE=200809L", "-O2", "-Wall", "-Wextra",
         HERE / "symlink_boundary.c", "-o", WORK / phase], phase + "-build")
    run([WORK / phase], phase + "-run", cwd=files)
elif phase in ("baseline", "final"):
    out = WORK / phase
    extra = ["-p:LibcProject=" + str(WORK / "before-lib/Before.csproj")] if phase == "baseline" else []
    run(["dotnet", "build", HERE / "SymlinkR39.csproj", *common, *extra,
         "--artifacts-path", out / "products", "-o", out / "app"], phase + "-build")
    run(["dotnet", out / "app/SymlinkR39.dll", WORK / (phase + "-files"), ART / "native-final-run.log"],
        phase + "-jit", check=phase != "baseline")
elif phase == "aot":
    out = WORK / phase
    run(["dotnet", "publish", HERE / "SymlinkR39.csproj", *common, "-r", "linux-x64",
         "-p:PublishAot=true", "--artifacts-path", out / "products", "-o", out / "app"],
        phase + "-build", timeout=180)
    run([out / "app/SymlinkR39", WORK / (phase + "-files"), ART / "native-final-run.log"], phase + "-run")
elif phase in ("units-red", "units-final"):
    extra = ["-p:LibcProject=" + str(WORK / "before-lib/Before.csproj")] if phase == "units-red" else []
    run(["dotnet", "test", HERE / "Unit/Unit.csproj", *common, *extra,
         "--artifacts-path", WORK / (phase + "-products"), "--logger", "trx;LogFileName=focused.trx",
         "--results-directory", ART / (phase + "-results")], phase, check=phase != "units-red")
elif phase == "metadata":
    inspector = ROOT / "cpython/build/owner-a-r30-20261018/metadata-app/MetadataAudit.dll"
    for name, base in (("jit", WORK / "final/app"),
                       ("aot", WORK / "aot/products/bin/SymlinkR39/release_linux-x64")):
        run(["dotnet", inspector, base / "DotCC.Libc.dll", WORK / (name + "-libc-metadata.json")],
            name + "-metadata")
elif phase == "unit-inputs":
    run(["dotnet", "msbuild", HERE / "Unit/Unit.csproj", "-p:Configuration=Release",
         "-p:UseArtifactsOutput=true", "-p:ArtifactsPath=" + str(WORK / "units-final-products"),
         "-getItem:Compile"], "unit-evaluated-compile-inputs")
else:
    raise SystemExit("Unknown phase")
