"""Small recipe API: project knowledge lives outside the execution engine."""
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable


@dataclass(frozen=True)
class Source:
    name: str
    url: str
    archive: Path
    directory: Path
    prefix: str
    sha256: str | None = None
    required: tuple[str, ...] = ()
    group: str = "product"
    files: dict[str, str] = field(default_factory=dict)


@dataclass(frozen=True)
class Unit:
    path: Path
    flags: tuple[str, ...] = ()


@dataclass
class Translation:
    units: list[Unit]
    flags: list[str]
    link_flags: list[str]
    objects: bool = False
    configure: Callable | None = None
    validate_objects: Callable | None = None
    finish: Callable | None = None


@dataclass(frozen=True)
class Suite:
    name: str
    command: tuple[str, ...]
    timeout: int = 1800
    matrix: bool = False
    platforms: tuple[str, ...] = ()
    profiles: tuple[str, ...] = ()
    dependencies: tuple[str, ...] = ()
    scope: str = "project test"
    required_profiles: tuple[str, ...] = ()
    run: Callable | None = None


@dataclass(frozen=True)
class Consumer:
    project: str = "samples/ManagedConsumer/ManagedConsumer.csproj"
    property: str = "TranslatedProject"
    arguments: tuple[str, ...] = ()
    build_solution: bool = False
    build_arguments: tuple[str, ...] = ()


@dataclass
class Recipe:
    name: str
    product: str
    profiles: tuple[str, ...]
    projects: tuple[str, ...]
    sources: Callable
    prepare: Callable | None = None
    suites: tuple[Suite, ...] = ()
    default_suites: tuple[str, ...] = ()
    verify_suites: tuple[str, ...] = ()
    translate: Callable | None = None
    before_translate: Callable | None = None
    after_translate: Callable | None = None
    consumer: Consumer = field(default_factory=Consumer)
    dependencies: tuple[str, ...] = ()
    supports_pristine: bool = False
    supports_fast: bool = False
    validate_options: Callable | None = None
    before_suite: Callable | None = None
    script_arguments: Callable | None = None
    legacy_owned: tuple[str, ...] = ()

    @property
    def default_profile(self):
        return self.profiles[0]


@dataclass(frozen=True)
class Task:
    name: str
    action: Callable
    dependencies: tuple[str, ...] = ()


def ordered_tasks(tasks: list[Task]) -> list[Task]:
    """Validate the entire graph before running anything."""
    by_name = {task.name: task for task in tasks}
    if len(by_name) != len(tasks):
        raise ValueError("Duplicate task name")
    visiting, done, result = set(), set(), []

    def visit(name):
        if name not in by_name:
            raise ValueError(f"Unknown task dependency: {name}")
        if name in visiting:
            raise ValueError(f"Task dependency cycle: {name}")
        if name in done:
            return
        visiting.add(name)
        for dependency in by_name[name].dependencies:
            visit(dependency)
        visiting.remove(name)
        done.add(name)
        result.append(by_name[name])

    for name in by_name:
        visit(name)
    return result
