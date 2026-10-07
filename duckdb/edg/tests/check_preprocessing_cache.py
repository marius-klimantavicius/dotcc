#!/usr/bin/env python3
"""Focused preprocessing-cache provenance checks; no translation claims."""
import copy
import json
from pathlib import Path
import sys
import tempfile
import unittest
import subprocess
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import translate


class CacheTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.stage = self.root / 'stage'
        self.source = self.stage / 'source'
        self.source.mkdir(parents=True)
        self.headers = self.root / 'headers'
        self.headers.mkdir()
        self.header = self.headers / 'api.h'
        self.header.write_text('struct API { int value; };\n')
        self.provider = self.headers / 'provider.cpp'
        self.provider.write_text('int provider() { return 1; }\n')
        self.forced = self.headers / 'uniform.h'
        self.forced.write_text('#define MANAGED_ABI 1\n')
        self.compiler = self.root / 'compiler'
        self.compiler.write_bytes(b'compiler snapshot')
        self.edg = self.root / 'edg'
        target = self.edg / 'backend/target'
        target.mkdir(parents=True)
        for suffix in ('.def', '.json'):
            (target / ('portable-lp64-binary80-v2' + suffix)).write_text('portable-target')
        self.base = self.edg / 'base'
        self.base.mkdir()
        (self.base / 'gcc.cfg').write_text('gcc profile')
        self.patch = patch.object(translate, 'EDG', self.edg)
        self.patch.start()
        self.addCleanup(self.patch.stop)
        self.units = []
        for name in ('one', 'two'):
            source = self.source / (name + '.cpp')
            source.write_text('#include "api.h"\nint ' + name + '() { return MANAGED_ABI; }\n')
            self.units.append(dict(file=str(source), directory=str(self.source), role='library',
                                   language='c++', arguments=['c++', '-DENABLED=1']))
        self.portable = dict(stagedSource=str(self.source), stagedInputs=[
            dict(path=str(Path(u['file']).relative_to(self.stage)), sha256=translate.sha(u['file']))
            for u in self.units])
        self.portable_path = self.root / 'portable.json'
        self.portable_path.write_text(json.dumps(self.portable))
        self.env = dict(EDG_BASE=str(self.base), EDG_DOT_TARGET='portable-lp64-binary80-v2', LANG='C')
        self.fingerprint, self.initial = self.inputs()
        self.entries = []
        for i, unit in enumerate(self.units):
            output = self.root / (str(i) + '.ii')
            output.write_text('# 1 ' + json.dumps(unit['file']) + '\n# 1 ' + json.dumps(str(self.header)) + '\nint x;\n')
            command, _ = translate.preprocessing_command(self.compiler, 'c++17', [self.headers], [self.forced], unit)
            self.entries.append(dict(index=i, source=unit['file'], sourceSha256=translate.sha(unit['file']),
                command=command, path=str(output), sha256=translate.sha(output), passed=True, exitCode=0))
        dependencies, volatile = translate.dependency_receipt(
            {p for e in self.entries for p in translate.included_files(e['path'])}, self.initial)
        self.report = dict(status='failed', failure='later codegen failure', executionVerified=False,
            selectedUnits=len(self.units), compiler=dict(path=str(self.compiler), sha256=translate.sha(self.compiler)),
            portableManifest=dict(path=str(self.portable_path), sha256=translate.sha(self.portable_path)),
            preprocessingInputs=self.fingerprint, preprocessingDependencies=dependencies,
            preprocessingVolatileInputs=volatile, preprocessingComplete=True, preprocessingInputsUnchanged=True,
            preprocessing=self.entries)
        self.receipt = self.root / 'results.json'
        self.save()

    def inputs(self, **changes):
        options = dict(portable=self.portable, portable_path=self.portable_path, compiler=self.compiler,
            standard='c++17', includes=[self.headers], preincludes=[self.forced], units=self.units,
            env=self.env, target='library')
        options.update(changes)
        return translate.preprocessing_inputs(**options)

    def save(self):
        self.report['preprocessingReceiptSha256'] = translate.preprocessing_receipt_digest(self.report)
        self.receipt.write_text(json.dumps(self.report))

    def reject(self, fingerprint=None):
        with self.assertRaises((ValueError, OSError)):
            translate.reuse_preprocessed(self.receipt, fingerprint or self.fingerprint)

    def test_valid_preprocessing_reused_after_codegen_failure(self):
        entries, deps = translate.reuse_preprocessed(self.receipt, self.fingerprint)
        self.assertEqual(len(entries), 2)
        self.assertEqual(deps, self.report['preprocessingDependencies'])
        self.assertEqual(entries[0]['reusedFrom']['receiptSha256'], translate.sha(self.receipt))
        self.assertNotIn('executionVerified', entries[0])

    def test_provider_implementation_edit_is_eligible(self):
        self.provider.write_text('int provider() { return 42; }\n')
        current, _ = self.inputs()
        self.assertEqual(current, self.fingerprint)
        translate.reuse_preprocessed(self.receipt, current)

    def test_changed_header_forced_header_and_base_profile_rejected(self):
        for path in (self.header, self.forced, self.base / 'gcc.cfg'):
            old = path.read_bytes()
            with self.subTest(path=path):
                path.write_bytes(old + b'\n// changed\n')
                self.reject(self.inputs()[0])
                path.write_bytes(old)

    def test_added_include_candidate_rejected(self):
        (self.headers / 'previously_missing.h').write_text('// changes __has_include')
        self.reject(self.inputs()[0])

    def test_linked_header_directory_target_is_tracked(self):
        external = self.root / 'external'
        external.mkdir()
        header = external / 'linked.h'
        header.write_text('int linked;')
        (self.headers / 'linked').symlink_to(external, target_is_directory=True)
        first, initial = self.inputs()
        self.assertEqual(initial[str(header)], translate.sha(header))
        header.write_text('int changed;')
        self.assertNotEqual(self.inputs()[0], first)

    def test_changed_source_compiler_standard_environment_and_order_rejected(self):
        self.reject(self.inputs(standard='c++20')[0])
        self.reject(self.inputs(units=list(reversed(self.units)))[0])
        self.reject(self.inputs(env=dict(self.env, CPATH='/different'))[0])
        modified = copy.deepcopy(self.units)
        modified[0]['arguments'].append('-DNEW_DEFINE=1')
        self.reject(self.inputs(units=modified)[0])
        source = Path(self.units[0]['file'])
        source.write_text('int changed;')
        self.reject(self.inputs()[0])
        self.compiler.write_bytes(b'different compiler')
        self.reject()

    def test_tampered_preprocessed_file_rejected(self):
        Path(self.entries[1]['path']).write_text('int tampered;')
        self.reject()

    def test_tampered_receipt_rejected(self):
        self.report['preprocessing'][0]['command'].append('-DTAMPERED')
        self.receipt.write_text(json.dumps(self.report))
        self.reject()

    def test_incomplete_or_reordered_entries_rejected(self):
        original = copy.deepcopy(self.report)
        for entries in (self.entries[:1], list(reversed(self.entries))):
            self.report = copy.deepcopy(original)
            self.report['preprocessing'] = entries
            self.save()
            self.reject()

    def test_legacy_and_incomplete_fingerprint_rejected(self):
        self.receipt.write_text('{"status":"preprocessed"}')
        self.reject()
        self.receipt.write_text('{"preprocessingInputs":{"schema":1}}')
        self.reject()

    def test_dependency_content_and_volatile_inputs_rejected(self):
        # A consumed non-header is checked even though ordinary implementation
        # source edits do not change the public-header fingerprint.
        self.report['preprocessingDependencies'].append(dict(path=str(self.provider), sha256=translate.sha(self.provider)))
        self.save()
        self.provider.write_text('changed consumed implementation')
        self.reject()
        self.report['preprocessingDependencies'].pop()
        self.report['preprocessingVolatileInputs'] = [str(self.header)]
        self.save()
        self.reject()

    def test_new_compiler_snapshot_path_is_compatible(self):
        snapshot = self.root / 'compiler-copy'
        snapshot.write_bytes(self.compiler.read_bytes())
        current, _ = self.inputs(compiler=snapshot)
        self.assertEqual(current, self.fingerprint)
        translate.reuse_preprocessed(self.receipt, current)

    def test_untracked_or_changed_consumed_input_rejected(self):
        self.header.write_text('changed during preprocessing')
        with self.assertRaises(ValueError):
            translate.dependency_receipt([str(self.header)], self.initial)
        with self.assertRaises(ValueError):
            translate.dependency_receipt([str(self.compiler)], self.initial)

    def test_driver_reuses_only_application_inputs_and_preprocesses_provider_fresh(self):
        self.portable_path.write_text(json.dumps(dict(self.portable, units=self.units)))
        commands = []

        def provider(edg, acquisition, output, **kwargs):
            output.mkdir()
            wrapper = output / 'provider.cpp'
            wrapper.write_text('#include ' + json.dumps(str(self.provider)) + '\n')
            return dict(standard='c++17', includeDirectories=[str(self.headers)],
                preincludeHeaders=[str(self.forced)], translationUnits=[str(wrapper)],
                generatedInputs=[dict(path=str(wrapper), sha256=translate.sha(wrapper))],
                sources=[dict(path=str(self.provider), sha256=translate.sha(self.provider))])

        def invoke(command, **kwargs):
            commands.append(command)
            if '--preprocess' in command:
                self.assertIn('--preinclude', command)
                # Model EDG's retained line markers, including the forced ABI
                # header. The real CLI smoke separately checks actual EDG.
                kwargs['stdout'].write(('# 1 ' + json.dumps(command[-1]) + '\n# 1 ' +
                    json.dumps(str(self.forced)) + '\nint preprocessed;\n').encode())
            return subprocess.CompletedProcess(command, 0)

        reports = []
        with patch.object(translate, 'prepare_provider', side_effect=provider), \
             patch.object(translate.subprocess, 'run', side_effect=invoke):
            for index in range(2):
                out = self.root / ('attempt-' + str(index))
                args = ['translate.py', '--portable-manifest', str(self.portable_path),
                        '--acquisition', str(self.root), '--compiler', str(self.compiler),
                        '--output-dir', str(out), '--jobs', '1']
                if index:
                    args += ['--reuse-preprocessed', str(self.root / 'attempt-0/results.json')]
                with patch.object(sys, 'argv', args):
                    translate.main()
                reports.append(json.loads((out / 'results.json').read_text()))
        self.assertEqual(len([c for c in commands if '--preprocess' in c]), 4)
        final = [c for c in commands if '--unbound-externals' in c]
        self.assertEqual(len(final), 2)
        for command in final:
            self.assertNotIn('-I', command)
            first_option = command.index('--compiler')
            self.assertTrue(all(path.endswith('.ii') for path in command[2:first_option]))
            self.assertEqual(command[command.index('--unbound-externals') + 1], 'reject')
        a, b = reports
        self.assertEqual(a['preprocessing'][0]['path'], b['preprocessing'][0]['path'])
        self.assertNotEqual(a['providerPreprocessing'][0]['path'], b['providerPreprocessing'][0]['path'])
        self.assertFalse(b['executionVerified'])


if __name__ == '__main__':
    unittest.main()
