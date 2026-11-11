"""Emit a focused GLFW override + upstream GL renderer library for host tests."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True, help="Existing DotCC executable or DLL")
    parser.add_argument("--source-root", type=Path, required=True)
    parser.add_argument("--work-root", type=Path, required=True, help="Caller-selected disposable output directory")
    args = parser.parse_args()
    work = args.work_root.resolve()
    work.mkdir(parents=True, exist_ok=True)
    temporary = work / "tmp"
    temporary.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary))
    compiler = args.compiler.resolve()
    command = ["dotnet", str(compiler)] if compiler.suffix.lower() == ".dll" else [str(compiler)]
    source = args.source_root.resolve()
    objects = work / "objects"
    objects.mkdir(exist_ok=True)
    output = work / "emitted"
    common = ["-std=c99", "-U_WIN32", "-U__APPLE__", "-DGLFW_INCLUDE_NONE", "-I", str(ROOT / "config/include"), "-I", str(source / "include")]
    common += ["-D" + value for value in json.loads((ROOT / "config/profile.json").read_text())["defines"]]
    subprocess.run([*command, str(ROOT / "tests/host-glfw/abi-probe.c"), "--emit=obj", *common,
                    "--overrides-file", str(ROOT / "config/glfw-overrides.json"), "-o", str(objects / "glfw.cs")], check=True, env=env)
    subprocess.run([*command, str(source / "src/render/cui_render_gl.c"), "--emit=obj", *common,
                    "-o", str(objects / "render.cs")], check=True, env=env)
    subprocess.run([*command, str(objects / "glfw.cs"), str(objects / "render.cs"), "--emit=managedlib",
                    "--nest-types", "--runtime=c", "--class-name", "Cui", "--namespace", "Managed.Forms",
                    "--split=size", "--split-size=262144", "--literal-pool", "--deduplicate-inline", "-o", str(output)], check=True, env=env)
    project = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    properties = ET.SubElement(project, "PropertyGroup")
    for name, value in {"TargetFramework": "net10.0", "AllowUnsafeBlocks": "true", "Nullable": "enable", "EnableDefaultCompileItems": "false"}.items():
        ET.SubElement(properties, name).text = value
    items = ET.SubElement(project, "ItemGroup")
    ET.SubElement(items, "Compile", Include="emitted/*.cs")
    for pattern in ("GlfwHost*.cs", "OpenGlHost.cs"):
        ET.SubElement(items, "Compile", Include=str(ROOT / "src/Host" / pattern))
    ET.SubElement(items, "ProjectReference", Include=str(ROOT / "src/Glfw/Glfw.csproj"))
    tree = ET.ElementTree(project)
    ET.indent(tree, space="  ")
    target = work / "ProbeLibrary.csproj"
    tree.write(target, encoding="unicode")
    print(target)


if __name__ == "__main__":
    main()
