"""A-owned recipe integration: existing products, framework processes/reports.

No translation, acquisition, publisher, native Python execution or alternate
engine. Public hooks: run_delivery, run_audit, run_boundary, run_behavior.
"""
import json
import os
from pathlib import Path
import shutil
import tempfile

LOCK = '/tmp/dotcc-cpython-shared-build.lock'


def audit_module(ctx):
    from campaigns.recipes import helper
    return helper(ctx.root/'tests/Audit/audit_product.py')


def managed(ctx, action, project, label, *arguments):
    restore = (['--no-restore'] if ctx.options.restore == 'none' else
               ['-p:RestoreLockedMode=true'] if ctx.options.restore == 'locked' else [])
    # --close prevents compiler/build-server descendants retaining the lock.
    return ctx.run(['flock', '-n', '--close', LOCK, 'dotnet', action, project, '-c', 'Release',
                    '--nologo', *restore, *arguments], label, timeout=1800)


def product(ctx, form):
    from campaigns.translation import check_product
    if ctx.profile != 'embedded-gil' or ctx.options.rid != 'linux-x64':
        raise ValueError('Delivery/boundary qualification requires embedded-gil/linux-x64')
    chosen = check_product(ctx, form)
    audit = audit_module(ctx)
    identity = audit.product_identity(ctx.root, form, chosen, os.environ.get('DOTCC_CPYTHON_EXPECTED_RECEIPT'))
    pins = ctx.__dict__.setdefault('_cpython_selected_products', {})
    if form in pins:
        audit.require(pins[form] == identity, 'Selected product bytes changed during specialist suite: '+form)
    else:
        pins[form] = identity
    return chosen


def build_cell(ctx, form, mode, project, name):
    cache = ctx.__dict__.setdefault('_cpython_delivery_builds', {})
    key = (form, mode, name)
    if key in cache:
        return cache[key]
    chosen = product(ctx, form)
    label = name+'-'+form+'-'+mode
    output = ctx.work/label/'app'
    arguments = [f'-p:CPythonProject={chosen}', '--artifacts-path', ctx.work/label/'products',
                 '--disable-build-servers', '-m:1', '-o', output]
    if mode == 'aot':
        arguments += ['-r', ctx.options.rid, '-p:PublishAot=true', '-p:StripSymbols=true']
    managed(ctx, 'publish' if mode == 'aot' else 'build', project, label+'-build', *arguments)
    product(ctx, form)
    audit = audit_module(ctx)
    executable = output/(Path(project).stem if mode == 'aot' else Path(project).stem+'.dll')
    audit.require(executable.is_file(), 'Actual specialist executable missing after build: '+str(executable))
    binaries = {str(path): audit.digest(path) for path in sorted({executable, *output.glob('*.dll')})}
    result = {'form': form, 'mode': mode, 'profile': ctx.profile, 'product': str(chosen),
              'label': label, 'output': str(output), 'build_products': str(ctx.work/label/'products'),
              'translation_identity': ctx.__dict__['_cpython_selected_products'][form], 'compiled_artifacts': binaries}
    cache[key] = result
    return result


def command(output, name, mode):
    # Resolve the lock utility before relocation poisons PATH. The child still
    # runs with an empty executable search path and cannot find ambient Python.
    flock = shutil.which('flock')
    if not flock:
        raise RuntimeError('flock required by the Linux-x64 shared-build contract')
    prefix = [str(Path(flock).resolve()), '-n', '--close', LOCK]
    if mode == 'jit':
        dotnet = shutil.which('dotnet')
        if not dotnet:
            raise RuntimeError('dotnet host required for JIT; Python is never a runtime prerequisite')
        return [*prefix, str(Path(dotnet).resolve()), Path(output)/(name+'.dll')]
    return [*prefix, Path(output)/name]


def record(ctx, label, row):
    report = ctx.artifacts/(label+'.json')
    report.write_text(json.dumps(row, indent=2)+'\n')
    ctx.receipt.setdefault('cpython_specialist_reports', {})[label] = str(report)
    ctx.save()


