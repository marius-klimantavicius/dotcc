"""Bounded focused commands through the existing production command boundary."""
from pathlib import Path
import json
import os
import sys

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.process import execute

HERE = Path(__file__).resolve().parent
WORK = ROOT / "cpython/build/owner-b-r35-20261018"
ART = ROOT / "cpython/artifacts/owner-b-r35-20261018"
WORK.mkdir(parents=True, exist_ok=True)
ART.mkdir(parents=True, exist_ok=True)
phase = sys.argv[1]
receipt = ART / (phase + "-commands.json")
if receipt.exists():
    raise SystemExit("Preserve existing phase evidence; choose a fresh phase for another execution.")
commands = []
environment = dict(os.environ, DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_USE_MSBUILD_SERVER="0",
                   MSBUILDDISABLENODEREUSE="1", UseSharedCompilation="false",
                   DOTCC_CAMPAIGN_COMMAND_LOCKS="production")

def run(argv, label, check=True, timeout=120):
    record = {"declared_environment": {key: environment[key] for key in
              ("DOTNET_PROCESSOR_COUNT", "DOTNET_CLI_USE_MSBUILD_SERVER", "MSBUILDDISABLENODEREUSE",
               "UseSharedCompilation", "DOTCC_CAMPAIGN_COMMAND_LOCKS")}}
    commands.append(record)
    try:
        execute(list(map(str, argv)), label=label, cwd=Path("/tmp"), output=ART / (label + ".log"),
                timeout=timeout, env=environment, record=record, check=check)
    finally:
        receipt.write_text(json.dumps(commands, indent=2) + "\n")
    print(json.dumps({key: record.get(key) for key in ("label", "status", "exit_code", "seconds")}), flush=True)
    return record

if phase in ("native", "native-corrected"):
    run(["cc", "-D_POSIX_C_SOURCE=200809L", "-std=c11", "-Wall", "-Wextra", "-Werror", "-O2",
         ROOT / "cpython/tests/Abi/ftruncate_position_r35.c", "-o", WORK / phase], phase + "-build")
    run([WORK / phase, WORK / (phase + "-file")], phase + "-control")
elif phase in ("baseline", "baseline-faithful", "fixed"):
    out = WORK / phase
    run(["dotnet", "build", HERE / "FtruncateR35.csproj", "-c", "Release", "--disable-build-servers",
         "-m:1", "-p:BuildInParallel=false", "-p:UseSharedCompilation=false", "--artifacts-path", out / "products",
         "-o", out / "app"], phase + "-build")
    run(["dotnet", out / "app/FtruncateR35.dll", out / "file", ART / "native-corrected-control.log"],
        phase + "-jit", check=not phase.startswith("baseline"))
elif phase == "aot":
    out = WORK / phase
    run(["dotnet", "publish", HERE / "FtruncateR35.csproj", "-c", "Release", "-r", "linux-x64",
         "-p:PublishAot=true", "--disable-build-servers", "-m:1", "-p:BuildInParallel=false",
         "-p:UseSharedCompilation=false", "--artifacts-path", out / "products", "-o", out / "app"],
        "fixed-aot-build", timeout=180)
    run([out / "app/FtruncateR35", out / "file", ART / "native-corrected-control.log"], "fixed-aot")
elif phase in ("units-red", "units-fixed"):
    run(["dotnet", "test", HERE / "Unit/Unit.csproj", "-c", "Release", "--disable-build-servers",
         "-m:1", "-p:BuildInParallel=false", "-p:UseSharedCompilation=false", "--artifacts-path", WORK / (phase + "-products"),
         "--logger", "trx;LogFileName=focused.trx", "--results-directory", ART / (phase + "-results")], phase,
        check=phase != "units-red")
else:
    raise SystemExit("Unknown phase")
