"""Bounded subprocess execution with owned process-tree cleanup."""
import os
from pathlib import Path
import signal
import subprocess
import time


class CommandError(RuntimeError):
    def __init__(self, record):
        self.record = record
        super().__init__(f"{record['label']}: {record['status']} (exit {record.get('exit_code')}); "
                         f"see {record['stdout']}")


def descendant_processes(parent):
    """Include Linux descendants that started their own sessions (AOT harnesses)."""
    def identity(path):
        fields = path.read_text().rsplit(")", 1)[1].split()
        return int(fields[1]), fields[19]  # PPID and process start time
    processes = {}
    for path in Path("/proc").glob("[0-9]*/stat"):
        try:
            processes[int(path.parent.name)] = identity(path)
        except (OSError, ValueError, IndexError):
            pass
    owned = {parent}
    while True:
        children = {pid for pid, (ppid, _) in processes.items() if ppid in owned}
        if children <= owned:
            break
        owned.update(children)
    return {pid: processes[pid][1] for pid in owned - {parent}}


def signal_descendants(processes, sig):
    for pid, started in processes.items():
        try:
            # Avoid signalling a reused PID after an earlier termination.
            fields = Path(f"/proc/{pid}/stat").read_text().rsplit(")", 1)[1].split()
            if fields[19] == started:
                os.kill(pid, sig)
        except (OSError, ValueError, IndexError):
            pass


def stop_tree(process):
    if os.name == "nt":
        subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"],
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=False)
    else:
        descendants = descendant_processes(process.pid)
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        signal_descendants(descendants, signal.SIGTERM)
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            pass
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        signal_descendants(descendants, signal.SIGKILL)
    process.wait()


def execute(command, *, label, cwd, output, timeout=600, env=None, check=True,
            separate=False, record=None, cancellation=None):
    command = list(map(str, command))
    environment = os.environ if env is None else env
    if command[0] in ("python", "python3"):
        import sys
        command[0] = environment.get("PYTHON_CMD", sys.executable)
    restore = environment.get("DOTCC_CAMPAIGN_RESTORE", "normal")
    if command[0] == "dotnet" and len(command) > 1:
        if command[1] == "restore":
            if restore == "none":
                # Existing assets are checked by the following managed build.
                project = next((Path(arg) for arg in command[2:] if arg.endswith('.csproj')), None)
                if project is None or not (project.parent / "obj/project.assets.json").is_file():
                    raise RuntimeError("Restore disabled and existing project assets are missing")
                record = record if record is not None else {}
                Path(output).parent.mkdir(parents=True, exist_ok=True)
                Path(output).write_text("Restore disabled; using existing project assets.\n")
                record.update(label=label, command=command, cwd=str(cwd), stdout=str(output),
                              status="skipped", exit_code=0, seconds=0)
                return record
            if restore == "locked" and "--locked-mode" not in command:
                command.append("--locked-mode")
        elif command[1] in ("build", "publish", "test", "run"):
            if restore == "none" and "--no-restore" not in command:
                command.insert(2, "--no-restore")
            if restore == "locked" and "-p:RestoreLockedMode=true" not in command:
                command.insert(2, "-p:RestoreLockedMode=true")
    output = Path(output)
    output.parent.mkdir(parents=True, exist_ok=True)
    record = record if record is not None else {}
    record.update(label=label, command=command, cwd=str(cwd), stdout=str(output),
                  timeout=timeout, status="running", exit_code=None)
    started = time.monotonic()
    process = None
    stderr_path = output.with_suffix(".stderr") if separate else None
    error_stream = None
    try:
        with output.open("wb") as stream:
            if stderr_path:
                error_stream = stderr_path.open("wb")
                record["stderr"] = str(stderr_path)
            process = subprocess.Popen(command, cwd=cwd, env=env, stdout=stream,
                                       stderr=error_stream or subprocess.STDOUT,
                                       start_new_session=os.name != "nt",
                                       creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0)
            try:
                if cancellation is None:
                    record["exit_code"] = process.wait(timeout=timeout)
                else:
                    while True:
                        if cancellation.is_set():
                            record.update(status="cancelled", exit_code=130)
                            stop_tree(process)
                            raise CommandError(record)
                        remaining = timeout - (time.monotonic() - started)
                        if remaining <= 0:
                            raise subprocess.TimeoutExpired(command, timeout)
                        try:
                            record["exit_code"] = process.wait(timeout=min(0.2, remaining))
                            break
                        except subprocess.TimeoutExpired:
                            pass
                record["status"] = "passed" if process.returncode == 0 else "failed"
            except subprocess.TimeoutExpired:
                record.update(status="timed_out", exit_code=124)
                stop_tree(process)
            except BaseException:
                record["status"] = "cancelled"
                stop_tree(process)
                raise
    except OSError as error:
        record.update(status="failed", error=str(error))
    finally:
        if error_stream:
            error_stream.close()
        record["seconds"] = time.monotonic() - started
    if check and record["status"] != "passed":
        raise CommandError(record)
    return record
