"""Bind B's helper-role audit to existing, fully published profile assemblies.

This performs metadata inspection only. Product generation, behavioral suites
and upstream assertion execution remain separate campaign operations.
"""
import copy
import hashlib
import json
import os
from pathlib import Path


def run_suite(ctx, suite):
    from campaigns.recipes import helper
    from campaigns.testing import forms
    from campaigns.translation import check_product

    selected = copy.copy(ctx)
    selected.profile = 'upstream-tests-static'
    pointer = selected.root/'artifacts/campaign/current-upstream-tests-static.json'
    pointer_bytes = pointer.read_bytes()
    publication = json.loads(pointer_bytes)
    receipt = Path(publication['receipt'])
    receipt_bytes = receipt.read_bytes()
    key = (hashlib.sha256(receipt_bytes).hexdigest(), tuple(forms(ctx)))
    if ctx.__dict__.get('_cpython_helper_profile_review') == key:
        return
    products = {form: check_product(selected, form) for form in forms(ctx)}
    delivery = helper(ctx.root/'tests/Delivery/campaign_hooks.py')
    auditor = helper(ctx.root/'tests/Audit/helper_profile.py')
    evidence_path = os.environ.get('DOTCC_CPYTHON_TEST_PROFILE_EVIDENCE')
    if evidence_path:
        evidence = json.loads(Path(evidence_path).read_text())
        tool = evidence.get('metadata_tool')
        if not tool or hashlib.sha256(Path(tool['path']).read_bytes()).hexdigest() != tool['sha256']:
            raise RuntimeError('Explicit immutable helper-profile metadata reader missing/changed')
        tools = Path(tool['path']).parent
    else:
        tools = delivery.metadata_tools(ctx)
    rows = ctx.receipt.setdefault('helper_profile_readiness', [])
    failed = []
    for form, project in products.items():
        label = 'helper-profile-'+form
        report = ctx.artifacts/(label+'.json')
        row = {'form': form, 'project': str(project), 'status': 'unrun',
               'translation_receipt': str(receipt), 'receipt_sha256': key[0]}
        rows.append(row)
        try:
            # Translation's final build produced this PE. MetadataAudit does
            # not execute it or implicitly rebuild a missing/stale product.
            assembly = project.parent/'bin/Release/net10.0/TranslatedCPython.dll'
            delivery.inspect(ctx, tools, assembly, label)
            metadata = ctx.artifacts/(label+'.metadata.json')
            result = auditor.audit(receipt, ctx.root, metadata)
            report.write_text(json.dumps(result, indent=2)+'\n')
            check_product(selected, form)
            if pointer.read_bytes() != pointer_bytes or receipt.read_bytes() != receipt_bytes:
                raise RuntimeError('Helper-profile publication changed during readiness review')
            row.update(report=str(report), report_sha256=hashlib.sha256(report.read_bytes()).hexdigest(),
                       status='passed' if result['ready'] else 'failed',
                       issues=result['issues'], compiled=result['compiled'])
            if not result['ready']:
                failed.append(form)
        except Exception as error:
            row.update(status='failed', error=str(error))
            failed.append(form)
        finally:
            ctx.save()
    if failed:
        raise RuntimeError('Required helper-profile readiness failed: '+', '.join(failed))
    ctx.__dict__['_cpython_helper_profile_review'] = key
