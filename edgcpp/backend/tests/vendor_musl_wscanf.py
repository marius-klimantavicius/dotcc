from pathlib import Path
import difflib,json,hashlib,shutil
import argparse
parser=argparse.ArgumentParser(description="Reproduce selected musl wide scanf core and adapters.");parser.add_argument('--musl-root',required=True,type=Path);parser.add_argument('--output-dir',required=True,type=Path);args=parser.parse_args();up=args.musl_root;out=args.output_dir;out.mkdir(parents=True,exist_ok=False)
for name in ['originals','patches']:(out/name).mkdir()
original=(up/'src/stdio/vfwscanf.c').read_text();s=original
s=s.replace('#include "stdio_impl.h"','#include "wscan_file.h"').replace('#include "shgetc.h"','#include "../strto/scan_string.h"').replace('#include "intscan.h"','#include "../strto/intscan.h"').replace('#include "floatscan.h"','#include "../strto/floatscan.h"')
a=s.index('#if 1\n#undef getwc');b=s.index('int vfwscanf(',a);s=s[:a]+s[b:]
s=s.replace('int vfwscanf(FILE *restrict f,','int __edg_musl_vwscan(struct edg_wscan_input *restrict f,').replace('off_t pos = 0, cnt;','long pos = 0, cnt;')
s=s.replace('\tstatic const char size_pfx[][3] = { "hh", "h", "", "l", "L", "ll" };\n','')
s=s.replace('\tFLOCK(f);\n\n\tfwide(f, 1);\n','').replace('\tFUNLOCK(f);\n','').replace('weak_alias(vfwscanf,__isoc99_vfwscanf);','').replace('getwc(f)','edg_wget(f)').replace('ungetwc(c, f)','edg_wunget(c, f)')
s=s.replace('wcs = malloc(', 'wcs = (wchar_t *)malloc(').replace('wchar_t *tmp = realloc(', 'wchar_t *tmp = (wchar_t *)realloc(').replace('s = malloc(', 's = (char *)malloc(').replace('char *tmp = realloc(', 'char *tmp = (char *)realloc(').replace('(size == SIZE_def) ? dest : 0','(size == SIZE_def) ? (char *)dest : 0').replace('(size == SIZE_l) ? dest : 0','(size == SIZE_l) ? (wchar_t *)dest : 0')
s=s.replace("\t\tcase '[':\n\t\t\tif (t == 'c')", "\t\tcase '[': {\n\t\t\tif (t == 'c')")
s=s.replace("\t\tcase 'd': case 'i': case 'o': case 'u': case 'x':\n\t\tcase 'a': case 'e': case 'f': case 'g':\n\t\tcase 'A': case 'E': case 'F': case 'G': case 'X':\n\t\tcase 'p':\n", "\t\t}\n\t\tcase 'd': case 'i': case 'o': case 'u': case 'x':\n\t\tcase 'a': case 'e': case 'f': case 'g':\n\t\tcase 'A': case 'E': case 'F': case 'G': case 'X':\n\t\tcase 'p':\n")
a=s.index('\t\t\tsnprintf(tmp,');b=s.index('\t\t\tpos += cnt;',a)
s=s[:a]+'''\t\t\tif (!edg_wscan_number(f,t,size,width,dest,&cnt)) goto match_fail;
'''+s[b:]
helper='''struct edg_wscan_numeric {struct edg_wscan_input *input;int last;};
static int edg_wscan_ascii(void *context)
{
 struct edg_wscan_numeric *source=(struct edg_wscan_numeric*)context;
 source->last=edg_wget(source->input);
 return source->last<0?-1:source->last<128?source->last:'@';
}
static int edg_wscan_number(struct edg_wscan_input *f,int conversion,int size,int width,void *destination,long *count)
{
 struct edg_wscan_numeric source={f,-1};
 struct edg_scan_input input={0,0,0,0,0,edg_wscan_ascii,&source};
 struct edg_string_scan cursor;sh_frominput(&cursor,&input);shlim(&cursor,width);
 if(conversion=='d'||conversion=='i'||conversion=='o'||conversion=='u'||conversion=='x'||conversion=='X'||conversion=='p'){
  unsigned base=conversion=='i'?0:conversion=='o'?8:(conversion=='x'||conversion=='X'||conversion=='p')?16:10;
  unsigned long long value=__intscan(&cursor,base,0,ULLONG_MAX);
  if(shcnt(&cursor)){if(conversion=='p' && destination)*(void**)destination=(void*)(uintptr_t)value;else store_int(destination,size,value);}
 }else{
  long double value=__floatscan(&cursor,size,0);
  if(shcnt(&cursor)&&destination){if(size==SIZE_def)*(float*)destination=value;else if(size==SIZE_l)*(double*)destination=value;else if(size==SIZE_L)*(long double*)destination=value;}
 }
 *count=shcnt(&cursor);if(input.pending)edg_wunget(source.last,f);
 return *count!=0;
}
'''
s=s.replace('int __edg_musl_vwscan(',helper+'\nint __edg_musl_vwscan(')
s=s.replace("\t\t\t\tk = t=='c' ? width+1U : 31;", "\t\t\t\tk = t=='c' ? width+1U : 31;\n\t\t\t\t/* C/UTF-8: reserve a complete scalar before the first wctomb. */\n\t\t\t\tif (size != SIZE_l && k < 4) k = 4;")
(out/'originals/vfwscanf.c').write_text(original);(out/'vfwscanf.c').write_text(s);(out/'patches/vfwscanf.c.patch').write_text(''.join(difflib.unified_diff(original.splitlines(True),s.splitlines(True),fromfile='a/vfwscanf.c',tofile='b/vfwscanf.c')))
(out/'source.json').write_text(json.dumps({'version':'musl 1.2.6','releaseSha256':'d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a','sources':{'vfwscanf.c':{'upstream':'src/stdio/vfwscanf.c','originalSha256':hashlib.sha256(original.encode()).hexdigest(),'adaptedSha256':hashlib.sha256(s.encode()).hexdigest()}},'adapters':{}},indent=2)+'\n');shutil.copyfile(up/'COPYRIGHT',out/'COPYRIGHT')

