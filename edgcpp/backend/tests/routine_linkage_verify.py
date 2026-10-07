#!/usr/bin/env python3
"""Compare exact C-linkage symbol resolution with native and stock EDG C."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--oracle", type=Path, default=ROOT / "build/bin/cpfe-dot-oracle")
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    compiler = out / "cpfe-dot"
    shutil.copy2(args.compiler, compiler)
    source = TESTS / "routine_linkage.cpp"
    provider = TESTS / "routine_linkage_provider.cpp"
    pruning = out / "prunable-sources.txt"
    pruning.write_text(str(provider.resolve()) + "\n")
    # The public driver deliberately clears inherited pruning manifests.
    # Exercise the backend provider-linking contract after that boundary,
    # without changing the production musl source profile for this fixture.
    pruning_compiler = out / "cpfe-dot-pruning"
    pruning_compiler.write_text("#!/usr/bin/env python3\nimport os,sys\n"
        "os.environ['EDG_DOT_PRUNABLE_SOURCES'] = " + repr(str(pruning)) + "\n"
        "os.execv(" + repr(str(compiler)) + ", [" + repr(str(compiler)) + ", *sys.argv[1:]])\n")
    pruning_compiler.chmod(0o755)
    report = {"passed": False, "commands": [], "checks": [], "inputs": []}
    for path in [Path(__file__), source, provider, compiler, pruning_compiler, pruning, args.oracle,
                 ROOT / "backend/dot_gen_routine_symbols.h", ROOT / "build/lib/libC.a"]:
        report["inputs"].append({"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})

    def save():
        (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")

    def run(stage, command, rejection=None, extra_env=None):
        command = list(map(str, command))
        item = {"stage": stage, "command": command}
        report["commands"].append(item)
        save()
        start = time.monotonic()
        with (out / (stage + ".stdout")).open("w") as stdout, (out / (stage + ".stderr")).open("w") as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=900,
                                    env=dict(os.environ, EDG_BASE=str(ROOT / "upstream/bases/docker/dev-env/gcc"), **(extra_env or {})))
        item.update(exitCode=result.returncode, elapsedSeconds=time.monotonic() - start)
        save()
        if rejection:
            if result.returncode == 0 or rejection not in (out / (stage + ".stderr")).read_text():
                raise RuntimeError(stage + " did not reject the expected condition")
        elif result.returncode:
            raise RuntimeError(stage + " failed; see " + str(out))
        return (out / (stage + ".stdout")).read_text()

    expected = "routine linkage: calls, addresses, aggregates and varargs passed\n"
    run("native-build", ["g++", "-std=c++17", "-O2", source, "-o", out / "native"])
    assert run("native-run", [out / "native"]) == expected
    run("c-lower", [args.oracle.resolve(), "--c++17", "--gnu_version", "130300", "--instantiate", "used",
        "--gen_c_file_name", out / "original.c", "-o", out / "original.il", source])
    run("c-build", ["gcc", "-std=gnu11", "-w", out / "original.c", ROOT / "build/lib/libC.a", "-o", out / "original"])
    assert run("c-run", [out / "original"]) == expected
    report["checks"].append("Native and unchanged EDG C agree on cross-namespace calls, aggregate returns, variadics and function-address identity")
    for mode in ("static", "instance"):
        generated = out / mode
        command = [sys.executable, ROOT / "dotnet.py", source, "--compiler", pruning_compiler,
                   "--std", "c++17", "--libc", "dotcc", "--output-dir", generated]
        if mode == "instance":
            command.append("--instance-methods")
        run(mode + "-translate", command)
        manifest = json.loads((generated / "dot-gen.json").read_text())
        assert not manifest["unboundExternals"]["symbols"]
        generated_text = "\n".join(f.read_text() for f in generated.glob("*.cs"))
        assert "unused_provider_body" not in generated_text
        assert "uint export_value(" in generated_text
        run(mode + "-build", ["dotnet", "build", generated / "Generated.csproj", "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
        assert run(mode + "-jit", ["dotnet", generated / "bin/Release/net10.0/Generated.dll"]) == expected
        report["checks"].append(mode + " JIT: native observations match; exact-symbol provider bodies retained and unrelated provider body pruned")
        if args.nativeaot:
            rid = "linux-arm64" if os.uname().machine == "aarch64" else "linux-x64"
            run(mode + "-publish", ["dotnet", "publish", generated / "Generated.csproj", "-c", "Release", "-r", rid,
                                  "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", generated / "aot"])
            assert run(mode + "-aot", [generated / "aot/Generated"]) == expected
            report["checks"].append(mode + " NativeAOT: native observations match")

    negatives = {
        "abi": ('extern "C" int storage(int value) { return value; }\n'
                'extern "C" double view(int) __asm__("storage");\n'
                'int main() { return view(4) == 4 ? 0 : 1; }\n', "routine linker symbol ABI mismatch: storage"),
        "ambiguous": ('extern "C" int first(int) __asm__("shared_symbol");\n'
                      'extern "C" int second(int) __asm__("shared_symbol");\n'
                      'extern "C" int view(int) __asm__("shared_symbol");\n'
                      'int first(int x) { return x; }\nint second(int x) { return x + 1; }\n'
                      'int main() { return view(4); }\n', "multiple definitions for routine linker symbol shared_symbol"),
        "internal": ('static int hidden(int x) __asm__("private_symbol");\n'
                     'static int hidden(int x) { return x; }\n'
                     'extern "C" int view(int) __asm__("private_symbol");\n'
                     'int main() { return hidden(1) + view(2); }\n', "unsupported external call: view"),
        "cpp_namespace": ('namespace wanted { int value(int); }\n'
                          'namespace available { int value(int x) { return x; } }\n'
                          'int main() { return wanted::value(2); }\n', "unsupported external call:"),
    }
    for name, (text, rejection) in negatives.items():
        path = out / (name + ".cpp")
        path.write_text(text)
        run(name + "-reject", [sys.executable, ROOT / "dotnet.py", path, "--compiler", compiler,
                               "--std", "c++17", "--output-dir", out / (name + "-generated")], rejection=rejection)
        report["checks"].append("Rejected " + name + " symbol binding")
    report["passed"] = True
    save()


if __name__ == "__main__":
    main()
