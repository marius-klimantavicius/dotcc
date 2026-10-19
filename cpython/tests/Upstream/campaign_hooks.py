"""Integration hook for A's recipe; consumes real separate translated artifacts.

No fetcher, compiler driver, publisher, alternate Python backend or subprocess
regrtest. The campaign owns profile preparation/publication and process receipts.
"""
import copy
import hashlib
import json
import os
from pathlib import Path

PROFILE = 'upstream-tests-static'
ACCEPTED = 'accepted-with-five-exceptions-and-two-configuration-witnesses'


def checked_reference(reference):
    path = Path(reference['path'])
    if not path.is_absolute() or not path.is_file():
        raise RuntimeError('Explicit upstream acceptance artifact missing: ' + str(path))
    if hashlib.sha256(path.read_bytes()).hexdigest() != reference['sha256']:
        raise RuntimeError('Upstream acceptance artifact bytes changed: ' + str(path))
    return path


def acceptance_gate(ctx):
    from campaigns.recipes import helper
    handoff = json.loads((ctx.root/'artifacts/owner-b-r40-20261018/final-handoff.json').read_text())
    path = checked_reference(handoff['integratedGate'])
    if path != (ctx.root/'tests/ActualQualificationR40/acceptance.py').resolve():
        raise RuntimeError('Wrong released upstream acceptance gate')
    checked_reference(handoff['fiveExceptions'])
    return helper(path)


def reviewed_acceptance(ctx, cell, artifacts, compilation_review, label):
    """Use the released gate separately from unchanged raw runner outcomes.

    The caller first obtains A32's strict review for these exact actual inputs.
    This helper neither changes that review nor substitutes canonical evidence.
    """
    if not compilation_review or compilation_review.get('status') != 'passed':
        raise RuntimeError('Strict completed actual-PE compilation review required')
    names = ('rawReport','rawJournal','actualInputProof','executionProof','commands','configurationCoverage')
    paths = {name:checked_reference(artifacts[name]) for name in names}
    actual = json.loads(paths['actualInputProof'].read_text())['library']['identity']
    compiled = compilation_review['compiled']
    if (compiled['assembly_sha256'] != actual['sha256']
            or compiled['mvid'] != actual['mvid']):
        raise RuntimeError('Completed compilation review selects different actual library')
    gate = acceptance_gate(ctx)
    policy = ctx.root/'tests/ActualQualificationR40/five-exceptions.json'
    result = gate.evaluate_integrated(paths['rawReport'],paths['rawJournal'],paths['actualInputProof'],
        paths['executionProof'],paths['commands'],policy,paths['configurationCoverage'])
    if result['actualLibrary']['sha256'] != actual['sha256'] or result['actualLibrary']['mvid'] != actual['mvid']:
        raise RuntimeError('Upstream acceptance selects different actual library')
    report = ctx.artifacts/(label+'-acceptance.json')
    report.write_text(json.dumps(result,indent=2)+'\n')
    cell.update(acceptance=str(report),acceptance_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
        acceptance_verdict=result['acceptance'],raw_runner_status=result['rawRunnerStatus'],
        raw_qualifies_translated_interpreter=result['rawQualifiesTranslatedInterpreter'])
    if result['acceptance'] != ACCEPTED:
        raise RuntimeError('Required upstream acceptance blocked: ' + json.dumps(result['blockingOutcomes']))
    return result