def relocation(ctx, cell):
    base = Path(tempfile.mkdtemp(prefix='dotcc-cpython-delivery-')).resolve()
    if base.is_relative_to(ctx.repo.resolve()):
        raise RuntimeError('Delivery relocation must be outside the repository: '+str(base))
    destination = base/'relocated payload'
    shutil.copytree(cell['output'], destination)
    audit_module(ctx).copied_payload(Path(cell['output']), destination)
    cwd = base/'unrelated working directory'
    cwd.mkdir()
    empty = base/'empty executable path'
    empty.mkdir()
    poison = base/'ambient python must be ignored'
    poison.mkdir()
    (poison/'json.py').write_text("raise AssertionError('ambient json was imported')\n")
    (poison/'campaign_app.py').write_text("raise AssertionError('ambient application was imported')\n")
    environment = {'PATH': str(empty), 'PYTHONHOME': str(poison), 'PYTHONPATH': str(poison),
        'PYTHONUSERBASE': str(poison), 'PYTHONSTARTUP': str(poison/'json.py'),
        'PYTHONUTF8': '0', 'PYTHONDONTWRITEBYTECODE': '0',
        'LD_LIBRARY_PATH': str(empty), 'LD_PRELOAD': ''}
    return base, destination, cwd, environment


def notices(ctx, payload, mode):
    audit = audit_module(ctx)
    path = payload/'notices/DotCC-LICENSE.txt'
    audit.require(path.is_file() and audit.digest(path) == audit.digest(ctx.repo/'LICENSE'),
                  'Shared runtime/facade/application MIT notice missing or changed')
    result = {'DotCC': str(path), 'sha256': audit.digest(path)}
    if mode == 'aot':
        for name in ('DotNet-Runtime-LICENSE.txt', 'DotNet-Runtime-THIRD-PARTY-NOTICES.txt'):
            runtime = payload/'notices'/name
            audit.require(runtime.is_file() and runtime.stat().st_size > 0, 'AOT runtime notice missing: '+str(runtime))
            result[name] = {'path': str(runtime), 'sha256': audit.digest(runtime)}
    return result


def run_delivery(ctx, suite):
    from campaigns.testing import forms, modes
    audit = audit_module(ctx)
    cells = ctx.receipt.setdefault('delivery_cells', [])
    failed = []
    for form in forms(ctx):
        for mode in modes(ctx):
            row = {'form': form, 'mode': mode, 'profile': ctx.profile, 'status': 'unrun'}
            cells.append(row)
            try:
                # Build the ordinary useful sample with normal trimming/rooting.
                useful = build_cell(ctx, form, mode, ctx.root/'samples/ManagedConsumer/ManagedConsumer.csproj', 'delivery-consumer')
                base, payload, cwd, environment = relocation(ctx, useful)
                row.update(product=useful['product'], translation_identity=useful['translation_identity'],
                           compiled_artifacts=useful['compiled_artifacts'],
                           relocation=str(base), notices=notices(ctx, payload, mode))
                row['assets'] = audit.assets(payload/'cpython', ctx.root, form, packaged=True, policy=ctx.policy)
                row['payload'] = audit.payload(payload)
                ctx.run([*command(payload, 'ManagedConsumer', mode), payload/'cpython/stdlib', base/'workflow'],
                        useful['label']+'-relocated-run', cwd=cwd, env=environment, timeout=suite.timeout, separate=True)
                for cycle in (0, 1):
                    audit.require((base/'workflow'/f'output-{cycle}.json').read_bytes() == b'{"sum":20,"values":[4,6,10]}',
                                  'Relocated useful workflow bytes differ')
                checks = build_cell(ctx, form, mode, ctx.root/'tests/Delivery/Delivery.csproj', 'delivery-contract')
                check_base, checked, check_cwd, check_env = relocation(ctx, checks)
                row['contract_relocation'] = str(check_base)
                row['scenarios'] = []
                for scenario in ('normal', 'missing-stdlib', 'missing-json', 'missing-app'):
                    # All negative mutations affect this new private copy only.
                    variant = check_base/scenario
                    shutil.copytree(checked, variant)
                    if scenario == 'missing-stdlib':
                        shutil.rmtree(variant/'cpython/stdlib/encodings')
                    elif scenario == 'missing-json':
                        shutil.rmtree(variant/'cpython/stdlib/json')
                    elif scenario == 'missing-app':
                        (variant/'app/delivery_contract.py').unlink()
                    report = ctx.artifacts/(checks['label']+'-'+scenario+'.json')
                    ctx.run([*command(variant, 'Delivery', mode), variant/'cpython/stdlib', report, scenario],
                            checks['label']+'-'+scenario, cwd=check_cwd, env=check_env, timeout=suite.timeout, separate=True)
                    observed = json.loads(report.read_text())
                    audit.require(observed['status'] == ('passed' if scenario == 'normal' else 'expected-error'),
                                  'Missing actual delivery scenario assertion: '+scenario)
                    if scenario == 'normal':
                        expected = json.loads((ctx.root/'config/stdlib.json').read_text())['builtins']
                        audit.require(observed['builtin_modules'] == sorted(expected),
                                      'Actual relocated builtin registrations differ from reviewed product closure')
                    row['scenarios'].append({'name': scenario, 'report': str(report)})
                product(ctx, form)
                row['status'] = 'passed'
            except Exception as error:
                row.update(status='blocked-or-failed', error=str(error))
                failed.append(form+'-'+mode)
            finally:
                record(ctx, 'delivery-'+form+'-'+mode, row)
    if failed:
        raise RuntimeError('Required delivery cells blocked/failed: '+', '.join(failed))


