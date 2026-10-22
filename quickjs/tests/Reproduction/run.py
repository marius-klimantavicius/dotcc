#!/usr/bin/env python3
"""Disposable offline provenance, guarded staging and publication checks.

Uses the shared framework APIs and the compiler snapshot from a completed translation.
Never acquires network inputs or writes canonical generated products.
"""
import argparse
from dataclasses import replace
import json
from pathlib import Path
import shutil
import sys
import tarfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.delivery import promote
from campaigns.identity import HashPolicy, digest, write_json
from campaigns.inputs import acquire
from campaigns.process import execute
from campaigns.recipes import helper


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def rejects(action, fragment):
    try:
        action()
    except RuntimeError as error:
        require(fragment in str(error), str(error))
        return str(error)
    raise RuntimeError('Expected rejection: ' + fragment)


def authored_pickup(receipt, output, report):
    """Rebuild both products with a temporary original-path host edit."""
    host = ROOT / 'src/Host/QuickJsHost.cs'
    original = host.read_bytes()
    generated = {Path(row['project']).parent / name: value
                 for row in receipt['outputs'].values() for name, value in row['hashes'].items()}
    probe = b'\npublic static class DotccAuthoredPickupProbe { public static int Value => 42; }\n'
    observations = []
    try:
        host.write_bytes(original + probe)
        for form in ('raw', 'processed'):
            product = Path(receipt['outputs'][form]['project'])
            require(any(node.get('Include') == str(host) for node in ET.parse(product).iter('Compile')),
                    'Product does not link the original authored host: ' + form)
            directory = output / 'authored-pickup' / form
            directory.mkdir(parents=True)
            project = directory / 'Pickup.csproj'
            tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
            properties = ET.SubElement(tree, 'PropertyGroup')
            ET.SubElement(properties, 'OutputType').text = 'Exe'
            ET.SubElement(properties, 'TargetFramework').text = 'net10.0'
            ET.SubElement(properties, 'ImplicitUsings').text = 'enable'
            ET.SubElement(ET.SubElement(tree, 'ItemGroup'), 'ProjectReference', Include=str(product))
            ET.ElementTree(tree).write(project, encoding='unicode')
            (directory / 'Program.cs').write_text(
                'using Managed.Interpreters;\n'
                'if (DotccAuthoredPickupProbe.Value != 42) return 1;\n'
                'Console.WriteLine("PASS original-path authored host pickup: 42");\nreturn 0;\n')
            for label, command in [
                (form + '-authored-pickup-build', ['dotnet', 'build', str(project), '-c', 'Release', '--nologo', '-m:1',
                    '--artifacts-path', str(directory / 'managed'), '-o', str(directory / 'app')]),
                (form + '-authored-pickup-run', ['dotnet', str(directory / 'app/Pickup.dll')]),
            ]:
                record = {}
                log = output / (label + '.log')
                execute(command, label=label, cwd=ROOT.parent, output=log, record=record, timeout=900)
                report['commands'].append(record)
                if label.endswith('-run'):
                    require(log.read_text().strip() == 'PASS original-path authored host pickup: 42',
                            'Authored host edit was not observed: ' + form)
            observations.append(dict(form=form, status='passed', executable=str(directory / 'app/Pickup.dll'),
                                     executable_sha256=digest(directory / 'app/Pickup.dll')))
    finally:
        host.write_bytes(original)
    require(host.read_bytes() == original, 'Authored pickup did not restore host bytes')
    require(all(digest(path) == value for path, value in generated.items()), 'Authored pickup changed generated source')
    return dict(status='passed', host=str(host), restored_sha256=digest(host),
                generated_sources_unchanged=True, observations=observations)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--receipt', required=True, type=Path)
    parser.add_argument('--qualification-receipt', type=Path,
                        help='Passing current-product receipt when a later suite failed in the translation receipt')
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'scope': 'disposable full-engine object emission; no canonical publication',
              'receipt': str(args.receipt.resolve()), 'checks': [], 'commands': []}
    try:
        receipt = json.loads(args.receipt.read_text())
        require(any(task['name'] == 'translation' and task['status'] == 'passed'
                    for task in receipt.get('tasks', [])), 'A completed translation task is required')
        report['translation_receipt_status'] = receipt['status']
        if receipt['status'] != 'passed':
            require(args.qualification_receipt is not None,
                    'A later passing qualification receipt is required for these products')
            qualified = json.loads(args.qualification_receipt.read_text())
            require(qualified['status'] == 'passed', 'Qualification receipt did not pass')
            require(set(qualified.get('tested_artifacts', {})) == set(receipt['outputs']),
                    'Qualification receipt did not test both translated product forms')
            for form, row in receipt['outputs'].items():
                require(qualified['tested_artifacts'][form] == row['project'],
                        'Qualification receipt tested another product: ' + form)
            report['qualification_receipt'] = dict(path=str(args.qualification_receipt.resolve()),
                                                   sha256=digest(args.qualification_receipt))
        for row in receipt['outputs'].values():
            for name, expected in row['hashes'].items():
                require(digest(Path(row['project']).parent / name) == expected,
                        'Translated product changed since its recorded emission: ' + name)
        recipe = helper(ROOT / 'scripts/campaign.py')
        staging = helper(ROOT / 'scripts/stage.py')
        pinned = next(recipe.sources(ROOT))
        authored = sorted((ROOT / 'src').rglob('*.cs')) + sorted((ROOT / 'samples').rglob('*.cs'))
        authored = [p for p in authored if not {'bin', 'obj'}.intersection(p.relative_to(ROOT).parts)]
        before = {str(p.relative_to(ROOT)): digest(p) for p in authored}
        reference_before = digest(pinned.directory / 'quickjs.c')
        edited = output / 'edited-reference'
        shutil.copytree(pinned.directory, edited)
        marker = '\n/* Legitimate local reproduction edit outside adaptation anchors. */\n'
        with (edited / 'quickjs.c').open('a') as stream:
            stream.write(marker)
        archive = output / 'edited-source.tar'
        with tarfile.open(archive, 'w') as bundle:
            bundle.add(edited, arcname=pinned.prefix)
        require(digest(archive) != pinned.sha256, 'Edited archive must differ from pinned provenance')
        template = next(c['command'] for c in receipt['commands'] if c['label'] == '000-quickjs')
        original_work = ROOT / receipt['staging']
        compiler = Path(template[1])
        require(digest(compiler) == receipt['tools']['DotCC']['dotcc.dll'], 'Compiler snapshot changed')
        require(digest(compiler.parent / 'DotCC.Lib.dll') == receipt['tools']['DotCC']['DotCC.Lib.dll'], 'Compiler library changed')
        candidates = []
        for mode in ('warn', 'off'):
            work = output / mode
            work.mkdir()
            source = replace(pinned, archive=archive, directory=work / 'reference')
            policy = HashPolicy(mode)
            tree = acquire(source, policy, fetch='never')
            require((tree / 'quickjs.c').read_text().endswith(marker), 'Local archive edit lost')
            require(bool(policy.warnings) == (mode == 'warn'), 'Unexpected provenance warning policy')
            # Existing edited trees remain valid and are never repaired from archives.
            with (tree / 'cutils.c').open('a') as stream:
                stream.write(marker)
            edited_hash = digest(tree / 'cutils.c')
            require(acquire(source, policy, fetch='never') == tree, 'Existing tree replaced')
            require(digest(tree / 'cutils.c') == edited_hash, 'Local tree edit overwritten')
            staged = work / 'source'
            adaptations = staging.stage(tree, staged)
            require((staged / 'quickjs.c').read_text().endswith(marker), 'Staging lost unrelated edit')
            require(digest(tree / 'quickjs.c') == digest(edited / 'quickjs.c'), 'Staging changed reference')
            candidate = work / 'candidate'
            candidate.mkdir()
            obj = candidate / 'quickjs.cs'
            overrides = work / 'overrides.jsonl'
            command = [arg.replace(str(original_work / 'source'), str(staged)) for arg in template]
            command[command.index('-o') + 1] = str(obj)
            command[command.index('--override-report') + 1] = str(overrides)
            record = {}
            execute(command, label=mode + '-full-engine-object', cwd=ROOT.parent,
                    output=output / (mode + '-object.log'), record=record)
            report['commands'].append(record)
            records = [json.loads(line) for line in overrides.read_text().splitlines()]
            require(any(row.get('name') == 'DIRECT_DISPATCH' and row.get('event') == 'selected'
                        and row.get('effective') == '0' for row in records), 'Dispatch override absent')
            require(obj.stat().st_size > 0, 'No emitted full-engine object')
            write_json(candidate / '.campaign-files.json', ['quickjs.cs'])
            candidates.append(candidate)
            report['checks'].append(dict(mode=mode, fetch='never', warnings=policy.warnings,
                                         archive_sha256=digest(archive), local_tree_edit_preserved=True,
                                         full_engine_object_sha256=digest(obj), adaptations=adaptations))
        strict = replace(pinned, archive=archive, directory=output / 'strict-reference')
        report['strict_archive_rejection'] = rejects(lambda: acquire(strict, HashPolicy('strict'), 'never'), 'Hash differs')
        strict_tree = replace(strict, sha256=digest(archive), directory=output / 'warn/reference')
        report['strict_tree_rejection'] = rejects(lambda: acquire(strict_tree, HashPolicy('strict'), 'never'), 'Hash differs')
        # Exact local guards remain mandatory regardless of hash policy.
        for mode in ('warn', 'off'):
            tree = output / mode / 'reference'
            path = tree / 'quickjs.c'
            anchor = '#define DIRECT_DISPATCH  1\n'
            require(path.read_text().count(anchor) == 1, 'Dispatch guard test anchor drifted')
            path.write_text(path.read_text().replace(anchor, '#define DIRECT_DISPATCH  2\n', 1))
            staged = output / mode / 'invalid-stage'
            staging.stage(tree, staged)
            command = [arg.replace(str(original_work / 'source'), str(staged)) for arg in template]
            command[command.index('-o') + 1] = str(output / mode / 'rejected.cs')
            command[command.index('--override-report') + 1] = str(output / mode / 'rejected-overrides.jsonl')
            record = {}
            log = output / (mode + '-dispatch-guard.log')
            execute(command, label=mode + '-dispatch-guard', cwd=ROOT.parent, output=log, record=record, check=False)
            report['commands'].append(record)
            require(record['exit_code'] != 0 and 'DIRECT_DISPATCH' in log.read_text(), 'Changed dispatch macro was not rejected')
            require(not (output / mode / 'rejected.cs').exists(), 'Rejected override emitted an object')
            report['checks'].append(dict(mode=mode, guard_rejection='Compiler rejected changed DIRECT_DISPATCH definition', log=str(log)))
            # Ignoring the expected branch hint is justified only by the
            # approved literal body. A local edit introducing a call must fail
            # even when provenance checks are permissive.
            path.write_text(path.read_text().replace('#define DIRECT_DISPATCH  2\n', anchor, 1))
            hints = tree / 'cutils.h'
            hint_anchor = '#define likely(x)       __builtin_expect(!!(x), 1)'
            require(hints.read_text().count(hint_anchor) == 1, 'Branch-hint guard test anchor drifted')
            hints.write_text(hints.read_text().replace(hint_anchor,
                '#define likely(x)       __builtin_expect(!!(x), expected_side_effect())', 1))
            staged = output / mode / 'invalid-hint-stage'
            staging.stage(tree, staged)
            command = [arg.replace(str(original_work / 'source'), str(staged)) for arg in template]
            command[command.index('-o') + 1] = str(output / mode / 'rejected-hint.cs')
            command[command.index('--override-report') + 1] = str(output / mode / 'rejected-hint-overrides.jsonl')
            record = {}
            log = output / (mode + '-branch-hint-guard.log')
            execute(command, label=mode + '-branch-hint-guard', cwd=ROOT.parent, output=log, record=record, check=False)
            report['commands'].append(record)
            require(record['exit_code'] != 0 and 'likely' in log.read_text(), 'Changed branch hint was not rejected')
            require(not (output / mode / 'rejected-hint.cs').exists(), 'Rejected branch hint emitted an object')
            report['checks'].append(dict(mode=mode, guard_rejection='Compiler rejected side-effecting expected branch hint', log=str(log)))
        # Exercise the actual shared transactional publisher on emitted objects,
        # with an initially absent destination and a preserved user-owned file.
        target, journal = output / 'published-object', output / 'publication.json'
        promote([(candidates[0], target)], journal, lambda: require((target / 'quickjs.cs').is_file(), 'Publication missing'))
        (target / 'user-note.txt').write_text('Unowned files survive repeated publication.\n')
        promote([(candidates[1], target)], journal, lambda: require((target / 'user-note.txt').is_file(), 'Unowned file lost'))
        published_hash = digest(target / 'quickjs.cs')
        failed = output / 'failed-candidate'
        failed.mkdir()
        (failed / 'quickjs.cs').write_text('Deliberately invalid candidate')
        write_json(failed / '.campaign-files.json', ['quickjs.cs'])
        report['rollback'] = rejects(lambda: promote([(failed, target)], journal,
            lambda: require(False, 'deliberate publication failure')), 'deliberate publication failure')
        require(digest(target / 'quickjs.cs') == published_hash and (target / 'user-note.txt').is_file(), 'Rollback lost valid output')
        report['authored_pickup_evidence'] = authored_pickup(receipt, output, report)
        after = {str(p.relative_to(ROOT)): digest(p) for p in authored}
        require(before == after, 'Authored implementations changed during reproduction')
        require(digest(pinned.directory / 'quickjs.c') == reference_before, 'Canonical acquired reference changed')
        report['authored_hashes_before'] = before
        report['authored_hashes_after'] = after
        report['reference_preserved'] = True
        report['transactional_publication'] = 'clean destination, repeat preserving unowned file, rejected candidate restored prior output'
        report['status'] = 'passed'
    except BaseException as error:
        report['status'] = 'failed'
        report['error'] = str(error)
        raise
    finally:
        write_json(output / 'report.json', report)
    print(output / 'report.json')


if __name__ == '__main__':
    main()
