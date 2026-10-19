"""Final actual-consumer matrix using the existing process helper and projects.

Preparation reads sources only. Executions require A32's directly received SAFE
handoff. Each build uses ordinary project references, records its actual inputs,
and emits a request for A32 rather than running a second data/address auditor.
"""
import argparse
import ast
import datetime
import fcntl
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sys
import tempfile
import traceback

ROOT = Path(__file__).resolve().parents[3]
CPY = ROOT / 'cpython'
HERE = Path(__file__).resolve().parent
ART = CPY / 'artifacts/owner-b-r41-20261018'
WORK = CPY / 'build/owner-b-r41-20261018'
A32 = CPY / 'artifacts/owner-a-r32-20261018'
B40 = CPY / 'artifacts/owner-b-r40-20261018'
LOCK = '/tmp/dotcc-cpython-shared-build.lock'
sys.path.insert(0, str(ROOT / 'Scripts'))
from campaigns.process import execute
from campaigns.recipes import helper


def read(path):
    return json.loads(Path(path).read_text())


def ref(path):
    path = Path(path).resolve()
    return dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), bytes=path.stat().st_size)


def verify(reference):
    assert ref(reference['path'])['sha256'] == reference['sha256'], reference['path']
    return reference


def write(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + '.pending')
    temporary.write_text(json.dumps(value, indent=2, default=str) + '\n')
    temporary.replace(path)
    return ref(path)


def utc():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()


def source_pins():
    released = read(B40 / 'final-handoff.json')
    for key in ('startupSourcePins', 'fiveExceptions', 'twoModeCoverage', 'integratedGate', 'controls'):
        verify(released[key])
    for source in read(released['startupSourcePins']['path'])['sources']:
        if Path(source['path']) == CPY / 'tests/Upstream/Program.cs':
            before = ART / 'upstream-raw-exit-before.cs'
            assert ref(before)['sha256'] == source['currentSha256']
            expected = before.read_text().replace(
                '    if (code.AsInt64() != 0) throw new InvalidOperationException("Translated upstream cases failed/blocked; inspect " + report);',
                '    // Preserve the unchanged runner\'s raw exit, including declared SKIP/XFAIL.\n'
                '    // The ordinary campaign derives any authorized acceptance separately.\n'
                '    if (code.AsInt64() != 0) return checked((int)code.AsInt64());') + 'return 0;\n'
            assert Path(source['path']).read_text() == expected, 'Only the authorized raw exit change is allowed'
        else:
            assert ref(source['path'])['sha256'] == source['currentSha256'], source['path']
    text = (CPY / 'docs/PLAN.md').read_text()
    d10 = text[text.index('## Generated code simplification and constant data\n'):text.index('## Milestones and evidence\n')]
    assert hashlib.sha256(d10.encode()).hexdigest() == '30ccf75f558d60ee5060ef6cb1a44edc397d13aa872f68607669a3ebfc44d279'
    paths = [*HERE.glob('*.py'), *HERE.glob('*.cs'), *HERE.glob('*.csproj'), *HERE.glob('*AotR41.cs.txt'), CPY / 'samples/ManagedConsumer/Program.cs',
             * (CPY / 'tests/Behavior').glob('*.cs'), *(CPY / 'tests/fixtures').glob('*.py'),
             CPY / 'tests/Abi/Program.cs', CPY / 'tests/ExecutionStackContractR34/Program.cs',
             CPY / 'tests/Upstream/Program.cs', CPY / 'tests/Upstream/campaign_hooks.py',
             CPY / 'tests/Upstream/run_inventory.py', CPY / 'tests/Upstream/inventory.frozen.json',
             CPY / 'config/pyconfig-policy.json', CPY / 'scripts/pyconfig.py',
             CPY / 'tests/StartupQualificationR37/Qualification/Program.cs',
             CPY / 'tests/ActualQualificationR40/ComplementaryProgram.cs.txt',
             CPY / 'tests/ActualQualificationR40/complementary_probe.py']
    return dict(b40Handoff=ref(B40 / 'final-handoff.json'), sources=[ref(p) for p in sorted(set(paths))],
                normativeD10Sha256='30ccf75f558d60ee5060ef6cb1a44edc397d13aa872f68607669a3ebfc44d279')


def authorize(path, profile):
    authority = read(path)
    assert authority['directA32SafeReceived'] is True
    verify(authority['release'])
    release = read(authority['release']['path'])
    assert 'SAFE' in release['status'].upper()
    assert release['activeOwned'] == release['queuedOwned'] == []
    for pid in release.get('terminalPidsAbsent', []):
        assert not Path('/proc', str(pid)).exists(), pid
    source_pins()
    publication_ref = authority['publications'][profile]
    verify(publication_ref)
    publication = read(publication_ref['path'])
    verify(publication['receipt'])
    receipt = read(publication['receipt']['path'])
    assert receipt['status'] == 'passed' and receipt['profile'] == profile
    assert len(receipt['units']) == (193 if profile == 'embedded-gil' else 286)
    published_forms = set(receipt['outputs'])
    assert published_forms == set(publication['products']) and published_forms <= {'raw','processed'}
    assert published_forms == {'raw','processed'} or (profile == 'upstream-tests-static'
        and published_forms == {'raw'} and receipt['selection']['form'] == 'raw')
    assert receipt['selection']['jobs'] == 8
    assert all(r['status'] == 'passed' and r['exit_code'] == 0 for r in receipt['commands'])
    for product in publication['products'].values():
        for key in ('pe', 'pdb', 'project'):
            verify(product[key])
    # Only execution, after the direct handoff, attempts a lease.
    for lock in (LOCK, '/tmp/dotcc-cpython-native-build.lock', str(CPY / 'build/campaign/run.lock')):
        with open(lock, 'a') as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            fcntl.flock(stream, fcntl.LOCK_UN)
    return publication


