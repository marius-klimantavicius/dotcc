"""Pure GETPID policy controls, executable only against an explicit candidate root."""
from pathlib import Path
import copy, importlib.util, json, sys, tempfile, unittest
from types import SimpleNamespace
R=Path('/home/marius/p/dotcc');sys.path.insert(0,str(R/'Scripts'))
from campaigns.model import Unit
from campaigns.recipes import helper
ROOT=Path(sys.argv.pop(1)).resolve() if len(sys.argv)>1 and not sys.argv[1].startswith('-') else Path(__file__).resolve().parents[2]
POLICY=helper(ROOT/'scripts/pyconfig.py')

SOCKET_DEPENDENCIES = ('HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT', 'HAVE_ACCEPT',
    'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO', 'HAVE_NETDB_H',
    'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO', 'HAVE_GETHOSTBYNAME')
class GetpidPolicy(unittest.TestCase):
 def setUp(self):self.document=json.loads((ROOT/'config/pyconfig-policy.json').read_text())
 def bind(self,document=None,profile='embedded-gil',common=(),unit=(),edit=lambda s:s):
  policy=POLICY.resolve(document or self.document,profile)
  with tempfile.TemporaryDirectory() as temporary:
   header=Path(temporary)/'pyconfig.h';header.write_text(edit(POLICY.render(policy)))
   ctx=SimpleNamespace(root=ROOT,profile=profile,receipt={})
   POLICY.bind(ctx,policy,header,[Unit(Path('actual-selected.c'),unit)],common)
   return ctx.receipt['profile_feature_policy']
 def test_real_identity_is_common_but_process_features_stay_test_only(self):
  embedded=POLICY.resolve(self.document,'embedded-gil');upstream=POLICY.resolve(self.document,'upstream-tests-static')
  self.assertEqual(embedded['values']['HAVE_GETPID'],'1')
  self.assertEqual(upstream['values'],{**embedded['values'],'HAVE_FORK':'1','HAVE_WAITPID':'1','HAVE_SYS_WAIT_H':'1','HAVE_GETHOSTNAME':'1',**dict.fromkeys(SOCKET_DEPENDENCIES,'1')})
  for name in ['HAVE_FORK','HAVE_WAITPID','HAVE_SYS_WAIT_H','HAVE_WCSFTIME','HAVE_KILL']:self.assertNotIn(name,embedded['values'])
  self.assertNotIn('HAVE_WCSFTIME',upstream['values']);self.assertNotIn('HAVE_KILL',upstream['values'])
 def test_selected_getpid_cannot_be_overridden_in_any_unit_or_common_spelling(self):
  for profile in ['embedded-gil','upstream-tests-static']:
   for options in [('-UHAVE_GETPID',),('-U','HAVE_GETPID'),('-DHAVE_GETPID=1',),('-D','HAVE_GETPID=0')]:
    for where in ['common','unit']:
     with self.subTest(profile=profile,options=options,where=where),self.assertRaisesRegex(RuntimeError,'override of profile feature'):
      self.bind(profile=profile,**{where:options})
 def test_absent_getpid_still_cannot_be_selected_by_unit_override(self):
  doc=copy.deepcopy(self.document);del doc['values']['HAVE_GETPID']
  for profile in ['embedded-gil','upstream-tests-static']:
   for options in [('-DHAVE_GETPID=1',),('-D','HAVE_GETPID=1'),('-UHAVE_GETPID',),('-U','HAVE_GETPID')]:
    with self.subTest(profile=profile,options=options),self.assertRaisesRegex(RuntimeError,'override of profile feature'):
     self.bind(document=doc,profile=profile,unit=options)
 def test_defined_zero_is_not_an_absent_ifdef_service(self):
  doc=copy.deepcopy(self.document);doc['values']['HAVE_GETPID']='0'
  with self.assertRaisesRegex(RuntimeError,'must be 1'):POLICY.resolve(doc,'embedded-gil')
 def test_header_cannot_drop_or_contradict_identity_selection(self):
  for transform in [lambda s:s.replace('#define HAVE_GETPID 1\n',''),lambda s:s.replace('#define HAVE_GETPID 1','#define HAVE_GETPID 0')]:
   with self.subTest(transform=transform),self.assertRaisesRegex(RuntimeError,'header differs'):self.bind(edit=transform)
 def test_effective_binding_records_common_selected_identity(self):
  for profile in ['embedded-gil','upstream-tests-static']:
   row=self.bind(profile=profile);self.assertEqual(row['uniform_features']['HAVE_GETPID'],'1');self.assertNotIn('HAVE_GETPID',row['selected_features'])
 def test_resolve_does_not_mutate_document_or_other_profile(self):
  before=copy.deepcopy(self.document);POLICY.resolve(self.document,'upstream-tests-static')['values']['HAVE_GETPID']='99';self.assertEqual(self.document,before)
 def test_unrelated_core_roles_still_retained(self):
  row=self.bind(common=('-DUSE_COMPUTED_GOTOS=0',),unit=('-DPy_BUILD_CORE_BUILTIN',));self.assertEqual(row['units'][0]['ordered_options'],['-DUSE_COMPUTED_GOTOS=0','-DPy_BUILD_CORE_BUILTIN'])
if __name__=='__main__':unittest.main(verbosity=2)
