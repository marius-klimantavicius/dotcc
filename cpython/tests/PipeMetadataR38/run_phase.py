"""Focused service checks using the existing production command boundary."""
from pathlib import Path
import json
import os
import sys

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.process import execute

HERE = Path(__file__).resolve().parent
WORK = ROOT / "cpython/build/owner-b-r38-20261018"
ART = ROOT / "cpython/artifacts/owner-b-r38-20261018"
phase = sys.argv[1]
receipt = ART / (phase + "-commands.json")
if receipt.exists():
    raise SystemExit("Preserve existing phase evidence; select a fresh phase name.")
environment = dict(os.environ, DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_USE_MSBUILD_SERVER="0",
                   MSBUILDDISABLENODEREUSE="1", UseSharedCompilation="false",
                   DOTCC_CAMPAIGN_COMMAND_LOCKS="production")
records = []

def run(argv, label, *, check=True, timeout=120):
    record = {"declared_environment": {k: environment[k] for k in
              ("DOTNET_PROCESSOR_COUNT", "DOTNET_CLI_USE_MSBUILD_SERVER", "MSBUILDDISABLENODEREUSE",
               "UseSharedCompilation", "DOTCC_CAMPAIGN_COMMAND_LOCKS")}}
    records.append(record)
    try:
        execute(list(map(str, argv)), label=label, cwd=Path("/tmp"), output=ART / (label + ".log"),
                timeout=timeout, env=environment, record=record, check=check)
    finally:
        receipt.write_text(json.dumps(records, indent=2) + "\n")
    print(json.dumps({k: record.get(k) for k in ("label", "status", "exit_code", "seconds")}), flush=True)
    return record

common = ["-c", "Release", "--disable-build-servers", "-m:1",
          "-p:BuildInParallel=false", "-p:UseSharedCompilation=false"]
if phase in ("native-identity", "native-final", "native-fileio"):
    run(["cc", "-D_POSIX_C_SOURCE=200809L", "-std=c17", "-Wall", "-Wextra", "-O2",
         ROOT / "cpython/tests/Abi/pipe_identity_r38.c", "-o", WORK / (phase + "-pipe")], phase + "-build")
    run([WORK / (phase + "-pipe")], phase + "-run")
elif phase in ("baseline", "fixed", "fixed-timestamps", "final"):
    out = WORK / phase
    extra = ["-p:LibcProject=" + str(WORK / "baseline-lib/LibcBefore.csproj")] if phase == "baseline" else []
    run(["dotnet", "build", HERE / "PipeMetadataR38.csproj", *common, *extra,
         "--artifacts-path", out / "products", "-o", out / "app"], phase + "-build")
    run(["dotnet", out / "app/PipeMetadataR38.dll"], phase + "-jit", check=phase != "baseline")
elif phase in ("aot", "aot-final"):
    out = WORK / phase
    run(["dotnet", "publish", HERE / "PipeMetadataR38.csproj", *common, "-r", "linux-x64",
         "-p:PublishAot=true", "--artifacts-path", out / "products", "-o", out / "app"], phase + "-build", timeout=180)
    run([out / "app/PipeMetadataR38"], phase + "-run")
elif phase in ("units-red", "units-fixed", "units-final"):
    extra = ["-p:LibcProject=" + str(WORK / "baseline-lib/LibcBefore.csproj")] if phase == "units-red" else []
    run(["dotnet", "test", HERE / "Unit/Unit.csproj", *common, *extra,
         "--artifacts-path", WORK / (phase + "-products"), "--logger", "trx;LogFileName=focused.trx",
         "--results-directory", ART / (phase + "-results")], phase, check=phase != "units-red")
else:
    raise SystemExit("Unknown phase")
