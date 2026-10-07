#!/usr/bin/env python3
"""Focused opt-in extra unity validation; preserve all synthetic evidence."""
import argparse
import copy
import json
from pathlib import Path
import sys
import unittest

sys.dont_write_bytecode = True
import check_unity
from check_unity import unity


class ExtraUnityTests(unittest.TestCase):
    write_members = check_unity.UnityTests.write_members

    def setUp(self):
        check_unity.UnityTests.setUp(self)
        self.remaining_map = {item['source']: item for item in self.remaining}
        self.upstream = unity.select_groups(self.native, self.portable, self.remaining_map)['groups']
        self.evidence = self.root / 'approval.json'
        self.manifest = dict(schema=1, kind='duckdb-extra-unity-plan', groups=[dict(
            id='extra', target='library', orderedSources=['source3.cpp', 'source5.cpp'])])

    def select(self, preprocess=False):
        self.evidence.write_text(json.dumps(self.manifest))
        return unity.select_extra_groups(self.evidence, unity.sha(self.evidence), self.portable,
                                        self.remaining_map, self.upstream, 'c++17', preprocess)

    def approve(self):
        wrapper = self.root / 'tested.cpp'
        members = [unity.record(self.source / ('source%d.cpp' % i)) for i in (3, 5)]
        wrapper.write_text(''.join('#include ' + json.dumps(m['path']) + '\n' for m in members))
        units = [dict(self.units[i], file=str(self.source / ('source%d.cpp' % i)), arguments=[
            value.replace(str(self.stage), str(self.source)) for value in self.units[i]['arguments']]) for i in (3, 5)]
        flags = unity.native_compile_flags(units[0])
        output = self.root / 'object.o'
        output.write_bytes(b'fixture object')
        log = self.root / 'log'
        log.write_text('synthetic successful command receipt')
        command = dict(exitCode=0, argv=['c++', *flags, '-c', str(wrapper), '-o', str(output)],
                       stdout=unity.record(log), stderr=unity.record(log))
        counters = dict(assertionsPassed=3, assertionsFailed=0, assertionsExpectedFailure=0,
                        casesPassed=1, casesFailed=0, casesExpectedFailure=0, skipped=0)
        observation = dict(exitCode=0, events=[dict(kind='case', name='case', aborting=False, **counters),
                                             dict(kind='run', aborting=False, **counters)])
        acquired = unity.record(self.source.parent / 'source-manifest.json')
        self.comparison = dict(status='native-comparison-passed', selectedCases=['case'],
            tests=[dict(selector='case', modes=dict(baseline=observation, candidate=copy.deepcopy(observation)),
                        observationsEqual=True)], totals=dict(baseline=counters, candidate=copy.deepcopy(counters)),
            staticExtensions=['example'], baselineInputsUnchanged=True, baselineInputsBefore=[], baselineInputsAfter=[],
            unmatchedCases=[], incompleteCases=[], commands=[command], profiles=dict(extra=dict(units=units,
                memberInputs=members, wrapper=unity.record(wrapper), compileArgv=command['argv'], object=unity.record(output))),
            archives=dict(extra=dict(original=unity.record(output), candidate=unity.record(output))),
            baseline=unity.record(log), baselineRunner=unity.record(output), candidateRunner=unity.record(output),
            sourceManifest=acquired, sourceInventoryReceipt=unity.record(log), recipe=unity.record(log))
        self.comparison_path = self.root / 'comparison.json'
        self.manifest.update(kind='duckdb-extra-unity-native-comparison', status='passed', standard='c++17',
                             sourceRoot=str(self.source), acceptedCaseSelectors=['case'], additionalPartialObservations=[])
        self.manifest['groups'][0].update(nativeCompileFlags=flags, wrapperCompileFlags=flags,
            effectiveSourceHashes={'source%d.cpp' % i: unity.sha(self.stage / ('source%d.cpp' % i)) for i in (3, 5)})
        self.save_comparison()

    def save_comparison(self):
        self.comparison_path.write_text(json.dumps(self.comparison))
        self.manifest['comparison'] = unity.record(self.comparison_path)

    def test_default_and_opt_in_partition(self):
        self.assertEqual(len(self.upstream), 1)
        result = self.select()
        self.assertFalse(result['approved'])
        groups = self.upstream + result['groups']
        for group in groups:
            group['coveredSourceIndices'] = [self.remaining_map[s]['sourceIndex'] for s in group['orderedMembers']]
        inputs = unity.compose_inputs(self.base['programInputs'], groups, [dict(path='one'), dict(path='two')], 6)
        self.assertEqual(len(inputs), 3)
        self.assertEqual(inputs[-1]['kind'], 'audited-extra-unity-group')
        with self.assertRaisesRegex(ValueError, 'without passing'):
            self.select(preprocess=True)

    def test_membership_order_overlap_core_and_duplicates(self):
        for names in (['source5.cpp', 'source3.cpp'], ['source3.cpp', 'source3.cpp'],
                      ['source0.cpp', 'source3.cpp'], ['source2.cpp', 'source3.cpp'],
                      ['../source3.cpp', 'source5.cpp']):
            self.manifest['groups'][0]['orderedSources'] = names
            with self.subTest(names=names), self.assertRaises(ValueError):
                self.select()

    def test_native_and_frontend_flags_must_match(self):
        for flag in ('-DOTHER=1', '-O0', '-fno-exceptions'):
            self.units[5]['arguments'].append(flag)
            with self.subTest(flag=flag), self.assertRaisesRegex(ValueError, 'flags'):
                self.select()
            self.units[5]['arguments'].pop()

    def test_approved_profile_and_checksums(self):
        self.approve()
        result = self.select(preprocess=True)
        self.assertTrue(result['approved'])
        self.assertEqual(result['observations']['acceptedTotals']['candidate']['assertionsPassed'], 3)
        self.evidence.write_text('changed')
        with self.assertRaisesRegex(ValueError, 'checksum'):
            unity.select_extra_groups(self.evidence, 'bad', self.portable, self.remaining_map, [], 'c++17')

    def test_wrapper_omission_and_reordering_rejected_even_when_rehashed(self):
        self.approve()
        profile = self.comparison['profiles']['extra']
        path = Path(profile['wrapper']['path'])
        for members in (profile['memberInputs'][:1], list(reversed(profile['memberInputs']))):
            path.write_text(''.join('#include ' + json.dumps(item['path']) + '\n' for item in members))
            profile['wrapper'] = unity.record(path)
            self.save_comparison()
            with self.assertRaisesRegex(ValueError, 'contents/order'):
                self.select(True)

    def test_changed_binary_and_failed_native_command_rejected(self):
        self.approve()
        self.comparison['commands'][0]['exitCode'] = 1
        self.save_comparison()
        with self.assertRaisesRegex(ValueError, 'failed commands'):
            self.select(True)
        self.comparison['commands'][0]['exitCode'] = 0
        self.save_comparison()
        Path(self.comparison['candidateRunner']['path']).write_bytes(b'changed')
        with self.assertRaisesRegex(ValueError, 'dependency changed'):
            self.select(True)

    def test_standard_and_effective_source_mismatch(self):
        self.approve()
        self.manifest['standard'] = 'c++20'
        with self.assertRaisesRegex(ValueError, 'standard'):
            self.select(True)
        self.manifest['standard'] = 'c++17'
        (self.stage / 'source3.cpp').write_text('changed')
        with self.assertRaisesRegex(ValueError, 'effective source'):
            self.select(True)

    def test_failures_skips_expected_failures_empty_and_observation_mismatch(self):
        self.approve()
        original = copy.deepcopy(self.comparison)
        for key, value in [('assertionsFailed', 1), ('assertionsExpectedFailure', 1),
                           ('skipped', 1), ('assertionsPassed', 0)]:
            self.comparison = copy.deepcopy(original)
            self.comparison['tests'][0]['modes']['candidate']['events'][0][key] = value
            self.save_comparison()
            with self.subTest(key=key), self.assertRaises(ValueError):
                self.select(True)
        self.comparison = copy.deepcopy(original)
        self.comparison['tests'] = []
        self.save_comparison()
        with self.assertRaisesRegex(ValueError, 'omitted'):
            self.select(True)

    def test_relabelled_native_standard_rejected(self):
        self.approve()
        spec = self.manifest['groups'][0]
        spec['nativeCompileFlags'].append('-std=c++20')
        spec['wrapperCompileFlags'] = list(spec['nativeCompileFlags'])
        profile = self.comparison['profiles']['extra']
        for unit in profile['units']:
            unit['arguments'].append('-std=c++20')
        profile['compileArgv'].append('-std=c++20')
        self.save_comparison()
        with self.assertRaisesRegex(ValueError, 'effective language standard'):
            self.select(True)

    def test_matched_partial_observation_is_separate_from_acceptance(self):
        self.approve()
        partial = copy.deepcopy(self.comparison['tests'][0])
        partial['selector'] = 'partial'
        for mode in partial['modes'].values():
            mode['events'][0]['name'] = 'partial'
            for event in mode['events']:
                event['skipped'] = 1
        self.comparison['tests'].append(partial)
        self.comparison['selectedCases'].append('partial')
        for total in self.comparison['totals'].values():
            total.update(assertionsPassed=6, casesPassed=2, skipped=1)
        self.manifest['additionalPartialObservations'] = [dict(selector='partial', reason='unbundled dependency')]
        self.save_comparison()
        result = self.select(True)
        self.assertEqual(result['observations']['acceptedTotals']['candidate']['assertionsPassed'], 3)
        self.assertEqual(result['observations']['totals']['candidate']['assertionsPassed'], 6)
        self.manifest['acceptedCaseSelectors'].append('partial')
        self.manifest['additionalPartialObservations'] = []
        with self.assertRaisesRegex(ValueError, 'skips'):
            self.select(True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    args = parser.parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=False)
    check_unity.OUTPUT = args.output_dir.resolve()
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(ExtraUnityTests))
    (args.output_dir / 'results.json').write_text(json.dumps(dict(testsRun=result.testsRun,
        failures=len(result.failures), errors=len(result.errors), success=result.wasSuccessful()), indent=2) + '\n')
    sys.exit(not result.wasSuccessful())
