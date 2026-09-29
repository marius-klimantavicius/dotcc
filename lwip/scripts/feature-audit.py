"""Audit effective emitted options and real definitions, not source-list presence."""
import json
import re
from pathlib import Path


def run(ctx):
    definitions={}; macros={}
    for path in sorted((ctx.work/'objects').glob('*.cs')):
        content=path.read_text()
        for symbol in re.findall(r'^//!!dotcc-obj (?:def|global-def):([^\n]+)',content,re.M):
            definitions.setdefault(symbol.split('__unit_')[0],set()).add(path.name)
        for name,value in re.findall(r'^//!!dotcc-obj type:macro-constant:([^\n]+)\n\s*public const [^\n=]+ = ([^\n]+);',content,re.M):
            macros.setdefault(name,set()).add(value)
    symbols=json.loads((ctx.root/'config/feature-symbols.json').read_text())
    features=json.loads((ctx.root/'config/features.json').read_text())
    profiles={ctx.profile}
    if ctx.profile in ('contrib','http-content'):profiles.add('threaded')
    records=[];missing=[]
    for feature in features:
        if not profiles.intersection(feature.get('profiles',[])) or feature['disposition']!='required':continue
        required=symbols.get(feature['id'])
        if required is None:raise RuntimeError('Feature lacks active-body audit metadata: '+feature['id'])
        absent=[name for name in required if name not in definitions]
        if absent:missing.append(feature['id']+': '+', '.join(absent))
        records.append({'feature':feature['id'],'status':'missing-definitions' if absent else 'definitions-present',
                        'required_definitions':{name:sorted(definitions.get(name,())) for name in required},
                        'effective_options':{name:({'emitted_expressions':sorted(macros[name])} if name in macros else {'status':'not-observed-in-emitted-constants'}) for name in feature.get('options',[])}})
    report={'profile':ctx.profile,'scope':'Emitted definitions/options establish compiled coverage only; behavioral suites remain independent.',
            'features':records,'definition_count':len(definitions),'effective_options':{name:sorted(values) for name,values in sorted(macros.items())},
            'object_sources':{f'{index:03d}-{Path(source).stem}.cs':source for index,source in enumerate(ctx.receipt.get('units',[]))}}
    output=ctx.artifacts/'active-features.json';output.write_text(json.dumps(report,indent=2)+'\n')
    ctx.receipt['active_feature_audit']=str(output)
    if missing:raise RuntimeError('Required compiled feature definitions absent: '+'; '.join(missing))
    return report
