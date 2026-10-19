"""Read-only controls using the completed final execution scope artifacts."""
import copy
from pathlib import Path
import unittest
from unittest.mock import patch

from run import ART,read
import selection


class ExecutionScopeTests(unittest.TestCase):
    def test_four_exact_successful_scopes_preserve_failed_diagnostic(self):
        for form in ('raw','processed'):
            for mode in ('jit','aot'):
                with self.subTest(form=form,mode=mode):
                    path=ART/'cells'/('upstream-tests-static-'+form+'-'+mode+'-upstream-final-policy-r42')/'result.json'
                    successful,failed=selection.execution_inputs(path,read(path))
                    self.assertEqual(len(successful),5)
                    self.assertEqual(len(failed),int(form=='raw' and mode=='aot'))

    def test_cell_required_entries_and_failed_history_cannot_be_changed(self):
        path=ART/'cells/upstream-tests-static-raw-aot-upstream-final-policy-r42/result.json'
        cell=read(path)
        scope_path=path.parent/'execution-qualification-scope.json'
        scope=read(scope_path)
        changes=(
            ('wrong cell identity',lambda x:x['cell'].update(sha256='f'*64)),
            ('omit ordinary Upstream',lambda x:x['successfulActualInputs'].pop()),
            ('drop failed diagnostic history',lambda x:x['failedDiagnosticInputs'].clear()),
            ('promote failed diagnostic',lambda x:x['successfulActualInputs'].extend(x['failedDiagnosticInputs'])
                or x['failedDiagnosticInputs'].clear()),
        )
        for name,change in changes:
            with self.subTest(control=name):
                modified=copy.deepcopy(scope)
                change(modified)
                def altered(p):
                    return modified if Path(p)==scope_path else read(p)
                with patch.object(selection,'read',side_effect=altered):
                    with self.assertRaises(AssertionError):
                        selection.execution_inputs(path,cell)


if __name__=='__main__':
    unittest.main()
