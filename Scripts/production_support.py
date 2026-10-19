"""Tool identity and command boundaries for the six legacy production harnesses.

This module is deliberately separate from campaigns.process: CPython's live
specialist hooks have their own lock protocol. No lock encloses a peer session
or an orchestration script. Existing runners retain timeouts and process cleanup.
"""
from __future__ import annotations

import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess as _subprocess
import sys
import uuid

REPO = Path(__file__).resolve().parents[1]
SHARED_LOCK = "/tmp/dotcc-cpython-shared-build.lock"
NATIVE_LOCK = "/tmp/dotcc-cpython-native-build.lock"
TOOLS = {
    "compiler": ("DOTCC_COMPILER", "DotCC", "dotcc.dll"),
    "postprocessor": ("DOTCC_POSTPROCESSOR", "DotCC.PostProcess", "dotcc-postprocess.dll"),
}


def selected_tool(kind="compiler", explicit=None, *, repo=REPO, env=None):
    environment = os.environ if env is None else env
    variable, project, name = TOOLS[kind]
    value = explicit if explicit is not None else environment.get(variable)
    if value is not None:
        if not str(value).strip():
            raise RuntimeError(f"{variable} selects an empty tool path")
        path = Path(value).expanduser().resolve()
    else:
        path = Path(repo) / project / "bin/Release/net10.0" / name
    if not path.is_file():
        raise RuntimeError(f"Missing selected {variable} tool: {path}; no root fallback")
    return path


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def tool_manifest(directory):
    """Include PDBs, satellite resources and every recursive dependency byte."""
    directory = Path(directory)
    return {p.relative_to(directory).as_posix(): digest(p)
            for p in sorted(directory.rglob("*")) if p.is_file()}


def selected_tools_manifest():
    return {str(tool.parent / name): value
            for kind in TOOLS for tool in [selected_tool(kind)]
            for name, value in validate_tool_closure(tool).items()}


def production_sources(root):
    root = Path(root)
    paths = [REPO / "Scripts/production_support.py", REPO / "Scripts/production-common.sh",
             REPO / "Scripts/campaign-common.sh", *(REPO / "Scripts/campaigns").glob("*.py")]
    paths += [p for directory in ("scripts", "config", "src") for p in (root / directory).rglob("*")
              if p.is_file() and not {"bin", "obj", "__pycache__"}.intersection(p.relative_to(root).parts)]
    return {str(p.resolve()): digest(p) for p in sorted(set(paths))}


def validate_tool_closure(tool):
    tool = Path(tool)
    required = [tool, tool.with_suffix(".deps.json"), tool.with_suffix(".runtimeconfig.json")]
    if tool.name == "dotcc.dll":
        required.append(tool.parent / "DotCC.Lib.dll")
    for path in required:
        if not path.is_file():
            raise RuntimeError(f"Selected tool dependency is missing: {path}")
    dependencies = json.loads(tool.with_suffix(".deps.json").read_text())
    for target in dependencies.get("targets", {}).values():
        for library in target.values():
            for section in ("runtime", "native", "resources", "runtimeTargets"):
                for name, metadata in library.get(section, {}).items():
                    if Path(name).name == "_._":
                        continue
                    # SDK output flattens package runtime/native assets; satellite
                    # resources keep their locale directory. No missing asset is
                    # supplied from the repository CLI or global package cache.
                    candidates = [tool.parent / name]
                    if section == "resources" and metadata.get("locale"):
                        candidates.append(tool.parent / metadata["locale"] / Path(name).name)
                    elif section != "resources":
                        candidates.append(tool.parent / Path(name).name)
                    if not any(p.is_file() for p in candidates):
                        raise RuntimeError(f"Selected tool dependency is missing: {name} ({tool})")
    return tool_manifest(tool.parent)


def snapshot_tool(tool, destination):
    """Copy and verify a full closure, retaining any previous snapshot bytes."""
    tool, destination = Path(tool).resolve(), Path(destination).resolve()
    if destination == tool.parent or destination.is_relative_to(tool.parent):
        raise RuntimeError("Tool snapshot must be outside the selected closure")
    before = validate_tool_closure(tool)
    temporary = destination.with_name(destination.name + ".pending-" + uuid.uuid4().hex)
    shutil.copytree(tool.parent, temporary)
    if tool_manifest(temporary) != before or tool_manifest(tool.parent) != before:
        raise RuntimeError(f"Selected tool closure changed while copying: {tool}")
    previous = None
    if destination.exists():
        previous = destination.with_name(destination.name + ".previous-" + uuid.uuid4().hex)
        destination.rename(previous)
    temporary.rename(destination)
    return destination / tool.name, dict(selected=str(tool), executable=tool.name,
        sha256=digest(tool), files=before, previous=None if previous is None else str(previous))


