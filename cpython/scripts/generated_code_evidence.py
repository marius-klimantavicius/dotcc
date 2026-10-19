#!/usr/bin/env python3
"""Inventory immutable payloads and prepare an authored JIT/AOT allocation probe.

This never edits translated sources, builds tools, or publishes a product. The
normal compiler/framework owns those operations. GeneratedDataAudit inspects the
actual Release PE against this source/receipt-bound manifest.
"""
import argparse
import hashlib
import json
import re
from collections import Counter
from bisect import bisect_right
from pathlib import Path
from xml.sax.saxutils import escape


WIDTHS = {"byte": 1, "sbyte": 1, "ushort": 2, "short": 2, "char": 2,
          "uint": 4, "int": 4, "float": 4, "ulong": 8, "long": 8, "double": 8, "CBool": 1}


def prepare_typed(source, receipt, output, probe=None, holder=None, project=None, owner=None,
                  reference=None, native_template=None):
    """Extend the byte inventory with exact fixed-width backings and typed views.

    This is preparation only. The PE reader and executable probe independently
    validate the representation, width, bytes and allocation behavior later.
    The legacy byte-only entrypoint below remains available to historical proofs.
    """
    if not holder or not owner:
        raise ValueError("typed inventories require actual owner and literal-holder types")
    owner = owner.removeprefix("global::")
    holder = holder.removeprefix("global::")
    holder_metadata = owner + "+" + holder.removeprefix(owner + ".")
    names_file = source / "Dotcc.SourceFiles.txt"
    names = names_file.read_text().splitlines()
    if not names or len(names) != len(set(names)):
        raise ValueError("missing/duplicate generated source closure")
    paths = [(source / name).resolve() for name in names]
    if any(not path.is_relative_to(source.resolve()) or path.suffix != ".cs" for path in paths):
        raise ValueError("invalid generated source path")
    files, properties, views, copies, recipes, rows = [], {}, [], [], [], []
    aliases, totals = {}, Counter()
    for path in paths:
        text = path.read_text(encoding="utf-8")
        newlines = [match.start() for match in re.finditer("\n", text)]
        aliases.update(re.findall(r"using (\w+) = (global::[\w.]+);", text))
        counts = {
            "constant_AsSpan_new_byte_sites": len(re.findall(r"\bAsSpan\s*\(\s*new\s+(?:global::System\.)?byte\s*\[", text)),
            "allocating_AsSpan_payload_sites": len(re.findall(r"\bAsSpan\s*\(\s*new\s+[\w.:<>]+\s*\[", text)),
            "PyRuntime_base_ref_mentions": text.count("ref global::" + owner + ".Globals._PyRuntime"),
            "tracked_address_locals": len(re.findall(r"ref \w+ __dotcc_address\d+ = ref ", text)),
            "pinned_initializer_chunks": len(re.findall(
                r"\bfixed\s*\(\s*[\w.:<>]+\s*\*\s*@?(?:__dotcc_staticBase|\w+_destination)\s*=", text)),
            "lazy_tracked_address_caches": len(re.findall(r"__DotccGlobalAddressCache\s+__dotcc_\w+\s*=\s*default", text)),
            "indexed_record_rows": len(re.findall(r"\b\w+_destination\[\d+\]\s*=\s*new\(\)", text)),
        }
        totals.update(counts)
        files.append({"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), **counts})
        pattern = r"(?:internal|private) static (?:global::System\.)?ReadOnlySpan<(byte|ushort|uint|ulong)> ((?:B|P(?:ushort|uint|ulong)_)[0-9A-F]{64})\s*=>\s*\[([^\]]*)\]\s*;"
        for match in re.finditer(pattern, text):
            element, name, initializer = match.groups()
            width = WIDTHS[element]
            if re.sub(r"0x[0-9A-F]+(?:UL|U)?|[\s,]", "", initializer):
                raise ValueError("nonconstant initializer " + name)
            values = re.findall(r"0x([0-9A-F]+)(?:UL|U)?", initializer)
            data = b"".join(int(value, 16).to_bytes(width, "little") for value in values)
            sha = hashlib.sha256(data).hexdigest()
            if name[-64:].lower() != sha or name in properties or not values or len(values) > 4096:
                raise ValueError("duplicate, oversized or incorrectly named payload " + name)
            properties[name] = {"name": name, "owner": holder_metadata, "elementType": element,
                                "elementWidth": width, "elements": len(values), "length": len(data),
                                "sha256": sha, "source": str(path), "line": bisect_right(newlines, match.start()) + 1}
        view_pattern = (r"internal static class (__DotccPrimitiveView_[0-9A-F]{64})\s*\{\s*"
                        r"public static (?:global::System\.)?ReadOnlySpan<([^>]+)> Values\s*=>\s*"
                        r"global::System.Runtime.InteropServices.MemoryMarshal.Cast<([^,]+),\s*([^>]+)>"
                        r"\(DotCcLiterals\.([\w]+)\);\s*\}")
        for match in re.finditer(view_pattern, text):
            view, target, backing, cast_target, payload = match.groups()
            if target.strip() != cast_target.strip():
                raise ValueError("typed view conversion mismatch")
            views.append({"name": view + ".Values", "owner": owner + "+" + view, "method": "get_Values",
                          "elementType": target.strip(), "backingType": backing.strip(), "backing": payload,
                          "source": str(path), "line": bisect_right(newlines, match.start()) + 1})
        for number, line in enumerate(text.splitlines(), 1):
            if ".CopyTo(new global::System.Span<" in line:
                copies.append({"source": str(path), "line": number, "expression": line.strip()})
            if re.search(r"\b(?:private|public|internal)\s+(?:static\s+)?(?:unsafe\s+)?\w+\*\s+__dotcc_owned_array_", line):
                recipes.append({"source": str(path), "line": number, "declaration": line.strip()})
            if "_destination[" in line and " = new()" in line and len(rows) < 20:
                rows.append({"source": str(path), "line": number, "expression": line.strip()})
    for view in views:
        backing = properties[view["backing"]]
        width = WIDTHS.get(view["elementType"], backing["elementWidth"])
        if width != backing["elementWidth"] or view["backingType"] != backing["elementType"]:
            raise ValueError("typed view changes scalar width")
        view.update(elementWidth=width, elements=backing["elements"], length=backing["length"], sha256=backing["sha256"])
        target = view["elementType"]
        view["csharpType"] = target if target in WIDTHS and target != "CBool" else aliases.get(target, "global::" + owner + "." + target)
    totals.update(constant_properties=len(properties), typed_views=len(views), payload_bytes=sum(p["length"] for p in properties.values()))
    report = {"schemaVersion": 2, "scope": "typed source preparation only; actual IL/RVA/native/allocation proof required",
              "translationReceipt": str(receipt.resolve()), "receiptSha256": hashlib.sha256(receipt.read_bytes()).hexdigest(),
              "source": str(source.resolve()), "files": files, "properties": list(properties.values()), "typedViews": views,
              "totals": dict(totals), "propertyTypes": dict(Counter(p["elementType"] for p in properties.values())),
              "viewTypes": dict(Counter(p["elementType"] for p in views)), "copySites": copies,
              "ownedRecipes": recipes, "recordRowExamples": rows,
              "unsupported": {"Half": "no backend source type", "Int128/UInt128/Float128/Complex": "scalar-store fallback; no primitive blob", "long double": "backend binary64; native extended precision is distinct"}}
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    if probe is not None:
        if project is None or reference is None:
            raise ValueError("typed executable probes require exact product project and reference PE")
        _typed_probe(report, output, probe, project, reference, owner, native_template)
    return report


def _typed_probe(report, inventory, probe, project, reference, owner, native_template=None):
    """Bind closed typed delegates once; measure only repeated access and copy."""
    import xml.etree.ElementTree as ET
    probe.mkdir(parents=True, exist_ok=True)
    assembly = ET.parse(project).getroot().findtext(".//AssemblyName") or project.stem
    ref_xml = escape(str(reference.resolve()), {'"': '&quot;'})
    (probe / "AllocationProbe.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
  <ItemGroup><Compile Include="Program.cs" /><Compile Include="ProductTables.cs" /><Reference Include="''' + assembly + '''"><HintPath>''' + ref_xml + '''</HintPath><Private>true</Private></Reference></ItemGroup>
</Project>
''', encoding="utf-8")
    observations = [(p, p["elementType"], p["name"], "properties") for p in report["properties"]]
    observations += [(p, p["csharpType"], "Values", "views") for p in report["typedViews"]]
    calls = []
    for p, scalar, prop, group in observations:
        calls.append(f'        Measure<{scalar}>("{p["name"]}", Bind<{scalar}>("{p["owner"]}", "get_{prop}"), {p["length"]}, {p["elementWidth"]}, "{p["sha256"]}", "{group}");')
    groups = [calls[i:i + 64] for i in range(0, len(calls), 64)]
    roots = "\n".join('    [System.Diagnostics.CodeAnalysis.DynamicDependency(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, "'
                      + name.replace("+", ".") + '", "' + assembly + '")]' for name in sorted({p[0]["owner"] for p in observations}))
    program = '''using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
internal static partial class AllocationProbe {
    private delegate ReadOnlySpan<T> Read<T>() where T : unmanaged;
    private sealed record Observation(string Name, int Length, int Width, long Allocated, string Sha256);
    private static readonly List<Observation> Properties = new(), Views = new();
ROOTS
    private static void Main(string[] args) {
        if (!BitConverter.IsLittleEndian) throw new Exception("selected target is little-endian");
        if (args.Length == 2) CheckNativeTables(args[1]);
MAIN_CALLS
        using var output = File.Create(args[0]);
        using var json = new Utf8JsonWriter(output, new JsonWriterOptions { Indented=true });
        json.WriteStartObject(); json.WriteString("status", "passed");
        json.WriteBoolean("dynamicCode", RuntimeFeature.IsDynamicCodeSupported);
        json.WriteString("productAssembly", typeof(OWNER).Assembly.FullName);
        json.WriteString("productMvid", typeof(OWNER).Module.ModuleVersionId);
        Write("properties", Properties); Write("views", Views);
        json.WriteStartArray("nativeTables");
        foreach (var table in NativeTables) {
            json.WriteStartObject(); json.WriteString("name", table.Name); json.WriteNumber("length", table.Length);
            json.WriteString("address", table.Address); json.WriteString("sha256", table.Sha256);
            json.WriteBoolean("nativeBytes", true); json.WriteEndObject();
        }
        json.WriteEndArray();
        json.WriteString("scope", "all exact typed getters/views and warmed copy; required C destination and pointer-pool lifetime allocations excluded");
        json.WriteEndObject();
        Console.WriteLine($"{Properties.Count} backings + {Views.Count} typed views: exact bytes, zero warmed allocations");
        void Write(string name, List<Observation> observations) {
            json.WriteStartArray(name);
            foreach (var row in observations) {
                json.WriteStartObject(); json.WriteString("name", row.Name); json.WriteNumber("length", row.Length);
                json.WriteNumber("width", row.Width); json.WriteNumber("allocated", row.Allocated);
                json.WriteNumber("iterations", 10000); json.WriteString("sha256", row.Sha256); json.WriteEndObject();
            }
            json.WriteEndArray();
        }
    }
    private static Read<T> Bind<T>(string owner, string name) where T : unmanaged {
        Type type = Type.GetType(owner + ", ASSEMBLY", throwOnError:true)!;
        return type.GetMethod(name, BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)!.CreateDelegate<Read<T>>();
    }
    private static unsafe void Measure<T>(string name, Read<T> read, int length, int width, string expected, string group) where T : unmanaged {
        if (sizeof(T) != width || read().Length * sizeof(T) != length) throw new Exception(name + " width/extent");
        byte[] destination = new byte[length];
        Copy(read, destination, 200);
        long before = GC.GetAllocatedBytesForCurrentThread();
        Copy(read, destination, 10000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        string sha = Convert.ToHexString(SHA256.HashData(destination)).ToLowerInvariant();
        if (allocated != 0 || sha != expected) throw new Exception($"{name}: allocated={allocated}, sha={sha}");
        (group == "properties" ? Properties : Views).Add(new Observation(name, length, width, allocated, sha));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy<T>(Read<T> read, byte[] destination, int count) where T : unmanaged {
        for (int i=0;i<count;i++) MemoryMarshal.AsBytes(read()).CopyTo(destination);
    }
GROUPS
}
'''.replace("ROOTS", roots).replace("OWNER", "global::" + owner).replace("ASSEMBLY", assembly)
    program = program.replace("MAIN_CALLS", "\n".join(f"        Group{i}();" for i in range(len(groups))))
    program = program.replace("GROUPS", "\n".join(f"    private static void Group{i}() {{\n" + "\n".join(group) + "\n    }" for i, group in enumerate(groups)))
    (probe / "Program.cs").write_text(program, encoding="utf-8")
    template = Path(native_template) if native_template else Path(__file__).resolve().parents[1] / "NativeOracle/GeneratedDataAudit/ProductTables.cs.txt"
    (probe / "ProductTables.cs").write_text(template.read_text().replace("PRODUCT_OWNER", "global::" + owner), encoding="utf-8")
    def item(path):
        return {"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
    (probe / "probe-inputs.json").write_text(json.dumps({
        "mode": "exact actual product PE reference; product is not rebuilt by the probe", "project": item(project),
        "reference": item(reference), "inventory": item(inventory), "program": item(probe / "Program.cs"),
        "productTables": item(probe / "ProductTables.cs"),
        "probeProject": item(probe / "AllocationProbe.csproj"), "properties": len(report["properties"]), "views": len(report["typedViews"])
    }, indent=2) + "\n", encoding="utf-8")


def prepare(source, receipt, output, probe=None, holder=None, project=None, owner=None):
    files, properties, examples = [], {}, []
    arrays = bases = refs = pins = lazy_caches = 0
    for path in sorted(source.glob("*.cs")):
        text = path.read_text(encoding="utf-8")
        sites = len(re.findall(r"MemoryExtensions\.AsSpan\(new byte\[\]", text))
        base_sites = text.count("ref global::Managed.Interpreters.CPython.Globals._PyRuntime")
        local_refs = len(re.findall(r"ref \w+ __dotcc_address\d+ = ref ", text))
        local_pins = text.count("* __dotcc_staticBase = &")
        local_lazy_caches = len(re.findall(r"__DotccGlobalAddressCache\s+__dotcc_(?:address\d+|initializerAddress_[0-9A-F]+)\s*=\s*default", text))
        arrays += sites
        bases += base_sites
        refs += local_refs
        pins += local_pins
        lazy_caches += local_lazy_caches
        files.append({"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                      "constant_AsSpan_new_byte_sites": sites, "PyRuntime_base_ref_mentions": base_sites,
                      "tracked_address_locals": local_refs, "pinned_initializer_chunks": local_pins,
                      "lazy_tracked_address_caches": local_lazy_caches})
        for match in re.finditer(r"internal static global::System\.ReadOnlySpan<byte> (B[0-9A-F]{64}) => \[([^\]]*)\];", text):
            name, initializer = match.groups()
            values = re.findall(r"0x([0-9A-F]{2})", initializer)
            if re.sub(r"0x[0-9A-F]{2}|[\s,]", "", initializer):
                raise ValueError("nonconstant payload " + name)
            data = bytes(int(value, 16) for value in values)
            digest = hashlib.sha256(data).hexdigest()
            if name[1:].lower() != digest or name in properties:
                raise ValueError("duplicate or non-content-addressed payload " + name)
            properties[name] = {"name": name, "length": len(data), "sha256": digest, "source": str(path.resolve())}
        if len(examples) < 8:
            for number, line in enumerate(text.splitlines(), 1):
                if "__dotcc_staticBase = &" in line or re.search(r"ref \w+ __dotcc_address\d+ = ref ", line):
                    examples.append({"path": str(path.resolve()), "line": number, "text": line.strip()[:400]})
                    if len(examples) == 8:
                        break
    report = {"scope": "source inventory and allocation-probe preparation; no qualification by syntax alone",
              "translationReceipt": str(receipt.resolve()), "receiptSha256": hashlib.sha256(receipt.read_bytes()).hexdigest(),
              "source": str(source.resolve()), "files": files, "properties": list(properties.values()),
              "totals": {"constant_AsSpan_new_byte_sites": arrays, "PyRuntime_base_ref_mentions": bases,
                         "tracked_address_locals": refs, "pinned_initializer_chunks": pins,
                         "lazy_tracked_address_caches": lazy_caches,
                         "constant_properties": len(properties), "payload_bytes": sum(value["length"] for value in properties.values())},
              "address_examples": examples}
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    if probe is not None:
        if not holder or not properties:
            raise ValueError("a typed holder and actual generated constant properties are required")
        probe.mkdir(parents=True, exist_ok=True)
        if project is not None and not owner:
            raise ValueError("actual-assembly probes require the public product owner type")
        includes = ('    <ProjectReference Include="' + escape(str(project.resolve()), {'"': '&quot;'}) + '" />') if project is not None else "\n".join(
            '    <Compile Include="' + escape(value["path"], {'"': '&quot;'}) + '" />' for value in files)
        (probe / "AllocationProbe.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>disable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings></PropertyGroup>
  <ItemGroup>
''' + includes + '\n  </ItemGroup>\n</Project>\n', encoding="utf-8")
        calls = []
        for value in properties.values():
            accessor = ('Bind("' + value["name"] + '")') if project is not None else ('static () => ' + holder + '.' + value["name"])
            calls.append('        Measure("' + value["name"] + '", ' + accessor
                         + ', ' + str(value["length"]) + ', "' + value["sha256"] + '");')
        # Keep this authored probe bounded too. Properties are consumed through
        # normal typed references, so AOT reflection/trimming cannot hide them.
        groups = [calls[index:index + 80] for index in range(0, len(calls), 80)]
        methods = "\n".join("    private static void Group" + str(index) + "() {\n" + "\n".join(group) + "\n    }" for index, group in enumerate(groups))
        main_calls = "\n".join("        Group" + str(index) + "();" for index in range(len(groups)))
        program = '''using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
internal static class AllocationProbe {
    private delegate ReadOnlySpan<byte> Read();
    private static readonly List<(string Name, int Length, long Allocated, string Hash)> Results = new();
ROOTS
    private static void Main(string[] args) {
MAIN_CALLS
        using var stream = File.Create(args[0]);
        using var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        json.WriteStartObject();
        json.WriteString("status", "passed");
        json.WriteBoolean("dynamicCode", RuntimeFeature.IsDynamicCodeSupported);
        json.WriteString("framework", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
ASSEMBLY_IDENTITY
        json.WriteString("scope", "warmed constant payload access/copy; destination and owned literal-pool startup allocations excluded");
        json.WriteStartArray("properties");
        foreach (var result in Results) {
            json.WriteStartObject(); json.WriteString("name", result.Name);
            json.WriteNumber("length", result.Length); json.WriteNumber("allocated", result.Allocated);
            json.WriteNumber("iterations", 10000); json.WriteString("sha256", result.Hash); json.WriteEndObject();
        }
        json.WriteEndArray(); json.WriteEndObject();
        Console.WriteLine($"{Results.Count} properties: exact bytes, zero warmed access/copy allocations; dynamicCode={RuntimeFeature.IsDynamicCodeSupported}");
    }
    private static void Measure(string name, Read read, int length, string expected) {
        byte[] destination = new byte[length];
        Copy(read, destination, 200);
        long before = GC.GetAllocatedBytesForCurrentThread();
        Copy(read, destination, 10000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        string hash = Convert.ToHexString(SHA256.HashData(destination));
        if (allocated != 0 || !hash.Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new Exception($"{name}: allocated={allocated}, hash={hash}");
        Results.Add((name, length, allocated, hash));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy(Read read, byte[] destination, int count) {
        for (int index = 0; index < count; index++) read().CopyTo(destination);
    }
GROUP_METHODS
BIND_GETTER
}
'''.replace("MAIN_CALLS", main_calls).replace("GROUP_METHODS", methods)
        if project is not None:
            # Consume the actual project assembly, including under NativeAOT.
            # Internal methods are explicitly rooted, then bound once to the
            # exact ref-struct-returning delegate signature before measurement.
            # No reflection Invoke/boxing can distort access/copy allocations.
            owner_name = owner.removeprefix("global::")
            holder_name = holder.removeprefix("global::")
            if holder_name.startswith(owner_name + "."):
                holder_name = owner_name + "+" + holder_name[len(owner_name) + 1:]
            import xml.etree.ElementTree as ET
            assembly_name = ET.parse(project).getroot().findtext(".//AssemblyName") or project.stem
            roots = ('    [System.Diagnostics.CodeAnalysis.DynamicDependency(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, "'
                     + holder_name.replace("+", ".") + '", "' + assembly_name + '")]')
            bind = '''    private static Read Bind(string name) {
        var holder = GetHolder();
        var method = holder.GetMethod("get_" + name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        return method.CreateDelegate<Read>();
    }
    [return: System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)]
    private static Type GetHolder() => Type.GetType("HOLDER, ASSEMBLY", throwOnError: true);
'''.replace("HOLDER", holder_name).replace("ASSEMBLY", assembly_name)
            identity = '        json.WriteString("productAssembly", typeof(' + owner + ').Assembly.FullName);'
        else:
            roots = bind = identity = ""
        program = program.replace("ROOTS", roots).replace("BIND_GETTER", bind).replace("ASSEMBLY_IDENTITY", identity)
        (probe / "Program.cs").write_text(program, encoding="utf-8")
        (probe / "probe-inputs.json").write_text(json.dumps({
            "mode": "actual product project reference" if project is not None else "focused compiler source control",
            "inventory": str(output.resolve()), "inventory_sha256": hashlib.sha256(output.read_bytes()).hexdigest(),
            "project": str(project.resolve()) if project is not None else None,
            "project_sha256": hashlib.sha256(project.read_bytes()).hexdigest() if project is not None else None,
            "owner": owner, "holder": holder, "properties": len(properties),
            "program_sha256": hashlib.sha256((probe / "Program.cs").read_bytes()).hexdigest(),
            "probe_project_sha256": hashlib.sha256((probe / "AllocationProbe.csproj").read_bytes()).hexdigest(),
        }, indent=2) + "\n", encoding="utf-8")
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--receipt", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--probe", type=Path)
    parser.add_argument("--holder")
    parser.add_argument("--project", type=Path)
    parser.add_argument("--owner")
    parser.add_argument("--typed", action="store_true", help="include fixed-width backings and typed views")
    parser.add_argument("--reference", type=Path, help="exact actual product PE; typed probes never rebuild it")
    args = parser.parse_args()
    result = (prepare_typed(args.source, args.receipt, args.output, args.probe, args.holder, args.project, args.owner, args.reference)
              if args.typed else prepare(args.source, args.receipt, args.output, args.probe, args.holder, args.project, args.owner))
    print(json.dumps(result["totals"]))
