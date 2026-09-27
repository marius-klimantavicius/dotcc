#!/usr/bin/env python3
"""Managed QuickJS suites using the shared campaign build, policy and process runner."""
from pathlib import Path
import hashlib
import json
import os
import re
import sys


def run_consumer(ctx, suite):
    from copy import copy
    from dataclasses import replace
    from campaigns.testing import consumer, forms, modes
    original_options, original_recipe = ctx.options, ctx.recipe
    cells = [(form, mode) for form in forms(ctx) for mode in modes(ctx)]
    try:
        for form, mode in cells:
            selected = copy(original_options)
            selected.form, selected.mode = form, mode
            arguments = (*original_recipe.consumer.build_arguments, '--artifacts-path',
                         str(ctx.root / 'build/managed-consumer' / form / mode))
            ctx.options = selected
            ctx.recipe = replace(original_recipe, consumer=replace(original_recipe.consumer, build_arguments=arguments))
            consumer(ctx, suite)
    finally:
        ctx.options, ctx.recipe = original_options, original_recipe


def run(ctx, suite):
    from campaigns.inputs import acquire
    from campaigns.testing import forms, modes
    from campaigns.translation import check_product
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('QuickJS qualification currently requires linux-x64')
    project = ctx.root / 'tests/Behavior/QuickJs.Behavior.csproj'
    source = ctx.sources.get('product')
    if suite.name == 'upstream' and source is None:
        source = acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
        ctx.sources['product'] = source
    cache = getattr(ctx, '_quickjs_harnesses', {})
    ctx._quickjs_harnesses = cache
    results = []
    report = ctx.artifacts / f'{suite.name}-cases.json'
    try:
        for form in forms(ctx):
            product = check_product(ctx, form)
            for mode in modes(ctx):
                rooted = suite.name == 'abi'
                key = (form, mode, rooted)
                label = f'{suite.name}-{form}-{mode}'
                if key not in cache:
                    build_label = f'harness-{form}-{mode}-' + ('rooted' if rooted else 'consumer')
                    output = ctx.work / build_label
                    props = [f'-p:QuickJsProject={product}', f'-p:QuickJsRootLibrary={str(rooted).lower()}']
                    # Separate intermediate assets prevent JIT/AOT/raw/processed state reuse.
                    props += ['--artifacts-path', ctx.root / 'build/managed-harness' / build_label]
                    if mode == 'jit':
                        ctx.managed('build', project, build_label + '-build', *props, '-o', output / 'app')
                        command = ['dotnet', output / 'app/QuickJs.Behavior.dll']
                    else:
                        if rooted:
                            props.append('-p:IlcGenerateMapFile=true')
                        ctx.managed('publish', project, build_label + '-publish', *props,
                                    '-r', ctx.options.rid, '-p:PublishAot=true', '-o', output / 'app', timeout=1800)
                        command = [output / 'app/QuickJs.Behavior']
                    failure = ctx.run([*command, 'fail', ctx.root], build_label + '-failure-control', timeout=30,
                                      check=False, env={'TZ': 'UTC'})
                    command_record = ctx.receipt['commands'][-1]
                    command_record['expected_exit_code'] = 1
                    command_record['deliberate_failure_control'] = True
                    if command_record.get('exit_code') != 1 or 'deliberate assertion failure' not in failure:
                        raise RuntimeError(f'{build_label}: deliberate assertion did not propagate correctly')
                    executable = Path(command[1] if mode == 'jit' else command[0])
                    ctx.receipt.setdefault('managed_harnesses', []).append(dict(
                        form=form, mode=mode, whole_library_rooted=rooted,
                        product=str(product), output=str(output / 'app'),
                        executable=str(executable), executable_sha256=hashlib.sha256(executable.read_bytes()).hexdigest(),
                        expected_failure_verified=True))
                    if mode == 'aot' and rooted:
                        maps = list((ctx.root / 'build/managed-harness' / build_label).rglob('QuickJs.Behavior.map.xml'))
                        if len(maps) != 1:
                            raise RuntimeError(f'{build_label}: expected one actual NativeAOT method map, got {maps}')
                        import shutil
                        retained = ctx.artifacts / f'{build_label}.map.xml'
                        shutil.copy2(maps[0], retained)
                        ctx.receipt['managed_harnesses'][-1]['aot_map'] = str(retained)
                        ctx.receipt['managed_harnesses'][-1]['aot_map_sha256'] = hashlib.sha256(retained.read_bytes()).hexdigest()
                    ctx.save()
                    cache[key] = command
                transcript = ctx.run([*cache[key], suite.name, ctx.root, source or ''], label + '-run',
                                     timeout=180, env={'TZ': 'UTC'})
                match = re.search(r'^SUMMARY passed=(\d+) failed=0$', transcript, re.M)
                if match is None or int(match.group(1)) < 1:
                    raise RuntimeError(f'{label}: missing passing case summary')
                results.append(dict(form=form, mode=mode, status='pass',
                                    harness_cases=[line[5:] for line in transcript.splitlines() if line.startswith('PASS ')],
                                    engine_cases=[line[10:] for line in transcript.splitlines() if line.startswith('CASE PASS ')],
                                    exclusions=[line[5:] for line in transcript.splitlines() if line.startswith('SKIP ')],
                                    whole_library_rooted=rooted))
    finally:
        report.write_text(json.dumps({'suite': suite.name, 'requested_cells': [[f, m] for f in forms(ctx) for m in modes(ctx)], 'results': results}, indent=2) + '\n')


if __name__ == '__main__':
    root = Path(__file__).resolve().parents[1]
    # Reuse shared argument handling, source policy, build/publish and receipts.
    os.execv(sys.executable, [sys.executable, str(root.parent / 'Scripts/campaign.py'), 'test', 'quickjs', *sys.argv[1:]])
