"""Extract and execute exact deterministic upstream fuzzer routines privately."""
from __future__ import annotations

import hashlib
import json
from pathlib import Path
import re
import shutil
import sys

from campaigns.inputs import acquire
from campaigns.recipes import flags, helper, json_file, link_flags
from campaigns.testing import forms, modes
from campaigns.translation import check_product, relocate_project

ROUTINES = ("test_compressBound", "test_decompressBound", "test_setCParams")
EXCERPTS = ("fuzzer-compression-roundtrip", "fuzzer-training-and-dictionary", "zstream-dictionary-skip-reuse")
SELECTED = (*ROUTINES, *EXCERPTS)
EXPECTED_CASES = dict(zip(SELECTED, (1, 1, 1, 11, 13, 6)))


def between(text, first, last):
    if text.count(first) != 1 or text.count(last) != 1:
        raise RuntimeError("Ambiguous upstream extraction anchors")
    start, end = text.index(first), text.index(last)
    if start >= end:
        raise RuntimeError("Reversed upstream extraction anchors")
    return text[start:end]


def function(text, name):
    signature = "static void " + name + "(unsigned tnb)"
    if text.count(signature) != 1:
        raise RuntimeError("Upstream function signature differs: " + name)
    start = text.index(signature)
    # Ignore braces inside C comments and strings while retaining exact offsets.
    tokens = re.compile(r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)
    masked = tokens.sub(lambda match: " " * len(match.group()), text)
    opening = masked.index("{", start)
    depth = 0
    for end in range(opening, len(masked)):
        if masked[end] == "{":
            depth += 1
        elif masked[end] == "}":
            depth -= 1
            if depth == 0:
                body = text[start:end + 1]
                return body, dict(name=name, first_line=text.count("\n", 0, start) + 1,
                                  last_line=text.count("\n", 0, end) + 1,
                                  sha256=hashlib.sha256(body.encode()).hexdigest())
    raise RuntimeError("Unterminated upstream function: " + name)


