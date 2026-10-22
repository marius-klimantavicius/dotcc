"""Current delivery ownership is mandatory; historical hashes are optional."""
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import copy
import subprocess
import sys
from contextlib import ExitStack

import campaign_delivery as delivery


class CampaignDeliveryTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        owner = patch.object(delivery, 'ROOT', self.root)
        owner.start()
        self.addCleanup(owner.stop)

        def write(name, value):
            path = self.root / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(json.dumps(value) if not isinstance(value, str) else value)
            return path

        self.write = write
        def tool(folder, name, value):
            prefix = folder + '/' + name
            write(prefix + '.dll', value)
            write(prefix + '.pdb', 'debug')
            write(prefix + '.runtimeconfig.json', {})
            write(folder + '/cs/Resource.resources.dll', 'satellite')
            write(prefix + '.deps.json', {'targets': {'fake': {'fake': {'resources': {
                'cs/Resource.resources.dll': {'locale': 'cs'}}}}}})
            if name == 'dotcc':
                write(folder + '/DotCC.Lib.dll', 'library')
            return delivery.validate_tool_closure(self.root / (prefix + '.dll'))
        self.tool = tool
        self.tools = {'DotCC': tool('work/tools/DotCC', 'dotcc', 'compiler'),
                      'DotCC.PostProcess': tool('work/tools/DotCC.PostProcess', 'dotcc-postprocess', 'postprocessor')}
        source = write('src/Bridge.cs', 'authored')
        write('profile/managed/Bridge.cs', 'authored')
        write('profile/host-bindings.json', {'managedSources': ['src/Bridge.cs']})
        write('profile/inputs.json', {'compiler': self.tools['DotCC'],
                                     'staged_headers': {'managed/Bridge.cs': delivery.sha(source)}})
        boundary = write('boundary.json', {'profile': str(self.root / 'profile')})
        log = write('build.log', 'passed')
        assembly = write('assembly.json', {})
        outputs = {}
        for form in ('raw', 'processed'):
            project = write(form + '/Product.csproj', '<Project/>')
            write(form + '/.campaign-files.json', ['Product.csproj'])
            outputs[form] = {'project': str(project),
                             'hashes': {p.name: delivery.sha(p) for p in project.parent.iterdir()}}
        self.receipt = dict(schema_version=1, project='blink', profile='threaded', hash_policy='warn',
                            commands=[dict(label=name, status='passed', exit_code=0, stdout=str(log))
                                      for name in ('delivery-link', 'raw-build', 'postprocess', 'processed-build',
                                                   'final-raw-build', 'final-processed-build')],
                            threaded_derivation=str(boundary), outputs=outputs, staging='work',
                            tools=self.tools, assembly=str(assembly), semantic_intrinsics={}, managed_boundaries={})

    def load(self, off=False):
        if off:
            self.receipt['hash_policy'] = 'off'
            self.receipt['tools'] = {'DotCC': {}, 'DotCC.PostProcess': {}}
            for output in self.receipt['outputs'].values():
                output['hashes'] = {}
        return delivery.load_delivery(self.write('receipt.json', self.receipt))

    def test_warn_validates_recorded_delivery(self):
        self.assertEqual(self.load()['final_files'], self.receipt['outputs']['processed']['hashes'])

    def test_off_derives_current_manifests(self):
        result = self.load(off=True)
        self.assertEqual(set(result['final_files']), {'Product.csproj', '.campaign-files.json'})
        self.assertEqual(result['postprocessor'], self.tools['DotCC.PostProcess'])

    def test_warn_rejects_changed_output(self):
        self.write('processed/Product.csproj', '<Changed/>')
        with self.assertRaisesRegex(RuntimeError, 'delivered files differ'):
            self.load()

    def test_off_still_rejects_unowned_files(self):
        self.write('processed/Extra.cs', 'unexpected')
        with self.assertRaisesRegex(RuntimeError, 'ownership differs'):
            self.load(off=True)

    def test_off_still_rejects_changed_producer_compiler(self):
        self.write('work/tools/DotCC/dotcc.dll', 'changed')
        with self.assertRaisesRegex(RuntimeError, 'compiler identities differ'):
            self.load(off=True)

    def test_off_still_requires_completed_delivery(self):
        self.receipt['commands'][-1]['status'] = 'failed'
        with self.assertRaisesRegex(RuntimeError, 'delivery command did not pass'):
            self.load(off=True)

    def relink(self):
        """108 tiny fake objects; no repository producer or executable is run."""
        producer = self.tool('object-tools', 'dotcc', 'object-compiler-v22')
        inputs_path = self.root / 'profile/inputs.json'
        inputs = json.loads(inputs_path.read_text())
        inputs.update(compiler=producer, upstream_inputs={})
        rows, entries = {}, []
        for index in range(108):
            name = f'unit{index}.c'
            source = self.write('profile/' + name, f'int value{index};')
            obj = self.write('objects/' + name + '.cs', '//!dotcc object 2\n//!!dotcc-obj calling-convention:instance-v1\n')
            inputs['staged_headers'][name] = delivery.sha(source)
            rows[name] = dict(object_path=str(obj), object_sha256=delivery.sha(obj),
                              source_sha256=delivery.sha(source), emission_identity={'compiler_sha256': producer})
            entries.append(dict(path=name, staged_path=str(source), sha256=delivery.sha(source)))
        self.write('profile/inputs.json', inputs)
        profile = self.root / 'profile'
        old_command = ['dotnet', str(self.root / 'object-tools/dotcc.dll'),
                       *[r['object_path'] for r in rows.values()], '--emit=managedlib', '--instance-methods',
                       '-o', str(self.root / 'old-output')]
        original = dict(profile=str(profile), identity={'profile_inputs_sha256': delivery.sha(inputs_path),
                        'compiler_sha256': producer}, objects=rows, selected=list(rows), failures={}, linked=False,
                        link={'command': old_command, 'exit_code': 2, 'output': str(self.root / 'old-output')})
        original_path = self.write('original-assembly.json', original)
        output = self.root / 'relink-output'
        linked = self.write('relink-output/Core.cs', 'class Core {}')
        handoff = self.write('a32-handoff.json', {'sealed-capture': 'fake'})
        reference = lambda p: {'path': str(p), 'sha256': delivery.sha(p)}
        command = list(old_command)
        command[1] = str(self.root / 'work/tools/DotCC/dotcc.dll'); command[-1] = str(output)
        assembly = copy.deepcopy(original)
        assembly.update(linked=True, link={'command': command, 'exit_code': 0, 'output': str(output)},
                        relink_handoff=reference(handoff))
        assembly_path = self.write('assembly.json', assembly)
        manifest = dict(format='blink-validation-only-relink-v1', handoff=reference(handoff),
                        original_assembly=reference(original_path), profile_inputs=reference(inputs_path),
                        object_tool_directory=str(self.root / 'object-tools'), assembly=reference(assembly_path),
                        link_outputs={'Core.cs': delivery.sha(linked)})
        manifest_path = self.write('relink.json', manifest)
        self.receipt.update(hash_policy='strict', object_relink=reference(manifest_path))
        self.receipt['commands'].append(dict(label='assemble-relink', status='passed', exit_code=0,
                                            command=command, stdout=str(self.root / 'build.log')))
        from campaigns.recipes import link_flags
        self.receipt['commands'][0]['command'] = ['dotnet', str(self.root / 'work/tools/DotCC/dotcc.dll'),
            *[r['object_path'] for r in rows.values()], '--emit=managedlib', *link_flags('BlinkCore', 'Managed.Emulation'),
            '--deduplicate-inline', '--instance-methods', '-o', str(self.root / 'work/linked')]
        evidence = {name: reference(self.write(name + '.json', {name: 'fake evidence'}))
                    for name in ('validation_only_source', 'object_format_identity', 'runtime_identity')}
        approval = dict(path=str(handoff), object_producer=delivery.manifest_sha(producer),
                        linker=delivery.manifest_sha(self.tools['DotCC']),
                        postprocessor=delivery.manifest_sha(self.tools['DotCC.PostProcess']),
                        original_assembly=reference(original_path), profile_inputs=reference(inputs_path), evidence=evidence)
        for name, value in [('APPROVED_RELINK_HANDOFFS', {delivery.sha(handoff): approval})]:
            guard = patch.object(delivery, name, value); guard.start(); self.addCleanup(guard.stop)
        self.write('scripts/core_inputs.py',
                   'def profile_sources(*args): return ' + repr(entries) + '\n'
                   'def upstream_identity(*args): return {}\n')
        return manifest_path, manifest, approval

    def test_relink_helper_works_without_project_script_search_path(self):
        self.relink()
        scripts = Path(delivery.__file__).resolve().parent
        with patch.object(sys, 'path', [p for p in sys.path if Path(p).resolve() != scripts]):
            with patch.dict(sys.modules):
                sys.modules.pop('core_inputs', None)
                self.assertTrue(self.load()['passed'])

    def test_same_compiler_full_nested_closure_is_bound(self):
        self.assertIn('cs/Resource.resources.dll', self.load()['compiler'])
        self.write('work/tools/DotCC/cs/Resource.resources.dll', 'changed')
        with self.assertRaisesRegex(RuntimeError, 'Current tools differ'):
            self.load()

    def test_bounded_relink_exposes_honest_distinct_roles(self):
        self.relink()
        result = self.load()
        self.assertEqual(result['compiler'], result['object_producer']['compiler'])
        self.assertNotEqual(result['compiler'], result['linker']['compiler'])
        self.assertEqual(result['linker']['compiler'], self.receipt['tools']['DotCC'])
        self.assertEqual(result['tool_directories']['compiler'], str(self.root / 'object-tools'))

    def test_different_compiler_without_manifest_rejected(self):
        self.relink(); del self.receipt['object_relink']
        with self.assertRaisesRegex(RuntimeError, 'compiler identities differ'):
            self.load()

    def test_no_root_accepted_handoff_rejected(self):
        self.relink()
        with patch.object(delivery, 'APPROVED_RELINK_HANDOFFS', {}):
            with self.assertRaisesRegex(RuntimeError, 'root-accepted sealed'):
                self.load()

    def test_unapproved_handoff_hash_rejected(self):
        manifest_path, manifest, _ = self.relink()
        handoff = self.write('a32-handoff.json', {'sealed-capture': 'unapproved'})
        manifest['handoff']['sha256'] = delivery.sha(handoff)
        self.write('relink.json', manifest)
        self.receipt['object_relink']['sha256'] = delivery.sha(manifest_path)
        with self.assertRaisesRegex(RuntimeError, 'root-accepted sealed'):
            self.load()

    def test_approved_handoff_at_unapproved_path_rejected(self):
        manifest_path, manifest, _ = self.relink()
        handoff = self.write('caller-handoff.json', Path(manifest['handoff']['path']).read_text())
        manifest['handoff']['path'] = str(handoff)
        self.write('relink.json', manifest)
        self.receipt['object_relink']['sha256'] = delivery.sha(manifest_path)
        with self.assertRaisesRegex(RuntimeError, 'root-accepted sealed'):
            self.load()

    def test_relink_requires_strict_hash_policy(self):
        for policy in ('warn', 'off'):
            with self.subTest(policy=policy):
                self.relink()
                self.receipt['hash_policy'] = policy
                with self.assertRaisesRegex(RuntimeError, 'strict owning delivery receipt'):
                    self.load()

    def test_changed_sealed_evidence_rejected(self):
        self.relink(); self.write('runtime_identity.json', {'changed': True})
        with self.assertRaisesRegex(RuntimeError, 'evidence changed'):
            self.load()

    def test_changed_source_profile_object_or_link_output_rejected(self):
        for name in ('profile/managed/Bridge.cs', 'profile/unit0.c', 'profile/inputs.json', 'objects/unit0.c.cs',
                     'relink-output/Core.cs', 'assembly.json', 'a32-handoff.json'):
            with self.subTest(name=name):
                self.relink()
                path = self.root / name; before = path.read_bytes()
                if path.suffix == '.json':
                    changed = json.loads(before); changed['_tampered'] = True
                    path.write_text(json.dumps(changed))
                else:
                    path.write_bytes(before + b'changed')
                try:
                    with self.assertRaises(RuntimeError):
                        self.load()
                finally:
                    path.write_bytes(before)

    def test_missing_nested_tool_rejected(self):
        self.relink(); (self.root / 'work/tools/DotCC/cs/Resource.resources.dll').unlink()
        with self.assertRaisesRegex(RuntimeError, 'dependency is missing'):
            self.load()

    def test_swapped_roles_or_unapproved_closure_rejected(self):
        manifest_path, manifest, _ = self.relink()
        manifest['object_tool_directory'] = str(self.root / 'work/tools/DotCC')
        self.write('relink.json', manifest)
        self.receipt['object_relink']['sha256'] = delivery.sha(manifest_path)
        with self.assertRaisesRegex(RuntimeError, 'approved recursive tool identity'):
            self.load()

    def test_changed_actual_link_command_rejected(self):
        self.relink(); self.receipt['commands'][-1]['command'] = ['not-the-linker']
        with self.assertRaisesRegex(RuntimeError, 'actual successful command'):
            self.load()


if __name__ == '__main__':
    launches = []
    def forbid(*args, **kwargs):
        launches.append((args, kwargs))
        raise AssertionError('Pure delivery controls cannot start a process')
    with ExitStack() as stack:
        for name in ('Popen', 'run', 'call', 'check_call', 'check_output'):
            stack.enter_context(patch.object(subprocess, name, forbid))
        result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(CampaignDeliveryTests))
    assert launches == [], launches
    print(json.dumps({'actual_process_spawns': 0, 'tests': result.testsRun, 'passed': result.wasSuccessful()}))
    raise SystemExit(not result.wasSuccessful())
