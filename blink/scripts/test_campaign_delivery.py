"""Current delivery ownership is mandatory; historical hashes are optional."""
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

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
        compiler = write('work/tools/DotCC/compiler.dll', 'compiler')
        postprocessor = write('work/tools/DotCC.PostProcess/postprocessor.dll', 'postprocessor')
        self.tools = {'DotCC': {'compiler.dll': delivery.sha(compiler)},
                      'DotCC.PostProcess': {'postprocessor.dll': delivery.sha(postprocessor)}}
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
        self.write('work/tools/DotCC/compiler.dll', 'changed')
        with self.assertRaisesRegex(RuntimeError, 'compiler identities differ'):
            self.load(off=True)

    def test_off_still_requires_completed_delivery(self):
        self.receipt['commands'][-1]['status'] = 'failed'
        with self.assertRaisesRegex(RuntimeError, 'delivery command did not pass'):
            self.load(off=True)


if __name__ == '__main__':
    unittest.main()