def extract(source, destination):
    original = (source / "tests/fuzzer.c").read_text()
    streaming = (source / "tests/zstreamtest.c").read_text()
    macros = between(original, "#define CHECK(fn)", "/*=============================================\n*   Memory Tests")
    display = between(original, "#define DISPLAY(...)", "static const U64 g_refreshRate")
    extracted = [function(original, name) for name in ROUTINES]
    records = []
    exact_chunks = []

    def assertion_sites(text):
        return len(re.findall(r'\b(?:CHECK(?:_[A-Z]+)?|CHECKPLUS|assert)\s*\(', text)) + text.count("goto _output_error")

    def chunk(text, first, last, filename):
        result = between(text, first, last)
        start = text.index(first)
        records.append(dict(source=filename, first_line=text.count("\n", 0, start) + 1,
                            last_line=text.count("\n", 0, start + len(result) - 1) + 1,
                            sha256=hashlib.sha256(result.encode()).hexdigest(),
                            assertion_sites=assertion_sites(result),
                            cases=re.findall(r'DISPLAYLEVEL\(3, "test%[^:]+ : ([^"\\]+)', result)))
        exact_chunks.append(result)
        return result

    basic = between(original, "static int basicUnitTests(U32 const seed, double compressibility)", "static int longUnitTests(U32 const seed, double compressibility)")
    fixture = between(basic, "    size_t const CNBuffSize = 5 MB;", "    /* Basic tests */")
    cleanup = '''
_end:
    free(CNBuffer); free(compressedBuffer); free(decodedBuffer); return testResult;
_output_error:
    testResult = 1; goto _end;
}
'''
    roundtrip = chunk(original,
                      '    DISPLAYLEVEL(3, "test%3u : compress %u bytes : ", testNb++, (unsigned)CNBuffSize);',
                      '    DISPLAYLEVEL(3, "test%3u : invalid endDirective : ", testNb++);', "tests/fuzzer.c")
    fuzzer_roundtrip = "static int dz_fuzzer_roundtrip(void) {\n U32 const seed=0; double const compressibility=0.5;\n" + fixture + roundtrip + cleanup
    training_start = original.index("    /* Dictionary and dictBuilder tests */")
    training_source = between(original, "    /* Dictionary and dictBuilder tests */", "    /* COVER dictionary builder tests */")
    training = chunk(training_source, "    /* Dictionary and dictBuilder tests */",
                     '        DISPLAYLEVEL(3, "test%3i : Multithreaded COVER dictBuilder : ", testNb++);', "tests/fuzzer.c")
    # Record actual original line positions, since the unique training anchor
    # scopes repeated display strings later in the same upstream function.
    records[-1]["first_line"] += original[:training_start].count("\n")
    records[-1]["last_line"] += original[:training_start].count("\n")
    dictionary_source = between(training_source,
                                '        DISPLAYLEVEL(3, "test%3i : check dictID : ", testNb++);',
                                '        DISPLAYLEVEL(3, "test%3i : compress with static CDict : ", testNb++);')
    dictionary_offset = original.index(dictionary_source)
    records.append(dict(source="tests/fuzzer.c", first_line=original.count("\n", 0, dictionary_offset) + 1,
                        last_line=original.count("\n", 0, dictionary_offset + len(dictionary_source) - 1) + 1,
                        sha256=hashlib.sha256(dictionary_source.encode()).hexdigest(),
                        assertion_sites=assertion_sites(dictionary_source),
                        cases=re.findall(r'DISPLAYLEVEL\(3, "test%[^:]+ : ([^"\\]+)', dictionary_source)))
    exact_chunks.append(dictionary_source)
    fuzzer_dictionary = ("static int dz_fuzzer_dictionary(void) {\n U32 const seed=0; double const compressibility=0.5;\n" + fixture + training + dictionary_source
                         + "\n ZSTD_freeCCtx(cctx); free(dictBuffer); free(samplesSizes);\n }\n" + cleanup)
    stream_compress = chunk(streaming, "    /* Basic compression test */", '    /* context size functions */\n    DISPLAYLEVEL(3, "test%3i : estimate CStream size : ", testNb++);', "tests/zstreamtest.c")
    stream_decode = chunk(streaming, "    /* skippable frame test */", "    /* check decompression fails early if first bytes are wrong */", "tests/zstreamtest.c")
    stream_macros = between(streaming, "#define CHECK(cond, ...)", "/*======================================================\n *   Basic Unit tests")
    stream_display = between(streaming, "#define DISPLAY(...)", "static const U64 g_refreshRate")
    stream_case = '''
#undef CHECK
#undef CHECK_Z
#undef DISPLAY
#undef DISPLAYLEVEL
#define g_displayLevel dz_zstream_displayLevel
''' + stream_display + stream_macros + '''
#define COMPRESSIBLE_NOISE_LENGTH (10 MB)
static int dz_zstream_cases(void) {
    g_displayLevel=3;
    U32 const seed=0;
    size_t const CNBufferSize=COMPRESSIBLE_NOISE_LENGTH;
    size_t const skippableFrameSize=200 KB;
    size_t const compressedBufferSize=(8+skippableFrameSize)+ZSTD_compressBound(CNBufferSize);
    size_t const dictSize=128 KB;
    void* CNBuffer=malloc(CNBufferSize);
    void* compressedBuffer=malloc(compressedBufferSize);
    void* decodedBuffer=malloc(CNBufferSize);
    size_t cSize; int testNb=1, result=1;
    ZSTD_CStream* zc=ZSTD_createCStream(); ZSTD_DStream* zd=ZSTD_createDStream();
    ZSTD_inBuffer inBuff,inBuff2; ZSTD_outBuffer outBuff;
    if(!CNBuffer || !compressedBuffer || !decodedBuffer || !zc || !zd) goto _output_error;
    RDG_genBuffer(CNBuffer,CNBufferSize,0.5,0.,seed);
''' + stream_compress + stream_decode + '''
    result=0;
_output_error:
    ZSTD_freeCStream(zc); ZSTD_freeDStream(zd); free(CNBuffer); free(compressedBuffer); free(decodedBuffer); return result;
}
'''
    notice = original[:original.index("*/") + 2]
    wrapper = "\nstatic int dz_zstream_cases(void);\nint dz_upstream(int index) {\n g_displayLevel=3;\n switch(index) {\n"
    wrapper += "\n".join(f' case {i}: fprintf(stderr,"CASE BEGIN {name}\\n"); {name}({i + 1}); fprintf(stderr,"CASE PASS {name}\\n"); return 0;' for i, name in enumerate(ROUTINES))
    for index, (name, routine) in enumerate(zip(EXCERPTS, ("dz_fuzzer_roundtrip", "dz_fuzzer_dictionary", "dz_zstream_cases")), start=len(ROUTINES)):
        wrapper += f'\n case {index}: fprintf(stderr,"CASE BEGIN {name}\\n"); if({routine}()) return 1; fprintf(stderr,"CASE PASS {name}\\n"); return 0;'
    wrapper += '\n default: return 1;\n }\n}\nint dz_upstream_fail_control(void) { CHECK(0); return 0; }\n'
    native_main = f'\n#ifdef DZ_NATIVE_MAIN\nint main(int argc, char** argv) {{ int i; if(argc==2 && strcmp(argv[1], "--fail-control")==0) return dz_upstream_fail_control(); for(i=0;i<{len(SELECTED)};++i) if(dz_upstream(i)) return 1; return 0; }}\n#endif\n'
    content = notice + '''
/* Test-only exact extraction; the recorded routines and CHECK macros below
 * are copied without modification from tests/fuzzer.c. */
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#undef NDEBUG
#include <assert.h>
#define ZSTD_STATIC_LINKING_ONLY
#define ZSTD_DISABLE_DEPRECATE_WARNINGS
#include "zstd.h"
#include "zstd_errors.h"
#include "debug.h"
#include "mem.h"
#include "zstd_internal.h"
#define ZDICT_STATIC_LINKING_ONLY
#include "zdict.h"
#include "datagen.h"
''' + display + macros + "\n\n".join(body for body, _ in extracted) + fuzzer_roundtrip + fuzzer_dictionary + wrapper + stream_case + native_main
    observed = [(len(record["cases"]), record["assertion_sites"]) for record in records]
    expected = [(11, 12), (2, 4), (11, 16), (2, 11), (4, 11)]
    if observed != expected:
        raise RuntimeError("Selected upstream case/assertion inventory differs: " + repr(observed))
    for body in [*exact_chunks, *(body for body, _ in extracted)]:
        if content.count(body) != 1:
            raise RuntimeError("Exact upstream body was lost or duplicated during staging")
    destination.write_text(content)
    candidates = sorted(set(re.findall(r'^static void (test_\w+)\(', original, re.M)))
    inventory = dict(source=str(source / "tests/fuzzer.c"), selected=[record for _, record in extracted], excerpts=records,
                     selected_groups=list(SELECTED),
                     expected_case_counts=EXPECTED_CASES,
                     expected_excerpt_cases=sum(count for count, _ in expected),
                     expected_excerpt_assertion_sites=sum(count for _, count in expected),
                     unselected=[dict(name=name, status="excluded", reason=exclusion(name))
                                 for name in candidates if name not in ROUTINES],
                     other_upstream_drivers=[
                         dict(name="fuzzer.c:basicUnitTests", status="partial", reason="selected exact compression/training/dictionary excerpts run; full driver includes MT dictionary trainers, fixed efficiency bounds, huge allocation and unrelated specialized parameter regressions"),
                         dict(name="fuzzer.c:fuzzerTests", status="excluded", reason="randomized driver and sustained stress are outside this bounded deterministic acceptance selection"),
                         dict(name="zstreamtest.c:basicUnitTests", status="partial", reason="exact stream/dictionary/skip/reuse excerpts run; unmodified full driver unconditionally requests ZSTD_c_nbWorkers=2, incompatible with required single-thread product"),
                         dict(name="playTests.sh", status="excluded", reason="shell driver validates upstream CLI executable and filesystem command options; CLI is outside the managed-library product")],
                     macros_sha256=hashlib.sha256(macros.encode()).hexdigest(),
                     staged_sha256=hashlib.sha256(content.encode()).hexdigest(),
                     test_dependencies=["programs/datagen.c"],
                     source_changes=[], scope="exact original routines and selected assertion excerpts; original CHECK macros; authored fixture/cleanup/entrypoint wrappers")
    return inventory