def bind(pe, project, products):
    pe, project = Path(pe), Path(project)
    inputs = helper(CPY / 'tests/Audit/actual_profile_input.py')
    mapper = helper(CPY / 'tests/Audit/pdb_sources.py')
    identities = helper(CPY / 'tests/Audit/pe_identity.py')
    maps, sources = inputs.project_sources(project, products)
    pdb = mapper.bind(pe, pe.with_suffix('.pdb'), maps, sources)
    assert pdb['documents'] and all(Path(d['path']).resolve() in sources for d in pdb['documents'])
    pdb.update(actualProject=ref(project), actualCompileInputs={str(p): ref(p)['sha256'] for p in sorted(sources)})
    return dict(identity=identities.pe_identity(pe), sourceBinding=pdb,
                pathMap=[list(m) for m in maps],products=str(Path(products).resolve()) if products else None)


class Matrix:
    def __init__(self, args):
        self.args = args
        self.publication = authorize(args.authority, args.profile)
        self.publication_ref = read(args.authority)['publications'][args.profile]
        self.product = self.publication['products'][args.form]
        self.project = Path(self.product['project']['path'])
        self.tag = '-'.join((args.profile, args.form, args.mode, args.phase, args.attempt))
        self.art = ART / 'cells' / self.tag
        self.work = WORK / 'cells' / self.tag
        assert args.resume or not self.art.exists(), 'Preserve previous attempt; choose a new attempt name'
        self.art.mkdir(parents=True,exist_ok=True)
        self.work.mkdir(parents=True,exist_ok=True)
        # All normal entries in one cell reuse their ordinary library/facade
        # build inputs. Modes stay separate so PDB Compile inventories are exact.
        self.products = WORK / 'products' / args.profile / args.form / args.mode / self.publication['receipt']['sha256'][:16]
        self.commands = read(self.art / 'commands.json') if args.resume else []
        self.proofs = {p.name.removesuffix('-actual-inputs.json'):read(p)
                       for p in self.art.glob('*-actual-inputs.json')} if args.resume else {}
        self.start_pins = source_pins()
        entry_path = self.art / ('resume-entry.json' if args.resume else 'entry.json')
        write(entry_path, dict(utc=utc(), authority=ref(args.authority),
              publication=self.publication_ref,
              sourcePins=self.start_pins, actualPEProofsRequired=True))

    def run(self, argv, label, *, timeout=180, check=True, env=None, cwd=None, native=False):
        prior = [r for r in self.commands if r.get('label') == label]
        if prior:
            assert self.args.resume and len(prior) == 1 and prior[0]['status'] == 'passed', 'Retain failed command; reduce explicitly'
            return prior[0]
        environment = {k:v for k,v in os.environ.items() if k in ('PATH','HOME','USER','LOGNAME','TZ',
            'TMPDIR','DOTNET_ROOT','DOTNET_ROOT_X64','NUGET_PACKAGES','SSL_CERT_FILE','SSL_CERT_DIR')}
        environment.update(DOTNET_PROCESSOR_COUNT='2', DOTNET_CLI_USE_MSBUILD_SERVER='0',
            MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_TELEMETRY_OPTOUT='1', UseSharedCompilation='false',
            LC_ALL='C.UTF-8', LANG='C.UTF-8', PYTHONUTF8='1', PYTHONDONTWRITEBYTECODE='1',
            PYTHONHOME='/absent-b41-python-home', PYTHONPATH='/absent-b41-python-path')
        environment.update(env or {})
        record = dict(declared_environment={k:v for k,v in environment.items()
                      if k.startswith(('DOTNET_', 'MSBUILD', 'PYTHON', 'LD_', 'DOTCC_')) or k in ('LC_ALL','LANG')})
        self.commands.append(record)
        command = [shutil.which('flock'), '-n', '--close', '/tmp/dotcc-cpython-native-build.lock' if native else LOCK,
                   *map(str, argv)]
        print(json.dumps(dict(start=label, utc=utc())), flush=True)
        try:
            execute(command, label=label, cwd=Path(cwd or '/tmp'), output=self.art / (label + '.log'),
                    timeout=timeout, env=environment, record=record, check=check)
        finally:
            write(self.art / 'commands.json', self.commands)
        print(json.dumps(dict(label=label, status=record['status'], exit=record.get('exit_code'),
                              seconds=record.get('seconds'))), flush=True)
        return record

    def build(self, project, name=None, *, reference=None, built_output=None):
        project = Path(project)
        name = name or project.stem
        output = Path(built_output) if built_output else self.work / name
        products = self.products if reference is None else self.work / (name + '-reference-products')
        argv = ['dotnet', 'publish' if self.args.mode == 'aot' else 'build', project, '-c','Release',
                '--disable-build-servers','-m:1','-p:UseSharedCompilation=false','-nodeReuse:false',
                '-p:NuGetAudit=false','--artifacts-path',products,'-o',output]
        if project.stem == 'Abi':
            configuration = 'release_linux-x64' if self.args.mode == 'aot' else 'release'
            argv += ['-p:EmitCompilerGeneratedFiles=true',
                     '-p:CompilerGeneratedFilesOutputPath=' + str(products / 'obj/Abi' / configuration)]
        if reference is None:
            argv.append('-p:CPythonProject=' + str(self.project))
        else:
            # ILC selects the facade and translated library from distinct
            # project output directories. Supply both exact selected inputs to
            # companions whose existing project takes one reference directory.
            selected_sha = ref(Path(reference) / 'TranslatedCPython.dll')['sha256']
            selected_input = next(p for p in self.proofs.values()
                                  if p['library']['identity']['sha256'] == selected_sha)
            reference = self.work / (name + '-selected-inputs')
            reference.mkdir(exist_ok=True)
            staged = []
            for role in ('library','facade'):
                selected_pe = Path(selected_input[role]['identity']['path'])
                verify(selected_input[role]['identity'])
                for source in (selected_pe,selected_pe.with_suffix('.pdb')):
                    destination = reference / source.name
                    if destination.exists():
                        assert ref(destination)['sha256'] == ref(source)['sha256'], destination
                    else:
                        shutil.copyfile(source,destination)
                    assert ref(destination)['sha256'] == ref(source)['sha256']
                    staged.append(dict(role=role,source=ref(source),selectedCopy=ref(destination)))
            write(self.art / (name + '-selected-reference-inputs.json'),dict(inputs=staged))
            argv.append('-p:ActualInputDirectory=' + str(reference))
        if self.args.mode == 'aot':
            argv += ['-r','linux-x64','-p:PublishAot=true','-p:StripSymbols=true']
        # The first ABI build succeeded without materializing generator source
        # files. Retain that PE/PDB and command; this scoped rebuild supplies
        # actual generator bytes for its otherwise mandatory PDB binding.
        build_label = name + '-build'
        if self.args.resume and reference is not None and any(
                r.get('label') == build_label and r['status'] != 'passed' for r in self.commands):
            build_label += '-selected-inputs'
        if self.args.resume and project.stem == 'Abi':
            if 'Abi' in self.proofs:
                build_label = next(r['label'] for r in reversed(self.commands) if r['label'].startswith('Abi-build'))
            else:
                build_label += '-materialized-source'
                if any(r.get('label') == build_label for r in self.commands):
                    build_label += '-forced-csc'
        if built_output is None:
            self.run(argv, build_label, timeout=1200)
        if self.args.mode == 'aot':
            responses = list(products.rglob(project.stem + '.ilc.rsp'))
            assert len(responses) == 1, responses
            response = responses[0]
            lines = response.read_text().splitlines()
            def resolve(s):
                path = (project.parent / s.strip('"')).resolve()
                assert path.is_file(), path
                return path
            references = [resolve(s[3:]) for s in lines if s.startswith('-r:')]
            entries = [resolve(s) for s in lines if s and not s.startswith('-')]
            objects = [resolve(s[3:]) for s in lines if s.startswith('-o:')]
            selected = [p for p in references if p.name == 'TranslatedCPython.dll']
            assert len(entries) == len(objects) == len(selected) == 1
            assert len(references) == len(set(references))
            library, entry = selected[0], entries[0]
            facade = next((p for p in references if p.name == 'Managed.CPython.dll'), None)
            ilc = dict(response=ref(response), entry=ref(entry), objects=[ref(p) for p in objects],
                       references=[dict(ref(p), **helper(CPY / 'tests/Audit/pe_identity.py').pe_identity(p)) for p in references],
                       executable=ref(output / project.stem), roots=[s for s in lines if s.startswith('--root:')])
            if project.stem == 'RootedBoundary':
                assert {'--root:TranslatedCPython','--root:Managed.CPython'} <= set(ilc['roots'])
            if project.stem == 'ManagedConsumer':
                assert not {'--root:TranslatedCPython','--root:Managed.CPython'} & set(ilc['roots'])
            command = [output / project.stem]
        else:
            library, entry = output / 'TranslatedCPython.dll', output / (project.stem + '.dll')
            facade = output / 'Managed.CPython.dll' if (output / 'Managed.CPython.dll').exists() else None
            references = sorted(output.glob('*.dll'))
            ilc = None
            command = [shutil.which('dotnet'), entry]
        # Reference-only companions retain the originating library Compile and
        # products context, then independently bind the actual selected PE/PDB.
        # Trimming may change its bytes; such bytes need a distinct A32 request.
        if reference is None:
            library_binding = bind(library, self.project, products)
        else:
            original_sha = ref(Path(reference) / 'TranslatedCPython.dll')['sha256']
            original = next(p['library'] for p in self.proofs.values()
                            if p['library']['identity']['sha256'] == original_sha)
            library_binding = bind(library,original['sourceBinding']['actualProject']['path'],original['products'])
        proof = dict(profile=self.args.profile, form=self.args.form, mode=self.args.mode,
            publication=self.publication_ref,
            entry=bind(entry, project, products), library=library_binding,
            references=[ref(p) for p in references], ILC=ilc, executable=ref(command[-1]))
        if facade:
            if reference is None:
                proof['facade'] = bind(facade, CPY / 'src/Managed.CPython/Managed.CPython.csproj', products)
            else:
                original_sha = ref(Path(reference) / 'Managed.CPython.dll')['sha256']
                original = next(p['facade'] for p in self.proofs.values() if 'facade' in p
                    and p['facade']['identity']['sha256'] == original_sha)
                proof['facade'] = bind(facade,original['sourceBinding']['actualProject']['path'],original['products'])
        proof_path = write(self.art / (name + '-actual-inputs.json'), proof)
        self.proofs[name] = proof
        if library_binding:
            identity = library_binding['identity']
            request = dict(profile=self.args.profile, form=self.args.form, receipt=self.publication['receipt'],
                publication=self.publication_ref,pe=identity, products=library_binding['products'],
                compileInputs=library_binding['sourceBinding']['actualCompileInputs'], pathMap=library_binding['pathMap'])
            request_path = ART / 'actual-pe-requests' / (self.args.profile + '-' + self.args.form + '-' + identity['sha256'][:16] + '.json')
            if request_path.exists():
                previous = read(request_path)
                assert previous['pe']['sha256'] == request['pe']['sha256']
                assert previous['receipt'] == request['receipt'], 'Same actual PE has another receipt; coordinate an immutable review binding'
                assert previous.get('publication',self.publication_ref) == self.publication_ref
                assert previous['compileInputs'] == request['compileInputs'] and previous['pathMap'] == request['pathMap']
            else:
                write(request_path, request)
        return dict(output=output, command=command, proof=proof, proofPath=proof_path, project=project,
                    products=products, library=library, entry=entry)

    def finish(self, status='executed-awaiting-actual-PE-proof', **extra):
        assert source_pins() == self.start_pins, 'Frozen input drift during matrix cell'
        for proof in self.proofs.values():
            for name in ('entry','library','facade'):
                if name in proof:
                    verify(proof[name]['identity'])
                    for path, digest in proof[name].get('sourceBinding', {}).get('actualCompileInputs', {}).items():
                        assert ref(path)['sha256'] == digest, path
            if proof['ILC']:
                for reference in proof['ILC']['references']:
                    verify(reference)
        write(self.art / 'result.json', dict(status=status, profile=self.args.profile, form=self.args.form,
              mode=self.args.mode, phase=self.args.phase, commands=ref(self.art / 'commands.json'),
              actualInputs=[ref(p) for p in sorted(self.art.glob('*-actual-inputs.json'))],
              commandSeconds=sum(r.get('seconds',0) for r in self.commands), utc=utc(), **extra))


