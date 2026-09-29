"""Profile selection and nested receipt behavior, without compiler builds."""
import importlib.util
from pathlib import Path
from types import SimpleNamespace
import sys
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent/'Scripts'))
spec = importlib.util.spec_from_file_location('lwip_recipe', ROOT/'scripts/campaign.py')
campaign = importlib.util.module_from_spec(spec); spec.loader.exec_module(campaign)


class FakeContext:
    def __init__(self, repo, recipe, options, session=None):
        self.repo,self.recipe,self.options,self.session=repo,recipe,options,session
        self.profile=options.profile; self.artifacts=Path('/receipts')/self.profile
        self.receipt={}; self.env={}; self.sources={}
    def start(self): self.receipt['status']='running'
    def finish(self,error=None): self.receipt['status']='failed' if error else 'passed'
    def save(self): pass


class RecipeTests(unittest.TestCase):
    def context(self,action='verify',profile='threaded'):
        return FakeContext(ROOT.parent,campaign.recipe,SimpleNamespace(action=action,profile=profile,form='raw'),set())

    def test_verify_dispatch_records_every_profile(self):
        ctx=self.context(); suite=next(x for x in campaign.recipe.suites if x.name=='consumer')
        with patch('campaigns.runner.Context',FakeContext), patch.object(campaign,'run_one') as run:
            campaign.run_suite(ctx,suite)
        self.assertEqual([call.args[0].profile for call in run.call_args_list],['threaded','nosys','threaded-locking','threaded-enhanced'])
        self.assertTrue(all(x['status']=='passed' for x in ctx.receipt['profile_suites']))

    def test_failure_preserves_child_receipt_then_propagates(self):
        ctx=self.context();suite=next(x for x in campaign.recipe.suites if x.name=='upstream')
        with patch('campaigns.runner.Context',FakeContext), patch.object(campaign,'run_one',side_effect=RuntimeError('witness')):
            with self.assertRaisesRegex(RuntimeError,'witness'):campaign.run_suite(ctx,suite)
        self.assertEqual(ctx.receipt['profile_suites'],[{'suite':'upstream','profile':'unit','receipt':'/receipts/unit/receipt.json','status':'failed'}])

    def test_full_verify_prepares_generator_before_http_content(self):
        from campaigns.cli import preparation_profiles
        profiles=preparation_profiles(campaign.recipe,campaign.recipe.suites,'verify','threaded')
        self.assertLess(profiles.index('makefsdata'),profiles.index('http-content'))
        self.assertEqual(len(profiles),len(set(profiles)))
        suite=next(x for x in campaign.recipe.suites if x.name=='http-transactions')
        self.assertEqual(preparation_profiles(campaign.recipe,[suite],'test','nosys'),())

    def test_targeted_raw_runs_once_without_alternate_artifacts(self):
        ctx=self.context('test');suite=next(x for x in campaign.recipe.suites if x.name=='consumer')
        with patch('campaigns.runner.Context',side_effect=AssertionError('unexpected child')),patch.object(campaign,'run_one') as run:
            campaign.run_suite(ctx,suite)
        run.assert_called_once_with(ctx,suite)
        suite=next(x for x in campaign.recipe.suites if x.name=='upstream')
        with self.assertRaisesRegex(ValueError,'does not apply'):campaign.run_suite(ctx,suite)


if __name__=='__main__':unittest.main()
