"""Supported EDG VLA lowering with NativeMemory storage and CLR cleanup."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_vla(root,tests,out,nativeaot,run):
    checks=[]
    for language in ("c","c++"):
        directory=out/language.replace('+','x');directory.mkdir(parents=True)
        checks.extend(verify_vla_case(root,tests,directory,nativeaot,run,language))
    return checks


def verify_vla_case(root, tests, out, nativeaot, run, language):
    source = tests / ("vla.c" if language=="c" else "vla_objects.cpp")
    options_frontend=["--c11"] if language=="c" else ["--c++17","--instantiate","used"]
    env = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"))
    c_source = out / "vla-oracle.c"
    run([root / "build/bin/cpfe", *options_frontend, "--gnu_version", "130300",
         "--gen_c_file_name", c_source, "-o", out / "vla-oracle.il", source],
        cwd=out, env=env)
    native = out / "vla-oracle"
    runtime=out/"vla-runtime-aligned.o"
    # The supplied demonstration runtime defaults to 8-byte alignment. Select
    # its documented configuration for our 16-byte long-double target.
    run(["g++","-std=c++17","-fno-exceptions","-fno-rtti","-D__EDG_RUNTIME_USES_NAMESPACES=1",
         "-D__EDG_LOWER_VARIABLE_LENGTH_ARRAYS=1","-DMOST_STRICT_ALIGNMENT=16","-include","stddef.h",
         "-c",root/"upstream/lib_src/vla_alloc.c","-o",runtime])
    run(["gcc", "-std=gnu11", c_source, runtime, root / "build/lib/libC.a", "-lm", "-o", native])
    run([native])
    checks = ["VLA: original EDG C oracle"]
    for mode in ("static", "instance"):
        directory = out / mode
        options = ["--instance-methods"] if mode == "instance" else []
        run([sys.executable, root / "dotnet.py", source,"--language",language, "--output-dir", directory, *options])
        if mode == "instance":
            entry = directory / "EntryPoint.cs"
            (directory / "EntryPoint.generated.cs.saved").write_bytes(entry.read_bytes())
            entry.write_text("""internal static class EntryPoint {
  public static int Main() {
    using var a = new Edg.Generated.TranslatedProgram();
    using var b = new Edg.Generated.TranslatedProgram();
    return a.Run() == 0 && b.Run() == 0 && global::Edg.Runtime.Vla.Mark()==0 ? 0 : 99;
  }
}
""")
        project = directory / "Generated.csproj"
        run(["dotnet", "build", project, "-c", "Release", "--nologo"])
        run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
        checks.append(f"VLA {mode}: JIT dimensions, strides, scope release and activation lifetimes")
        if nativeaot:
            run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                 "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
            run([directory / "aot/Generated"])
            checks.append(f"VLA {mode}: Linux x64 NativeAOT")
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
    checks = verify_vla(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} VLA checks passed")
