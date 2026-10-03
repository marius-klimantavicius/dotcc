#include <stdio.h>
#include <stdlib.h>
#include <wchar.h>
#include <locale.h>
#include <errno.h>
#include <stdint.h>
#include <string.h>
#ifdef __cplusplus
extern "C" {
#endif
int wide_file_cases(const char *path){
 if(!setlocale(LC_ALL,"C.UTF-8"))return 1;
 FILE *file=fopen(path,"wb");if(!file)return 2;
 int (*orient)(FILE*,int)=fwide;wint_t(*put)(wchar_t,FILE*)=fputwc;
 if(orient(file,0)!=0||orient(file,1)<=0||put(0x20ac,file)!=0x20ac||putwc(0x1f642,file)!=0x1f642||fputws(L"A\n",file)<0)return 3;
 if(fclose(file))return 4;file=fopen(path,"rb");if(!file)return 5;
 if(fwide(file,1)<=0||!setlocale(LC_ALL,"C"))return 6;
 wint_t(*get)(FILE*)=fgetwc;
 if(get(file)!=0x20ac||ftell(file)!=3)return 7;
 fpos_t position;if(fgetpos(file,&position)||getwc(file)!=0x1f642||ftell(file)!=7)return 8;
 if(fsetpos(file,&position)||get(file)!=0x1f642)return 9;
 if(ungetwc(0x1f642,file)!=0x1f642||ftell(file)!=3||get(file)!=0x1f642||ftell(file)!=7)return 10;
 wchar_t line[8]={0};if(fgetws(line,8,file)!=line||line[0]!='A'||line[1]!='\n'||line[2]!=0)return 11;
 if(get(file)!=WEOF||!feof(file)||ferror(file))return 12;
 if(ungetwc('Z',file)!='Z'||feof(file)||get(file)!='Z')return 13;
#ifdef __EDG_FPOS_ABI_VERSION
 if(__EDG_FPOS_ABI_VERSION!=2||sizeof(fpos_t)!=16)return 14;
 errno=0;if(fgetc(file)!=EOF||errno!=EINVAL||!ferror(file))return 15;clearerr(file);
#endif
 if(fclose(file))return 16;
 if(!setlocale(LC_ALL,"C.UTF-8"))return 17;file=fopen(path,"rb");if(!file)return 18;
 FILE *saved=stdin;stdin=file;wint_t value=getwchar();stdin=saved;if(value!=0x20ac)return 19;if(fclose(file))return 20;
 file=fopen(path,"wb");if(!file)return 21;saved=stdout;stdout=file;value=putwchar(0x20ac);stdout=saved;if(value!=0x20ac||fclose(file))return 22;
 file=fopen(path,"rb");if(!file)return 23;unsigned char bytes[4]={0};if(fread(bytes,1,4,file)!=3||bytes[0]!=0xe2||bytes[1]!=0x82||bytes[2]!=0xac)return 24;if(fclose(file)||remove(path))return 25;
#ifdef __EDG_FPOS_ABI_VERSION
 if(!setlocale(LC_ALL,"C"))return 26;file=fopen(path,"wb");if(!file)return 27;
 for(int i=0;i<256;++i)if(fputc(i,file)!=i)return 28;if(fclose(file))return 29;
 file=fopen(path,"rb");if(!file)return 30;
 for(int i=0;i<256;++i){wint_t wc=fgetwc(file);char byte[4];if(wc!=(wint_t)(i<128?i:0xdf00+i)||wctomb(byte,(wchar_t)wc)!=1||(unsigned char)byte[0]!=i)return 31;}
 if(fclose(file)||remove(path))return 32;
#endif
 return 0;
}
#ifdef __cplusplus
}
#endif
