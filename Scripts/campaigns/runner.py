"""Run context, policy propagation, tool preparation, and attempt receipts."""
import json
import os
from pathlib import Path
import shutil
import sys
import threading
import time
import uuid
from .identity import HashPolicy, write_json
from .layout import Layout
from .process import execute


class Context:
    def __init__(self, repo, recipe, options, session=None):
        self.repo, self.recipe, self.options = Path(repo), recipe, options
        self.root = self.repo / recipe.name
        self.profile = options.profile or recipe.default_profile
        self.layout = Layout(self.root, recipe.product, recipe.default_profile)
        self.run_id = time.strftime("%Y%m%d-%H%M%S") + "-" + uuid.uuid4().hex[:8]
        self.artifacts = self.root / "artifacts/campaign" / self.run_id
        self.work = self.root / "build/campaign" / self.run_id
        self.receipt = dict(schema_version=1, project=recipe.name, action=options.action,
                            profile=self.profile, hash_policy=options.hashes,
                            run_id=self.run_id, status="running", started=time.time(),
                            staging=str(self.work.relative_to(self.root)), python=sys.executable,
                            warnings=[], commands=[], tasks=[], outputs={})
        self.receipt["selection"] = {name: getattr(options, name, None) for name in
                                     ("form", "mode", "rid", "suite", "fetch", "tools", "restore", "fast", "jobs")}
        self.policy = HashPolicy(options.hashes, self.receipt["warnings"])
        self.session = session if session is not None else set()
        self._mutex = threading.RLock()
        self.cancellation = threading.Event()
        self.sources = {}
        self.env = dict(os.environ, PYTHON_CMD=sys.executable, DOTCC_CAMPAIGN_HASHES=options.hashes,
                        DOTCC_CAMPAIGN_FETCH=options.fetch, DOTCC_CAMPAIGN_RESTORE=options.restore,
                        PYTHONPATH=str(self.repo / "Scripts") + os.pathsep + os.environ.get("PYTHONPATH", ""),
                        TMPDIR=str(self.work / "tmp"))

    def start(self):
        self.artifacts.mkdir(parents=True)
        (self.work / "tmp").mkdir(parents=True)
        self.save()
        print(f"{self.recipe.name}: receipt {self.artifacts / 'receipt.json'}", flush=True)

    def save(self):
        with self._mutex:
            write_json(self.artifacts / "receipt.json", self.receipt)

    def finish(self, error=None):
        from .process import CommandError
        status = ("passed" if error is None else "cancelled" if isinstance(error, KeyboardInterrupt)
                  else error.record["status"] if isinstance(error, CommandError) else "failed")
        self.receipt.update(status=status, finished=time.time())
        if error:
            self.receipt["error"] = str(error)
            self.receipt["previous_product_stale_for_attempt"] = self.layout.directory(self.profile).exists()
            for task in self.receipt.get("planned_tasks", []):
                if task["status"] == "pending":
                    task["status"] = "blocked"
        self.save()
        write_json(self.root / "artifacts/campaign/latest-attempt.json",
                   {"receipt": str(self.artifacts / "receipt.json"), "status": self.receipt["status"]})

    def run(self, command, label, timeout=600, *, cwd=None, check=True, separate=False, env=None):
        command = list(map(str, command))
        if command[0] in ("python", "python3"):
            command[0] = sys.executable
        if command[0] == "dotnet" and len(command) > 1:
            action = command[1]
            if action == "restore":
                if self.options.restore == "none":
                    raise RuntimeError("Restore requested with --restore none; provide existing assets")
                if self.options.restore == "locked":
                    command.append("--locked-mode")
            elif action in ("build", "publish", "test", "run"):
                if self.options.restore == "none" and "--no-restore" not in command:
                    command.insert(2, "--no-restore")
                if self.options.restore == "locked":
                    command.insert(2, "-p:RestoreLockedMode=true")
        with self._mutex:
            record = {}
            pending = {"label": label, "command": command, "status": "running"}
            self.receipt["commands"].append(pending)
            self.save()
        print(f"{self.recipe.name}: {label}", flush=True)
        try:
            execute(command, label=label, cwd=cwd or self.repo,
                    output=self.artifacts / (label + ".log"), timeout=timeout,
                    env={**self.env, **(env or {})}, check=check, separate=separate, record=record,
                    cancellation=self.cancellation)
        finally:
            with self._mutex:
                pending.update(record)
                log = self.artifacts / (label + ".log")
                if log.exists():
                    for line in log.read_text(errors="replace").splitlines():
                        if line.startswith("WARNING: [test] "):
                            # Behavioral observations are independent of the
                            # selected provenance/hash enforcement policy.
                            message = line.removeprefix("WARNING: ")
                            if message not in self.receipt["warnings"]:
                                self.receipt["warnings"].append(message)
                                print(line, file=sys.stderr, flush=True)
                        elif self.policy.mode == "warn" and line.startswith("WARNING: "):
                            self.policy.issue(line.removeprefix("WARNING: "))
                self.save()
        return (self.artifacts / (label + ".log")).read_text(errors="replace")

    def script(self, name, *args, label=None, timeout=600):
        path = self.root / "scripts" / name
        command = [sys.executable if path.suffix == ".py" else "bash", path, *args]
        return self.run(command, label or path.stem, timeout)

    def managed(self, action, project, label, *args, timeout=900):
        return self.run(["dotnet", action, project, "-c", "Release", "--nologo", *args], label, timeout)

    def restore(self, project, label):
        if self.options.restore == "none":
            if not (Path(project).parent / "obj/project.assets.json").exists():
                raise RuntimeError(f"Missing restore assets for --restore none: {project}")
            return
        self.run(["dotnet", "restore", project, "--nologo"], label)

    def tools(self):
        self.receipt["dotnet_sdk"] = self.run(["dotnet", "--version"], "dotnet-version").strip()
        for project, executable in (("DotCC", "dotcc.dll"), ("DotCC.PostProcess", "dotcc-postprocess.dll")):
            if self.options.tools == "build" and project not in self.session:
                from .delivery import lock
                with lock(self.repo / "Scripts/.campaign-tools.lock"):
                    self.managed("build", self.repo / project / (project + ".csproj"),
                                 project + "-build", "-p:UseLocalLalrCc=false")
                self.session.add(project)
            source = self.repo / project / "bin/Release/net10.0"
            if project == "DotCC" and os.environ.get("DOTCC_COMPILER"):
                source = Path(os.environ["DOTCC_COMPILER"]).resolve().parent
                executable = Path(os.environ["DOTCC_COMPILER"]).name
            if not (source / executable).is_file():
                raise RuntimeError(f"Missing tool: {source / executable}; use --tools build")
            destination = self.work / "tools" / project
            destination.mkdir(parents=True, exist_ok=True)
            paths = [path for path in source.iterdir() if path.suffix in (".dll", ".json")]
            for path in paths:
                shutil.copy2(path, destination / path.name)
            self.receipt.setdefault("tools", {})[project] = self.policy.manifest(paths, source)
            if project == "DotCC":
                self.compiler = destination / executable
            else:
                self.postprocessor = destination / executable
        self.save()

    def task(self, name, action):
        record = {"name": name, "status": "running"}
        planned = next((task for task in self.receipt.get("planned_tasks", []) if task["name"] == name), record)
        planned["status"] = "running"
        self.receipt["tasks"].append(record)
        self.save()
        try:
            action()
            record["status"] = "passed"
        except BaseException as error:
            from .process import CommandError
            record["status"] = ("cancelled" if isinstance(error, KeyboardInterrupt) else
                                error.record["status"] if isinstance(error, CommandError) else "failed")
            raise
        finally:
            planned["status"] = record["status"]
            self.save()