metadata=json.loads((out/"source.json").read_text())
content='#ifndef EDG_WSCAN_FILE_H\n#define EDG_WSCAN_FILE_H\n#include <stdio.h>\n#include <stdarg.h>\n#include <wchar.h>\n#include <stdint.h>\n#include "../strto/scan_string.h"\nstruct edg_wscan_input {int (*read)(void *);int (*unread)(int,void *);void *context;};\nstatic inline int edg_wget(struct edg_wscan_input *f){return f->read(f->context);}\nstatic inline int edg_wunget(int value,struct edg_wscan_input *f){return value<0?value:f->unread(value,f->context);}\n#ifdef __cplusplus\n#define restrict __restrict\nextern "C" {\n#endif\ntypedef int (*__edg_wscan_body)(void *,const wchar_t *,va_list);\nint __edg_with_file_wide_read_lock(FILE *,__edg_wscan_body,void *,const wchar_t *,va_list);\nint __edg_musl_vwscan(struct edg_wscan_input *,const wchar_t *,va_list);\n#ifdef __cplusplus\n}\n#endif\n#endif\n';(out/'wscan_file.h').write_text(content);metadata["adapters"]['wscan_file.h']=hashlib.sha256(content.encode()).hexdigest()
content='#include "wscan_file.h"\nstruct edg_wscan_string {const wchar_t *position;};\nstatic int string_read(void *context){struct edg_wscan_string *source=(struct edg_wscan_string*)context;int value=*source->position;if(!value)return -1;++source->position;return value;}\nstatic int string_unread(int value,void *context){struct edg_wscan_string *source=(struct edg_wscan_string*)context;--source->position;return value;}\nstatic int file_read(void *context){return (int)fgetwc((FILE*)context);}\nstatic int file_unread(int value,void *context){return (int)ungetwc((wint_t)value,(FILE*)context);}\nstatic int file_body(void *context,const wchar_t *format,va_list arguments)\n{struct edg_wscan_input input={file_read,file_unread,context};return __edg_musl_vwscan(&input,format,arguments);}\nint vfwscanf(FILE *file,const wchar_t *format,va_list arguments)\n{return __edg_with_file_wide_read_lock(file,file_body,file,format,arguments);}\nint vswscanf(const wchar_t *text,const wchar_t *format,va_list arguments)\n{struct edg_wscan_string source={text};struct edg_wscan_input input={string_read,string_unread,&source};return __edg_musl_vwscan(&input,format,arguments);}\nint vwscanf(const wchar_t *format,va_list arguments){return vfwscanf(stdin,format,arguments);}\nint fwscanf(FILE *file,const wchar_t *format,...)\n{va_list arguments;va_start(arguments,format);int result=vfwscanf(file,format,arguments);va_end(arguments);return result;}\nint swscanf(const wchar_t *text,const wchar_t *format,...)\n{va_list arguments;va_start(arguments,format);int result=vswscanf(text,format,arguments);va_end(arguments);return result;}\nint wscanf(const wchar_t *format,...)\n{va_list arguments;va_start(arguments,format);int result=vfwscanf(stdin,format,arguments);va_end(arguments);return result;}\n';(out/'wscan_wrappers.c').write_text(content);metadata["adapters"]['wscan_wrappers.c']=hashlib.sha256(content.encode()).hexdigest()
(out/"source.json").write_text(json.dumps(metadata,indent=2)+"\n")