def contracts(matrix):
    startup_reports = startup(matrix)
    sample = matrix.build(CPY / 'samples/ManagedConsumer/ManagedConsumer.csproj')
    workflow = matrix.work / 'workflow'
    matrix.run([*sample['command'], sample['output'] / 'cpython/stdlib', workflow], 'useful-consumer')
    for cycle in (0,1):
        assert (workflow / ('output-' + str(cycle) + '.json')).read_bytes() == b'{"sum":20,"values":[4,6,10]}'
    abi = matrix.build(CPY / 'tests/Abi/Abi.csproj')
    layout, operations = matrix.art / 'layout.json', matrix.art / 'operations.json'
    matrix.run([*abi['command'], '--layout', layout, matrix.publication['receipt']['path']], 'abi-layout')
    matrix.run([*abi['command'], '--operations', sample['output'] / 'cpython/stdlib', operations], 'abi-operations')
    oracle = read(ART / 'native' / 'result.json')
    compared = helper(CPY / 'tests/Abi/compare_layouts.py').compare(oracle['layout'], read(layout))
    assert read(operations)['cycles'] == 2 and read(operations)['assertions'] >= 36
    behavior = matrix.build(CPY / 'tests/Behavior/Behavior.csproj')
    cases = []
    for suite in ('behavior','threads','lifecycle'):
        work = matrix.work / suite
        matrix.run([*behavior['command'], behavior['output'] / 'cpython/stdlib', work, suite], suite, timeout=300)
        for cycle in (0,1):
            path = work / ('cycle-' + str(cycle)) / 'cases.json'
            report = read(path)
            assert report['suite'] == suite and any(r['status'] == 'passed' for r in report['cases'])
            assert all(r['status'] in ('passed','unselected') for r in report['cases'])
            cases.append(ref(path))
    owner = matrix.build(CPY / 'tests/ExecutionStackContractR34/ExecutionStackContractR34.csproj')
    owner_reports = []
    for scenario in ('owner','pool','legacy','bad-init'):
        path = matrix.art / ('owner-' + scenario + '.json')
        matrix.run([*owner['command'], scenario, sample['output'] / 'cpython/stdlib',
                    matrix.work / ('owner-' + scenario), path], 'owner-' + scenario, timeout=180)
        observed = read(path)
        assert observed['Status'] == 'passed' and observed['LibraryMvid'].lower() == owner['proof']['library']['identity']['mvid'].lower()
        owner_reports.append(ref(path))
    matrix.finish(startupReports=startup_reports, abiComparison=compared, fixtureReports=cases, ownerReports=owner_reports,
                  scope='Actual useful workflow, ABI, behavior/GC, thread/GIL, cancellation/lifecycle/owner contracts; per-PE D10 proof remains required')


