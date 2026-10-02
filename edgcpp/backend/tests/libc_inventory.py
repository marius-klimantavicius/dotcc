"""Reproducible portable-provider API inventory; declarations are not coverage."""
import argparse,json,re
from pathlib import Path

def inventory(root):
    runtime=root/'backend/runtime';bindings={}
    for path in sorted(runtime.glob('libc*-bindings.def')):
        for symbol,result,parameters,target in re.findall(r'EDG_LIBC\("([^"]+)",\s*"([^"]*)",\s*"([^"]*)",\s*"([^"]*)"\)',path.read_text()):
            if symbol in bindings:raise ValueError('duplicate binding '+symbol)
            bindings[symbol]={'result':result,'parameters':parameters,'target':target,'source':path.name}
    # Explicit public definitions of the automatically packaged selected sources.
    # Keep this list reviewed against provider.json; private helpers excluded.
    selected=['printf','fprintf','sprintf','snprintf','vprintf','vfprintf','vsprintf','vsnprintf','asprintf','vasprintf',
              'strtol','strtoul','strtoll','strtoull','strtof','strtod','strtold','atoi','atol','atoll','atof',
              'strtol_l','strtoul_l','strtoll_l','strtoull_l','strtof_l','strtod_l','strtold_l']
    provider=json.loads((runtime/'musl/provider.json').read_text())
    if any(component['directory']=='wide' for component in provider['components']):
        wide_classifiers=['isw'+name for name in ['alnum','alpha','blank','cntrl','digit','graph','lower','print','punct','space','upper','xdigit']]
        wide_descriptors=['iswctype','wctype','towlower','towupper','towctrans','wctrans']
        selected += wide_classifiers+wide_descriptors+[name+'_l' for name in wide_classifiers+wide_descriptors]
        selected += ['wcslen','wcscpy','wcsncpy','wcscat','wcsncat','wcscmp','wcsncmp','wcschr','wcsrchr','wcscspn','wcsspn','wcspbrk','wcsstr','wcstok','wmemchr','wmemcmp','wmemcpy','wmemmove','wmemset','wcscoll','wcscoll_l','wcsxfrm','wcsxfrm_l']
    headers={}
    for path in sorted((runtime/'libc/include').glob('*.h')):
        text=re.sub(r'/\*.*?\*/','',path.read_text(),flags=re.S)
        text='\n'.join(line for line in text.splitlines() if not line.lstrip().startswith('#'))
        # One-line declarations in our headers; excludes inline implementations,
        # macros, typedefs, data symbols, and language/compiler facilities.
        symbols=sorted(set(re.findall(r'^\s*(?!typedef\b)[\w\s*]+\b(\w+)\s*\([^;{}]*\)\s*;',text,re.M)))
        headers[path.name]={symbol:('runtime-binding' if symbol in bindings else 'selected-source' if symbol in selected else 'declaration-only') for symbol in symbols if not symbol.startswith('__edg_')}
    standard='assert complex ctype errno fenv float inttypes iso646 limits locale math setjmp signal stdalign stdarg stdatomic stdbool stddef stdint stdio stdlib stdnoreturn string tgmath threads time uchar wchar wctype'.split()
    return {'schema':1,'scope':'C17 header-category ledger plus explicit POSIX extensions; not an exhaustive POSIX API catalogue; regex declaration census excludes macros, data and compiler facilities',
            'bindingCount':len(bindings),'publicBindingCount':sum(not n.startswith('__') for n in bindings),'selectedPublicDefinitions':selected,
            'standardHeadersAbsent':[n+'.h' for n in standard if not (runtime/'libc/include'/f'{n}.h').exists()],
            'bindings':bindings,'headers':headers,'provider':json.loads((runtime/'musl/provider.json').read_text())}
if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',required=True,type=Path);a=p.parse_args()
    result=inventory(Path(__file__).resolve().parents[2]);a.output.write_text(json.dumps(result,indent=2,sort_keys=True)+'\n')
    print(json.dumps({key:result[key] for key in ['bindingCount','publicBindingCount','standardHeadersAbsent']},indent=2))