def metadata_tools(ctx):
    if '_cpython_audit_tools' not in ctx.__dict__:
        output = ctx.work/'metadata-audit-tools/app'
        managed(ctx, 'build', ctx.root/'tests/Audit/MetadataAudit.csproj', 'metadata-audit-tools-build',
            '--artifacts-path', ctx.work/'metadata-audit-tools/products', '--disable-build-servers', '-m:1', '-o', output)
        ctx.__dict__['_cpython_audit_tools'] = output
    return ctx.__dict__['_cpython_audit_tools']


def inspect(ctx, tools, assembly, label):
    report = ctx.artifacts/(label+'.metadata.json')
    ctx.run([*command(tools, 'MetadataAudit', 'jit'), assembly, report], label+'-metadata', timeout=300)
    return json.loads(report.read_text())


def generated_data(ctx, form, chosen, document, *, label=None):
    from campaigns.recipes import helper
    audit = audit_module(ctx)
    evidence = helper(ctx.root/'scripts/generated_code_evidence.py')
    identity = ctx.__dict__['_cpython_selected_products'][form]
    label = label or 'audit-'+form
    inventory = ctx.artifacts/(label+'-generated-code-inventory.json')
    evidence.prepare_typed(Path(chosen).parent, Path(identity['receipt']), inventory,
        holder='Managed.Interpreters.CPython.CPythonLiterals', owner='Managed.Interpreters.CPython')
    if '_cpython_generated_data_tools' not in ctx.__dict__:
        output = ctx.work/'generated-data-tools/app'
        managed(ctx, 'build', ctx.root/'NativeOracle/GeneratedDataAudit/GeneratedDataAudit.csproj',
                'generated-data-tools-build', '--artifacts-path', ctx.work/'generated-data-tools/products',
                '--disable-build-servers', '-m:1', '-o', output)
        ctx.__dict__['_cpython_generated_data_tools'] = output
    report = ctx.artifacts/(label+'-generated-code-il-rva.json')
    ctx.run([*command(ctx.__dict__['_cpython_generated_data_tools'], 'GeneratedDataAudit', 'jit'),
             document['path'], inventory, report], label+'-generated-code-il-rva', timeout=300)
    product(ctx, form)
    return audit.d10_compiled(identity, inventory, report, document)


def actual_data_evidence(ctx, compiled, evidence):
    from campaigns.recipes import helper
    return helper(ctx.root/'tests/Audit/consumer_evidence.py').data_review(compiled, evidence)


