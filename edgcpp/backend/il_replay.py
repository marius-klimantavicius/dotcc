"""Optional stock EDG lowered-IL tools and verified checkpoint receipts.

Build with: python3 edgcpp/backend/il_replay.py build --output-dir NEW_DIRECTORY
The ordinary compiler/build tree is read only. Checkpoints are host/configuration
specific; they are not a portable semantic export format.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shlex
import shutil
import struct
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
SCHEMA = 1
ENV_KEYS = ("LANG", "LC_ALL", "LC_CTYPE", "TZ", "SOURCE_DATE_EPOCH")
# These affect packaging only. All other recorded options must be preserved.
PACKAGING_OPTIONS = {"output_dir", "raw_output", "frontend_invocation", "split",
                     "split_size", "format", "method_sharding", "runtime_source"}
CONTROL_OPTIONS = {"compiler", "compiler_timing", "il_tools", "il_checkpoint",
                   "il_replay", "raw_input"}


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def object_digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode()).hexdigest()


def file_record(path):
    path = Path(path).absolute()
    if not path.is_file():
        raise ValueError(f"missing checkpoint input: {path}")
    return {"path": str(path), "sha256": digest(path)}


def verify_files(records):
    for record in records:
        path = Path(record["path"])
        if not path.is_file() or digest(path) != record["sha256"]:
            raise ValueError(f"checkpoint input checksum mismatch: {path}")


def write_json(path, value):
    Path(path).write_text(json.dumps(value, indent=2) + "\n")


def host_abi():
    return {"system": platform.system(), "machine": platform.machine(),
            "byteorder": sys.byteorder, "pointerBytes": struct.calcsize("P"),
            "longBytes": struct.calcsize("l")}


def emission_environment(env):
    return {k: v for k, v in env.items() if k.startswith("EDG_") or k in ENV_KEYS}


def restored_environment(saved):
    env = {k: v for k, v in os.environ.items() if not k.startswith("EDG_") and k not in ENV_KEYS}
    env.update(saved)
    return env


def load_tools(path):
    path = Path(path).resolve()
    tools = json.loads(path.read_text())
    if tools.get("schema") != SCHEMA or tools.get("kind") != "edg-lowered-il-tools":
        raise ValueError("unsupported IL tools receipt")
    identity = tools["compatibility"]
    if object_digest(identity) != tools["compatibilityId"]:
        raise ValueError("IL tools compatibility checksum mismatch")
    if identity["host"] != host_abi():
        raise ValueError("IL tools host ABI mismatch")
    verify_files([tools["writer"], tools["reader"]])
    tools["receipt"] = file_record(path)
    return tools


def serialized_options(args):
    def convert(value):
        if isinstance(value, Path):
            return str(value.resolve())
        if isinstance(value, list):
            return [convert(item) for item in value]
        return value
    return {key: convert(value) for key, value in vars(args).items()
            if key not in CONTROL_OPTIONS and key not in {"output_dir", "raw_output", "frontend_invocation"}}


def restore_options(parser, args, receipt, argv):
    """Restore recorded semantics; explicit conflicts fail before invoking EDG."""
    explicit = set()
    for action in parser._actions:
        if any(token == option or token.startswith(option + "=") or
               (option in ("-I", "-D") and token.startswith(option) and token != option)
               for option in action.option_strings for token in argv):
            explicit.add(action.dest)
    if args.source:
        explicit.add("source")
    actions = {action.dest: action for action in parser._actions}
    for key, value in receipt["options"].items():
        action = actions.get(key)
        if not action:
            raise ValueError(f"checkpoint option is no longer supported: {key}")
        if key in explicit and key not in PACKAGING_OPTIONS:
            current = serialized_options(args).get(key)
            if current != value:
                raise ValueError(f"explicit --{key.replace('_', '-')} conflicts with checkpoint")
        if key in PACKAGING_OPTIONS and key in explicit:
            continue
        if action.type is Path:
            value = [Path(item) for item in value] if isinstance(value, list) else (Path(value) if value else None)
        setattr(args, key, value)


def load_checkpoint(directory, tools):
    directory = Path(directory).resolve()
    receipt_path = directory / "checkpoint.json"
    receipt = json.loads(receipt_path.read_text())
    if receipt.get("schema") != SCHEMA or receipt.get("kind") != "edg-lowered-il-checkpoint" or receipt.get("status") != "ready":
        raise ValueError("checkpoint is incomplete or unsupported")
    expected = receipt.pop("receiptSha256", None)
    if object_digest(receipt) != expected:
        raise ValueError("checkpoint receipt checksum mismatch")
    receipt["receiptSha256"] = expected
    if receipt["compatibilityId"] != tools["compatibilityId"]:
        raise ValueError("checkpoint IL source/configuration/host ABI mismatch")
    verify_files([receipt["il"], *receipt["inputs"]])
    if Path(receipt["il"]["path"]).resolve() != directory / "program.cil":
        raise ValueError("checkpoint IL path does not match its directory")
    receipt["receipt"] = file_record(receipt_path)
    return receipt


def dependency_inputs(writer, arguments, sources, env, directory):
    """Use stock dependency output; .i/.ii files are already self-contained."""
    options = arguments[:arguments.index("--")]
    paths = {Path(source).resolve() for source in sources}
    commands = []
    for index, source in enumerate(sources):
        source = Path(source).resolve()
        if source.suffix in (".i", ".ii"):
            continue
        command = [str(writer), *options, "--dependencies", "--", str(source)]
        result = subprocess.run(command, env=env, capture_output=True, text=True)
        (directory / f"dependencies-{index:04d}.stdout").write_text(result.stdout)
        (directory / f"dependencies-{index:04d}.stderr").write_text(result.stderr)
        commands.append({"arguments": command, "exitCode": result.returncode})
        if result.returncode:
            raise ValueError(f"checkpoint dependency discovery failed for {source}; see {directory}")
        for line in result.stdout.splitlines():
            # EDG emits one target: dependency per line. Refuse ambiguous
            # spellings instead of silently omitting an input.
            if ": " not in line:
                raise ValueError(f"unrecognized EDG dependency: {line!r}")
            dependency = line.split(": ", 1)[1]
            path = Path(dependency)
            if not path.is_file():
                raise ValueError(f"unresolved EDG dependency: {dependency!r}")
            paths.add(path.resolve())
    return paths, commands


def create_checkpoint(directory, tools, arguments, env, sources, options, metadata, extra_inputs=()):
    directory = Path(directory).resolve()
    directory.mkdir(parents=True, exist_ok=False)
    writer = tools["writer"]["path"]
    paths, dependencies = dependency_inputs(writer, arguments, sources, env, directory)
    paths.update(Path(path).resolve() for path in extra_inputs if path)
    pruning = env.get("EDG_DOT_PRUNABLE_SOURCES")
    if pruning:
        paths.add(Path(pruning).resolve())
    inputs = [file_record(path) for path in sorted(paths)]
    command = [writer, "--trace_includes", "--output", str(directory / "program.cil"), *arguments]
    started = time.monotonic()
    # Stream diagnostics to retained files; a large frontend need not buffer
    # its output or lose it when the subsequent backend reports a limitation.
    with (directory / "writer.stdout").open("wb") as stdout, (directory / "writer.stderr").open("wb") as stderr:
        result = subprocess.run(command, env=env, stdout=stdout, stderr=stderr)
    write_json(directory / "writer.json", {"arguments": command, "exitCode": result.returncode,
               "elapsedSeconds": time.monotonic() - started, "dependencies": dependencies})
    if result.returncode:
        raise ValueError(f"IL writer failed ({result.returncode}); see {directory / 'writer.stderr'}")
    # Ensure the real compilation did not discover an include missing from
    # the dependency pass (for example through a time-dependent condition).
    for line in (directory / "writer.stderr").read_text().splitlines():
        path = Path(line.strip())
        try:
            actual_file = path.is_file()
        except OSError:  # A diagnostic/source line is not necessarily a path.
            actual_file = False
        if actual_file and path.resolve() not in paths:
            raise ValueError(f"unrecorded include encountered by IL writer: {path}")
    verify_files(inputs)
    receipt = {"schema": SCHEMA, "kind": "edg-lowered-il-checkpoint", "status": "ready",
               "compatibilityId": tools["compatibilityId"], "writer": tools["writer"],
               "toolsReceipt": tools["receipt"], "il": file_record(directory / "program.cil"),
               "inputs": inputs, "frontendArguments": arguments, "cwd": str(Path.cwd()),
               "environment": emission_environment(env), "options": options, "metadata": metadata}
    receipt["receiptSha256"] = object_digest(receipt)
    write_json(directory / "checkpoint.json", receipt)
    return load_checkpoint(directory, tools)


def replay(receipt, tools):
    # Called only after validation. Keep backend execution provenance separate
    # from the completed checkpoint, even if code generation fails.
    attempts = Path(receipt["il"]["path"]).parent / "emissions"
    attempts.mkdir(exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=attempts))
    command = [tools["reader"]["path"], receipt["il"]["path"]]
    start = time.monotonic()
    with (attempt / "raw.cs").open("wb") as stdout, (attempt / "stderr").open("wb") as stderr:
        completed = subprocess.run(command, cwd=receipt["cwd"],
                                   env=restored_environment(receipt["environment"]), stdout=stdout, stderr=stderr)
    write_json(attempt / "result.json", {"arguments": command, "cwd": receipt["cwd"],
               "checkpointReceipt": receipt["receipt"], "reader": tools["reader"],
               "backendId": tools["backendId"], "exitCode": completed.returncode,
               "elapsedSeconds": time.monotonic() - start,
               "output": file_record(attempt / "raw.cs"), "diagnostics": file_record(attempt / "stderr")})
    result = subprocess.CompletedProcess(command, completed.returncode,
                                          (attempt / "raw.cs").read_text(), (attempt / "stderr").read_text())
    result.il_attempt = file_record(attempt / "result.json")
    return result


def build_tools(output, compile_commands, jobs=4):
    """Build optional isolated tools from a frozen copy of the current profile."""
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=False)
    commands = [item for item in json.loads(Path(compile_commands).read_text())
                if "/cpfe-dot.dir/" in item["command"]]
    if not commands:
        raise ValueError("compile_commands.json has no cpfe-dot profile")
    arguments = shlex.split(commands[0]["command"])
    compiler = arguments[0]
    definitions = [arg[2:] for arg in arguments if arg.startswith("-D") and not arg.startswith(
        ("-DIL_SHOULD_BE_WRITTEN_TO_FILE=", "-DBACK_END_SHOULD_BE_CALLED="))]
    definitions += ["IL_SHOULD_BE_WRITTEN_TO_FILE=1", "BACK_END_SHOULD_BE_CALLED=0"]
    include_args = [arg[2:] for arg in arguments if arg.startswith("-I")]
    generated = next(Path(path) for path in include_args if (Path(path) / "cmake_defines.h").is_file())
    snapshot = output / "snapshot"
    source_root = Path(commands[0]["file"]).parent
    for name, origin in (("src", source_root), ("util", source_root.parent / "util"), ("includes", generated)):
        target = snapshot / name
        target.mkdir(parents=True)
        for path in origin.iterdir():
            if path.is_file():
                shutil.copy2(path, target / path.name)
    # Canonical backend-only edits do not require prepare.py or a main rebuild.
    # Target predefined macros remain part of the frozen frontend identity.
    for path in (ROOT / "backend").glob("dot_gen*.[ch]"):
        shutil.copy2(path, snapshot / "src" / path.name)
    for path in (ROOT / "backend/runtime").glob("*-bindings.def"):
        shutil.copy2(path, snapshot / "src" / path.name)
    shutil.copy2(Path(__file__).with_name("il_replay_main.cpp"), output / "reader.cpp")
    (output / "read_unit.cpp").write_text('#include "snapshot/src/il_read.c"\n')
    (output / "init_unit.cpp").write_text('#include "snapshot/src/fe_init.c"\n')
    writer_sources = [snapshot / ("util" if "/util/" in item["file"] else "src") / Path(item["file"]).name
                      for item in commands if Path(item["file"]).name not in ("dot_gen_be.c", "decode.c")]
    core = [path for path in writer_sources if path.name not in ("cfe.c", "fe_init.c", "il_read.c")]
    quote = lambda value: '"' + str(value).replace('\\', '/').replace('"', '\\"') + '"'
    cmake = ['cmake_minimum_required(VERSION 3.19)', 'project(dot_il_replay LANGUAGES CXX)',
             'set(CMAKE_CXX_STANDARD 14)', 'set(CMAKE_CXX_STANDARD_REQUIRED ON)',
             'add_compile_options(-O1 -g0 -fno-exceptions -fno-rtti -ffunction-sections -fdata-sections)',
             'add_link_options(-Wl,--gc-sections)',
             'include_directories(' + ' '.join(quote(snapshot / name) for name in ("src", "includes")) + ')',
             'add_compile_definitions(' + ' '.join(quote(item) for item in definitions) + ')',
             'set_source_files_properties(' + ' '.join(quote(path) for path in writer_sources +
                 [snapshot / "src/dot_gen_be.c", snapshot / "util/decode.c"]) + ' PROPERTIES LANGUAGE CXX)',
             'add_library(replay_support OBJECT ' + ' '.join(quote(path) for path in core) + ')',
             'add_executable(writer $<TARGET_OBJECTS:replay_support> ' + ' '.join(quote(path) for path in writer_sources if path not in core) + ')',
             'add_executable(reader $<TARGET_OBJECTS:replay_support> reader.cpp read_unit.cpp init_unit.cpp ' +
                 quote(snapshot / "src/dot_gen_be.c") + ' ' + quote(snapshot / "util/decode.c") + ')',
             'target_compile_options(reader PRIVATE -UBACK_END_SHOULD_BE_CALLED -DBACK_END_SHOULD_BE_CALLED=1)']
    (output / "CMakeLists.txt").write_text('\n'.join(cmake) + '\n')
    write_json(output / "original-compile-commands.json", commands)
    with (output / "build.log").open("w") as log:
        subprocess.run(["cmake", "-S", str(output), "-B", str(output / "build"), "-G", "Ninja",
                        "-DCMAKE_EXPORT_COMPILE_COMMANDS=ON", "-DCMAKE_CXX_COMPILER=" + compiler], stdout=log, stderr=log, check=True)
        subprocess.run(["cmake", "--build", str(output / "build"), "--parallel", str(jobs)], stdout=log, stderr=log, check=True)
    stock, backend = {}, {}
    for path in sorted(snapshot.rglob('*')):
        if path.is_file():
            relative = str(path.relative_to(snapshot))
            is_backend = path.name.startswith("dot_gen") and not path.name.startswith("dot_gen_target_macros") or path.name.endswith("-bindings.def")
            (backend if is_backend else stock)[relative] = digest(path)
    backend["reader.cpp"] = digest(output / "reader.cpp")
    compatibility = {"host": host_abi(), "definitions": sorted(definitions), "supportSources": stock,
                     "hostCompiler": subprocess.check_output([compiler, "--version"], text=True).splitlines()[0],
                     "readerProtocol": 1}
    manifest = {"schema": SCHEMA, "kind": "edg-lowered-il-tools", "compatibility": compatibility,
                "compatibilityId": object_digest(compatibility), "backendSources": backend,
                "backendId": object_digest(backend), "readerEntrypoint": file_record(output / "reader.cpp"),
                "writer": file_record(output / "build/writer"), "reader": file_record(output / "build/reader")}
    write_json(output / "tools.json", manifest)
    return output / "tools.json"


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)
    build = subparsers.add_parser("build")
    build.add_argument("--output-dir", required=True, type=Path)
    build.add_argument("--compile-commands", default=ROOT / "build/compile_commands.json", type=Path)
    build.add_argument("--jobs", default=4, type=int)
    args = parser.parse_args()
    if args.jobs < 1:
        parser.error("--jobs must be positive")
    print(build_tools(args.output_dir, args.compile_commands, args.jobs))