def copied_stdlib(matrix, label):
    source = matrix.project.parent / 'stdlib'
    destination = matrix.work / label / 'stdlib'
    forbidden = [str(p) for p in source.rglob('*') if p.suffix in ('.so','.pyd','.dll','.dylib')]
    assert not forbidden, forbidden
    if not destination.exists():
        shutil.copytree(source, destination, ignore=shutil.ignore_patterns('__pycache__','*.pyc'))
    originals = {str(p.relative_to(source)):ref(p)['sha256'] for p in source.rglob('*')
                 if p.is_file() and '__pycache__' not in p.parts and p.suffix != '.pyc'}
    assert all(ref(destination / p)['sha256'] == sha for p,sha in originals.items())
    return destination, dict(source=str(source),destination=str(destination),files=originals,nativeDSOs=forbidden)


def startup(matrix):
    cell = matrix.build(CPY / 'tests/ActualQualificationR40/ActualQualificationR40.csproj')
    results = []
    for mode in ('host','startup'):
        library, manifest = copied_stdlib(matrix,'configured-' + mode)
        app = library.parent / 'app'; app.mkdir(exist_ok=True)
        for cycle in range(6):
            (app / ('startup_fixture_' + str(cycle) + '.py')).write_text('VALUE = 37\n')
        report = matrix.art / ('configured-' + mode + '.json')
        matrix.run([*cell['command'],mode,library,app,CPY / 'tests/Upstream/inventory.frozen.json',report],
                   'configured-' + mode)
        caches = list(app.rglob('startup_fixture_*.pyc'))
        assert len(caches) == 2 and all(any('startup_fixture_' + str(i) in p.name for p in caches) for i in (3,4))
        if mode == 'host':
            expected = CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-suite-loaders.log'
            assert (matrix.art / 'configured-host.log').read_text() == expected.read_text()
        results.append(dict(mode=mode,generations=6,invalidBooleanInputs=4 if mode=='host' else 0,
            callbackGenerations=6 if mode=='startup' else 0,fixtureCaches=list(map(str,caches)),
            input=cell['proofPath'],payload=manifest,transcript=ref(matrix.art / ('configured-' + mode + '.log'))))
    return results


def locale_preflight_inputs(library):
    source = Path(library) / 'test/test_codecs.py'
    native = CPY / 'build/owner-b-r37-20261018/native-full/source/Lib/test/test_codecs.py'
    assert ref(source)['sha256'] == ref(native)['sha256'], 'Frozen locale test source changed'
    text = source.read_text()
    classes = [node for node in ast.parse(text).body if isinstance(node,ast.ClassDef) and node.name == 'LocaleCodecTest']
    assert len(classes) == 1
    methods = {node.name:node for node in classes[0].body if isinstance(node,ast.FunctionDef)}
    required = set(read(CPY / 'tests/Upstream/inventory.frozen.json')['required_cases'])
    rows = []
    for name in ('decode_strict','decode_surrogateescape','encode_strict','encode_surrogateescape'):
        method = 'test_' + name
        case = 'test.test_codecs.LocaleCodecTest.' + method
        node = methods[method]
        assert case in required and not node.decorator_list
        detail = dict(source=''.join(text.splitlines(keepends=True)[node.lineno-1:node.end_lineno]),
            method=method,declaring_class='test.test_codecs.LocaleCodecTest',expected_failure=False)
        rows.append(dict(case=case,detail=detail,sourceSha256=ref(source)['sha256']))
    return dict(cases=rows,nativeSource=ref(native))


