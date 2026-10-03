"""Independent pinned musl C-locale byte mapping reference for FILE adapters."""
from pathlib import Path
import shutil

def build_c_locale_oracle(root,out,run):
 out.mkdir(parents=True,exist_ok=True);original=root/'backend/runtime/musl/multibyte/originals'
 for name in ['mbrtowc.c','wcrtomb.c','internal.c','internal.h']:shutil.copy2(original/name,out/name)
 (out/'locale_impl.h').write_text('#include <stdlib.h>\n')
 (out/'main.c').write_text('''#include <stdio.h>
#include <wchar.h>
#include <locale.h>
int main(int argc,char**argv){if(argc!=2||!setlocale(LC_ALL,"C"))return 1;FILE*f=fopen(argv[1],"wb");if(!f)return 2;
for(unsigned i=0;i<256;++i){char source=(char)i,destination[4]={0};wchar_t value=-1;mbstate_t state={0};size_t nr=mbrtowc(&value,&source,1,&state),nw=wcrtomb(destination,value,&state);if(nr!=(i!=0)||nw!=1||(unsigned char)destination[0]!=i){fprintf(stderr,"byte %u value %x read %zu write %zu output %u\\n",i,(unsigned)value,nr,nw,(unsigned char)destination[0]);return 3;}if(fwrite(&value,4,1,f)!=1)return 4;}return fclose(f);}
''')
 run(['gcc','-U_FORTIFY_SOURCE','-D_FORTIFY_SOURCE=0','-Dhidden=','-std=c11','-O2','-fno-builtin',out/'main.c',out/'mbrtowc.c',out/'wcrtomb.c',out/'internal.c','-o',out/'oracle']);run([out/'oracle',out/'mapping.bin']);return out/'mapping.bin'