def require_tool_identity(tool, expected, label):
    actual = validate_tool_closure(tool)
    if not expected or actual != expected:
        raise RuntimeError(label + ": selected recursive tool closure differs")
    return actual


def require_root_test_compiler():
    """Repository test binaries carry their own CLI; reject a mismatched override."""
    if "DOTCC_COMPILER" in os.environ:
        selected = selected_tool()
        root = selected_tool(env={})
        require_tool_identity(root, validate_tool_closure(selected), "Repository test compiler")


def existing_product_binding(root, product, profile="default", *, compiler=None):
    """Bind current dependency source/tool/runtime bytes before a P6 consumer.

    This is an input guard. It supplies no runtime, ABI, D10 or provider pass.
    """
    root = Path(root).resolve()
    compiler = selected_tool(explicit=compiler)
    pointer = json.loads((root / f"artifacts/campaign/current-{profile}.json").read_text())
    receipt_path = Path(pointer["receipt"]).resolve()
    receipt = json.loads(receipt_path.read_text())
    if (receipt_path.parent.parent != root / "artifacts/campaign" or
            receipt.get("run_id") != receipt_path.parent.name or
            receipt.get("project") != root.name or receipt.get("profile") != profile or
            receipt.get("action") not in ("translate", "verify") or
            not any(t.get("name") == "translation" and t.get("status") == "passed"
                    for t in receipt.get("tasks", []))):
        raise RuntimeError(f"{product}: current receipt lacks a completed translation")
    if receipt.get("production_inputs") != production_sources(root):
        raise RuntimeError(f"{product}: current production source/command bindings differ or are absent")
    require_tool_identity(compiler, receipt.get("tools", {}).get("DotCC"), product)
    postprocessor = selected_tool("postprocessor")
    require_tool_identity(postprocessor, receipt.get("tools", {}).get("DotCC.PostProcess"), product)
    outputs = receipt.get("outputs", {})
    if set(outputs) != {"raw", "processed"} or pointer.get("outputs") != outputs:
        raise RuntimeError(f"{product}: current delivery does not bind both forms")
    bindings = {}
    for form, output in outputs.items():
        expected = root / "generated" / (product + (".Raw" if form == "raw" else "")) / (product + ".csproj")
        if Path(output.get("project", "")).resolve() != expected or not expected.is_file():
            raise RuntimeError(f"{product}: missing/wrong {form} project")
        hashes = output.get("hashes", {})
        if not hashes or expected.name not in hashes:
            raise RuntimeError(f"{product}: missing current generated source binding")
        for name, value in hashes.items():
            path = expected.parent / name
            if (not path.resolve().is_relative_to(expected.parent) or
                    not path.is_file() or digest(path) != value):
                raise RuntimeError(f"{product}: changed {form} generated input: {name}")
        names = (expected.parent / "Dotcc.SourceFiles.txt").read_text().splitlines()
        actual = {p.relative_to(expected.parent).as_posix() for p in expected.parent.rglob("*.cs")
                  if not {"bin", "obj"}.intersection(p.relative_to(expected.parent).parts)}
        if (not names or len(set(names)) != len(names) or set(names) != actual or
                not set(names).issubset(hashes)):
            raise RuntimeError(f"{product}: incomplete or unmanifested {form} source closure")
        if not any(row.get("label") == f"final-{form}-build" and row.get("status") == "passed"
                   and row.get("exit_code") == 0 for row in receipt.get("commands", [])):
            raise RuntimeError(f"{product}: missing actual final {form} build")
        bindings[form] = dict(project=str(expected), files=hashes)
    # Reuse the already-authored pure CLR-resource/raw-section guard. It reads
    # the real selected PE; it neither loads a managed assembly nor runs a tool.
    import importlib.util
    path = REPO / "sqlite/scripts/product_identity.py"
    spec = importlib.util.spec_from_file_location("production_runtime_identity", path)
    identity = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(identity)
    runtime = identity.runtime_binding(REPO, compiler, root / "generated" / (product + ".Raw"))
    resources = identity.clr_resources(compiler.parent / "DotCC.Lib.dll")
    import struct
    headers = {}
    for header in (REPO / "DotCC.Lib/include").rglob("*.h"):
        data = header.read_bytes()
        position = resources.find(data)
        if (not data or position < 4 or resources.count(data) != 1 or
                struct.unpack_from("<I", resources, position - 4)[0] != len(data)):
            raise RuntimeError(f"{product}: selected tool embeds another current header: {header}")
        headers[str(header)] = digest(header)
    authored = {str(p.relative_to(root)): digest(p) for p in (root / "src").rglob("*")
                if p.is_file() and not {"bin", "obj", "__pycache__"}.intersection(p.relative_to(root).parts)}
    return dict(receipt=str(receipt_path), receiptSha256=digest(receipt_path),
                compiler=str(compiler), compilerFiles=validate_tool_closure(compiler),
                postprocessorFiles=validate_tool_closure(postprocessor), runtime=runtime, headers=headers,
                forms=bindings, authored=authored,
                scope="Existing input bytes only; actual consumer/provider PE and runtime gates remain required")


