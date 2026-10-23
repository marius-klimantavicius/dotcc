#!/usr/bin/env python3
"""Independent real emission/header/callback probe; uses an existing immutable compiler."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--certificate",type=Path,default=ROOT / "artifacts/provider/jit/abi-leaf.pem")
    parser.add_argument("--work",type=Path,default=ROOT / "build/provider/emitted-probe")
    parser.add_argument("--artifacts",type=Path,default=ROOT / "artifacts/provider/emitted-probe")
    args = parser.parse_args()
    compiler = args.compiler.resolve()
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    commands = []

    def status(value, **extra):
        (artifacts / "receipt.json").write_text(json.dumps({"status": value, "scope": "actual emitted header/EVP/callback ABI; not upstream s2n wrapper/TLS qualification",
            "compiler": str(compiler), "compiler_sha256": hashlib.sha256(compiler.read_bytes()).hexdigest(), "commands": commands, **extra}, indent=2) + "\n")

    def run(argv, label):
        argv = list(map(str, argv)); commands.append(argv)
        try:
            with (artifacts / label).open("w") as log:
                result = subprocess.run(argv, cwd=ROOT.parent, stdout=log, stderr=subprocess.STDOUT, timeout=600)
        except Exception as error:
            status("failed", error=str(error), log=label)
            raise
        if result.returncode:
            status("failed", exit_code=result.returncode, log=label)
            raise RuntimeError(f"Probe command failed ({result.returncode}): {artifacts / label}")

    status("running")
    metadata = json.loads((ROOT / "config/provider.json").read_text())
    snapshot = work / "provider-overrides.json"
    shutil.copyfile(ROOT / metadata["override_file"], snapshot)
    # Native declaration control uses the real configured oracle headers; managed
    # declaration config uses the supervisor's fixed LP64 header, with unchanged ref headers.
    native = ROOT / "build/native/openssl-legacy"
    run(["cc", "-std=c11", "-O2", "-DPROVIDER_NATIVE_PROBE=1", "-I", native / "include", "-I", ROOT / "src/CryptoProvider/include",
         ROOT / "tests/Abi/provider-probe.c", native / "libcrypto.a", "-ldl", "-pthread", "-o", work / "native-probe"], "native-build.log")
    run([work / "native-probe"], "native.txt")
    run(["dotnet", compiler, "--emit=managedlib", "--class-name", "S2nTls", "--namespace", "Managed.Security.S2n", "--split=size", "--literal-pool",
         "--deduplicate-inline", "--prune-unused-inline", "--overrides-file", snapshot, "-std=c11", "-I", ROOT / "config/headers", "-I", ROOT / "ref/openssl-legacy/include",
         "-I", ROOT / "src/CryptoProvider/include", ROOT / "tests/Abi/provider-probe.c", "-o", work / "Emitted"], "emission.log")
    project = work / "Emitted/Emitted.csproj"
    tree = ET.parse(project); item = ET.SubElement(tree.getroot(), "ItemGroup")
    ET.SubElement(item, "ProjectReference", Include=str(ROOT / metadata["project_reference"]))
    for authored in metadata["authored_bindings"]:
        ET.SubElement(item, "Compile", Include=str(ROOT / authored), Link=Path(authored).name)
    ET.indent(tree); tree.write(project, encoding="unicode")
    if args.mode in ("jit", "all"):
        run(["dotnet", "run", "--project", ROOT / "tests/Abi/ProviderAbiProbe.csproj", "-p:ProviderEmittedProject=" + str(project), "-c", "Release", "--artifacts-path", work / "dotnet", "--", artifacts / "native.txt", artifacts / "jit.json",args.certificate.resolve()], "jit.log")
    if args.mode in ("aot", "all"):
        run(["dotnet", "publish", ROOT / "tests/Abi/ProviderAbiProbe.csproj", "-p:ProviderEmittedProject=" + str(project), "-c", "Release", "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot", "-m:1", "--nologo", "-v", "minimal", "--artifacts-path", work / "dotnet"], "aot-build.log")
        run([work / "aot/ProviderAbiProbe", artifacts / "native.txt", artifacts / "aot.json",args.certificate.resolve()], "aot.log")
    status("passed", mode=args.mode, override_sha256=hashlib.sha256(snapshot.read_bytes()).hexdigest())
    print(f"Emitted provider ABI passed ({args.mode}): {artifacts / 'receipt.json'}")


if __name__ == "__main__":
    main()
