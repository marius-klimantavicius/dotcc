"""Audit effective emitted options and real definitions, not source-list presence."""
import json
import hashlib
import re
from pathlib import Path


def integer_constant(expression):
    """Read emitted integral literals only; never evaluate arbitrary C# text."""
    text = re.sub(r'\b(?:unchecked|checked)\b', '', expression)
    text = re.sub(r'\(\s*(?:s?byte|u?short|u?int|u?long|n?u?int)\s*\)', '', text)
    text = re.sub(r'[\s()]', '', text)
    match = re.fullmatch(r'(-?(?:0[xX][0-9a-fA-F]+|[0-9]+))[uUlL]*', text)
    return int(match[1], 16 if "x" in match[1].lower() else 10) if match else None


def audit_options(root, source, profile, definitions, macros, closure):
    manifest=json.loads((root/'config/feature-options.json').read_text())
    records=[];failures=[];identifiers=set()
    for option in manifest['options']:
        if option['id'] in identifiers:
            raise RuntimeError('Duplicate feature option: '+option['id'])
        identifiers.add(option['id'])
        expected=option['expected_by_profile'].get(profile)
        if expected is None:continue
        name=option['macro'];expressions=sorted(macros.get(name,()))
        values=[integer_constant(value) for value in expressions]
        absent=[symbol for symbol in option['required_definitions'] if symbol not in definitions]
        guard=option['implementation_guard'];path=source/guard['path'];body=path.read_text()
        condition=re.sub(r'\s+','',guard['condition'])
        guards=[line for line in re.findall(r'^\s*#\s*(?:if|elif)\s+([^\n]+)',body,re.M)
                if re.sub(r'\s+','',line)==condition]
        problems=[]
        if not values or any(value!=expected for value in values):problems.append('effective option does not equal '+str(expected))
        dependencies={}
        for dependency,wanted in option.get('required_options',{}).items():
            dependency_values=[integer_constant(value) for value in macros.get(dependency,())]
            dependencies[dependency]={'expected':wanted,'observed_values':dependency_values}
            if not dependency_values or any(value!=wanted for value in dependency_values):problems.append('required option '+dependency+' does not equal '+str(wanted))
        if absent:problems.append('missing definitions: '+', '.join(absent))
        if guard['path'] not in closure:problems.append('implementation not selected')
        if not guards:problems.append('reviewed implementation guard not found')
        if problems:failures.append(name+': '+'; '.join(problems))
        records.append({'id':option['id'],'macro':name,'expected':expected,
                        'emitted_expressions':expressions,'observed_values':values,'required_options':dependencies,
                        'required_definitions':{symbol:sorted(definitions.get(symbol,())) for symbol in option['required_definitions']},
                        'implementation_guard':{**guard,'matches':len(guards),'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest()},
                        'status':'missing-compiled-contract' if problems else 'compiled-contract-present',
                        'behavior_contract':option['behavior'],'lifetime_contract':option['lifetime']})
    return records,failures


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
    profile_config=json.loads((ctx.root/'config/profiles.json').read_text())[ctx.profile]
    profiles.update(profile_config.get('feature_profiles',[]))
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
    option_records,option_failures=audit_options(ctx.root,ctx.sources['product'],ctx.profile,definitions,macros,set(ctx.receipt.get('source_closure',[])))
    report={'profile':ctx.profile,'scope':'Emitted definitions/options establish compiled coverage only; behavioral suites remain independent.',
            'features':records,'definition_count':len(definitions),'effective_options':{name:sorted(values) for name,values in sorted(macros.items())},
            'option_contracts':option_records,
            'object_sources':{f'{index:03d}-{Path(source).stem}.cs':source for index,source in enumerate(ctx.receipt.get('units',[]))}}
    output=ctx.artifacts/'active-features.json';output.write_text(json.dumps(report,indent=2)+'\n')
    ctx.receipt['active_feature_audit']=str(output)
    if missing:raise RuntimeError('Required compiled feature definitions absent: '+'; '.join(missing))
    if option_failures:raise RuntimeError('Required compiled option contracts absent: '+'; '.join(option_failures))
    return report