def _jobs_two(args, style="make"):
    result, index = [], 0
    while index < len(args):
        value = args[index]
        if value in ("-j", "--jobs", "--parallel"):
            index += 1
            if index < len(args) and args[index].isdigit():
                index += 1
            continue
        if re.fullmatch(r"-j\d*|--(?:jobs|parallel)=\d*", value):
            index += 1
            continue
        result.append(value)
        index += 1
    if style == "cmake":
        boundary = result.index("--") if "--" in result else len(result)
        result[boundary:boundary] = ["--parallel", "2"]
        return result
    return result + ["-j2"]


def native_build_command(command):
    """An explicitly identified container/configure build, never a peer run."""
    argv = list(map(str, command))
    if os.name == "nt":
        return argv
    return ["flock", "-n", "--close", SHARED_LOCK,
            "flock", "-n", "--close", NATIVE_LOCK, *argv]


def private_object_output(command, *, env=None):
    """Only the ordinary run's isolated object writer may borrow a read lease.

    The run lock owns the campaign tree; output and optional override report
    must be distinct per-unit paths in that same run. A second writer to the
    exact same object also needs its own exclusive output lease.
    """
    environment = os.environ if env is None else env
    argv = list(map(str, command))
    if len(argv) < 7 or Path(argv[0]).name not in ("dotnet", "dotnet.exe"):
        return None
    # Parse the supported read-only subset, including option operands. Merely
    # finding '--emit=obj' somewhere in argv would accept it as a macro value,
    # another output mode, a response file or a different executable's input.
    values = {'-I', '-D', '-U', '-std', '--overrides-file', '--override-macro',
              '--emit-define', '--type-name', '--class-name', '--namespace'}
    switches = {'--instance-methods', '--state-context', '--literal-pool',
                '--nest-types', '--deduplicate-inline', '-pedantic',
                '-pedantic-errors', '-Wconversion', '-Wno-discarded-qualifiers',
                '-Wimplicit-fallthrough'}
    outputs, reports, sources, modes = [], [], [], 0
    index = 2
    while index < len(argv):
        arg = argv[index]
        if arg == '--emit=obj':
            modes += 1
        elif arg in values or arg in ('-o', '--override-report'):
            index += 1
            if index == len(argv) or argv[index].startswith(('-', '@')):
                return None
            if arg == '-o':
                outputs.append(argv[index])
            elif arg == '--override-report':
                reports.append(argv[index])
        elif arg.startswith('--override-report='):
            reports.append(arg.split('=', 1)[1])
        elif arg in switches:
            pass
        elif any(arg.startswith(option + '=') and arg != option + '=' for option in values):
            pass
        elif any(arg.startswith(option) and arg != option for option in ('-I', '-D', '-U')):
            pass
        elif arg.startswith(('-', '@')):
            return None
        else:
            sources.append(Path(arg))
        index += 1
    if modes != 1 or len(outputs) != 1 or len(sources) != 1:
        return None
    source, output = sources[0], Path(outputs[0])
    if (not source.is_absolute() or source != source.resolve() or source.suffix != '.c'
            or not source.is_file() or not output.is_absolute() or output != output.resolve()):
        return None
    try:
        parts = output.relative_to(REPO).parts
    except ValueError:
        return None
    if (len(parts) != 6 or parts[1:3] != ('build', 'campaign') or parts[4] != 'objects'
            or not re.fullmatch(r'\d{8}-\d{6}-[0-9a-f]{8}', parts[3])
            or not re.fullmatch(r'\d{3,}-[^/]+\.cs', parts[5]) or not output.parent.is_dir()):
        return None
    run = output.parent.parent
    tool = Path(argv[1])
    name = Path(environment.get('DOTCC_COMPILER') or 'dotcc.dll').name
    if (tool.name != name or not tool.is_absolute() or tool != tool.resolve()
            or tool.parent != run / 'tools/DotCC' or not tool.is_file()):
        return None
    # The CLI writes these reports outside the object directory; permit only
    # this object's matching private report, never a shared recipe report.
    expected = REPO / parts[0] / 'artifacts/campaign' / parts[3] / (output.stem + '-overrides.jsonl')
    if len(reports) > 1 or any(Path(p) != expected or Path(p).resolve() != expected for p in reports):
        return None
    # Aliased destinations or lock files would defeat per-output exclusion.
    for path in (output, Path(str(output) + '.lock'), expected):
        if path.is_symlink() or path.exists() and (not path.is_file() or path.stat().st_nlink != 1):
            return None
    return output


