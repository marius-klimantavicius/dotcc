from pathlib import Path
import re,hashlib,json,difflib,shutil
import argparse
parser=argparse.ArgumentParser(description='Retain pinned musl wide algorithms and bounded portable adaptations');parser.add_argument('--musl-root',required=True,type=Path);parser.add_argument('--output-dir',required=True,type=Path);args=parser.parse_args()
base=args.musl_root;out=args.output_dir;out.mkdir(parents=True,exist_ok=False)
(out/'originals').mkdir(exist_ok=True);(out/'patches').mkdir(exist_ok=True)
ctype=['iswalnum','iswalpha','iswblank','iswcntrl','iswctype','iswdigit','iswgraph','iswlower','iswprint','iswpunct','iswspace','iswupper','iswxdigit','towctrans','wctrans']
strings=['wcslen','wcscpy','wcsncpy','wcscat','wcsncat','wcscmp','wcsncmp','wcschr','wcsrchr','wcscspn','wcsspn','wcspbrk','wcsstr','wcstok','wmemchr','wmemcmp','wmemcpy','wmemmove','wmemset']
files=[*(f'src/ctype/{n}.c' for n in ctype),*(f'src/string/{n}.c' for n in strings),*(f'src/locale/{n}.c' for n in ['wcscoll','wcsxfrm']),*(f'src/ctype/{n}.h' for n in ['alpha','punct','casemap'])]
manifest={'version':'musl 1.2.6','releaseSha256':'d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a','sources':{},'adapters':{}}
for source in files:
 name=Path(source).name;original=(base/source).read_text();adapted=original
 if name.endswith('.c'):
  adapted='#include "wide_compat.h"\n'+adapted
  adapted=re.sub(r'^weak_alias\([^\n]+\);\n','',adapted,flags=re.M)
  adapted=re.sub(r'\b__(\w+_l)\b',r'\1',adapted)
  if name=='towctrans.c':
   adapted=re.sub(r'^static const [^\n]+;\n','',adapted,flags=re.M)
   adapted=re.sub(r'\btry\b','candidate',adapted)
  if name=='wctrans.c':adapted=re.sub(r'\bclass\b','property_name',adapted)
  if name in ['wcscoll.c','wcsxfrm.c']:
   adapted=adapted.replace('#include "locale_impl.h"\n','')
   if name=='wcscoll.c':adapted=adapted.replace('return wcscoll_l(l, r, CURRENT_LOCALE);','return wcscmp(l, r);')
   else:
    # Share the exact upstream transform implementation; validation belongs only
    # to the public explicit-locale entry, not the plain-locale function.
    adapted=adapted.replace('size_t wcsxfrm_l(wchar_t *restrict dest, const wchar_t *restrict src, size_t n, locale_t loc)','static size_t transform(wchar_t *restrict dest, const wchar_t *restrict src, size_t n)')
    adapted=adapted.replace('return wcsxfrm_l(dest, src, n, CURRENT_LOCALE);','return transform(dest, src, n);')
    adapted+='\nsize_t wcsxfrm_l(wchar_t *restrict dest, const wchar_t *restrict src, size_t n, locale_t loc)\n{\n\tif (!__edg_locale_validate(loc)) return 0;\n\treturn transform(dest, src, n);\n}\n'
  def validate(m):
   whole,kind,symbol,params=m.group(0),m.group(1),m.group(2),m.group(3)
   if symbol=='wcsxfrm_l':return whole
   loc=re.search(r'locale_t\s+(\w+)',params).group(1)
   failure='c' if symbol in ['towlower_l','towupper_l','towctrans_l'] else '0'
   return whole+'\n\tif (!__edg_locale_validate('+loc+')) return '+failure+';'
  adapted=re.sub(r'\b(int|size_t|wint_t|wctype_t|wctrans_t)\s+(\w+_l)\(([^)]*)\)\n\{',validate,adapted)
 (out/'originals'/name).write_text(original);(out/name).write_text(adapted)
 patch=''.join(difflib.unified_diff(original.splitlines(True),adapted.splitlines(True),fromfile='a/'+name,tofile='b/'+name))
 (out/'patches'/(name+'.patch')).write_text(patch)
 manifest['sources'][name]={'upstream':source,'originalSha256':hashlib.sha256(original.encode()).hexdigest(),'adaptedSha256':hashlib.sha256(adapted.encode()).hexdigest()}
compat='''/* Portable glue; all algorithms and tables retain their pinned musl source. */
#ifndef EDG_WIDE_COMPAT_H
#define EDG_WIDE_COMPAT_H
#include <locale.h>
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
int __edg_locale_validate(locale_t);
#ifdef __cplusplus
}
#endif
#endif
'''
(out/'wide_compat.h').write_text(compat);manifest['adapters']['wide_compat.h']=hashlib.sha256(compat.encode()).hexdigest()
(out/'source.json').write_text(json.dumps(manifest,indent=2)+'\n');shutil.copyfile(base/'COPYRIGHT',out/'COPYRIGHT')
print(len([x for x in files if x.endswith('.c')]),'translation units')