def feature_native_evidence():
    ready_path = CPY / 'artifacts/owner-b-r42-20261018/source-ready.json'
    assert ref(ready_path)['sha256'] == '280d97483287c2510fe37369ad44671de327a9903a9412a87f4587d76e847322'
    ready = read(ready_path)
    for item in ready['sourcePins']: verify(item)
    native_ref = ready['controls']['native']; verify(native_ref)
    native = read(native_ref['path'])
    assert native['featureCPPFLAGS'] == '-D_Py_FORCE_UTF8_LOCALE=1' and native['lowerLibcUnchanged']
    for item in [native['sourcePin'],native['commands'],native['native104'],*native['inputs'],*native['outputs'],
                 *native['configuredHostReports']]: verify(item)
    markers = read(native['native104']['path'])
    assert markers['counts'] == {'passed':99,'required-skip':2,'required-expected-failure':3}
    assert not markers['required_unexecuted']
    exceptions = read(CPY / 'tests/ActualQualificationR40/five-exceptions.json')['exceptions']
    nonpass = {row['case']:row for row in markers['cases'] if row['status'] != 'passed'}
    assert set(nonpass) == {row['case'] for row in exceptions}
    assert all(nonpass[row['case']]['status'] == row['status']
               and nonpass[row['case']].get('reason') == row.get('reason') for row in exceptions)
    modes = {read(item['path'])['before']['utf8_mode']:item for item in native['configuredHostReports']}
    assert set(modes) == {0,1}
    for mode,item in modes.items():
        report = read(item['path'])
        assert report['executed'] == len(report['cases']) == 6 and all(row['status'] == 'passed' for row in report['cases'])
        assert report['before']['utf8_mode'] == report['after']['utf8_mode'] == mode
    return dict(sourceReady=ref(ready_path),nativeResult=native_ref,modes=modes,
        nativeInputs=[next(row for row in native['outputs'] if Path(row['path']).name == 'native-configured-control'),
                      *native['inputs'],*native['outputs'],native['sourcePin']],
        nativeLibrary=str(Path(native_ref['path']).parent / 'consumer/lib/python3.13'))


def locale_preflight(matrix, cell, feature):
    """Execute the four unchanged locale regressions before the full suite."""
    project = (HERE / 'LocaleControlAotR41.csproj' if matrix.args.mode == 'aot'
               else CPY / 'tests/ActualQualificationR40/LocaleControlR40.csproj')
    entry = matrix.build(project,reference=cell['library'].parent)
    library, manifest = copied_stdlib(matrix,'locale-preflight')
    app = library.parent / 'app'; app.mkdir(exist_ok=True)
    for source,name in ((CPY / 'tests/Upstream/run_inventory.py','run_inventory.py'),
                        (HERE / 'locale_regression_probe.py','locale_probe.py')):
        shutil.copyfile(source,app / name)
    inventory = CPY / 'tests/Upstream/inventory.frozen.json'
    inputs = locale_preflight_inputs(library)
    ids = [row['case'] for row in inputs['cases']]
    write(app / 'locale-probe-inputs.json',inputs)
    report = matrix.art / 'locale-preflight-four.json'
    label = 'locale-preflight-four'
    if matrix.args.resume and any(r.get('label') == label and r['status'] != 'passed' for r in matrix.commands):
        label += '-aot-diagnostic'
    command = matrix.run([*entry['command'],'legacy','selected','probe',library,app,inventory,report],
                         label,check=False)
    observed = read(report)
    assert observed['executed'] == 4 and {row['case'] for row in observed['cases']} == set(ids)
    for stage in ('before','after'):
        flags = observed[stage]
        assert flags['utf8_mode'] == 0 and flags['isolated'] == flags['ignore_environment'] == 1
        assert flags['locale'] == 'C.UTF-8' and flags['filesystemencoding'] == 'utf-8'
    native = read(feature['modes'][0]['path'])
    witnesses = {row['case']:row for row in native['sourceWitnesses']}
    assert all(witnesses[row['case']]['sourceSha256'] == row['sourceSha256']
               and witnesses[row['case']]['methodAndDecoratorUnchanged'] for row in inputs['cases'])
    assert all(observed[stage][key] == native[stage][key] for stage in ('before','after')
               for key in observed[stage])
    assert command['exit_code'] == 0 and all(row['status'] == 'passed' for row in observed['cases']), observed['cases']
    return dict(actualInput=entry['proofPath'],report=ref(report),payload=manifest,
                matchedNative=feature['modes'][0],nativeFeature=feature['nativeResult'],
                scope='Exact four unchanged mode0 locale methods PASS on the selected actual library; no full-suite claim')


