#!/usr/bin/env python3
"""Validate this finite API slice against genuine CLI source and public T0 facts."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA="f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185"
EXTRA=["sqlite3_preupdate_hook","sqlite3_preupdate_count","sqlite3_preupdate_depth","sqlite3_preupdate_old","sqlite3_preupdate_new",
    "sqlite3_value_type","sqlite3_value_int64","sqlite3_value_double","sqlite3_value_text","sqlite3_value_blob","sqlite3_value_bytes",
    "sqlite3session_create","sqlite3session_attach","sqlite3session_changeset","sqlite3session_delete","sqlite3_free",
    "sqlite3changeset_start","sqlite3changeset_next","sqlite3changeset_op","sqlite3changeset_old","sqlite3changeset_new","sqlite3changeset_finalize","sqlite3changeset_apply",
    "sqlite3_backup_init","sqlite3_backup_step","sqlite3_backup_remaining","sqlite3_backup_pagecount","sqlite3_backup_finish"]


def record(path):
    path=Path(path)
    with path.open('rb') as stream:sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('generated-project','base-api-report','aliases','report'):parser.add_argument('--'+name,type=Path,required=True)
    parser.add_argument('--verify-only',action='store_true');args=parser.parse_args()
    base=json.loads(args.base_api_report.read_text())
    if base.get('sourceMode')!='production' or base.get('sourcePublished') is not True or base.get('completionValidated') is not True:
        raise RuntimeError('This API slice requires actual validated CLI source publication evidence.')
    validator=Path(base['validator']['path'])
    if record(validator)!=base['validator']:raise RuntimeError('Baseline production validator changed.')
    subprocess.run([sys.executable,str(validator),'--generated-project',str(args.generated_project.resolve()),
        '--candidate-receipt',base['productionEvidence']['completion']['path'],'--inventory',base['acceptedInventory']['path'],
        '--provider-plan',base['providerPlan']['path'],'--aliases',base['aliases']['path'],'--report',str(args.base_api_report.resolve()),'--verify-only'],check=True)
    inventory_path=Path(base['acceptedInventory']['path'])
    if record(inventory_path)['sha256']!=INVENTORY_SHA:raise RuntimeError('Accepted public inventory changed.')
    inventory=json.loads(inventory_path.read_text());types={t['locator']:t for t in inventory['types']}
    functions={f['currentName']:f for f in inventory['functions'] if f['requiredBody']}
    source=args.generated_project.resolve().with_name('Generated.cs');text=source.read_text()
    methods={}
    for m in re.finditer(r'^    public static ([\w@*.]+) (@?\w+)\(([^\n]*)\)\s*$',text,re.M):
        methods.setdefault(m[2].lstrip('@'),[]).append((m[1],[p.rsplit(' ',1)[0] for p in m[3].split(', ')] if m[3] else []))
    def raw(locator):
        active=set()
        while types[locator]['kind']=='tk_typeref':
            if locator in active:raise RuntimeError('Type-reference cycle.')
            active.add(locator);locator=re.match(r'underlying=([^;]+)',types[locator]['description'])[1]
        return locator
    def peel(locator,count):
        for _ in range(count):
            t=types[raw(locator)]
            if t['kind']!='tk_pointer' or t['size']!='8' or t['alignment']!='8':raise RuntimeError('Unexpected retained pointer representation.')
            locator=re.match(r'pointee=([^;]+);reference=False;rvalue-reference=False',t['description'])[1]
        return raw(locator)
    derived=[('CapturedDatabase','sqlite3_open_v2',1,2),('CapturedValue','sqlite3_preupdate_old',2,2),
        ('CapturedSession','sqlite3session_create',2,2),('CapturedIterator','sqlite3changeset_start',0,2),('CapturedBackup','sqlite3_backup_init',None,1)]
    records={};aliases={}
    for alias,name,index,stars in derived:
        if len(methods.get(name,[]))!=1:raise RuntimeError('Missing/ambiguous actual API: '+name)
        actual=methods[name][0][0] if index is None else methods[name][0][1][index]
        if not re.fullmatch(r'@?\w+'+r'\*'*stars,actual):raise RuntimeError('Expected concrete pointer shape for '+name)
        display=actual[:-stars]
        if not re.search(r'^public unsafe struct '+re.escape(display)+r'\s*$',text,re.M):raise RuntimeError('No actual record declaration for '+display)
        signature=functions[name]['signature'];locator=signature['returnType'] if index is None else signature['parameters'][index]['type']
        record_id=peel(locator,stars);descriptor=types[record_id]
        if descriptor['kind']!='tk_struct' or descriptor['incomplete'] or descriptor['alignment']!='8':raise RuntimeError('Unexpected retained complete handle representation.')
        records[record_id]=display;aliases[alias]=display
    callback_shapes=[]
    def routine(locator):
        t=types[raw(locator)]
        match=re.fullmatch(r'([^()]+)\(([^()]*)\) cc=cc_default linkage=nlk_external result-parameter=False',t['description'])
        if t['kind']!='tk_routine' or not match:raise RuntimeError('Unaccepted captured callback convention/roles.')
        parameters=[]
        for p in match[2].split(',') if match[2] else []:
            m=re.fullmatch(r'(unit/\d+/type/\d+) ordinary_parameter',p)
            if not m:raise RuntimeError('Unaccepted callback execution role.')
            parameters.append(view(m[1]))
        return view(match[1]),parameters
    def view(locator):
        locator=raw(locator);t=types[locator];kind=t['kind'];description=t['description']
        if locator in records:return records[locator]
        if kind=='tk_void':return 'void'
        if kind=='tk_pointer':
            if t['size']!='8' or t['alignment']!='8':raise RuntimeError('Unaccepted pointer ABI.')
            target=peel(locator,1);pointee=types[target]
            if pointee['kind']=='tk_routine':routine(target);return 'nint'
            if pointee['kind']=='tk_struct' and pointee['incomplete'] and pointee['name']=='sqlite3_stmt':return 'void*'
            return view(target)+'*'
        if kind=='tk_integer':
            m=re.search(r';bits=(\d+);signed=(True|False);',description)
            names={('8','True'):'sbyte',('8','False'):'byte',('32','True'):'int',('64','True'):'long'}
            if m and (m[1],m[2]) in names:return names[m[1],m[2]]
        if kind=='tk_float' and ';bits=64;' in description and t['size']=='8':return 'double'
        raise RuntimeError('Unaccepted public API representation: '+locator)
    names=sorted(set(EXTRA)|{item['name'] for item in base['apis']});validated=[]
    for name in names:
        f=functions[name];s=f['signature']
        if (s['variadic'] or not s['hasPrototype'] or s['returnsValueAsParameter'] or s['returnsValueViaCopyConstructor']
            or s['callingConvention'] not in ('cc_default','cc_cdecl') or any(p['role']!='ordinary_parameter' for p in s['parameters'])):
            raise RuntimeError('Unaccepted API signature roles: '+name)
        expected=(view(s['returnType']),[view(p['type']) for p in s['parameters']])
        if len(methods.get(name,[]))!=1 or methods[name][0]!=expected:raise RuntimeError('Actual generated/public API shape mismatch: '+name)
        validated.append({'name':name,'publicSignature':s,'generatedReturn':expected[0],'generatedParameters':expected[1]})
    callbacks=[('sqlite3_bind_blob',4,('void',['void*'])),('sqlite3_preupdate_hook',1,('void',['void*',aliases['CapturedDatabase']+'*','int','sbyte*','sbyte*','long','long'])),
        ('sqlite3changeset_apply',3,('int',['void*','sbyte*'])),('sqlite3changeset_apply',4,('int',['void*','int',aliases['CapturedIterator']+'*']))]
    for name,index,expected in callbacks:
        shape=routine(peel(functions[name]['signature']['parameters'][index]['type'],1))
        if shape!=expected:raise RuntimeError('Captured callback differs from the exact test Cdecl shape: '+name)
        callback_shapes.append({'api':name,'parameter':index,'returnType':shape[0],'parameters':shape[1],'convention':'Cdecl'})
    base_aliases=Path(base['aliases']['path']).read_text();namespace=re.search(r'global using Api = global::(.+)\.[^.]+;',base_aliases)[1]
    alias_text=base_aliases+''.join(f'global using {alias} = global::{namespace}.{display};\n' for alias,display in aliases.items() if alias!='CapturedDatabase')
    checks={'baseApiReport':record(args.base_api_report),'acceptedInventory':record(inventory_path),'generatedSource':record(source),
        'validator':record(__file__),'apis':validated,'callbacks':callback_shapes}
    if args.verify_only:
        previous=json.loads(args.report.read_text())
        if args.aliases.read_text()!=alias_text or any(previous[k]!=v for k,v in checks.items()):raise RuntimeError('API checker source/evidence changed.')
        print('api-callback-surface=verified');return 0
    if args.aliases.exists() or args.report.exists():raise RuntimeError('Choose fresh aliases/report files.')
    args.aliases.parent.mkdir(parents=True,exist_ok=True);args.aliases.write_text(alias_text)
    args.report.parent.mkdir(parents=True,exist_ok=True);args.report.write_text(json.dumps({'status':'validated-published-api-callback-surface',
        'sourcePublished':True,'completionValidated':True,'executionAccepted':False,**checks,'aliases':record(args.aliases)},indent=2)+'\n')
    print(json.dumps({'status':'validated-published-api-callback-surface','apis':len(validated),'callbacks':len(callback_shapes)}));return 0


if __name__=='__main__':raise SystemExit(main())