def locked_command(command, *, shared_lock=SHARED_LOCK, native_lock=NATIVE_LOCK, env=None):
    """Return a bounded, nonqueuing command; runtime/peer commands are unchanged."""
    argv = list(map(str, command))
    environment = os.environ if env is None else env
    if not argv:
        raise ValueError("Empty production command")
    executable = Path(argv[0]).name
    native = False
    build = False
    override_compilers = [environment.get(name) for name in ("DOTCC_OVERRIDE_COMPILER", "DOTCC_EXECUTABLE")]
    if (executable in ("dotcc", "dotcc.exe", "dotcc-postprocess", "dotcc-postprocess.exe") or
            any(value and executable == Path(value).name for value in override_compilers)):
        build = True
    elif executable in ("dotnet", "dotnet.exe") and len(argv) > 1:
        action = argv[1]
        build = action in ("build", "publish", "restore", "test", "msbuild") or (
            action == "run" and "--no-build" not in argv)
        build |= Path(action).name in ("dotcc.dll", "dotcc-postprocess.dll")
        # Explicit custom tool filenames are accepted by the framework too.
        build |= any(value and Path(value).name == Path(action).name
                     for variable, _, _ in TOOLS.values()
                     if (value := environment.get(variable)))
        restore = environment.get("DOTCC_CAMPAIGN_RESTORE", "normal")
        if action == "restore":
            if restore == "none":
                raise RuntimeError("Restore disabled; use existing assets instead of an explicit restore")
            if restore == "locked" and "--locked-mode" not in argv:
                argv.append("--locked-mode")
        elif action in ("build", "publish", "test", "run"):
            boundary = argv.index("--") if "--" in argv else len(argv)
            if restore == "none" and "--no-restore" not in argv:
                argv.insert(boundary, "--no-restore")
            elif restore == "locked" and "-p:RestoreLockedMode=true" not in argv:
                argv.insert(boundary, "-p:RestoreLockedMode=true")
        if action in ("build", "publish", "test"):
            argv = [a for a in argv if not re.match(r"^(?:-m(?::|$)|-maxcpucount|--maxcpucount)", a)]
            boundary = argv.index("--") if "--" in argv else len(argv)
            argv[boundary:boundary] = ["--disable-build-servers", "-m:1", "-p:BuildInParallel=false", "-p:UseSharedCompilation=false"]
        elif action == "msbuild":
            argv += ["-m:1", "-nodeReuse:false", "-p:UseSharedCompilation=false"]
    elif (executable in ("cc", "c++", "ar", "as", "ld", "configure") or
          re.fullmatch(r"(?:.*-)?(?:gcc|g\+\+|clang|clang\+\+)(?:-\d+)?", executable)):
        native = not any(a in ("--version", "-dumpversion", "-dumpmachine", "-print-multiarch") for a in argv[1:])
        build = native
    elif executable in ("make", "gmake", "ninja"):
        build = native = True
        argv = _jobs_two(argv)
    elif executable == "cmake":
        build = native = "--version" not in argv
        if "--build" in argv:
            argv = _jobs_two(argv, "cmake")
    # Do not acquire a build lock around bash/python/docker/timeout or any
    # interpreter/server invocation. Their individual build children own locks.
    if os.name != "nt" and build:
        output = private_object_output(argv, env=environment)
        if output is not None:
            return ["flock", "-s", "-n", "--close", str(shared_lock),
                    "flock", "-n", "--close", str(output) + ".lock", *argv]
        if native:
            argv = ["flock", "-n", "--close", str(native_lock), *argv]
        argv = ["flock", "-n", "--close", str(shared_lock), *argv]
    return argv