def upstream(matrix):
    assert matrix.args.profile == 'upstream-tests-static'
    feature = feature_native_evidence()
    write(matrix.art / 'matched-native-feature.json',feature)
    startup_reports = startup(matrix)
    # Use the ordinary campaign entry with its approved raw-exit return. Old
    # Qualification executions remain bound to their original entry and bytes.
    cell = matrix.build(CPY / 'tests/Upstream/Upstream.csproj')
    preflight = locale_preflight(matrix,cell,feature)
    imports = matrix.build(HERE / 'ProfileImportsR41.csproj',reference=cell['library'].parent)
    import_library, _ = copied_stdlib(matrix,'builtin-imports')
    native_imports = CPY / 'artifacts/owner-b-r37-20261018/native-imports.json'
    names = [r['name'] for r in read(native_imports)['selectedModuleImports']]
    assert len(names) == len(set(names)) == 75
    names_path = matrix.art / 'selected-builtin-names.json'
    write(names_path,names)
    matrix.run([*imports['command'],import_library,names_path],'all75-builtin-imports')
    library, copied = copied_stdlib(matrix, 'primary')
    report = matrix.art / 'full-required.json'
    command = matrix.run([*cell['command'],library,CPY / 'tests/Upstream/inventory.frozen.json',report,'qualify'],
                         'full-required',timeout=900,check=False)
    assert command['exit_code'] in (0,1), 'Fatal/aborted primary execution'
    journal = report.with_suffix('.journal.jsonl')
    accountant = helper(CPY / 'tests/ActualValidationR36/reporting.py')
    selection = read(CPY / 'tests/Upstream/inventory.frozen.json')
    counts = accountant.count_journal(journal,selection['required_cases'])
    write(matrix.art / 'full-journal-counts.json',counts)
    observed = read(report)
    metadata_case = 'test.test_importlib.metadata.test_main.FileSystem.test_unicode_dir_on_sys_path'
    metadata = [r for r in observed['cases'] if r['case'] == metadata_case]
    assert metadata and all(r['status'] == 'passed' for r in metadata), metadata
    assert counts['uniqueStarted'] == counts['uniqueCompleted'] == counts['required'] == 4867
    assert counts['withoutCompletedOutcome'] == 0
    assert not counts['unexpectedIds'] and not counts['outcomesWithoutStart']

    # Only these two unchanged cases get complementary supported-mode coverage.
    # Reuse the tested same-feature B42 native executable/archive, while both
    # native and actual execute a fresh exact-two report for this actual cell.
    ids = sorted(helper(CPY / 'tests/ActualQualificationR40/acceptance.py').TWO_MODE_IDS)
    details = {r['case']:r for g in observed['discovery'] for r in g['case_details']}
    inputs = dict(cases=[dict(case=case,detail=details[case],sourceSha256=ref(details[case]['upstream_path'])['sha256']) for case in ids])
    complementary_project = (HERE / 'ComplementaryAotR41.csproj' if matrix.args.mode == 'aot'
                             else CPY / 'tests/ActualQualificationR40/ComplementaryR40.csproj')
    complementary = matrix.build(complementary_project,reference=cell['library'].parent)
    companion_library, companion_manifest = copied_stdlib(matrix,'complementary')
    app = matrix.work / 'complementary/app'; app.mkdir()
    shutil.copyfile(CPY / 'tests/Upstream/run_inventory.py',app / 'run_inventory.py')
    shutil.copyfile(CPY / 'tests/Upstream/inventory.frozen.json',app / 'inventory.frozen.json')
    shutil.copyfile(CPY / 'tests/ActualQualificationR40/complementary_probe.py',app / 'locale_probe.py')
    write(app / 'locale-probe-inputs.json',inputs)
    complementary_report = matrix.art / 'complementary-two.json'
    matrix.run([*complementary['command'],'legacy','selected','probe',companion_library,app,
                app / 'inventory.frozen.json',complementary_report],'complementary-two')
    native_library = matrix.work / 'complementary-native/stdlib'
    shutil.copytree(feature['nativeLibrary'],native_library,ignore=shutil.ignore_patterns('__pycache__','*.pyc'))
    native_app = native_library.parent / 'app'; native_app.mkdir()
    for name in ('run_inventory.py','locale_probe.py','locale-probe-inputs.json'):
        shutil.copyfile(app / name,native_app / name)
    native_report = matrix.art / 'complementary-native-feature-two.json'
    matrix.run([feature['nativeInputs'][0]['path'],native_library,native_app,native_report,'1','C.UTF-8'],
               'complementary-native-feature-two',native=True)
    complementary_proof = dict(entry=complementary['proof']['executable'],
        sourceBinding=complementary['proof']['entry']['sourceBinding'],
        references=complementary['proof']['references'],ILC=complementary['proof']['ILC'])
    entry_ref = write(matrix.art / 'complementary-execution-input.json',complementary_proof)
    combined_commands = write(matrix.art / 'coverage-commands.json',matrix.commands)
    coverage_result = write(matrix.art / 'complementary-result.json',dict(
        status='Fresh same-actual-PE TWO PASS with unchanged matched native mode1 witness',
        native=ref(native_report),actual=ref(complementary_report),actualInput=entry_ref,
        nativeInputs=feature['nativeInputs'],commands=combined_commands,
        nativeFeature=feature['nativeResult'],nativePayload=str(native_library),actualPayload=companion_manifest))
    coverage_manifest = matrix.art / 'two-mode-coverage.json'
    write(coverage_manifest,dict(scope='exact two supported startup configurations',caseIds=ids,result=coverage_result))
    execution_ref = write(matrix.art / 'primary-execution-input.json',dict(
        entry=cell['proof']['executable'],references=cell['proof']['references'],ILC=cell['proof']['ILC']))
    gate = helper(CPY / 'tests/ActualQualificationR40/acceptance.py')
    accepted = gate.evaluate_integrated(report,journal,cell['proofPath']['path'],execution_ref['path'],
        matrix.art / 'commands.json',CPY / 'tests/ActualQualificationR40/five-exceptions.json',coverage_manifest)
    accepted_ref = write(matrix.art / 'integrated-acceptance.json',accepted)
    for manifest in (copied,companion_manifest):
        assert all(ref(Path(manifest['source']) / p)['sha256'] == sha
                   and ref(Path(manifest['destination']) / p)['sha256'] == sha for p,sha in manifest['files'].items())
    assert accepted['acceptance'] == 'accepted-with-five-exceptions-and-two-configuration-witnesses', accepted['blockingOutcomes']
    matrix.finish(startupReports=startup_reports,localePreflight=preflight,upstreamAcceptance=accepted_ref,counts=counts,rawRunnerExit=command['exit_code'],
        primaryPayload=copied,metadataCaseActuallyPassed=metadata,
        scope='Full unchanged 4867 primary mode0 plus exact samePE mode1 pair; raw SKIP/XFAIL/exit preserved. Actual D10/helper-profile/audit gates remain separate.')


