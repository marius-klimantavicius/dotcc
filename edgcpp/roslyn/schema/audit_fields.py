#!/usr/bin/env python3
"""Inventory declarations and guards mechanically; never infer IL semantics."""
from pathlib import Path
import argparse
import csv
import re

p=argparse.ArgumentParser()
p.add_argument('upstream', type=Path)
p.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1]/'docs'/'field-coverage.tsv')
args=p.parse_args()
raw=(args.upstream/'src'/'il_def.h').read_text()
# Keep byte positions and newlines so upstream line references remain exact.
def blank(m): return ''.join('\n' if c=='\n' else ' ' for c in m.group())
source=re.sub(r'/\*.*?\*/|//[^\n]*',blank,raw,flags=re.S)
guards={}; stack=[]; offset=0
for line in source.splitlines(True):
    text=line.strip()
    if text.startswith('#if '): stack.append(text[4:])
    elif text.startswith('#ifdef '): stack.append('defined('+text[7:]+')')
    elif text.startswith('#ifndef '): stack.append('!defined('+text[8:]+')')
    elif text.startswith('#elif ') and stack: stack[-1]='elif('+text[6:]+')'
    elif text=='#else' and stack: stack[-1]='else('+stack[-1]+')'
    elif text=='#endif' and stack: stack.pop()
    guards[offset]= ' && '.join(stack) or 'always'
    offset+=len(line)
source=re.sub(r'^\s*#[^\n]*',blank,source,flags=re.M)
line_offsets=list(guards)
def guard(pos):
    import bisect
    return guards[line_offsets[bisect.bisect_right(line_offsets,pos)-1]]
def close_brace(start):
    depth=1
    for i in range(start+1,len(source)):
        depth += (source[i]=='{')-(source[i]=='}')
        if not depth:return i
    raise ValueError('unbalanced IL source')
rows=[]
def members(owner,start,end,path=''):
    i=start; declaration=start
    while i<end:
        if source[i]=='{':
            j=close_brace(i)
            semi=source.find(';',j,end)
            if semi<0: break
            tail=source[j+1:semi].strip()
            name=re.match(r'(\w+)',tail)
            prefix=path+(name.group(1)+'.' if name else '')
            members(owner,i+1,j,prefix)
            i=semi+1; declaration=i; continue
        if source[i]==';':
            text=source[declaration:i].strip()
            # Multi-declarators (e.g. then_statement, else_statement) retain a
            # row for each field. Commas inside macro invocations aren't splits.
            declarators=[]; depth=0; begin=0
            for at,char in enumerate(text):
                depth+=(char in '([')-(char in ')]')
                if char==',' and depth==0:
                    declarators.append(text[begin:at]);begin=at+1
            declarators.append(text[begin:])
            for declarator in declarators:
                name=re.search(r'(\w+)\s*(?::[^;]+|\[[^;]*\])?\s*$',declarator)
                if not(name and text and not text.startswith(('typedef','return'))): continue
                pos=declaration+len(source[declaration:i])-len(source[declaration:i].lstrip())
                field=path+name.group(1)
                disposition='pending'
                reason='Requires guarded field-to-relation audit and extraction evidence.'
                # Member spelling cannot determine semantics. In particular,
                # numeric "last" values may be meaningful range endpoints.
                # Linked-list exclusions require an explicit typed adapter and
                # comment/ordering audit in the maintained worksheet.
                if field in ('variant.typeref.orig_type','variant.routine.unlowered_type') or name.group(1)=='destructible_entity_descr':
                    disposition='excluded_private'
                    reason='Approved contract forbids backend use of lowering-private or freed state.'
                rows.append([owner,field,guard(pos),'src/il_def.h:'+str(raw.count('\n',0,pos)+1),disposition,'','','',reason])
            declaration=i+1
        i+=1
for m in re.finditer(r'(?:typedef\s+)?struct\s+(\w+)\s*\{',source):
    start=m.end()-1; end=close_brace(start)
    members(m.group(1),start+1,end)
existing={}
if args.output.exists():
    with args.output.open() as f:
        for row in csv.DictReader(f,delimiter='\t'): existing[(row['owner'],row['member'])]=row
names=['owner','member','guard','source','disposition','destination','api_property','fixture','reason']
args.output.parent.mkdir(parents=True,exist_ok=True)
with args.output.open('w') as f:
    writer=csv.writer(f,delimiter='\t',lineterminator='\n');writer.writerow(names)
    for row in rows:
        prior=existing.get((row[0],row[1]))
        if prior and prior['disposition'] not in ('pending','list_plumbing'):
            row=[prior[n] for n in names]
        writer.writerow(row)
print(f'{len(rows)} guarded fields inventoried; pending classifications are explicit implementation work.')
