"""Pinned selected-musl packaging uses semantic translation units and audited BCL services."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_musl_provider(root, tests, out, nativeaot, run):
    source = tests / "musl_provider.c"
    checks=[]
    for language in ("c", "c++"):
      for mode in ("static", "instance"):
        directory = out / (language.replace('+','x')+'-'+mode)
        options = ["--instance-methods"] if mode == "instance" else []
        if language=="c++": options += ["--runtime","edg-exceptions"]
        run([sys.executable, root / "dotnet.py", source, "--language",language,"--libc","musl",
             "--split","size","--split-size","24000","--output-dir", directory, *options])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        provider=manifest['libcProvider']
        assert provider['id']=='musl-selected-bcl-lp64-v1'
        assert len(provider['translationUnits'])==213
        import hashlib
        snapshot=Path(provider['snapshot'])
        for item in provider['translationUnits']:
            path=snapshot/item['component']/item['file']
            assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256']
        if mode == "instance":
            entry = directory / "EntryPoint.cs"
            (directory / "EntryPoint.generated.cs.saved").write_bytes(entry.read_bytes())
            entry.write_text("""internal static class EntryPoint {
  public static int Main() {
    using var a = new Edg.Generated.TranslatedProgram();
    using var b = new Edg.Generated.TranslatedProgram();
    return a.Run() == 0 && b.Run() == 0 ? 0 : 99;
  }
}
""")
        project = directory / "Generated.csproj"
        run(["dotnet", "build", project, "-c", "Release", "--nologo"])
        run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
        checks.append(f"selected musl {language}/{mode}: JIT source hashes, formatting, numeric conversion, bootstrap and globals")
        if nativeaot:
            run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                 "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-p:DebugType=none", "-p:DebugSymbols=false", "-o", directory / "aot"])
            run([directory / "aot/Generated"])
            checks.append(f"selected musl {language}/{mode}: Linux x64 NativeAOT")
    return checks


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    commands = []

    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, text=True, capture_output=True, **kwargs)
        index = len(commands)
        (output / f"command-{index:03d}.stdout").write_text(result.stdout)
        (output / f"command-{index:03d}.stderr").write_text(result.stderr)
        commands.append({"command": command, "exit": result.returncode})
        (output / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")
        return result

    root = Path(__file__).resolve().parents[2]
    checks = verify_musl_provider(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} selected musl provider checks passed")
