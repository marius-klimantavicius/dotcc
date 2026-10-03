"""Named fixed-array indexing with preserved pointer identity and evaluation."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys


def verify_source_arrays(root, tests, out, nativeaot, run):
    checks = []
    source = tests / "source_arrays.c"
    snapshot = out / "inputs"
    snapshot.mkdir(exist_ok=True)
    hashes = {}
    inputs = [source, Path(__file__), *[root / "backend" / name for name in (
        "dot_gen_source_expression_api.h", "dot_gen_source_control.h", "dot_gen_bitfield_properties.h")]]
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
            arrays = function_body(generated, "named_arrays")
            buffer = function_body(generated, "copy_buffer")
            for name in ("values", "pointers", "items", "global_values", "thread_values"):
                assert re.search(r"\b"+name+r"\[", arrays), (name, arrays)
            assert re.search(r"\bbuf\[", buffer), buffer
            (directory / "array-review.json").write_text(json.dumps({"arrays": arrays, "buffer": buffer}, indent=2)+"\n")
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
            checks.append(directory.name + ": named fixed arrays, aliases, one-past addresses and evaluation JIT")
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
    output.mkdir(parents=True, exist_ok=False)
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
    checks = verify_source_arrays(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} source array checks passed")
