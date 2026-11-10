"""Compare every reviewed GL signature with the selected upstream renderer."""
import argparse
import json
from pathlib import Path
import re
import runpy

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-root", type=Path, required=True)
    args = parser.parse_args()
    helper = runpy.run_path(str(ROOT / "scripts/generate-gl.py"))
    csharp = helper["csharp"]
    manifest = json.loads((ROOT / "config/opengl.json").read_text())
    upstream = (args.source_root / "src/render/cui_render_gl.c").read_text()
    declarations = re.findall(r"^\s*(.*?)\(CUI_GLAPI \*(\w+)\)\(([^)]*)\);", upstream, re.MULTILINE)
    expected = {}
    for ret, name, parameters in declarations:
        params = [] if parameters.strip() == "void" else [p.strip() for p in parameters.split(",")]
        expected["gl" + name] = (csharp(ret, native=True), [csharp(p, native=True) for p in params])
    actual = {}
    for function in manifest:
        name = function["name"]
        if name in actual:
            raise SystemExit("Duplicate manifest entry: " + name)
        actual[name] = (csharp(function["return"], native=True), [csharp(p, native=True) for p in function["parameters"]])
    if not expected or expected != actual:
        differing = sorted(name for name in expected.keys() | actual.keys() if expected.get(name) != actual.get(name))
        raise SystemExit("GL ABI manifest mismatch: " + ", ".join(differing))
    if (ROOT / "src/Host/OpenGlHost.cs").read_text() != helper["generate"]():
        raise SystemExit("Regenerate OpenGlHost.cs with scripts/generate-gl.py")
    print(f"PASS {len(expected)} exact native GL signatures and current generated thunks")


if __name__ == "__main__":
    main()
