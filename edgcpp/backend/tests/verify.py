#!/usr/bin/env python3
"""Differential scalar tests, rejection checks and an optional real NativeAOT run."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent
command_log = []
evidence_directory = None


def run(command, expected=0, **kwargs):
    result = subprocess.run(list(map(str, command)), text=True, capture_output=True, **kwargs)
    if evidence_directory is not None:
        index=len(command_log)
        (evidence_directory/f"command-{index:04d}.stdout").write_text(result.stdout)
        (evidence_directory/f"command-{index:04d}.stderr").write_text(result.stderr)
        command_log.append({"command":list(map(str,command)),"exit":result.returncode,"expected":expected})
        (evidence_directory/'commands.json').write_text(json.dumps(command_log,indent=2)+'\n')
    if result.returncode != expected:
        raise RuntimeError(f"{command}\nexit={result.returncode}, expected={expected}\n{result.stdout}\n{result.stderr}")
    return result


def main():
    global evidence_directory
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--nativeaot", action="store_true")
    parser.add_argument("--libcxx-acquisition", type=Path, help="retained pinned libc++ source acquisition for synthetic ABI component tests")
    parser.add_argument("--output-dir", type=Path, default=ROOT / "artifacts" / f"backend-tests-{time.time_ns()}")
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=True)
    evidence_directory=out/'commands'
    evidence_directory.mkdir(exist_ok=False)
    class CheckpointChecks(list):
        def save(self):
            (out / "progress.json").write_text(json.dumps({"checks":self,"nativeaot":args.nativeaot},indent=2)+"\n")
        def append(self,item):
            super().append(item);self.save()
        def extend(self,items):
            super().extend(items);self.save()
    checks = CheckpointChecks()
    def suite_output(name):
        directory=out / "suites" / name
        directory.mkdir(parents=True,exist_ok=False)
        return directory
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
        "aggregate-storage-budget": "int main() { char data[67108865]; data[0]=1; return data[0]; }",
        "external-variable-reference": "extern int n; int main(){return n;}",
        "external-variable-address": "extern int n; int* p=&n; int main(){return *p;}",
        "external-delete-provider": "struct X { virtual ~X() {} }; int main() { try { throw X{}; } catch (...) { return 0; } }",
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
    checks.extend(verify_storage(ROOT, TESTS, suite_output("storage"), args.nativeaot, run))
    from runtime_verify import verify_runtime
    checks.extend(verify_runtime(ROOT, TESTS, suite_output("runtime"), args.nativeaot, run))
    from lifetime_verify import verify_lifetimes
    checks.extend(verify_lifetimes(ROOT, TESTS, suite_output("lifetimes"), args.nativeaot, run))
    from array_temps_verify import verify_array_temps
    checks.extend(verify_array_temps(ROOT, TESTS, suite_output("array_temps"), args.nativeaot, run))
    from return_objects_verify import verify_return_objects
    checks.extend(verify_return_objects(ROOT, TESTS, suite_output("return_objects"), args.nativeaot, run))
    from unordered_verify import verify_unordered
    checks.extend(verify_unordered(ROOT, TESTS, suite_output("unordered"), args.nativeaot, run))
    from exceptions_verify import verify_exceptions
    checks.extend(verify_exceptions(ROOT, TESTS, suite_output("exceptions"), args.nativeaot, run))
    from class_exceptions_verify import verify_class_exceptions
    checks.extend(verify_class_exceptions(ROOT, TESTS, suite_output("class_exceptions"), args.nativeaot, run))
    from pointer_exceptions_verify import verify_pointer_exceptions
    checks.extend(verify_pointer_exceptions(ROOT, TESTS, suite_output("pointer_exceptions"), args.nativeaot, run))
    from inheritance_verify import verify_inheritance
    checks.extend(verify_inheritance(ROOT, TESTS, suite_output("inheritance"), args.nativeaot, run))
    from virtual_dispatch_verify import verify_virtual_dispatch
    checks.extend(verify_virtual_dispatch(ROOT, TESTS, suite_output("virtual_dispatch"), args.nativeaot, run))
    from virtual_inheritance_verify import verify_virtual_inheritance
    checks.extend(verify_virtual_inheritance(ROOT, TESTS, suite_output("virtual_inheritance"), args.nativeaot, run))
    from dynamic_cast_verify import verify_dynamic_cast
    checks.extend(verify_dynamic_cast(ROOT, TESTS, suite_output("dynamic_cast"), args.nativeaot, run))
    from reference_cast_verify import verify_reference_cast
    checks.extend(verify_reference_cast(ROOT, TESTS, suite_output("reference_cast"), args.nativeaot, run))
    from type_info_verify import verify_type_info
    checks.extend(verify_type_info(ROOT, TESTS, suite_output("type_info"), args.nativeaot, run))
    from basic_language_verify import verify_basic_language
    checks.extend(verify_basic_language(ROOT, TESTS, suite_output("basic_language"), args.nativeaot, run))
    from initialization_allocation_verify import verify_initialization_allocation
    checks.extend(verify_initialization_allocation(ROOT, TESTS, suite_output("initialization_allocation"), args.nativeaot, run))
    from c_language_verify import verify_c_language
    checks.extend(verify_c_language(ROOT, TESTS, suite_output("c_language"), args.nativeaot, run))
    from function_try_verify import verify_function_try
    checks.extend(verify_function_try(ROOT, TESTS, suite_output("function_try"), args.nativeaot, run))
    from multi_tu_verify import verify_multi_tu
    checks.extend(verify_multi_tu(ROOT, TESTS, suite_output("multi_tu"), args.nativeaot, run))
    from libc_imports_verify import verify_libc_imports
    checks.extend(verify_libc_imports(ROOT, TESTS, suite_output("libc_imports"), args.nativeaot, run))
    from portable_target_verify import verify_portable_target
    checks.extend(verify_portable_target(ROOT, TESTS, suite_output("portable_target"), args.nativeaot, run))
    from portable_include_verify import verify_portable_includes
    checks.extend(verify_portable_includes(ROOT, out, run))
    from template_entities_verify import verify_template_entities
    checks.extend(verify_template_entities(ROOT, TESTS, suite_output("template_entities"), args.nativeaot, run))
    from varargs_verify import verify_varargs
    checks.extend(verify_varargs(ROOT, TESTS, suite_output("varargs"), args.nativeaot, run))
    from exception_storage_verify import verify_exception_storage
    checks.extend(verify_exception_storage(ROOT, TESTS, suite_output("exception_storage"), args.nativeaot, run))
    from libc_files_verify import verify_libc_files
    checks.extend(verify_libc_files(ROOT, TESTS, suite_output("libc_files"), args.nativeaot, run))
    from atomic_builtins_verify import verify_atomic_builtins
    checks.extend(verify_atomic_builtins(ROOT, TESTS, suite_output("atomic_builtins"), args.nativeaot, run))
    from constant_intrinsics_verify import verify_constant_intrinsics
    checks.extend(verify_constant_intrinsics(ROOT, TESTS, suite_output("constant_intrinsics"), args.nativeaot, run))
    from template_packs_verify import verify_template_packs
    checks.extend(verify_template_packs(ROOT, TESTS, suite_output("template_packs"), args.nativeaot, run))
    from language_modes_verify import verify_language_modes
    checks.extend(verify_language_modes(ROOT, TESTS, suite_output("language_modes"), args.nativeaot, run))
    from member_pointers_verify import verify_member_pointers
    checks.extend(verify_member_pointers(ROOT, TESTS, suite_output("member_pointers"), args.nativeaot, run))
    from member_type_info_verify import verify_member_type_info
    checks.extend(verify_member_type_info(ROOT, TESTS, suite_output("member_type_info"), args.nativeaot, run))
    from materialize_verify import verify_materialize
    checks.extend(verify_materialize(ROOT, TESTS, suite_output("materialize"), args.nativeaot, run))
    from extended_pointer_exceptions_verify import verify_extended_pointer_exceptions
    checks.extend(verify_extended_pointer_exceptions(ROOT, TESTS, suite_output("extended_pointer_exceptions"), args.nativeaot, run))
    from portable_integer_headers_verify import verify_portable_libc_headers
    checks.extend(verify_portable_libc_headers(ROOT, TESTS, suite_output("portable_libc_headers"), args.nativeaot, run))
    from musl_printf_translated_verify import verify_musl_printf
    checks.extend(verify_musl_printf(ROOT, TESTS, suite_output("musl_printf"), args.nativeaot, run))
    from libcxx_threads_verify import verify_libcxx_threads
    checks.extend(verify_libcxx_threads(ROOT, TESTS, suite_output("libcxx_threads"), args.nativeaot, run))
    from libcxx_threads_fatal_verify import verify_libcxx_threads_fatal
    checks.extend(verify_libcxx_threads_fatal(ROOT, TESTS, out / 'libcxx-threads-fatal', args.nativeaot, run))
    from libc_import_abi_verify import verify_libc_import_abi
    checks.extend(verify_libc_import_abi(ROOT, TESTS, out / "libc-import-abi"))
    from main_arguments_verify import verify_main_arguments
    checks.extend(verify_main_arguments(ROOT, TESTS, suite_output("main_arguments"), args.nativeaot, run))
    from libc_math_translated_verify import verify_libc_math
    checks.extend(verify_libc_math(ROOT, TESTS, suite_output("libc_math"), args.nativeaot, run))
    from libc_math_abi_verify import verify_libc_math_abi
    checks.extend(verify_libc_math_abi(ROOT, TESTS, out / "libc-math-abi"))
    from large_aggregates_verify import verify_large_aggregates
    checks.extend(verify_large_aggregates(ROOT, TESTS, suite_output("large_aggregates"), args.nativeaot, run))
    from anonymous_records_verify import verify_anonymous_records
    checks.extend(verify_anonymous_records(ROOT, TESTS, suite_output("anonymous_records"), args.nativeaot, run))
    from libc_numeric_verify import verify_libc_numeric
    checks.extend(verify_libc_numeric(ROOT, TESTS, suite_output("libc_numeric"), args.nativeaot, run))
    from type_info_fields_verify import verify_type_info_fields
    checks.extend(verify_type_info_fields(ROOT, TESTS, suite_output("type_info_fields"), args.nativeaot, run))
    from void_constants_verify import verify_void_constants
    checks.extend(verify_void_constants(ROOT, TESTS, suite_output("void_constants"), args.nativeaot, run))
    from floating_intrinsics_verify import verify_floating_intrinsics
    checks.extend(verify_floating_intrinsics(ROOT, TESTS, suite_output("floating_intrinsics"), args.nativeaot, run))
    from musl_provider_verify import verify_musl_provider
    checks.extend(verify_musl_provider(ROOT, TESTS, suite_output("musl_provider"), args.nativeaot, run))
    from libc_locale_translated_verify import verify_libc_locale
    checks.extend(verify_libc_locale(ROOT, TESTS, suite_output("libc_locale"), args.nativeaot, run))
    from packed_bitfields_verify import verify_packed_bitfields
    checks.extend(verify_packed_bitfields(ROOT, TESTS, suite_output("packed_bitfields"), args.nativeaot, run))
    from vla_verify import verify_vla
    checks.extend(verify_vla(ROOT, TESTS, suite_output("vla"), args.nativeaot, run))
    from vla_alignment_verify import verify_vla_alignment
    checks.extend(verify_vla_alignment(ROOT, TESTS, suite_output("vla_alignment"), args.nativeaot, run))
    from restrict_qualifiers_verify import verify_restrict_qualifiers
    checks.extend(verify_restrict_qualifiers(ROOT, TESTS, suite_output("restrict_qualifiers"), args.nativeaot, run))
    from union_constant_members_verify import verify_union_constant_members
    checks.extend(verify_union_constant_members(ROOT, TESTS, suite_output("union_constant_members"), args.nativeaot, run))
    from libc_ctype_verify import verify_libc_ctype
    checks.extend(verify_libc_ctype(ROOT, TESTS, suite_output("libc_ctype"), args.nativeaot, run))
    from libc_clock_verify import verify_libc_clock
    checks.extend(verify_libc_clock(ROOT, TESTS, suite_output("libc_clock"), args.nativeaot, run))
    from libc_clock_abi_verify import verify_libc_clock_abi
    checks.extend(verify_libc_clock_abi(ROOT, TESTS, suite_output("libc_clock_abi")))
    from libc_wide_verify import verify_libc_wide
    checks.extend(verify_libc_wide(ROOT, TESTS, suite_output("libc_wide"), args.nativeaot, run))
    from array_pointer_compat_verify import verify_array_pointer_compat
    checks.extend(verify_array_pointer_compat(ROOT, TESTS, suite_output("array_pointer_compat"), args.nativeaot, run))
    from physical_types_verify import verify_physical_types
    checks.extend(verify_physical_types(ROOT, TESTS, suite_output("physical_types"), args.nativeaot, run))
    from libc_multibyte_verify import verify_libc_multibyte
    checks.extend(verify_libc_multibyte(ROOT, TESTS, suite_output("libc_multibyte"), args.nativeaot, run))
    from libc_wide_numeric_verify import verify_libc_wide_numeric
    checks.extend(verify_libc_wide_numeric(ROOT, TESTS, suite_output("libc_wide_numeric"), args.nativeaot, run))
    from libc_calendar_verify import verify_libc_calendar
    checks.extend(verify_libc_calendar(ROOT, TESTS, suite_output("libc_calendar"), args.nativeaot, run))
    from libc_wide_calendar_verify import verify_libc_wide_calendar
    checks.extend(verify_libc_wide_calendar(ROOT, TESTS, suite_output("libc_wide_calendar"), args.nativeaot, run))
    from libc_stdlib_algorithms_verify import verify_libc_stdlib_algorithms
    checks.extend(verify_libc_stdlib_algorithms(ROOT, TESTS, suite_output("libc_stdlib_algorithms"), args.nativeaot, run))
    from libc_string_builtins_verify import verify_libc_string_builtins
    checks.extend(verify_libc_string_builtins(ROOT, TESTS, suite_output("libc_string_builtins"), args.nativeaot, run))
    from libcxx_atomic_wait_verify import verify_libcxx_atomic_wait
    checks.extend(verify_libcxx_atomic_wait(ROOT, TESTS, suite_output("libcxx_atomic_wait"), args.nativeaot, run))
    from diagnostics_verify import verify_diagnostics
    checks.extend(verify_diagnostics(ROOT, TESTS, suite_output("diagnostics"), run))
    if args.libcxx_acquisition:
        from libcxx_exception_ptr_verify import verify_libcxx_exception_ptr
        checks.extend(verify_libcxx_exception_ptr(ROOT, TESTS, suite_output("libcxx_exception_ptr"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_exception_owner_verify import verify_libcxx_exception_owner
        checks.extend(verify_libcxx_exception_owner(ROOT, TESTS, suite_output("libcxx_exception_owner"), args.nativeaot, run, args.libcxx_acquisition))
        checks.extend(verify_libcxx_exception_owner(ROOT, TESTS, suite_output("libcxx_exception_foreign"), args.nativeaot, run, args.libcxx_acquisition,foreign=True,allocation_free=True))
        from libcxx_atomic_protocol_verify import verify_libcxx_atomic_protocol
        checks.extend(verify_libcxx_atomic_protocol(ROOT, TESTS, suite_output("libcxx_atomic_protocol"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_exception_counts_verify import verify_libcxx_exception_counts
        checks.extend(verify_libcxx_exception_counts(ROOT, TESTS, suite_output("libcxx_exception_counts"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_terminate_verify import verify_libcxx_terminate
        checks.extend(verify_libcxx_terminate(ROOT, TESTS, suite_output("libcxx_terminate"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_terminate_boundaries_verify import verify_libcxx_terminate_boundaries
        checks.extend(verify_libcxx_terminate_boundaries(ROOT, TESTS, suite_output("libcxx_terminate_boundaries"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_dynamic_specs_verify import verify_libcxx_dynamic_specs
        checks.extend(verify_libcxx_dynamic_specs(ROOT, TESTS, suite_output("libcxx_dynamic_specs"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_dynamic_specs_fatal_verify import verify_libcxx_dynamic_specs_fatal
        checks.extend(verify_libcxx_dynamic_specs_fatal(ROOT, TESTS, suite_output("libcxx_dynamic_specs_fatal"), args.nativeaot, run, args.libcxx_acquisition))
        from libcxx_dynamic_specs_lifetime_verify import verify_libcxx_dynamic_specs_lifetime
        checks.extend(verify_libcxx_dynamic_specs_lifetime(ROOT, TESTS, suite_output("libcxx_dynamic_specs_lifetime"), args.nativeaot, run, args.libcxx_acquisition))
    report = {"checks": checks, "nativeaot": args.nativeaot, "rid": "linux-x64",
              "edgRevision": "89e67e07c7f6d0fc38622f9778d8e79f40deaa0f"}
    (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
