#!/usr/bin/env python3
"""CUI upstream/native differential qualification. All work is caller-selected.

See tests/qualification/README.md for the coordinator integration contract.
No native GLFW binary is ever loaded by a translated executable.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
FIXTURES = ROOT / "tests/qualification"
sys.dont_write_bytecode = True
sys.path.insert(0, str(REPO / "Scripts"))
from campaigns.recipes import helper, link_flags
RANDOM_INPUTS = helper(FIXTURES / "random_inputs.py")

# Native-only C++ header/ABI smoke is explicitly inventoried separately.
PROGRAMS = {
    "cui_tests": ("tests/test_main.c", "core"),
    "cui_test_devtools": ("tests/test_devtools.c", "core"),
    "cui_test_instrument": ("tests/test_instrument.c", "core"),
    "cui_test_inflate": ("tests/test_inflate.c", "inflate"),
    "cui_test_todo": ("tests/test_todo.c", "core"),
    "cui_test_bidi": ("tests/test_bidi.c", "bidi"),
    "cui_rtl_layout": ("tests/rtl_layout.c", "core"),
    "cui_rtl_paragraph": ("tests/rtl_paragraph.c", "core"),
    "cui_shape_dump": ("tests/shape_dump.c", "shape"),
    "qualification_pixels": ("@pixels.c", "core"),
    "qualification_images": ("@images.c", "core"),
    "qualification_lifecycle": ("@lifecycle.c", "core"),
}
CTEST = [p for p in PROGRAMS if p not in ("cui_rtl_paragraph", "cui_shape_dump") and not p.startswith("qualification_")]
CTEST += ["cui_screenshots", "cui_cpp_compat"]
SUITES = {
    "native": [],
    "upstream": [n for n in PROGRAMS if not n.startswith("qualification_")],
    "render": ["qualification_images", "qualification_pixels"],
    "lifecycle": ["qualification_lifecycle"],
}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


class Run:
    def __init__(self, args):
        self.args = args
        self.work = args.work.resolve()
        self.work.mkdir(parents=True, exist_ok=True)
        self.env = dict(os.environ, TMPDIR=str(self.work / "tmp"), TEMP=str(self.work / "tmp"), TMP=str(self.work / "tmp"))
        (self.work / "tmp").mkdir(exist_ok=True)
        self.report = dict(schema=1, scope="native controls and actual emitted upstream tests; no desktop or Windows claim",
                           source=json.loads((ROOT / "config/source.json").read_text()),
                           programs={}, commands=[], started_unix=time.time(), status="running")
        self.report["harness_sha256"] = sha(__file__)
        self.report["configuration_sha256"] = {str(p.relative_to(ROOT)): sha(p) for p in sorted((ROOT / "config").rglob("*")) if p.is_file()}
        self.report["tool_sha256"] = {p.name: sha(p) for p in (args.compiler, args.compiler.parent / "DotCC.Lib.dll") if p.is_file()}

    def save(self):
        write_json(self.work / "result.json", self.report)

    def run(self, command, label, *, check=True, stdin=None, cwd=None, timeout=600):
        command = [str(x) for x in command]
        log = self.work / "logs" / (label + ".log")
        log.parent.mkdir(parents=True, exist_ok=True)
        started = time.monotonic()
        with log.open("wb") as out:
            try:
                result = subprocess.run(command, cwd=cwd or self.work, env=self.env, input=stdin,
                                        stdout=out, stderr=subprocess.STDOUT, timeout=timeout)
                code = result.returncode
            except subprocess.TimeoutExpired:
                code = 124
        row = dict(command=command, exit_code=code, seconds=round(time.monotonic()-started, 3), log=str(log))
        self.report["commands"].append(row)
        if check and code:
            raise RuntimeError(f"{label} failed ({code}); see {log}")
        return row, log.read_text(encoding="utf-8", errors="replace")

    def inventory(self):
        row, output = self.run(["ctest", "--test-dir", self.args.native_build, "--show-only=json-v1"], "ctest-inventory")
        entries = json.loads(output)["tests"]
        selected = set(CTEST)
        inventory = []
        for entry in entries:
            name = entry["name"]
            reason = None if name in selected else (
                "Existing fuzz campaign not selected by PLAN D9" if name.startswith("cui_fuzz") else
                "Timed memory/performance soak not selected by PLAN D9" if name.startswith("cui_soak") else
                "Unclassified upstream test: update the qualification inventory")
            inventory.append(dict(name=name, command=entry["command"], selected=name in selected, exclusion=reason))
        if selected - {x["name"] for x in entries}:
            raise RuntimeError("Native control lacks required deterministic CTest entries")
        self.report["inventory"] = dict(registered=len(entries), selected=sum(x["selected"] for x in inventory),
                                       excluded=sum(not x["selected"] for x in inventory), entries=inventory,
                                       translated_cpp_compat="Native-only C++ public-header/ABI smoke; translated C harness uses the same C API",
                                       translated_screenshots="Not translated here: native gallery captures are baseline only; dedicated pixels runner supplies differential pixels",
                                       other_sources={p.name: "Specialist/manual program (not registered with CTest); selected cases listed in programs"
                                                      for p in sorted((self.args.source / "tests").glob("*.c"))
                                                      if p.name not in {Path(v[0]).name for v in PROGRAMS.values()}})

    def native(self):
        # Require the promised GLFW profile; an X11-only baseline is not accepted.
        cache = (self.args.native_build / "CMakeCache.txt").read_text()
        for option in ("CUI_TEXT_SHAPING:BOOL=ON", "CUI_IMAGE_ALL_FORMATS:BOOL=ON", "CUI_WITH_APP:BOOL=ON"):
            if option not in cache:
                raise RuntimeError("Native feature mismatch: " + option)
        backend = re.search(r"CUI_PLATFORM:[^=]*=(.*)", cache)
        if not backend or backend[1].lower() != "glfw":
            raise RuntimeError("Native control must use CUI_PLATFORM=glfw")
        # Execute exactly the registered commands, relocating writable fixture
        # arguments to private scratch. CTest's generated paths otherwise write
        # screenshots and todo fixtures into the shared native build directory.
        rows = []
        for entry in self.report["inventory"]["entries"]:
            if not entry["selected"]:
                continue
            name = entry["name"]
            directory = self.work / "native" / name
            directory.mkdir(parents=True, exist_ok=True)
            (directory / "qualification-directory-fixture.txt").write_text("CUI upstream file-list test requires a nonempty working directory.\n")
            command = [directory if Path(x) == self.args.native_build / "tests" else x for x in entry["command"]]
            row, text = self.run(command, "native/" + name, check=False, cwd=directory)
            rows.append(dict(name=name, exit_code=row["exit_code"], log=row["log"],
                             checks=[dict(checks=int(c), failed=int(f)) for c,f in re.findall(r"(\d+) checks, (\d+) failed", text)]))
        self.report["native_ctest"] = dict(exit_code=1 if any(x["exit_code"] for x in rows) else 0, tests=rows,
                                           registered_selected=len(CTEST),
                                           uncounted="bidi CHECK and rtl CHECK abort at first failure; inflate validates vectors/truncations and 8000 mutations; screenshots/C++ have status/output contracts")
        if self.report["native_ctest"]["exit_code"]:
            raise RuntimeError("Native deterministic baseline failed")

    def image_fixtures(self):
        from PIL import Image
        directory = self.work / "fixtures"
        directory.mkdir(exist_ok=True)
        colors = [(255,0,0), (0,255,0), (0,0,255), (255,255,255)]
        image = Image.new("RGB", (2,2))
        image.putdata(colors)
        for name in ("png", "bmp", "gif", "tga"):
            image.save(directory / ("colors." + name))
        Image.new("RGB", (8,8), (80,120,160)).save(directory / "colors.jpg", quality=95, subsampling=0)
        (directory / "invalid.bin").write_bytes(b"not an image\x00\xff")
        self.report["image_fixtures"] = {p.name: sha(p) for p in directory.iterdir()}

    def flags(self, source):
        return ["-std=c99", "-U_WIN32", "-U__APPLE__", "-DGLFW_INCLUDE_NONE",
                "-DCUI_TEXT_SHAPING", "-DCUI_IMAGE_ALL_FORMATS", "-I", ROOT / "config/include", "-I", source / "include"]

    def emit(self, source, output, flags, label):
        output.parent.mkdir(parents=True, exist_ok=True)
        self.run(["dotnet", self.args.compiler, *flags, source, "--emit=obj", "-o", output], label)
        return output

    def library(self):
        # The recipe remains the authority for staged adaptations and typed bindings.
        if self.args.objects_dir:
            objects = sorted(self.args.objects_dir.glob("*.cs"))
            if not objects or any(not p.read_bytes().startswith(b"//!dotcc object") for p in objects):
                raise RuntimeError("--objects-dir must contain retained DotCC object fragments")
            if not self.args.staged_source:
                raise RuntimeError("--objects-dir requires --staged-source from that same translation")
            source = self.args.staged_source
        else:
            recipe = helper(ROOT / "scripts/campaign.py")
            from types import SimpleNamespace
            ctx = SimpleNamespace(root=ROOT, work=self.work / "library", sources={"product": self.args.source}, receipt={})
            ctx.work.mkdir(exist_ok=True)
            spec = recipe.prepare(ctx)
            source = ctx.work / "source"
            def one(row):
                index, unit = row
                return self.emit(unit.path, ctx.work / "objects" / f"{index:03d}-{unit.path.stem}.cs",
                                 [*spec.flags, *unit.flags], f"core/{index:03d}-{unit.path.stem}")
            with ThreadPoolExecutor(max_workers=self.args.jobs) as pool:
                objects = list(pool.map(one, enumerate(spec.units)))
            self.report["adaptations"] = ctx.receipt.get("adaptations", [])
        self.report["library_objects"] = {str(p): sha(p) for p in objects}
        return source, objects

    def bundle(self, names, source, core):
        """One core link, separate processes per original upstream test entry."""
        extra = []
        declarations, branches = [], []
        for name in names:
            relative, _ = PROGRAMS[name]
            original = FIXTURES / relative[1:] if relative.startswith("@") else source / relative
            text = original.read_text(encoding="utf-8")
            test_flags = self.flags(source)
            if name == "cui_tests":
                text, sites = RANDOM_INPUTS.adapt(text)
                self.report["random_input_sites"] = sites
                override = self.work / "random-overrides.json"
                RANDOM_INPUTS.overrides(override)
                test_flags += ["--overrides-file", override]
            # Only rename the test entry. Bodies, assertions, and nested included
            # example/layout entrypoints remain unchanged.
            anchor = r"(?m)^int main\("
            if len(re.findall(anchor, text)) != 1:
                raise RuntimeError("Expected one test entry anchor: " + str(original))
            symbol = "cui_qualification_" + name
            # Retained coordinator source/objects are read-only inputs. Preserve
            # quoted include resolution while staging test entry copies privately.
            text = re.sub(r'(?m)^(\s*#\s*include\s*)"([^"]+)"',
                          lambda m: m[1] + '"' + (original.parent / m[2]).resolve().as_posix() + '"', text)
            copied = self.work / "test-inputs" / (symbol + ".c")
            copied.parent.mkdir(exist_ok=True)
            copied.write_text(re.sub(anchor, "int " + symbol + "(", text), encoding="utf-8")
            emitted = self.emit(copied, self.work / "bundle-objects" / (name + ".cs"), test_flags, name + "/emit")
            extra.append(emitted)
            takes_args = bool(re.search(r"\b" + symbol + r"\(int argc,", emitted.read_text()))
            declarations.append("int " + symbol + ("(int,char**);" if takes_args else "(void);"))
            branches.append('if(!strcmp(argv[1],"' + name + '")) return ' + symbol + ("(argc-1,argv+1);" if takes_args else "();"))
            self.report.setdefault("test_input_sha256", {})[name] = sha(original)
            self.report.setdefault("test_entry_adaptations", []).append(dict(program=name, kind="rename-main-only", symbol=symbol))
        dispatch = self.work / "bundle-dispatch.c"
        dispatch.write_text('#include <string.h>\n' + "\n".join(declarations) +
                            '\nint main(int argc,char **argv){if(argc<2)return 2;\n' + "\n".join(branches) + "\nreturn 2;}\n")
        managed, _ = self.build("cui_qualification_bundle", source, [*core,*extra], (str(dispatch), "core"))
        return {name: {label:[*command,name] for label,command in managed.items()} for name in names}

    def native_command(self, name):
        native = self.args.native_build / "tests" / name
        if name == "cui_tests":
            program = self.work / "programs" / name
            program.mkdir(parents=True, exist_ok=True)
            text, _ = RANDOM_INPUTS.adapt((self.args.source / "tests/test_main.c").read_text())
            input_file = program / "native-input-aligned.c"
            input_file.write_text(text)
            native = program / "native-input-aligned"
            self.run(["cc", "-O3", "-DNDEBUG", "-I", self.args.source / "include", input_file,
                      FIXTURES / "random_replay.c", self.args.native_build / "libcui.a", "-lm", "-lglfw", "-o", native], name + "/native-input-aligned-build")
        if name.startswith("qualification_"):
            program = self.work / "programs" / name
            program.mkdir(parents=True, exist_ok=True)
            native = program / "native"
            input_file = FIXTURES / PROGRAMS[name][0][1:]
            self.run(["cc", "-std=c99", "-DCUI_TEXT_SHAPING", "-DCUI_IMAGE_ALL_FORMATS", "-I", self.args.source / "include",
                      input_file, self.args.native_build / "libcui.a", "-lm", "-lglfw", "-o", native], name + "/native-build")
        return [str(native)]

    def build(self, name, source, core, specification=None):
        relative, kind = specification or PROGRAMS[name]
        program = self.work / "programs" / name
        program.mkdir(parents=True, exist_ok=True)
        input_file = FIXTURES / relative[1:] if relative.startswith("@") else source / relative
        self.report.setdefault("test_input_sha256", {})[name] = sha(input_file)
        extra = []
        objects = core if kind == "core" else []
        if kind in ("bidi", "shape"):
            extra += [source / "src/cui_bidi.c"]
        if kind == "shape":
            extra += [source / "src/cui_shape.c"]
        flags = self.flags(source)
        if name == "cui_tests":
            adapted, sites = RANDOM_INPUTS.adapt(input_file.read_text())
            self.report["random_input_sites"] = sites
            input_file = program / "input-recorded.c"
            input_file.write_text(adapted)
            override = self.work / "random-overrides.json"
            RANDOM_INPUTS.overrides(override)
            flags += ["--overrides-file", override]
        main = self.emit(input_file, program / "main.cs", flags, name + "/emit")
        for i, unit in enumerate(extra):
            objects = [*objects, self.emit(unit, program / f"extra-{i}.cs", flags, name + f"/extra-{i}")]
        raw = program / "raw"
        self.run(["dotnet", self.args.compiler, *objects, main, "--emit=managedlib", *link_flags("Cui", "Managed.Cui"),
                  "-o", raw], name + "/link")
        project = next(raw.glob("*.csproj"))
        if kind == "core":
            RANDOM_INPUTS.configure(raw)
        tree = ET.parse(project)
        ET.SubElement(ET.SubElement(tree.getroot(), "PropertyGroup"), "OutputType").text = "Exe"
        takes_args = bool(re.search(r"\bmain\(int argc,", main.read_text()))
        entry = 'Cui.main(args.Length + 1, argv)' if takes_args else 'Cui.main()'
        # Authored test entry point calls the emitted typed API, with stable UTF-8 argv.
        (raw / "QualificationEntry.cs").write_text('''using System;
using System.Runtime.InteropServices;
using Managed.Cui;
internal static unsafe class QualificationEntry {
    public static int Main(string[] args) {
        byte** argv = (byte**)NativeMemory.AllocZeroed((nuint)(args.Length + 2), (nuint)sizeof(byte*));
        for (int i=0;i<=args.Length;i++) argv[i]=(byte*)Marshal.StringToCoTaskMemUTF8(i==0?"qualification":args[i-1]);
        try { return ''' + entry + '''; }
        finally { for (int i=0;i<=args.Length;i++) Marshal.FreeCoTaskMem((nint)argv[i]); NativeMemory.Free(argv); }
    }
}
''')
        if kind == "core":
            # Configure authored host links only. Never alter generated C#.
            group = ET.SubElement(tree.getroot(), "ItemGroup")
            for path in sorted((ROOT / "src/Host").glob("*.cs")):
                ET.SubElement(group, "Compile", Include=str(path), Link="Host/" + path.name)
            ET.SubElement(group, "ProjectReference", Include=str(ROOT / "src/Glfw/Glfw.csproj"))
        tree.write(project, encoding="unicode")
        variants = {}
        forms = ("raw", "processed") if self.args.form == "all" else (self.args.form,)
        for form in forms:
            target = project
            artifacts = program / "artifacts" / form
            if form == "processed":
                directory = program / "processed"
                shutil.copytree(raw, directory)
                target = directory / project.name
                processed_tree = ET.parse(target)
                ET.SubElement(ET.SubElement(processed_tree.getroot(), "PropertyGroup"), "GeneratedMSBuildEditorConfigFile").text = str(
                    artifacts / "obj" / project.stem / "release" / (project.stem + ".GeneratedMSBuildEditorConfig.editorconfig"))
                for node in processed_tree.getroot().iter("Compile"):
                    include = node.get("Include", "")
                    if include and Path(include).is_absolute():
                        copied = program / "processed-host" / Path(include).name
                        copied.parent.mkdir(exist_ok=True)
                        shutil.copyfile(include, copied)
                        node.set("Include", str(copied))
                processed_tree.write(target, encoding="unicode")
                self.run(["dotnet", "build", target, "-c", "Release", "--artifacts-path", artifacts, "-m:1", "--nologo"],
                         name + "/semantic-context", timeout=1200)
                # Its MSBuild evaluation must resolve the same private outputs.
                previous = self.env
                self.env = dict(previous, ArtifactsPath=str(artifacts), UseArtifactsOutput="true")
                try:
                    self.run(["dotnet", self.args.postprocessor, target, "--in-place"], name + "/postprocess", timeout=1200)
                finally:
                    self.env = previous
            self.run(["dotnet", "build", target, "-c", "Release", "--artifacts-path", artifacts, "-m:1", "--nologo"],
                     name + "/" + form + "-build", timeout=1200)
            dll = artifacts / "bin" / project.stem / "release" / (project.stem + ".dll")
            if not dll.is_file():
                raise RuntimeError("Missing managed executable: " + str(dll))
            if self.args.mode in ("jit", "all"):
                variants[form + "-jit"] = ["dotnet", str(dll)]
            if self.args.mode in ("aot", "all"):
                output = program / (form + "-aot")
                self.run(["dotnet", "publish", target, "-c", "Release", "--artifacts-path", artifacts,
                          "-m:1", "-r", self.args.rid, "--self-contained", "true", "-p:PublishAot=true", "-o", output],
                         name + "/" + form + "-aot-publish", timeout=1800)
                variants[form + "-aot"] = [str(output / (project.stem + (".exe" if self.args.rid.startswith("win-") else "")))]
        return variants, self.native_command(name)

    def execute(self, name, managed, native):
        if name == "cui_tests":
            return self.random_execution(managed, native)
        directory = self.work / "results" / name
        directory.mkdir(parents=True, exist_ok=True)
        arguments = []
        if name == "cui_test_todo":
            arguments = [str(directory)]
        if name.startswith("qualification_"):
            arguments = [str(self.work / "fixtures")]
        if name in ("cui_rtl_layout", "cui_rtl_paragraph"):
            arguments = [str(self.args.font)]
        observations = {}
        for label, command in [("native", native), *managed.items()]:
            out = directory / label
            out.mkdir(exist_ok=True)
            (out / "qualification-directory-fixture.txt").write_text("CUI upstream file-list test requires a nonempty working directory.\n")
            actual_arguments = [*arguments, str(out / "paragraph")] if name == "cui_rtl_paragraph" else arguments
            row, text = self.run([*command, *actual_arguments], name + "/" + label, check=False, cwd=out,
                                  timeout=self.args.timeout)
            counts = re.findall(r"(\d+) checks, (\d+) failed", text)
            observations[label] = dict(exit_code=row["exit_code"], log=row["log"], checks=[list(map(int,c)) for c in counts],
                                       stdout_sha256=hashlib.sha256(text.encode()).hexdigest())
        success = all(x["exit_code"] == 0 for x in observations.values())
        if any(observations["native"]["checks"] != observations[label]["checks"] for label in managed):
            success = False
        if name.startswith("qualification_") or name == "cui_rtl_paragraph":
            left = directory / "native"
            files = sorted(left.glob("*.png" if name == "cui_rtl_paragraph" else "*.rgba"))
            comparisons = []
            for label in managed:
                right = directory / label
                for p in files:
                    other = right / p.name
                    match = other.is_file() and p.read_bytes() == other.read_bytes()
                    if name == "cui_rtl_paragraph" and other.is_file():
                        from PIL import Image
                        with Image.open(p) as native_image, Image.open(other) as managed_image:
                            match = native_image.size == managed_image.size and native_image.convert("RGBA").tobytes() == managed_image.convert("RGBA").tobytes()
                    comparisons.append(dict(variant=label, file=p.name, bytes=p.stat().st_size, native_sha256=sha(p),
                                            managed_sha256=sha(other) if other.is_file() else None,
                                            comparison="normalized-rgba" if name == "cui_rtl_paragraph" else "exact-file-bytes", equal=match))
            if name != "qualification_lifecycle" and not files:
                success = False
            if name == "cui_rtl_paragraph" and len(files) != 5:
                success = False
            success &= all(c["equal"] for c in comparisons)
            observations["exact_bytes"] = comparisons
        observations["status"] = "passed" if success else "failed"
        self.report["programs"][name] = observations
        self.save()
        print(name + ": " + observations["status"], flush=True)

    def random_execution(self, managed, native):
        name = "cui_tests"
        directory = self.work / "results" / name
        observations, transcripts, traces = {}, {}, []
        commands = [("native-baseline", [str(self.args.native_build / "tests/cui_tests")]), *managed.items(), ("native", native)]
        for label, command in commands:
            out = directory / label
            out.mkdir(parents=True, exist_ok=True)
            (out / "qualification-directory-fixture.txt").write_text("CUI nonempty directory fixture\n")
            trace = out / "random-inputs.bin"
            previous = self.env
            if label != "native-baseline":
                self.env = dict(previous, CUI_QUALIFICATION_RANDOM_TRACE=str((traces[0] if traces else out / "missing-inputs.bin") if label == "native" else trace))
            try:
                row, text = self.run(command, name + "/" + label, check=False, cwd=out, timeout=self.args.timeout)
            finally:
                self.env = previous
            observations[label] = dict(exit_code=row["exit_code"], log=row["log"],
                                       checks=[list(map(int,c)) for c in re.findall(r"(\d+) checks, (\d+) failed",text)])
            transcripts[label] = text
            if label in managed and trace.is_file():
                traces.append(trace)
                observations[label]["random_input_sha256"] = sha(trace)
                observations[label]["random_input_records"] = trace.stat().st_size // 16
        native_groups = RANDOM_INPUTS.groups(transcripts["native"])
        success = len(native_groups) == 69 and all(v["exit_code"] == 0 for v in observations.values())
        for label in managed:
            success &= observations[label]["checks"] == observations["native"]["checks"]
            success &= RANDOM_INPUTS.groups(transcripts[label]) == native_groups
        success &= len(traces) == len(managed) and len({sha(p) for p in traces}) == 1
        observations["input_alignment"] = dict(seeds=[1234,99], groups=69, all_groups_exact=all(RANDOM_INPUTS.groups(transcripts[l]) == native_groups for l in managed),
                                               provider="Actual emitted Cui.Libc.srand/rand; recorded seed,line,call-site,value vectors replayed by native C",
                                               replay_source_sha256=sha(FIXTURES / "random_replay.c"), harness_source_sha256=sha(FIXTURES / "random_inputs.py"),
                                               native_all_records_consumed="all consumed" in transcripts["native"])
        success &= observations["input_alignment"]["native_all_records_consumed"]
        observations["status"] = "passed" if success else "failed"
        self.report["programs"][name] = observations
        self.save()
        print(name + ": " + observations["status"], flush=True)

    def shaping(self, managed, native):
        # Reuse upstream's corpus and independent HarfBuzz oracle unchanged.
        compare = helper(self.args.source / "tools/compare_shape.py")
        import uharfbuzz as hb
        import itertools
        lines = compare.corpus()
        marks = [chr(cp) for cp in range(0x64b, 0x660)]
        lines += ["ب" + a + b + "ت" for a,b in itertools.product(marks, repeat=2)]
        expected = []
        face = hb.Face(self.args.font.read_bytes())
        font = hb.Font(face)
        font.scale = (face.upem, face.upem)
        for line in lines:
            buf = hb.Buffer()
            buf.add_str(line)
            buf.direction, buf.script, buf.language = "rtl", "Arab", "und"
            hb.shape(font, buf)
            expected.append([(g.codepoint,p.x_advance,p.x_offset,p.y_offset)
                             for g,p in zip(buf.glyph_infos,buf.glyph_positions)])
        rows = {}
        transcripts = []
        for label, command in [("native", native), *managed.items()]:
            row, text = self.run([*command, self.args.font], "shaping/" + label,
                                  stdin=("\n".join(lines)+"\n").encode(), check=False)
            actual = [[tuple(map(int,v.split(","))) for v in line.split()[1:]] for line in text.splitlines()]
            transcripts.append(actual)
            mismatches = [dict(index=i, text=lines[i], expected=want, actual=got)
                          for i,(want,got) in enumerate(zip(expected,actual)) if want != got]
            rows[label] = dict(exit_code=row["exit_code"], cases=len(actual), exact_cases=sum(w==g for w,g in zip(expected,actual)),
                               mismatches=mismatches, log=row["log"])
        success = all(r["exit_code"] == 0 and r["cases"] == len(lines) and not r["mismatches"] for r in rows.values())
        rows.update(font=dict(file=self.args.font.name, sha256=sha(self.args.font), bytes=self.args.font.stat().st_size),
                    harfbuzz=hb.version_string(), cases=len(lines), native_translated_exact=all(t == transcripts[0] for t in transcripts[1:]) if managed else None,
                    status="passed" if success else "failed")
        self.report["programs"]["cui_shape_dump"] = rows
        self.save()
        print("cui_shape_dump: " + rows["status"], flush=True)


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--source", type=Path, required=True)
    p.add_argument("--native-build", type=Path, required=True)
    p.add_argument("--work", type=Path, required=True)
    p.add_argument("--action", choices=("inventory", "native", "managed", "all"), default="all")
    p.add_argument("--suite", choices=tuple(SUITES))
    p.add_argument("--form", choices=("raw", "processed", "all"), default="raw")
    p.add_argument("--mode", choices=("jit", "aot", "all"), default="jit")
    p.add_argument("--rid", default="linux-x64" if sys.platform == "linux" else "win-x64")
    p.add_argument("--compiler", type=Path, default=Path(os.environ.get("DOTCC_COMPILER", REPO / "DotCC/bin/Release/net10.0/dotcc.dll")))
    p.add_argument("--objects-dir", type=Path)
    p.add_argument("--postprocessor", type=Path, default=Path(os.environ.get("DOTCC_POSTPROCESSOR", REPO / "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll")))
    p.add_argument("--staged-source", type=Path)
    p.add_argument("--font", type=Path, help="Trusted Arabic-script TTF; required for shape/RTL execution")
    p.add_argument("--program", choices=tuple(PROGRAMS), action="append")
    p.add_argument("--separate-programs", action="store_true", help="Link a separate core executable per test (slower diagnostic mode)")
    p.add_argument("--jobs", type=int, choices=(1,2), default=2)
    p.add_argument("--timeout", type=int, default=300)
    args = p.parse_args()
    if (args.suite == "native" or (not args.suite and args.action in ("native", "all"))) and not args.font:
        p.error("--font is required for the trusted native shaping oracle")
    for attr in ("source", "native_build", "work", "compiler", "postprocessor", "objects_dir", "staged_source", "font"):
        if getattr(args, attr) is not None:
            setattr(args, attr, getattr(args, attr).resolve())
    if args.work == ROOT or ROOT in args.work.parents:
        p.error("--work must be disposable external storage, outside the campaign tree")
    run = Run(args)
    if args.suite:
        args.action = "native" if args.suite == "native" else "managed"
    run.report["selection"] = dict(suite=args.suite, action=args.action, form=args.form, mode=args.mode, rid=args.rid)
    try:
        run.inventory()
        if args.action in ("native", "all"):
            run.native()
            if args.font:
                run.shaping({}, [str(args.native_build / "tests/cui_shape_dump")])
        if args.action in ("managed", "all"):
            programs = list(dict.fromkeys(args.program or (SUITES[args.suite] if args.suite else PROGRAMS)))
            run.report["selection"]["programs"] = programs
            run.save()
            if any(n in programs for n in ("cui_shape_dump", "cui_rtl_layout", "cui_rtl_paragraph")) and not args.font:
                p.error("--font is required for selected shaping/RTL tests")
            run.image_fixtures()
            source, core = run.library() if any(PROGRAMS[n][1] == "core" for n in programs) else (args.source, [])
            run.save()
            bundled = {}
            core_names = [n for n in programs if PROGRAMS[n][1] == "core"]
            if len(core_names) > 1 and not args.separate_programs:
                try:
                    bundled = run.bundle(core_names, source, core)
                except Exception as e:
                    # A single test-only lowering failure must not prevent
                    # independent tests from producing execution evidence.
                    run.report["bundle_failure"] = str(e)
                    run.save()
                    print("Bundle failed; retrying selected programs independently: " + str(e), flush=True)
            for name in programs:
                try:
                    if name in bundled:
                        managed, native = bundled[name], run.native_command(name)
                    else:
                        managed, native = run.build(name, source, core)
                    if name == "cui_shape_dump":
                        run.shaping(managed, native)
                    else:
                        run.execute(name, managed, native)
                except Exception as e:
                    run.report["programs"][name] = dict(status="failed", error=str(e))
                    run.save()
                    print(name + ": " + str(e), flush=True)
        failed = any(v.get("status") == "failed" for v in run.report["programs"].values())
        run.report["status"] = "failed" if failed else "passed"
        run.save()
        print(run.work / "result.json")
        return 1 if failed else 0
    except Exception as e:
        run.report.update(status="failed", error=str(e))
        run.save()
        raise


if __name__ == "__main__":
    raise SystemExit(main())