def relocate(matrix, cell, label):
    saved = matrix.art / (label + '-relocation.json')
    if matrix.args.resume and saved.exists():
        base = Path(read(saved)['base'])
    else:
        executed = next((row for row in matrix.commands if row.get('label') ==
            ('relocated-useful' if label == 'useful' else 'normal') and row['status'] == 'passed'),None)
        base = Path(executed['cwd']).parent if matrix.args.resume and executed else Path(
            tempfile.mkdtemp(prefix='dotcc-cpython-b41-' + label + '-')).resolve()
    assert not base.is_relative_to(ROOT)
    destination = base / 'relocated payload'
    if not destination.exists(): shutil.copytree(cell['output'],destination)
    audit = helper(CPY / 'tests/Audit/audit_product.py')
    audit.copied_payload(cell['output'],destination)
    cwd, empty, poison = base / 'unrelated cwd',base / 'empty PATH',base / 'ambient poison'
    for path in (cwd,empty,poison): path.mkdir(exist_ok=True)
    (poison / 'json.py').write_text("raise AssertionError('ambient json imported')\n")
    (poison / 'campaign_app.py').write_text("raise AssertionError('ambient app imported')\n")
    environment = dict(PATH=str(empty),PYTHONHOME=str(poison),PYTHONPATH=str(poison),
        PYTHONUSERBASE=str(poison),PYTHONSTARTUP=str(poison / 'json.py'),PYTHONUTF8='0',
        PYTHONDONTWRITEBYTECODE='0',LD_LIBRARY_PATH=str(empty),LD_PRELOAD='')
    executable = destination / cell['project'].stem
    command = [executable] if matrix.args.mode == 'aot' else [shutil.which('dotnet'),executable.with_suffix('.dll')]
    if not saved.exists(): write(saved,dict(base=str(base),payload=str(destination),cwd=str(cwd)))
    return base,destination,cwd,environment,command


def delivery(matrix):
    assert matrix.args.profile == 'embedded-gil'
    audit = helper(CPY / 'tests/Audit/audit_product.py')
    sample = matrix.build(CPY / 'samples/ManagedConsumer/ManagedConsumer.csproj')
    base,payload,cwd,env,command = relocate(matrix,sample,'useful')
    matrix.run([*command,payload / 'cpython/stdlib',base / 'workflow'],'relocated-useful',env=env,cwd=cwd)
    for cycle in (0,1):
        assert (base / 'workflow' / ('output-' + str(cycle) + '.json')).read_bytes() == b'{"sum":20,"values":[4,6,10]}'
    policy = helper(ROOT / 'Scripts/campaigns/identity.py').HashPolicy('strict')
    asset_report = audit.assets(payload / 'cpython',CPY,matrix.args.form,packaged=True,policy=policy)
    notice_report = helper(CPY / 'tests/Delivery/campaign_hooks.py').notices(
        type('Context',(),dict(repo=ROOT,root=CPY))(),payload,matrix.args.mode)
    contract = matrix.build(CPY / 'tests/Delivery/Delivery.csproj')
    contract_base,checked,check_cwd,check_env,_ = relocate(matrix,contract,'contract')
    reports = []
    for scenario in ('normal','missing-stdlib','missing-json','missing-app'):
        variant = contract_base / scenario
        shutil.copytree(checked,variant)
        if scenario == 'missing-stdlib': shutil.rmtree(variant / 'cpython/stdlib/encodings')
        elif scenario == 'missing-json': shutil.rmtree(variant / 'cpython/stdlib/json')
        elif scenario == 'missing-app': (variant / 'app/delivery_contract.py').unlink()
        path = matrix.art / (scenario + '.json')
        executable = variant / 'Delivery'
        command = [executable] if matrix.args.mode == 'aot' else [shutil.which('dotnet'),executable.with_suffix('.dll')]
        matrix.run([*command,variant / 'cpython/stdlib',path,scenario],scenario,env=check_env,cwd=check_cwd)
        observed = read(path)
        assert observed['status'] == ('passed' if scenario == 'normal' else 'expected-error')
        if scenario == 'normal':
            assert observed['builtin_modules'] == sorted(read(CPY / 'config/stdlib.json')['builtins'])
        reports.append(ref(path))
    elf = None
    if matrix.args.mode == 'aot':
        matrix.run(['readelf','-dW',payload / 'ManagedConsumer'],'aot-elf')
        elf = audit.elf_dependencies((matrix.art / 'aot-elf.log').read_text())
    matrix.finish(relocation=str(base),contractRelocation=str(contract_base),scenarios=reports,
        assets=asset_report,notices=notice_report,payload=audit.payload(payload),elf=elf,
        scope='Ordinary references and normally trimmed consumer, relocated outside repo with empty PATH and poisoned ambient Python; required assets and errors actually executed.')


def boundary(matrix):
    assert matrix.args.profile == 'embedded-gil' and matrix.args.mode == 'aot'
    cell = matrix.build(CPY / 'tests/Boundary/RootedBoundary.csproj')
    cases = matrix.work / 'rooted-cases'
    matrix.run([*cell['command'],cell['output'] / 'cpython/stdlib',cases,'all'],'rooted-full-behavior',timeout=300)
    reports = []
    for cycle in (0,1):
        path = cases / ('cycle-' + str(cycle)) / 'cases.json'
        report = read(path)
        assert report['suite'] == 'all' and all(r['status'] == 'passed' for r in report['cases'])
        reports.append(ref(path))
    matrix.finish(fixtureReports=reports,rooting=cell['proof']['ILC']['roots'],
        scope='Both actual TranslatedCPython and facade assemblies rooted; all behavior/thread/GC/lifecycle cases and cancellation/drain executed.')


def solution(matrix):
    assert (matrix.args.profile,matrix.args.form,matrix.args.mode) == ('embedded-gil','processed','jit')
    matrix.run(['dotnet','build',CPY / 'ManagedConsumer.slnx','-c','Release','--disable-build-servers',
        '-m:1','-p:UseSharedCompilation=false','-nodeReuse:false','-p:NuGetAudit=false',
        '--artifacts-path',matrix.products,'-p:CPythonProject=' + str(matrix.project)],'root-solution-build',timeout=300)
    cell = matrix.build(CPY / 'samples/ManagedConsumer/ManagedConsumer.csproj',
        built_output=matrix.products / 'bin/ManagedConsumer/release')
    workflow = matrix.work / 'root-solution-workflow'
    matrix.run([*cell['command'],cell['output'] / 'cpython/stdlib',workflow],'root-solution-useful')
    for cycle in (0,1):
        assert (workflow / ('output-' + str(cycle) + '.json')).read_bytes() == b'{"sum":20,"values":[4,6,10]}'
    matrix.finish(scope='Root solution ordinary project references, actual useful workflow and per-PE request; canonical publication unchanged.')


