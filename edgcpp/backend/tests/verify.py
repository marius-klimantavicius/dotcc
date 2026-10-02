#!/usr/bin/env python3
"""Differential scalar tests, rejection checks and an optional real NativeAOT run."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def run(command, expected=0, **kwargs):
    result = subprocess.run(list(map(str, command)), text=True, capture_output=True, **kwargs)
    if result.returncode != expected:
        raise RuntimeError(f"{command}\nexit={result.returncode}, expected={expected}\n{result.stdout}\n{result.stderr}")
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--nativeaot", action="store_true")
    parser.add_argument("--output-dir", type=Path, default=ROOT / "artifacts/backend-tests")
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=True)
    checks = []
    run(["g++", "-std=c++17", "-O0", TESTS / "scalars.cpp", "-o", out / "native-scalars"])
    run([out / "native-scalars"])
    for mode in ("static", "instance"):
        directory = out / mode
        options = ["--instance-methods", "--split", "function"] if mode == "instance" else []
        run([sys.executable, ROOT / "dotnet.py", TESTS / "scalars.cpp", "--output-dir", directory, *options])
        project = directory / "Generated.csproj"
        run(["dotnet", "build", project, "-c", "Release", "--nologo"])
        run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
        checks.append(f"{mode}: scalar native/C# differential execution")
        if args.nativeaot:
            run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                 "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
            run([directory / "aot/Generated"])
            checks.append(f"{mode}: linux-x64 NativeAOT execution")
    directory = out / "owners"
    run([sys.executable, ROOT / "dotnet.py", TESTS / "instances.cpp", "--output-dir", directory,
         "--instance-methods", "--namespace", "test.namespace", "--class-name", "class",
         "--split", "size", "--split-size", "256"])
    (directory / "EntryPoint.cs").write_text('''internal static class EntryPoint {
    public static int Main() {
        var a = new test.@namespace.@class();
        var b = new test.@namespace.@class();
        return a.Run() == 11 && a.Run() == 12 && b.Run() == 11 && a.Run() == 13 ? 0 : 99;
    }
}
''')
    run(["dotnet", "build", directory / "Generated.csproj", "-c", "Release", "--nologo"])
    run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
    checks.append("independent owners and C# keyword identifiers")
    if args.nativeaot:
        run(["dotnet", "publish", directory / "Generated.csproj", "-c", "Release", "-r", "linux-x64",
             "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
        run([directory / "aot/Generated"])
        checks.append("independent owners: linux-x64 NativeAOT execution")
    cases = {
        "variadic-pointer": "int f(int, ...); int (*p)(int, ...) = f; int main() { return 0; }",
        "overaligned-local": "int main() { alignas(8192) int n = 3; return *(&n); }",
        "reference-dynamic-cast-without-runtime": "struct B { virtual int f(){return 1;} }; struct D:B {}; D& query(B& b){return dynamic_cast<D&>(b);} int main(){B b;try{query(b);}catch(...){return 0;}return 1;}",
        "large-aggregate": "int main() { int data[20000]; data[0]=1; return data[0]; }",
        "external-variable-reference": "extern int n; int main(){return n;}",
        "external-variable-address": "extern int n; int* p=&n; int main(){return *p;}",
        "external-delete-provider": "struct X { virtual ~X() {} }; int main() { try { throw X{}; } catch (...) { return 0; } }",
        "function-pointer-exception": "void f() {} int main() { try { throw &f; } catch (...) { return 0; } }",
        "array-pointer-exception": "int main() { int n[3]; try { throw &n; } catch (...) { return 0; } }",
        "member-pointer-exception": "struct S { int n; }; int main() { try { throw &S::n; } catch (...) { return 0; } }",
        "external": "int f(); int main() { return f(); }",
    }
    rejected = Path(tempfile.mkdtemp(prefix="rejection-", dir=out)) / "output"
    for label, source in cases.items():
        source_path = out / f"{label}.cpp"
        source_path.write_text(source + "\n")
        result = subprocess.run([sys.executable, str(ROOT / "dotnet.py"), str(source_path),
                                 "--output-dir", str(rejected)], text=True, capture_output=True)
        if result.returncode == 0 or "dot_gen_be: unsupported" not in result.stderr or rejected.exists():
            raise RuntimeError(f"Expected clean rejection of {label}: {result.stdout}\n{result.stderr}")
        checks.append(f"rejected {label} without committing output")
    # Failure must preserve a previously successful generated project.
    before = (directory / "Translated.cs").read_bytes()
    run([sys.executable, ROOT / "dotnet.py", out / "variadic-pointer.cpp", "--output-dir", directory], expected=1)
    assert (directory / "Translated.cs").read_bytes() == before
    checks.append("failed translation preserves previous output")
    # Regeneration removes owned split files only; determinism excludes paths
    # in the manifest and does not depend on IL pointer addresses.
    directory = out / "regeneration"
    run([sys.executable, ROOT / "dotnet.py", TESTS / "scalars.cpp", "--output-dir", directory,
         "--instance-methods", "--split", "function"])
    sentinel = directory / "User.cs"
    sentinel.write_text("// Must not be deleted or implicitly compiled.\n")
    command = [sys.executable, ROOT / "dotnet.py", TESTS / "scalars.cpp", "--output-dir", directory,
               "--instance-methods"]
    run(command)
    assert not list(directory.glob("Translated.part*.cs")) and sentinel.exists()
    before = (directory / "Translated.cs").read_bytes()
    run(command)
    assert (directory / "Translated.cs").read_bytes() == before
    checks.append("deterministic regeneration and owned split-file cleanup")
    source_path = out / "implicit-main.cpp"
    source_path.write_text("int main() {}\n")
    directory = out / "identifier-collision"
    run([sys.executable, ROOT / "dotnet.py", source_path, "--output-dir", directory, "--class-name", "f1"])
    run(["dotnet", "build", directory / "Generated.csproj", "-c", "Release", "--nologo"])
    run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
    result = run([sys.executable, ROOT / "dotnet.py", source_path, "--output-dir", directory,
                  "--class-name", "Run"], expected=2)
    assert "conflicts" in result.stderr
    checks.append("implicit main return and wrapper/member name collisions")
    from storage_verify import verify_storage
    checks.extend(verify_storage(ROOT, TESTS, out, args.nativeaot, run))
    from runtime_verify import verify_runtime
    checks.extend(verify_runtime(ROOT, TESTS, out, args.nativeaot, run))
    from lifetime_verify import verify_lifetimes
    checks.extend(verify_lifetimes(ROOT, TESTS, out, args.nativeaot, run))
    from array_temps_verify import verify_array_temps
    checks.extend(verify_array_temps(ROOT, TESTS, out, args.nativeaot, run))
    from return_objects_verify import verify_return_objects
    checks.extend(verify_return_objects(ROOT, TESTS, out, args.nativeaot, run))
    from unordered_verify import verify_unordered
    checks.extend(verify_unordered(ROOT, TESTS, out, args.nativeaot, run))
    from exceptions_verify import verify_exceptions
    checks.extend(verify_exceptions(ROOT, TESTS, out, args.nativeaot, run))
    from class_exceptions_verify import verify_class_exceptions
    checks.extend(verify_class_exceptions(ROOT, TESTS, out, args.nativeaot, run))
    from pointer_exceptions_verify import verify_pointer_exceptions
    checks.extend(verify_pointer_exceptions(ROOT, TESTS, out, args.nativeaot, run))
    from inheritance_verify import verify_inheritance
    checks.extend(verify_inheritance(ROOT, TESTS, out, args.nativeaot, run))
    from virtual_dispatch_verify import verify_virtual_dispatch
    checks.extend(verify_virtual_dispatch(ROOT, TESTS, out, args.nativeaot, run))
    from virtual_inheritance_verify import verify_virtual_inheritance
    checks.extend(verify_virtual_inheritance(ROOT, TESTS, out, args.nativeaot, run))
    from dynamic_cast_verify import verify_dynamic_cast
    checks.extend(verify_dynamic_cast(ROOT, TESTS, out, args.nativeaot, run))
    from reference_cast_verify import verify_reference_cast
    checks.extend(verify_reference_cast(ROOT, TESTS, out, args.nativeaot, run))
    from type_info_verify import verify_type_info
    checks.extend(verify_type_info(ROOT, TESTS, out, args.nativeaot, run))
    from basic_language_verify import verify_basic_language
    checks.extend(verify_basic_language(ROOT, TESTS, out, args.nativeaot, run))
    from initialization_allocation_verify import verify_initialization_allocation
    checks.extend(verify_initialization_allocation(ROOT, TESTS, out, args.nativeaot, run))
    from c_language_verify import verify_c_language
    checks.extend(verify_c_language(ROOT, TESTS, out, args.nativeaot, run))
    from function_try_verify import verify_function_try
    checks.extend(verify_function_try(ROOT, TESTS, out, args.nativeaot, run))
    from multi_tu_verify import verify_multi_tu
    checks.extend(verify_multi_tu(ROOT, TESTS, out, args.nativeaot, run))
    from libc_imports_verify import verify_libc_imports
    checks.extend(verify_libc_imports(ROOT, TESTS, out, args.nativeaot, run))
    from portable_target_verify import verify_portable_target
    checks.extend(verify_portable_target(ROOT, TESTS, out, args.nativeaot, run))
    from portable_include_verify import verify_portable_includes
    checks.extend(verify_portable_includes(ROOT, out, run))
    from template_entities_verify import verify_template_entities
    checks.extend(verify_template_entities(ROOT, TESTS, out, args.nativeaot, run))
    report = {"checks": checks, "nativeaot": args.nativeaot, "rid": "linux-x64",
              "edgRevision": "89e67e07c7f6d0fc38622f9778d8e79f40deaa0f"}
    (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
