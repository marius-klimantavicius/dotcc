import argparse,copy,json,sys,unittest
from pathlib import Path
sys.dont_write_bytecode=True
import extra_unity_verify as base
unity=base.unity

class ReplacementTests(base.ExtraUnityTests):
    def replacement(self,approved=False):
        if approved:self.approve()
        for i in (3,5):self.units[i]['target']='other'
        wrapper=self.build/'second.cpp'
        wrapper.write_text(''.join('#include '+json.dumps(str(self.source/f'source{i}.cpp'))+'\n' for i in (3,5)))
        second=dict(target='other',unitySource=unity.record(wrapper),orderedMembers=[self.units[i]['file'] for i in (3,5)])
        self.upstream.append(second)
        spec=self.manifest['groups'][0]
        spec.update(target='coalesced',orderedSources=[f'source{i}.cpp' for i in range(2,6)],
            sourceTargets={f'source{i}.cpp':self.units[i]['target'] for i in range(2,6)},
            replacesUpstreamGroups=[dict(target=g['target'],unitySource=g['unitySource'],
                orderedSources=[str(Path(s).relative_to(self.stage)) for s in g['orderedMembers']]) for g in self.upstream])
        if approved:
            p=self.comparison['profiles']['extra']
            p['units']=[dict(self.units[i],file=str(self.source/f'source{i}.cpp'),arguments=[v.replace(str(self.stage),str(self.source)) for v in self.units[i]['arguments']]) for i in range(2,6)]
            p['memberInputs']=[unity.record(self.source/f'source{i}.cpp') for i in range(2,6)]
            w=Path(p['wrapper']['path']);w.write_text(''.join('#include '+json.dumps(x['path'])+'\n' for x in p['memberInputs']));p['wrapper']=unity.record(w)
            spec['effectiveSourceHashes']={f'source{i}.cpp':unity.sha(self.stage/f'source{i}.cpp') for i in range(2,6)}
            self.save_comparison()

    def test_complete_replacement_preserves_coverage(self):
        self.replacement(True);result=self.select(True)
        self.assertEqual(len(result['replacedUpstreamGroups']),2)
        selected=unity.apply_extra_groups(self.upstream,result)
        self.assertEqual(len(selected),1)
        selected[0]['coveredSourceIndices']=[2,3,4,5]
        inputs=unity.compose_inputs(self.base['programInputs'],selected,[dict(path='replacement')],6)
        self.assertEqual(len(inputs),2);self.assertEqual(inputs[0],self.base['programInputs'][0])

    def test_partial_replacement_rejected(self):
        self.replacement();self.manifest['groups'][0]['orderedSources'].remove('source4.cpp')
        with self.assertRaisesRegex(ValueError,'complete upstream'):self.select()

    def test_replacement_order_target_and_receipt_rejected(self):
        self.replacement();original=copy.deepcopy(self.manifest)
        for mutation in ('order','target','hash','unknown','duplicate'):
            self.manifest=copy.deepcopy(original);s=self.manifest['groups'][0]
            if mutation=='order':s['orderedSources'].reverse()
            elif mutation=='target':s['sourceTargets']['source3.cpp']='library'
            elif mutation=='hash':s['replacesUpstreamGroups'][0]['unitySource']['sha256']='changed'
            elif mutation=='unknown':s['replacesUpstreamGroups'][0]['unitySource']['path']='/unknown'
            else:s['replacesUpstreamGroups'].append(copy.deepcopy(s['replacesUpstreamGroups'][0]))
            with self.subTest(mutation=mutation),self.assertRaises(ValueError):self.select()

    def test_replacement_native_target_and_flags_rejected(self):
        self.replacement(True);original=copy.deepcopy(self.comparison)
        for mutation in ('target','flags'):
            self.comparison=copy.deepcopy(original);u=self.comparison['profiles']['extra']['units'][1]
            if mutation=='target':u['target']='library'
            else:u['arguments'].append('-DCHANGED=1')
            self.save_comparison()
            with self.subTest(mutation=mutation),self.assertRaisesRegex(ValueError,'member options'):self.select(True)

    def test_replacement_cannot_replace_prior_experimental_group(self):
        self.replacement();self.upstream[0]['additionalGroupId']='previous'
        with self.assertRaisesRegex(ValueError,'Unknown'):self.select()

    def test_mapping_without_explicit_replacement_rejected(self):
        self.manifest['groups'][0]['sourceTargets']={'source3.cpp':'library','source5.cpp':'library'}
        with self.assertRaisesRegex(ValueError,'requires explicit'):self.select()

    def test_second_replacement_of_group_rejected(self):
        self.replacement();duplicate=copy.deepcopy(self.manifest['groups'][0]);duplicate['id']='again';self.manifest['groups'].append(duplicate)
        with self.assertRaisesRegex(ValueError,'more than once'):self.select()

    def test_native_artifact_comments_only_without_line_splicing(self):
        self.replacement(True);profile=self.comparison['profiles']['extra'];path=Path(profile['wrapper']['path']);includes=path.read_text()
        path.write_text('// Independently audited native probe.\n'+includes)
        profile['wrapper']=unity.record(path);self.save_comparison();self.select(True)
        with self.assertRaisesRegex(ValueError,'syntax'):unity.upstream_members(path)
        for comment in ('// splice '+chr(92), '// trigraph ??/'):
            path.write_text(comment+'\n'+includes);profile['wrapper']=unity.record(path);self.save_comparison()
            with self.subTest(comment=comment),self.assertRaisesRegex(ValueError,'syntax'):self.select(True)

    def test_native_comments_use_only_physical_newlines(self):
        path=self.root/'physical-lines.cpp'
        for separator in ('\v','\f','\u2028','\u2029','\x85'):
            path.write_text('// comment'+separator+'#include "/definitely/not/existing.hpp"\n')
            with self.subTest(separator=repr(separator)),self.assertRaisesRegex(ValueError,'Empty'):
                unity.upstream_members(path,allow_artifact_comments=True)
        path.write_bytes(b'// artifact comment\r\n#include "/actual.hpp"\r\n')
        self.assertEqual(unity.upstream_members(path,allow_artifact_comments=True),['/actual.hpp'])

    def test_two_prior_receipts_bind_latest_cumulative_archives(self):
        def record(path):return dict(path=path,sha256='synthetic')
        def archive(name):return dict(original=record('/old/'+name+'.a'),candidate=record('/new/'+name+'.a'),afterMembers=[name+'.o'])
        first=dict(receipt=record('/first.json'),comparison=dict(baseline=record('/base'),archives={'first':archive('first')}))
        second=dict(receipt=record('/second.json'),comparison=dict(baseline=record('/base'),archives={'first':archive('first'),'second':archive('second')}))
        required=[first['receipt'],second['receipt']]
        manifest=dict(composedWithPriorGroups=required,groups=[dict(id='third')])
        comparison=dict(priorGroupAcceptances=copy.deepcopy(required),baseline=record('/base'),
            archives={**second['comparison']['archives'],'third':archive('third')},profiles={'third':{}},
            candidateRunner=record('/new/runner'),commands=[dict(cwd='/',argv=['c++','/new/first.a','/new/second.a','/new/third.a','-o','/new/runner'])])
        unity.validate_prior_groups(manifest,comparison,[first,second])
        for mutation in ('receipts','archive','link'):
            changed=copy.deepcopy(comparison)
            if mutation=='receipts':changed['priorGroupAcceptances'].reverse()
            elif mutation=='archive':del changed['archives']['second']
            else:changed['commands'][0]['argv'].remove('/new/second.a')
            with self.subTest(mutation=mutation),self.assertRaises(ValueError):unity.validate_prior_groups(manifest,changed,[first,second])

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output-dir',type=Path,required=True);a=parser.parse_args();a.output_dir.mkdir(parents=True,exist_ok=False);base.check_unity.OUTPUT=a.output_dir.resolve()
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(ReplacementTests))
    (a.output_dir/'results.json').write_text(json.dumps(dict(testsRun=result.testsRun,failures=len(result.failures),errors=len(result.errors),success=result.wasSuccessful()),indent=2)+'\n');sys.exit(not result.wasSuccessful())