def completed_cells(ctx, products, selected_modes, index_path, profile_audit):
    """Review finished ordinary Upstream cells without rebuilding their inputs.

    The explicit index binds real executions, including raw nonzero exits. A32
    independently rebinds every actual role and consumes the finished same-PE
    proofs before the released acceptance gate can accept a cell.
    """
    index_path = Path(index_path)
    if not index_path.is_absolute():
        raise RuntimeError('Completed upstream cell index must be absolute')
    index_bytes = index_path.read_bytes()
    index = json.loads(index_bytes)
    if index.get('profile') != PROFILE:
        raise RuntimeError('Completed upstream index selects another profile')
    receipt = checked_reference(index['receipt'])
    cells = ctx.receipt.setdefault('upstream_cells', [])
    failed = []
    for form, product in products.items():
        selected = profile_audit.identity(ctx.root,form,product,receipt)
        for mode in selected_modes:
            key = form+'-'+mode
            label = 'upstream-'+key
            cell = dict(profile=PROFILE,form=form,mode=mode,product=str(product),status='unrun',
                evidence_origin='completed-ordinary-Upstream-cell',translation_identity=selected,
                completed_index=dict(path=str(index_path),sha256=hashlib.sha256(index_bytes).hexdigest()))
            cells.append(cell)
            try:
                supplied = index['cells'][key]
                artifacts = supplied['artifacts']
                proof = json.loads(checked_reference(artifacts['actualInputProof']).read_text())
                if (proof['profile'],proof['form'],proof['mode']) != (PROFILE,form,mode):
                    raise RuntimeError('Completed upstream actual profile/form/mode differs')
                project = Path(proof['entry']['sourceBinding']['actualProject']['path']).resolve()
                if project != (ctx.root/'tests/Upstream/Upstream.csproj').resolve():
                    raise RuntimeError('Completed upstream execution requires the ordinary Upstream entry')
                report = checked_reference(artifacts['rawReport'])
                retain_report(cell,report)
                # Retain raw command results before either strict review. A
                # failed raw command is never rewritten as a passing command.
                commands = json.loads(checked_reference(artifacts['commands']).read_text())
                executable = proof['executable']['path']
                matching = [row for row in commands if executable in row['command']
                    and str(report) in row['command']]
                if len(matching) != 1:
                    raise RuntimeError('Completed upstream execution command is ambiguous/missing')
                cell.update(raw_runner_exit=matching[0]['exit_code'],
                    raw_runner_command_status=matching[0]['status'])
                review = profile_audit.completed_compilation_review(ctx.root,selected,
                    artifacts['actualInputProof'],supplied['a32Evidence'])
                cell['compiled_review'] = review
                reviewed_acceptance(ctx,cell,artifacts,review,label)
                if profile_audit.identity(ctx.root,form,product,receipt) != selected:
                    raise RuntimeError('Completed upstream publication changed during review')
                if index_path.read_bytes() != index_bytes:
                    raise RuntimeError('Completed upstream index changed during review')
                cell['status'] = 'accepted'
            except Exception as error:
                cell.update(status='blocked-or-failed',error=str(error))
                failed.append(label)
            finally:
                ctx.save()
    if failed:
        raise RuntimeError('Required translated upstream cells failed/blocked: '+', '.join(failed))


def retain_report(cell, report):
    """Retain real per-case evidence even when the interpreter exits nonzero.

    Required skips/xfails retain their raw statuses, occurrence counts and first
    outcome in the campaign receipt, independently of reviewed acceptance.
    """
    if not report.is_file():
        return None
    observed = json.loads(report.read_text(encoding='utf-8'))
    cell.update(report=str(report), report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                counts=observed.get('counts'), required_unexecuted=observed.get('required_unexecuted'))
    cell['first_required_nonpass'] = next((row for row in observed.get('cases', []) if row['status'] != 'passed'), None)
    return observed