class _ProductionSubprocess:
    """A local adapter; the standard subprocess module is never monkeypatched."""
    def __getattr__(self, name):
        return getattr(_subprocess, name)

    def _call(self, name, args, *positional, **kwargs):
        if kwargs.get("shell"):
            raise ValueError("Production commands must use an argument list, not shell=True")
        command = list(map(str, args))
        if kwargs.get("executable"):
            selected = [str(kwargs["executable"]), *command[1:]]
            if locked_command(selected) != selected:
                # Honor an explicit build executable through flock. Runtime
                # argv[0] protocols keep the standard subprocess behavior.
                command = selected
                kwargs.pop("executable")
        return getattr(_subprocess, name)(locked_command(command), *positional, **kwargs)

    def run(self, args, *positional, input=None, capture_output=False, timeout=None, check=False, **kwargs):
        """Keep run's result/exception contract while draining its owned tree."""
        from campaigns.process import stop_tree
        if input is not None:
            if kwargs.get("stdin") is not None:
                raise ValueError("stdin and input arguments may not both be used.")
            kwargs["stdin"] = _subprocess.PIPE
        if capture_output:
            if kwargs.get("stdout") is not None or kwargs.get("stderr") is not None:
                raise ValueError("stdout and stderr arguments may not be used with capture_output.")
            kwargs["stdout"] = kwargs["stderr"] = _subprocess.PIPE
        # stop_tree owns this session and snapshots detached descendants before
        # signalling the flock wrapper. The separate Popen peer API is unchanged.
        if os.name != "nt":
            kwargs["start_new_session"] = True
        else:
            kwargs["creationflags"] = kwargs.get("creationflags", 0) | _subprocess.CREATE_NEW_PROCESS_GROUP
        with self.Popen(args, *positional, **kwargs) as process:
            try:
                stdout, stderr = process.communicate(input, timeout=timeout)
            except _subprocess.TimeoutExpired as error:
                stop_tree(process)
                if os.name == "nt":
                    error.stdout, error.stderr = process.communicate()
                raise
            except BaseException:
                stop_tree(process)
                raise
            code = process.poll()
            if check and code:
                raise _subprocess.CalledProcessError(code, process.args, output=stdout, stderr=stderr)
        return _subprocess.CompletedProcess(process.args, code, stdout, stderr)

    def Popen(self, args, *positional, **kwargs):
        return self._call("Popen", args, *positional, **kwargs)

    def call(self, args, *positional, **kwargs):
        return self._call("call", args, *positional, **kwargs)

    def check_call(self, args, *positional, **kwargs):
        return self._call("check_call", args, *positional, **kwargs)

    def check_output(self, args, *positional, **kwargs):
        return self._call("check_output", args, *positional, **kwargs)


subprocess = _ProductionSubprocess()


def preflight_overrides():
    for kind, (variable, _, _) in TOOLS.items():
        if variable in os.environ:
            validate_tool_closure(selected_tool(kind))
    for variable in ("DOTCC_OVERRIDE_COMPILER", "DOTCC_EXECUTABLE"):
        if variable in os.environ:
            value = os.environ[variable]
            if not value.strip() or not Path(value).expanduser().is_file():
                raise RuntimeError(f"Missing selected {variable}; no root fallback: {value}")


preflight_overrides()


if __name__ == "__main__":
    # Shell helpers use this path only for individual executable invocations.
    if len(sys.argv) < 2:
        raise SystemExit("Expected an executable and arguments")
    if sys.argv[1] == "--preflight":
        pass
    elif sys.argv[1] == "--tool":
        print(selected_tool(sys.argv[2]))
    else:
        argv = locked_command(sys.argv[1:])
        os.execvp(argv[0], argv)
