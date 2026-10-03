"""Reproducible portable-provider API inventory; declarations are not coverage."""
import argparse,json,re
from pathlib import Path

def inventory(root):
    runtime=root/'backend/runtime';bindings={}
    for path in sorted(runtime.glob('libc*-bindings.def')):
        for symbol,result,parameters,target in re.findall(r'(?:EDG_LIBC|EDG_PROCESS_EXIT)\("([^"]+)",\s*"([^"]*)",\s*"([^"]*)",\s*"([^"]*)"\)',path.read_text()):
            if symbol in bindings:raise ValueError('duplicate binding '+symbol)
            bindings[symbol]={'result':result,'parameters':parameters,'target':target,'source':path.name}
    # Explicit public definitions of the automatically packaged selected sources.
    # Keep this list reviewed against provider.json; private helpers excluded.
    selected=['printf','fprintf','sprintf','snprintf','vprintf','vfprintf','vsprintf','vsnprintf','asprintf','vasprintf',
              'strtol','strtoul','strtoll','strtoull','strtoimax','strtoumax','strtof','strtod','strtold','atoi','atol','atoll','atof',
              'strtol_l','strtoul_l','strtoll_l','strtoull_l','strtof_l','strtod_l','strtold_l']
    provider=json.loads((runtime/'musl/provider.json').read_text())
    if any(component['directory']=='wide' for component in provider['components']):
        wide_classifiers=['isw'+name for name in ['alnum','alpha','blank','cntrl','digit','graph','lower','print','punct','space','upper','xdigit']]
        wide_descriptors=['iswctype','wctype','towlower','towupper','towctrans','wctrans']
        selected += wide_classifiers+wide_descriptors+[name+'_l' for name in wide_classifiers+wide_descriptors]
        selected += ['wcslen','wcscpy','wcsncpy','wcscat','wcsncat','wcscmp','wcsncmp','wcschr','wcsrchr','wcscspn','wcsspn','wcspbrk','wcsstr','wcstok','wmemchr','wmemcmp','wmemcpy','wmemmove','wmemset','wcscoll','wcscoll_l','wcsxfrm','wcsxfrm_l']
    if any(component['directory']=='multibyte' for component in provider['components']):
        selected += ['btowc','c16rtomb','c32rtomb','mblen','mbrlen','mbrtoc16','mbrtoc32','mbrtowc','mbsinit','mbsnrtowcs','mbsrtowcs','mbstowcs','mbtowc','wcrtomb','wcsnrtombs','wcsrtombs','wcstombs','wctob','wctomb']
    if any(component['directory']=='wide_numeric' for component in provider['components']):
        selected += ['wcstol','wcstoul','wcstoll','wcstoull','wcstoimax','wcstoumax','wcstof','wcstod','wcstold','wcstol_l','wcstoul_l','wcstoll_l','wcstoull_l','wcstof_l','wcstod_l','wcstold_l']
    if any(component['directory']=='stdlib_algorithms' for component in provider['components']):
        selected += ['abs','labs','llabs','imaxabs','div','ldiv','lldiv','imaxdiv','bsearch','qsort','qsort_r']
    if any(component['directory']=='calendar' for component in provider['components']):
        selected += ['strftime','strftime_l','gmtime_r','gmtime','timegm','asctime_r','asctime','nl_langinfo','nl_langinfo_l']
    if any(component['directory']=='wide_calendar' for component in provider['components']):
        selected += ['wcsftime','wcsftime_l']
    if any(component['directory']=='scanf' for component in provider['components']):
        selected += ['scanf','fscanf','sscanf','vscanf','vfscanf','vsscanf']
    if any(component['directory']=='wscanf' for component in provider['components']):
        selected += ['wscanf','fwscanf','swscanf','vwscanf','vfwscanf','vswscanf']
    if any(component['directory']=='environment' for component in provider['components']):
        selected += ['getenv','setenv','putenv','unsetenv','clearenv']
    if any(component['directory']=='timezone' for component in provider['components']):
        selected += ['tzset','localtime','localtime_r','mktime','ctime','ctime_r']
    if any(component['directory']=='string_case' for component in provider['components']):
        selected += ['strcasecmp','strcasecmp_l','strncasecmp','strncasecmp_l']
    if any(component['directory']=='strptime' for component in provider['components']):
        selected += ['strptime']
    if any(component['directory']=='threads' and component.get('sourceBase')=='libc' for component in provider['components']):
        selected += ['thrd_create','thrd_current','thrd_equal','thrd_join','thrd_detach','thrd_exit','thrd_sleep','thrd_yield','call_once','mtx_init','mtx_destroy','mtx_lock','mtx_trylock','mtx_timedlock','mtx_unlock','cnd_init','cnd_destroy','cnd_signal','cnd_broadcast','cnd_wait','cnd_timedwait','tss_create','tss_delete','tss_get','tss_set']
    if any(component['directory']=='prng' for component in provider['components']):
        selected += ['rand','srand','rand_r','random','srandom','initstate','setstate','drand48','erand48','lrand48','nrand48','mrand48','jrand48','srand48','seed48','lcong48']
    if any(component['directory']=='math_parts' for component in provider['components']):
        selected += ['fmod','fmodf','fmodl','modf','modff','modfl']
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