def run_audit(ctx, suite):
    from campaigns.testing import forms, modes
    audit = audit_module(ctx)
    # Do not spend build time if the actual product is absent.
    products = {form: product(ctx, form) for form in forms(ctx)}
    tools = metadata_tools(ctx)
    basis = inspect(ctx, tools, tools/'DotCC.Libc.dll', 'shared-runtime-basis')
    failed = []
    for form, chosen in products.items():
        row = {'form': form, 'profile': ctx.profile, 'status': 'unrun'}
        try:
            row['generated'] = audit.generated(chosen, ctx.root, form, policy=ctx.policy)
            row['closure'] = audit.closure(ctx.root, form, chosen)
            # Metadata comes from the real form's compiled JIT assembly even
            # when auditing its native AOT delivery (whose IL was compiled away).
            jit = build_cell(ctx, form, 'jit', ctx.root/'samples/ManagedConsumer/ManagedConsumer.csproj', 'delivery-consumer')
            output = Path(jit['output'])
            row['dependencies'] = audit.dependencies(output)
            row['assemblies'] = []
            for assembly in sorted(output.glob('*.dll')):
                document = inspect(ctx, tools, assembly, 'audit-'+form+'-'+assembly.stem)
                row['assemblies'].append(audit.metadata(document, basis, product=assembly.name == 'TranslatedCPython.dll'))
                if assembly.name == 'TranslatedCPython.dll':
                    product_document = document
                    row['exports'] = audit.exports(document, ctx.root)
                    config = next(item['path'] for item in row['closure']['generated_inputs']
                                  if Path(item['path']).name == 'config.c')
                    row['static_modules'] = audit.builtins(config, ctx.root, document)
                    row['d10'] = {'compiled': generated_data(ctx, form, chosen, document)}
            audit.require('exports' in row, 'Actual compiled product assembly missing')
            row['deliveries'] = []
            for mode in modes(ctx):
                cell = build_cell(ctx, form, mode, ctx.root/'samples/ManagedConsumer/ManagedConsumer.csproj', 'delivery-consumer')
                target = Path(cell['output'])
                delivery = {'mode': mode, 'assets': audit.assets(target/'cpython', ctx.root, form, packaged=True, policy=ctx.policy),
                            'notices': notices(ctx, target, mode), 'payload': audit.payload(target)}
                if mode == 'aot':
                    text = ctx.run(['readelf', '-dW', target/'ManagedConsumer'], 'audit-'+form+'-aot-elf', timeout=60)
                    delivery['elf_needed'] = audit.elf_dependencies(text)
                row['deliveries'].append(delivery)
            # The compiler owner supplies source-bound typed JIT/AOT probes and
            # semantic controls. Missing evidence is a required gate failure;
            # neither syntax nor a reduced substitute program qualifies it.
            index = os.environ.get('DOTCC_CPYTHON_GENERATED_EVIDENCE')
            bundle = json.loads(Path(index).read_text(encoding='utf-8')) if index else {}
            actual_bundle = actual_data_evidence(ctx, row['d10']['compiled'], bundle.get(form))
            row['d10']['allocation_and_semantics'] = audit.d10_allocations(row['d10']['compiled'], actual_bundle)
            row['d10']['address_reuse'] = {'status': 'unqualified', 'review': bundle.get(form, {}).get('address_review')}
            from campaigns.recipes import helper
            actual_evidence = helper(ctx.root/'tests/Audit/consumer_evidence.py')
            row['d10']['address_reuse'] = actual_evidence.address_review(ctx, row['d10']['compiled'], bundle.get(form),
                product_document, ctx.artifacts/('audit-'+form+'-TranslatedCPython.metadata.json'))
            # Qualify each actual compilation input. The JIT copy and the ILC
            # input can have different PE/MVIDs despite identical C# sources.
            for delivery in row['deliveries']:
                mode = delivery['mode']
                cell = build_cell(ctx, form, mode, ctx.root/'samples/ManagedConsumer/ManagedConsumer.csproj', 'delivery-consumer')
                binding = actual_evidence.compilation_input(cell, 'ManagedConsumer')
                if mode == 'jit':
                    proof = {'compiled':row['d10']['compiled'],
                             'allocation_and_semantics':row['d10']['allocation_and_semantics'],
                             'address_reuse':row['d10']['address_reuse']}
                    audit.require(binding['sha256'] == proof['compiled']['assembly_sha256'],
                                  'JIT delivery differs from audited PE')
                else:
                    label = 'audit-'+form+'-aot'
                    document = inspect(ctx, tools, binding['pe'], label+'-TranslatedCPython')
                    delivery['metadata'] = audit.metadata(document, basis, product=True)
                    delivery['exports'] = audit.exports(document, ctx.root)
                    delivery['static_modules'] = audit.builtins(config, ctx.root, document)
                    compiled = generated_data(ctx, form, chosen, document, label=label)
                    proof = {'compiled':compiled,
                             'allocation_and_semantics':audit.d10_allocations(compiled,
                                 actual_data_evidence(ctx, compiled, bundle.get(form))),
                             'address_reuse':actual_evidence.address_review(ctx, compiled, bundle.get(form), document,
                                 ctx.artifacts/(label+'-TranslatedCPython.metadata.json'), label=label+'-actual-consumer')}
                    audit.require(binding['sha256'] == compiled['assembly_sha256'],
                                  'AOT delivery differs from audited ILC input')
                audit.require(binding == actual_evidence.compilation_input(cell, 'ManagedConsumer'),
                              'Actual consumer compilation input changed during audit')
                delivery['compilation_input'] = binding
                delivery['d10'] = proof
                qualified = {'form':form, 'mode':mode, 'profile':ctx.profile, 'status':'passed',
                             'compilation_input':binding, 'd10':proof}
                ctx.receipt.setdefault('audit_cells', []).append(qualified)
                record(ctx, 'audit-'+form+'-'+mode, qualified)
            product(ctx, form)
            row['status'] = 'passed'
        except Exception as error:
            row.update(status='blocked-or-failed', error=str(error))
            failed.append(form)
        finally:
            record(ctx, 'audit-'+form, row)
    if failed:
        raise RuntimeError('Required actual-output audits blocked/failed: '+', '.join(failed))