def native(args):
    authorize(args.authority, 'embedded-gil')
    art, work = ART / 'native', WORK / 'native'
    art.mkdir(parents=True,exist_ok=True); work.mkdir(parents=True,exist_ok=True)
    # Reuse the exact existing same-pin native archive, never rebuild Python.
    base = CPY / 'build/native'
    build, source = base / 'build', base / 'source'
    common = ['cc','-std=c11','-Wall','-Wextra','-Werror','-I',build,'-I',source / 'Include']
    commands = read(art / 'commands.json') if (art / 'commands.json').exists() else []
    def run(argv, label, check=True):
        prior = [r for r in commands if r.get('label') == label]
        if prior:
            assert len(prior) == 1 and prior[0]['status'] == 'passed', 'Retain failed native command; reduce separately'
            return (art / (label + '.log')).read_text()
        row = {}; commands.append(row)
        try:
            text = execute(['flock','-n','--close','/tmp/dotcc-cpython-native-build.lock',*map(str,argv)],
                label=label,cwd=Path('/tmp'),output=art / (label + '.log'),timeout=180,check=check,record=row)
        finally:
            write(art / 'commands.json',commands)
        return (art / (label + '.log')).read_text()
    inputs = [ref(p) for p in (build / 'pyconfig.h', source / 'Include/Python.h',build / 'libpython3.13.a',
        build / 'Modules/_hacl/libHacl_Hash_SHA2.a',CPY / 'tests/Abi/cpython_layout.c',CPY / 'tests/Abi/cpython_operations.c')]
    run([*common,CPY / 'tests/Abi/cpython_layout.c','-o',work / 'layout'],'layout-build')
    layout = json.loads(run([work / 'layout'],'layout-run'))
    run([*common,'-DCP_B_ABI_MAIN',CPY / 'tests/Abi/cpython_operations.c',build / 'libpython3.13.a',
         build / 'Modules/_hacl/libHacl_Hash_SHA2.a','-lm','-ldl','-lpthread','-o',work / 'operations'],'operations-build')
    operations = json.loads(run([work / 'operations',source / 'Lib'],'operations-run'))
    assert operations == dict(cycles=2, refcounts=True, immortals=True, slots=True, varargs=True, vectorcall=True)
    control = CPY / 'tests/NativeOracle/control.c'
    inputs.append(ref(control))
    run(['cc','-std=gnu11','-I',build,'-I',source / 'Include',control,build / 'libpython3.13.a',
         build / 'Modules/_hacl/libHacl_Hash_SHA2.a','-lm','-ldl','-lpthread','-o',work / 'workflow'],'workflow-build')
    run([work / 'workflow',source / 'Lib',work / 'useful-work'],'useful-workflow')
    for kind, expected in (('assertion',23),('harness',24)):
        run([work / 'workflow',source / 'Lib',work / ('negative-' + kind),kind],'failure-' + kind,check=False)
        assert commands[-1]['exit_code'] == expected
    for item in inputs: verify(item)
    write(art / 'result.json',dict(status='passed',layout=layout,operations=operations,inputs=inputs,
          usefulWorkflowCycles=2, failureExitCodes=dict(assertion=23,harness=24),
          commands=ref(art / 'commands.json'),scope='Same-pin native ABI/useful workflow/GC/threads/failure propagation controls; no native interpreter rebuild'))


def prepare():
    pins = source_pins()
    write(ART / 'source-preparation.json',dict(status='SOURCE_ONLY',utc=utc(),threadId=os.environ.get('CODEX_THREAD_ID'),
        heavyCommands=0, locksAcquired=0, sourcePins=pins,
        matrix=[dict(profile=profile,form=form,mode=mode,status='unrun',
                    gates=(['consumer','ABI','behavior','GC','threads','GIL','owner','lifecycle','delivery','audit']
                           if profile=='embedded-gil' else ['upstream4867','complementary2','helper-profile','audit']))
                for profile in ('embedded-gil','upstream-tests-static') for form in ('raw','processed') for mode in ('jit','aot')],
        separateRequired=['native useful workflow and failure propagation','whole TranslatedCPython/facade rooted AOT both forms',
                          'ordinary trimmed delivery both forms/modes','all actualPE D10 via A32','current input/ILC audits',
                          'P6 director/A coordination and reproduction'],
        actualPERequestSchema=dict(profile='profile',form='form',receipt='exact final-publication receipt reference',
            pe='actual path/SHA/MVID/entryPointToken',products='actual --artifacts-path or null',
            compileInputs='complete actual library Compile path:SHA map',pathMap='exact [physical,logical] pairs'),
        acceptance='Exactly five identity-bound exceptions plus two source/native/samePE supported-mode witnesses; retain raw runner exits and every outcome.'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--prepare', action='store_true')
    parser.add_argument('--authority', type=Path)
    parser.add_argument('--profile', choices=('embedded-gil','upstream-tests-static'),default='embedded-gil')
    parser.add_argument('--form', choices=('raw','processed'),default='raw')
    parser.add_argument('--mode', choices=('jit','aot'),default='jit')
    parser.add_argument('--phase', choices=('native','contracts','upstream','delivery','boundary','solution'),default='contracts')
    parser.add_argument('--attempt', default='initial')
    parser.add_argument('--resume',action='store_true',help='Reuse successful commands after an explicitly retained harness repair')
    args = parser.parse_args()
    if args.prepare:
        prepare(); return
    assert args.authority and args.authority.is_file(), 'Direct A32 SAFE authority required'
    if args.phase == 'native':
        native(args); return
    matrix = Matrix(args)
    try:
        globals()[args.phase](matrix)
    except BaseException as error:
        blocker = matrix.art / 'blocker.json'
        if blocker.exists():
            blocker = matrix.art / ('blocker-' + str(len(list(matrix.art.glob('blocker*.json')))) + '.json')
        write(blocker,dict(status='blocked-or-failed',error=str(error),
            traceback=traceback.format_exc(),commands=ref(matrix.art / 'commands.json') if matrix.commands else None,
            profile=args.profile,form=args.form,mode=args.mode,phase=args.phase,utc=utc()))
        raise


if __name__ == '__main__':
    main()
