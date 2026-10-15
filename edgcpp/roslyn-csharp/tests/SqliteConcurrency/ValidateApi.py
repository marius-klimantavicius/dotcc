#!/usr/bin/env python3
"""Validate the finite concurrency APIs using accepted public report and CLI source."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA='f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185'
EXTRA=['sqlite3_busy_handler','sqlite3_unlock_notify','sqlite3_errcode','sqlite3_extended_errcode','sqlite3_extended_result_codes','sqlite3_threadsafe']


def record(path):
    path=Path(path)
    with path.open('rb') as stream:sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    for n in ('generated-project','base-api-report','aliases','report'):p.add_argument('--'+n,type=Path,required=True)
    p.add_argument('--verify-only',action='store_true');a=p.parse_args();base=json.loads(a.base_api_report.read_text())
    if base.get('sourceMode')!='production' or base.get('sourcePublished') is not True or base.get('completionValidated') is not True:
        raise RuntimeError('Requires genuine validated CLI publication.')
    validator=Path(base['validator']['path'])
    if record(validator)!=base['validator']:raise RuntimeError('Frozen baseline validator changed.')
    subprocess.run([sys.executable,str(validator),'--generated-project',str(a.generated_project.resolve()),
        '--candidate-receipt',base['productionEvidence']['completion']['path'],'--inventory',base['acceptedInventory']['path'],
        '--provider-plan',base['providerPlan']['path'],'--aliases',base['aliases']['path'],'--report',str(a.base_api_report.resolve()),'--verify-only'],check=True)
    ip=Path(base['acceptedInventory']['path'])
    if record(ip)['sha256']!=INVENTORY_SHA:raise RuntimeError('Accepted public inventory changed.')
    inventory=json.loads(ip.read_text());types={t['locator']:t for t in inventory['types']}
    functions={f['currentName']:f for f in inventory['functions'] if f['requiredBody']}
    source=a.generated_project.resolve().with_name('Generated.cs');text=source.read_text();methods={}
    for m in re.finditer(r'^    public static ([\w@*.]+) (@?\w+)\(([^\n]*)\)\s*$',text,re.M):
        methods.setdefault(m[2].lstrip('@'),[]).append((m[1],[p.rsplit(' ',1)[0] for p in m[3].split(', ')] if m[3] else []))
    def raw(loc):
        active=set()
        while types[loc]['kind']=='tk_typeref':
            if loc in active:raise RuntimeError('Type-reference cycle.')
            active.add(loc);loc=re.match(r'underlying=([^;]+)',types[loc]['description'])[1]
        return loc
    def pointee(loc):
        t=types[raw(loc)]
        if t['kind']!='tk_pointer' or t['size']!='8' or t['alignment']!='8':raise RuntimeError('Unaccepted retained pointer ABI.')
        return raw(re.fullmatch(r'pointee=([^;]+);reference=False;rvalue-reference=False',t['description'])[1])
    actual=methods['sqlite3_open_v2']
    if len(actual)!=1 or not re.fullmatch(r'@?\w+\*\*',actual[0][1][1]):raise RuntimeError('Expected actual concrete database pointer.')
    display=actual[0][1][1][:-2];db_type=pointee(pointee(functions['sqlite3_open_v2']['signature']['parameters'][1]['type']))
    t=types[db_type]
    if t['kind']!='tk_struct' or t['incomplete'] or t['alignment']!='8' or not re.search(r'^public unsafe struct '+re.escape(display)+r'\s*$',text,re.M):
        raise RuntimeError('Selected/actual database handle mismatch.')
    def routine(loc):
        t=types[raw(loc)];m=re.fullmatch(r'([^()]+)\(([^()]*)\) cc=cc_default linkage=nlk_external result-parameter=False',t['description'])
        if t['kind']!='tk_routine' or not m:raise RuntimeError('Unaccepted callback calling convention/roles.')
        parameters=[]
        for part in m[2].split(',') if m[2] else []:
            q=re.fullmatch(r'(unit/\d+/type/\d+) ordinary_parameter',part)
            if not q:raise RuntimeError('Unaccepted callback parameter role.')
            parameters.append(view(q[1]))
        return view(m[1]),parameters
    def view(loc):
        loc=raw(loc);t=types[loc];kind=t['kind']
        if loc==db_type:return display
        if kind=='tk_void':return 'void'
        if kind=='tk_pointer':
            target=pointee(loc);child=types[target]
            if child['kind']=='tk_routine':routine(target);return 'nint'
            if child['kind']=='tk_struct' and child['incomplete'] and child['name']=='sqlite3_stmt':return 'void*'
            return view(target)+'*'
        if kind=='tk_integer':
            m=re.search(r';bits=(\d+);signed=(True|False);',t['description']);names={('8','True'):'sbyte',('8','False'):'byte',('32','True'):'int',('64','True'):'long'}
            if m and (m[1],m[2]) in names:return names[m[1],m[2]]
        if kind=='tk_float' and ';bits=64;' in t['description'] and t['size']=='8':return 'double'
        raise RuntimeError('Unaccepted public scalar/pointer API type: '+loc)
    apis=[]
    for name in sorted(set(EXTRA)|{item['name'] for item in base['apis']}):
        s=functions[name]['signature']
        if (s['variadic'] or not s['hasPrototype'] or s['returnsValueAsParameter'] or s['returnsValueViaCopyConstructor']
            or s['callingConvention'] not in ('cc_default','cc_cdecl') or any(p['role']!='ordinary_parameter' for p in s['parameters'])):
            raise RuntimeError('Unaccepted ordinary API execution roles: '+name)
        expected=(view(s['returnType']),[view(p['type']) for p in s['parameters']])
        if len(methods.get(name,[]))!=1 or methods[name][0]!=expected:raise RuntimeError('Actual/public API mismatch: '+name)
        apis.append({'name':name,'publicSignature':s,'generatedReturn':expected[0],'generatedParameters':expected[1]})
    callbacks=[]
    for name,expected in [('sqlite3_busy_handler',('int',['void*','int'])),('sqlite3_unlock_notify',('void',['void**','int']))]:
        shape=routine(pointee(functions[name]['signature']['parameters'][1]['type']))
        if shape!=expected:raise RuntimeError('Actual callback differs from finite exact Cdecl shape: '+name)
        callbacks.append({'api':name,'returnType':shape[0],'parameters':shape[1],'convention':'Cdecl'})
    aliases=Path(base['aliases']['path']).read_text()
    evidence={'baseApiReport':record(a.base_api_report),'acceptedInventory':record(ip),'generatedSource':record(source),'validator':record(__file__),'apis':apis,'callbacks':callbacks}
    if a.verify_only:
        previous=json.loads(a.report.read_text())
        if a.aliases.read_text()!=aliases or any(previous[k]!=v for k,v in evidence.items()):raise RuntimeError('Concurrency API evidence changed.')
        print('concurrency-surface=verified');return 0
    if a.aliases.exists() or a.report.exists():raise RuntimeError('Choose fresh aliases/report files.')
    a.aliases.parent.mkdir(parents=True,exist_ok=True);a.aliases.write_text(aliases)
    a.report.parent.mkdir(parents=True,exist_ok=True);a.report.write_text(json.dumps({'status':'validated-published-concurrency-surface',
        'sourcePublished':True,'completionValidated':True,'executionAccepted':False,**evidence,'aliases':record(a.aliases)},indent=2)+'\n')
    print(json.dumps({'status':'validated-published-concurrency-surface','apis':len(apis),'callbacks':len(callbacks)}));return 0


if __name__=='__main__':raise SystemExit(main())
