"""Pin portable musl integer utilities, binary search and allocation-free smoothsort."""
from pathlib import Path
import argparse,difflib,hashlib,json,re,shutil
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--musl-root',required=True,type=Path);p.add_argument('--output-dir',required=True,type=Path);a=p.parse_args();out=a.output_dir;out.mkdir(parents=True,exist_ok=False);(out/'originals').mkdir();(out/'patches').mkdir()
manifest={'version':'musl 1.2.6','releaseSha256':'d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a','sources':{},'adapters':{}}
for name in ['abs.c','labs.c','llabs.c','imaxabs.c','div.c','ldiv.c','lldiv.c','imaxdiv.c','bsearch.c','qsort.c','qsort_nr.c','atomic.h']:
 original=(a.musl_root/('src/internal' if name=='atomic.h' else 'src/stdlib')/name).read_text();adapted=original
 if name=='atomic.h':
  adapted='#include <stdint.h>\n'+original[original.index('#ifndef a_ctz_32'):original.index('#ifndef a_clz_64')]
 else:
  adapted='#include "algorithms_compat.h"\n'+adapted
  if name=='bsearch.c':adapted=re.sub(r'\btry\b','probe',adapted)
  if name=='qsort.c':
   adapted=adapted.replace('head = base;','head = (unsigned char *)base;')
   adapted=adapted.replace('weak_alias(__qsort_r, qsort_r);','void qsort_r(void *base,size_t count,size_t width,cmpfun compare,void *context)\n{ __qsort_r(base,count,width,compare,context); }')
 (out/'originals'/name).write_text(original);(out/name).write_text(adapted)
 (out/'patches'/(name+'.patch')).write_text(''.join(difflib.unified_diff(original.splitlines(True),adapted.splitlines(True),fromfile='a/'+name,tofile='b/'+name)))
 manifest['sources'][name]={'upstream':('src/internal/' if name=='atomic.h' else 'src/stdlib/')+name,'originalSha256':hashlib.sha256(original.encode()).hexdigest(),'adaptedSha256':hashlib.sha256(adapted.encode()).hexdigest()}
compat='''/* Portable GNU/POSIX qsort_r ABI: comparator(a,b,context), context last.
   This is a translated API, never the host OS qsort_r ABI. */
#ifndef EDG_ALGORITHMS_COMPAT_H
#define EDG_ALGORITHMS_COMPAT_H
#include <stddef.h>
#ifdef __cplusplus
extern "C" {
#endif
void __qsort_r(void *,size_t,size_t,int (*)(const void *,const void *,void *),void *);
void qsort_r(void *,size_t,size_t,int (*)(const void *,const void *,void *),void *);
#ifdef __cplusplus
}
#endif
#endif
'''
(out/'algorithms_compat.h').write_text(compat);manifest['adapters']['algorithms_compat.h']=hashlib.sha256(compat.encode()).hexdigest()
(out/'source.json').write_text(json.dumps(manifest,indent=2)+'\n');shutil.copyfile(a.musl_root/'COPYRIGHT',out/'COPYRIGHT')