def exclusion(name):
    lower = name.lower()
    if any(part in lower for part in ("thread", "mt", "worker")):
        return "requires multithreaded compression or worker scheduling, excluded from selected single-thread profile"
    if any(part in lower for part in ("long", "large", "overflow", "memory", "big")):
        return "large-window, allocation-scale or overflow stress is outside the bounded deterministic selection; ordinary bounds remain selected"
    if any(part in lower for part in ("sequence", "producer", "external")):
        return "sequence-producer/externally supplied match-finder APIs are outside the owning API contract"
    return "specialized optimizer/experimental-parameter regression outside the selected portable standard-frame/dictionary/stream workflows; not claimed as executed"


def observed_cases(transcript):
    counts, passed, active = {}, [], None
    for line in transcript.splitlines():
        if line.startswith("CASE BEGIN "):
            active = line.removeprefix("CASE BEGIN ")
            if active in counts:
                raise RuntimeError("Repeated upstream case group: " + active)
            counts[active] = 0
        elif line.startswith("CASE PASS "):
            name = line.removeprefix("CASE PASS ")
            if name != active:
                raise RuntimeError("Upstream case group boundaries differ")
            passed.append(name)
            active = None
        elif active:
            counts[active] += len(re.findall(r'\btest\s*\d+\s*:', line))
    if passed != list(SELECTED) or counts != EXPECTED_CASES or active:
        raise RuntimeError("Upstream executed case inventory differs: " + repr((passed, counts)))
    return counts


