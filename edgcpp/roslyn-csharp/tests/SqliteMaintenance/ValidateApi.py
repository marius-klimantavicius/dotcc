#!/usr/bin/env python3
"""Check maintenance APIs against accepted public facts and actual CLI source."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

INVENTORY_SHA="f1b89135e2e37bb78fe175c49cbf0b14df9743f16a84bb7142df8291f72e7185"
EXTRA=["sqlite3_snapshot_get","sqlite3_snapshot_open","sqlite3_snapshot_recover","sqlite3_snapshot_cmp","sqlite3_snapshot_free",
    "sqlite3rbu_open","sqlite3rbu_step","sqlite3rbu_savestate","sqlite3rbu_close","sqlite3rbu_progress","sqlite3rbu_state",
    "sqlite3rbu_bp_progress","sqlite3rbu_db","sqlite3_free"]


def record(path):
    path=Path(path)
    with path.open('rb') as stream: sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('generated-project','base-api-report','aliases','report'): parser.add_argument('--'+name,type=Path,required=True)
    parser.add_argument('--verify-only',action='store_true');args=parser.parse_args()
    base=json.loads(args.base_api_report.read_text())
    if base.get('sourceMode')!='production' or base.get('sourcePublished') is not True or base.get('completionValidated') is not True:
        raise RuntimeError('Requires actual validated CLI source publication.')
    validator=Path(base['validator']['path'])
    if record(validator)!=base['validator']: raise RuntimeError('Frozen production validator changed.')
    subprocess.run([sys.executable,str(validator),'--generated-project',str(args.generated_project.resolve()),
        '--candidate-receipt',base['productionEvidence']['completion']['path'],'--inventory',base['acceptedInventory']['path'],
        '--provider-plan',base['providerPlan']['path'],'--aliases',base['aliases']['path'],'--report',str(args.base_api_report.resolve()),'--verify-only'],check=True)
    inventory_path=Path(base['acceptedInventory']['path'])
    if record(inventory_path)['sha256']!=INVENTORY_SHA: raise RuntimeError('Accepted public inventory changed.')
    inventory=json.loads(inventory_path.read_text());types={t['locator']:t for t in inventory['types']}
    functions={f['currentName']:f for f in inventory['functions'] if f['requiredBody']}
    source=args.generated_project.resolve().with_name('Generated.cs');text=source.read_text();methods={}
    for m in re.finditer(r'^    public static ([\w@*.]+) (@?\w+)\(([^\n]*)\)\s*$',text,re.M):
        methods.setdefault(m[2].lstrip('@'),[]).append((m[1],[p.rsplit(' ',1)[0] for p in m[3].split(', ')] if m[3] else []))
    def raw(locator):
        active=set()
        while types[locator]['kind']=='tk_typeref':
            if locator in active: raise RuntimeError('Type-reference cycle.')
            active.add(locator);locator=re.match(r'underlying=([^;]+)',types[locator]['description'])[1]
        return locator
    def peel(locator,count):
        for _ in range(count):
            t=types[raw(locator)]
            if t['kind']!='tk_pointer' or t['size']!='8' or t['alignment']!='8': raise RuntimeError('Unexpected retained pointer representation.')
            locator=re.fullmatch(r'pointee=([^;]+);reference=False;rvalue-reference=False',t['description'])[1]
        return raw(locator)
    records={};aliases={};layouts=[]
    for alias,name,index,stars,size,alignment in [('CapturedDatabase','sqlite3_open_v2',1,2,None,'8'),
        ('CapturedSnapshot','sqlite3_snapshot_get',2,2,'48','1'),('CapturedRbu','sqlite3rbu_open',None,1,'416','8')]:
        if len(methods.get(name,[]))!=1: raise RuntimeError('Missing/ambiguous actual API: '+name)
        actual=methods[name][0][0] if index is None else methods[name][0][1][index]
        if not re.fullmatch(r'@?\w+'+r'\*'*stars,actual): raise RuntimeError('Unexpected concrete handle pointer: '+name)
        display=actual[:-stars];signature=functions[name]['signature']
        locator=peel(signature['returnType'] if index is None else signature['parameters'][index]['type'],stars);t=types[locator]
        if t['kind']!='tk_struct' or t['incomplete'] or t['alignment']!=alignment or size is not None and t['size']!=size:
            raise RuntimeError('Unexpected public handle representation.')
        declaration=r'^\[StructLayout\(LayoutKind.Explicit, Size = '+t['size']+r', Pack = '+alignment+r'\)\]\npublic unsafe struct '+re.escape(display)+r'\s*$'
        if not re.search(declaration,text,re.M): raise RuntimeError('Actual handle declaration/layout differs from public facts: '+name)
        records[locator]=display;aliases[alias]=display;layouts.append({'alias':alias,'publicType':locator,'size':t['size'],'alignment':alignment})
    def routine(locator):
        t=types[raw(locator)];m=re.fullmatch(r'([^()]+)\(([^()]*)\) cc=cc_default linkage=nlk_external result-parameter=False',t['description'])
        if t['kind']!='tk_routine' or not m: raise RuntimeError('Unaccepted retained callback convention/roles.')
        parameters=[]
        for p in m[2].split(',') if m[2] else []:
            part=re.fullmatch(r'(unit/\d+/type/\d+) ordinary_parameter',p)
            if not part: raise RuntimeError('Unaccepted callback role.')
            parameters.append(view(part[1]))
        return view(m[1]),parameters
    def view(locator):
        locator=raw(locator);t=types[locator];kind=t['kind']
        if locator in records: return records[locator]
        if kind=='tk_void': return 'void'
        if kind=='tk_pointer':
            target=peel(locator,1);pointee=types[target]
            if pointee['kind']=='tk_routine': routine(target);return 'nint'
            if pointee['kind']=='tk_struct' and pointee['incomplete'] and pointee['name']=='sqlite3_stmt': return 'void*'
            return view(target)+'*'
        if kind=='tk_integer':
            m=re.search(r';bits=(\d+);signed=(True|False);',t['description'])
            names={('8','True'):'sbyte',('8','False'):'byte',('32','True'):'int',('64','True'):'long'}
            if m and (m[1],m[2]) in names: return names[m[1],m[2]]
        if kind=='tk_float' and ';bits=64;' in t['description'] and t['size']=='8': return 'double'
        raise RuntimeError('Unaccepted public API representation: '+locator)
    validated=[]
    for name in sorted(set(EXTRA)|{item['name'] for item in base['apis']}):
        s=functions[name]['signature']
        if (s['variadic'] or not s['hasPrototype'] or s['returnsValueAsParameter'] or s['returnsValueViaCopyConstructor']
            or s['callingConvention'] not in ('cc_default','cc_cdecl') or any(p['role']!='ordinary_parameter' for p in s['parameters'])):
            raise RuntimeError('Unaccepted retained signature: '+name)
        expected=(view(s['returnType']),[view(p['type']) for p in s['parameters']])
        if len(methods.get(name,[]))!=1 or methods[name][0]!=expected: raise RuntimeError('Actual/generated public API mismatch: '+name)
        validated.append({'name':name,'publicSignature':s,'generatedReturn':expected[0],'generatedParameters':expected[1]})
    base_aliases=Path(base['aliases']['path']).read_text();namespace=re.search(r'global using Api = global::(.+)\.[^.]+;',base_aliases)[1]
    alias_text=base_aliases+''.join(f'global using {alias} = global::{namespace}.{display};\n' for alias,display in aliases.items() if alias!='CapturedDatabase')
    checks={'baseApiReport':record(args.base_api_report),'acceptedInventory':record(inventory_path),'generatedSource':record(source),
        'validator':record(__file__),'apis':validated,'handleLayouts':layouts}
    if args.verify_only:
        previous=json.loads(args.report.read_text())
        if args.aliases.read_text()!=alias_text or any(previous[k]!=v for k,v in checks.items()): raise RuntimeError('Maintenance evidence/source changed.')
        print('maintenance-surface=verified');return 0
    if args.aliases.exists() or args.report.exists(): raise RuntimeError('Choose fresh aliases/report files.')
    args.aliases.parent.mkdir(parents=True,exist_ok=True);args.aliases.write_text(alias_text)
    args.report.parent.mkdir(parents=True,exist_ok=True);args.report.write_text(json.dumps({'status':'validated-published-maintenance-surface',
        'sourcePublished':True,'completionValidated':True,'executionAccepted':False,**checks,'aliases':record(args.aliases)},indent=2)+'\n')
    print(json.dumps({'status':'validated-published-maintenance-surface','apis':len(validated)}));return 0


if __name__=='__main__': raise SystemExit(main())
