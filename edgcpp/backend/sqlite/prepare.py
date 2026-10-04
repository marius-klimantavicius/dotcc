#!/usr/bin/env python3
"""Adapt the shared, authored dotcc SQLite host services to EDG's owner ABI.

This deliberately copies from sqlite/src at preparation time: platform fixes
have one source, and generated projects remain self-contained/AOT-compatible.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re


def replace_once(source: str, old: str, new: str) -> str:
    if source.count(old) != 1:
        raise ValueError(f"Host adapter expected exactly one occurrence: {old!r}")
    return source.replace(old, new)


def callbacks(source: str, all_source: str, owner_property: str) -> str:
    """Generate typed static thunks; the generated ABI carries its owner first."""
    names = re.findall(r"= &(\w+),", source)
    thunks = []
    for name in names:
        match = re.search(r"private static ([\w*]+) " + name + r"\(([^)]*)\)", all_source)
        if not match:
            raise ValueError(f"Missing callback declaration {name}")
        result, parameters = match.groups()
        arguments = []
        adapted = []
        for parameter in parameters.split(",") if parameters else []:
            parameter = parameter.strip()
            kind, argument = parameter.rsplit(" ", 1)
            adapted.append(("sbyte*" if kind == "byte*" else kind) + " " + argument)
            arguments.append(("(byte*)" if kind == "byte*" else "") + argument)
        signature = ", ".join(["SqliteCore owner", *adapted])
        thunks.append(f"    private static {result} Callback{name}({signature}) => owner.{owner_property}.{name}({', '.join(arguments)});")
        source = source.replace(f"= &{name},", f"= &Callback{name},")
    close = source.rfind("}")
    return source[:close] + "\n" + "\n".join(thunks) + "\n" + source[close:]


def prepare(repository_root: Path, output_dir: Path) -> list[Path]:
    repository_root, output_dir = Path(repository_root), Path(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    names = ["HostVfs.cs", "HostVfs.Mmap.cs", "HostVfs.Mutex.cs", "HostVfs.Platform.cs", "HostVfs.Shm.cs"]
    sources = {name: (repository_root / "sqlite/src" / name).read_text() for name in names}
    all_source = "\n".join(sources.values())
    outputs = []
    for name, original in sources.items():
        source = original.replace("using static Managed.Database.Sqlite;", "using static Edg.SQLite.SqliteCore;").replace("namespace Managed.Database;", "namespace Edg.SQLite.Host;")
        if name in ("HostVfs.cs", "HostVfs.Mmap.cs"):
            source = source.replace("public static unsafe partial class HostVfs", "public unsafe partial class HostVfs")
            if name == "HostVfs.cs":
                source = callbacks(source, all_source, "Host")
                source = replace_once(source, "vfs->zName = name;", "vfs->zName = (sbyte*)name;")
                source = source.replace("sqlite3_vfs_register(_registeredVfs, 1)", "_owner.HostRegister(_registeredVfs)").replace("sqlite3_vfs_unregister(_registeredVfs)", "_owner.HostUnregister(_registeredVfs)")
                source = source.replace("new DatabaseMapping(handle.Handle)", "new DatabaseMapping(this, handle.Handle)")
                source = replace_once(source, "    private const int PathBytes = 32768;", "    private readonly SqliteCore _owner;\n    public HostVfs(SqliteCore owner) => _owner = owner;\n    private const int PathBytes = 32768;")
                source = replace_once(source, "    [ThreadStatic] private static string? _lastError;\n    [ThreadStatic] private static int _lastErrorNumber;", "    private readonly ThreadLocal<string?> _errorText = new();\n    private readonly ThreadLocal<int> _errorNumber = new();\n    private string? _lastError { get => _errorText.Value; set => _errorText.Value = value; }\n    private int _lastErrorNumber { get => _errorNumber.Value; set => _errorNumber.Value = value; }")
                # The callback tables are global storage, backed by pinned arrays.
                # They disappear with their owner, without finalizer races or native leaks.
                start = source.index("                    sqlite3_io_methods* methods = null;")
                end = source.index("\n                }", start)
                source = source[:start] + '''                    _methodStorage ??= GC.AllocateArray<sqlite3_io_methods>(1, pinned: true);
                    _vfsStorage ??= GC.AllocateArray<sqlite3_vfs>(1, pinned: true);
                    _nameStorage ??= GC.AllocateArray<byte>(11, pinned: true);
                    fixed (sqlite3_io_methods* methods = _methodStorage)
                    fixed (sqlite3_vfs* vfs = _vfsStorage)
                    fixed (byte* name = _nameStorage)
                    {
                        *methods = MethodTable;
                        *vfs = VfsTable;
                        "dotcc-host\\0"u8.CopyTo(_nameStorage);
                        vfs->zName = (sbyte*)name;
                        _registeredMethods = methods;
                        _registeredVfs = vfs;
                    }''' + source[end:]
                source = source.replace("    private const int PathBytes", "    private sqlite3_io_methods[]? _methodStorage;\n    private sqlite3_vfs[]? _vfsStorage;\n    private byte[]? _nameStorage;\n    private const int PathBytes")
            else:
                source = replace_once(source, "DatabaseMapping(SafeFileHandle handle)", "DatabaseMapping(HostVfs host, SafeFileHandle handle)")
                source = source.replace("Sqlite.Globals.sqlite3Config.szMmap", "host._owner.HostMmapDefault()").replace("Sqlite.Globals.sqlite3Config.mxMmap", "host._owner.HostMmapMaximum()")
                # Nested helper methods need the same owner's diagnostics.
                boundary = source.index("    private static int Fetch(")
                source = source[:boundary].replace("Failure(", "host.Failure(").replace("ValidRange(", "host.ValidRange(") + source[boundary:]
                # Nested mappings report into their owning VFS, not process globals.
                source = re.sub(r"ref (_mapped\w+)", r"ref host.\1", source)
                source = source.replace("ref host._mappedReads);", "ref _mappedReads);", 1).replace("ref host._mappedViews);", "ref _mappedViews);", 1).replace("ref host._mappedReferences);", "ref _mappedReferences);", 1)
            # Preserve immutable callback tables and the owner-first thunks static.
            source = re.sub(r"^(    (?:private|public)) static (?!readonly sqlite3_|\w+[ *]+Callback)", r"\1 ", source, flags=re.M)
        elif name == "HostVfs.Mutex.cs":
            source = callbacks(source, all_source, "Mutexes")
            source = source.replace("public static unsafe class HostMutex", "public unsafe class HostMutex")
            source = re.sub(r"^(    (?:private|public)) static (?!readonly sqlite3_|\w+[ *]+Callback)", r"\1 ", source, flags=re.M)
            # SQLite static mutex identities survive shutdown/reinitialization.
            # The shared implementation therefore leaves xMutexEnd a no-op.
            # An EDG owner is collectible, unlike the shared static API: release
            # its stable handles only when the adapter itself is collected.
            release = '''    ~HostMutex() { ReleaseStaticHandles(); }

    private void ReleaseStaticHandles()
    {
        lock (InitializationGate)
        {
            if (_staticHandles != null)
            {
                foreach (var handle in _staticHandles)
                    if (handle != 0) GCHandle.FromIntPtr(handle).Free();
                _staticHandles = null;
            }
        }
    }

'''
            source = source.replace("    private const int FirstStatic", release + "    private const int FirstStatic")
        path = output_dir / ("Runtime.Sqlite." + name)
        path.write_text("// <auto-generated />\n#nullable enable\n// Adapted from sqlite/src/" + name + "; edit the shared source or backend/sqlite/prepare.py.\n" + source)
        outputs.append(path)
    provider = repository_root / "edgcpp/backend/sqlite/HostBindings.cs"
    target = output_dir / "Runtime.Sqlite.HostBindings.cs"
    target.write_text(provider.read_text())
    outputs.append(target)
    project = output_dir / "Generated.csproj"
    if project.exists():
        project_text = project.read_text()
        item = '<Compile Include="Runtime.Sqlite.*.cs" />'
        if item not in project_text:
            project.write_text(project_text.replace("</Project>", f"  <ItemGroup>{item}</ItemGroup>\n</Project>"))
    (output_dir / "sqlite-host-sources.json").write_text(json.dumps({"sources": {f"sqlite/src/{name}": hashlib.sha256(text.encode()).hexdigest() for name, text in sources.items()}, "adapter": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(), "bindings": hashlib.sha256(provider.read_bytes()).hexdigest()}, indent=2) + "\n")
    return outputs


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    args = parser.parse_args()
    prepare(Path(__file__).resolve().parents[3], args.output_dir)
