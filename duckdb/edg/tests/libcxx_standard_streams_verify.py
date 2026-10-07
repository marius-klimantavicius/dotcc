"""Verify real libc++ standard streams, typed storage aliases and initialization."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--acquisition", type=Path, required=True)
    parser.add_argument("--compiler", type=Path)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[3]
    edg = root / "edgcpp"
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    compiler = out / "cpfe-dot"
    shutil.copy2(args.compiler or edg / "build/bin/cpfe-dot", compiler)
    source = Path(__file__).with_name("libcxx_standard_streams.cpp")
    spec = importlib.util.spec_from_file_location("duckdb_libcxx", Path(__file__).resolve().parents[1] / "libcxx.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    provider = module.prepare_provider(edg, args.acquisition, out / "provider", standard="c++17",
                                       unity_provider=True, primary_consumers=[source])
    report = {"passed": False, "provider": provider, "commands": [], "checks": [],
              "sourceSha256": hashlib.sha256(source.read_bytes()).hexdigest(),
              "compilerSha256": hashlib.sha256(compiler.read_bytes()).hexdigest()}

    def save():
        (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")

    def run(stage, command, stdin=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, input=stdin, capture_output=True, text=True,
                                env=dict(os.environ, EDG_DOT_TARGET="portable-lp64-binary80-v2"))
        (out / (stage + ".stdout")).write_text(result.stdout)
        (out / (stage + ".stderr")).write_text(result.stderr)
        report["commands"].append({"stage": stage, "command": command, "exitCode": result.returncode})
        save()
        if result.returncode:
            raise RuntimeError(stage + " failed; see " + str(out))
        return result.stdout, result.stderr

    run("native-build", ["g++", "-std=c++17", "-pthread", source, "-o", out / "native"])
    expected = run("native-run", [out / "native"], stdin="41\n")
    includes = [*provider["includeDirectories"], edg / "backend/runtime/posix/include",
                edg / "backend/runtime/libc/include"]
    command = [compiler, "--c++17", "--gnu_version", "130300", "--clear_flag", "use_predefined_macro_file",
               "--preprocess", *[v for inc in includes for v in ("-I", inc)], provider["translationUnits"][0]]
    preprocessed, _ = run("preprocess", command)
    source_input = out / "provider.ii"
    source_input.write_text(preprocessed)
    report["preprocessedSha256"] = hashlib.sha256(source_input.read_bytes()).hexdigest()
    for mode in ("static", "instance"):
        generated = out / mode
        command = [sys.executable, edg / "dotnet.py", source_input, "--compiler", compiler,
                   "--std", "c++17", "--libc", "musl", "--posix",
                   "--emit", "exe", "--output-dir", generated]
        if mode == "instance":
            command.append("--instance-methods")
        run(mode + "-translate", command)
        manifest = json.loads((generated / "dot-gen.json").read_text())
        if manifest.get("unboundExternals", {}).get("requiredPartials"):
            raise RuntimeError("Unresolved externs remain")
        run(mode + "-build", ["dotnet", "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        if run(mode + "-jit", ["dotnet", generated / "bin/Release/net10.0/Generated.dll"], stdin="41\n") != expected:
            raise RuntimeError(mode + " JIT stdout/stderr differs from native")
        report["checks"].append(mode + " JIT: all eight public stream object types/storage, initialization before user globals, ties/buffers, real stdin, cout/cerr/clog, redirected wide input/output and exit flushing")
        if args.nativeaot:
            run(mode + "-publish", ["dotnet", "publish", generated / "Generated.csproj", "-c", "Release",
                "-r", "linux-x64", "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", generated / "aot"])
            if run(mode + "-aot", [generated / "aot/Generated"], stdin="41\n") != expected:
                raise RuntimeError(mode + " NativeAOT stdout/stderr differs from native")
            report["checks"].append(mode + " NativeAOT: identical native-oracle stdout/stderr and successful execution")
        save()
    report["passed"] = True
    save()


if __name__ == "__main__":
    main()
