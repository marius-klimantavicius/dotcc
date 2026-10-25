#!/usr/bin/env python3
"""Qualify valid ELF execution via public BlinkMachine in both execution modes.

Requires a wholly fresh strict campaign producer. Builds only private copies of
the delivered product and authored public API/worker source closure.
"""
import argparse
import base64
import difflib
import json
import os
from pathlib import Path
import shutil
import sys
import xml.etree.ElementTree as ET
from evidence import Evidence, HERE, ROOT, REPO, module, sha, workers

sys.path.insert(0, str(ROOT / 'scripts'))
from campaign_delivery import load_clean_delivery
from core_inputs import profile_sources


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--clean-delivery', type=Path, required=True)
    parser.add_argument('--native-receipt', type=Path, help='Passing build.py receipt; otherwise build/run native fixtures first')
    parser.add_argument('--form', choices=['raw', 'processed', 'all'], default='processed')
    parser.add_argument('--aot', action='store_true', help='Also publish/run a matching NativeAOT consumer and worker')
    args = parser.parse_args()
    evidence = Evidence('managed-' + args.form + ('-jit-aot' if args.aot else '-jit'))
    receipt, directory = evidence.value, evidence.directory
    pin, run = evidence.pin, evidence.run
    receipt['executions'] = []
    try:
        for path in (Path(__file__), HERE / 'evidence.py', HERE / 'Program.cs', HERE / 'Probe.csproj',
                     ROOT / 'scripts/campaign_delivery.py', ROOT / 'scripts/core_inputs.py'):
            pin(path)
        producer_path, delivery = load_clean_delivery(pin(args.clean_delivery))
        pin(producer_path)
        receipt['delivery'] = dict(path=str(producer_path), sha256=sha(producer_path), assembly=delivery['assembly'])
        profile = Path(delivery['profile'])
        inputs = json.loads(pin(profile / 'inputs.json', delivery['profile_inputs_sha256']).read_text())
        assembly = json.loads(pin(delivery['assembly']['path'], delivery['assembly']['sha256']).read_text())
        if json.loads(pin(profile / 'instance-abi.json').read_text()) != {'abi': 'instance-v1'}:
            raise RuntimeError('Public ELF suite requires the instance-v1 threaded product')
        config = pin(profile / 'config.h').read_text()
        if '#define DISABLE_X87' in config or '#define DISABLE_MMX' in config or not {
                'blink/fpu.c', 'blink/mmx.c', 'blink/ldbl.c'} <= set(assembly['objects']):
            raise RuntimeError('Fresh real upstream x87/MMX baseline implementation required')
        # A previous passing producer is insufficient: require today's actual
        # staged argv correction and preserve its source provenance in this run.
        entries = profile_sources(profile, ROOT, inputs)
        argv = next(row for row in entries if row['path'] == 'blink/argv.c')
        staged_argv = pin(ROOT / argv['staged_path'], assembly['objects']['blink/argv.c']['source_sha256'])
        stage = module(pin(ROOT / 'src/Host/scripts/stage_elf_entry.py'), 'elf_runtime_entry_stage')
        if staged_argv.read_bytes().count(stage.CORRECTION) != 1 or stage.ANCHOR in staged_argv.read_bytes():
            raise RuntimeError('Fresh corrected argv.c producer required')
        bindings = json.loads(pin(profile / 'host-binding-receipt.json').read_text())
        entry_row = next(row for row in bindings['upstream_preambles'] if row['source'] == 'blink/argv.c')
        if entry_row['elf_entry_register_correction']['helper_sha256'] != sha(ROOT / 'src/Host/scripts/stage_elf_entry.py'):
            raise RuntimeError('Entry correction provenance differs')
        receipt['entry_register_producer'] = entry_row
        memory = module(pin(ROOT / 'scripts/memory_bindings.py'), 'elf_runtime_memory_bindings')
        for name in memory.INPUTS:
            pin(ROOT / name, bindings['source_inputs'][name])
        mapping = pin(ROOT / 'src/Host/guest-private-mapping.inc').read_text()
        memory_sources = {}
        upstream = json.loads(pin(ROOT / 'config/source-manifest.json').read_text())['upstream']['directory']
        for unit in ('memorymalloc.c', 'memory.c', 'throw.c'):
            entry = next(row for row in entries if row['path'] == 'blink/' + unit)
            source = pin(ROOT / entry['staged_path'], assembly['objects']['blink/' + unit]['source_sha256'])
            text = source.read_text()
            if unit == 'memorymalloc.c':
                if mapping not in text: raise RuntimeError('Fresh private file mapping producer required')
            else:
                original = pin(ROOT / 'ref' / upstream / 'blink' / unit).read_bytes()
                before = original.decode().splitlines(keepends=True)
                after = memory.adapt('blink/' + unit, original).decode().splitlines(keepends=True)
                for tag, _, _, start, end in difflib.SequenceMatcher(a=before, b=after, autojunk=False).get_opcodes():
                    if tag in ('insert', 'replace') and ''.join(after[start:end]) not in text:
                        raise RuntimeError('Fresh private mapping permission/EOF/signal producer required: ' + unit)
            memory_sources[unit] = dict(path=str(source), sha256=sha(source))
        receipt['memory_producer'] = dict(inputs={name: bindings['source_inputs'][name] for name in memory.INPUTS},
                                        sources=memory_sources)
        if args.native_receipt:
            native_path = pin(args.native_receipt)
        else:
            stdout, _ = run('native-fixtures', [sys.executable, HERE / 'build.py'], timeout=180)
            native_path = pin(stdout.decode().strip().split('receipt=')[-1])
        native = json.loads(native_path.read_text())
        if not native.get('passed') or not native.get('completed') or native.get('managed_execution') is not False:
            raise RuntimeError('A passing finite native ELF fixture receipt is required')
        for path, digest in native['inputs'].items(): pin(path, digest)
        images = Path(native['images'])
        if workers.manifest(images, True) != native['image_files']:
            raise RuntimeError('Retained native/guest fixture closure changed')
        receipt['native'] = dict(path=str(native_path), sha256=sha(native_path))
        body = staged_argv.read_bytes(); body = body[body.index(b'#define STACKALIGN'):]
        if body != (native_path.parent / 'adapted-argv.inc').read_bytes():
            raise RuntimeError('Native LoadArgv invariant did not execute the actual producer body')
        api = module(pin(ROOT / 'tests/MachineApi/run.py'), 'elf_runtime_machine_api')
        source_closure = api.source_closure(HERE / 'Probe.csproj')
        forms = ['raw', 'processed'] if args.form == 'all' else [args.form]
        dotnet = pin(shutil.which('dotnet'))
        for form in forms:
            private = directory / form / 'private'
            library = Path(delivery['raw_snapshot' if form == 'raw' else 'stable_output'])
            copied_library = private / library.relative_to(ROOT)
            selected = workers.source_closure(library / 'TranslatedBlink.csproj')
            # The API closure selects the current default processed library.
            # Replace only that library selection with the requested form.
            default_library = Path(delivery['stable_output'])
            closure = {p: d for p, d in source_closure.items() if not p.is_relative_to(default_library)}
            closure.update(selected)
            frozen = {}
            for original, digest in closure.items():
                pin(original, digest)
                target = private / original.relative_to(ROOT)
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(original, target)
                pin(target, digest)
                frozen[str(original)] = digest
            receipt.setdefault('source_closures', {})[form] = frozen
            for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json'):
                source = REPO / name
                if source.is_file():
                    pin(source); shutil.copyfile(source, private / name); pin(private / name)
            # Preserve original generated project/reference paths in the private
            # copy; BlinkProject selects this form for API and worker alike.
            project = private / 'tests/ElfRuntime/Probe.csproj'
            common = ['-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false',
                      '-m:1', '-p:BlinkProject=' + str(copied_library / 'TranslatedBlink.csproj')]
            run(form + '-build', [dotnet, 'build', project, *common], timeout=1200)
            jit = directory / form / 'jit'
            shutil.copytree(project.parent / 'bin/Release/net10.0', jit)
            if not (jit / 'blink-worker/Managed.Emulation.Worker.dll').is_file():
                raise RuntimeError('Ordinary public consumer build did not deploy its worker')
            modes = [('jit', jit, [dotnet, jit / 'Probe.dll'])]
            if args.aot:
                aot = directory / form / 'aot'
                run(form + '-publish-aot', [dotnet, 'publish', project, *common, '-r', 'linux-x64',
                    '-p:PublishAot=true', '-o', aot], timeout=2400)
                if not (aot / 'Probe').is_file() or not (aot / 'blink-worker/Managed.Emulation.Worker').is_file():
                    raise RuntimeError('Ordinary public consumer publish did not deploy its matching AOT worker')
                modes.append(('aot', aot, [aot / 'Probe']))
            for execution, app, command in modes:
                before = workers.manifest(app, True)
                receipt.setdefault('execution_closures', {})[form + '-' + execution] = before
                for mode in ('InProcess', 'SeparateProcess'):
                    for fixture in native['fixtures']:
                        scenarios = ['run', 'disabled', 'missing-interpreter', 'directory-interpreter', 'missing-libc'] if fixture['interpreter'] else ['run']
                        for scenario in scenarios:
                            label = '-'.join((form, execution, mode, fixture['name'], scenario))
                            report_path = directory / (label + '.json')
                            stdout, stderr = run(label, [*command, mode, images, fixture['name'], scenario,
                                fixture['interpreter'] or '-', report_path], timeout=40)
                            report = json.loads(pin(report_path).read_text())
                            if (not report['Passed'] or report['Mode'] != mode or report['Fixture'] != fixture['name'] or
                                report['Scenario'] != scenario or not report['WorkerGone'] or
                                stdout != f"elf-runtime {mode} {fixture['name']} {scenario} PASS\n".encode() or stderr):
                                raise RuntimeError('Public API evidence differs: ' + label)
                            result = report['Result']
                            if scenario == 'run':
                                if (base64.b64decode(result['StandardOutput']) != fixture['stdout'].encode() or
                                    base64.b64decode(result['StandardError']) != fixture['stderr'].encode() or
                                    result['ExitCode'] != fixture['exit_code'] or report['Written'] != fixture.get('written')):
                                    raise RuntimeError('Exact native comparison differs: ' + label)
                            receipt['executions'].append(dict(form=form, execution=execution, mode=mode,
                                fixture=fixture['name'], scenario=scenario, report=str(report_path),
                                sha256=sha(report_path), passed=True, instructions=result['Instructions']))
                            evidence.save()
                if workers.manifest(app, True) != before:
                    raise RuntimeError('Executed app/worker closure changed: ' + form + '/' + execution)
        evidence.check()
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    return evidence.finish()


if __name__ == '__main__':
    raise SystemExit(main())