def printed_metrics(transcript):
    values = re.findall(r'size of cctx for level 1\s*:\s*(\d+) bytes', transcript)
    if len(values) != 1:
        raise RuntimeError("Exact upstream CCtx size observation is missing or repeated")
    return dict(sizeof_cctx_level1=int(values[0]))


def objects(ctx):
    if (ctx.work / "objects").is_dir():
        directory = ctx.work / "objects"
    else:
        pointer = json_file(ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json"))
        receipt = json_file(Path(pointer["receipt"]))
        directory = ctx.root / receipt["staging"] / "objects"
    selected = sorted(directory.glob("*.cs"))
    policy = json_file(ctx.root / "config/translation.json")
    expected = [Path(name).stem for name in policy["units"]] + [Path(policy["bridge"]).stem]
    actual = [path.stem.split("-", 1)[1] for path in selected]
    if actual != expected:
        raise RuntimeError("Complete original translated core objects unavailable for exact upstream tests: " + str(directory))
    return selected


def run(ctx, suite):
    if ctx.options.rid != "linux-x64":
        raise RuntimeError("Upstream qualification currently requires executed linux-x64")
    report = dict(status="running", rows=[], inventory=None)
    report_path = ctx.artifacts / "upstream-report.json"
    try:
        source = ctx.sources.get("product") or acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
        core = objects(ctx)
        core_source = core[0].parent.parent / "source"
        if not (core_source / "lib/zstd.h").is_file():
            raise RuntimeError("Original staged core header closure is unavailable; regenerate the product")
        # Anonymous typedef identity includes its declaring header provenance.
        # Reuse the exact header paths of the linked core objects. The data
        # generator's relative ../lib includes need the adjacent staged programs
        # directory too; original references remain untouched.
        shutil.copytree(source / "programs", core_source / "programs", dirs_exist_ok=True)
        datagen = core_source / "programs/datagen.c"
        work = ctx.work / "upstream"
        work.mkdir(parents=True, exist_ok=True)
        test = work / "upstream_cases.c"
        report["inventory"] = extract(source, test)
        report["header_closure"] = dict(core_objects=[str(path) for path in core], headers=str(core_source / "lib"),
                                         data_generator=str(datagen), data_generator_sha256=hashlib.sha256(datagen.read_bytes()).hexdigest())
        report_path.write_text(json.dumps(report, indent=2) + "\n")
        policy = json_file(ctx.root / "config/translation.json")
        common = ["-std=" + policy["dialect"], *flags(policy["defines"], [core_source / "lib", core_source / "lib/common", core_source / "programs"])]
        tests = helper(ctx.root / "tests/managed_qualification.py")
        native = tests.oracle(ctx)
        native_objects = sorted(path for path in native.parent.rglob("*.c.o") if path.name != "oracle.c.o")
        if len(native_objects) != len(policy["units"]) + 1:
            raise RuntimeError("Exact-source native core object inventory is incomplete")
        native_runner = work / "upstream-native"
        ctx.run(["cc", "-O2", *common, "-DDZ_NATIVE_MAIN=1", test, datagen, *native_objects, "-o", native_runner], "upstream-native-build", timeout=300)
        for failure in (False, True):
            label = "upstream-native-" + ("failure-control" if failure else "run")
            transcript = ctx.run([native_runner, *(["--fail-control"] if failure else [])], label, check=not failure)
            if failure:
                control = ctx.receipt["commands"][-1]
                control.update(expected_exit_code=1, deliberate_failure_control=True)
                if control.get("exit_code") != 1:
                    raise RuntimeError("Upstream native CHECK failure did not exit 1")
            else:
                report["native_cases"] = observed_cases(transcript)
                report["native_metrics"] = printed_metrics(transcript)
        ctx.tools()
        primitives = helper(ctx.root / "scripts/primitives.py")
        groups = primitives.inventory(ctx.root)["groups"]
        # Exact header closure: the extracted TU includes zstd_internal.h ->
        # fse.h -> bitstream.h -> bits.h; datagen.c includes only mem.h.
        test_primitives = sorted([*groups["mem"], *groups["bits"]])
        datagen_primitives = sorted(groups["mem"])
        test_object = work / "upstream_cases.cs"
        test_overrides = ctx.artifacts / "upstream-test-overrides.jsonl"
        ctx.run(["dotnet", ctx.compiler, *common, *primitives.unit_flags(ctx, test, core_source, test_primitives),
                 "--override-report", test_overrides, test, "--emit=obj", "-o", test_object], "upstream-emit", timeout=600)
        datagen_object = work / "datagen.cs"
        datagen_overrides = ctx.artifacts / "upstream-datagen-overrides.jsonl"
        ctx.run(["dotnet", ctx.compiler, *common, *primitives.unit_flags(ctx, datagen, core_source, datagen_primitives),
                 "--override-report", datagen_overrides, datagen, "--emit=obj", "-o", datagen_object], "upstream-datagen-emit", timeout=600)
        report["primitive_overrides"] = [primitives.validate_report(ctx, path, unit, core_source, names)
            for path, unit, names in ((test_overrides, test, test_primitives), (datagen_overrides, datagen, datagen_primitives))]
        raw = work / "raw/TranslatedUpstreamZstd"
        ctx.run(["dotnet", ctx.compiler, *core, test_object, datagen_object, "--emit=managedlib",
                 *link_flags("UpstreamZstd", "Managed.Zstd.Upstream"), "-o", raw], "upstream-link", timeout=600)
        # Assembly name follows the generated project; use the actual project path.
        projects = list(raw.glob("*.csproj"))
        if len(projects) != 1:
            raise RuntimeError("Expected one test-only generated project")
        raw_project = projects[0]
        primitives.configure(ctx, raw_project)
        products = {"raw": raw_project}
        ctx.managed("build", raw_project, "upstream-raw-build", "-m:1", timeout=900)
        if "processed" in forms(ctx):
            processed = work / "processed/TranslatedUpstreamZstd"
            shutil.copytree(raw, processed, ignore=shutil.ignore_patterns("bin", "obj"))
            processed_project = processed / raw_project.name
            relocate_project(processed_project, raw)
            ctx.restore(processed_project, "upstream-processed-restore")
            ctx.managed("build", processed_project, "upstream-semantic-build", "-m:1")
            ctx.run(["dotnet", ctx.postprocessor, processed_project, "--in-place"], "upstream-postprocess", timeout=900)
            ctx.managed("build", processed_project, "upstream-processed-build", "-m:1")
            products["processed"] = processed_project
        runner = work / "runner"
        runner.mkdir()
        (runner / "UpstreamTests.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><IsAotCompatible>true</IsAotCompatible><UpstreamProject Condition="'$(UpstreamProject)' == ''">MISSING</UpstreamProject></PropertyGroup><ItemGroup><ProjectReference Include="$(UpstreamProject)" /></ItemGroup></Project>''')
        (runner / "Program.cs").write_text('''using VM = Managed.Zstd.Upstream.UpstreamZstd;
using var owner = new VM.Libc.RuntimeContext();
try { using var binding = owner.Enter(); if(args is ["--fail-control"]) return VM.dz_upstream_fail_control(); for(int i=0;i<6;i++) if(VM.dz_upstream(i)!=0) return 1; return 0; }
catch(VM.Libc.RuntimeTerminationException error) when(ReferenceEquals(error.Owner,owner)) { Console.Error.WriteLine(error); return error.Termination.Status == 0 ? 1 : error.Termination.Status; }
catch(Exception error) { Console.Error.WriteLine(error); return 1; }
''')
        for form in forms(ctx):
            check_product(ctx, form)
            for mode in modes(ctx):
                label = "upstream-" + form + "-" + mode
                row = dict(form=form, mode=mode, status="running", cases=[])
                report["rows"].append(row)
                report_path.write_text(json.dumps(report, indent=2) + "\n")
                output = work / label
                arguments = ["-p:UpstreamProject=" + str(products[form]), "--artifacts-path", work / "intermediates" / label, "-m:1", "-o", output]
                if mode == "jit":
                    ctx.managed("build", runner / "UpstreamTests.csproj", label + "-build", *arguments)
                    command = ["dotnet", output / "UpstreamTests.dll"]
                else:
                    ctx.managed("publish", runner / "UpstreamTests.csproj", label + "-publish", *arguments,
                                "-r", ctx.options.rid, "-p:PublishAot=true", timeout=1800)
                    command = [output / "UpstreamTests"]
                ctx.run([*command, "--fail-control"], label + "-failure-control", check=False)
                control = ctx.receipt["commands"][-1]
                control.update(expected_exit_code=1, deliberate_failure_control=True)
                if control.get("exit_code") != 1:
                    raise RuntimeError("Translated exact upstream CHECK failure did not exit 1")
                transcript = ctx.run(command, label + "-run", timeout=300)
                counts = observed_cases(transcript)
                metrics = printed_metrics(transcript)
                comparisons = [dict(name=name, native=value, translated=metrics[name], status="passed" if value == metrics[name] else "failed")
                               for name, value in report["native_metrics"].items()]
                row.update(passed=sum(counts.values()), groups=len(counts), failed=0, skipped=0, cases=counts,
                           printed_metrics=metrics, exact_native_comparisons=comparisons)
                if any(item["status"] != "passed" for item in comparisons):
                    raise RuntimeError("Native/emitted original upstream context-size observation differs: " + repr(comparisons))
                row["status"] = "passed"
        report["status"] = "passed"
    except BaseException as error:
        report.update(status="failed", error=str(error))
        if report["rows"] and report["rows"][-1]["status"] == "running":
            report["rows"][-1].update(status="failed", error=str(error))
        raise
    finally:
        report_path.write_text(json.dumps(report, indent=2) + "\n")
        ctx.receipt["zstd_upstream_report"] = str(report_path)
        ctx.save()
