#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <wchar.h>
#ifdef __cplusplus
extern "C"
#endif
int stdio_management_cases(const char *first,const char *second,const char *missing,const char *diagnostic)
{
 FILE *f=fopen(first,"w+"); char buffer[BUFSIZ],text[128]; if(!f)return 1;
 setbuf(f,buffer);if(fputs("old",f)<0)return 2;
 FILE *(*reopen_address)(const char*,const char*,FILE*)=freopen;
 if(reopen_address(second,"w+",f)!=f || fwide(f,0)!=0 || ferror(f) || feof(f))return 3;
 if(fputs("new",f)<0)return 4;rewind(f);if(ferror(f)||feof(f)||fread(text,1,3,f)!=3||memcmp(text,"new",3))return 5;
 if(fgetc(f)!=EOF||!feof(f))return 6;rewind(f);if(feof(f)||fgetc(f)!='n')return 7;
 if(freopen(second,"r",f)!=f||fwide(f,0)!=0||fgetwc(f)!='n')return 8;
 if(fclose(f))return 9;
 f=fopen(first,"r");if(!f||fread(text,1,3,f)!=3||memcmp(text,"old",3)||fclose(f))return 10;
 f=fopen(second,"r");if(!f)return 11;errno=0;if(freopen(missing,"r",f)!=0||errno!=ENOENT)return 12;
 FILE *(*temporary_address)(void)=tmpfile;f=temporary_address();if(!f)return 13;
 setbuf(f,0);if(fputc(255,f)!=255)return 14;rewind(f);if(fgetc(f)!=255||fclose(f))return 15;
 char name[L_tmpnam];char *(*name_address)(char*)=tmpnam;if(name_address(name)!=name||!name[0])return 16;
 char *global_name=tmpnam(0);if(!global_name||!global_name[0]||!strcmp(name,global_name))return 17;
 if(freopen(diagnostic,"w",stderr)!=stderr)return 18;errno=ENOENT;void(*error_address)(const char*)=perror;error_address("probe");if(fflush(stderr))return 19;
 f=fopen(diagnostic,"r");if(!f||!fgets(text,sizeof(text),f)||strncmp(text,"probe: ",7)||text[strlen(text)-1]!='\n'||fclose(f))return 20;
 return 0;
}
