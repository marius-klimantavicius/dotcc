"""Source expression readability plus exact mutation/branch/layout semantics."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys


def verify_source_expressions(root, tests, out, nativeaot, run):
    checks = []
    source = tests / "source_expressions.c"
    snapshot = out / "inputs"
    snapshot.mkdir(exist_ok=True)
    hashes = {}
    inputs = [source, Path(__file__), *[root / "backend" / name for name in (
        "dot_gen_source_expression_api.h", "dot_gen_source_expressions.h", "dot_gen_bitfield_properties.h",
        "dot_gen_volatile_bits.h")]]
    for path in inputs:
        data = path.read_bytes()
        (snapshot / path.name).write_bytes(data)
        hashes[path.name] = hashlib.sha256(data).hexdigest()
    (snapshot / "sha256.json").write_text(json.dumps(hashes, indent=2)+"\n")
    env = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"))
    for language in ("c", "c++"):
        label = "cxx" if language == "c++" else "c"
        oracle = out / (label + "-oracle.c")
        run([root / "build/bin/cpfe", "--c++17" if language == "c++" else "--c11",
             "--gnu_version", "130300", *(["--instantiate", "used"] if language == "c++" else []),
             "--gen_c_file_name", oracle, "-o", out / (label + ".il"), source], cwd=out, env=env)
        executable = out / (label + "-oracle")
        run(["gcc", "-std=gnu11", oracle, root / "build/lib/libC.a", "-lm", "-o", executable])
        run([executable])
        checks.append(label + ": original EDG C expression oracle")
        for instance in (False, True):
            directory = out / (label + ("-instance" if instance else "-static"))
            run([sys.executable, root / "dotnet.py", source, "--language", language,
                 "--output-dir", directory, *(["--instance-methods"] if instance else [])])
            generated = "\n".join(p.read_text() for p in directory.glob("Translated*.cs"))
            from source_correspondence_verify import function_body
            body = function_body(generated, "readable_assignments")
            assert not re.search(r"__edg_(assign|bits_mutate)\d+", body), body
            assert re.search(r"->@?mode\s*\+=", body), body
            assert re.search(r"->@?ready\s*=", body), body
            loops = function_body(generated, "readable_comma_loops")
            assert re.search(r"for\s*\([^;]*;[^;]*;\s*j\s*--\s*,\s*i\s*\+\+\s*\)", loops), loops
            assert re.search(r"j\s*--\s*,\s*i\s*\+\+\s*,\s*total\s*\+=\s*i", loops), loops
            shifts = function_body(generated, "readable_shifts")
            assert re.search(r"value\s*>>=\s*7\s*;", shifts), shifts
            assert re.search(r"value\s*<<=\s*2\s*;", shifts), shifts
            (directory / "expression-readability.json").write_text(json.dumps({"body": body,"commaLoops":loops,"shifts":shifts}, indent=2)+"\n")
            if instance:
                entry = directory / "EntryPoint.cs"
                (directory / "EntryPoint.generated.cs.saved").write_bytes(entry.read_bytes())
                entry.write_text("""internal static class EntryPoint {
 public static int Main() {
  using var first = new Edg.Generated.TranslatedProgram();
  using var second = new Edg.Generated.TranslatedProgram();
  return first.Run() == 0 && second.Run() == 0 ? 0 : 99;
 }
}
""")
            project = directory / "Generated.csproj"
            run(["dotnet", "build", project, "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
            run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
            checks.append(directory.name + ": readable mutations, truncation, sequencing, branches and split guards JIT")
            if nativeaot:
                run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                     "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-p:DebugType=none",
                     "-p:DebugSymbols=false", "-o", directory / "aot"])
                run([directory / "aot/Generated"])
                checks.append(directory.name + ": Linux x64 NativeAOT")
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
    checks = verify_source_expressions(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} source expression checks passed")
