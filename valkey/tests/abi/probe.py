#!/usr/bin/env python3
"""Compare pinned Valkey C layouts with an actual translated product assembly.

No source rewrites: dictEntry's private declaration is copied verbatim to this
probe's generated header, with its original file/declaration hashes in receipt.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import struct
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))
from inputs import prepare_inputs


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assembly", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if sys.platform != "linux" or sys.byteorder != "little" or struct.calcsize("P") != 8:
        parser.error("This native comparison qualifies the Linux little-endian LP64 profile only")
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    inputs = prepare_inputs(no_fetch=True)
    source = Path(inputs["source_root"])
    assembly = output / "TranslatedValkey.dll"
    shutil.copy2(args.assembly, assembly)
    dictionary = source / "src/dict.c"
    body = dictionary.read_text()
    begin = body.index("struct dictEntry {")
    end = body.index("\n};", begin) + 3
    declaration = body[begin:end]
    (output / "dict-entry.h").write_text(declaration + "\n")
    layouts = json.loads(Path(__file__).with_name("layouts.json").read_text())
    native = ['#include "server.h"', '#include "lstate.h"', '#include "dict-entry.h"', '#include <stddef.h>', '#include <stdio.h>', 'int main(void) {']
    managed = ['using System;', 'using System.Runtime.CompilerServices;', 'using Core = Managed.Database.ValkeyCore;', 'unsafe class Probe {', 'struct Alignment<T> where T : unmanaged { public byte first; public T value; }', 'static long Align<T>() where T : unmanaged { Alignment<T> value = default; return (byte*)Unsafe.AsPointer(ref value.value) - (byte*)Unsafe.AsPointer(ref value.first); }', 'static void Main() {']
    for index, layout in enumerate(layouts):
        c, cs = layout["c"], layout["cs"]
        native += [f'printf("{cs}.size %zu\\n{cs}.align %zu\\n", sizeof({c}), _Alignof({c}));']
        managed += [f'Core.{cs} value{index} = default;', f'Console.WriteLine("{cs}.size " + sizeof(Core.{cs}));', f'Console.WriteLine("{cs}.align " + Align<Core.{cs}>());']
        for field in layout.get("fields", []) + layout.get("flexible", []) + layout.get("fixed", []):
            native += [f'printf("{cs}.{field} %zu\\n", offsetof({c}, {field}));']
            address = f'value{index}.@{field}' if field in layout.get("flexible", []) + layout.get("fixed", []) else f'&value{index}.@{field}'
            managed += [f'Console.WriteLine("{cs}.{field} " + ((byte*)({address}) - (byte*)&value{index}));']
        for field in layout.get("promoted", []):
            native += [f'printf("{cs}.{field} %zu\\n", offsetof({c}, {field}));']
            managed += [f'value{index}.@{field} = 1; for(int offset=0;offset<sizeof(Core.{cs});offset++) if (((byte*)&value{index})[offset] != 0) {{ Console.WriteLine("{cs}.{field} " + offset); break; }} value{index}.@{field} = 0;']
    native += ['robj object = {0}; object.type=5; object.encoding=10; object.lru=0x123456; object.hasexpire=1; object.hasembkey=0; object.hasembval=1; object.refcount=0x1234567;', 'printf("serverObject.bitfields "); for(int i=0;i<8;i++) printf("%02x", ((unsigned char*)&object)[i]); printf("\\n");', 'return 0; }']
    managed += ['Core.serverObject obj = default; obj.type=5; obj.encoding=10; obj.lru=0x123456; obj.hasexpire=1; obj.hasembkey=0; obj.hasembval=1; obj.refcount=0x1234567;', 'Console.Write("serverObject.bitfields "); for(int i=0;i<8;i++) Console.Write(((byte*)&obj)[i].ToString("x2")); Console.WriteLine();', '} }']
    (output / "probe.c").write_text("\n".join(native) + "\n")
    (output / "Program.cs").write_text("\n".join(managed) + "\n")
    (output / "Probe.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="Program.cs"/><Reference Include="TranslatedValkey"><HintPath>TranslatedValkey.dll</HintPath></Reference></ItemGroup></Project>\n')
    receipt = {"schema_version": 1, "profile": "Linux little-endian LP64", "inputs": inputs, "assembly": str(args.assembly.resolve()), "assembly_sha256": sha(assembly), "private_declaration": {"path": "src/dict.c", "file_sha256": sha(dictionary), "declaration_sha256": hashlib.sha256(declaration.encode()).hexdigest()}, "commands": []}
    def run(command, log):
        receipt["commands"].append(command)
        with (output / log).open("w") as stream:
            result = subprocess.run(command, cwd=output, stdout=stream, stderr=subprocess.STDOUT, timeout=180)
        (output / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"Command failed: see {output / log}")
    includes = ["src", "src/trace", "deps/libvalkey/include", "deps/linenoise", "deps/hdr_histogram", "deps/fpconv", "deps/fast_float", "deps/lua/src"]
    run(["cc", "-std=c11", "-DLUA_ENABLED", "-DSTATIC_LUA=1", *[arg for include in includes for arg in ["-I", str(source / include)]], "probe.c", "-o", "native"], "native-build.log")
    run([str(output / "native")], "native.txt")
    run(["dotnet", "build", "Probe.csproj", "-c", "Release", "--nologo"], "managed-build.log")
    run(["dotnet", "bin/Release/net10.0/Probe.dll"], "managed.txt")
    native_text, managed_text = (output / "native.txt").read_text(), (output / "managed.txt").read_text()
    native_values = dict(line.split(" ", 1) for line in native_text.splitlines())
    managed_values = dict(line.split(" ", 1) for line in managed_text.splitlines())
    # pthread objects are managed registry handles, not native pthread storage.
    # Their internal containing layout is deliberately different; no such object
    # may be shared with a native library. Keep the exact expected deltas visible.
    approved = {"aeEventLoop.size": ("128", "88"), "aeEventLoop.flags": ("120", "84")}
    differences = [{"field": key, "native": native_values.get(key), "managed": managed_values.get(key),
                    "expected": approved.get(key) == (native_values.get(key), managed_values.get(key))}
                   for key in sorted(native_values.keys() | managed_values.keys())
                   if native_values.get(key) != managed_values.get(key)]
    receipt["differences"] = differences
    receipt["exact_match"] = not differences
    receipt["passed"] = not any(not difference["expected"] for difference in differences)
    receipt["checks"] = len(native_text.splitlines())
    if differences:
        import difflib
        (output / "difference.txt").write_text("".join(difflib.unified_diff(native_text.splitlines(True), managed_text.splitlines(True), fromfile="native", tofile="translated")))
    (output / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
    print(json.dumps({"passed": receipt["passed"], "checks": receipt["checks"], "expected_differences": len(differences), "receipt": str(output / "receipt.json")}))
    return 0 if receipt["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
