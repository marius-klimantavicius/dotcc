"""Pin musl resumable conversion algorithms and explicit portable TLS adaptation."""
from pathlib import Path
import argparse,difflib,hashlib,json,re,shutil
parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--musl-root',required=True,type=Path);parser.add_argument('--output-dir',required=True,type=Path);args=parser.parse_args()
base=args.musl_root;out=args.output_dir;out.mkdir(parents=True,exist_ok=False);(out/'originals').mkdir();(out/'patches').mkdir()
manifest={'version':'musl 1.2.6','releaseSha256':'d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a','sources':{},'adapters':{}}
for source in sorted((base/'src/multibyte').glob('*')):
 if source.suffix not in ['.c','.h']:continue
 original=source.read_text();adapted=original;name=source.name
 if name.endswith('.c'):
  adapted='#include "mb_compat.h"\n'+adapted
  adapted=re.sub(r'\bstatic unsigned (internal(?:_state)?);',r'static EDG_MB_TLS unsigned \1;',adapted)
  adapted=adapted.replace('(void *)&internal_state','(mbstate_t *)&internal_state')
  adapted=adapted.replace('*src = (const void *)s;','*src = (const char *)s;')
  adapted=adapted.replace('(const void *)src','(const unsigned char *)src').replace('(const void *)*src','(const unsigned char *)*src')
  if name=='mbstowcs.c':adapted=adapted.replace('return mbsrtowcs(ws, (void*)&s, wn, 0);','const char *source = s;\n\treturn mbsrtowcs(ws, &source, wn, 0);')
  if name=='wcstombs.c':adapted=adapted.replace('return wcsrtombs(s, &(const wchar_t *){ws}, n, 0);','const wchar_t *source = ws;\n\treturn wcsrtombs(s, &source, n, 0);')
 else:
  adapted=adapted.replace('#include <features.h>\n','')
  adapted=adapted.replace('extern hidden const uint32_t bittab[];','#ifdef __cplusplus\nextern "C" {\n#endif\nextern const uint32_t bittab[];\n#ifdef __cplusplus\n}\n#endif')
  adapted=adapted.replace('#include "locale_impl.h"','#include <stdlib.h>')
 (out/'originals'/name).write_text(original);(out/name).write_text(adapted)
 (out/'patches'/(name+'.patch')).write_text(''.join(difflib.unified_diff(original.splitlines(True),adapted.splitlines(True),fromfile='a/'+name,tofile='b/'+name)))
 manifest['sources'][name]={'upstream':str(source.relative_to(base)),'originalSha256':hashlib.sha256(original.encode()).hexdigest(),'adaptedSha256':hashlib.sha256(adapted.encode()).hexdigest()}
compat='''/* Hidden restartable state is per translated owner/thread, separately per API. */
#ifndef EDG_MB_COMPAT_H
#define EDG_MB_COMPAT_H
#include <wchar.h>
#include <stdlib.h>
#ifdef __cplusplus
#define restrict __restrict
#define EDG_MB_TLS thread_local
#else
#define EDG_MB_TLS _Thread_local
#endif
#endif
''';(out/'mb_compat.h').write_text(compat);manifest['adapters']['mb_compat.h']=hashlib.sha256(compat.encode()).hexdigest()
(out/'source.json').write_text(json.dumps(manifest,indent=2)+'\n');shutil.copyfile(base/'COPYRIGHT',out/'COPYRIGHT')
print(len([n for n in manifest['sources'] if n.endswith('.c')]),'translation units')