def run_suite(ctx, suite):
    from campaigns.testing import forms, modes
    from campaigns.translation import check_product
    if ctx.options.rid != 'linux-x64':
        raise ValueError('CPython upstream test profile requires linux-x64')
    profile_context = copy.copy(ctx)
    profile_context.profile = PROFILE
    # Readiness precedes all builds; no native fallback or implicit emission.
    products = {form: check_product(profile_context, form) for form in forms(ctx)}
    from campaigns.recipes import helper
    profile_audit = helper(ctx.root/'tests/Audit/profile_evidence.py')
    completed = os.environ.get('DOTCC_CPYTHON_UPSTREAM_COMPLETED_CELLS')
    if completed:
        return completed_cells(ctx,products,list(modes(ctx)),completed,profile_audit)
    index_path = os.environ.get('DOTCC_CPYTHON_TEST_PROFILE_EVIDENCE')
    if not index_path or not Path(index_path).is_absolute():
        raise RuntimeError('Explicit test-profile compiled/lifetime evidence index required')
    evidence = json.loads(Path(index_path).read_text())
    identities = {form:profile_audit.identity(ctx.root,form,product,evidence['receipt'])
                  for form,product in products.items()}
    inventory = ctx.root/'tests/Upstream/inventory.frozen.json'
    selection = json.loads(inventory.read_text())
    if selection['status'] != 'frozen':
        raise RuntimeError('Upstream inventory is not frozen')
    project = ctx.root/'tests/Upstream/Upstream.csproj'
    cells = ctx.receipt.setdefault('upstream_cells', [])
    failed = []
    for form, product in products.items():
        for mode in modes(ctx):
            label = 'upstream-' + form + '-' + mode
            cell = {'profile': PROFILE, 'form': form, 'mode': mode, 'product': str(product), 'status': 'unrun'}
            cell['inventory_sha256'] = hashlib.sha256(inventory.read_bytes()).hexdigest()
            cell['translation_identity'] = identities[form]
            cells.append(cell)
            output = ctx.work/label/'app'
            report = ctx.artifacts/(label+'.json')
            arguments = [f'-p:CPythonProject={product}', '--artifacts-path', ctx.work/label/'products',
                         '--disable-build-servers', '-m:1', '-o', output]
            try:
                delivery = helper(ctx.root/'tests/Delivery/campaign_hooks.py')
                if mode == 'jit':
                    delivery.managed(ctx, 'build', project, label+'-build', *arguments)
                else:
                    delivery.managed(ctx, 'publish', project, label+'-publish', *arguments, '-r', ctx.options.rid,
                                     '-p:PublishAot=true', '-p:StripSymbols=true')
                library = output/'cpython/stdlib'
                if not (library/'test/test_grammar.py').is_file():
                    raise RuntimeError('Separate test-profile stdlib/assets missing: ' + str(library))
                executable = output/('Upstream.dll' if mode == 'jit' else 'Upstream')
                cell.update(output=str(output),build_products=str(ctx.work/label/'products'),
                            compiled_artifacts={str(path):hashlib.sha256(path.read_bytes()).hexdigest()
                                for path in sorted({executable,*output.glob('*.dll')})})
                cell['compiled_review'] = profile_audit.compilation_review(
                    ctx,identities[form],cell,'Upstream',label)
                command = delivery.command(output, 'Upstream', mode)
                ctx.run([*command, library, inventory, report, 'qualify'], label+'-run', timeout=suite.timeout, separate=True)
                observed = retain_report(cell, report)
                if (not observed['qualifies_translated_interpreter'] or observed['counts']['selected'] != len(selection['required_cases'])
                        or observed['required_unexecuted']):
                    raise RuntimeError('Invalid/incomplete translated upstream qualification report')
                if profile_audit.identity(ctx.root,form,product,evidence['receipt']) != identities[form]:
                    raise RuntimeError('Test-profile publication changed during upstream execution')
                cell.update(status='passed', report=str(report), counts=observed['counts'])
            except Exception as error:
                cell.update(status='blocked-or-failed', error=str(error), report=str(report) if report.exists() else None)
                try:
                    retain_report(cell, report)
                except Exception as report_error:
                    cell['report_error'] = str(report_error)
                failed.append(label)
            finally:
                ctx.save()
    if failed:
        raise RuntimeError('Required translated upstream cells failed/blocked: ' + ', '.join(failed))
