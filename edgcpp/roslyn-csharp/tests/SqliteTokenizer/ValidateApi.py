#!/usr/bin/env python3
"""Validate FTS5 selected public layouts, function fields and actual CLI names."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA='f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185'


def record(path):
    path=Path(path)
    with path.open('rb') as stream:sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    for n in ('generated-project','base-api-report','aliases','report'):p.add_argument('--'+n,type=Path,required=True)
    p.add_argument('--verify-only',action='store_true');a=p.parse_args();base=json.loads(a.base_api_report.read_text())
    if base.get('sourceMode')!='production' or base.get('sourcePublished') is not True or base.get('completionValidated') is not True:
        raise RuntimeError('Requires validated genuine CLI source publication.')
    validator=Path(base['validator']['path'])
    if record(validator)!=base['validator']:raise RuntimeError('Frozen production validator changed.')
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
        if t['kind']!='tk_pointer' or t['size']!='8' or t['alignment']!='8':raise RuntimeError('Unaccepted pointer ABI.')
        return raw(re.fullmatch(r'pointee=([^;]+);reference=False;rvalue-reference=False',t['description'])[1])
    records={};aliases={};layouts=[];field_types={}
    expected_layouts=[('CapturedFts5Api','fts5_api',48,[(0,'iVersion','int'),(8,'xCreateTokenizer','nint'),(16,'xFindTokenizer','nint'),(24,'xCreateFunction','nint'),(32,'xCreateTokenizer_v2','nint'),(40,'xFindTokenizer_v2','nint')]),
        ('CapturedTokenizer','fts5_tokenizer',24,[(0,'xCreate','nint'),(8,'xDelete','nint'),(16,'xTokenize','nint')])]
    for alias,name,size,fields in expected_layouts:
        candidates=[t for t in inventory['types'] if t['name']==name and t['kind']=='tk_struct' and not t['incomplete']]
        if len(candidates)!=1:raise RuntimeError('Ambiguous retained FTS5 record: '+name)
        t=candidates[0]
        if t['size']!=str(size) or t['alignment']!='8' or [(int(f['offset']),f['name']) for f in t['members']]!=[(off,n) for off,n,_ in fields]:
            raise RuntimeError('Unaccepted retained FTS5 layout: '+name)
        expected=''.join(r'    \[FieldOffset\('+str(off)+r'\)\] public '+kind+' '+n+r';\n' for off,n,kind in fields)
        match=list(re.finditer(r'^\[StructLayout\(LayoutKind.Explicit, Size = '+str(size)+r', Pack = 8\)\]\npublic unsafe struct (@?\w+)\n\{\n'+expected+r'\}',text,re.M))
        if len(match)!=1:raise RuntimeError('Missing/ambiguous actual C# FTS5 record layout: '+name)
        records[t['locator']]=match[0][1];aliases[alias]=match[0][1]
        for field in t['members']:field_types[name+'.'+field['name']]=field['type']
        layouts.append({'name':name,'publicType':t['locator'],'size':size,'alignment':8,'generatedType':match[0][1],'fields':t['members']})
    db_type=pointee(pointee(functions['sqlite3_open_v2']['signature']['parameters'][1]['type']))
    records[db_type]=methods['sqlite3_open_v2'][0][1][1][:-2]
    callbacks=[]
    def routine(loc):
        t=types[raw(loc)];m=re.fullmatch(r'([^()]+)\(([^()]*)\) cc=cc_default linkage=nlk_external result-parameter=False',t['description'])
        if t['kind']!='tk_routine' or not m:raise RuntimeError('Unaccepted FTS5 callback calling convention/roles.')
        params=[]
        for part in m[2].split(',') if m[2] else []:
            q=re.fullmatch(r'(unit/\d+/type/\d+) ordinary_parameter',part)
            if not q:raise RuntimeError('Unaccepted callback execution role.')
            params.append(view(q[1]))
        return view(m[1]),params
    def view(loc):
        loc=raw(loc);t=types[loc];kind=t['kind']
        if loc in records:return records[loc]
        if kind=='tk_void':return 'void'
        if kind=='tk_pointer':
            target=pointee(loc);child=types[target]
            if child['kind']=='tk_routine':routine(target);return 'nint'
            if child['kind']=='tk_struct' and child['incomplete'] and child['name'] in ('sqlite3_stmt','Fts5Tokenizer'):return 'void*'
            return view(target)+'*'
        if kind=='tk_integer':
            m=re.search(r';bits=(\d+);signed=(True|False);',t['description']);names={('8','True'):'sbyte',('8','False'):'byte',('32','True'):'int',('64','True'):'long'}
            if m and (m[1],m[2]) in names:return names[m[1],m[2]]
        if kind=='tk_float' and ';bits=64;' in t['description'] and t['size']=='8':return 'double'
        raise RuntimeError('Unaccepted selected FTS5 scalar/pointer type: '+loc)
    apis=[]
    for name in sorted({'sqlite3_bind_pointer'}|{item['name'] for item in base['apis']}):
        s=functions[name]['signature']
        if (s['variadic'] or not s['hasPrototype'] or s['returnsValueAsParameter'] or s['returnsValueViaCopyConstructor']
            or s['callingConvention'] not in ('cc_default','cc_cdecl') or any(p['role']!='ordinary_parameter' for p in s['parameters'])):
            raise RuntimeError('Unaccepted API execution roles: '+name)
        expected=(view(s['returnType']),[view(p['type']) for p in s['parameters']])
        if len(methods.get(name,[]))!=1 or methods[name][0]!=expected:raise RuntimeError('Actual/public API mismatch: '+name)
        apis.append({'name':name,'publicSignature':s,'generatedReturn':expected[0],'generatedParameters':expected[1]})
    expected_callbacks=[('fts5_tokenizer.xCreate',('int',['void*','sbyte**','int','void**'])),('fts5_tokenizer.xDelete',('void',['void*'])),
        ('fts5_tokenizer.xTokenize',('int',['void*','void*','int','sbyte*','int','nint'])),
        ('fts5_api.xCreateTokenizer',('int',[aliases['CapturedFts5Api']+'*','sbyte*','void*',aliases['CapturedTokenizer']+'*','nint']))]
    for name,expected in expected_callbacks:
        shape=routine(pointee(field_types[name]))
        if shape!=expected:raise RuntimeError('Exact FTS5 callback shape differs: '+name)
        callbacks.append({'field':name,'returnType':shape[0],'parameters':shape[1],'convention':'Cdecl'})
    def parameters_of(loc):
        return [m[1] for m in re.finditer(r'(unit/\d+/type/\d+) ordinary_parameter',types[pointee(loc)]['description'])]
    nested=[('engine.xToken',parameters_of(field_types['fts5_tokenizer.xTokenize'])[5],('int',['void*','int','sbyte*','int','int','int'])),
        ('registration.xDestroy',parameters_of(field_types['fts5_api.xCreateTokenizer'])[4],('void',['void*']))]
    for name,loc,expected in nested:
        shape=routine(pointee(loc))
        if shape!=expected:raise RuntimeError('Nested callback shape differs: '+name)
        callbacks.append({'field':name,'returnType':shape[0],'parameters':shape[1],'convention':'Cdecl'})
    base_aliases=Path(base['aliases']['path']).read_text();namespace=re.search(r'global using Api = global::(.+)\.[^.]+;',base_aliases)[1]
    alias_text=base_aliases+''.join(f'global using {alias} = global::{namespace}.{display};\n' for alias,display in aliases.items())
    checks={'baseApiReport':record(a.base_api_report),'acceptedInventory':record(ip),'generatedSource':record(source),'validator':record(__file__),'apis':apis,'layouts':layouts,'callbacks':callbacks}
    if a.verify_only:
        previous=json.loads(a.report.read_text())
        if a.aliases.read_text()!=alias_text or any(previous[k]!=v for k,v in checks.items()):raise RuntimeError('Tokenizer API evidence changed.')
        print('tokenizer-surface=verified');return 0
    if a.aliases.exists() or a.report.exists():raise RuntimeError('Choose fresh aliases/report files.')
    a.aliases.parent.mkdir(parents=True,exist_ok=True);a.aliases.write_text(alias_text)
    a.report.parent.mkdir(parents=True,exist_ok=True);a.report.write_text(json.dumps({'status':'validated-published-tokenizer-surface','sourcePublished':True,
        'completionValidated':True,'executionAccepted':False,**checks,'aliases':record(a.aliases)},indent=2)+'\n')
    print(json.dumps({'status':'validated-published-tokenizer-surface','apis':len(apis),'callbacks':len(callbacks),'layouts':len(layouts)}));return 0


if __name__=='__main__':raise SystemExit(main())
