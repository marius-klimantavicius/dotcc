#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <errno.h>
struct number_result {uint64_t bits;unsigned signexp;int error;long consumed;};
void number_case(int,const char*,int,void*);
double __edg_scalbn_nearest(double x,int e){int saved=errno;double r=scalbn(x,e);errno=saved;return r;}
long double __edg_scan_scalbnl_nearest(long double x,int e){int saved=errno;long double r=scalbnl(x,e);errno=saved;return r;}
long double __edg_scan_remainder(long double x,long double y){return fmodl(x,y);}
int main(int argc,char**argv) {
 if(argc!=3)return 1;FILE*in=fopen(argv[1],"rb"),*out=fopen(argv[2],"wb");if(!in||!out)return 2;
 char line[50000];while(fgets(line,sizeof line,in)) {
  int kind,base,n=0;if(sscanf(line,"%d %d %n",&kind,&base,&n)!=2)return 3;
  size_t length=strcspn(line+n,"\r\n");if(length%2)return 4;char input[25000];
  for(size_t i=0;i<length/2;++i){unsigned b;if(sscanf(line+n+2*i,"%2x",&b)!=1)return 5;input[i]=(char)b;}input[length/2]=0;
  struct number_result result;number_case(kind,input,base,&result);if(fwrite(&result,sizeof result,1,out)!=1)return 6;
 }
 return fclose(in)||fclose(out);
}
