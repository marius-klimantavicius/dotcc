#include <stdio.h>
#include <stdlib.h>
#include <locale.h>
#include <string.h>
#include <wchar.h>
void wscanf_case(int,const wchar_t*,const wchar_t*,void*);
static void decode(char *text){size_t n=strlen(text);for(size_t i=0;i<n/2;++i){unsigned value;sscanf(text+2*i,"%2x",&value);text[i]=(char)value;}memset(text+n/2,0,4);}
int main(int argc,char**argv){if(argc!=3)return 1;setlocale(LC_ALL,"C.UTF-8");FILE *in=fopen(argv[1],"rb"),*out=fopen(argv[2],"wb");if(!in||!out)return 2;char line[50000];while(fgets(line,sizeof line,in)){int kind,n;sscanf(line,"%d %n",&kind,&n);char *format=line+n,*text=strchr(format,' ');if(!text)return 3;*text++=0;text[strcspn(text,"\r\n")]=0;size_t fl=strlen(format)/2,tl=strlen(text)/2;char tc[50000];strcpy(tc,text);decode(format);decode(tc);unsigned char record[512];wchar_t wf[12500],wt[12500];memcpy(wf,format,fl+4);memcpy(wt,tc,tl+4);wscanf_case(kind,wt,wf,record);if(fwrite(record,512,1,out)!=1)return 4;}return fclose(in)||fclose(out);}
