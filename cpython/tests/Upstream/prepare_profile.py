"""Prepare a private static helper oracle using campaign process/staging support.

The callable accepts an ordinary campaign context (root, sources, run, receipt).
No acquisition, product publication, emitted-source modification, or core emission.
The CLI is a narrow local preparation adapter; A may call prepare(ctx) from verify.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys


def prepare(ctx, base):
    if sys.platform != "linux":
        raise RuntimeError("Preparing this native helper oracle requires Linux; use prepared exact-pin tools on other orchestration hosts")
    from campaigns.delivery import lock
    definition = Path(__file__).with_name("helper-profile.json")
    profile = json.loads(definition.read_text())
    pin = json.loads((definition.parent / profile["pin_metadata"]).read_text())
    source = ctx.sources["product"]
    base = Path(base).resolve()
    if base.exists():
        raise RuntimeError("Refusing to replace existing private helper build: " + str(base))
    staged, build = base / "source", base / "build"
    # Same ordinary-copy staging mechanism as campaign preparation; no hardlinks.
    shutil.copytree(source, staged, ignore=shutil.ignore_patterns("__pycache__", "*.pyc"))
    build.mkdir()
    (build / "Modules").mkdir()
    native = json.loads((definition.parent / profile["base_profile"]).read_text())
    args = [arg for arg in native["native_configure"] if arg != "--disable-test-modules"]
    args += profile["configure_additions"]
    baseline = (definition.parent / profile["base_setup"]).read_text()
    lines = baseline.splitlines()
    selected = set(profile["static_modules"])
    disabled_at = lines.index("*disabled*")
    static = lines[:disabled_at]
    disabled = [line for line in lines[disabled_at + 1:] if line not in selected]
    declared = {line.split()[0] for line in static if line and not line.startswith(("#", "*"))}
    template = (source / "Modules/Setup.stdlib.in").read_text().splitlines()
    rows = []
    for name in profile["static_modules"]:
        matches = [(n, re.sub(r"^@MODULE_[A-Z0-9_]+_TRUE@", "", line))
                   for n, line in enumerate(template, 1)
                   if re.match(r"^@MODULE_[A-Z0-9_]+_TRUE@" + re.escape(name) + r"\s", line)]
        if len(matches) != 1:
            raise RuntimeError("Expected one real Setup row for " + name)
        number, upstream_row = matches[0]
        row = profile["setup_overrides"].get(name, upstream_row)
        paths = ["Modules/" + word for word in upstream_row.split()[1:] if word.endswith(".c")]
        for path in paths:
            if not (source / path).is_file():
                raise RuntimeError("Missing actual helper source " + path)
        rows.append({"module": name, "setup_path": "Modules/Setup.stdlib.in", "line": number,
                     "upstream_row": upstream_row, "setup_row": row, "sources": paths,
                     "sha256": {p: hashlib.sha256((source / p).read_bytes()).hexdigest() for p in paths}})
        if name not in declared:
            static.append(row)
    setup = "\n".join([*static, "*disabled*", *disabled]) + "\n"
    (build / "Modules/Setup.local").write_text(setup)
    (staged / "Modules/Setup.local").write_text(setup)
    manifest = {"requested_revision": pin["revision"], "version": pin["version"],
                "acquired_source": str(source), "private_source": str(staged),
                "configure": args, "modules": rows, "scope": profile["scope"],
                "external_dependencies": profile["external_dependencies"], "status": "prepared-not-built"}
    manifest_path = base / "helper-sources.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    ctx.receipt["helper_profile"] = str(manifest_path)
    with lock(Path("/tmp/dotcc-cpython-native-build.lock")):
        ctx.run(["bash", staged / "configure", *args], "helper-configure", 600, cwd=build)
    return finish(ctx, base)


def finish(ctx, base):
    from campaigns.delivery import lock
    definition = Path(__file__).with_name("helper-profile.json")
    profile = json.loads(definition.read_text())
    pin = json.loads((definition.parent / profile["pin_metadata"]).read_text())
    manifest_path = base / "helper-sources.json"
    manifest = json.loads(manifest_path.read_text())
    staged, build = base / "source", base / "build"
    identity = json.loads((definition.parent / profile["bootstrap_identity"]).read_text())
    if identity["requested_revision"] != pin["revision"] or manifest["requested_revision"] != pin["revision"]:
        raise RuntimeError("Bootstrap/native source revision mismatch")
    freezer = definition.parent / profile["bootstrap_freezer"]
    private_freezer = base / "tools/_freeze_module"
    private_freezer.parent.mkdir(exist_ok=True)
    shutil.copy2(freezer, private_freezer)
    manifest["bootstrap_generator"] = {"original": str(freezer.resolve()), "private": str(private_freezer),
        "sha256": hashlib.sha256(private_freezer.read_bytes()).hexdigest(), "reason": profile["bootstrap_reason"],
        "identity": identity}
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    with lock(Path("/tmp/dotcc-cpython-native-build.lock")):
        ctx.run(["make", "-j2", "FREEZE_MODULE_BOOTSTRAP=" + str(private_freezer),
                 "FREEZE_MODULE_BOOTSTRAP_DEPS=" + str(private_freezer),
                 "python", "libpython3.13.a", "pybuilddir.txt"], "helper-build", 1800, cwd=build)
    # No build lock while executing the private control interpreter.
    ctx.run([build / "python", "-I", "-c",
             "import sys,sysconfig,json,importlib; names=" + repr(profile["static_modules"]) + "; "
             "modules={n:importlib.import_module(n).__spec__.origin for n in names}; "
             "assert all(v=='built-in' for v in modules.values()),modules; "
             "assert sys.version.split()[0]==" + repr(pin["version"]) + "; "
             "assert not sysconfig.get_config_var('Py_GIL_DISABLED'); "
             "assert sysconfig.get_config_var('WITH_PYMALLOC')==1; "
             "print(json.dumps({'version':sys.version,'modules':modules,'wchar':sysconfig.get_config_var('SIZEOF_WCHAR_T'),'builtin_module_names':sys.builtin_module_names}))"],
            "helper-identity", 60, cwd=build)
    manifest["status"] = "native-static-built; translated-test-profile-unbuilt"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    snapshot(ctx, base)
    return build, staged


def snapshot(ctx, base):
    from campaigns.delivery import lock
    manifest_path = base / 'helper-sources.json'
    manifest = json.loads(manifest_path.read_text())
    definition = json.loads(Path(__file__).with_name('helper-profile.json').read_text())
    if {m['module'] for m in manifest['modules']} != set(definition['static_modules']):
        raise RuntimeError('Profile definition/source rows differ; prepare a new private tree')
    source, build = base/'source', base/'build'
    variables = ['LIBRARY_OBJS', 'LIBHACL_SHA2_OBJS', 'MODBUILT_NAMES', 'MODDISABLED_NAMES',
                 'LOCALMODLIBS', 'MODLIBS', 'LIBS', 'SYSLIBS', 'FROZEN_FILES_IN', 'FROZEN_FILES_OUT',
                 'PY_CFLAGS', 'PY_CORE_CFLAGS', 'PY_STDMODULE_CFLAGS', 'CONFIG_ARGS', 'SOABI']
    for module in definition['static_modules']:
        variables += ['MODULE_' + module.upper() + suffix for suffix in ('_CFLAGS', '_LDFLAGS', '_DEPS')]
    extra = base/'helper-inventory.mk'
    extra.write_text(''.join('$(info ' + name + '=$(' + name + '))\n' for name in variables) + '.PHONY: helper-inventory\nhelper-inventory:\n\t@:\n')
    with lock(Path('/tmp/dotcc-cpython-native-build.lock')):
        raw = ctx.run(['make', '--no-print-directory', '-s', '-f', 'Makefile', '-f', extra, 'helper-inventory'], 'helper-make-inventory', 60, cwd=build)
    values = dict(line.split('=',1) for line in raw.splitlines() if '=' in line)
    manifest['native_make_variables'] = values
    manifest['common_translation_contract'] = {
        'macro_policy': 'Core and builtin module options are per-unit, in actual argv order; never apply Py_BUILD_CORE to every helper.',
        'helper_api_roles': {
            '_testcapi': 'Public API by default; only source-declared PYTESTCAPI_NEED_INTERNAL_API opts into internals.',
            '_testlimitedcapi': 'Source-selected Py_LIMITED_API versions; parts.h rejects inherited Py_BUILD_CORE.',
            '_testinternalcapi': 'Internal API through actual builtin/module role and pyport.h implications.'},
        'evidence': 'Review actual translated receipt module/source rows, ordered command argv, source/header guards and compiled static registrations. Native module CFLAGS below are oracle evidence only.',
        'include_order': ['DotCC test-profile configuration (16-bit wchar)', 'same-pin Include', 'same-pin Include/internal', 'same-pin source root', 'shared DotCC C headers'],
        'core': 'Reuse actual product closure/evaluator/GC/GIL; deduplicate helper source paths and regenerate separate static Modules/config.c; do not reuse native pyconfig.'}
    for module in manifest['modules']:
        for path, observed in list(module['sha256'].items()):
            current = hashlib.sha256((source/path).read_bytes()).hexdigest()
            if current != observed:
                # Provenance drift is advisory; a missing source still fails.
                manifest.setdefault('provenance_warnings', []).append({'path': path, 'recorded': observed, 'observed': current})
                module['sha256'][path] = current
        module['native_cflags'] = values.get('MODULE_' + module['module'].upper() + '_CFLAGS', '')
        module['native_ldflags'] = values.get('MODULE_' + module['module'].upper() + '_LDFLAGS', '')
    assets = [build/'pyconfig.h',build/'Modules/config.c',build/'pybuilddir.txt']
    assets += list((build/'Python/frozen_modules').glob('*.h'))
    assets += list((source/'Python/frozen_modules').glob('*.h'))
    assets += list(build.glob('build/lib.*/_sysconfigdata*.py'))
    manifest['generated_assets'] = [{'path': str(p.relative_to(base)), 'sha256': hashlib.sha256(p.read_bytes()).hexdigest(),
        'role': 'native controls/generator evidence; DotCC ABI/platform-dependent assets must be prepared from its reviewed config'} for p in sorted(set(assets)) if p.is_file()]
    manifest['source_count'] = len({p for m in manifest['modules'] for p in m['sources']})
    manifest['module_count'] = len(manifest['modules'])
    manifest['frozen_generator_inputs'] = values['FROZEN_FILES_IN'].split()
    manifest['required_test_assets'] = ['Lib/test for selected seed modules and transitive support/data; separate test-profile staging only',
        'Lib/unittest, doctest, inspect, traceback and required stdlib dependency closure',
        'Native _sysconfigdata is oracle evidence only; translated test-profile sysconfig values must describe actual 16-bit target']
    manifest_path.write_text(json.dumps(manifest,indent=2)+'\n')
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--work", required=True)
    parser.add_argument("--artifacts", required=True)
    parser.add_argument("--snapshot-only", action="store_true")
    parser.add_argument("--resume", action="store_true", help="Continue the existing private build; never replace it")
    options = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    repo = root.parent
    sys.path.insert(0, str(repo / "Scripts"))
    from campaigns.process import execute
    pin = json.loads((root / "config/source.json").read_text())
    artifacts = Path(options.artifacts).resolve()
    artifacts.mkdir(parents=True, exist_ok=True)
    class Context:
        def __init__(self):
            self.root = root
            self.sources = {"product": root / "ref" / pin["directory"]}
            old = artifacts / "preparation-commands.json"
            self.receipt = json.loads(old.read_text()) if (options.resume or options.snapshot_only) and old.is_file() else {"scope": "private native helper preparation only", "commands": []}
        def run(self, command, label, timeout=600, *, cwd):
            record = {}
            self.receipt["commands"].append(record)
            try:
                execute(command, label=label, cwd=cwd, output=artifacts / (label + "-" + str(len(self.receipt["commands"])) + ".log"), timeout=timeout, record=record)
            finally:
                (artifacts / "preparation-commands.json").write_text(json.dumps(self.receipt, indent=2) + "\n")
            return Path(record['stdout']).read_text()
    context = Context()
    base = Path(options.work).resolve()
    if options.snapshot_only:
        snapshot(context, base)
    elif options.resume:
        finish(context, base)
    else:
        prepare(context, base)


if __name__ == "__main__":
    main()
