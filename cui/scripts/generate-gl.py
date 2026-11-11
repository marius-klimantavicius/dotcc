"""Regenerate the authored GL ABI thunks from the reviewed signature manifest."""
import argparse
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
TYPES = {
    "GLenum_": "uint", "GLuint_": "uint", "GLint_": "int",
    "GLsizei_": "int", "GLfloat_": "float", "GLboolean_": "byte",
    "GLchar_": "byte", "GLsizeiptr_": "long", "GLintptr_": "long",
    "GLbitfield_": "uint", "unsigned char": "byte", "void": "void",
}


def csharp(value, native=False):
    value = re.sub(r"\bconst\b", "", value).strip()
    pointers = value.count("*")
    name = value.replace("*", "").strip()
    scalar = "nint" if native and name in ("GLsizeiptr_", "GLintptr_") else TYPES[name]
    return scalar + "*" * pointers


def generate():
    functions = json.loads((ROOT / "config/opengl.json").read_text())
    code = ["// Generated from config/opengl.json by scripts/generate-gl.py.",
            "// CUI calls managed thunks; only these thunks invoke native GL addresses.",
            "#nullable enable", "using System;", "using System.Runtime.InteropServices;",
            "namespace Managed.Forms;", "internal static unsafe class OpenGlHost", "{"]
    for function in functions:
        name = function["name"]
        ret = csharp(function["return"])
        params = [csharp(p) for p in function["parameters"]]
        native_params = [csharp(p, native=True) for p in function["parameters"]]
        signature = ", ".join([*native_params, csharp(function["return"], native=True)])
        args = ", ".join(f"{t} a{i}" for i, t in enumerate(params))
        call = ", ".join(f"a{i}" if t == params[i] else f"({t})a{i}" for i, t in enumerate(native_params))
        # On the selected x64 targets the platform C convention also covers WGL.
        code += [f"    private static delegate* unmanaged<{signature}> native_{name};",
                 f"    private static {ret} {name}({args})", "    {",
                 "        GlfwHost.CheckContext();",
                 f'        if (native_{name} == null) throw new InvalidOperationException("OpenGL entry point {name} must be loaded for the current app.");',
                 f"        {'return ' if ret != 'void' else ''}native_{name}({call});", "    }"]
    code += ["    internal static void Reset()", "    {"]
    for function in functions:
        code += [f"        native_{function['name']} = null;"]
    code += ["    }"]
    code += ["    public static void* GetProc(byte* utf8)", "    {",
             "        GlfwHost.CheckThread();",
             "        if (utf8 == null || global::Glfw.Glfw.glfwGetCurrentContext() is null) return null;",
             "        string? name = Marshal.PtrToStringUTF8((nint)utf8);",
             "        if (name is null) return null;",
             "        nint address = global::Glfw.Glfw.glfwGetProcAddress(name);",
             "        if (address == 0) return null;", "        switch (name)", "        {"]
    for function in functions:
        name = function["name"]
        signature = ", ".join([*[csharp(p) for p in function["parameters"]], csharp(function["return"])])
        native_signature = ", ".join([*[csharp(p, native=True) for p in function["parameters"]], csharp(function["return"], native=True)])
        code += [f'            case "{name}":',
                 f"                native_{name} = (delegate* unmanaged<{native_signature}>)address;",
                 f"                return (void*)(delegate*<{signature}>)&{name};"]
    code += ["            default: return null;", "        }", "    }", "}"]
    return "\n".join(code) + "\n"


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="Check the authored thunks without writing files")
    args = parser.parse_args()
    output = ROOT / "src/Host/OpenGlHost.cs"
    code = generate()
    if args.check:
        if output.read_text() != code:
            raise SystemExit("OpenGlHost.cs differs from config/opengl.json; run scripts/generate-gl.py")
    else:
        output.write_text(code)
