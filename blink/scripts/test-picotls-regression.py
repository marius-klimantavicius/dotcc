#!/usr/bin/env python3
"""Revalidate picotls using existing frozen tools; never build the compiler.

Preparation copies only checksum-verified archives, verifies/extracts using the
campaign recipe, builds its native oracle, and regenerates raw/optimized output.
Validation runs copied-source/ABI and normal/authentication peer subsets only.
"""
import sys
import argparse
import ast
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / "picotls"
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()

def tools_identity():
    files = []
    for project in ("DotCC", "DotCC.PostProcess"):
        directory = ROOT / project / "bin/Release/net10.0"
        files.extend(directory.glob("*.dll"))
        files.extend(directory.glob("*.deps.json"))
        files.extend(directory.glob("*.runtimeconfig.json"))
    for name in ("DotCC/bin/Release/net10.0/dotcc.dll", "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"):
        if not (ROOT / name).is_file():
            raise RuntimeError("Missing frozen tool: " + name)
    return {str(p.relative_to(ROOT)): sha(p) for p in sorted(set(files))}

def authored_identity():
    paths = subprocess.check_output(["git", "ls-files", "picotls"], cwd=ROOT, text=True).splitlines()
    return {name: sha(ROOT / name) for name in paths}

def save():
    (run / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

def execute(name, argv, seconds):
    command = dict(name=name, argv=[str(x) for x in argv], cwd=str(ROOT), timeout_seconds=seconds,
                   started_unix=time.time(), log=str(run / (name + ".log")))
    receipt["commands"].append(command)
    save()
    print("RUN " + name, flush=True)
    with Path(command["log"]).open("w") as log:
        process = subprocess.Popen(command["argv"], cwd=ROOT, stdout=log, stderr=subprocess.STDOUT,
                                   start_new_session=True, env=dict(os.environ, LC_ALL="C", TMPDIR=str(run / "tmp"),
                                       DOTCC_COMPILER=str(ROOT / "DotCC/bin/Release/net10.0/dotcc.dll")))
        try:
            command["exit_code"] = process.wait(timeout=seconds)
        except BaseException as error:
            try: os.killpg(process.pid, signal.SIGTERM)
            except ProcessLookupError: pass
            try: process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                try: os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError: pass
                process.wait()
            command["exit_code"] = process.returncode
            command["interrupted"] = type(error).__name__
            command["timed_out"] = isinstance(error, subprocess.TimeoutExpired)
            raise
        finally:
            command["finished_unix"] = time.time()
            log.flush()
            command["log_sha256"] = sha(Path(command["log"]))
            save()
    if tools_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise RuntimeError("Frozen tool or tracked picotls source drift during " + name)
    if command["exit_code"]:
        raise RuntimeError(name + " failed; preserved log: " + command["log"])
    return Path(command["log"]).read_text()

def derive_peer_runner():
    original = CAMPAIGN / 'scripts/test-managed-peer.py'
    source = original.read_text()
    tree = ast.parse(source)
    call_text = "pair(\"reject-server-selected-unoffered-alpn\", False, \"native\", expected_success=False, force_unoffered_alpn=True)"
    expected = ast.parse(call_text).body[0]
    def selected(node):
        return (isinstance(node, ast.Call) and isinstance(node.func, ast.Name) and node.func.id == 'pair'
                and node.args and isinstance(node.args[0], ast.Constant)
                and node.args[0].value == 'reject-server-selected-unoffered-alpn')
    all_matches = [node for node in ast.walk(tree) if selected(node)]
    top_matches = [node for node in tree.body if isinstance(node, ast.Expr) and selected(node.value)]
    if len(all_matches) != 1 or len(top_matches) != 1:
        raise RuntimeError('Expected exactly one excluded top-level peer call')
    target = top_matches[0]
    if ast.dump(target, include_attributes=False) != ast.dump(expected, include_attributes=False):
        raise RuntimeError('Excluded peer call shape changed')
    lines = source.splitlines(keepends=True)
    removed = ''.join(lines[target.lineno-1:target.end_lineno])
    derived = ''.join(lines[:target.lineno-1] + lines[target.end_lineno:])
    tree.body.remove(target)
    if ast.dump(ast.parse(derived), include_attributes=False) != ast.dump(tree, include_attributes=False):
        raise RuntimeError('Peer adaptation changed more than the reviewed call')
    folder = CAMPAIGN / 'generated'; folder.mkdir(exist_ok=True)
    destination = folder / ('normal-peer-' + run.name + '.py')
    destination.write_text(derived)
    shutil.copyfile(original, run / 'original-peer.py')
    receipt['peer_adaptation'] = dict(original=str(original), original_sha256=sha(original),
        derived=str(destination), derived_sha256=sha(destination), removed_call=removed,
        reason='Authored forced selection of an unoffered ALPN; ordinary authentication/configuration rejection remains selected.')
    return destination


def expected_peer_names():
    names = []
    for oracle in ('native', 'independent'):
        for server in (False, True):
            role = oracle + ('-managed-server' if server else '-managed-client')
            for cipher in ('TLS_AES_128_GCM_SHA256', 'TLS_AES_256_GCM_SHA384'):
                for identity in ('server-rsa', 'server-ecdsa'):
                    names.append(f'{role}-{cipher}-{identity}')
            names += [role + '-' + suffix for suffix in ('empty', 'large-key-update', 'mutual-rsa', 'mutual-ecdsa',
                'wrong-name', 'expired', 'untrusted', 'missing-client', 'untrusted-client')]
    names += ['managed-both-TLS_AES_128_GCM_SHA256', 'managed-both-TLS_AES_256_GCM_SHA384',
              'reject-key-encipherment-only-leaf']
    if len(names) != 55 or len(set(names)) != 55: raise RuntimeError('Expected peer inventory differs')
    return names


def source_hashes(directory):
    return {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*')
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}


def execute_binary(label, argv, binary, expected=None):
    paths = sorted(set([binary, *binary.parent.glob('*.dll'), *binary.parent.glob('*.json')])) if binary.suffix == '.dll' else [binary]
    before = {str(p): sha(p) for p in paths}
    value = execute(label, argv, 180)
    after = {str(p): sha(p) for p in paths}
    if before != after: raise RuntimeError('Execution binary drift: ' + label)
    receipt.setdefault('executions', {})[label] = dict(before=before, after=after, output_sha256=hashlib.sha256(value.encode()).hexdigest())
    if expected is not None and value != expected: raise RuntimeError('Deterministic output mismatch: ' + label)
    return value


def normal_matrix():
    pins = json.loads((CAMPAIGN / 'config/inputs.json').read_text())
    source = CAMPAIGN / 'ref' / pins['picotls']['directory']
    definitions = ['-D' + line.strip() for line in (CAMPAIGN / 'config/core-defines.txt').read_text().splitlines()
                   if line.strip() and not line.lstrip().startswith('#')]
    layout = run / 'native-layout'
    execute('native-layout-build', ['cc', '-std=c11', *definitions, '-I', source / 'include',
                                   CAMPAIGN / 'tests/native-layout.c', '-o', layout], 300)
    native = execute_binary('native-layout', [layout], layout)
    values = native.splitlines()
    if len(values) != 92 or len({line.split('=', 1)[0] for line in values}) != 92:
        raise RuntimeError('Expected 92 distinct native ABI values')
    native_values = run / 'native-layout.values'; native_values.write_text(native)
    receipt['native_layout_values_sha256'] = sha(native_values)
    peer = derive_peer_runner()
    expected_peers = expected_peer_names()
    receipt['selection'] = dict(suites=['CopiedConsumer', 'TranslatedAbi'], abi_observations=92,
        peer_names=expected_peers, modes=['raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot'],
        excluded_suites={
            'ProviderVectors': 'Mixed target includes authored allocation/handshake allocation fault injection.',
            'TlsTests': 'Mixed target includes authored altered/crafted records and authenticated handshake corruption.',
            'UpstreamVectors': 'Mixed target adds truncated QUIC prefixes absent from pinned test_quicint; no selector.'},
        excluded_peer='reject-server-selected-unoffered-alpn',
        scope='Partial normal/upstream-native regression only; not the complete picotls campaign or dependency audit.')
    receipt['native_upstream'] = dict(revision=pins['picotls']['revision'],
        tests=['t/openssl.c', 't/minicrypto.c', 't/picotls.c'], oracle_script_sha256=sha(CAMPAIGN / 'scripts/oracle.sh'))
    receipt['suite_matrix'] = []; receipt['peer_matrix'] = []
    expected_outputs = {}; expected_public_peers = None
    for variant, directory in [('raw', 'TranslatedPicotls.Raw'), ('optimized', 'TranslatedPicotls')]:
        product = CAMPAIGN / 'generated' / directory / 'TranslatedPicotls.csproj'
        frozen = source_hashes(product.parent)
        receipt.setdefault('products', {})[variant] = dict(project=str(product), sources=frozen)
        properties = ['-p:PicotlsProject=' + str(product)]
        for suite in ('CopiedConsumer', 'TranslatedAbi'):
            project = CAMPAIGN / 'tests' / suite / (suite + '.csproj')
            prefix = variant + '-' + suite
            execute(prefix + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', *properties], 600)
            extra = [native_values] if suite == 'TranslatedAbi' else []
            for mode in ('jit', 'aot'):
                if mode == 'aot':
                    publish = run / 'publish' / variant / suite
                    execute(prefix + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                        '-p:PublishAot=true', *properties, '-o', publish, '--nologo'], 900)
                    binary = publish / suite; argv = [binary, *extra]
                else:
                    binary = project.parent / 'bin/Release/net10.0' / (suite + '.dll'); argv = ['dotnet', binary, *extra]
                value = execute_binary(prefix + '-' + mode, argv, binary, expected_outputs.get(suite))
                required = ('PASS actual translated product ABI: 92 size/alignment/member-address checks\n'
                            if suite == 'TranslatedAbi' else 'PASS copied PicoTls sources: consumer aliases/types, nested runtime and callbacks; implicit usings disabled\n')
                if value != required: raise RuntimeError('Unexpected suite result: ' + prefix)
                expected_outputs[suite] = value
                receipt['suite_matrix'].append(dict(suite=suite, mode=variant + '-' + mode, passed=True)); save()
        for mode in ('jit', 'aot'):
            peer_receipt = run / (variant + '-peer-' + mode + '.json')
            argv = [sys.executable, peer, '--no-prepare', '--runtime', 'linux-x64', '--receipt', peer_receipt]
            if variant == 'raw': argv.append('--raw')
            if mode == 'aot': argv.append('--aot')
            execute(variant + '-peer-' + mode, argv, 2400)
            result = json.loads(peer_receipt.read_text())
            if (result['raw'] != (variant == 'raw') or result['aot'] != (mode == 'aot')
                    or result['runtime'] != 'linux-x64' or [row['case'] for row in result['cases']] != expected_peers):
                raise RuntimeError('Peer mode/case inventory mismatch')
            if expected_public_peers is not None and result['cases'] != expected_public_peers:
                raise RuntimeError('Peer public results differ between modes')
            expected_public_peers = result['cases']
            receipt['peer_matrix'].append(dict(mode=variant + '-' + mode, receipt=str(peer_receipt),
                sha256=sha(peer_receipt), cases=result['cases'])); save()
        if source_hashes(product.parent) != frozen: raise RuntimeError('Generated product changed: ' + variant)
    expected_suites = [(suite, variant + '-' + mode) for variant in ('raw', 'optimized')
                       for suite in ('CopiedConsumer', 'TranslatedAbi') for mode in ('jit', 'aot')]
    if [(row['suite'], row['mode']) for row in receipt['suite_matrix']] != expected_suites:
        raise RuntimeError('Suite coverage differs')
    if [row['mode'] for row in receipt['peer_matrix']] != receipt['selection']['modes']:
        raise RuntimeError('Peer mode coverage differs')
    if sha(peer) != receipt['peer_adaptation']['derived_sha256'] or sha(Path(receipt['peer_adaptation']['original'])) != receipt['peer_adaptation']['original_sha256']:
        raise RuntimeError('Peer runner changed')
    if sha(native_values) != receipt['native_layout_values_sha256']: raise RuntimeError('Native ABI reference changed')
    receipt['peer_comparisons'] = sum(len(row['cases']) for row in receipt['peer_matrix'])
    receipt['scope'] = receipt['selection']['scope']

def interrupted(signum, frame):
    raise InterruptedError('Received signal ' + str(signum))
signal.signal(signal.SIGTERM, interrupted)

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--prepare-only", action="store_true")
parser.add_argument("--resume", type=Path)
parser.add_argument("--cache", type=Path, default=CAMPAIGN / "ref")
args = parser.parse_args()
artifacts = ROOT / "blink/artifacts/picotls-regression"
artifacts.mkdir(parents=True, exist_ok=True)
if args.resume:
    run = args.resume.resolve()
    run.relative_to(artifacts.resolve())
    receipt = json.loads((run / "receipt.json").read_text())
    if receipt.get("kind") != "picotls-normal-subset-frozen-tools" or receipt.get("runner_sha256") != sha(Path(__file__)):
        raise SystemExit("Resume requires this exact normal-subset runner")
    if (any(command['name'] == 'native-layout-build' for command in receipt.get('commands', []))
            or any(key in receipt for key in ('suite_matrix', 'peer_matrix', 'peer_adaptation'))):
        raise SystemExit('Matrix already started; preserve its logs and retry with a new attempt')
    if not receipt.get("prepared") or receipt.get("passed"):
        raise SystemExit("Resume requires a prepared, uncompleted attempt")
    if tools_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise SystemExit("Cannot resume: frozen input identity changed")
else:
    run = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = dict(kind="picotls-normal-subset-frozen-tools", passed=False, prepared=False,
                   root=str(ROOT), started_unix=time.time(), tools=tools_identity(),
                   authored=authored_identity(), runner_sha256=sha(Path(__file__)),
                   git_head=subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
                   commands=[], archives=[])
(run / "tmp").mkdir(exist_ok=True)
save()
print("RECEIPT " + str(run / "receipt.json"), flush=True)
try:
    if not receipt["prepared"]:
        pins = json.loads((CAMPAIGN / "config/inputs.json").read_text())
        (CAMPAIGN / "ref").mkdir(parents=True, exist_ok=True)
        for item in pins.values():
            source = args.cache / (item["directory"] + ".tar.gz")
            if sha(source) != item["sha256"]:
                raise RuntimeError("Pinned cache archive mismatch: " + str(source))
            destination = CAMPAIGN / "ref" / source.name
            if destination.exists() and sha(destination) != item["sha256"]:
                raise RuntimeError("Existing archive mismatch: " + str(destination))
            if not destination.exists():
                shutil.copyfile(source, destination)
            if sha(destination) != item["sha256"]:
                raise RuntimeError("Copied archive mismatch")
            receipt["archives"].append(dict(source=str(source), destination=str(destination), sha256=sha(destination)))
        save()
        execute("fetch-verify", [CAMPAIGN / "scripts/fetch.sh"], 120)
        execute("environment", ["dotnet", "--info"], 30)
        execute("native-oracle", [CAMPAIGN / "scripts/oracle.sh"], 1200)
        execute("regenerate", [CAMPAIGN / "scripts/translate.sh", "--no-build-tools", "--no-fetch"], 1800)
        translation = CAMPAIGN / "artifacts/translation/success.json"
        shutil.copyfile(translation, run / "translation.json")
        receipt["translation_sha256"] = sha(translation)
        receipt["prepared"] = True
        save()
    if not args.prepare_only:
        if sha(CAMPAIGN / "artifacts/translation/success.json") != receipt["translation_sha256"]:
            raise RuntimeError("Translation receipt changed before matrix")
        normal_matrix()
        receipt["passed"] = True
        receipt["completed_unix"] = time.time()
except BaseException as error:
    receipt["passed"] = False
    receipt["error"] = str(error)
    raise
finally:
    if sha(Path(__file__)) != receipt["runner_sha256"]:
        receipt["passed"] = False
        receipt["error"] = "Runner changed during qualification"
    receipt["tools_after"] = tools_identity()
    receipt["authored_after"] = authored_identity()
    receipt["final_identity_stable"] = (sha(Path(__file__)) == receipt["runner_sha256"]
        and receipt["tools_after"] == receipt["tools"] and receipt["authored_after"] == receipt["authored"])
    if not receipt["final_identity_stable"]:
        receipt["passed"] = False
        receipt["error"] = "Frozen tool or authored input identity changed"
    # This broad disk inventory is archival only; the qualified subset is named
    # by suite_matrix/peer_matrix and command logs, never an old campaign PASS.
    receipt["archival_disk_inventory_not_qualification"] = {str(p.relative_to(ROOT)): sha(p)
                            for directory in (CAMPAIGN / "artifacts", CAMPAIGN / "generated", CAMPAIGN / "build")
                            for p in sorted(directory.rglob("*")) if p.is_file()
                            and "tmp" not in p.relative_to(directory).parts
                            and "obj" not in p.relative_to(directory).parts}
    save()
if not receipt.get("final_identity_stable") or not receipt.get("prepared") or (not args.prepare_only and not receipt.get("passed")):
    raise SystemExit(1)
print("PREPARED" if args.prepare_only else "PASS scoped normal/upstream-native subset", flush=True)