def run_boundary(ctx, suite):
    from campaigns.testing import forms
    failed = []
    for form in forms(ctx):
        row = {'form': form, 'mode': 'aot', 'rooting': 'whole TranslatedCPython and Managed.CPython assemblies', 'status': 'unrun'}
        try:
            cell = build_cell(ctx, form, 'aot', ctx.root/'tests/Boundary/RootedBoundary.csproj', 'rooted-boundary')
            row.update(cell)
            output = Path(cell['output'])
            ctx.run([*command(output, 'RootedBoundary', 'aot'), output/'cpython/stdlib',
                     ctx.work/cell['label']/'cases', 'all'], cell['label']+'-run', timeout=suite.timeout, separate=True)
            product(ctx, form)
            row['status'] = 'passed'
        except Exception as error:
            row.update(status='blocked-or-failed', error=str(error))
            failed.append(form)
        finally:
            record(ctx, 'rooted-boundary-'+form, row)
    if failed:
        raise RuntimeError('Required rooted AOT boundary forms blocked/failed: '+', '.join(failed))


def run_behavior(ctx, suite):
    from campaigns.testing import forms, modes
    failed = []
    for form in forms(ctx):
        for mode in modes(ctx):
            label = 'behavior-'+form+'-'+mode+'-'+suite.name
            row = {'form': form, 'mode': mode, 'profile': ctx.profile, 'suite': suite.name, 'status': 'unrun'}
            try:
                cell = build_cell(ctx, form, mode, ctx.root/'tests/Behavior/Behavior.csproj', 'behavior')
                row.update(cell)
                output = Path(cell['output'])
                case_work = ctx.work/cell['label']/suite.name
                ctx.run([*command(output, 'Behavior', mode), output/'cpython/stdlib', case_work, suite.name],
                        label+'-run', timeout=suite.timeout, separate=True)
                row['fixture_reports'] = []
                for cycle in (0, 1):
                    report = case_work/f'cycle-{cycle}'/'cases.json'
                    observed = json.loads(report.read_text())
                    if (observed['suite'] != suite.name or not any(case['status'] == 'passed' for case in observed['cases'])
                            or any(case['status'] not in ('passed', 'unselected') for case in observed['cases'])):
                        raise RuntimeError('Incomplete/failed actual fixture report: '+str(report))
                    row['fixture_reports'].append({'path': str(report), 'counts': {
                        status: sum(case['status'] == status for case in observed['cases'])
                        for status in ('passed', 'failed', 'unselected')}})
                product(ctx, form)
                row['status'] = 'passed'
            except Exception as error:
                row.update(status='blocked-or-failed', error=str(error))
                failed.append(form+'-'+mode)
            finally:
                record(ctx, label, row)
    if failed:
        raise RuntimeError('Required '+suite.name+' cells blocked/failed: '+', '.join(failed))
